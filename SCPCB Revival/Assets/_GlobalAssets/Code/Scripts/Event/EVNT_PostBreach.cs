using UnityEngine;
using SickDev.CommandSystem;
using FMODUnity;

public class EVNT_PostBreach : MonoBehaviour {

    [SerializeField] private EventReference postBreachSound;

    private void OnEnable() {
        DevConsole.singleton.AddCommand(new ActionCommand(TriggerPostBreachEvent) { className = "Event" });
    }
    
    public void TriggerPostBreachEvent() {
        Debug.Log("Post-breach event triggered.");
        AudioManager.instance.PlaySound(postBreachSound, transform.position);
    }
}