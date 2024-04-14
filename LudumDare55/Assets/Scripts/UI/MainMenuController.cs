using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject menuButtons;
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private GameObject exitButton;
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject optionsButton;
    [SerializeField] private Button optionsBackButton;
    [SerializeField] private Button difficultyBackButton;
    [SerializeField] private GameObject difficultyPanel;
    [SerializeField] private GameObject easyButton;
    private PlayerControls playerControls;
    private InputAction backAction;
    private CanvasGroup menuButtonsCG;
    private CanvasGroup optionsMenuCG;
    private CanvasGroup difficultyPanelCG;
    private GameObject lastSelect;
    private void Awake()
    {
        UIFader.InitializeFader();
        playerControls = new PlayerControls();
        backAction = playerControls.UI.Back;
        menuButtonsCG = menuButtons.AddComponent<CanvasGroup>();
        optionsMenuCG = optionsMenu.AddComponent<CanvasGroup>();
        difficultyPanelCG = difficultyPanel.GetComponent<CanvasGroup>();
        lastSelect = EventSystem.current.firstSelectedGameObject;
    }
    // Start is called before the first frame update
    void Start()
    {
        AudioManager.Instance.InitializeMusic(FMODEvents.Instance.MenuTheme);
        if (BuildConstants.isMobile || BuildConstants.isWebGL) exitButton.SetActive(false);
        LevelChanger.Instance.FadeIn();
        AudioManager.Instance.StartMusic();
    }

    // handle keyboard input stuff
    private void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            EventSystem.current.SetSelectedGameObject(lastSelect);
        }
        else
        {
            lastSelect = EventSystem.current.currentSelectedGameObject;
        }
    }

    private void OnEnable()
    {
        backAction.Enable();
        backAction.performed += OnBackButton;
    }

    private void OnDisable()
    {
        backAction.performed -= OnBackButton;
        backAction.Disable();
    }
    public void OnBackButton(InputAction.CallbackContext context)
    {
        if (optionsBackButton.IsActive() && optionsBackButton.interactable) optionsBackButton.onClick.Invoke();
        else if (difficultyBackButton.IsActive() && difficultyBackButton.interactable) difficultyBackButton.onClick.Invoke();
    }

    public void OnPlayPressed()
    {
        difficultyPanelCG.DOFade(1f, 1f);
        difficultyPanelCG.blocksRaycasts = true;
        menuButtonsCG.interactable = false;
        EventSystem.current.SetSelectedGameObject(easyButton);
    }

    public void OnExitDifficulty()
    {
        difficultyPanelCG.DOFade(0f, 1f);
        difficultyPanelCG.blocksRaycasts = false;
        menuButtonsCG.interactable = true;
        EventSystem.current.SetSelectedGameObject(playButton);
    }
    public void OnOptionsPressed()
    {
        if (optionsMenu.activeSelf) return;
        UIFader.FadeObjects(optionsMenu, optionsMenuCG, menuButtons, menuButtonsCG);
        EventSystem.current.SetSelectedGameObject(optionsBackButton.gameObject);
    }

    private void OnDifficultySelect(Difficulty difficulty)
    {
        SaveManager.Instance.gameData.difficulty = difficulty;
        LevelChanger.Instance.FadeToLevel("Opening");
    }

    public void SelectEasy()
    {
        OnDifficultySelect(Difficulty.Easy);
    }
    public void SelectHard()
    {
        OnDifficultySelect(Difficulty.Hard);
    }
    public void OnCreditsPressed()
    {
        LevelChanger.Instance.FadeToLevel("Credits");
    }

    public void OnLeaveOptions()
    {
        if (!optionsMenu.activeSelf) return;
        UIFader.FadeObjects(menuButtons, menuButtonsCG, optionsMenu, optionsMenuCG);
        EventSystem.current.SetSelectedGameObject(optionsButton);
    }

    public void OnExitPressed()
    {
        menuButtonsCG.interactable = false;
        LevelChanger.Instance.FadeToDesktop();
    }
}
