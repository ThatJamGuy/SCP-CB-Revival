using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour {
    public static OptionsMenu instance;

    [Header("References")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown windowModeDropdown;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private TMP_Dropdown soundtrackDropdown;
    [SerializeField] private Toggle vSyncToggle;
    [SerializeField] private Toggle fpsCounterToggle;
    [SerializeField] private Toggle consoleToggle;
    [SerializeField] private GameObject frameLimitOption;
    [SerializeField] private TMP_InputField frameLimitInput;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider voiceVolumeSlider;

    public static event Action<bool> OnSettingsChanged;

    private const string SaveFileName = "settings.json";
    private const int UnlimitedFrameRate = -1;
    private const string OptionsSceneName = "Settings";

    private Vector2Int[] resolutions;
    private FullScreenMode[] windowModes;
    private string[] qualityLevels;
    private SettingsData settings;

    private void Awake() {
        if (instance == null) instance = this;
        else { Destroy(gameObject); return; }

        //settings = SaveSystem.Load<SettingsData>(SaveFileName);
        PopulateResolutions();
        PopulateWindowModes();
        PopulateQualityLevels();
        ApplySettings();
    }

    //private void Save() => SaveSystem.Save(settings, SaveFileName);

    private void ApplySettings() {
        int resIndex = Array.FindIndex(resolutions, r => r.x == settings.resolutionWidth && r.y == settings.resolutionHeight);
        if (resIndex < 0) resIndex = 0;

        int winIndex = Mathf.Clamp(settings.windowMode, 0, windowModes.Length - 1);
        int qualIndex = Mathf.Clamp(settings.qualityLevel, 0, qualityLevels.Length - 1);

        resolutionDropdown.SetValueWithoutNotify(resIndex);
        windowModeDropdown.SetValueWithoutNotify(winIndex);
        qualityDropdown.SetValueWithoutNotify(qualIndex);
        vSyncToggle.SetIsOnWithoutNotify(settings.vSync);
        soundtrackDropdown.SetValueWithoutNotify(settings.soundtrack);
        consoleToggle.SetIsOnWithoutNotify(settings.console);
        fpsCounterToggle.SetIsOnWithoutNotify(settings.fpsCounter);
        masterVolumeSlider.SetValueWithoutNotify(settings.masterVolume);
        musicVolumeSlider.SetValueWithoutNotify(settings.musicVolume);
        sfxVolumeSlider.SetValueWithoutNotify(settings.sfxVolume);
        voiceVolumeSlider.SetValueWithoutNotify(settings.voiceVolume);
        frameLimitInput.SetTextWithoutNotify(settings.frameLimit > 0 ? settings.frameLimit.ToString() : string.Empty);

        SetResolution(resIndex);
        SetWindowMode(winIndex);
        SetQualityLevel(qualIndex);
        SetVSync(settings.vSync);
        SetSoundtrack(settings.soundtrack);
        SetConsoleState(settings.console);
        SetFpsCounter(settings.fpsCounter);
        SetMasterVolume(settings.masterVolume);
        SetMusicVolume(settings.musicVolume);
        SetSfxVolume(settings.sfxVolume);
        SetVoiceVolume(settings.voiceVolume);

        Application.targetFrameRate = settings.vSync || settings.frameLimit <= 0
            ? UnlimitedFrameRate
            : settings.frameLimit;
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

    private void PopulateQualityLevels() {
        qualityLevels = QualitySettings.names;
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(qualityLevels.ToList());
    }

    public void SetResolution(int index) {
        if ((uint)index >= resolutions.Length) return;
        var r = resolutions[index];
        Screen.SetResolution(r.x, r.y, Screen.fullScreenMode);
        settings.resolutionWidth = r.x;
        settings.resolutionHeight = r.y;
        //Save();
    }

    public void SetWindowMode(int index) {
        if ((uint)index >= windowModes.Length) return;
        Screen.fullScreenMode = windowModes[index];
        settings.windowMode = index;
        //Save();
    }

    public void SetQualityLevel(int index) {
        if ((uint)index >= qualityLevels.Length) return;
        QualitySettings.SetQualityLevel(index, true);
        settings.qualityLevel = index;
        //Save();
    }

    public void SetVSync(bool enabled) {
        QualitySettings.vSyncCount = enabled ? 1 : 0;
        frameLimitOption.SetActive(!enabled);
        settings.vSync = enabled;
        //Save();

        Application.targetFrameRate = enabled || settings.frameLimit <= 0
            ? UnlimitedFrameRate
            : settings.frameLimit;
    }

    public void SetFrameLimit(string value) {
        if (!int.TryParse(value, out var limit) || limit <= 0) {
            Application.targetFrameRate = UnlimitedFrameRate;
            settings.frameLimit = UnlimitedFrameRate;
        }
        else {
            Application.targetFrameRate = limit;
            settings.frameLimit = limit;
        }
        //Save();
    }

    public void SetSoundtrack(int soundtrackID) {
        if (MusicManager.instance == null) return;
        MusicManager.instance.SetSoundtrack(soundtrackID);
        settings.soundtrack = soundtrackID;
        //Save();
    }

    public void SetConsoleState(bool enabled) {
        settings.console = enabled;
        //Save();
    }

    public void SetFpsCounter(bool enabled) {
        settings.fpsCounter = enabled;
        //Save();
        OnSettingsChanged?.Invoke(enabled);
    }

    public void SetMasterVolume(float value) {
        if (AudioManager.instance != null) AudioManager.instance.masterVolume = value;
        settings.masterVolume = value;
        //Save();
    }

    public void SetMusicVolume(float value) {
        if (AudioManager.instance != null) AudioManager.instance.musicVolume = value;
        settings.musicVolume = value;
        //Save();
    }

    public void SetSfxVolume(float value) {
        if (AudioManager.instance != null) AudioManager.instance.SFXVolume = value;
        settings.sfxVolume = value;
        //Save();
    }

    public void SetVoiceVolume(float value) {
        if (AudioManager.instance != null) AudioManager.instance.voiceVolume = value;
        settings.voiceVolume = value;
        //Save();
    }

    public void CloseOptionsScene() {
        if (SceneManager.GetSceneByName(OptionsSceneName).isLoaded)
            SceneManager.UnloadSceneAsync(OptionsSceneName);
    }
}