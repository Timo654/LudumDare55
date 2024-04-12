using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameplayInputHandler : MonoBehaviour
{
    // This class is for handling input stuff. It could be better but hopefully it'll work well enough
    // and make it easier to disable buttons during certain situations...

    // INPUTS
    public static Action pause;
    public static Action back;

    // BOOLS TO CHECK STUFF
    private bool isPaused = false;

    private InputAction pauseAction;
    private InputAction backAction;

    // Start is called before the first frame update
    void Awake()
    {
        PlayerControls playerControls = new();
        pauseAction = playerControls.UI.Pause;
        backAction = playerControls.UI.Back;
    }

    private void OnEnable()
    {
        pauseAction.Enable();
        backAction.Enable();
        pauseAction.performed += PausePerformed;
        backAction.performed += BackPerformed;
        PauseMenuController.GamePaused += TogglePause;
    }

    private void OnDisable()
    {
        pauseAction.Disable();
        backAction.Disable();
        pauseAction.performed -= PausePerformed;
        backAction.performed -= BackPerformed;
        PauseMenuController.GamePaused -= TogglePause;
    }

    private void TogglePause(bool obj)
    {
        isPaused = obj;
    }

    private void PausePerformed(InputAction.CallbackContext context)
    {
        pause?.Invoke();
    }

    private void BackPerformed(InputAction.CallbackContext context)
    {
        back?.Invoke();
    }
}
