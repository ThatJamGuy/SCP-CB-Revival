using UnityEngine;

public class EVNT_914 : MonoBehaviour {
    public void OnRoomEntered() {
        if (EntitySystem.AnyEntityChasingPlayer()) return;
        MusicManager.Instance.SetTrack(MusicManager.MusicTrack.SCP_914);
    }

    public void OnRoomExited() {
        if (EntitySystem.AnyEntityChasingPlayer()) return;
        RevivalSessionEngine.SetGlobalAudioBasedOnZone();
    }
}