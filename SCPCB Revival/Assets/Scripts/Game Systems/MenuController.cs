using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("Graphics Settings")]
    [SerializeField] private Toggle fullscreenToggle, vsyncToggle, fpsCounterToggle;
    [SerializeField] private TMP_InputField frameLimitInputField;

    [Header("Sound Settings")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider voiceVolumeSlider;

    [SerializeField] private AudioClip menuMusic, menuInteract, menuFailInteract;
    [SerializeField] private AudioSource interactSource;

    [Header("New Game Settings")]
    [SerializeField] private Toggle enableIntroToggle;

    private void Start()
    {
        MusicPlayer.Instance.StartMusic(menuMusic);

        fullscreenToggle.isOn = PlayerSettings.Instance.isFullscreen;
        vsyncToggle.isOn = PlayerSettings.Instance.vsyncEnabled;
        fpsCounterToggle.isOn = PlayerSettings.Instance.enableFPSCounter;

        masterVolumeSlider.value = PlayerSettings.Instance.masterVolume;
        musicVolumeSlider.value = PlayerSettings.Instance.musicVolume;
        sfxVolumeSlider.value = PlayerSettings.Instance.sfxVolume;
        voiceVolumeSlider.value = PlayerSettings.Instance.voiceVolume;

        GameSettings.Instance.skipIntro = false;
    }

    public void PlayInteractSFX(bool failed)
    {
        interactSource.PlayOneShot(failed ? menuFailInteract : menuInteract);
    }

    public void ApplyGraphics()
    {
        PlayerSettings.Instance.SetFullscreen(fullscreenToggle.isOn);
        PlayerSettings.Instance.SetVsync(vsyncToggle.isOn);
    }

    public void ToggleFPSCounter()
    {
        PlayerSettings.Instance.enableFPSCounter = fpsCounterToggle.isOn;
        PlayerPrefs.SetInt("FPSCounter", fpsCounterToggle.isOn ? 1 : 0);
    }

    public void ToggleIntro() => GameSettings.Instance.skipIntro = enableIntroToggle.isOn;

    public void SetMasterVolume() => PlayerSettings.Instance.SetMasterVolume(masterVolumeSlider.value);
    public void SetMusicVolume() => PlayerSettings.Instance.SetMusicVolume(musicVolumeSlider.value);
    public void SetSFXVolume() => PlayerSettings.Instance.SetSFXVolume(sfxVolumeSlider.value);
    public void SetVoiceVolume() => PlayerSettings.Instance.SetVoiceVolume(voiceVolumeSlider.value);

    public void OpenLink(string linkURL) => Application.OpenURL(linkURL);
    public void QuitGame() => Application.Quit();
}