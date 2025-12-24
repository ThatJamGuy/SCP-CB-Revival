using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI currentSaveNameText;
    [SerializeField] private TextMeshProUGUI currentSeedText;

    private const string optionsSceneName = "Settings";

    private MapGenerator mapGenerator;

    private void Start() {
        if (SaveSystem.SaveFileExists("savefile.json")) {
            var saveData = SaveSystem.Load<SaveData>("savefile.json");
            currentSaveNameText.text = "<color=grey>Save Name: </color>" + saveData.currentSaveName;
        }

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

    public void ReturnToMenu() {
        GameManager.instance.UnpauseGame();

        SceneController.instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.MainMenu, setActive: true)
            .Unload(SceneDatabase.Slots.Session)
            .Unload(SceneDatabase.Slots.Game)
            .WithClearUnusedAssets()
            .WithOverlay()
            .Perform();
    }
}