using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class RhythmManager : MonoBehaviour
{
    [Header("Song data")]
    [SerializeField] private SongData songData;
    [SerializeField] private float displayDuration = 3;
    [SerializeField] private float greatRange = 0.1f;
    [SerializeField] private float goodRange = 0.2f;
    [Header("General objects")]
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform buttonScroller;
    [SerializeField] private Transform hitter;

    [Header("Stuff to calculate")]
    private float greatRangeInCoords, goodRangeInCoords;

    [Header("Runtime stuff")]
    private readonly List<GameObject> notes = new();
    private ButtonType currentPress = ButtonType.None;
    private bool pressedButton = false;
    private float movementSpeed = 10f;
    private double holdDuration = 0f;
    private bool holdStarted = false;
    private int combo;
    private int score;
    public static event Action<NoteType, HitGrade, ButtonType> OnHit;
    public static event Action OnMiss;

    // Start is called before the first frame update
    void Awake()
    {
        movementSpeed = ((1080.0f + hitter.localPosition.x) / displayDuration) * 2;
        greatRangeInCoords = greatRange * movementSpeed;
        goodRangeInCoords = goodRange * movementSpeed;
        CreateButtons(songData.chart.notes);
        Debug.Log("hello?");
        // TODO - add input handling
    }

    private void Start()
    {
        buttonScroller.GetComponent<ButtonScroller>().movementSpeed = movementSpeed;
    }

    private void OnEnable()
    {
        OnHit += HandleHit;
        OnMiss += HandleMiss;
        GameplayInputHandler.RhythmButtonPressed += OnInputPressed;
        GameplayInputHandler.RhythmButtonReleased += OnInputReleased;
    }

    private void OnDisable()
    {
        OnHit -= HandleHit;
        OnMiss -= HandleMiss;
        GameplayInputHandler.RhythmButtonPressed -= OnInputPressed;
        GameplayInputHandler.RhythmButtonReleased -= OnInputReleased;
    }
    void OnInputPressed(ButtonType button)
    {
        pressedButton = true;
        currentPress = button;
    }

    void OnInputReleased(double duration)
    {
        holdDuration = duration;
    }

    void DestroyNote()
    {
        var noteData = notes[0].GetComponent<ButtonScript>();
        noteData.DestroyNote();
        notes.RemoveAt(0);
    }

    void ResetHitData()
    {
        holdDuration = 0;
        holdStarted = false;
    }

    void UpdateStats()
    {
        Debug.Log(combo);
    }

    void HandleMiss()
    {
        Debug.Log("miss!!");
        combo = 0;
        ResetHitData();
        DestroyNote();
        UpdateStats();
        // TODO - connect these to actual stuff and add input to inputhandler
    }

    void HandleHit(NoteType note, HitGrade grade, ButtonType button)
    {
        Debug.Log("hit!!");
        switch (grade)
        {
            case HitGrade.Good:
                score += 5; // TODO - verify if this is correct
                break;
            case HitGrade.Great:
                score += 10;
                break;
        }

        switch (note)
        {
            case NoteType.Regular:
                // TODO AUDIO ClickButton.start();
                break;
            case NoteType.Hold:
                // TODO AUDIO ClickButton.start();
                break;
            default:
                break;
        }

        combo += 1;
        ResetHitData();
        DestroyNote();
        UpdateStats();
    }

    void CreateButtons(Note[] inputNotes)
    {

        foreach (Note note in inputNotes)
        {
            var noteObject = Instantiate(buttonPrefab);
            noteObject.transform.SetParent(buttonScroller, false);
            var noteScript = noteObject.transform.GetComponent<ButtonScript>();
            var requiredDistance = movementSpeed * (note.startTiming / 1000f) + hitter.localPosition.x;
            noteScript.InitializeNote(note.buttonType, note.noteType, requiredDistance);
            notes.Add(noteObject); // for the list we'll use during gameplay
        }
    }
    void ResetCurrentFrameData()
    {
        pressedButton = false;
        currentPress = ButtonType.None;
    }

    HitGrade VerifyHit(float noteDiff)
    {
        if (noteDiff <= greatRangeInCoords) return HitGrade.Great;
        else return HitGrade.Good;
    }

    // Update is called once per frame
    void Update()
    {
        if (notes.Count <= 0) return; // song is over

        var noteData = notes[0].GetComponent<ButtonScript>();
        float noteDiff = hitter.position.x - notes[0].transform.position.x;
        float absDiff = Mathf.Abs(noteDiff);
        switch (noteData.noteType)
        {
            case NoteType.Regular:
                if (noteDiff > goodRangeInCoords) OnMiss?.Invoke();
                else if (absDiff <= goodRangeInCoords && pressedButton)
                {
                    print($"hit? {absDiff} and {noteDiff} and {goodRangeInCoords}, expected {noteData.buttonType}, got {currentPress}");
                    if (currentPress == noteData.buttonType) OnHit?.Invoke(noteData.noteType, VerifyHit(noteDiff), noteData.buttonType);
                    else OnMiss?.Invoke();
                }
                break;
        }
        ResetCurrentFrameData();
    }

}

public enum HitGrade
{
    Great,
    Good,
    Bad // miss
}
