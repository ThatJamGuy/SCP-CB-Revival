using EditorAttributes;
using PrimeTween;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class RevivalSessionEngine : MonoBehaviour {
    private static readonly int Quicksave = Animator.StringToHash("Quicksave");
    public static RevivalSessionEngine Instance { get; private set; }
    public static SaveData CurrentSaveData { get; private set; }

    public static int currentDifficulty;
    public static int otherDifficultyFactor;
    public static int currentZone = 1;
    public static int currentPursuit = -1;
    public static bool lczLockdownLifted;

    [Header("Heads Up Display")]
    [SerializeField] private TextMeshProUGUI infoText;

    [Header("Game States")]
    [ReadOnly] public bool playerNear096;

    private InputAction quicksaveAction;

    private Tween infoTextTween;

    #region Unity Callbacks

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        CurrentSaveData = DataSaver.Load<SaveData>("save.json");

        currentDifficulty = CurrentSaveData.difficulty;
        currentZone = CurrentSaveData.currentZone;
    }

    private void Start() {
        quicksaveAction = InputManager.Instance.GetAction("Player", "Quicksave");
    }

    private void Update() {
        // Check for quicksave action
        if (quicksaveAction.triggered) {
            SaveGame(true);
        }
    }

    #endregion

    #region Public Methods

    public void SaveGame(bool playSound = true) {
        CanvasInstance.Instance.HUD_QuickSave.SetTrigger(Quicksave);

        CurrentSaveData.currentDateTime = Time.time.ToString(CultureInfo.CurrentCulture);
        CurrentSaveData.currentGameVersion = "v" + Application.version;
        CurrentSaveData.playerPos = Player.Instance.transform.position;
        CurrentSaveData.playerRot = Player.Instance.transform.rotation;
        CurrentSaveData.currentZone = currentZone;
        CurrentSaveData.lczLockdownLifted = lczLockdownLifted;

        // List of things that should be saved on file for v0.0.6. Map seed & name are already saved on save creation
        //TODO: Save player inventory
        //TODO: Save player blink and stamina stats
        //TODO: Save SCP-173 location
        //TODO: Save SCP-173 state (Is he chasing the player right now?)
        //TODO: Save SCP-106 spawn counter
        //TODO: Save SCP-106 state (Is he chasing the player right now?)
        //TODO: Save SCP-106 location (IF HE IS CHASING THE PLAYER OR OTHER TARGET ONLY)
        //TODO: Save currently playing music (Even if a chase is happening, the triggers don't account for loading)
        //TODO: Save door states. Might put this one off for now, not as important to save open/close states

        DataSaver.Save(CurrentSaveData, "save.json");

        if (playSound) {
            AudioManager.PlayOneShot(AudioEventsHolder.Instance.quicksave01, Player.Instance.transform.position);
        }
    }

    public static void PauseGame() {
        // Set the game's timescale to 0 (Pausing Time.deltaTime) and pause FMOD via the AudioManager
        Time.timeScale = 0f;
        Player.SetCursorState(true);
        Player.Instance.disableInput = true;
        AudioManager.Instance.PauseAllSFX();
    }

    public static void ResumeGame() {
        // Set the game's timescale to 1 (Resuming Time.deltaTime to normal) and resume FMOD via the AudioManager
        Time.timeScale = 1f;
        Player.SetCursorState(false);
        Player.Instance.disableInput = false;
        AudioManager.Instance.ResumeAllSFX();
    }

    public void ShowDeathScreen(string causeOfDeath) {
        MusicManager.Instance.StopAllMusic();
        CanvasInstance.Instance.deathMenu.SetActive(true);
        CanvasInstance.Instance.deathMenuDeathCauseText.text = causeOfDeath;
        Player.SetCursorState(true);
    }

    public void SetTrackBasedOnZone() {
        switch (currentZone) {
            case 0:
                MusicManager.Instance.SetTrack(MusicManager.MusicTrack.Intro, 0);
                break;
            case 1:
                MusicManager.Instance.SetTrack(MusicManager.MusicTrack.LCZ, 0);
                break;
            case 2:
                MusicManager.Instance.SetTrack(MusicManager.MusicTrack.HCZ, 0);
                break;
            case 3:
                // Entrance Zone Music
                break;
        }
    }

    /// <summary>
    /// Call this to make a chase music request and determine if it's all good to go through with it.
    /// </summary>
    /// <param name="priority">A higher number prioritizes this music over other chase music. Set it to -1 to play the current zones music.</param>
    /// <param name="musicTrack">The music track to play. Works the same as MusicManager.SetTrack calls (ie. MusicManager.MusicTrack.*)</param>
    /// <param name="intensity">The intensity of the track IF APPLICABLE. Works the same as MusicManager.SetTrack calls)</param>
    public void PlayChaseTrack(int priority, MusicManager.MusicTrack musicTrack, int intensity = 0) {
        if (priority == -1) { SetTrackBasedOnZone(); return; }
        if (currentPursuit == -1 || priority >= currentPursuit) {
            MusicManager.Instance.SetTrack(musicTrack, intensity);
        }

        // TODO: Add a list of requests that works with the NPCs so if under chase by 2 guys and 1 leaves
        // then it wont forget about the other guy that had already made a music call, as right now it'll set it to the zone music.
    }

    /// <summary>
    /// Displays a heads-up bit of text to the player similar to how Containment Breach did it.
    /// </summary>
    /// <param name="textToDisplay">The text that will appear on screen</param>
    /// <param name="displayDuration">How long the text will be displayed before fading begins</param>
    /// <param name="fadeDuration">How long it takes from the text to fade from full visibility to nothing</param>
    public void NotifyPlayer(string textToDisplay, float displayDuration = 3f, float fadeDuration = 2f) {
        if (!infoText) return; // Don't do anything if the infoText object is missing
        if (infoTextTween.isAlive) infoTextTween.Stop(); // Reset the tween if one is already active

        var startColor = infoText.color; // Create and set startColor to the info text color
        var endColor = startColor; // Create and set endColor to the startColor value
        startColor.a = 1f; // Set the start colors alpha value to 1 (Fully visible)
        endColor.a = 0f; // Set the end colors alpha value to 0 (Not visible)

        infoText.text = textToDisplay;
        infoText.color = startColor;

        infoTextTween = Tween.Delay(displayDuration, () => {
            infoTextTween = Tween.Color(infoText, endColor, fadeDuration);
        });
    }

    #endregion
}