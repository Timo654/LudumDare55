using DG.Tweening;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class OpeningScript : MonoBehaviour
{
    [SerializeField] private Image openingImage;
    [SerializeField] EventReference openingAudio;
    [SerializeField] SongData firstSong;
    [SerializeField] Subtitle subtitle;
    [SerializeField] TextMeshProUGUI subtitleText;
    private int currentSubtitleIndex = 0;
    private SubtitleEntry currentSubtitle;
    private InputAction skipOpening;
    private bool hasEnded = false;
    private bool subtitlesOver = false;
    private bool currentlyShowingSubtitle = false;
    // Start is called before the first frame update
    void Awake()
    {
        var playerControls = new PlayerControls();
        skipOpening = playerControls.UI.SkipOpening;
        AudioManager.Instance.InitializeMusic(openingAudio);
        currentSubtitle = subtitle.subtitles[currentSubtitleIndex];
        HideSubtitle();
    }

    private void OnEnable()
    {
        skipOpening.Enable();
        skipOpening.performed += OnSkipOpening;
    }
    private void OnDisable()
    {
        skipOpening.Disable();
        skipOpening.performed += OnSkipOpening;
    }
    private void Start()
    {
        AudioManager.Instance.StartMusic();
        LevelChanger.Instance.FadeIn();
        openingImage.DOFade(1f, 20f).SetEase(Ease.InOutSine);
    }

    private void OnSkipOpening(InputAction.CallbackContext context)
    {
        if (!hasEnded)
        {
            LoadNextLevel();
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (!AudioManager.IsPlaying() && !hasEnded)
        {
            LoadNextLevel();
        }

        // sync subtitles
        if (subtitlesOver) return;
        float currentTime = AudioManager.Instance.GetMusicPosition() / 1000f;

        if (!currentlyShowingSubtitle && currentSubtitle.startPosition <= currentTime)
        {
            UpdateSubtitle();
        }
        if (currentSubtitle.endPosition <= currentTime)
        {
            currentSubtitleIndex++;
            if (currentSubtitleIndex < subtitle.subtitles.Length)
            {
                currentSubtitle = subtitle.subtitles[currentSubtitleIndex];
                HideSubtitle();
            }
            else
            {
                subtitlesOver = true;
                HideSubtitle();
            }
        }
    }

    private void UpdateSubtitle()
    {
        if (currentSubtitle == null) return; //shouldnt happen
        subtitleText.text = currentSubtitle.text;
        currentlyShowingSubtitle = true;
    }

    private void HideSubtitle()
    {
        subtitleText.text = "";
        currentlyShowingSubtitle = false;
    }

    void LoadNextLevel()
    {
        openingImage.DOKill();
        hasEnded = true;
        SaveManager.Instance.runtimeData.currentSong = firstSong;
        LevelChanger.Instance.FadeToLevel("GameScene");
    }
}
