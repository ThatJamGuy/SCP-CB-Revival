using UnityEngine;
using FMODUnity;

public class AudioEventsHolder : MonoBehaviour {
    public static AudioEventsHolder Instance { get; private set; }

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}