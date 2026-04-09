using FMODUnity;
using System.Collections;
using UnityEngine;

public class EVNT_Checkpoint : MonoBehaviour {
    [SerializeField] private Door masterDoor;
    [SerializeField] private EventReference lockroomSirenEvent;

    public void StartAutoCloseCountdown(int countdown) {
        StartCoroutine(AutoCloseCountdown(countdown));
    }

    private IEnumerator AutoCloseCountdown(int countdown) {
        yield return new WaitForSeconds(countdown);
        if (masterDoor.isOpen) {
            AudioManager.instance.PlaySound(lockroomSirenEvent, transform.position);
            yield return new WaitForSeconds(2);
            masterDoor.CloseDoor();
        }
    }
}