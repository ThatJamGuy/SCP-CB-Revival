using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using EditorAttributes;

/// <summary>
/// Global script to handle everything music related, current regular loops as of right now
/// </summary>
public class MusicManager : MonoBehaviour {
    public static MusicManager Instance { get; private set; }
    
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

        Init();
    }
    
    private void OnDestroy() {
        musicInstance.stop(STOP_MODE.IMMEDIATE);
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

    public void SetSoundtrack(int soundtrackToUse) {
        if (!initialized) Init(); // Ensure that there is a music instance available
        
        currentSoundtrack = soundtrackToUse; // Set local currentSoundtrack to the new soundtrack integer
        settingsData.soundtrack = soundtrackToUse; // Set global settingsData.soundtrack to new soundtrack integer
        DataSaver.Save(settingsData, "settings.json"); // Save changes so they are ready on next launch
        
        musicInstance.setParameterByID(soundtrackParameterID, currentSoundtrack); // Finally set the FMOD parameter
    }

    public void StopAllMusic(STOP_MODE stopMode = STOP_MODE.ALLOWFADEOUT) {
        if (!initialized) return; // If the MusicManager isn't ready yet do nothing
        
        musicInstance.stop(stopMode); // Stop all music using a fade out by default but can be changed
        initialized = false; // No longer initialized, trigger another Init() on next track played
    }
    #endregion
    
    #region Debugging Methods

    // DELETE ALL THESE LATER, ESPECIALLY WHEN THE CONSOLE IS ADDED BACK
    
    [Button("Pick Random Soundtrack")]
    public void PickRandomSoundtrack() {
        var num = Random.Range(0, 3);
        SetSoundtrack(num);
    }
    
    [Button("Play LCZ Music (Subtle)")]
    public void PlayLCZMusic() {
        SetTrack(1);
    }
    
    [Button("Play LCZ Music (Primary)")]
    public void PlayLCZMusicPrimary() {
        SetTrack(1, 1);
    }

    [Button("Play Security Room Music")]
    public void PlaySLMusic() {
        SetTrack(2);
    }

    [Button("Stop All Music")]
    public void StopMusic() {
        StopAllMusic();
    }
    #endregion
}