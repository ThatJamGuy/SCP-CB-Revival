using UnityEngine;
using UnityEngine.Events;

public class EVNT_Trigger : MonoBehaviour
{
    public UnityEvent OnTriggerEnterEvent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (OnTriggerEnterEvent != null)
            {
                OnTriggerEnterEvent.Invoke();
            }
        }
    }
}