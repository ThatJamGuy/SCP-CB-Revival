using FMODUnity;
using System.Collections;
using UnityEngine;

public class EVNT_Testroom : MonoBehaviour {
    [SerializeField] private Transform postGlassShatter173Pos;
    [SerializeField] private Transform glassShatterOrigin;
    [SerializeField] private EventReference glassShatterEvent;
    [SerializeField] private GameObject glassObject;

    private bool shatterEventReady = false;
    private bool glassEventTriggered;

    private void OnEnable() {
        PlayerBlink.OnPlayerBlink += ShatterGlass;
    }

    private void OnDisable() {
        PlayerBlink.OnPlayerBlink -= ShatterGlass;
    }

    public void Bring173ToTestroom(Transform placeToBringHim) {
        if (glassEventTriggered) return;

        EntitySystem.instance.MoveEntity(EntitySystem.EntityType.SCP173, placeToBringHim);

        Debug.Log("Brought SCP-173 to the testroom for an event.");
    }

    public void SetEventReadyState(bool ready) {
        shatterEventReady = ready;

        Debug.Log("Is the event ready to be triggered:" + ready);
    }

    private void ShatterGlass() {
        if (!shatterEventReady || glassEventTriggered) return;

        glassEventTriggered = true;
        shatterEventReady = false;
        glassObject.SetActive(false);
        AudioManager.instance.PlaySound(glassShatterEvent, glassShatterOrigin.position);
        EntitySystem.instance.MoveEntity(EntitySystem.EntityType.SCP173, postGlassShatter173Pos);

        Debug.Log("Shatter! SCP-173 should now be in the room with the player.");
    }
}