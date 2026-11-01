using FMODUnity;
using System.Collections;
using UnityEngine;

public class EVNT_Testroom : MonoBehaviour {
    [Header("References")]
    [SerializeField] private Transform postGlassShatter173Pos;
    [SerializeField] private GameObject glassObject;
    [SerializeField] private EventReference glassShatterEvent;

    private bool scpInTestroom;
    private bool shatterEventReady;
    private bool glassEventTriggered;

    #region Unity Lifecycle

    private void Update() {
        // If all the pieces are in place then start checking for a players blink to make 173 break shit
        if (shatterEventReady && !glassEventTriggered && scpInTestroom && Player.isBlinking) ShatterGlass();
    }

    #endregion

    #region Private Methods

    private void ShatterGlass() {
        if (!shatterEventReady || glassEventTriggered || !scpInTestroom) return;

        glassEventTriggered = true;
        shatterEventReady = false;
        glassObject.SetActive(false);

        AudioManager.PlayOneShot(glassShatterEvent, glassObject.transform.position);
        EntitySystem.Instance.MoveEntity(EntitySystem.Instance.cached173Entity, postGlassShatter173Pos, true);

        // Long thing to disallow 173 from killing the player for a few microseconds so you don't instantly die upon glass shattering
        EntitySystem.Instance.cached173Entity.latestActiveInstance.GetComponent<SCP_173>().disableKilling = true;
        StartCoroutine(Delayed173Killing());
    }

    #endregion

    #region Private IEnumerators

    private IEnumerator Delayed173Killing() {
        yield return new WaitForSeconds(0.6f);
        EntitySystem.Instance.cached173Entity.latestActiveInstance.GetComponent<SCP_173>().disableKilling = false;
    }

    #endregion

    #region Public Methods

    // Force 173 into the testroom if the glass event hasn't happened yet (Regardless of chasing or not)
    // Call this via the trigger in the dead janitor room
    public void Bring173ToTestroom(Transform placeToBringHim) {
        // If the glass event already happened or SCP-173 is visible before entering the chamber do NOT teleport him in. Cancels the whole event
        if (glassEventTriggered || EntitySystem.Instance.cached173Entity.latestActiveInstance.GetComponent<SCP_173>().isVisibleByPlayer) return;
        EntitySystem.Instance.MoveEntity(EntitySystem.Instance.cached173Entity, placeToBringHim, true);

        // If we made it this far then confirm that 173 is now in the tesroom
        // If we never make it here on the first attempt then the event will never happen again. This is intentional
        // Why? Because 173 is already outside your door (Or very close nearby with malicious intent)
        scpInTestroom = true;
    }

    // Similar to Bring173ToTestroom() but for the actual observation area
    public void SetEventReadyState(bool ready) {
        if (glassEventTriggered || !scpInTestroom) return;
        shatterEventReady = ready;
    }

    #endregion
}