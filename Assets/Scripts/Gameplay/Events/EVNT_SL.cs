using FMODUnity;
using UnityEngine;

public class EVNT_SL : MonoBehaviour {
    [SerializeField] private Door downDoor;
    [SerializeField] private Door upDoor;
    [SerializeField] private Transform temp049Spawn;
    [SerializeField] private EventReference taskComplete;

    public void ChangeMusicToSL() {
        if (EntitySystem.AnyEntityChasingPlayer()) return;
        MusicManager.Instance.SetTrack(MusicManager.MusicTrack.SL);
    }

    public void ChangeMusicToLCZ() {
        RevivalSessionEngine.SetGlobalAudioBasedOnZone();
    }

    public void DisableLCZLockdown() {
        RevivalSessionEngine.lczUnlocked = true;

        // Play stinger and fade out to silence
        MusicManager.Instance.PlayStinger(taskComplete, MusicManager.MusicTrack.GeneralHorror04, instant: true);

        downDoor.OpenDoor();
        upDoor.CloseDoor();
    }

    // To be called via OnOpening on upDoorNew for the temp 049 spawn scare
    public void TempSpawn049() {
        if (RevivalSessionEngine.event049Triggered || !RevivalSessionEngine.lczUnlocked) return;

        EntitySystem.Instance.SpawnEntity(EntitySystem.Instance.cached049Entity, temp049Spawn);
        EntitySystem.Instance.cached049Entity.disabled = false;

        upDoor.isBroken = true;
        downDoor.isBroken = true;

        RevivalSessionEngine.event049Triggered = true;
    }

    // Method called from external sources! Disables lockdown and enables SCP-049 spawning
    public static void DisableLCZLockdownViaCMD() {
        if (RevivalSessionEngine.lczUnlocked == false) {
            RevivalSessionEngine.lczUnlocked = true;
            RevivalSessionEngine.event049Triggered = true;
            EntitySystem.Instance.cached049Entity.disabled = false;
            Debug.Log("<color=#00ffff>Light Containment Zone lockdown has been lifted.</color>");
        }

    }
}