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

    public static AudioManager instance { get; private set; }

    private void Awake() {
        if (instance != null) Debug.LogError("Multiple AudioManager instances.");
        instance = this;

        foreach (var emitter in FindObjectsByType<StudioEventEmitter>(0))
            TrackEmitter(emitter);

        masterBus = RuntimeManager.GetBus("bus:/");
        musicBus = RuntimeManager.GetBus("bus:/Music");
        sfxBus = RuntimeManager.GetBus("bus:/SFX");
        voiceBus = RuntimeManager.GetBus("bus:/Voice");
    }

    private void Start() {
        StartMusic(MusicManager.instance.musicEvent);

        // Since it doesn't like to behave in the menu controllers start method, I decided to put this initial stuff here.
        // Doesn't need to be anywhere else either because the average player will start in the menu,
        // while the gigachad dev (Me) has to change it everywhere else by default unless loading in from the main menu.
        if (SceneManager.GetSceneByName("Menu").isLoaded) {
            SetMusicParameter("MusicState", (int)MusicState.Menu);
            MusicManager.instance.SetSoundtrack(PlayerPrefs.GetInt("opt_soundtrack"));
            masterVolume = PlayerPrefs.GetFloat("opt_volume_master");
            musicVolume = PlayerPrefs.GetFloat("opt_volume_music");
            SFXVolume = PlayerPrefs.GetFloat("opt_volume_sfx");
            voiceVolume = PlayerPrefs.GetFloat("opt_volume_voice");
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
        if (!musicInstance.isValid()) StartMusic(MusicManager.instance.musicEvent);
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
            if (emitter.EventReference.Guid == MusicManager.instance.musicEvent.Guid) continue;
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
            if (emitter.EventReference.Guid == MusicManager.instance.musicEvent.Guid) continue;
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
