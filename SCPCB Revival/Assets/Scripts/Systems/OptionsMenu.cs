using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour {
    [Header("References")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown windowModeDropdown;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private TMP_Dropdown soundtrackDropdown;
    [SerializeField] private Toggle vSyncToggle;
    [SerializeField] private Toggle consoleToggle;
    [SerializeField] private GameObject frameLimitOption;
    [SerializeField] private TMP_InputField frameLimitInput;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider voiceVolumeSlider;

    private const string ResolutionWidthKey = "opt_res_w";
    private const string ResolutionHeightKey = "opt_res_h";
    private const string WindowModeKey = "opt_windowmode";
    private const string QualityKey = "opt_quality";
    private const string VSyncKey = "opt_vsync";
    private const string ConsoleKey = "opt_console";
    private const string FrameLimitKey = "opt_framelimit";
    private const string ChosenSoundtrackKey = "opt_soundtrack";
    private const string MasterVolumeKey = "opt_volume_master";
    private const string MusicVolumeKey = "opt_volume_music";
    private const string SfxVolumeKey = "opt_volume_sfx";
    private const string VoiceVolumeKey = "opt_volume_voice";

    private Vector2Int[] resolutions;
    private FullScreenMode[] windowModes;
    private string[] qualityLevels;

    private const int UnlimitedFrameRate = -1;
    private const string optionsSceneName = "Settings";

    private void Awake() {
        PopulateResolutions();
        PopulateWindowModes();
        PopulateQualityLevels();
        LoadSettings();
    }

    private void Start() {
        RichPresence.instance.ChangeActivity("Configuring the Settings");
    }

    private void LoadSettings() {
        int savedW = PlayerPrefs.GetInt(ResolutionWidthKey, Screen.width);
        int savedH = PlayerPrefs.GetInt(ResolutionHeightKey, Screen.height);

        int resIndex = System.Array.FindIndex(resolutions, r => r.x == savedW && r.y == savedH);
        if (resIndex < 0) resIndex = 0;

        int winIndex = Mathf.Clamp(PlayerPrefs.GetInt(WindowModeKey, windowModeDropdown.value), 0, windowModes.Length - 1);
        int qualIndex = Mathf.Clamp(PlayerPrefs.GetInt(QualityKey, qualityDropdown.value), 0, qualityLevels.Length - 1);
        bool vsync = PlayerPrefs.GetInt(VSyncKey, QualitySettings.vSyncCount > 0 ? 1 : 0) == 1;
        int frameLimit = PlayerPrefs.GetInt(FrameLimitKey, UnlimitedFrameRate);
        int soundtrack = PlayerPrefs.GetInt(ChosenSoundtrackKey, soundtrackDropdown.value);
        bool console = PlayerPrefs.GetInt(ConsoleKey, 0) == 1;
        float masterVol = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        float musicVol = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        float sfxVol = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
        float voiceVol = PlayerPrefs.GetFloat(VoiceVolumeKey, 1f);

        resolutionDropdown.SetValueWithoutNotify(resIndex);
        windowModeDropdown.SetValueWithoutNotify(winIndex);
        qualityDropdown.SetValueWithoutNotify(qualIndex);
        vSyncToggle.SetIsOnWithoutNotify(vsync);
        soundtrackDropdown.SetValueWithoutNotify(soundtrack);
        consoleToggle.SetIsOnWithoutNotify(console);
        masterVolumeSlider.SetValueWithoutNotify(masterVol);
        musicVolumeSlider.SetValueWithoutNotify(musicVol);
        sfxVolumeSlider.SetValueWithoutNotify(sfxVol);
        voiceVolumeSlider.SetValueWithoutNotify(voiceVol);

        SetWindowMode(winIndex);
        SetResolution(resIndex);
        SetQualityLevel(qualIndex);
        SetVSync(vsync);
        SetSoundtrack(soundtrack);
        SetConsoleState(console);
        SetMasterVolume(masterVol);
        SetMusicVolume(musicVol);
        SetSfxVolume(sfxVol);
        SetVoiceVolume(voiceVol);

        if (frameLimit > 0)
            frameLimitInput.SetTextWithoutNotify(frameLimit.ToString());
        else
            frameLimitInput.SetTextWithoutNotify(string.Empty);

        Application.targetFrameRate = vsync || frameLimit <= 0
            ? UnlimitedFrameRate
            : frameLimit;
    }

    private void PopulateResolutions() {
        resolutions = Screen.resolutions
            .Select(r => new Vector2Int(r.width, r.height))
            .Distinct()
            .OrderBy(r => r.x)
            .ThenBy(r => r.y)
            .ToArray();

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(resolutions.Select(r => $"{r.x} x {r.y}").ToList());
    }

    public void SetResolution(int index) {
        if ((uint)index >= resolutions.Length) return;
        var r = resolutions[index];
        Screen.SetResolution(r.x, r.y, Screen.fullScreenMode);
        PlayerPrefs.SetInt(ResolutionWidthKey, r.x);
        PlayerPrefs.SetInt(ResolutionHeightKey, r.y);
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

        int current = System.Array.IndexOf(windowModes, Screen.fullScreenMode);
        windowModeDropdown.value = current < 0 ? 0 : current;
        windowModeDropdown.RefreshShownValue();
    }

    private void PopulateQualityLevels() {
        qualityLevels = QualitySettings.names;
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(qualityLevels.ToList());

        int current = QualitySettings.GetQualityLevel();
        qualityDropdown.value = current < 0 ? 0 : current;
        qualityDropdown.RefreshShownValue();
    }

    public void SetQualityLevel(int index) {
        if ((uint)index >= qualityLevels.Length) return;
        QualitySettings.SetQualityLevel(index, true);
        PlayerPrefs.SetInt(QualityKey, index);
    }

    public void SetWindowMode(int index) {
        if ((uint)index >= windowModes.Length) return;
        Screen.fullScreenMode = windowModes[index];
        PlayerPrefs.SetInt(WindowModeKey, index);
    }

    public void SetVSync(bool enabled) {
        QualitySettings.vSyncCount = enabled ? 1 : 0;
        frameLimitOption.SetActive(!enabled);
        PlayerPrefs.SetInt(VSyncKey, enabled ? 1 : 0);

        int frameLimit = PlayerPrefs.GetInt(FrameLimitKey, UnlimitedFrameRate);
        Application.targetFrameRate = enabled || frameLimit <= 0
            ? UnlimitedFrameRate
            : frameLimit;
    }

    public void SetFrameLimit(string value) {
        if (!int.TryParse(value, out var limit) || limit <= 0) {
            Application.targetFrameRate = UnlimitedFrameRate;
            PlayerPrefs.SetInt(FrameLimitKey, UnlimitedFrameRate);
            return;
        }

        Application.targetFrameRate = limit;
        PlayerPrefs.SetInt(FrameLimitKey, limit);
    }

    public void SetSoundtrack(int soundtrackID) {
        if (MusicManager.instance == null) return;
        MusicManager.instance.SetSoundtrack(soundtrackID);
        PlayerPrefs.SetInt(ChosenSoundtrackKey, soundtrackID);
    }

    public void SetConsoleState(bool enabled) {
        PlayerPrefs.SetInt(ConsoleKey, enabled ? 1 : 0);
    }

    public void SetMasterVolume(float value) {
        if (AudioManager.instance != null) {
            AudioManager.instance.masterVolume = value;
        }
        PlayerPrefs.SetFloat(MasterVolumeKey, value);
    }

    public void SetMusicVolume(float value) {
        if (AudioManager.instance != null) {
            AudioManager.instance.musicVolume = value;
        }
        PlayerPrefs.SetFloat(MusicVolumeKey, value);
    }

    public void SetSfxVolume(float value) {
        if (AudioManager.instance != null) {
            AudioManager.instance.SFXVolume = value;
        }
        PlayerPrefs.SetFloat(SfxVolumeKey, value);
    }

    public void SetVoiceVolume(float value) {
        if (AudioManager.instance != null) {
            AudioManager.instance.voiceVolume = value;
        }
        PlayerPrefs.SetFloat(VoiceVolumeKey, value);
    }

    public void CloseOptionsScene() {
        if (SceneManager.GetSceneByName("Menu").isLoaded)
            RichPresence.instance.ChangeActivity("In the main menu");
        else
            RichPresence.instance.ChangeActivity("In a game");

        if (SceneManager.GetSceneByName(optionsSceneName).isLoaded)
            SceneManager.UnloadSceneAsync(optionsSceneName);
    }
}