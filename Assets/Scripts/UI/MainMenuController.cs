using FMODUnity;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour {
    [Header("Session Setup")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_InputField seedInputField;
    [SerializeField] private Toggle[] modeToggles; // 0 - Safe, 1 - Euclid, 2 - Keter
    [SerializeField] private TextMeshProUGUI diffDesc;
    [SerializeField, TextArea] private string safeDesc, euclidDesc, keterDesc;
    [SerializeField] private Toggle skipIntroToggle;

    [Header("Menu Shakes")]
    [SerializeField] private bool enableMenuShakes;
    [SerializeField] private float minShakeTime = 25;
    [SerializeField] private float maxShakeTime = 40f;
    [SerializeField] private EventReference bigBooms;
    [SerializeField] private GameObject shakeDebris;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI versionText;
    [SerializeField] private StudioEventEmitter menuClick;

    private const string CHARACTERS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    #region Unity Lifecycle

    private void Start() {
        MusicManager.Instance.SetTrack(MusicManager.MusicTrack.Menu);
        RevivalRuntimeEngine.Instance.ChangeDiscordStatus("In the Main Menu");
        versionText.text = "v" + Application.version;

        AutomaticallyDefineSeed();
        SetDifficulty(0);

        if (enableMenuShakes) StartCoroutine(PeriodicMenuShake());
    }

    #endregion

    #region Private Methods

    // Generate a random 5 character seed or choose a random preset seed based on the dice roll done in this method
    private void AutomaticallyDefineSeed() {
        int seedTypeChance;
        seedTypeChance = Random.Range(0, 10);

        if (seedTypeChance <= 7) {
            seedInputField.text = GenerateRandomString(5);
        } else {
            ProvidePresetSeed();
        }
    }

    // Chooses a random preset seed and sets the seed input box to it
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
            "dzigo",
            "drsmokes",
            "shadow",
            "snoe",
            "yomama",
            "atotastic",
            "mashelux",
            "kriswithak",
            "press2place",
            "goboy",
            "krimbopple",
            "jdm",
            "bob",
            "brine",
            "nikki",
            "printer",
            "number1clerklover",
            "21kdark",
            "xtrithx",
            "snapped",
            "poppopicard",
            "jdk",
            "spaghetti",
            "leejihyeok",
            "shahir",
            "skev",
            "seer",
            "salvage",
            "simoxus",
            "smokes",
            "fathergaslight",
            "girlfriendwhen"
        };

        seedInputField.text = presetSeeds[Random.Range(0, presetSeeds.Length)];
    }

    // As the name implies, occasionally shakes the panoramic main menu camera for cool things and stuff reasons
    private IEnumerator PeriodicMenuShake() {
        if (!enableMenuShakes) StopCoroutine(PeriodicMenuShake());

        yield return new WaitForSeconds(Random.Range(minShakeTime, maxShakeTime));

        if (!enableMenuShakes) StopCoroutine(PeriodicMenuShake());

        shakeDebris.SetActive(true);
        RevivalRuntimeEngine.Instance.ShakeCamera(0.02f, 0f, 4);
        AudioManager.PlayOneShot(bigBooms);

        yield return new WaitForSeconds(5);

        shakeDebris.SetActive(false);

        if (enableMenuShakes) StartCoroutine(PeriodicMenuShake());
    }

    #endregion

    #region Private Utilities

    // Generates a random string based on the defined length
    private static string GenerateRandomString(int stringLength) {
        StringBuilder randomString = new StringBuilder();

        for (int i = 0; i < stringLength; i++) {
            char randomChar = CHARACTERS[Random.Range(0, CHARACTERS.Length)];
            randomString.Append(randomChar);
        }

        return randomString.ToString();
    }

    private int GetDifficulty() {
        for (int i = 0; i < modeToggles.Length; i++)
            if (modeToggles[i].isOn) return i;

        return 1;
    }

    #endregion

    #region Public Methods

    public void StartGame() {
        if (string.IsNullOrEmpty(nameInputField.text)) return;

        if (!SavegameEngine.NewGame(nameInputField.text, seedInputField.text, GetDifficulty(), skipIntroToggle.isOn)) {
            return;
        }

        Debug.Log("Starting game with name: " + nameInputField.text + " and seed: " + seedInputField.text);

        SceneController.instance
            .NewTransition()
            .Load(SceneDatabase.Slots.SESSION, SceneDatabase.Scenes.SESSION)
            .Load(SceneDatabase.Slots.GAME, SceneDatabase.Scenes.GAME, setActive: true)
            .Unload(SceneDatabase.Slots.MENU)
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perform();
    }

    public void SetDifficulty(int diff) {
        switch (diff) {
            case 0:
                modeToggles[1].SetIsOnWithoutNotify(false);
                modeToggles[2].SetIsOnWithoutNotify(false);
                diffDesc.text = safeDesc;
                break;
            case 1:
                modeToggles[0].SetIsOnWithoutNotify(false);
                modeToggles[2].SetIsOnWithoutNotify(false);
                diffDesc.text = euclidDesc;
                break;
            case 2:
                modeToggles[0].SetIsOnWithoutNotify(false);
                modeToggles[1].SetIsOnWithoutNotify(false);
                diffDesc.text = keterDesc;
                break;
        }

        menuClick.Play();
    }

    public void OpenOptionsMenu() {
        RevivalRuntimeEngine.ToggleOptionsMenu(true);
    }

    public void OpenAchievementsMenu() {
        RevivalRuntimeEngine.ToggleAchievementsMenu(true);
    }

    public void OpenLink(string link) {
        Application.OpenURL(link);
    }

    public void QuitGame() {
        Application.Quit();
    }

    #endregion
}