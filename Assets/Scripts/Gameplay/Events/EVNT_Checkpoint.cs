using FMODUnity;
using System.Collections;
using UnityEngine;

public class EVNT_Checkpoint : MonoBehaviour {
    [Header("Door Mechanics")]
    [SerializeField] private Door masterDoor;
    [SerializeField] private EventReference lockroomSirenEvent;
    [SerializeField] private StudioEventEmitter lockdownAlert;

    private bool lockroomStopped;

    private void Update() {
        if (!RevivalSessionEngine.lczInLockdown && !lockroomStopped) {
            lockdownAlert.Stop();
            lockroomStopped = true;
        }
    }

    public void StartAutoCloseCountdown(int countdown) {
        StartCoroutine(AutoCloseCountdown(countdown));
    }

    public void EnterLCZ() {
        RevivalSessionEngine.SetZone(1);
    }

    public void EnterHCZ() {
        RevivalSessionEngine.SetZone(2);
    }

    public void EnterEZ() {
        // Even though there's stuff in here EZ isn't in the game yet just FYI
        RevivalSessionEngine.SetZone(3);
    }

    private IEnumerator AutoCloseCountdown(int countdown) {
        yield return new WaitForSeconds(countdown);
        if (masterDoor.isOpen) {
            AudioManager.PlayOneShot(lockroomSirenEvent, transform.position);
            yield return new WaitForSeconds(2);
            masterDoor.CloseDoor();
        }
    }
}