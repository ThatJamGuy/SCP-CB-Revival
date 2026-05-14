using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections.Generic;

/// <summary>
/// Globally accessible class to play One-Shots and manage some other base audio things like volume
/// </summary>
public class AudioManager : MonoBehaviour {
    public static AudioManager Instance { get; private set; }

    private readonly List<EventInstance> activeInstances = new();
    
    private Bus SFXBus;

    #region Unity Callbacks
    private void Awake() {
        // Ensure that only of one these exist to prevent issues
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Set the SFX bus to the SFX mixer channel in FMOD Studio
        SFXBus = RuntimeManager.GetBus("bus:/SFX");
    }
    
    private void Update() {
        // Prune stopped or invalid instances each frame to keep the list clean
        activeInstances.RemoveAll(i => {
            if (!i.isValid()) return true;
            i.getPlaybackState(out var state);
            return state == PLAYBACK_STATE.STOPPED;
        });
    }
    #endregion

    #region Public Methods
    // Play a one-shot with no parameters
    // ReSharper disable Unity.PerformanceAnalysis
    public static void PlayOneShot(EventReference soundToPlay, Vector3 position) {
        var soundInstance = RuntimeManager.CreateInstance(soundToPlay); // Create a new sound instance
        soundInstance.set3DAttributes(position.To3DAttributes()); // Make this sound play in 3D space
        soundInstance.start(); // Play the sound
        soundInstance.release(); // Cleanup leftovers to save memory
        Instance.activeInstances.Add(soundInstance); // Add to activeInstances list to keep track of it
    }
    
    // Play a one-shot with a single named parameter
    // ReSharper disable Unity.PerformanceAnalysis
    public static void PlayOneShot(EventReference sound, Vector3 position, string paramName, float paramValue) {
        var soundInstance = RuntimeManager.CreateInstance(sound); // Create a new sound instance
        soundInstance.set3DAttributes(position.To3DAttributes()); // Make this sound play in 3D space
        soundInstance.setParameterByName(paramName, paramValue); // Set the parameter defined while calling the method
        soundInstance.start(); // Play the sound
        soundInstance.release(); // Cleanup leftovers to save memory
        Instance.activeInstances.Add(soundInstance); // Add to activeInstances list to keep track of it
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

    #endregion
}