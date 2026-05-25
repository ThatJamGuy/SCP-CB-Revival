using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Script to handle generic trigger events through Unity's event system. Only for basic stuff, like enabling things
/// </summary>
public class EVNT_Trigger : MonoBehaviour {
    [SerializeField] private bool triggerOnce;
    [SerializeField] private bool useOnTriggerExit;
    [SerializeField] private bool allowNPCTrigger = false;
    
    public UnityEvent OnTriggerEnterEvent;
    public UnityEvent OnTriggerExitEvent;

    private bool hasBeenTriggered = false;

    #region Unity Callbacks
    private void OnTriggerEnter(Collider other) {
        // Is the tag that just collided with the trigger the Player? If so continue on.
        if (other.CompareTag("Player")) {
            // If triggerOnce is enabled and the trigger was already triggered don't do anything
            if (triggerOnce && hasBeenTriggered) return;

            // If the OnTrigger event is all set then invoke, and it set hasBeenTriggered to true
            if (OnTriggerEnterEvent != null) {
                OnTriggerEnterEvent.Invoke();
                hasBeenTriggered = true;
            }
        }
        // Are NPCs are allowed to trigger and is the tag that just collided with the trigger an NPC?
        if (allowNPCTrigger && other.CompareTag("NPC")) {
            // If triggerOnce is enabled and the trigger was already triggered don't do anything
            // If the OnTrigger event is all set then invoke, and it set hasBeenTriggered to true 
            if (triggerOnce && hasBeenTriggered) return;
            if (OnTriggerEnterEvent == null) return;
            
            OnTriggerEnterEvent.Invoke();
            hasBeenTriggered = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        // Is the tag that just left the trigger the Player?
        if (other.CompareTag("Player")) {
            // If so invoke the OnTriggerExit event (Not doing anything else as trigger once is redundant here)
            OnTriggerExitEvent.Invoke();
        }
    }
    #endregion
}