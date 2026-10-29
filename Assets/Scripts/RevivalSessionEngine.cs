using EditorAttributes;
using PrimeTween;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RevivalSessionEngine : MonoBehaviour {
    public static RevivalSessionEngine Instance { get; private set; }

    private struct ChaseRequest {
        public EntityId sourceId; // FIX: Use EntityId, not int
        public int priority;
        public int intensity;
        public int order; // Lower number means older
        public MusicManager.MusicTrack track;
    }

    public static string saveName = "DefaultSaveName";
    public static string mapSeed = "DefaultSeed";
    public static int currentDifficulty = 0;
    public static int otherDifficultyFactor = 0;
    public static int currentZone = 1;
    public static int currentPursuit = -1;
    public static bool lczInLockdown = true;
    public static bool skipIntro = false;

    [Header("Heads Up Display")]
    [SerializeField] private TextMeshProUGUI infoText;

    [Header("Game States")]
    [ReadOnly] public bool playerNear096;

    private readonly List<ChaseRequest> chaseRequests = new List<ChaseRequest>(5);

    private MusicManager.MusicTrack activeTrack;

    private Tween infoTextTween;

    private bool hasActiveChase;
    private int requestCounter;
    private int activeIntensity;

    #region Unity Lifecycle

    private void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        // Safeguard for direct testing from 3_Game because I can't live with it. Keeps the seed random still and diff on Safe
#if UNITY_EDITOR
        if (!SavegameEngine.HasActiveSave && !SavegameEngine.LoadGame())
            SavegameEngine.NewGame("EditorTest", "", 0, true);
#endif

        SessionData session = SavegameEngine.Session;

        saveName = session.saveName;
        mapSeed = session.mapSeed;
        currentDifficulty = session.difficulty;
        skipIntro = session.skipIntro;

        otherDifficultyFactor = currentDifficulty;

        chaseRequests.Clear();
        currentPursuit = -1;
    }

    private void Start() {
        // Ensure this john is in lockdown every session because it didn't reset when testing the build
        lczInLockdown = true;
    }

    #endregion

    private void ApplyTopChaseRequest() {
        int best = -1;
        int count = chaseRequests.Count;

        for (int i = 0; i < count; i++) {
            if (best == -1) { best = i; continue; }
            ChaseRequest candidate = chaseRequests[i];
            ChaseRequest current = chaseRequests[best];
            if (candidate.priority > current.priority ||
                (candidate.priority == current.priority && candidate.order < current.order)) {
                best = i;
            }
        }

        // No requests left: fall back to zone music
        if (best == -1) {
            if (!hasActiveChase) return;
            hasActiveChase = false;
            currentPursuit = -1;
            SetGlobalAudioBasedOnZone(bypassChaseCheck: true);
            return;
        }

        ChaseRequest top = chaseRequests[best];
        currentPursuit = top.priority;

        // Winner's track unchanged, so don't retrigger
        if (hasActiveChase && activeTrack == top.track && activeIntensity == top.intensity) return;

        hasActiveChase = true;
        activeTrack = top.track;
        activeIntensity = top.intensity;
        MusicManager.Instance.SetTrack(top.track, top.intensity);
    }

    private int IndexOfSource(EntityId id) {
        for (int i = 0; i < chaseRequests.Count; i++) {
            if (chaseRequests[i].sourceId == id) return i;
        }

        return -1;
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
    /// Registers or updates a chase music request from a source. The highest priority active request plays.
    /// Safe to call repeatedly; the track is only changed when the winning request changes.
    /// </summary>
    public void RequestChaseTrack(Object source, int priority, MusicManager.MusicTrack track, int intensity = 0) {
        if (priority < 0) { ReleaseChaseTrack(source); return; }

        EntityId id = source.GetEntityId();
        int index = IndexOfSource(id);

        ChaseRequest request;
        request.sourceId = id;
        request.priority = priority;
        request.track = track;
        request.intensity = intensity;
        request.order = index >= 0 ? chaseRequests[index].order : requestCounter++;

        if (index >= 0) chaseRequests[index] = request;
        else chaseRequests.Add(request);

        ApplyTopChaseRequest();
    }

    /// <summary>
    /// Call when a source stops chasing (lost the player, died, disabled, destroyed).
    /// </summary>
    public void ReleaseChaseTrack(Object source) {
        EntityId id = source.GetEntityId();
        int index = IndexOfSource(id);
        if (index < 0) return;

        // Swap-remove: O(1), order is stored per request so list order doesn't matter
        int last = chaseRequests.Count - 1;
        chaseRequests[index] = chaseRequests[last];
        chaseRequests.RemoveAt(last);

        ApplyTopChaseRequest();
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