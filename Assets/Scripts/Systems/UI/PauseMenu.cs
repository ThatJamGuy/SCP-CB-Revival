using FMODUnity;
using TMPro;
using UnityEngine;

/// <summary>
/// Local script to handle stuff for the pause menu, such as displaying the basic save stats and opening menus
/// </summary>
public class PauseMenu : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI currentSaveNameText;
    [SerializeField] private TextMeshProUGUI currentSeedText;
    [SerializeField] private TextMeshProUGUI currentDifficultyText;

    [Header("References")]
    [SerializeField] private MenuManager menuManager;

    private GameManager gameManager;
    private MapGenerator mapGenerator;

    private int currentDifficulty;

    #region Unity Callbacks

    private void OnEnable() {
        // So if the InputManager thinks we have a controller plugged in, then select resume button so it works
        if (InputManager.Instance.UsingController) {
            CanvasInstance.Instance.controllerTooltips.SetActive(true);
            CanvasInstance.Instance.resumeButton.Select();
        } else {
            CanvasInstance.Instance.controllerTooltips.SetActive(false);
        }
    }

    private void Start() {
        // If a file called savefile.json exists, load the current save name from that to display it properly
        if (DataSaver.DataFileExists("savefile.json")) {
            var saveData = DataSaver.Load<SaveData>("savefile.json");
            currentSaveNameText.text = "<color=grey>Save Name: </color>" + saveData.currentSaveName;
        }

        // If a GameManager is present, set the gameManager to that instance and grab the current difficulty
        if (GameManager.Instance != null) {
            gameManager = GameManager.Instance;
            currentDifficulty = gameManager.currentDifficulty;
        }

        // Give the current difficulty the appropriate color when displaying it in the pause menu
        currentDifficultyText.text = currentDifficulty switch {
            0 => "<color=grey>Difficulty:</color> <color=green>Easy</color>",
            1 => "<color=grey>Difficulty:</color> <color=yellow>Euclid</color>",
            2 => "<color=grey>Difficulty:</color> <color=red>Keter</color>",
            _ => currentDifficultyText.text
        };

        // If the MapGenerator is present, display the proper seed. Otherwise, use the dev default
        if (MapGenerator.Instance != null) {
            mapGenerator = MapGenerator.Instance;
        } else return;
        if (mapGenerator.useRandomSeed) {
            currentSeedText.text = "<color=grey>Map Seed: </color>" + mapGenerator.currentSeed;
        } else {
            currentSeedText.text = "<color=grey>Map Seed: </color>" + mapGenerator.seed;
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Public method to resume the game while in a paused state. Should be accessed via buttons
    /// </summary>
    public void ResumeGame() {
        // Toggle menu 0 (Pause menu atm) off by forcing it's state to false. Then resume the game
        menuManager.ToggleMenu(0, false);
        GameManager.ResumeGame();
    }

    /// <summary>
    /// Public method to switch the active state of the achievements menu, mostly via that one button in pause menu
    /// </summary>
    /// <param name="active">Boolean whether the achievements screen should be set to active</param>
    public void ToggleAchievementsMenu(bool active) {
        GlobalCanvasInstance.ToggleAchievementsMenu(active);
    }

    /// <summary>
    /// Public method to clean up some game stuff and return to the main menu. Should be accessed via buttons
    /// </summary>
    public void ReturnToMenu() {
        // Resume game as to not get stuck before attempting to load the main menu
        GameManager.ResumeGame();

        // Prevents sounds from bleeding into the main menu
        RuntimeManager.GetBus("bus:/").stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);

        // If available, use the SceneController instance to load the menu scene and unload Session and Game scenes
        if (SceneController.instance == null) return;
        SceneController.instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.MainMenu, setActive: true)
            .Unload(SceneDatabase.Slots.Intro)
            .Unload(SceneDatabase.Slots.Session)
            .Unload(SceneDatabase.Slots.Game)
            .WithClearUnusedAssets()
            .WithOverlay()
            .Perform();
    }
    #endregion
}