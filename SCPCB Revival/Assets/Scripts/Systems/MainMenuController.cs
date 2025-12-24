using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI versionText;

    private const string optionsSceneName = "Settings";

    private void Start() {
        //MusicManager.instance.SetMusicState(MusicState.Menu);
        AmbienceController.Instance.currentZone = -1;

        versionText.text = Application.version;
    }

    public void OpenOptionsScene() {
        if (!SceneManager.GetSceneByName(optionsSceneName).isLoaded)
            SceneManager.LoadSceneAsync(optionsSceneName, LoadSceneMode.Additive);
    }

    public void OpenLink(string link) {
        Application.OpenURL(link);
    }

    public void QuitGame() {
        Application.Quit();
    }
}