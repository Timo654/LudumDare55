using DG.Tweening;
using FMOD.Studio;
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
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private GameObject longNoteLinePrefab;
    [SerializeField] private Image timerFillImage;
    [SerializeField] private EndingData[] endings;
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
    public static event Action<int> OnGetMaxScore; // name might be misleading, this is when the game figures out what max score is.
    public static event Action<int> OnGetScore; 
    private int maxScore;
    private EventInstance missSound;
    // Start is called before the first frame update
    void Awake()
    {
        missSound = AudioManager.Instance.CreateInstance(FMODEvents.Instance.WrongSound);
        movementSpeed = ((1080.0f + hitter.localPosition.x) / songData.displayDuration) * 2;
        if (SaveManager.Instance.runtimeData.currentSong != null)
        {
            songData = SaveManager.Instance.runtimeData.currentSong;
            SaveManager.Instance.runtimeData.currentSong = null;
        }
        songData.chart = ChartLoader.LoadChart(songData.chartFile);
        CreateButtons(songData.chart.notes);
        timerFillImage.fillAmount = 0f;
        OnGetMaxScore?.Invoke(maxScore);
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

    void DestroyNote(HitGrade grade)
    {
        var noteData = notes[0].GetComponent<ButtonScript>();
        noteData.DestroyNote(grade);
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
        missSound.start();
        Debug.Log("miss!!");
        combo = 0;
        ResetHitData();
        DestroyNote(HitGrade.Bad);
        UpdateStats();
        UpdateGrade(HitGrade.Bad);
    }

    void HandleHit(NoteType note, HitGrade grade, ButtonType button)
    {
        UpdateGrade(grade);
        switch (grade)
        {
            case HitGrade.Good:
                score += 5;
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
        DestroyNote(grade);
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
        var yPosition = hitter.localPosition.y - 80 + (85 * note.verticalPosition); // TODO - adjust this to change the difference between vert lines
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

        noteScript.InitializeNote(requiredDistance, yPosition, note);
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
        if (noteDiff <= songData.greatRange) return HitGrade.Great;
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
        gradeText.DOFade(1f, 0.25f);
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

        gradeText.DOFade(0f, 0.25f).SetDelay(1f);
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
                tutorialText.DOFade(0f, 0.25f).SetDelay(5f);
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
        EndingType endingType;
        if ((score / (float)maxScore) > 0.6f)
        {
            endingType = EndingType.Good;
        } 
        else
        {
            endingType = EndingType.Bad;
        }

        if (songData.nextSong != null)
        {
            SaveManager.Instance.runtimeData.currentSong = songData.nextSong;
            LevelChanger.Instance.FadeToLevel("GameScene");
        }
        else
        {
            foreach (EndingData ending in endings)
            {
                if (ending.endingType == endingType)
                {
                    SaveManager.Instance.runtimeData.currentEnding = ending;
                    break;
                }
            }
            LevelChanger.Instance.FadeToLevel("Ending");
        }
        
        
    }

    private int UpdateTimerValue()
    {
        int musicPos = AudioManager.Instance.GetMusicPosition();
        if (musicPos != 0) timerFillImage.fillAmount = musicPos / (float)songData.musicLengthMs;
        return musicPos;
    }

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
        
        
        int musicPos = UpdateTimerValue();
        if (notes.Count <= 0) return; // song is over
        var noteData = notes[0].GetComponent<ButtonScript>();
        var timeUntilNote = noteData.note.startTiming - musicPos;
        var timeUntilEndNote = 0f;
        if (noteData.note.noteType == NoteType.Hold)
        {
            timeUntilEndNote = noteData.note.endTiming - musicPos;
        }

        float noteDiff = hitter.position.x - notes[0].transform.position.x;
        switch (noteData.noteType)
        {
            case NoteType.Regular:
                
                if (timeUntilNote < -songData.goodRange) OnMiss?.Invoke();
                else if (timeUntilNote <= songData.goodRange && pressedButton)
                {
                    if (currentPress == noteData.buttonType) OnHit?.Invoke(noteData.noteType, VerifyHit(timeUntilNote), noteData.buttonType);
                    else OnMiss?.Invoke();
                }
                else if (timeUntilNote < songData.badRange && pressedButton) OnMiss?.Invoke();
                break;

            case NoteType.Hold:
                if ((timeUntilNote <= songData.goodRange) && pressedButton) // start hold
                {
                    if (currentPress == noteData.buttonType && !holdStarted) holdStarted = true;
                    else
                    {
                        var hitGrade = VerifyHold(noteData.noteLength);
                        if (hitGrade != HitGrade.Bad) OnHit?.Invoke(noteData.noteType, hitGrade, noteData.buttonType);
                        else OnMiss?.Invoke();
                    }
                }
                else if (timeUntilEndNote < -songData.goodRange) OnMiss?.Invoke();
                else if (holdStarted && holdDuration > 0f)
                {
                    var hitGrade = VerifyHold(noteData.noteLength);
                    if (hitGrade != HitGrade.Bad) OnHit?.Invoke(noteData.noteType, hitGrade, noteData.buttonType);
                    else OnMiss?.Invoke();
                }
                else if (timeUntilNote < songData.badRange && pressedButton) OnMiss?.Invoke();
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
