using FMOD.Studio;
using FMODUnity;
using IngameDebugConsole;
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

    [Header("Music Settings")]
    [SerializeField] private EventReference musicMasterEvent;

    [Header("Ambience Settings")]
    [SerializeField] private EventReference zoneAmbienceMasterEvent;

    private const string CURR_TRACK_PARAM = "CurrentTrack";
    private const string CURR_INTENSITY_PARAM = "IntensityState";
    private const string CURR_SOUNDTRACK_PARAM = "Soundtrack";

    private const string CURR_ZONE_AMB_PARAM = "CurrentZone";

    private SettingsData settingsData;
    private EventInstance musicInstance;
    private PARAMETER_ID trackParameterID;
    private PARAMETER_ID intensityParameterID;
    private PARAMETER_ID soundtrackParameterID;

    private EventInstance zoneAmbienceInstance;
    private PARAMETER_ID zoneAmbienceParameterID;

    private bool initialized;
    private int currentSoundtrack;

    private bool zoneAmbienceInitialized;
    private int currentZoneAmbience;

    private float relocateTimeElapsed;
    private float relocateTimer = 5;

    #region Unity Callbacks

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        settingsData = DataSaver.Load<SettingsData>("settings.json");
    }

    private void Start() {
        DebugLogConsole.AddCommand<int, int>("play_music", "Plays the music track associated with the given integer identifier, followed by intensity from 0-1.", SetTrack);
        DebugLogConsole.AddCommand("stop_music", "Stops all currently playing music.", StopAllMusic);
        DebugLogConsole.AddCommand("stop_zone_ambience", "Stops all currently playing zone ambience.", StopAllZoneAmbience);
    }

    private void OnDestroy() {
        musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        musicInstance.release();

        zoneAmbienceInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        zoneAmbienceInstance.release();
    }

    private void Update() {
        // Make sure ambience for the right zone is playing; Only does it once to prevent zone ambience call spam
        if (RevivalSessionEngine.Instance != null && currentZoneAmbience != RevivalSessionEngine.currentZone) {
            SetZoneAmbience(RevivalSessionEngine.currentZone);
            currentZoneAmbience = RevivalSessionEngine.currentZone;
        }

        RepositionAmbienceEmitter();
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
        if (zoneAmbienceInitialized) return; // If already initialized do nothing

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
            zoneAmbienceInstance.set3DAttributes(Player.Instance.transform.To3DAttributes());
            relocateTimeElapsed = 0;
        }
    }

    #endregion

    #region Public Methods

    public void SetTrack(int trackIndex, int intensity = 0) {
        if (!initialized) Init(); // Ensure that there is a music instance available

        musicInstance.setParameterByID(soundtrackParameterID, currentSoundtrack); // Set the soundtrack to the right one
        musicInstance.setParameterByID(intensityParameterID, intensity); // Set intensity of the track (LCZ ONLY RN)
        musicInstance.setParameterByID(trackParameterID, trackIndex); // Play the track by setting the parameter
    }

    public void SetTrack(MusicTrack trackIndex, int intensity = 0) {
        if (!initialized) Init(); // Ensure that there is a music instance available

        musicInstance.setParameterByID(soundtrackParameterID, currentSoundtrack); // Set the soundtrack to the right one
        musicInstance.setParameterByID(intensityParameterID, intensity); // Set intensity of the track (LCZ ONLY RN)
        musicInstance.setParameterByID(trackParameterID, ((float)trackIndex)); // Play the track by setting the parameter
    }

    // TODO: I maight use AI nodes or bring back the room script to tell what room/zone/etc. the script is on
    public void SetZoneAmbience(int zone = 1) {
        if (!zoneAmbienceInitialized) InitZoneAmbience();

        // defaults to LCZ ambience for now
        zoneAmbienceInstance.setParameterByID(zoneAmbienceParameterID, zone);
    }

    public void SetSoundtrack(int soundtrackToUse) {
        if (!initialized) Init(); // Ensure that there is a music instance available

        currentSoundtrack = soundtrackToUse; // Set local currentSoundtrack to the new soundtrack integer
        settingsData.soundtrack = soundtrackToUse; // Set global settingsData.soundtrack to new soundtrack integer
        DataSaver.Save(settingsData, "settings.json"); // Save changes so they are ready on next launch

        musicInstance.setParameterByID(soundtrackParameterID, currentSoundtrack); // Finally set the FMOD parameter
    }

    public void StopAllMusic() {
        if (!initialized) return; // If the MusicManager isn't ready yet do nothing

        musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); // Stop all music using a fade out by default but It no work :(
        initialized = false; // No longer initialized, trigger another Init() on next track played
    }

    public void StopAllZoneAmbience() {
        if (!zoneAmbienceInitialized) return;

        zoneAmbienceInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        zoneAmbienceInitialized = false;
    }

    #endregion
}