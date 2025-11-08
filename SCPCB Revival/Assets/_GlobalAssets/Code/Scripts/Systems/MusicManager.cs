using FMODUnity;
using SickDev.CommandSystem;
using UnityEngine;

public enum MusicState {
    CreepyMusic03 = 0,
    LCZ = 1,
    scp106 = 2
}

[RequireComponent(typeof(StudioEventEmitter))]
public class MusicManager : MonoBehaviour {
    private StudioEventEmitter emitter;

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