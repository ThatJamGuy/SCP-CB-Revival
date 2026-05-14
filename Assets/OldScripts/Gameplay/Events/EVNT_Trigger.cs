using UnityEngine;
using UnityEngine.Events;

public class EVNT_Trigger : MonoBehaviour {
    [SerializeField] private bool triggerOnce;
    [SerializeField] private bool allowNPCTrigger = false;
    public UnityEvent OnTriggerEnterEvent;

    private bool hasBeenTriggered = false;

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Player") {
            if (triggerOnce && hasBeenTriggered) return;

            if (OnTriggerEnterEvent != null) {
                OnTriggerEnterEvent.Invoke();
                hasBeenTriggered = true;
            }
        }
        if (allowNPCTrigger && other.tag == "NPC") {
            if (triggerOnce && hasBeenTriggered) return;
            if (OnTriggerEnterEvent != null) {
                OnTriggerEnterEvent.Invoke();
                hasBeenTriggered = true;
            }
        }
    }
}