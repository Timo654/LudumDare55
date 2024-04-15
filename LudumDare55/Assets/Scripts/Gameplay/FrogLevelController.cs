using DG.Tweening;
using FMODUnity;
using System;
using UnityEngine;

public class FrogLevelController : MonoBehaviour
{
    [SerializeField] EventReference[] music;
    [SerializeField] private CanvasGroup tutorialBox;
    [SerializeField] private SpriteRenderer tiktaalik;
    [SerializeField] private SpriteRenderer flounder;
    private int musicIndex = 0;
    public static Action FrogLevelStart;
    private bool loop = false;
    // Start is called before the first frame update
    private void Awake()
    {
        AudioManager.Instance.InitializeMusic(music[musicIndex]);
    }
    void Start()
    {
        AudioManager.Instance.StartMusic();
        LevelChanger.Instance.FadeIn();
        tutorialBox.DOFade(0f, 0.25f).SetDelay(8f);
        FrogLevelStart?.Invoke();
    }

    private void OnEnable()
    {
        GameplayInputHandler.RhythmButtonPressed += HandleInput;
    }

    private void OnDisable()
    {
        GameplayInputHandler.RhythmButtonPressed -= HandleInput;
    }

    private void HandleInput(ButtonType button)
    {
        switch (button)
        {
            case ButtonType.Left:
                NextSong(-1);
                break;
            case ButtonType.Right:
                NextSong(1);
                break;
            case ButtonType.Up:
                loop = !loop;
                break;
            case ButtonType.Down:
                NextSong(0); // restart song
                break;
        }
    }

    private void NextSong(int increment)
    {
        AudioManager.Instance.FadeOutMusic(); // in case it's playing
        musicIndex += increment;
        if (musicIndex >= music.Length)
        {
            musicIndex = 0;
        }
        else if (musicIndex < 0)
        {
            musicIndex = music.Length - 1;
        }
        AudioManager.Instance.InitializeMusic(music[musicIndex]);
        AudioManager.Instance.StartMusic();
        tiktaalik.DOKill();
        flounder.DOKill();
        if (UnityEngine.Random.value > 0.5f)
        {
            tiktaalik.DOFade(1f, 25f);
            flounder.DOFade(0f, 4f);
        }
        else
        {
            tiktaalik.DOFade(0f, 4f);
            flounder.DOFade(1f, 16f);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (!AudioManager.IsPlaying())
        {
            if (loop)
            {
                NextSong(0);
            }
            else
            {
                NextSong(1);
            }
        }
    }
}
