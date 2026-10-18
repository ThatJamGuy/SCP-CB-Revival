using IngameDebugConsole;
using UnityEngine;

public class EVNT_SL : MonoBehaviour {
    //[SerializeField] private GameObject[] musicChangeTriggers;
    [SerializeField] private Door downDoor;
    [SerializeField] private Door upDoor;
    [SerializeField] private Door upDoorNew;
    [SerializeField] private Transform temp049Spawn;

    private bool triggeredSpawn;

    private void Awake() {
        DebugLogConsole.AddCommand("disable_lockdown", "Disables the LCZ lockdown.", () => DisableLCZLockdown(true));
    }

    public void ChangeMusicToSL() {
        MusicManager.Instance.SetTrack(MusicManager.MusicTrack.SL);
    }

    public void ChangeMusicToLCZ() {
        RevivalSessionEngine.SetGlobalAudioBasedOnZone();
    }

    public void DisableLCZLockdown(bool doneViaConsole) {
        // Adding this check so that if lockdown is disabled via console (Or early HCZ entrance) it won't trigger the 049 scene
        RevivalSessionEngine.lczInLockdown = false;

        if (doneViaConsole) return;

        MusicManager.Instance.SetTrack(MusicManager.MusicTrack.GeneralHorror04); // Fade out to silence

        downDoor.OpenDoor();

        if (upDoor.isOpen)
            upDoor.CloseDoor();
        else {
            upDoor.gameObject.SetActive(false);
            upDoorNew.gameObject.SetActive(true);
        }

        // TODO: SCP-049 scene (Using trimmed down version for now)

        // PLAN: All monitors except the middle one turn off, with 079s face on it for a second to draw attention.
        // SCP-049 will be visible walking down the hall and into the SL room with the player.
        // I will probably add his speech later on for time reasons, but I'll just get him to walk up there
        // and look around for a second before leaving and free-roaming.
    }

    // To be called via OnOpening on upDoorNew for the temp 049 spawn scare
    public void TempSpawn049() {
        if (triggeredSpawn) return;
        EntitySystem.Instance.SpawnEntity(EntitySystem.Instance.cached049Entity, temp049Spawn);
        EntitySystem.Instance.cached049Entity.disabled = false; // Alow 049 to be spawned/moved around now
        triggeredSpawn = true;
    }
}