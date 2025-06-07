using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ModernMenu : MonoBehaviour
{
    [SerializeField] private GameObject welcomeMessageScreen;

    [SerializeField] private AudioClip menuButtonHover;
    [SerializeField] private Button[] menuMainButtons;

    private AudioSource menuSource;

    private void Awake() {
        menuSource = GetComponent<AudioSource>();

        foreach (var button in menuMainButtons) {
            AddHoverSoundTrigger(button);
        }

        MusicPlayer.Instance.StartMusicByName("Menu");

        // Show welcome message if it hasn't been read yet
        if (welcomeMessageScreen != null) {
            int hasRead = PlayerPrefs.GetInt("ReadWelcomeMessage", 0);
            if (hasRead == 0) {
                welcomeMessageScreen.SetActive(true);
                PlayerPrefs.SetInt("ReadWelcomeMessage", 1);
                PlayerPrefs.Save();
            }
            else {
                welcomeMessageScreen.SetActive(false);
            }
        }
    }

    #region UI Hovering
    private void AddHoverSoundTrigger(Button button) {
        var trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        var entry = new EventTrigger.Entry {
            eventID = EventTriggerType.PointerEnter
        };
        entry.callback.AddListener((eventData) => PlayHoverSound(button));
        trigger.triggers.Add(entry);
    }

    private void PlayHoverSound(Button button) {
        if (menuSource == null || menuButtonHover == null)
            return;
        menuSource.PlayOneShot(menuButtonHover);
    }
    #endregion

    public void QuitGame() {
        Application.Quit();
    }
}