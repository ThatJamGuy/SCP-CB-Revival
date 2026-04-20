using UnityEngine;
using FMODUnity;

/// <summary>
/// Globally accessible class to play One-Shots and manage some other base audio things like volume
/// </summary>
public class AudioManager : MonoBehaviour {
    public static AudioManager Instance { get; private set; }

    #region Unity Callbacks
    private void Awake() {
        // Ensure that only of one these exist to prevent issues
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    #endregion

    #region Public Methods
    // Play a one-shot with no parameters
    public static void PlayOneShot(EventReference soundToPlay, Vector3 worldPosition) {
        RuntimeManager.PlayOneShot(soundToPlay, worldPosition); // Play a sound at a position in world space
    }
    
    // Play a one-shot with a single named parameter
    public static void PlayOneShot(EventReference sound, Vector3 position, string paramName, float paramValue) {
        var soundInstance = RuntimeManager.CreateInstance(sound); // Create a new sound instance
        soundInstance.set3DAttributes(position.To3DAttributes()); // Make this sound play in 3D space
        soundInstance.setParameterByName(paramName, paramValue); // Set the parameter defined while calling the method
        soundInstance.start(); // Play the sound
        soundInstance.release(); // Cleanup leftovers to save memory
    }
    
    // Temporary method to pause/resume all the sounds, as if I remember correctly this didn't really work last time
    public void PauseAllSounds() => RuntimeManager.PauseAllEvents(true);
    public void ResumeAllSounds() => RuntimeManager.PauseAllEvents(false);
    #endregion
}