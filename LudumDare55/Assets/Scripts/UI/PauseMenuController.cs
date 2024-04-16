using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    public static Action<bool> GamePaused;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private GameObject pauseButtons;
    [SerializeField] private Button settingsBackButton;
    [SerializeField] private GameObject firstSelectedUIElement;
    [SerializeField] private GameObject touchUI;
    [SerializeField] private CanvasGroup completeScreen;
    [SerializeField] private TextMeshProUGUI completeTitle;
    [SerializeField] private TextMeshProUGUI completeDescription;
    [SerializeField] private GameObject continueButton;
    private bool pauseSubscribed = true;

    private CanvasGroup optionsMenuCG;
    private CanvasGroup pauseMenuCG;
    private CanvasGroup pauseButtonsCG;
    private EventSystem EVRef;
    private GameObject lastSelect;
    private bool isPaused;
    private bool canPause = true;
    public static event Action OnContinue;

    private void Awake()
    {
        UIFader.InitializeFader();
        EVRef = EventSystem.current;
        optionsMenuCG = optionsMenu.AddComponent<CanvasGroup>();
        pauseMenuCG = pauseMenu.AddComponent<CanvasGroup>();
        pauseButtonsCG = pauseButtons.AddComponent<CanvasGroup>();
        lastSelect = firstSelectedUIElement;
        if (touchUI != null)
        {
            if (BuildConstants.isMobile)
            {
                touchUI.SetActive(true);
            }
            else
            {
                touchUI.SetActive(false);
            }
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        if (!focus && canPause)
        {
            PauseGame();
        }
    }

    public void OnEnable()
    {
        GameplayInputHandler.pause += OnPauseButton;
        RhythmManager.OnSongEnd += HandleSongEnd;
    }

    public void OnDisable()
    {
        GameplayInputHandler.pause -= OnPauseButton;
        GameplayInputHandler.back -= OnPauseButton;
        RhythmManager.OnSongEnd -= HandleSongEnd;
    }

    private void Update()
    {
        if (isPaused)
        {
            if (EVRef.currentSelectedGameObject == null)
            {
                EVRef.SetSelectedGameObject(lastSelect);
            }
            else
            {
                lastSelect = EVRef.currentSelectedGameObject;
            }
        }
        else if (!pauseSubscribed)
        {
            SubscribeToPause();
        }
    }

    public void OnPauseButton()
    {
        if (settingsBackButton != null && settingsBackButton.IsActive()) settingsBackButton.onClick.Invoke();
        else HandlePause();
    }

    public void HandlePause()
    {
        if (optionsMenu != null && optionsMenu.activeSelf)
        {
            HandleOptions();
            return;
        }
        if (isPaused) UnpauseGame();
        else PauseGame();
    }

    private void SubscribeToBack()
    {
        if (!pauseSubscribed) return;
        GameplayInputHandler.back += OnPauseButton;
        GameplayInputHandler.pause -= OnPauseButton;
        pauseSubscribed = false;
    }

    private void SubscribeToPause()
    {
        if (pauseSubscribed) return;
        GameplayInputHandler.back -= OnPauseButton;
        GameplayInputHandler.pause += OnPauseButton;
        pauseSubscribed = true;
    }
    void PauseGame()
    {
        if (Time.timeScale < 1.0f) return;
        GamePaused?.Invoke(true);
        Time.timeScale = 0f;
        UIFader.FadeCanvasGroup(pauseMenuCG);
        EVRef.SetSelectedGameObject(firstSelectedUIElement);
        AudioManager.Pause();
        isPaused = true;
    }

    void UnpauseGame()
    {
        SubscribeToPause();
        Time.timeScale = 1f;
        GamePaused?.Invoke(false);
        pauseMenu.SetActive(false);
        AudioManager.Unpause();
        isPaused = false;
    }

    public void HandleOptions()
    {
        if (optionsMenu.activeSelf)
        {
            SubscribeToPause();
            UIFader.FadeObjects(pauseButtons, pauseButtonsCG, optionsMenu, optionsMenuCG);
        }
        else
        {
            SubscribeToBack();
            UIFader.FadeObjects(optionsMenu, optionsMenuCG, pauseButtons, pauseButtonsCG);
        }
    }



    public void HandleContinue()
    {
        OnContinue?.Invoke();
    }

    private void HandleSongEnd(int score, int maxCombo, float scorePercentage, int currentLevel)
    {
        canPause = false;
        completeDescription.text = $"Score: {score} ({(int)(scorePercentage * 100)}%)\nMaximum combo: {maxCombo}";
        switch (currentLevel)
        {
            case 0:
                completeTitle.text = "First chant complete";
                break;
            case 1:
                completeTitle.text = "Final chant complete";
                break;
            default:
                completeTitle.text = "Chant complete";
                break;
        }
        completeScreen.interactable = true;
        completeScreen.blocksRaycasts = true;
        completeScreen.DOFade(1f, 0.5f);
        EVRef.SetSelectedGameObject(continueButton);
    }

    public void HandleQuit()
    {
        LevelChanger.Instance.FadeToLevel("MainMenu");
    }
}
