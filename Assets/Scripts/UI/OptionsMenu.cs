using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// PURPOSE: Handles all things getting, applying, and saving settings. 
/// Heavily Canvas based (Lot's of references to UI Objects)
/// </summary>
public class OptionsMenu : MonoBehaviour {
    public static OptionsMenu Instance { get; private set; }
    public static UnityEvent<int> OnMinorGraphicsChanged = new UnityEvent<int>();
    public static UnityEvent<int> OnMajorGraphicsChanged = new UnityEvent<int>();
    public static UnityEvent<float> OnBrightnessChanged = new UnityEvent<float>();
    public static UnityEvent<bool> OnBloomChanged = new UnityEvent<bool>();
    public static UnityEvent<bool> OnSSRChanged = new UnityEvent<bool>();
    public static UnityEvent<bool> OnSSAOChanged = new UnityEvent<bool>();
    public static UnityEvent<int> OnStatueModelChanged = new UnityEvent<int>();

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
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private TextMeshProUGUI brightnessText;
    [SerializeField] private Toggle bloomToggle;
    [SerializeField] private Toggle ssrToggle;
    [SerializeField] private Toggle ssaoToggle;

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
    [SerializeField] private TMP_Dropdown scp173ModelDropdown;

    [Header("Advanced Settings")]
    [SerializeField] private Toggle consoleToggle;

    [Header("References")]
    [SerializeField] private GameObject fpsDisplayObj;

    private SettingsData localSettings;

    private Vector2Int[] resolutions;
    private FullScreenMode[] windowModes;

    private int lastHudPreset = -1;

    #region Unity Lifecycle

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

        // Graphics and VFX settings
        resolutionDropdown.SetValueWithoutNotify(-resIndex); // Now a negative to support the new big to small dropdown
        windowModeDropdown.SetValueWithoutNotify(winIndex);
        vSyncToggle.SetIsOnWithoutNotify(localSettings.vSync);
        frameLimitInput.SetTextWithoutNotify(localSettings.frameLimit.ToString());
        fpsCounterToggle.SetIsOnWithoutNotify(localSettings.fpsCounter);
        qualityDropdown.SetValueWithoutNotify(localSettings.qualityLevel);
        textureDropdown.SetValueWithoutNotify(localSettings.globalTextureMipmapLimit);
        antiAliasingDropdown.SetValueWithoutNotify(localSettings.antiAliasingMode);
        shadowQualityDropdown.SetValueWithoutNotify(localSettings.shadowQualityMode);
        brightnessSlider.SetValueWithoutNotify((localSettings.brightness * 25f) + 50f);
        bloomToggle.SetIsOnWithoutNotify(localSettings.enableBloom);
        ssrToggle.SetIsOnWithoutNotify(localSettings.enableSSR);
        ssaoToggle.SetIsOnWithoutNotify(localSettings.enableSSAO);

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
        scp173ModelDropdown.SetValueWithoutNotify(localSettings.scp173Model);

        // Refresh most sliders (Audio settings don't have displayed values atm)
        RefreshSliderTexts();

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

    /// <summary>Cool thing that automatically fills the res dropdown based what your monitor supports.</summary>
    private void PopulateResolutions() {
        HashSet<Vector2Int> seen = new HashSet<Vector2Int>();
        List<Vector2Int> uniqueResolutions = new List<Vector2Int>();

        // Grab available screen resolutions and add them to uniqueResolutions
        // Avoids resolutions that have already been added (IE. the old 1920x1080@180hz resolutions)
        foreach (Resolution r in Screen.resolutions) {
            Vector2Int res = new Vector2Int(r.width, r.height);

            if (seen.Add(res)) {
                uniqueResolutions.Add(res);
            }
        }

        uniqueResolutions.Sort((a, b) => a.x != b.x ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));
        resolutions = uniqueResolutions.ToArray();

        // Create a string list containing each resolution and display as something like (1920 [x] x 1080 [y])
        List<string> options = new List<string>(resolutions.Length);
        foreach (Vector2Int r in resolutions) {
            options.Add($"{r.x} x {r.y}");
        }

        // Epic new feature: Reverse the resolution list so it goes from largest to smallest
        options.Reverse();

        // Clear existing options before adding the new ones from options just to be safe
        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
    }

    private void PopulateWindowModes() {
        windowModes = new[] {
            FullScreenMode.ExclusiveFullScreen,
            FullScreenMode.FullScreenWindow,
            FullScreenMode.Windowed
        };

        List<string> options = new List<string>(windowModes.Length);
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

    private void RefreshSliderTexts() {
        brightnessText.text = brightnessSlider.value.ToString();
        lookSensText.text = lookSensSlider.value.ToString();
        lookSmoothText.text = lookSmoothSlider.value.ToString();
    }

    #endregion

    #region Public Methods

    public void ApplyMinorGraphicsSettings() {
        // Set the vSync value and display framelimit if necessary
        localSettings.vSync = vSyncToggle.isOn;
        QualitySettings.vSyncCount = localSettings.vSync ? 1 : 0;

        // If V-Sync is off and the framelimit was set to something greater than 10 then apply it
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

        // Broadcast OnMinorGraphicsChanged to Remote Settings Loaders and save changes to the settings file
        OnMinorGraphicsChanged.Invoke(localSettings.antiAliasingMode);
        RevivalRuntimeEngine.SaveSettingsData();
    }

    public void ApplyMajorGraphicsSettings() {
        localSettings.shadowQualityMode = shadowQualityDropdown.value;

        // Broadcast OnMajorGraphicsChanged to Remote Settings Loaders and save changes to the settings file
        OnMajorGraphicsChanged.Invoke(localSettings.shadowQualityMode);
        RevivalRuntimeEngine.SaveSettingsData();
    }

    public void ApplyBrightness() {
        RefreshSliderTexts();

        localSettings.brightness = (brightnessSlider.value - 50f) / 25f;
        OnBrightnessChanged.Invoke(localSettings.brightness);

        RevivalRuntimeEngine.SaveSettingsData();
    }

    // NOTE: Specifically for things NOT slider related to prevent constant updates!
    // Tis why the brightness setting is it's own ApplyBrightness() method
    public void ApplyVFXSettings() {
        localSettings.enableBloom = bloomToggle.isOn;
        localSettings.enableSSR = ssrToggle.isOn;
        localSettings.enableSSAO = ssaoToggle.isOn;

        OnBloomChanged.Invoke(localSettings.enableBloom);
        OnSSRChanged.Invoke(localSettings.enableSSR);
        OnSSAOChanged.Invoke(localSettings.enableSSAO);

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
        Vector2Int r = resolutions[index];
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
        RefreshSliderTexts();

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

    public void ApplyStatueModel(int newModel) {
        localSettings.scp173Model = newModel;
        OnStatueModelChanged.Invoke(newModel);
        RevivalRuntimeEngine.SaveSettingsData();
    }

    public void ApplyAdvancedSettings() {
        localSettings.consoleEnabled = consoleToggle.isOn;
        RevivalRuntimeEngine.SaveSettingsData();
    }

    #endregion
}