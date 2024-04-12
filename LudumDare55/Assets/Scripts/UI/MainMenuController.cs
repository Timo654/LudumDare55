using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject menuButtons;
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private GameObject exitButton;
    [SerializeField] private GameObject optionsButton;
    [SerializeField] private Button optionsBackButton;
    private PlayerControls playerControls;
    private InputAction backAction;
    private CanvasGroup menuButtonsCG;
    private CanvasGroup optionsMenuCG;
    private GameObject lastSelect;
    private void Awake()
    {
        UIFader.InitializeFader();
        playerControls = new PlayerControls();
        backAction = playerControls.UI.Back;
        menuButtonsCG = menuButtons.AddComponent<CanvasGroup>();
        optionsMenuCG = optionsMenu.AddComponent<CanvasGroup>();
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
    }

    public void OnPlayPressed()
    {
        LevelChanger.Instance.FadeToLevel("GameScene");
    }

    public void OnOptionsPressed()
    {
        if (optionsMenu.activeSelf) return;
        UIFader.FadeObjects(optionsMenu, optionsMenuCG, menuButtons, menuButtonsCG);
        EventSystem.current.SetSelectedGameObject(optionsBackButton.gameObject);
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
