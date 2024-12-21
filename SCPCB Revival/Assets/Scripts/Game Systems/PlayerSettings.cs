using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class PlayerSettings : MonoBehaviour
{
    public static PlayerSettings Instance;

    [Header("Sound Settings")]
    [SerializeField] Slider masterVolumeSlider;
    [SerializeField] Slider musicVolumeSlider;
    [SerializeField] Slider sfxVolumeSlider;
    [SerializeField] AudioMixer gameAudioMixer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    private void Start() {
        SetMasterVolume(PlayerPrefs.GetFloat("SavedMasterVolume", 100));
        SetMusicVolume(PlayerPrefs.GetFloat("SavedMusicVolume", 100));
        SetSFXVolume(PlayerPrefs.GetFloat("SavedSFXVolume", 100));
    }

    public void SetMasterVolume(float value) {
        if(value < 1) {
            value = 0.001f;
        }

        RefreshSlider(value);
        PlayerPrefs.SetFloat("SavedMasterVolume", value);
        gameAudioMixer.SetFloat("MasterVolume", Mathf.Log10(value / 100) * 20f);
    }

    public void SetMusicVolume(float value) {
        if(value < 1) {
            value = 0.001f;
        }

        PlayerPrefs.SetFloat("SavedMusicVolume", value);
        gameAudioMixer.SetFloat("MusicVolume", Mathf.Log10(value / 100) * 20f);
    }

    public void SetSFXVolume(float value) {
        if(value < 1) {
            value = 0.001f;
        }

        PlayerPrefs.SetFloat("SavedSFXVolume", value);
        gameAudioMixer.SetFloat("SFXVolume", Mathf.Log10(value / 100) * 20f);
    }

    public void SetVolumeFromSlider() {
        SetMasterVolume(masterVolumeSlider.value);
    }

    public void SetMusicVolumeFromSlider() {
        SetMusicVolume(musicVolumeSlider.value);
    }

    public void SetSFXVolumeFromSlider() {
        SetSFXVolume(sfxVolumeSlider.value);
    }

    private void RefreshSlider(float value)  {
        masterVolumeSlider.value = value;
    }
}