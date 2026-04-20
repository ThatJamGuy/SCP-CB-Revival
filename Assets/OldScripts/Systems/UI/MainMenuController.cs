using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI versionText;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_InputField seedInputField;

    private const string characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const string optionsSceneName = "Settings";
    private const string SAVE_FILE_NAME = "savefile.json";

    private SaveData currentSaveData;

    #region Unity Callbacks
    private void Start() {
        MusicManager.instance.SetMusicState(MusicState.Menu);

        versionText.text = Application.version;

        AutomaticallyDefineSeed();
    }
    #endregion

    #region Public Methods
    public void StartGame() {
        if (string.IsNullOrEmpty(nameInputField.text)) return;
        //SaveSystem.Save(new SaveData { currentSaveName = nameInputField.text, currentMapSeed = seedInputField.text }, SAVE_FILE_NAME);
        Debug.Log("Starting game with name: " + nameInputField.text + " and seed: " + seedInputField.text);

        SceneController.instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Session, SceneDatabase.Scenes.Session)
            .Load(SceneDatabase.Slots.Game, SceneDatabase.Scenes.Game, setActive: true)
            .Unload(SceneDatabase.Slots.Menu)
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perform();
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
    #endregion

    #region Private Methods
    /// <summary>
    /// Generate a random 5 character seed or choose a random preset seed based on the dice roll done in this method
    /// </summary>
    private void AutomaticallyDefineSeed() {
        int seedTypeChance;
        seedTypeChance = Random.Range(0, 2);

        if (seedTypeChance != 2) {
            seedInputField.text = GenerateRandomString(5);
        }
        else {
            ProvidePresetSeed();
        }
    }

    /// <summary>
    /// Chooses a random preset seed and sets the seed input box to it
    /// </summary>
    private void ProvidePresetSeed() {
        string[] presetSeeds = new string[] {
            "dirtymetal",
            "scpcbr",
            "whatpumpkin",
            "radicallarry",
            "peanut",
            "anythingbutmapgen",
            "tso",
            "thatjamguy",
            "misc",
            "halflife3",
            "bucksplitter",
            "scpcb",
            "scp",
            "subscribe",
            "dzigo"
        };

        seedInputField.text = presetSeeds[Random.Range(0, presetSeeds.Length)];
    }
    #endregion

    #region Helpers
    /// <summary>
    /// Generates a random string based on the defined length
    /// </summary>
    /// <param name="stringLength"></param>
    /// <returns></returns>
    private static string GenerateRandomString(int stringLength) {
        StringBuilder randomString = new StringBuilder();

        for (int i = 0; i < stringLength; i++) {
            char randomChar = characters[Random.Range(0, characters.Length)];
            randomString.Append(randomChar);
        }

        return randomString.ToString();
    }
    #endregion
}