using EditorAttributes;
using PrimeTween;
using TMPro;
using UnityEngine;

public class RevivalSessionEngine : MonoBehaviour, ISaveable {
    public static RevivalSessionEngine Instance { get; private set; }

    public static string saveName = "DefaultSaveName";
    public static string mapSeed = "DefaultSeed";
    public static int currentDifficulty = 0;
    public static int otherDifficultyFactor = 0;
    public static int currentZone = 1;
    public static bool lczUnlocked;
    public static bool skipIntro;
    public static bool event049Triggered;
    public static bool nukeRoomEntered;

    [Header("Heads Up Display")]
    [SerializeField] private TextMeshProUGUI infoText;

    [Header("Game States")]
    [ReadOnly] public bool playerNear096;

    private SaveData saveData;

    private Tween infoTextTween;

    #region Unity Lifecycle

    private void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        SaveGameEngine.Register(this);
        saveData = SaveGameEngine.Get<SaveData>();

        saveName = saveData.saveName;
        mapSeed = saveData.mapSeed;
        currentDifficulty = saveData.difficulty;
        skipIntro = saveData.skipIntro;
        currentZone = saveData.currentZone;
        lczUnlocked = saveData.lczUnlocked;
        event049Triggered = saveData.event049Triggered;
        nukeRoomEntered = saveData.enteredNukeRoom;

        otherDifficultyFactor = currentDifficulty;
    }

    private void OnDestroy() {
        SaveGameEngine.Unregister(this);
        if (Instance == this) Instance = null;
    }

    #endregion

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
        CanvasInstance.Instance.pauseMenu.RefreshLoadGameText();
        Player.SetCursorState(true);
    }

    // for getting the current zone and applying audio changes
    // Automatically called by SetZone()
    public static void SetGlobalAudioBasedOnZone(int zone = -1, bool setAmbience = true, bool setMusic = true, bool bypassChaseCheck = false) {
        int targetZone = zone == -1 ? currentZone : zone;

        if (zone != -1) currentZone = targetZone; // only update if explicitly setting

        // Skip the scan entirely when music is already off or the check is bypassed
        if (setMusic && !bypassChaseCheck && EntitySystem.AnyEntityChasingPlayer()) setMusic = false;


        switch (targetZone) {
            case 0:
                if (setAmbience) MusicManager.Instance.SetZoneAmbience(0);
                if (setMusic) MusicManager.Instance.SetTrack(MusicManager.MusicTrack.Intro, 0);
                break;
            case 1:
                if (setAmbience) MusicManager.Instance.SetZoneAmbience(1);
                if (setMusic) MusicManager.Instance.SetTrack(MusicManager.MusicTrack.LCZ, 0);
                break;
            case 2:
                if (setAmbience) MusicManager.Instance.SetZoneAmbience(2);
                if (setMusic) MusicManager.Instance.SetTrack(MusicManager.MusicTrack.HCZ, 0);
                break;
            case 3:
                if (setAmbience) MusicManager.Instance.SetZoneAmbience(3);
                break;
        }
    }

    /// <summary>
    /// Sets the global audio state for the specified zone and updates ambience and music based on the provided flags.
    /// </summary>
    /// <remarks>Delegates to SetGlobalAudioBasedOnZone. Default values: setAmbience and setMusic are
    /// true.</remarks>
    /// <param name="zone">Identifier of the zone to apply.</param>
    /// <param name="setAmbience">Whether to update ambience audio when changing the zone.</param>
    /// <param name="setMusic">Whether to update music playback when changing the zone.</param>
    public static void SetZone(int zone, bool setAmbience = true, bool setMusic = true) {
        currentZone = zone;
        SaveGameEngine.Get<SaveData>().currentZone = zone;
        SetGlobalAudioBasedOnZone(zone, setAmbience, setMusic);
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

    public void PrepareSaveableValues() {
        saveData.currentZone = currentZone;
        saveData.lczUnlocked = lczUnlocked;
        saveData.event049Triggered = event049Triggered;
        saveData.enteredNukeRoom = nukeRoomEntered;
    }

    public void DebugDispalyZone() {
        Debug.Log("Current estimated zone: " + currentZone);
    }
}