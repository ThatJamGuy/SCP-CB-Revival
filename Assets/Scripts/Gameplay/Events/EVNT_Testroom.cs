using FMODUnity;
using UnityEngine;

public class EVNT_Testroom : MonoBehaviour {
    [SerializeField] private Transform postGlassShatter173Pos;
    [SerializeField] private Transform glassShatterOrigin;
    [SerializeField] private EventReference glassShatterEvent;
    [SerializeField] private GameObject glassObject;

    private bool shatterEventReady = false;
    private bool glassEventTriggered;

    /*private void OnEnable() {
        PlayerBlink.OnPlayerBlink += ShatterGlass;
    }

    private void OnDisable() {
        PlayerBlink.OnPlayerBlink -= ShatterGlass;
    }*/

    public void Bring173ToTestroom(Transform placeToBringHim) {
        if (glassEventTriggered) return;

        //EntitySystem.instance.MoveEntity(EntitySystem.EntityType.SCP173, placeToBringHim);
    }

    public void SetEventReadyState(bool ready) {
        shatterEventReady = ready;
    }

    private void ShatterGlass() {
        if (!shatterEventReady || glassEventTriggered) return;

        glassEventTriggered = true;
        shatterEventReady = false;
        glassObject.SetActive(false);
        //AudioManager.instance.PlaySound(glassShatterEvent, glassShatterOrigin.position);
        //EntitySystem.instance.MoveEntity(EntitySystem.EntityType.SCP173, postGlassShatter173Pos);
    }
}