using FMOD.Studio;
using FMODUnity;
using UnityEngine;

/// <summary>
/// Global script to handle everything music related, current regular loops as of right now
/// </summary>
public class MusicManager : MonoBehaviour {
    public static MusicManager Instance { get; private set; }

    // Preset numbers for the music so whenever changes are needed I can just do them here
    // Prevents changes from breaking other calls for music and makes things generally nicer
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

    [Header("FMOD Settings")]
    [SerializeField] private EventReference musicMasterEvent;

    // Probably gonna rename these later to follow standard const naming conventions
    private const string currTrackParameterName = "CurrentTrack";
    private const string currIntensityParameterName = "IntensityState";
    private const string currSoundtrackParameterName = "Soundtrack";

    private SettingsData settingsData;
    private EventInstance musicInstance;
    private PARAMETER_ID trackParameterID;
    private PARAMETER_ID intensityParameterID;
    private PARAMETER_ID soundtrackParameterID;

    private bool initialized;
    private int currentSoundtrack;

    #region Unity Callbacks
    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        settingsData = DataSaver.Load<SettingsData>("settings.json");
    }

    private void Start() {
        DebugConsole.AddCommand<int, int>("startmusic", "Plays the music track with the given ID and with the given intensity.", SetTrack);
        DebugConsole.AddCommand<int>("setsoundtrack", "Sets the current soundtrack.", SetSoundtrack);
        DebugConsole.AddCommand("stopmusic", "Stops all currently playing music.", StopAllMusic);
    }

    private void OnDestroy() {
        musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        musicInstance.release();
    }
    #endregion

    #region Private Methods

    private void Init() {
        if (initialized) return; // If already initialized do nothing

        musicInstance = RuntimeManager.CreateInstance(musicMasterEvent); // Create the music instance
        musicInstance.getDescription(out var eventDescription); // Create eventDescription from instance description

        // Find the parameter that controls the current track, and create variable parameterDescription 
        eventDescription.getParameterDescriptionByName(currTrackParameterName, out var parameterDescription);
        eventDescription.getParameterDescriptionByName(currIntensityParameterName, out var intensityParameterDescription);
        eventDescription.getParameterDescriptionByName(currSoundtrackParameterName, out var soundtrackParameterDescription);

        trackParameterID = parameterDescription.id; // Set trackParameterId to parameterDescription's ID
        intensityParameterID = intensityParameterDescription.id; // Do the same thing for intensityParameterID
        soundtrackParameterID = soundtrackParameterDescription.id; // And of course the same for this long name guy

        currentSoundtrack = settingsData.soundtrack; // Set the currentSoundtrack value to the one saved in settings

        musicInstance.setParameterByID(soundtrackParameterID, currentSoundtrack);
        musicInstance.start(); // Start playing the default music track
        initialized = true; // Set initialized to true so the thing knows not to initialize again, though why would it
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
    #endregion
}