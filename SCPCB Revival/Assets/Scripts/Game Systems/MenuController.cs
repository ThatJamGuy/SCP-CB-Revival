using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MenuController : MonoBehaviour
{
    [Header("Graphics Settings")]
    [SerializeField] private Toggle fullscreenToggle, vsyncToggle;
    [SerializeField] private TMP_InputField frameLimitInputField;
    [SerializeField] private Toggle fpsCounterToggle;

    [Header("Sound Settings")]
    [SerializeField] Slider masterVolumeSlider;
    [SerializeField] Slider musicVolumeSlider;
    [SerializeField] Slider sfxVolumeSlider;
    [SerializeField] AudioMixer gameAudioMixer;

    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip menuInteract;
    [SerializeField] private AudioClip menuFailInteract;

    [SerializeField] private AudioSource interactSource;

    private void Start()
    {
        MusicPlayer.Instance.StartMusic(menuMusic);

        fullscreenToggle.isOn = Screen.fullScreen;

        if(QualitySettings.vSyncCount == 0)
            vsyncToggle.isOn = false;
        else
            vsyncToggle.isOn = true;

        SetMasterVolume(PlayerPrefs.GetFloat("SavedMasterVolume", 100));
        SetMusicVolume(PlayerPrefs.GetFloat("SavedMusicVolume", 100));
        SetSFXVolume(PlayerPrefs.GetFloat("SavedSFXVolume", 100));
    }

    public void PlayInteractSFX(bool failed)
    {
        if (!failed)
            interactSource.PlayOneShot(menuInteract);
        else
            interactSource.PlayOneShot(menuFailInteract);
    }

    public void ToggleFrameLimit()
    {
        frameLimitInputField.gameObject.SetActive(!vsyncToggle.isOn);
    }

    public void ApplyGraphics()
    {
        Screen.fullScreen = fullscreenToggle.isOn;

        if(vsyncToggle.isOn)
            QualitySettings.vSyncCount = 1;
        else
            QualitySettings.vSyncCount = 0;
    }

    public void ToggleFPSCounter() {
        PlayerSettings.Instance.enableFPSCounter = fpsCounterToggle.isOn;
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

    public void OpenLink(string linkURL)
    {
        Application.OpenURL(linkURL);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}