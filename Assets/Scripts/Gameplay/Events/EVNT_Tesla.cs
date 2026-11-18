using FMODUnity;
using UnityEngine;

public class EVNT_Tesla : MonoBehaviour {
    [Header("Tesla Toggling References")]
    [SerializeField] private GameObject teslaGateController;
    [SerializeField] private GameObject teslaEffects;
    [SerializeField] private Animator flasherOne;
    [SerializeField] private Animator flasherTwo;
    [SerializeField] private GameObject flasherLightOne;
    [SerializeField] private GameObject flasherLightTwo;
    [SerializeField] private Animator teslaDoor;

    [Header("Autio References")]
    [SerializeField] private StudioEventEmitter klaxonEmitter;
    [SerializeField] private StudioEventEmitter doorEmitter;

    private bool blastDoorClosed;

    private float checkLockdownTimeElapsed;

    private void Update() {
        checkLockdownTimeElapsed += Time.deltaTime;

        // Every 5 seconds check if the lockdown has been lifted and disable already enabled tesla if so
        if (checkLockdownTimeElapsed >= 5) {
            if (RevivalSessionEngine.lczUnlocked && blastDoorClosed) SetTeslaActive(false);
            checkLockdownTimeElapsed = 0;
        }
    }

    public void SetTeslaActive(bool enabled) {
        teslaGateController.SetActive(enabled);
        teslaEffects.SetActive(enabled);
        flasherOne.enabled = enabled;
        flasherTwo.enabled = enabled;
        flasherLightOne.SetActive(enabled);
        flasherLightTwo.SetActive(enabled);

        klaxonEmitter.Play();

        // If lockdown is disabled then don't close the big door
        // If the doors is currently closed then open it back up again
        if (RevivalSessionEngine.lczUnlocked) {
            if (blastDoorClosed == true) {
                teslaDoor.Play("TeslaDoor_Open");
                blastDoorClosed = false;

                doorEmitter.Play();
            }

            return;
        }

        switch (enabled) {
            case true:
                teslaDoor.Play("TeslaDoor_Open");
                blastDoorClosed = false;
                break;
            case false:
                teslaDoor.Play("TeslaDoor_Close");
                blastDoorClosed = true;
                break;
        }

        doorEmitter.Play();
    }
}