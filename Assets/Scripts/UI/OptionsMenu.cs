using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// PURPOSE: It's all the primary stuff pertaining to the settings menu. Modifies values and applies them

// It's also the thing people forget about the most. They'll forget the settings menu exists so when their
// 20 year old toaster doesn't run a DEMO that ISNT FINISHED YET very well because they LEFT THE SETTINGS ON HIGH
// they give the game a negative review calling it SLOP and also claim they dislike it becuase it's a WIP,
// like the fuck did you expect when you played A DEMO????? And who gave that review a like AND A FUCKING REWARD??????
// THERE'S EVEN TWO WHOLE ASS ZEROS BEFORE THE NUMBER, OF COURSE IT'S GOING TO LOOK LIKE AN ASSET FLIP THIS EARLY
// BUT ALSO LOWER THE GOD DAMN FUCKING SETTINGS BEFORE YOU COMPLAIN TO ME THAT IT RUNS LIKE ASS. IT'S NOT HARD
// AND NO, MEDIUM OR EVEN LOW DOESN'T LOOK THAT BAD BECAUSE IT DOESN'T TURN EVERYTHING TO MUSH LIKE URNEAL DOES

// That aside MOST of the critisim was valid, but calling my game "slop" is really not necessary
// You can prefer the original without giving this game degrading titles that should be reserved for low effort & AI games
public class OptionsMenu : MonoBehaviour {
    // EMERGENCY BULLSHIT FOR THE RENDERING API SWITCHING
#if UNITY_STANDALONE_WIN
    [DllImport("shell32.dll", CharSet = CharSet.Unicode, EntryPoint = "ShellExecuteW")]
    private static extern IntPtr ShellExecute(IntPtr hwnd, string operation, string file, string parameters, string directory, int showCmd);

    private const int SW_SHOWNORMAL = 1;
#endif

    public static OptionsMenu Instance { get; private set; }
    public static UnityEvent<int> OnMinorGraphicsChanged = new UnityEvent<int>();
    public static UnityEvent<int> OnMajorGraphicsChanged = new UnityEvent<int>();
    public static UnityEvent<bool> OnVFXChanged = new UnityEvent<bool>();
    public static UnityEvent<bool> OnSSRChanged = new UnityEvent<bool>();
    public static UnityEvent<bool> OnSSAOChanged = new UnityEvent<bool>();

    [Header("Graphics Settings")]
    [SerializeField] private TMP_Dropdown renderingAPIdropdown;
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

    [Header("Advanced Settings")]
    [SerializeField] private Toggle consoleToggle;

    [Header("References")]
    [SerializeField] private GameObject renderingAPIScreen;
    [SerializeField] private GameObject fpsDisplayObj;

    private SettingsData localSettings;

    private Vector2Int[] resolutions;
    private FullScreenMode[] windowModes;

    private int pendingRenderingAPI = -1;
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

        // Graphics settings — SetValueWithoutNotify prevents these from firing
        // Apply*() methods mid-restore and stomping localSettings with stale UI defaults

        // NOTE: THIS IS THE GRAPHICS API OPTION WHICH IS SUPER DELICATE
        // The order of options in this dropdown MUST be as follows: 0 - Dx12, 1 - Dx11, 2 - Vulkan
        renderingAPIdropdown.SetValueWithoutNotify(Mathf.Clamp(localSettings.renderingAPI, 0, renderingAPIdropdown.options.Count - 1));

        resolutionDropdown.SetValueWithoutNotify(resIndex);
        windowModeDropdown.SetValueWithoutNotify(winIndex);
        vSyncToggle.SetIsOnWithoutNotify(localSettings.vSync);
        frameLimitInput.SetTextWithoutNotify(localSettings.frameLimit.ToString());
        fpsCounterToggle.SetIsOnWithoutNotify(localSettings.fpsCounter);
        qualityDropdown.SetValueWithoutNotify(localSettings.qualityLevel);
        textureDropdown.SetValueWithoutNotify(localSettings.globalTextureMipmapLimit);
        antiAliasingDropdown.SetValueWithoutNotify(localSettings.antiAliasingMode);
        shadowQualityDropdown.SetValueWithoutNotify(localSettings.shadowQualityMode);
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
        RefreshSensitivitySliderText();

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

