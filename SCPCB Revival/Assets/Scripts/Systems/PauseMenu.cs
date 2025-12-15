using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour {
    private const string optionsSceneName = "Settings";

    public void ResumeGame() {
        IngameMenuManager.instance.ToggleMenuByID(2);
    }

    public void OpenOptionsScene() {
        if (!SceneManager.GetSceneByName(optionsSceneName).isLoaded)
            SceneManager.LoadSceneAsync(optionsSceneName, LoadSceneMode.Additive);
    }
}