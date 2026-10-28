using FMODUnity;
using System.Collections;
using UnityEngine;

public class EVNT_Testroom : MonoBehaviour {
    [Header("References")]
    [SerializeField] private Transform postGlassShatter173Pos;
    [SerializeField] private GameObject glassObject;
    [SerializeField] private EventReference glassShatterEvent;

    private bool shatterEventReady;
    private bool glassEventTriggered;

    private void Update() {
        if (shatterEventReady && !glassEventTriggered && Player.isBlinking) {
            ShatterGlass();
        }
    }

    #region Private Methods

    private void ShatterGlass() {
        if (!shatterEventReady || glassEventTriggered) return;

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
        yield return new WaitForSeconds(0.5f);
        EntitySystem.Instance.cached173Entity.latestActiveInstance.GetComponent<SCP_173>().disableKilling = false;
    }

    #endregion

    #region Public Methods

    public void Bring173ToTestroom(Transform placeToBringHim) {
        if (glassEventTriggered) return;
        EntitySystem.Instance.MoveEntity(EntitySystem.Instance.cached173Entity, placeToBringHim);
    }

    public void SetEventReadyState(bool ready) {
        if (glassEventTriggered) return;
        shatterEventReady = ready;
    }

    #endregion
}