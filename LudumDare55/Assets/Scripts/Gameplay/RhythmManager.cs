using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
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
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI gradeText;

    [Header("Stuff to calculate")]
    private float greatRangeInCoords, goodRangeInCoords;

    [Header("Runtime stuff")]
    private bool songStarted = false;
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
    }

    private void Start()
    {
        LevelChanger.Instance.FadeIn();
        buttonScroller.GetComponent<ButtonScroller>().movementSpeed = movementSpeed;
        songStarted = true;
    }

    private void HandleStart()
    {
        Time.timeScale = 1.0f;
        Debug.Log("start!");
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
        scoreText.text = $"S: {score}";
        comboText.text = $"C: {combo}";
    }

    void HandleMiss()
    {
        Debug.Log("miss!!");
        combo = 0;
        ResetHitData();
        DestroyNote();
        UpdateStats();
        UpdateGrade(HitGrade.Bad);
    }

    void HandleHit(NoteType note, HitGrade grade, ButtonType button)
    {
        UpdateGrade(grade);
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
            var yPosition = hitter.localPosition.y - 60 + (65 * note.verticalPosition); // TODO - adjust this to change the difference between vert lines
            var requiredDistance = movementSpeed * (note.startTiming / 1000f) + hitter.localPosition.x;
            noteScript.InitializeNote(note.buttonType, note.noteType, requiredDistance, yPosition);
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
        Debug.Log($"{noteDiff}, {greatRangeInCoords}, {goodRangeInCoords}");
        if (noteDiff <= greatRangeInCoords) return HitGrade.Great;
        else return HitGrade.Good;
    }

    void UpdateGrade(HitGrade grade)
    {
        gradeText.DOKill();
        gradeText.DOFade(1, 0.25f);
        switch (grade)
        {
            case HitGrade.Good:
                gradeText.text = ":/";
                break;
            case HitGrade.Great:
                gradeText.text = ":)";
                break;
            case HitGrade.Bad:
                gradeText.text = ":(";
                break;
        }
        
        gradeText.DOFade(0, 0.25f).SetDelay(1f);
    }

    // Update is called once per frame
    void Update()
    {
        if (!songStarted) return;
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
                    if (currentPress == noteData.buttonType) OnHit?.Invoke(noteData.noteType, VerifyHit(absDiff), noteData.buttonType);
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
