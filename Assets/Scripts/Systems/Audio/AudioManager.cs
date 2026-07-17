using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Globally accessible class to play One-Shots and manage some other base audio things like volume
/// </summary>
public class AudioManager : MonoBehaviour {
    public static AudioManager Instance { get; private set; }
    private readonly List<EventInstance> activeInstances = new();

    [Header("Volume Sliders")]
    public bool debugVolumes;
    [Range(0, 1)] public float masterVolume = 1;
    [Range(0, 1)] public float musicVolume = 1;
    [Range(0, 1)] public float sfxVolume = 1;
    [Range(0, 1)] public float voiceVolume = 1;

    private Bus masterBus;
    private Bus musicBus;
    private Bus SFXBus;
    private Bus UI_SFXBus;
    private Bus voiceBus;

    #region Unity Callbacks

    private void Awake() {
        // Ensure that only of one these exist to prevent issues
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Set the SFX bus to the SFX mixer channel in FMOD Studio
        masterBus = RuntimeManager.GetBus("bus:/");
        musicBus = RuntimeManager.GetBus("bus:/Music");
        SFXBus = RuntimeManager.GetBus("bus:/SFX");
        UI_SFXBus = RuntimeManager.GetBus("bus:/SFX_UI");
        voiceBus = RuntimeManager.GetBus("bus:/Voice");

        masterVolume = SettingsManager.settingsData.masterVolume;
        musicVolume = SettingsManager.settingsData.musicVolume;
        sfxVolume = SettingsManager.settingsData.sfxVolume;
        voiceVolume = SettingsManager.settingsData.voiceVolume;
        ApplyAllVolumes();
    }

    private void Update() {
        if (debugVolumes) {
            masterBus.setVolume(masterVolume);
            musicBus.setVolume(musicVolume);
            SFXBus.setVolume(sfxVolume);
            UI_SFXBus.setVolume(sfxVolume);
            voiceBus.setVolume(voiceVolume);
        }

        // Prune stopped or invalid instances each frame to keep the list clean
        activeInstances.RemoveAll(i => {
            if (!i.isValid()) return true;
            i.getPlaybackState(out var state);
            return state == PLAYBACK_STATE.STOPPED;
        });
    }

    #endregion

    #region Public Methods

    // Play a one-shot but automatically set to Player transform
    public static void PlayOneShot(EventReference soundToPlay) {
        var soundInstance = RuntimeManager.CreateInstance(soundToPlay); // Create a new sound instance
        soundInstance.set3DAttributes(Player.Instance.transform.position.To3DAttributes());
        soundInstance.start(); // Play the sound
        soundInstance.release(); // Cleanup leftovers to save memory
        Instance.activeInstances.Add(soundInstance); // Add to activeInstances list to keep track of it
    }

    // Play a one-shot with no parameters
    public static void PlayOneShot(EventReference soundToPlay, Vector3 position) {
        var soundInstance = RuntimeManager.CreateInstance(soundToPlay); // Create a new sound instance
        soundInstance.set3DAttributes(position.To3DAttributes()); // Make this sound play in 3D space
        soundInstance.start(); // Play the sound
        soundInstance.release(); // Cleanup leftovers to save memory
        Instance.activeInstances.Add(soundInstance); // Add to activeInstances list to keep track of it
    }

    // Play a one-shot with a single named parameter
    public static void PlayOneShot(EventReference sound, Vector3 position, string paramName, float paramValue) {
        var soundInstance = RuntimeManager.CreateInstance(sound); // Create a new sound instance
        soundInstance.set3DAttributes(position.To3DAttributes()); // Make this sound play in 3D space
        soundInstance.setParameterByName(paramName, paramValue); // Set the parameter defined while calling the method
        soundInstance.start(); // Play the sound
        soundInstance.release(); // Cleanup leftovers to save memory
        Instance.activeInstances.Add(soundInstance); // Add to activeInstances list to keep track of it
    }

    // Set a parameter without doing anything else
    public static void SetParameter(string name, float value) {
        RuntimeManager.StudioSystem.setParameterByName(name, value);
    }

    // Temporary method to pause/resume all the sounds, as if I remember correctly this didn't really work last time
    public void PauseAllSounds() => RuntimeManager.PauseAllEvents(true);
    public void ResumeAllSounds() => RuntimeManager.PauseAllEvents(false);

    // Pauses/Resumes all the sounds under the SFX Mixer channel in FMOD Studio
    // Pause the SFX bus and any tracked active instances
    public void PauseAllSFX() {
        SFXBus.setPaused(true);
        foreach (var instance in activeInstances)
            if (instance.isValid()) instance.setPaused(true);
    }
    // Resume the SFX bus and any tracked active instances
    public void ResumeAllSFX() {
        SFXBus.setPaused(false);
        foreach (var instance in activeInstances)
            if (instance.isValid()) instance.setPaused(false);
    }

    public void ApplyAllVolumes() {
        masterBus.setVolume(masterVolume);
        musicBus.setVolume(musicVolume);
        SFXBus.setVolume(sfxVolume);
        UI_SFXBus.setVolume(sfxVolume);
        voiceBus.setVolume(voiceVolume);
    }

    #endregion
}