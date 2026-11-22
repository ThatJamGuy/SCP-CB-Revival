using UnityEngine;

public class EVNT_SL : MonoBehaviour {
    [SerializeField] private Door downDoor;
    [SerializeField] private Door upDoor;
    [SerializeField] private Transform temp049Spawn;

    private bool triggeredSpawn;

    private void Update() {
        // Continuously check if the conditions allow for 049 to spawn so reloading allows him to
        // TODO: Probably move this somewhere else in a spot where we're not checking every frame
        if (RevivalSessionEngine.lczUnlocked && EntitySystem.Instance.cached049Entity.disabled == true) {
            EntitySystem.Instance.cached049Entity.disabled = false;
        }
    }

    public void ChangeMusicToSL() {
        if (EntitySystem.AnyEntityChasingPlayer()) return;
        MusicManager.Instance.SetTrack(MusicManager.MusicTrack.SL);
    }

    public void ChangeMusicToLCZ() {
        RevivalSessionEngine.SetGlobalAudioBasedOnZone();
    }

    public void DisableLCZLockdown() {
        // Adding this check so that if lockdown is disabled via console (Or early HCZ entrance) it won't trigger the 049 scene
        RevivalSessionEngine.lczUnlocked = true;

        MusicManager.Instance.SetTrack(MusicManager.MusicTrack.GeneralHorror04); // Fade out to silence

        downDoor.OpenDoor();
        upDoor.CloseDoor();
    }

    // To be called via OnOpening on upDoorNew for the temp 049 spawn scare
    public void TempSpawn049() {
        if (triggeredSpawn || !RevivalSessionEngine.lczUnlocked) return;

        EntitySystem.Instance.SpawnEntity(EntitySystem.Instance.cached049Entity, temp049Spawn);
        EntitySystem.Instance.cached049Entity.disabled = false; // Alow 049 to be spawned/moved around now

        upDoor.isBroken = true;
        downDoor.isBroken = true;

        triggeredSpawn = true;
    }

    public static void DisableLCZLockdownViaCMD() {
        if (RevivalSessionEngine.lczUnlocked == false) {
            RevivalSessionEngine.lczUnlocked = true;
            Debug.Log("<color=#00ffff>Light Containment Zone lockdown has been lifted.</color>");
        }

    }
}