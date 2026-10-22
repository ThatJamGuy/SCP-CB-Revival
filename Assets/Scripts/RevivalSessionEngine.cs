using EditorAttributes;
using PrimeTween;
using TMPro;
using UnityEngine;

public class RevivalSessionEngine : MonoBehaviour {
    public static RevivalSessionEngine Instance { get; private set; }

    public static int currentDifficulty;
    public static int otherDifficultyFactor;
    public static int currentZone = 1;
    public static int currentPursuit = -1;
    public static bool canSave;
    public static bool lczInLockdown = true;

    [Header("Heads Up Display")]
    [SerializeField] private TextMeshProUGUI infoText;

    [Header("Game States")]
    [ReadOnly] public bool playerNear096;

    private Tween infoTextTween;

    #region Unity Lifecycle

    private void Awake() {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        Instance = this;
    }

    private void Start() {
        // Set other difficulty factor value based on the current session difficulty
        switch (currentDifficulty) {
            case 0:
                otherDifficultyFactor = 0;
                break;
            case 1:
                otherDifficultyFactor = 1;
                break;
            case 2:
                otherDifficultyFactor = 2;
                break;
        }

        // Ensure this john is in lockdown every session because it didn't reset when testing the build
        lczInLockdown = true;
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
        // Manually set currentZone to the zone parameter that was passed through,
        // as calling this likely means we want it something really specific anyway
        currentZone = zone;
        SetGlobalAudioBasedOnZone(zone, setAmbience, setMusic);
    }

    /// <summary>
    /// Call this to make a chase music request and determine if it's all good to go through with it.
    /// </summary>
    /// <param name="priority">A higher number prioritizes this music over other chase music. Set it to -1 to play the current zones music.</param>
    /// <param name="musicTrack">The music track to play. Works the same as MusicManager.SetTrack calls (ie. MusicManager.MusicTrack.*)</param>
    /// <param name="intensity">The intensity of the track IF APPLICABLE. Works the same as MusicManager.SetTrack calls)</param>
    public void PlayChaseTrack(int priority, MusicManager.MusicTrack musicTrack, int intensity = 0) {
        if (priority == -1) { SetGlobalAudioBasedOnZone(); return; }
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

    public void DebugDispalyZone() {
        Debug.Log("Current estimated zone: " + currentZone);
    }
}