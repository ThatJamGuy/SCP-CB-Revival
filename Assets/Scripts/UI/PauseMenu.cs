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
    [SerializeField] private TextMeshProUGUI loadGameTextPause;
    [SerializeField] private TextMeshProUGUI loadGameTextDeath;

    private MapGenerator mapGenerator;

    private int currentDifficulty;

    #region Unity Callbacks

    private void OnEnable() {
        // Make sure the load game text is the proper color when opening the pause menu
        RefreshLoadGameText();
    }

    private void Start() {
        // If a GameManager is present, set the gameManager to that instance and grab the current difficulty
        if (RevivalSessionEngine.Instance != null) {
            currentSaveNameText.text = $"<color=grey>Save Name: </color>{RevivalSessionEngine.saveName}";

            currentDifficulty = RevivalSessionEngine.currentDifficulty;
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

    public void ResumeGame() {
        // Toggle menu 0 (Pause menu atm) off by forcing it's state to false. Then resume the game
        menuManager.ToggleMenu(0, false);
        RevivalSessionEngine.ResumeGame();
    }

    private bool CanLoadGame() {
        if (!SavegameEngine.HasActiveSave || !SavegameEngine.HasSave())
            return false;

        // Disallow the player from manually reloading on Keter to prevent save/load abuse
        if (RevivalSessionEngine.currentDifficulty == 2 && !Player.Instance.isDead) return false;

        GameStateData state = SavegameEngine.Get<GameStateData>();

        bool playerDeadOnKeter =
            RevivalSessionEngine.currentDifficulty == 2
            && Player.Instance != null
            && Player.Instance.isDead;

        return state.introCompleted && !playerDeadOnKeter;
    }

    public void LoadGame() {
        if (!CanLoadGame()) return;

        GameLoader.ReloadGame();
    }

    public void ToggleOptionsMenu(bool active) => RevivalRuntimeEngine.ToggleOptionsMenu(active);
    public void ToggleAchievementsMenu(bool active) => RevivalRuntimeEngine.ToggleAchievementsMenu(active);

    public void ReturnToMenu() {
        // Delete the save if the intro was not completed or if the player died on Keter mode
        if (!SavegameEngine.Get<GameStateData>().introCompleted
            || (RevivalSessionEngine.currentDifficulty == 2 && Player.Instance.isDead))
            SavegameEngine.DeleteSave("SaveFile");

        // Kind of resume game as to not get stuck before attempting to load the main menu
        Time.timeScale = 1f;
        AudioManager.Instance.ResumeAllSFX();

        // Prevents sounds from bleeding into the main menu
        RuntimeManager.GetBus("bus:/SFX").stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
        RuntimeManager.GetBus("bus:/Voice").stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);

        // If available, use the SceneController instance to load the menu scene and unload Session and Game scenes
        if (SceneController.instance == null) return;

        SceneController.instance
            .NewTransition()
            .Load(SceneDatabase.Slots.MENU, SceneDatabase.Scenes.MENU, setActive: true)
            .Unload(SceneDatabase.Slots.GAME)
            .Unload(SceneDatabase.Slots.SESSION)
            .WithClearUnusedAssets()
            .WithOverlay()
            .Perform();
    }

    public void RefreshLoadGameText() {
        loadGameTextPause.color = CanLoadGame()
            ? Color.white
            : new Color(0.4f, 0.4f, 0.4f);

        loadGameTextDeath.color = CanLoadGame()
            ? Color.white
            : new Color(0.4f, 0.4f, 0.4f);
    }

    #endregion
}