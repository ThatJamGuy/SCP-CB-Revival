using FMODUnity;
using UnityEngine;

public enum MusicState {
    Menu = 0,
    CreepyMusic03 = 1,
    LCZ = 2,
    scp106 = 3,
    scp173 = 4,
}

public class MusicManager : MonoBehaviour {
    public static MusicManager instance;

    public EventReference musicEvent;

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    private void Start() {
        if (DevConsole.Instance == null) return;
        DevConsole.Instance.Add<int>("set_music_state", state => SetMusicState(state));
        DevConsole.Instance.Add<int>("set_soundtrack", soundtrackID => SetSoundtrack(soundtrackID));
    }

    public void SetSoundtrack(int soundtrackID) {
        AudioManager.instance.SetSoundtrackParameter("SoundtrackID", soundtrackID);
    }

    public void SetMusicState(int state) {
        AudioManager.instance.SetMusicParameter("MusicState", state);
    }

    public void SetMusicState(MusicState state) {
        AudioManager.instance.SetMusicParameter("MusicState", (int)state);
    }
}