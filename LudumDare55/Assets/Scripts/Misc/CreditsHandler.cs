using UnityEngine;
using UnityEngine.InputSystem;

public class CreditsHandler : MonoBehaviour
{

    private PlayerControls playerControls;
    private InputAction escape;
    private InputAction skipCredits;
    private void Awake()
    {
        playerControls = new PlayerControls();
        skipCredits = playerControls.UI.SkipCredits;
        escape = playerControls.UI.Pause;
    }
    private void OnEnable()
    {
        
        escape.Enable();
        skipCredits.Enable();
        escape.performed += Pause;
        skipCredits.performed += Pause;
    }
    private void OnDisable()
    {
        escape.Disable();
        skipCredits.Disable();
        escape.performed -= Pause;
        skipCredits.performed -= Pause;
    }

    // Start is called before the first frame update
    void Start()
    {
        LevelChanger.Instance.FadeIn();
        AudioManager.Instance.InitializeMusic(FMODEvents.Instance.CreditsTheme);
        AudioManager.Instance.StartMusic();
    }

    public void Pause(InputAction.CallbackContext context) // we're just going to skip credits when esc is pressed
    {
        OnCreditsEnd();
    }

    public void OnCreditsEnd()
    {
        LevelChanger.Instance.FadeToLevel("MainMenu");
    }
}
