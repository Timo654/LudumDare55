using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    private Resolution[] resolutions;
    private List<Resolution> filteredResolutions;
    public TMPro.TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    [SerializeField] GameObject resolutionOption;
    [SerializeField] TextMeshProUGUI audioOffsetText;
    private void Start()
    {
        if (BuildConstants.isWebGL || BuildConstants.isMobile)
        {
            resolutionOption.SetActive(false);
        }
        audioOffsetText.text = SaveManager.Instance.gameData.audioOffsetMilliseconds.ToString();
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        filteredResolutions = new();
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (!filteredResolutions.Any(x => x.width == resolutions[i].width && x.height == resolutions[i].height))  //check if resolution already exists in list
            {
                filteredResolutions.Add(resolutions[i]);  //add resolution to list if it doesn't exist yet
            }
        }

        List<string> options = new();
        int currentResolutionIndex = 0;
        for (int i = 0; i < filteredResolutions.Count; i++)
        {
            string option = filteredResolutions[i].width + "x" + filteredResolutions[i].height;
            options.Add(option);
            if (filteredResolutions[i].width == Screen.width && filteredResolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
        fullscreenToggle.isOn = Screen.fullScreen;

        // add the listeners so i dont need to manually drag them in the editor
        fullscreenToggle.onValueChanged.AddListener(delegate { SetFullscreen(fullscreenToggle.isOn); });
        resolutionDropdown.onValueChanged.AddListener(delegate { SetResolution(resolutionDropdown.value); });
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void IncrementAudioOffset()
    {
        SetAudioOffset(1);
    }

    public void DecrementAudioOffset()
    {
        SetAudioOffset(-1);
    }

    public void SetAudioOffset(int increment)
    {
        var audioOffset = SaveManager.Instance.gameData.audioOffsetMilliseconds;
        audioOffset += increment;
        if (audioOffset > 150 || audioOffset < -150)
        {
            return; // do not change the offset
        }
        SaveManager.Instance.gameData.audioOffsetMilliseconds = audioOffset;
        audioOffsetText.text = audioOffset.ToString();
    }
    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = filteredResolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
}
