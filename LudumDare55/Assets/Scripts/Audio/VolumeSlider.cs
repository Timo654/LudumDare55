using FMOD.Studio;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    private AudioManager audioManager;

    private enum VolumeType
    {
        MASTER,
        MUSIC,
        SFX,
        UI
    }

    [Header("Type")]
    [SerializeField] private VolumeType volumeType;

    private Slider volumeSlider;
    private EventInstance sfxTestAudio;
    private EventInstance uiTestAudio;
    private bool firstTime = true; // workaround for test audio playing on menu open

    private void Start()
    {
        volumeSlider = GetComponent<Slider>();
        audioManager = AudioManager.Instance;
        sfxTestAudio = audioManager.CreateInstance(FMODEvents.Instance.TestSound);
        uiTestAudio = audioManager.CreateInstance(FMODEvents.Instance.ButtonClick);
        switch (volumeType)
        {
            case VolumeType.MUSIC:
                volumeSlider.value = audioManager.MusicVolume * 100f;
                break;
            case VolumeType.SFX:
                volumeSlider.value = audioManager.SFXVolume * 100f;
                break;
            case VolumeType.UI:
                volumeSlider.value = audioManager.UIVolume * 100f;
                break;
            case VolumeType.MASTER:
                volumeSlider.value = audioManager.MasterVolume * 100f;
                break;
            default:
                Debug.LogWarning("Volume Type not supported: " + volumeType);
                break;
        }
    }

    public void OnSliderValueChanged()
    {
        if (firstTime)
        {
            firstTime = false;
            return;
        }
        switch (volumeType)
        {
            case VolumeType.MUSIC:
                SaveManager.Instance.systemData.MusicVolume = volumeSlider.value;
                audioManager.MusicVolume = volumeSlider.value / 100f;
                break;
            case VolumeType.SFX:
                SaveManager.Instance.systemData.SFXVolume = volumeSlider.value;
                audioManager.SFXVolume = volumeSlider.value / 100f;
                sfxTestAudio.start();
                break;
            case VolumeType.UI:
                SaveManager.Instance.systemData.UIVolume = volumeSlider.value;
                audioManager.UIVolume = volumeSlider.value / 100f;
                uiTestAudio.start();
                break;
            case VolumeType.MASTER:
                SaveManager.Instance.systemData.MasterVolume = volumeSlider.value;
                audioManager.MasterVolume = volumeSlider.value / 100f;
                break;
            default:
                Debug.LogWarning("Volume Type not supported: " + volumeType);
                break;
        }
    }
}



