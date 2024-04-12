using FMOD.Studio;
using UnityEngine;
using UnityEngine.EventSystems;// Required when using Event data.
using UnityEngine.UI;

public class ButtonClick : MonoBehaviour, IDeselectHandler, IPointerEnterHandler
{
    public bool disableSelectAudio;
    private EventInstance ClickButton;
    private EventInstance BackButton;
    private EventInstance HoverButton;
    private bool wasButtonPressed = false;
    private bool firstToggle = true; // workaround for toggles that change their value on enable
    private Selectable selectableObject;
    void Start()
    {
        ClickButton = AudioManager.Instance.CreateInstance(FMODEvents.Instance.ButtonClick);
        BackButton = AudioManager.Instance.CreateInstance(FMODEvents.Instance.BackSound);
        HoverButton = AudioManager.Instance.CreateInstance(FMODEvents.Instance.HoverButton);
        selectableObject = GetComponent<Selectable>();
    }

    public void OnEnable()
    {
        wasButtonPressed = false; // reset this value for when submenu reappears
    }

    public void ButtonPressed()
    {
        if (firstToggle) wasButtonPressed = true; // this value only changes if toggle. toggles dont have submenus so no need to disable.
        ClickButton.start();
    }

    public void BackButtonPressed()
    {
        if (firstToggle) wasButtonPressed = true;
        if (!AudioManager.IsPlaying(BackButton))
        {
            BackButton.start();
        }
    }

    // select with controller/keyboard (this is on deselect cause it was easier to filter out false positives here)
    public void OnDeselect(BaseEventData eventData)
    {
        if (!selectableObject.interactable || disableSelectAudio || wasButtonPressed) return;
        HoverButton.start();

    }

    // highlight with mouse
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        if (!selectableObject.interactable || disableSelectAudio || EventSystem.current.currentSelectedGameObject == gameObject) return;
        HoverButton.start();
    }

    public void ToggleSound(bool value)
    {
        if (firstToggle)
        {
            firstToggle = false;
            return;
        }
        if (value) ButtonPressed();
        else BackButtonPressed();
    }
}