using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI currentSeedText;

    private const string optionsSceneName = "Settings";

    private MapGenerator mapGenerator;

    private void Start() {
        if (MapGenerator.instance != null) {
            mapGenerator = MapGenerator.instance;
        } else return;
        if (mapGenerator.useRandomSeed) {
            currentSeedText.text = "<color=grey>Map Seed: </color>" + mapGenerator.currentSeed;
            return;
        } else {
            currentSeedText.text = "<color=grey>Map Seed: </color>" + mapGenerator.seed;
            return;
        }
    }

    public void ResumeGame() {
        IngameMenuManager.instance.ToggleMenuByID(2);
    }

    public void OpenOptionsScene() {
        if (!SceneManager.GetSceneByName(optionsSceneName).isLoaded)
            SceneManager.LoadSceneAsync(optionsSceneName, LoadSceneMode.Additive);
    }
}