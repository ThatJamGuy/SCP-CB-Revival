using UnityEngine;

public class EVNT_PostBreach : MonoBehaviour {
    //private void OnEnable() {
    //    DevConsole.singleton.AddCommand(new ActionCommand(TriggerPostBreachEvent) { className = "Event" });
    //}

    public void TriggerPostBreachEvent() {
        Debug.Log("Post-breach event triggered.");
        AudioManager.instance.PlaySound(FMODEvents.instance.alarm2, transform.position);
    }
}