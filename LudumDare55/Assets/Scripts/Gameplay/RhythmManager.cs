using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RhythmManager : MonoBehaviour
{
    [Header("Song data")]
    [SerializeField] private SongData songData;
    [Header("General objects")]
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform buttonScroller;
    [SerializeField] private Transform hitter;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI gradeText;
    [SerializeField] private GameObject longNoteLinePrefab;
    [SerializeField] private Image timerFillImage;
    [Header("Stuff to calculate")]
    private float greatRangeInCoords, goodRangeInCoords;

    [Header("Runtime stuff")]
    private bool fadeinFinished = false;
    private bool gameStarted = false;
    private bool musicStarted = false;
    private bool finished = false;
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
    private int maxScore;
    // Start is called before the first frame update
    void Awake()
    {
        movementSpeed = ((1080.0f + hitter.localPosition.x) / songData.displayDuration) * 2;
        greatRangeInCoords = songData.greatRange * movementSpeed;
        goodRangeInCoords = songData.goodRange * movementSpeed;
        CreateButtons(songData.chart.notes);
        timerFillImage.fillAmount = 0f;
        Debug.Log($"Max score is {maxScore}");
    }

    private void Start()
    {
        AudioManager.Instance.InitializeMusic(songData.music);
        LevelChanger.Instance.FadeIn();
    }

    private void HandleStart()
    {
        Time.timeScale = 1.0f;
        fadeinFinished = true;
        Debug.Log("start!");
    }

    private void OnEnable()
    {
        OnHit += HandleHit;
        OnMiss += HandleMiss;
        LevelChanger.OnFadeInFinished += HandleStart;
        GameplayInputHandler.RhythmButtonPressed += OnInputPressed;
        GameplayInputHandler.RhythmButtonReleased += OnInputReleased;
    }

    private void OnDisable()
    {
        OnHit -= HandleHit;
        OnMiss -= HandleMiss;
        LevelChanger.OnFadeInFinished -= HandleStart;
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
                score += 20;
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
            var noteObject = CreateButton(note);
            if (note.noteType == NoteType.Hold)
            {
                CreateEnd(note, noteObject, Color.cyan);
            }
            notes.Add(noteObject); // for the list we'll use during gameplay
        }
    }
    private GameObject CreateButton(Note note, bool isEnd = false)
    {
        var noteObject = Instantiate(buttonPrefab);
        noteObject.transform.SetParent(buttonScroller, false);
        var noteScript = noteObject.transform.GetComponent<ButtonScript>();
        var yPosition = hitter.localPosition.y - 60 + (65 * note.verticalPosition); // TODO - adjust this to change the difference between vert lines
        var requiredDistance = hitter.localPosition.x;
        if (isEnd)
        {
            maxScore += 20;
            requiredDistance += movementSpeed * (note.endTiming / 1000f);
        }
        else
        {
            maxScore += 10;
            requiredDistance += movementSpeed * (note.startTiming / 1000f);
        }

        noteScript.InitializeNote(note.buttonType, note.noteType, requiredDistance, yPosition);
        return noteObject;
    }
    void CreateEnd(Note note, GameObject noteObject, Color color)
    {
        var endNoteObject = CreateButton(note, true);
        var noteData = noteObject.GetComponent<ButtonScript>();
        noteData.endNote = endNoteObject;
        noteData.noteLength = (note.endTiming - note.startTiming) / 1000f;
        var line = Instantiate(longNoteLinePrefab);
        line.transform.SetParent(buttonScroller, false);
        line.transform.localPosition = noteObject.transform.localPosition;
        line.GetComponent<RectTransform>().sizeDelta = new Vector2(endNoteObject.transform.localPosition.x - noteObject.transform.localPosition.x, 20);
        line.transform.SetSiblingIndex(0);
        line.GetComponent<Image>().color = color;
        noteData.longLine = line;
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

    HitGrade VerifyHold(double noteLength)
    {
        if (holdDuration > noteLength * (songData.holdPercentGreat / 100.0f)) return HitGrade.Great;
        else if (holdDuration > noteLength * (songData.holdPercentGood / 100.0f)) return HitGrade.Good;
        else return HitGrade.Bad;
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
    bool SyncAudio()
    {
        if (!gameStarted)
        {
            if (Input.GetKeyDown(KeyCode.P) || !Application.isEditor)
            {
                gameStarted = true;
                AudioManager.Instance.StartMusic();
            }
            else
            {
                return false;
            }
        }
        if (!musicStarted)
        {
            if (AudioManager.Instance.GetMusicPosition() > 0)
            {
                musicStarted = true;
                buttonScroller.GetComponent<ButtonScroller>().movementSpeed = movementSpeed;
                return true;
            }
            else
            {
                return false;
            }
        }
        return true;
    }

    private void HandleSongEnd()
    {
        Debug.Log("level finished!");
        SaveManager.Instance.gameData.previousScore = score;
        buttonScroller.GetComponent<ButtonScroller>().movementSpeed = 0;
    }

    private void UpdateTimerValue()
    {
        int musicPos = AudioManager.Instance.GetMusicPosition();
        if (musicPos != 0) timerFillImage.fillAmount = AudioManager.Instance.GetMusicPosition() / (float)songData.musicLengthMs;
    }

    // Update is called once per frame
    void Update()
    {
        if (!fadeinFinished) return;
        if (!SyncAudio()) return;

        if (!AudioManager.IsPlaying())
        {
            if (!finished)
            {
                HandleSongEnd();
                finished = true;
            }
        }

        UpdateTimerValue();
        if (notes.Count <= 0) return; // song is over

        var noteData = notes[0].GetComponent<ButtonScript>();

        float noteDiff = hitter.position.x - notes[0].transform.position.x;
        float absDiff = Mathf.Abs(noteDiff);


        float endNoteDiff = 0f;
        float absEndDiff = 0f;
        if (noteData.endNote != null)
        {
            endNoteDiff = hitter.position.x - noteData.endNote.transform.position.x;
            absEndDiff = Mathf.Abs(endNoteDiff);
        }

        switch (noteData.noteType)
        {
            case NoteType.Regular:
                if (noteDiff > goodRangeInCoords) OnMiss?.Invoke();
                else if (absDiff <= goodRangeInCoords && pressedButton)
                {
                    if (currentPress == noteData.buttonType) OnHit?.Invoke(noteData.noteType, VerifyHit(absDiff), noteData.buttonType);
                    else OnMiss?.Invoke();
                }
                break;
            case NoteType.Hold:
                if ((absDiff <= goodRangeInCoords) && pressedButton) // start hold
                {
                    if (currentPress == noteData.buttonType && !holdStarted) holdStarted = true;
                    else
                    {
                        var hitGrade = VerifyHold(noteData.noteLength);
                        if (hitGrade != HitGrade.Bad) OnHit?.Invoke(noteData.noteType, hitGrade, noteData.buttonType);
                        else OnMiss?.Invoke();
                    }
                }
                else if (endNoteDiff > goodRangeInCoords) OnMiss?.Invoke();
                else if (holdStarted && holdDuration > 0f)
                {
                    var hitGrade = VerifyHold(noteData.noteLength);
                    if (hitGrade != HitGrade.Bad) OnHit?.Invoke(noteData.noteType, hitGrade, noteData.buttonType);
                    else OnMiss?.Invoke();
                }
                else holdDuration = 0;
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
