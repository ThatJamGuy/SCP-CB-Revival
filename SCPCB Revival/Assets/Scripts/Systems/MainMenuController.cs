using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI versionText;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_InputField seedInputField;

    private const string optionsSceneName = "Settings";
    private const string SAVE_FILE_NAME = "savefile.json";

    private SaveData currentSaveData;

    private void Start() {
        AmbienceController.Instance.currentZone = -1;
        MusicManager.instance.SetMusicState(MusicState.Menu);

        versionText.text = Application.version;

        ProvidePresetSeed();
    }

    // Set the default map seed to a random preset seed from a list I quickly threw together
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
            "subscribe"
        };

        seedInputField.text = presetSeeds[Random.Range(0, presetSeeds.Length)];
    }

    public void StartGame() {
        if (string.IsNullOrEmpty(nameInputField.text)) return;
        SaveSystem.Save(new SaveData { currentSaveName = nameInputField.text, currentMapSeed = seedInputField.text }, SAVE_FILE_NAME);
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
}