using FMODUnity;
using System.Collections;
using UnityEngine;

public class EVNT_Storeroom : MonoBehaviour {
    [Header("Audio")]
    [SerializeField] private EventReference pain01;
    //[SerializeField] private EventReference pain02;
    [SerializeField] private EventReference doorSlam;

    [Header("References")]
    [SerializeField] private Actor_Generic manA;
    [SerializeField] private Actor_Generic manB;
    [SerializeField] private Transform doorPos;
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private LightFlicker[] roomLights;

    #region Private Methods

    private IEnumerator EventCoroutine() {
        manA.Speak(pain01);
        yield return new WaitForSeconds(2f);

        foreach (var light in roomLights) {
            light.gameObject.SetActive(false);
        }

        manA.PlayAnimation("173Death01");

        yield return new WaitForSeconds(1);

        foreach (var light in roomLights) {
            light.gameObject.SetActive(true);
        }
    }

    #endregion

    #region Public Methods

    public void TriggerEvent() {
        AudioManager.PlayOneShot(doorSlam, doorPos.position);
        doorAnimator.Play("OfficeDoor_ScriptedBurstOpen");
        manA.PlayAnimation("FallBack");
        StartCoroutine(EventCoroutine());
    }

    #endregion
}