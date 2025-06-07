using UnityEngine;
using UnityEngine.Audio;

public class PlayerGameSettings : MonoBehaviour
{
    public static PlayerGameSettings Instance;

    [Header("Graphics Settings")]
    public bool enableFPSCounter;
    public bool isFullscreen;
    public bool vsyncEnabled;

    [Header("Audio Settings")]
    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;
    public float voiceVolume;

    [SerializeField] private AudioMixer gameAudioMixer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);

        LoadSettings();
    }

    private void LoadSettings()
    {
        isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        vsyncEnabled = PlayerPrefs.GetInt("Vsync", 1) == 1;
        enableFPSCounter = PlayerPrefs.GetInt("FPSCounter", 0) == 1;

        masterVolume = PlayerPrefs.GetFloat("SavedMasterVolume", 100);
        musicVolume = PlayerPrefs.GetFloat("SavedMusicVolume", 100);
        sfxVolume = PlayerPrefs.GetFloat("SavedSFXVolume", 100);
        voiceVolume = PlayerPrefs.GetFloat("SavedVoiceVolume", 100);

        ApplyGraphicsSettings();
        ApplyAudioSettings();
    }

    public void ApplyGraphicsSettings()
    {
        Screen.fullScreen = isFullscreen;
        QualitySettings.vSyncCount = vsyncEnabled ? 1 : 0;
    }

    public void ApplyAudioSettings()
    {
        gameAudioMixer.SetFloat("MasterVolume", Mathf.Log10(masterVolume / 100) * 20f);
        gameAudioMixer.SetFloat("MusicVolume", Mathf.Log10(musicVolume / 100) * 20f);
        gameAudioMixer.SetFloat("SFXVolume", Mathf.Log10(sfxVolume / 100) * 20f);
        gameAudioMixer.SetFloat("VoiceVolume", Mathf.Log10(voiceVolume / 100) * 20f);
    }

    public void SetFullscreen(bool value)
    {
        isFullscreen = value;
        PlayerPrefs.SetInt("Fullscreen", value ? 1 : 0);
        ApplyGraphicsSettings();
    }

    public void SetVsync(bool value)
    {
        vsyncEnabled = value;
        PlayerPrefs.SetInt("Vsync", value ? 1 : 0);
        ApplyGraphicsSettings();
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Max(value, 0.001f);
        PlayerPrefs.SetFloat("SavedMasterVolume", masterVolume);
        ApplyAudioSettings();
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Max(value, 0.001f);
        PlayerPrefs.SetFloat("SavedMusicVolume", musicVolume);
        ApplyAudioSettings();
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Max(value, 0.001f);
        PlayerPrefs.SetFloat("SavedSFXVolume", sfxVolume);
        ApplyAudioSettings();
    }

    public void SetVoiceVolume(float value)
    {
        voiceVolume = Mathf.Max(value, 0.001f);
        PlayerPrefs.SetFloat("SavedVoiceVolume", voiceVolume);
        ApplyAudioSettings();
    }
}
