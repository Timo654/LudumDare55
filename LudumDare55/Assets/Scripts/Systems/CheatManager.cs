using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CheatManager : MonoBehaviour
{
    public static event Action<bool> ToggleUI;
    private PlayerControls playerControls;
    private InputAction toggleUI;
    private bool showUI = true;
    // Start is called before the first frame update
    void Awake()
    {
        if (!(BuildConstants.isDebug || BuildConstants.isExpo)) return;
        playerControls = new PlayerControls();
        toggleUI = playerControls.Cheats.ToggleUI;
    }

    private void OnEnable()
    {
        if (!(BuildConstants.isDebug || BuildConstants.isExpo)) return;
        toggleUI.Enable();
        toggleUI.performed += OnToggleUI;
    }

    private void OnDisable()
    {
        if (!(BuildConstants.isDebug || BuildConstants.isExpo)) return;
        toggleUI.Disable();
        toggleUI.performed -= OnToggleUI;
    }

    void OnToggleUI(InputAction.CallbackContext context)
    {
        showUI = !showUI;
        ToggleUI?.Invoke(showUI);
    }
}
