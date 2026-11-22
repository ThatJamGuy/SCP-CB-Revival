using FMODUnity;
using System.Collections;
using UnityEngine;

public class EVNT_NukeRoom : MonoBehaviour {
    [SerializeField] private EventReference enterRoomStinger;

    private IEnumerator EnterNukeRoomEvent() {
        yield return new WaitForSeconds(1);

        MusicManager.Instance.PlayStinger(enterRoomStinger);
        RevivalSessionEngine.nukeRoomEntered = true;
    }

    public void OnUsedElevatorToEnter() {
        if (RevivalSessionEngine.nukeRoomEntered) return;

        StartCoroutine(EnterNukeRoomEvent());
    }
}