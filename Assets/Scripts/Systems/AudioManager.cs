using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour {
    [Header("Volume")]
    [Range(0, 1)] public float masterVolume = 1.0f;
    [Range(0, 1)] public float musicVolume = 1.0f;
    [Range(0, 1)] public float SFXVolume = 1.0f;
    [Range(0, 1)] public float voiceVolume = 1.0f;

    private Bus masterBus;
    private Bus musicBus;
    private Bus sfxBus;
    private Bus voiceBus;

    private readonly HashSet<EventInstance> trackedInstances = new HashSet<EventInstance>();
    private readonly List<StudioEventEmitter> trackedEmitters = new List<StudioEventEmitter>();
    private EventInstance musicInstance;

    private const string SaveFileName = "settings.json";
    private SettingsData settings;

    public static AudioManager instance { get; private set; }

    private void Awake() {
        if (instance != null) Debug.LogError("Multiple AudioManager instances.");
        instance = this;

        foreach (var emitter in FindObjectsByType<StudioEventEmitter>((FindObjectsSortMode)FindObjectsInactive.Include))
            TrackEmitter(emitter);

        masterBus = RuntimeManager.GetBus("bus:/");
        musicBus = RuntimeManager.GetBus("bus:/Music");
        sfxBus = RuntimeManager.GetBus("bus:/SFX");
        voiceBus = RuntimeManager.GetBus("bus:/Voice");
    }

    private void Start() {
        // Start music if MusicManager is available
        if (MusicManager.instance != null)
            StartMusic(MusicManager.instance.musicEvent);

        // Load settings from JSON save and apply volumes/soundtrack
        settings = SaveSystem.Load<SettingsData>(SaveFileName);
        if (settings != null) {
            masterVolume = settings.masterVolume;
            musicVolume = settings.musicVolume;
            SFXVolume = settings.sfxVolume;
            voiceVolume = settings.voiceVolume;

            if (MusicManager.instance != null)
                MusicManager.instance.SetSoundtrack(settings.soundtrack);
        }

        // Initial music state for menu/core scenes
        if (SceneManager.GetSceneByName("Core").isLoaded || SceneManager.GetSceneByName("Menu").isLoaded) {
            SetMusicParameter("MusicState", (int)MusicState.Menu);
        }
    }

    private void Update() {
        masterBus.setVolume(masterVolume);
        musicBus.setVolume(musicVolume);
        sfxBus.setVolume(SFXVolume);
        voiceBus.setVolume(voiceVolume);
    }

    private void TrackInstance(EventInstance e) {
        if (e.isValid()) trackedInstances.Add(e);
    }

    public void TrackEmitter(StudioEventEmitter emitter) {
        if (!trackedEmitters.Contains(emitter))
            trackedEmitters.Add(emitter);

        var inst = emitter.EventInstance;
        if (!inst.isValid()) {
            emitter.Play();
            emitter.Stop();
            inst = emitter.EventInstance;
        }
        if (inst.isValid()) TrackInstance(inst);
    }

    public EventInstance CreateInstance(EventReference reference) {
        var inst = RuntimeManager.CreateInstance(reference);
        TrackInstance(inst);
        return inst;
    }

    public StudioEventEmitter InitializeEventEmitter(EventReference reference, GameObject target) {
        var emitter = target.GetComponent<StudioEventEmitter>();
        emitter.EventReference = reference;
        TrackEmitter(emitter);
        return emitter;
    }

    public void PlaySound(EventReference sound, Vector3 worldPos) {
        RuntimeManager.PlayOneShot(sound, worldPos);
    }

    public void StartMusic(EventReference reference) {
        if (musicInstance.isValid()) {
            musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            musicInstance.release();
        }
        musicInstance = CreateInstance(reference);
        musicInstance.start();
    }

    public void PlayMusic() {
        if (!musicInstance.isValid()) {
            if (MusicManager.instance != null)
                StartMusic(MusicManager.instance.musicEvent);
        }
        else musicInstance.start();
    }

    public void SetMusicParameter(string parameter, float value, bool ignoreSeekSpeed = false) {
        if (musicInstance.isValid())
            musicInstance.setParameterByName(parameter, value, ignoreSeekSpeed);
    }

    public void SetSoundtrackParameter(string parameter, float value, bool ignoreSeekSpeed = false) {
        if (musicInstance.isValid())
            musicInstance.setParameterByName(parameter, value, ignoreSeekSpeed);
    }

    public void StopMusic(FMOD.Studio.STOP_MODE mode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT) {
        if (musicInstance.isValid()) {
            musicInstance.stop(mode);
            musicInstance.release();
        }
    }

    public void PauseGameAudio() {
        foreach (var inst in trackedInstances) {
            if (musicInstance.isValid() && inst.handle == musicInstance.handle) continue;
            inst.setPaused(true);
        }

        foreach (var emitter in trackedEmitters) {
            if (MusicManager.instance != null && emitter.EventReference.Guid == MusicManager.instance.musicEvent.Guid) continue;
            var inst = emitter.EventInstance;
            if (inst.isValid()) inst.setPaused(true);
        }
    }

    public void UnpauseGameAudio() {
        foreach (var inst in trackedInstances) {
            if (musicInstance.isValid() && inst.handle == musicInstance.handle) continue;
            inst.setPaused(false);
        }

        foreach (var emitter in trackedEmitters) {
            if (MusicManager.instance != null && emitter.EventReference.Guid == MusicManager.instance.musicEvent.Guid) continue;
            var inst = emitter.EventInstance;
            if (inst.isValid()) inst.setPaused(false);
        }
    }

    private void OnDestroy() {
        foreach (var inst in trackedInstances) {
            inst.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            inst.release();
        }
        if (musicInstance.isValid()) {
            musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            musicInstance.release();
        }
    }
}
