using TMPro;
using UnityEngine;

public class MainMenuController : MonoBehaviour {
    [SerializeField] private int menuMusicID = 0;
    [SerializeField] private TextMeshProUGUI versionText;

    private void Start() {
        MusicManager.instance.SetMusicState(menuMusicID);
        AmbienceController.Instance.currentZone = -1;

        versionText.text = Application.version;
    }

    public void OpenLink(string link) {
        Application.OpenURL(link);
    }

    public void QuitGame() {
        Application.Quit();
    }
}