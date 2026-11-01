using UnityEngine;
using UnityEngine.Events;

public class EVNT_TriggerSave : MonoBehaviour {
    [SerializeField] private bool ignoreChaseCheck;
    [SerializeField] private bool allowMultiUse;

    public UnityEvent OnTriggerEntered;
    public UnityEvent OnSaveTriggered;

    private bool usedOnce;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            // If the intro isn't completed or checkpoints can only be used once then return
            if (!SavegameEngine.Get<GameStateData>().introCompleted
                || (usedOnce && !allowMultiUse)) return;

            OnTriggerEntered.Invoke();

            // Prevent a save from happening if being chased to prevent issues
            // (And also to hide the fact that SCPs aren't saved yet)
            foreach (Entity entity in EntitySystem.Instance.entities) {
                if (entity.chasingPlayer && !ignoreChaseCheck) return;
            }

            usedOnce = true;
            SavegameEngine.SaveAll();
            OnSaveTriggered.Invoke();
        }
    }
}