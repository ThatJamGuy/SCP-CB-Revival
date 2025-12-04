using FMODUnity;
using SickDev.CommandSystem;
using UnityEngine;

public enum MusicState {
    CreepyMusic03 = 0,
    LCZ = 1,
    scp106 = 2,
    scp173 = 3,
}

[RequireComponent(typeof(StudioEventEmitter))]
public class MusicManager : MonoBehaviour {
    public static MusicManager instance;

    private StudioEventEmitter emitter;

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    private void OnEnable() {
        DevConsole.singleton.AddCommand(new ActionCommand<int>(SetMusicState) { className = "Music" });
    }

    private void Start() {
        emitter = AudioManager.instance.InitializeEventEmitter(FMODEvents.instance.musicRevival, gameObject);
        emitter.Play();
    }

    public void SetMusicState(int state) {
        AudioManager.instance.SetMusicParameter("MusicState", state);

        if (emitter != null) {
            emitter.SetParameter("MusicState", state);
        }
    }

    public void SetMusicState(MusicState state) {
        SetMusicState((int)state);
    }
}