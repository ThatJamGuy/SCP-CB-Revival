using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour {
    private List<EventInstance> eventInstances;
    private List<StudioEventEmitter> eventEmitters;

    private EventInstance musicEventInstance;

    public static AudioManager instance { get; private set; }

    private void Awake() {
        if (instance != null) {
            Debug.LogError("Found more than one Audio Manager in the scene.");
        }
        instance = this;

        eventInstances = new List<EventInstance>();
        eventEmitters = new List<StudioEventEmitter>();
    }

    private void Start() {
        StartMusic(FMODEvents.instance.musicRevival);
    }

    /// <summary>
    /// Create and start a music EventInstance. The instance is tracked so it will be cleaned up on destroy.
    /// </summary>
    public void StartMusic(EventReference musicEventReference) {
        if (musicEventInstance.isValid()) {
            musicEventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            musicEventInstance.release();
        }

        musicEventInstance = CreateInstance(musicEventReference);
        musicEventInstance.start();
    }

    public EventInstance CreateInstance(EventReference eventReference) {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        eventInstances.Add(eventInstance);
        return eventInstance;
    }

    public StudioEventEmitter InitializeEventEmitter(EventReference eventReference, GameObject emitterGameObject) {
        StudioEventEmitter emitter = emitterGameObject.GetComponent<StudioEventEmitter>();
        emitter.EventReference = eventReference;
        eventEmitters.Add(emitter);
        return emitter;
    }

    public void PlaySound(EventReference sound, Vector3 worldPos) {
        RuntimeManager.PlayOneShot(sound, worldPos);
    }

    public void PlayMusic() {
        if (!musicEventInstance.isValid()) {
            StartMusic(FMODEvents.instance.musicRevival);
        } else {
            musicEventInstance.start();
        }
    }
    /// <summary>
    /// Set a parameter on the active music instance. This is intended for nested track control.
    /// </summary>
    public void SetMusicParameter(string parameterName, float value, bool ignoreSeekSpeed = false) {
        if (musicEventInstance.isValid()) {
            // EventInstance provides setParameterByName
            musicEventInstance.setParameterByName(parameterName, value, ignoreSeekSpeed);
        } else {
            Debug.LogWarning($"Attempted to set music parameter '{parameterName}' but no music instance is active.");
        }
    }

    /// <summary>
    /// Stop and release the current music instance.
    /// </summary>
    public void StopMusic(FMOD.Studio.STOP_MODE mode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT) {
        if (musicEventInstance.isValid()) {
            musicEventInstance.stop(mode);
            musicEventInstance.release();
        }
    }

    private void CleanUp() {
        foreach (EventInstance eventInstance in eventInstances) {
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }
        foreach (StudioEventEmitter emitter in eventEmitters) {
            emitter.Stop();
        }

        if (musicEventInstance.isValid()) {
            musicEventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            musicEventInstance.release();
        }
    }

    private void OnDestroy() {
        CleanUp();
    }
}