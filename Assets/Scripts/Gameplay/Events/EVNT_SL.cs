using UnityEngine;

public class EVNT_SL : MonoBehaviour {
    [SerializeField] private Door downDoor;
    [SerializeField] private Door upDoor;
    [SerializeField] private Transform temp049Spawn;

    private bool triggeredSpawn;

    public void ChangeMusicToSL() {
        MusicManager.Instance.SetTrack(MusicManager.MusicTrack.SL);
    }

    public void ChangeMusicToLCZ() {
        RevivalSessionEngine.SetGlobalAudioBasedOnZone();
    }

    public void DisableLCZLockdown() {
        // Adding this check so that if lockdown is disabled via console (Or early HCZ entrance) it won't trigger the 049 scene
        RevivalSessionEngine.lczInLockdown = false;

        MusicManager.Instance.SetTrack(MusicManager.MusicTrack.GeneralHorror04); // Fade out to silence

        downDoor.OpenDoor();
        upDoor.CloseDoor();

        // TODO: SCP-049 scene (Using trimmed down version for now)

        // PLAN: All monitors except the middle one turn off, with 079s face on it for a second to draw attention.
        // SCP-049 will be visible walking down the hall and into the SL room with the player.
        // I will probably add his speech later on for time reasons, but I'll just get him to walk up there
        // and look around for a second before leaving and free-roaming.
    }

    // To be called via OnOpening on upDoorNew for the temp 049 spawn scare
    public void TempSpawn049() {
        if (triggeredSpawn || RevivalSessionEngine.lczInLockdown) return;

        EntitySystem.Instance.SpawnEntity(EntitySystem.Instance.cached049Entity, temp049Spawn);
        EntitySystem.Instance.cached049Entity.disabled = false; // Alow 049 to be spawned/moved around now

        upDoor.isBroken = true;
        downDoor.isBroken = true;

        triggeredSpawn = true;
    }

    public static void DisableLCZLockdownViaCMD() {
        if (RevivalSessionEngine.lczInLockdown == true) {
            RevivalSessionEngine.lczInLockdown = false;
            Debug.Log("<color=#00ffff>Light Containment Zone lockdown has been lifted.</color>");
        }

    }
}