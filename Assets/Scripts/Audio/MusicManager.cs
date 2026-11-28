using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour {
    public static MusicManager Instance { get; private set; }

    // Preset numbers for the music so whenever changes are needed I can just do them here
    // Prevents changes from breaking other calls for music and makes things generally nicer
    // Just remember to update this for new tracks as an ordering change in FMOD will break it
    public enum MusicTrack {
        Menu = 0,
        Intro = 1,
        LCZ = 2,
        HCZ = 3,
        SL = 4,
        SCP_049 = 5,
        SCP_096 = 6,
        SCP_106 = 7,
        SCP_173 = 8,
        SCP_914 = 9,
        GeneralHorror01 = 10,
        GeneralHorror02 = 11,
        GeneralHorror03 = 12,
        GeneralHorror04 = 13,
        Credits = 14
    }

    private enum StingerState : byte { None, FadingOut, Playing, FadingIn }

    // Reusable request struct for SCPs that chase the player
    private struct ChaseRequest {
        public EntityId sourceID;
        public int priority;
        public int intensity;
        public int order;
        public MusicTrack track;
    }

    [Header("Music Settings")]
    [SerializeField] private EventReference musicMasterEvent;

    [Header("Ambience Settings")]
    [SerializeField] private EventReference zoneAmbienceMasterEvent;

    public bool IsChaseActive => anyChaseActive;

    public static int CurrentPursuit = -1;

    private const string CURR_TRACK_PARAM = "CurrentTrack";
    private const string CURR_INTENSITY_PARAM = "IntensityState";
    private const string CURR_SOUNDTRACK_PARAM = "Soundtrack";

    private const string CURR_ZONE_AMB_PARAM = "CurrentZone";

    private const float STINGER_FADE_OUT_TIME = 0.5f;
    private const float STINGER_FADE_IN_TIME = 1.5f;

    private readonly List<ChaseRequest> chaseRequests = new List<ChaseRequest>(3);

    private SettingsData settingsData;
    private EventInstance musicInstance;
    private PARAMETER_ID trackParameterID;
    private PARAMETER_ID intensityParameterID;
    private PARAMETER_ID soundtrackParameterID;

    private EventInstance zoneAmbienceInstance;
    private PARAMETER_ID zoneAmbienceParameterID;

    private MusicTrack activeChaseTrack;

    private bool initialized;
    private int currentSoundtrack;
    private int currentTrackIndex = -1;
    private int currentIntensity;

    private bool zoneAmbienceInitialized;
    private int currentZoneAmbience;

    private float relocateTimeElapsed;
    private float relocateTimer = 5;

    // Chase music system vars
    private bool anyChaseActive;
    private int requestCounter;
    private int activeChaseIntensity;

    // One shot musical stinger vars
    private EventInstance stingerInstance;
    private bool stingerActive;
    private bool stingerHasNextTrack;
    private bool instantStinger;
    private int stingerNextTrack;
    private int stingerNextIntensity;
    private StingerState stingerState;
    private EventReference pendingStinger;
    private float musicVolume = 1f;

    #region Unity Lifecycle

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        settingsData = DataSaver.Load<SettingsData>("settings.json");

        CurrentPursuit = -1;
        chaseRequests.Clear();
    }

    private void OnDestroy() {
        if (stingerState == StingerState.Playing) {
            stingerInstance.stop(STOP_MODE.IMMEDIATE);
            stingerInstance.release();
        }

        musicInstance.stop(STOP_MODE.IMMEDIATE);
        musicInstance.release();

        zoneAmbienceInstance.stop(STOP_MODE.IMMEDIATE);
        zoneAmbienceInstance.release();
    }

    private void Update() {
        if (stingerState != StingerState.None) UpdateStinger();

        if (zoneAmbienceInitialized) {
            // Make sure ambience for the right zone is playing; Only does it once to prevent zone ambience call spam
            if (RevivalSessionEngine.Instance != null && currentZoneAmbience != RevivalSessionEngine.currentZone) {
                SetZoneAmbience(RevivalSessionEngine.currentZone);
                currentZoneAmbience = RevivalSessionEngine.currentZone;
            }

            RepositionAmbienceEmitter();
        }
    }

    #endregion

    #region Private Methods

    private void Init() {
        if (initialized) return; // If already initialized do nothing

        musicInstance = RuntimeManager.CreateInstance(musicMasterEvent); // Create the music instance
        musicInstance.getDescription(out var eventDescription); // Create eventDescription from instance description

        // Find the parameter that controls the current track, and create variable parameterDescription 
        eventDescription.getParameterDescriptionByName(CURR_TRACK_PARAM, out var parameterDescription);
        eventDescription.getParameterDescriptionByName(CURR_INTENSITY_PARAM, out var intensityParameterDescription);
        eventDescription.getParameterDescriptionByName(CURR_SOUNDTRACK_PARAM, out var soundtrackParameterDescription);

        trackParameterID = parameterDescription.id; // Set trackParameterId to parameterDescription's ID
        intensityParameterID = intensityParameterDescription.id; // Do the same thing for intensityParameterID
        soundtrackParameterID = soundtrackParameterDescription.id; // And of course the same for this long name guy

        currentSoundtrack = settingsData.soundtrack; // Set the currentSoundtrack value to the one saved in settings

        musicInstance.setParameterByID(soundtrackParameterID, currentSoundtrack);
        musicInstance.start(); // Start playing the default music track
        initialized = true; // Set initialized to true so the thing knows not to initialize again, though why would it
    }

    private void InitZoneAmbience() {
        if (zoneAmbienceInitialized || Player.Instance == null) return; // If already initialized do nothing or if the player is not there

        zoneAmbienceInstance = RuntimeManager.CreateInstance(zoneAmbienceMasterEvent); // Create the zone ambience instance
        zoneAmbienceInstance.set3DAttributes(Player.Instance.transform.To3DAttributes());
        zoneAmbienceInstance.getDescription(out var eventDescription); // Create eventDescription from instance description

        eventDescription.getParameterDescriptionByName(CURR_ZONE_AMB_PARAM, out var zoneParameterDescription);

        zoneAmbienceParameterID = zoneParameterDescription.id;

        zoneAmbienceInstance.start(); // Start playing the default ambience track
        zoneAmbienceInitialized = true; // Set initialized to true so the thing knows not to initialize again, though why would it
    }

    private void RepositionAmbienceEmitter() {
        relocateTimeElapsed += Time.deltaTime;

        if (relocateTimeElapsed >= relocateTimer) {
            if (Player.Instance != null)
                zoneAmbienceInstance.set3DAttributes(Player.Instance.transform.To3DAttributes());
            else
                zoneAmbienceInstance.set3DAttributes(Vector3.zero.To3DAttributes());

            relocateTimeElapsed = 0;
        }
    }

    // Apply the winning chase request based on priority and the queue afterwards
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

        // If we run out of chase requests then set the audio based on the zone
        if (best == -1) {
            if (!anyChaseActive) return;

            anyChaseActive = false;
            CurrentPursuit = -1;

            RevivalSessionEngine.SetGlobalAudioBasedOnZone(bypassChaseCheck: true);

            return;
        }

        ChaseRequest top = chaseRequests[best];
        CurrentPursuit = top.priority;

        if (anyChaseActive && currentTrackIndex == (int)top.track && currentIntensity == top.intensity) return;

        anyChaseActive = true;
        activeChaseTrack = top.track;
        activeChaseIntensity = top.intensity;

        Instance.SetTrack(top.track, top.intensity);
    }

    private void ApplyCurrentTrack() {
        if (currentTrackIndex < 0) return;

        musicInstance.setParameterByID(soundtrackParameterID, currentSoundtrack);
        musicInstance.setParameterByID(intensityParameterID, currentIntensity);
        musicInstance.setParameterByID(trackParameterID, currentTrackIndex);
    }

    private void StartStinger(EventReference stinger, bool hasNext, int nextTrack, int nextIntensity, bool instant) {
        if (stinger.IsNull) return;
        if (!initialized) Init();

        stingerHasNextTrack = hasNext;
        stingerNextTrack = nextTrack;
        stingerNextIntensity = nextIntensity;
        pendingStinger = stinger;

        // Instant stingers skip the fade, rather opting to do both the things at the same time
        if (instant) {
            if (stingerState == StingerState.Playing) {
                stingerInstance.stop(STOP_MODE.IMMEDIATE);
                stingerInstance.release();
            }

            musicVolume = 0f;
            musicInstance.setVolume(0f);
            musicInstance.setPaused(true);
            BeginStingerPlayback();
            return;
        }

        switch (stingerState) {
            case StingerState.None:
            case StingerState.FadingIn:
                // Fade out from whatever the current volume is
                stingerState = StingerState.FadingOut;
                break;

            case StingerState.FadingOut:
                // Already fading out, the new stinger just replaces the pending one
                break;

            case StingerState.Playing:
                // Music is already silent and paused, swap the stinger instantly
                stingerInstance.stop(STOP_MODE.IMMEDIATE);
                stingerInstance.release();
                BeginStingerPlayback();
                break;
        }
    }

    private void BeginStingerPlayback() {
        stingerInstance = RuntimeManager.CreateInstance(pendingStinger);
        stingerInstance.start();
        stingerState = StingerState.Playing;
    }

    private void UpdateStinger() {
        float dt = Time.unscaledDeltaTime;

        switch (stingerState) {
            case StingerState.FadingOut: {
                    musicVolume = STINGER_FADE_OUT_TIME > 0f ? Mathf.MoveTowards(musicVolume, 0f, dt / STINGER_FADE_OUT_TIME) : 0f;
                    musicInstance.setVolume(musicVolume);

                    if (musicVolume <= 0f) {
                        musicInstance.setPaused(true);
                        BeginStingerPlayback();
                    }

                    break;
                }

            case StingerState.Playing: {
                    stingerInstance.getPlaybackState(out PLAYBACK_STATE state);
                    if (state != PLAYBACK_STATE.STOPPED) break;

                    stingerInstance.release();

                    if (stingerHasNextTrack) {
                        currentTrackIndex = stingerNextTrack;
                        currentIntensity = stingerNextIntensity;
                    }

                    // Music is still at volume 0 here, so the track change is inaudible
                    musicInstance.setPaused(false);
                    ApplyCurrentTrack();
                    stingerState = StingerState.FadingIn;
                    break;
                }

            case StingerState.FadingIn: {
                    musicVolume = STINGER_FADE_IN_TIME > 0f
                        ? Mathf.MoveTowards(musicVolume, 1f, dt / STINGER_FADE_IN_TIME)
                        : 1f;
                    musicInstance.setVolume(musicVolume);

                    if (musicVolume >= 1f) stingerState = StingerState.None;
                    break;
                }
        }
    }

    #endregion

    #region Private Extras

    private int IndexOfSource(EntityId id) {
        for (int i = 0; i < chaseRequests.Count; i++) {
            if (chaseRequests[i].sourceID == id) return i;
        }

        return -1;
    }

    #endregion

    #region Public Methods

    public void SetTrack(int trackIndex, int intensity = 0) {
        currentTrackIndex = trackIndex;
        currentIntensity = intensity;

        if (stingerState == StingerState.FadingOut || stingerState == StingerState.Playing) return;

        if (!initialized) Init();
        ApplyCurrentTrack();
    }

    public void SetTrack(MusicTrack trackIndex, int intensity = 0) {
        SetTrack((int)trackIndex, intensity);
    }

    /// <summary>Plays a one-shot stinger, then resumes whichever track should be playing.</summary>
    public void PlayStinger(EventReference stinger) {
        StartStinger(stinger, false, 0, 0, false);
    }

    /// <summary>Plays a one-shot stinger, then switches to the given track.</summary>
    public void PlayStinger(EventReference stinger, MusicTrack nextTrack, int nextIntensity = 0, bool instant = false) {
        StartStinger(stinger, true, (int)nextTrack, nextIntensity, instant);
    }

    public void SetZoneAmbience(int zone = 1) {
        if (!zoneAmbienceInitialized) InitZoneAmbience();
        zoneAmbienceInstance.setParameterByID(zoneAmbienceParameterID, zone);
    }

    public void SetSoundtrack(int soundtrackToUse) {
        if (!initialized) Init(); // Ensure that there is a music instance available

        currentSoundtrack = soundtrackToUse; // Set local currentSoundtrack to the new soundtrack integer
        settingsData.soundtrack = soundtrackToUse; // Set global settingsData.soundtrack to new soundtrack integer
        DataSaver.Save(settingsData, "settings.json"); // Save changes so they are ready on next launch

        musicInstance.setParameterByID(soundtrackParameterID, currentSoundtrack); // Finally set the FMOD parameter
    }

    ///<summary>Request a chase track to play via a priority. Higher priorities play first, and lower priorities play after if still relevant.</summary>
    public void RequestChaseTrack(Object source, int priority, MusicTrack track, int intensity = 0) {
        // If the priority is -1 or lower then release the track since it's basically asking this to do nothing
        if (priority < 0) { ReleaseChaseTrack(source); return; }

        EntityId id = source.GetEntityId();
        int index = IndexOfSource(id);

        ChaseRequest request;
        request.sourceID = id;
        request.priority = priority;
        request.track = track;
        request.intensity = intensity;
        request.order = index >= 0 ? chaseRequests[index].order : requestCounter++;

        if (index >= 0) chaseRequests[index] = request;
        else chaseRequests.Add(request);

        ApplyTopChaseRequest();
    }

    ///<summary>Release the chase track tied to a source. Usually called when the chase ends or the player dies.</summary>
    public void ReleaseChaseTrack(Object source) {
        EntityId id = source.GetEntityId();
        int index = IndexOfSource(id);

        if (index < 0) return;

        int last = chaseRequests.Count - 1;
        chaseRequests[index] = chaseRequests[last];
        chaseRequests.RemoveAt(last);

        ApplyTopChaseRequest();
    }

    public void StopAllMusic() {
        if (!initialized) return;

        if (stingerState == StingerState.Playing) {
            stingerInstance.stop(STOP_MODE.IMMEDIATE);
            stingerInstance.release();
        }

        stingerState = StingerState.None;
        musicVolume = 1f;

        chaseRequests.Clear();
        anyChaseActive = false;
        CurrentPursuit = -1;

        musicInstance.setPaused(false);
        musicInstance.stop(STOP_MODE.ALLOWFADEOUT);
        musicInstance.release();
        initialized = false;
    }

    public void StopAllZoneAmbience() {
        if (!zoneAmbienceInitialized) return;

        zoneAmbienceInstance.stop(STOP_MODE.ALLOWFADEOUT);
        zoneAmbienceInitialized = false;
    }

    #endregion
}