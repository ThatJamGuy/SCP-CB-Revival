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

        fullscreenToggle.isOn = PlayerGameSettings.Instance.isFullscreen;
        vsyncToggle.isOn = PlayerGameSettings.Instance.vsyncEnabled;
        fpsCounterToggle.isOn = PlayerGameSettings.Instance.enableFPSCounter;

        masterVolumeSlider.value = PlayerGameSettings.Instance.masterVolume;
        musicVolumeSlider.value = PlayerGameSettings.Instance.musicVolume;
        sfxVolumeSlider.value = PlayerGameSettings.Instance.sfxVolume;
        voiceVolumeSlider.value = PlayerGameSettings.Instance.voiceVolume;

        GameSettings.Instance.skipIntro = false;
    }

    public void PlayInteractSFX(bool failed)
    {
        interactSource.PlayOneShot(failed ? menuFailInteract : menuInteract);
    }

    public void ApplyGraphics()
    {
        PlayerGameSettings.Instance.SetFullscreen(fullscreenToggle.isOn);
        PlayerGameSettings.Instance.SetVsync(vsyncToggle.isOn);
    }

    public void ToggleFPSCounter()
    {
        PlayerGameSettings.Instance.enableFPSCounter = fpsCounterToggle.isOn;
        PlayerPrefs.SetInt("FPSCounter", fpsCounterToggle.isOn ? 1 : 0);
    }

    public void ToggleIntro() => GameSettings.Instance.skipIntro = enableIntroToggle.isOn;

    public void SetMasterVolume() => PlayerGameSettings.Instance.SetMasterVolume(masterVolumeSlider.value);
    public void SetMusicVolume() => PlayerGameSettings.Instance.SetMusicVolume(musicVolumeSlider.value);
    public void SetSFXVolume() => PlayerGameSettings.Instance.SetSFXVolume(sfxVolumeSlider.value);
    public void SetVoiceVolume() => PlayerGameSettings.Instance.SetVoiceVolume(voiceVolumeSlider.value);

    public void OpenLink(string linkURL) => Application.OpenURL(linkURL);
    public void QuitGame() => Application.Quit();
}