    private void RefreshSensitivitySliderText() {
        lookSensText.text = lookSensSlider.value.ToString();
        lookSmoothText.text = lookSmoothSlider.value.ToString();
    }

    #endregion

    #region Public Methods

    // Called via the rendering API dropdown, which adds sets the chosen API to a pending state and activates the confirm screen
    public void OnRenderingAPISelected(int index) {
        // Little check to ensure tampering with the settings file doesn't break things
        if (renderingAPIdropdown.options.Count == 0 ||
            (uint)index >= renderingAPIdropdown.options.Count ||
            index == localSettings.renderingAPI)
            return;

        pendingRenderingAPI = index;
        renderingAPIScreen.SetActive(true);
    }

    // Called via the "No" button on the confirm screen. Resets pending values, restores dropdown value, and closes the confirm
    public void CancelRenderingAPIChange() {
        pendingRenderingAPI = -1;
        renderingAPIScreen.SetActive(false);

        renderingAPIdropdown.SetValueWithoutNotify(Mathf.Clamp(localSettings.renderingAPI, 0, renderingAPIdropdown.options.Count - 1));
    }

    // Called via the "Yes" button on the confirm screen. Get's the associated args for the pending API and attempts to reboot with it
    public void ConfirmRenderingAPIChange() {
        // Since the rendering API cannot be changed via index it's done via args here like Steam launch options
        string graphicsArg = pendingRenderingAPI switch {
            0 => string.Empty, // This guy is empty because Dx12 is the default, so choosing 0 will use default rendering API :D
            1 => "-force-d3d11",
            2 => "-force-vulkan",
            _ => null
        };

        // If the pending index isn't good then don't do anything (Safefuard for typos and shi)
        if (graphicsArg == null) return;

        // Save only after the user confirms so that we're not doing strange things at runtime
        // The previously used SaveValueWithoutNotify() thing should prevent the prompt from displaying when booting up
        int previousRenderingAPI = localSettings.renderingAPI;
        localSettings.renderingAPI = pendingRenderingAPI;
        RevivalRuntimeEngine.SaveSettingsData();

        // Attempt to perform the restart, and if it fails then we restor previous settings and yell about it a bit
        // Initially tried a Process.GetCurrentProcess() but I then learned that only works on mono, so use it no more
        // New method uses the command line instead
        try {
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            string executablePath = Path.GetFullPath(commandLineArgs[0]);
            StringBuilder argBuilder = new StringBuilder(128);

            for (int i = 1; i < commandLineArgs.Length; i++) {
                string arg = commandLineArgs[i];

                if (arg == "-force-d3d12" || arg == "-force-d3d11" || arg == "-force-vulkan") continue;
                if (argBuilder.Length > 0) argBuilder.Append(' ');

                // Re-quote the arguments containing spaces so they survive when launching again
                if (arg.IndexOf(' ') >= 0) argBuilder.Append('"').Append(arg).Append('"');
                else argBuilder.Append(arg);
            }

            // Add the rendering api argument to the argument builder that passes in the cmd
            if (graphicsArg.Length > 0) {
                if (argBuilder.Length > 0) argBuilder.Append(' ');
                argBuilder.Append(graphicsArg);
            }

#if UNITY_STANDALONE_WIN
            // ShellExecuteW returns a value greater than 32 on success, otherwise an error code
            long result = ShellExecute(IntPtr.Zero, "open", executablePath, argBuilder.ToString(), Path.GetDirectoryName(executablePath), SW_SHOWNORMAL).ToInt64();
            if (result <= 32)
                throw new InvalidOperationException($"ShellExecute failed with code {result}");
#else
        throw new PlatformNotSupportedException("Restarting with a graphics API is only supported on Windows standalone builds");
#endif

            // And can't forget this guy, the one that kills it all
            Application.Quit();
        } catch (Exception exception) {
            UnityEngine.Debug.LogError($"Could not restart with the selected graphics API: {exception}");

            localSettings.renderingAPI = previousRenderingAPI;
            RevivalRuntimeEngine.SaveSettingsData();
            CancelRenderingAPIChange();
        }
    }

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

    public void ApplyVFXSettings() {
        localSettings.enableSSR = ssrToggle.isOn;
        localSettings.enableSSAO = ssaoToggle.isOn;

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
        RefreshSensitivitySliderText();

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