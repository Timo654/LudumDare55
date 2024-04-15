using DG.Tweening;
using FMOD.Studio;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RhythmManager : MonoBehaviour
{
    [Header("Song data")]
    [SerializeField] private SongData debugSongData;
    [Header("General objects")]
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform buttonScroller;
    [SerializeField] private Transform hitter;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI gradeText;
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private CanvasGroup tutorialBG;
    [SerializeField] private GameObject longNoteLinePrefab;
    [SerializeField] private Image timerFillImage;
    [SerializeField] private EndingData[] endings;
    [SerializeField] private ChartLoader chartLoader;
    [SerializeField] private GameObject fluteFrog;
    [SerializeField] private Sprite happyFrog;
    [SerializeField] private Sprite sadFrog;
    [SerializeField] private Sprite defaultFrog;
    [SerializeField] private float scoreCounterSpeed = 50f;
    [Header("Runtime stuff")]
    private SongData songData;
    private Image fluteFrogSprite;
    private Animator fluteFrogAnim;
    private bool fadeinFinished = false;
    private bool gameStarted = false;
    private bool musicStarted = false;
    private bool finished = false;
    private readonly List<GameObject> notes = new();
    private ButtonType currentPress = ButtonType.None;
    private bool pressedButton = false;
    private float movementSpeed = 10f;
    private double holdDuration = 0f;
    private int maxCombo = 0;
    private bool holdStarted = false;
    private int combo;
    private int score;
    private float visualScore;
    private bool currentlyEmoting;
    private HitGrade prevEmoteGrade;
    private EndingType endingType;
    public static event Action<NoteType, HitGrade, ButtonType> OnHit;
    public static event Action OnMiss;
    public static event Action<int> OnGetMaxScore; // name might be misleading, this is when the game figures out what max score is.
    public static event Action<int> OnGetScore;
    public static event Action<int> OnSongLoad;
    public static event Action<int, int, float, int> OnSongEnd;
    private int maxScore;
    private EventInstance missSound;

    // Start is called before the first frame update
    void Awake()
    {
        fluteFrogAnim = fluteFrog.GetComponent<Animator>();
        fluteFrogSprite = fluteFrog.GetComponent<Image>();
        missSound = AudioManager.Instance.CreateInstance(FMODEvents.Instance.WrongSound);
        if (SaveManager.Instance.runtimeData.currentSong != null)
        {
            songData = SaveManager.Instance.runtimeData.currentSong;
            SaveManager.Instance.runtimeData.currentSong = null;
        }
        else
        {
            Debug.LogWarning("No song data found, using debug one.");
            songData = debugSongData;
        }
        movementSpeed = ((1080.0f + hitter.localPosition.x) / songData.displayDuration) * 2;
        timerFillImage.fillAmount = 0f;
        tutorialBG.gameObject.SetActive(true);
        if (songData.levelId == 1)
        {
            tutorialText.text = "Good... \nWe must do one more dance to summon Them...";
        }

    }

    private void Start()
    {
        OnSongLoad?.Invoke(songData.levelId);
        chartLoader.LoadChart(songData.chartFile, SaveManager.Instance.gameData.difficulty);
    }
    private void ChartLoaded(Chart loadedChart)
    {
        songData.chart = loadedChart;
        CreateButtons(songData.chart.notes);
        OnGetMaxScore?.Invoke(maxScore);
        Debug.Log($"Max score is {maxScore}");

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
        ChartLoader.OnChartLoaded += ChartLoaded;
        LevelChanger.OnFadeInFinished += HandleStart;
        GameplayInputHandler.RhythmButtonPressed += OnInputPressed;
        GameplayInputHandler.RhythmButtonReleased += OnInputReleased;
        PauseMenuController.OnContinue += GoToNextLevel;
    }

    private void OnDisable()
    {
        OnHit -= HandleHit;
        OnMiss -= HandleMiss;
        ChartLoader.OnChartLoaded -= ChartLoaded;
        LevelChanger.OnFadeInFinished -= HandleStart;
        GameplayInputHandler.RhythmButtonPressed -= OnInputPressed;
        GameplayInputHandler.RhythmButtonReleased -= OnInputReleased;
        PauseMenuController.OnContinue -= GoToNextLevel;
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
        comboText.text = $"Combo: {combo}";
    }

    void HandleMiss()
    {
        missSound.start();
        Debug.Log("miss!!");
        if (combo > maxCombo)
        {
            maxCombo = combo;
        }
        combo = 0;
        UpdateGrade(HitGrade.Bad);
        ResetHitData();
        DestroyNote(HitGrade.Bad);
        UpdateStats();

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
                // TODO update sprite
                StartCoroutine(UpdateSpriteOnHit(grade));
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
        OnGetScore?.Invoke(score);
        combo += 1;
        if (combo > 20)
        {
            score += 5;
        }
        ResetHitData();
        DestroyNote(grade);
        UpdateStats();
    }

    IEnumerator UpdateSpriteOnHit(HitGrade newGrade)
    {
        if (currentlyEmoting && newGrade == prevEmoteGrade) yield break;
        else
        {
            currentlyEmoting = true;
            fluteFrogAnim.enabled = false;
            switch (newGrade)
            {
                case HitGrade.Great:
                    fluteFrogSprite.sprite = happyFrog;
                    break;
                case HitGrade.Bad:
                    fluteFrogSprite.sprite = sadFrog;
                    break;
            }
            prevEmoteGrade = newGrade;
        }
        yield return new WaitForSeconds(1f);
        if (prevEmoteGrade == newGrade)
        {
            prevEmoteGrade = HitGrade.Good; // as good as null tbf
            currentlyEmoting = false;
            fluteFrogAnim.enabled = true;
            fluteFrogSprite.sprite = defaultFrog;
        }
    }


    void CreateButtons(Note[] inputNotes)
    {
        foreach (Note note in inputNotes)
        {
            var noteObject = CreateButton(note);
            if (note.noteType == NoteType.Hold)
            {
                var color = Color.white;
                switch (note.buttonType)
                {
                    case ButtonType.Up:
                        color = new Color(0.8313726f, 0.882353f, 0.8705883f, 1f);
                        break;
                    case ButtonType.Down:
                        color = new Color(0.937255f, 0.8705883f, 0.9058824f, 1f);
                        break;
                    case ButtonType.Left:
                        color = new Color(0.9058824f, 0.8666667f, 0.8392158f, 1f);
                        break;
                    case ButtonType.Right:
                        color = new Color(0.8627452f, 0.882353f, 0.8941177f, 1f);
                        break;
                }
                CreateEnd(note, noteObject, color);
            }
            notes.Add(noteObject); // for the list we'll use during gameplay
        }
        maxScore += (notes.Count - 20) * 5; // combo
    }
    private GameObject CreateButton(Note note, bool isEnd = false)
    {
        var noteObject = Instantiate(buttonPrefab);
        noteObject.transform.SetParent(buttonScroller, false);
        var noteScript = noteObject.transform.GetComponent<ButtonScript>();
        var yPosition = hitter.localPosition.y - 160 - (87 * note.verticalPosition); // TODO - adjust this to change the difference between vert lines
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
        StartCoroutine(UpdateSpriteOnHit(grade));
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
                tutorialBG.DOFade(0f, 0.25f).SetDelay(8f);
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
        if (combo > maxCombo)
        {
            maxCombo = combo;
        }
        SaveManager.Instance.gameData.previousScore = score;
        buttonScroller.GetComponent<ButtonScroller>().movementSpeed = 0;
        float scoreReq = 0.75f;
        switch (SaveManager.Instance.gameData.difficulty)
        {
            case Difficulty.Easy:
                scoreReq = 0.75f;
                break;
            case Difficulty.Hard:
                scoreReq = 0.8f;
                break;
        }
        var scorePercentage = score / (float)maxScore;

        if (scorePercentage > scoreReq)
        {
            endingType = EndingType.Good;
        }
        else
        {
            endingType = EndingType.Bad;
        }

        OnSongEnd?.Invoke(score, maxCombo, scorePercentage, songData.levelId);
    }

    private void GoToNextLevel()
    {
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

        if (visualScore != score)
        {
            Debug.Log($"not equal, {visualScore} and {score}");
            visualScore = Mathf.MoveTowards(visualScore, score, scoreCounterSpeed * Time.deltaTime);
            scoreText.text = $"Score: {Mathf.RoundToInt(visualScore)}";
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
                    if (currentPress == noteData.buttonType && !holdStarted)
                    {
                        noteData.OnHoldStart();
                        noteData.endNote.GetComponent<ButtonScript>().OnHoldStart();
                        holdStarted = true;
                    }
                    else
                    {
                        noteData.OnHoldEnd();
                        noteData.endNote.GetComponent<ButtonScript>().OnHoldEnd();
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

public enum Difficulty
{
    Easy,
    Hard
}