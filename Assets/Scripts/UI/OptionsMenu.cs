using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour {
    public static OptionsMenu Instance { get; private set; }
    public static UnityEvent<int> OnMinorGraphicsChanged = new UnityEvent<int>();
    public static UnityEvent<int> OnMajorGraphicsChanged = new UnityEvent<int>();

    [Header("Graphics Settings")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown windowModeDropdown;
    [SerializeField] private Toggle vSyncToggle;
    [SerializeField] private GameObject frameLimitOption;
    [SerializeField] private TMP_InputField frameLimitInput;
    [SerializeField] private Toggle fpsCounterToggle;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private TMP_Dropdown textureDropdown;
    [SerializeField] private TMP_Dropdown antiAliasingDropdown;
    [SerializeField] private TMP_Dropdown shadowQualityDropdown;

    [Header("Audio Settings")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider voiceVolumeSlider;
    [SerializeField] private TMP_Dropdown soundtrackDropdown;

    [Header("Gameplay Settings")]
    [SerializeField] private Slider lookSensSlider;
    [SerializeField] private TextMeshProUGUI lookSensText;
    [SerializeField] private Slider lookSmoothSlider;
    [SerializeField] private TextMeshProUGUI lookSmoothText;
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
        localSettings = RevivalRuntimeEngine.SettingsData;

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

        // Graphics settings — SetValueWithoutNotify prevents these from firing
        // Apply*() methods mid-restore and stomping localSettings with stale UI defaults
        resolutionDropdown.SetValueWithoutNotify(resIndex);
        windowModeDropdown.SetValueWithoutNotify(winIndex);
        vSyncToggle.SetIsOnWithoutNotify(localSettings.vSync);
        frameLimitInput.SetTextWithoutNotify(localSettings.frameLimit.ToString());
        fpsCounterToggle.SetIsOnWithoutNotify(localSettings.fpsCounter);
        qualityDropdown.SetValueWithoutNotify(localSettings.qualityLevel);
        textureDropdown.SetValueWithoutNotify(localSettings.globalTextureMipmapLimit);
        antiAliasingDropdown.SetValueWithoutNotify(localSettings.antiAliasingMode);
        shadowQualityDropdown.SetValueWithoutNotify(localSettings.shadowQualityMode);

        // Audio settings
        soundtrackDropdown.SetValueWithoutNotify(localSettings.soundtrack);
        masterVolumeSlider.SetValueWithoutNotify(localSettings.masterVolume);
        musicVolumeSlider.SetValueWithoutNotify(localSettings.musicVolume);
        sfxVolumeSlider.SetValueWithoutNotify(localSettings.sfxVolume);
        voiceVolumeSlider.SetValueWithoutNotify(localSettings.voiceVolume);

        // Gameplay Settings
        lookSensSlider.SetValueWithoutNotify(localSettings.mouseSensitivity);
        lookSmoothSlider.SetValueWithoutNotify(localSettings.mouseSmoothing);
        hudDesignDropdown.SetValueWithoutNotify(localSettings.hudDesign);
        hudFunctionDropdown.SetValueWithoutNotify(localSettings.hudFunctionality);

        int presetValue;
        if (hudDesignDropdown.value != hudFunctionDropdown.value) presetValue = 0; // Custom
        else if (hudDesignDropdown.value == 0) presetValue = 1; // Revival
        else if (hudDesignDropdown.value == 1) presetValue = 2; // Legacy
        else presetValue = 0;
        hudPresetDropdown.SetValueWithoutNotify(presetValue);
        lastHudPreset = presetValue;

        // Advanced settings
        consoleToggle.SetIsOnWithoutNotify(localSettings.consoleEnabled);

        frameLimitOption.SetActive(!localSettings.vSync);
        fpsDisplayObj.SetActive(localSettings.fpsCounter);
        QualitySettings.vSyncCount = localSettings.vSync ? 1 : 0;
    }

    private void PopulateResolutions() {
        var seen = new HashSet<Vector2Int>();
        var uniqueResolutions = new List<Vector2Int>();

        foreach (Resolution r in Screen.resolutions) {
            Vector2Int res = new Vector2Int(r.width, r.height);
            if (seen.Add(res)) {
                uniqueResolutions.Add(res);
            }
        }

        uniqueResolutions.Sort((a, b) => a.x != b.x ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));
        resolutions = uniqueResolutions.ToArray();

        var options = new List<string>(resolutions.Length);
        foreach (Vector2Int r in resolutions) {
            options.Add($"{r.x} x {r.y}");
        }

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
    }

    private void PopulateWindowModes() {
        windowModes = new[] {
            FullScreenMode.ExclusiveFullScreen,
            FullScreenMode.FullScreenWindow,
            FullScreenMode.Windowed
        };

        var options = new List<string>(windowModes.Length);
        foreach (FullScreenMode m in windowModes) {
            options.Add(m switch {
                FullScreenMode.ExclusiveFullScreen => "Fullscreen",
                FullScreenMode.FullScreenWindow => "Borderless",
                FullScreenMode.Windowed => "Windowed",
                _ => m.ToString()
            });
        }

        windowModeDropdown.ClearOptions();
        windowModeDropdown.AddOptions(options);
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

        // Set Anti Aliasing value and broadcast to Remote Settings Loaders
        localSettings.antiAliasingMode = antiAliasingDropdown.value;


        OnMinorGraphicsChanged.Invoke(localSettings.antiAliasingMode);
        RevivalRuntimeEngine.SaveSettingsData();
    }

    public void ApplyMajorGraphicsSettings() {
        // Set shadow quality and broadcast to Remote Settings Loaders
        localSettings.shadowQualityMode = shadowQualityDropdown.value;

        OnMajorGraphicsChanged.Invoke(localSettings.shadowQualityMode);
        RevivalRuntimeEngine.SaveSettingsData();
    }

    public void SetOverallQuality(int value) {
        QualitySettings.SetQualityLevel(value, true);

        RevivalRuntimeEngine.SaveSettingsData();
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

        RevivalRuntimeEngine.SaveSettingsData();
    }

    public void SetWindowMode(int index) {
        if ((uint)index >= windowModes.Length) return;
        Screen.fullScreenMode = windowModes[index];
        localSettings.windowMode = index;

        RevivalRuntimeEngine.SaveSettingsData();
    }

    public void ApplyOtherAudioSettings() {
        // Set the soundtrack to the dropdown value and apply it via MusicManager
        localSettings.soundtrack = soundtrackDropdown.value;
        MusicManager.Instance.SetSoundtrack(localSettings.soundtrack);

        RevivalRuntimeEngine.SaveSettingsData();
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

        RevivalRuntimeEngine.SaveSettingsData();
    }

    public void ApplyInputSliders() {
        lookSensText.text = lookSensSlider.value.ToString();
        lookSmoothText.text = lookSmoothSlider.value.ToString();

        localSettings.mouseSensitivity = lookSensSlider.value;
        localSettings.mouseSmoothing = lookSmoothSlider.value;

        RevivalRuntimeEngine.SaveSettingsData();
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

        RevivalRuntimeEngine.SaveSettingsData();
    }

    public void ApplyAdvancedSettings() {
        localSettings.consoleEnabled = consoleToggle.isOn;

        RevivalRuntimeEngine.SaveSettingsData();
    }

    #endregion
}