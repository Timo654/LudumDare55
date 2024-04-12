using UnityEngine;
using UnityEngine.InputSystem;

public class CreditsHandler : MonoBehaviour
{

    private PlayerControls playerControls;
    private InputAction escape;

    private void Awake()
    {
        playerControls = new PlayerControls();
    }
    private void OnEnable()
    {
        escape = playerControls.UI.Pause;
        escape.Enable();
        escape.performed += Pause;
    }
    private void OnDisable()
    {
        escape.Disable();
        escape.performed -= Pause;
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
