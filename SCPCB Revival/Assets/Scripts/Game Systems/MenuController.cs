using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main stuff for the Menu, but due to settings also being accessed from the pause menu I'll move all the settings
/// stuff to another script later. Just not right now.
/// </summary>
public class MenuController : MonoBehaviour
{
    [SerializeField] private Toggle fullscreenToggle, vsyncToggle;
    [SerializeField] private TMP_InputField frameLimitInputField;

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

    public void OpenLink(string linkURL)
    {
        Application.OpenURL(linkURL);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}