using FMODUnity;
using System.Collections;
using UnityEngine;

public class EVNT_NukeRoom : MonoBehaviour {
    [SerializeField] private EventReference enterRoomStinger;
    [SerializeField] private EventReference leaveRoomStinger;
    [SerializeField] private EventReference activateAnn;
    [SerializeField] private EventReference warheadAnns;
    [SerializeField] private Transform warheadAnnouncerPos;
    [SerializeField] private Lever[] warheadLevers;

    // TODO: These prolly need to be saved later
    private bool alphaWarheadState = true;
    private bool omegaWarheadState = true;
    private bool hasPlayedLeaveStinger;

    private IEnumerator EnterNukeRoomEvent() {
        yield return new WaitForSeconds(1);

        MusicManager.Instance.PlayStinger(enterRoomStinger);
        RevivalSessionEngine.nukeRoomEntered = true;
    }

    private IEnumerator LeaveNukeRoomEvent() {
        yield return new WaitForSeconds(10);

        RevivalSessionEngine.Instance.NotifyPlayer("You've reached the temporary end of the SCP:CBR Demo! Feel free to continue playing, or leave a review on Steam!", 10, 3);
        RevivalRuntimeEngine.Instance.GiveAchievement("achv_demo");
    }

    private IEnumerator TemporarilyDisableLevers() {
        foreach (Lever lever in warheadLevers) {
            lever.ForceStopInteract();
            lever.GetComponent<SphereCollider>().enabled = false;
            lever.enabled = false;
        }

        yield return new WaitForSeconds(10);

        foreach (Lever lever in warheadLevers) {
            lever.ForceStopInteract();
            lever.GetComponent<SphereCollider>().enabled = true;
            lever.enabled = true;
        }
    }

    public void OnUsedElevatorToEnter() {
        if (RevivalSessionEngine.nukeRoomEntered) return;

        StartCoroutine(EnterNukeRoomEvent());
    }

    // If the player left the nuke room with both warheads disabled,
    // play a cool musical stinger definitely not taken from SOMA for now
    public void OnusedElevatorToLeave() {
        if (hasPlayedLeaveStinger) return;

        if (alphaWarheadState == false && omegaWarheadState == false) {
            MusicManager.Instance.PlayStinger(leaveRoomStinger);
            hasPlayedLeaveStinger = true;

            StartCoroutine(LeaveNukeRoomEvent());
        }
    }

    // Toggle alpha warhead state and do things accordingly
    public void ToggleAlpha(bool enabled) {
        alphaWarheadState = enabled;

        switch (alphaWarheadState) {
            case true:
                AudioManager.PlayOneShot(activateAnn, warheadAnnouncerPos.position);
                AudioManager.PlayOneShot(warheadAnns, warheadAnnouncerPos.position, "LocalEventIndex", 0);
                StartCoroutine(TemporarilyDisableLevers());
                break;
            case false:
                AudioManager.PlayOneShot(activateAnn, warheadAnnouncerPos.position);
                AudioManager.PlayOneShot(warheadAnns, warheadAnnouncerPos.position, "LocalEventIndex", 1);
                StartCoroutine(TemporarilyDisableLevers());
                break;
        }
    }

    // Toggle alpha warhead state and do things accordingly
    public void ToggleOmega(bool enabled) {
        omegaWarheadState = enabled;

        switch (omegaWarheadState) {
            case true:
                AudioManager.PlayOneShot(activateAnn, warheadAnnouncerPos.position);
                AudioManager.PlayOneShot(warheadAnns, warheadAnnouncerPos.position, "LocalEventIndex", 2);
                StartCoroutine(TemporarilyDisableLevers());
                break;
            case false:
                AudioManager.PlayOneShot(activateAnn, warheadAnnouncerPos.position);
                AudioManager.PlayOneShot(warheadAnns, warheadAnnouncerPos.position, "LocalEventIndex", 3);
                StartCoroutine(TemporarilyDisableLevers());
                break;
        }
    }

    // I had them merged into one method but remembered that unity events
    // don't like more than one parameters ^^^
}