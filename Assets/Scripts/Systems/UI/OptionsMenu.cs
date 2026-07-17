using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour {
    public static OptionsMenu Instance { get; private set; }

    [Header("Graphics Settings")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown windowModeDropdown;
    [SerializeField] private Toggle vSyncToggle;
    [SerializeField] private GameObject frameLimitOption;
    [SerializeField] private TMP_InputField frameLimitInput;
    [SerializeField] private Toggle fpsCounterToggle;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private TMP_Dropdown textureDropdown;

    [Header("Audio Settings")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider voiceVolumeSlider;
    [SerializeField] private TMP_Dropdown soundtrackDropdown;

    [Header("Gameplay Settings")]
    [SerializeField] private TMP_Dropdown hudPresetDropdown;
    [SerializeField] private TMP_Dropdown hudDesignDropdown;
    [SerializeField] private TMP_Dropdown hudFunctionDropdown;

    [Header("Advanced Settings")]
    [SerializeField] private Toggle consoleToggle;

    [Header("References")]
    [SerializeField] private GameObject fpsDisplayObj;

    private SettingsData localSettings;

    private Vector2Int[] resolutions;
    private FullScreenMode[] windowModes;

    private int lastHudPreset = -1;

    #region Unity Callbacks

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start() {
        localSettings = SettingsManager.settingsData;

        PopulateResolutions();
        PopulateWindowModes();
        InitializeSettingsUI();
    }

    #endregion

    #region Private Methods

    // Fill out all the different settings with what's stored on the players machine
    // Or should I say YOUR MACHINE. Yeah, I know you're reading this...
    // Also automatically applies the settings because they applications are linked to the OnValueChanged stuff
    private void InitializeSettingsUI() {
        int winIndex = Mathf.Clamp(localSettings.windowMode, 0, windowModes.Length - 1);
        int resIndex = Array.FindIndex(resolutions, r => r.x == localSettings.resolutionWidth && r.y == localSettings.resolutionHeight);
        if (resIndex < 0) resIndex = 0;

        // Graphics settings
        resolutionDropdown.value = resIndex;
        windowModeDropdown.value = winIndex;
        vSyncToggle.isOn = localSettings.vSync;
        frameLimitInput.text = localSettings.frameLimit.ToString();
        fpsCounterToggle.isOn = localSettings.fpsCounter;
        qualityDropdown.value = localSettings.qualityLevel;
        textureDropdown.value = localSettings.globalTextureMipmapLimit;

        // Audio settings
        soundtrackDropdown.value = localSettings.soundtrack;
        masterVolumeSlider.value = localSettings.masterVolume;
        musicVolumeSlider.value = localSettings.musicVolume;
        sfxVolumeSlider.value = localSettings.sfxVolume;
        voiceVolumeSlider.value = localSettings.voiceVolume;

        // Gameplay Settings
        hudDesignDropdown.value = localSettings.hudDesign;
        hudFunctionDropdown.value = localSettings.hudFunctionality;

        if (hudDesignDropdown.value != hudFunctionDropdown.value) hudPresetDropdown.value = 0; // Custom
        if (hudDesignDropdown.value == 0 && hudDesignDropdown.value == hudFunctionDropdown.value) hudPresetDropdown.value = 1; // Revival
        if (hudDesignDropdown.value == 1 && hudDesignDropdown.value == hudFunctionDropdown.value) hudPresetDropdown.value = 2; // Legacy

        lastHudPreset = hudPresetDropdown.value;

        // Advanced settings
        consoleToggle.isOn = localSettings.consoleEnabled;
    }

    private void PopulateResolutions() {
        resolutions = Screen.resolutions
            .Select(r => new Vector2Int(r.width, r.height))
            .Distinct().OrderBy(r => r.x).ThenBy(r => r.y).ToArray();

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(resolutions.Select(r => $"{r.x} x {r.y}").ToList());
    }

    private void PopulateWindowModes() {
        windowModes = new[] {
        FullScreenMode.ExclusiveFullScreen,
        FullScreenMode.FullScreenWindow,
        FullScreenMode.Windowed
    };

        windowModeDropdown.ClearOptions();
        windowModeDropdown.AddOptions(windowModes.Select(m => m switch {
            FullScreenMode.ExclusiveFullScreen => "Fullscreen",
            FullScreenMode.FullScreenWindow => "Borderless",
            FullScreenMode.Windowed => "Windowed",
            _ => m.ToString()
        }).ToList());
    }

    #endregion

    #region Public Methods

    public void ApplyMinorGraphicsSettings() {
        // Set the vSync value and display framelimit if necessary
        localSettings.vSync = vSyncToggle.isOn;
        QualitySettings.vSyncCount = localSettings.vSync ? 1 : 0;

        frameLimitOption.SetActive(!localSettings.vSync);
        if (!localSettings.vSync && localSettings.frameLimit >= 10) {
            Application.targetFrameRate = int.TryParse(frameLimitInput.text, out int parsedFrameLimit)
                ? parsedFrameLimit // Parsed input string to int
                : localSettings.frameLimit; // Fallback to last valid from localSettings
        }

        // Set the fps counter value and display the fps counter if necessary
        localSettings.fpsCounter = fpsCounterToggle.isOn;
        fpsDisplayObj.SetActive(localSettings.fpsCounter);

        SettingsManager.SaveSettingsData();
    }

    public void SetOverallQuality(int value) {
        QualitySettings.SetQualityLevel(value, true);

        SettingsManager.SaveSettingsData();
    }

    public void SetTextureQuality(int value) {
        // Key in case I forget
        // 0 = Superb - Full Res
        // 1 = High - Half Res
        // 2 = Medium - Quarter Res
        // 3 = Low - 1/8 Res (I think, I forgot at this point)
        // 4 = Potato - Something really dogshit

        QualitySettings.globalTextureMipmapLimit = value;
    }

    public void SetResolution(int index) {
        if ((uint)index >= resolutions.Length) return;
        var r = resolutions[index];
        Screen.SetResolution(r.x, r.y, Screen.fullScreenMode);
        localSettings.resolutionWidth = r.x;
        localSettings.resolutionHeight = r.y;

        SettingsManager.SaveSettingsData();
    }

    public void SetWindowMode(int index) {
        if ((uint)index >= windowModes.Length) return;
        Screen.fullScreenMode = windowModes[index];
        localSettings.windowMode = index;

        SettingsManager.SaveSettingsData();
    }

    public void ApplyOtherAudioSettings() {
        // Set the soundtrack to the dropdown value and apply it via MusicManager
        localSettings.soundtrack = soundtrackDropdown.value;
        MusicManager.Instance.SetSoundtrack(localSettings.soundtrack);

        SettingsManager.SaveSettingsData();
    }

    public void ApplyAudioSliders() {
        // Set all the volume values in the cached settings data to the slider values
        localSettings.masterVolume = masterVolumeSlider.value;
        localSettings.musicVolume = musicVolumeSlider.value;
        localSettings.sfxVolume = sfxVolumeSlider.value;
        localSettings.voiceVolume = voiceVolumeSlider.value;

        // Set all the volume values in the AudioManager to the slider values and apply them
        AudioManager.Instance.masterVolume = masterVolumeSlider.value;
        AudioManager.Instance.musicVolume = musicVolumeSlider.value;
        AudioManager.Instance.sfxVolume = sfxVolumeSlider.value;
        AudioManager.Instance.voiceVolume = voiceVolumeSlider.value;
        AudioManager.Instance.ApplyAllVolumes();

        SettingsManager.SaveSettingsData();
    }

    public void ApplyHudSettings() {
        int newPresetValue = hudPresetDropdown.value;

        if (newPresetValue != lastHudPreset) {
            if (newPresetValue == 1) { // Revival
                hudDesignDropdown.SetValueWithoutNotify(0);
                hudFunctionDropdown.SetValueWithoutNotify(0);
            } else if (newPresetValue == 2) { // Legacy
                hudDesignDropdown.SetValueWithoutNotify(1);
                hudFunctionDropdown.SetValueWithoutNotify(1);
            }
        }

        localSettings.hudDesign = hudDesignDropdown.value;
        localSettings.hudFunctionality = hudFunctionDropdown.value;

        if (hudDesignDropdown.value != hudFunctionDropdown.value) hudPresetDropdown.SetValueWithoutNotify(0); // Custom
        else if (hudDesignDropdown.value == 0) hudPresetDropdown.SetValueWithoutNotify(1); // Revival
        else if (hudDesignDropdown.value == 1) hudPresetDropdown.SetValueWithoutNotify(2); // Legacy

        lastHudPreset = hudPresetDropdown.value;

        SettingsManager.SaveSettingsData();
    }

    public void ApplyAdvancedSettings() {
        localSettings.consoleEnabled = consoleToggle.isOn;

        SettingsManager.SaveSettingsData();
    }

    #endregion
}