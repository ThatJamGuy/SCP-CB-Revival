using FMODUnity;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour {
    [Header("References")]
    [SerializeField] private TextMeshProUGUI versionText;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_InputField seedInputField;
    [SerializeField] private Toggle[] difficultyCheckboxes;

    [Header("Other References")]
    [SerializeField] private bool enableMenuShakes;
    [SerializeField] private float minShakeTime = 25;
    [SerializeField] private float maxShakeTime = 40f;
    [SerializeField] private EventReference bigBooms;
    [SerializeField] private GameObject shakeDebris;

    private const string CHARACTERS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    private SaveData currentSaveData;

    #region Unity Callbacks

    private void Start() {
        MusicManager.Instance.SetTrack(MusicManager.MusicTrack.Menu);
        DiscordSystems.Instance.ChangeDiscordStatus("In the Main Menu");

        versionText.text = Application.version;



        AutomaticallyDefineSeed();

        if (enableMenuShakes)
            StartCoroutine(PeriodicMenuShake());
    }

    #endregion

    #region Public Methods

    public void StartGame() {
        if (string.IsNullOrEmpty(nameInputField.text)) return;
        DataSaver.Save(new SaveData { currentSaveName = nameInputField.text, currentMapSeed = seedInputField.text }, "save.json");
        Debug.Log("Starting game with name: " + nameInputField.text + " and seed: " + seedInputField.text);

        SceneController.instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Session, SceneDatabase.Scenes.Session)
            .Load(SceneDatabase.Slots.Intro, SceneDatabase.Scenes.Intro, setActive: true)
            .Unload(SceneDatabase.Slots.Menu)
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perform();
    }

    public void LoadPreviousGame() {
        if (DataSaver.DataFileExists("save.json")) {
            var previousSave = DataSaver.Load<SaveData>("save.json");
            if (previousSave.newGame == true) return;

            // TODO: Implement loading of the most recent save
            Debug.Log("TODO: Implement loading of the most recent save");
        }
    }

    public void OpenOptionsMenu() {
        GlobalCanvasInstance.ToggleOptionsMenu(true);
    }

    public void OpenAchievementsMenu() {
        GlobalCanvasInstance.ToggleAchievementsMenu(true);
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
        } else {
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

    private IEnumerator PeriodicMenuShake() {
        if (!enableMenuShakes) StopCoroutine(PeriodicMenuShake());

        yield return new WaitForSeconds(Random.Range(minShakeTime, maxShakeTime));

        if (!enableMenuShakes) StopCoroutine(PeriodicMenuShake());

        shakeDebris.SetActive(true);
        GlobalCameraShake.Instance.ShakeCamera(0.02f, 0f, 4);
        AudioManager.PlayOneShot(bigBooms);

        yield return new WaitForSeconds(5);

        shakeDebris.SetActive(false);

        if (enableMenuShakes)
            StartCoroutine(PeriodicMenuShake());
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
            char randomChar = CHARACTERS[Random.Range(0, CHARACTERS.Length)];
            randomString.Append(randomChar);
        }

        return randomString.ToString();
    }

    #endregion
}