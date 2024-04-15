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
    public static event Action<ButtonType> RhythmButtonPressed;
    public static event Action<double> RhythmButtonReleased;

    // BOOLS TO CHECK STUFF
    private bool isPaused = false;
    private bool canPause = false;
    private InputAction pauseAction;
    private InputAction backAction;
    private InputAction rhythmPadAction;

    // misc data
    double holdDuration = 0f;

    // Start is called before the first frame update
    void Awake()
    {
        PlayerControls playerControls = new();
        pauseAction = playerControls.UI.Pause;
        backAction = playerControls.UI.Back;
        rhythmPadAction = playerControls.Gameplay.RhythmPad;
    }

    private void OnEnable()
    {
        pauseAction.Enable();
        backAction.Enable();
        rhythmPadAction.Enable();
        pauseAction.performed += PausePerformed;
        backAction.performed += BackPerformed;
        PauseMenuController.GamePaused += TogglePause;
        rhythmPadAction.started += OnRhythmPress;
        rhythmPadAction.canceled += OnRhythmPress;
        LevelChanger.OnFadeInFinished += EnablePause;
    }

    private void OnDisable()
    {
        pauseAction.Disable();
        backAction.Disable();
        rhythmPadAction.Disable();
        pauseAction.performed -= PausePerformed;
        backAction.performed -= BackPerformed;
        PauseMenuController.GamePaused -= TogglePause;
        LevelChanger.OnFadeInFinished -= EnablePause;
    }

    private void EnablePause()
    {
        canPause = true;
    }
    private void TogglePause(bool obj)
    {
        isPaused = obj;
    }

    private void PausePerformed(InputAction.CallbackContext context)
    {
        if (canPause)
        {
            pause?.Invoke();
        }    
    }

    private void BackPerformed(InputAction.CallbackContext context)
    {
        back?.Invoke();
    }

    void OnRhythmPress(InputAction.CallbackContext context)
    {
        if (isPaused) return; // do not listen to input when paused
        if (context.started)
        {
            ButtonType currentPress = ButtonType.None;
            var vector = context.ReadValue<Vector2>();
            switch (vector.x)
            {
                case -1f:
                    currentPress = ButtonType.Left;
                    break;
                case 1f:
                    currentPress = ButtonType.Right;
                    break;
                default:
                    switch (vector.y)
                    {
                        case 1f:
                            currentPress = ButtonType.Up;
                            break;
                        case -1f:
                            currentPress = ButtonType.Down;
                            break;
                        default:
                            break;
                    }
                    break;
            }
            RhythmButtonPressed?.Invoke(currentPress);
        }
        else if (context.canceled)
        {
            holdDuration = context.duration;
            RhythmButtonReleased?.Invoke(holdDuration);
        }
    }
}
