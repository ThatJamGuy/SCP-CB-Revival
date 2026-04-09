using UnityEngine;

public class TrackThisEmitter : MonoBehaviour {
    private void Start() {
        var emitter = GetComponent<FMODUnity.StudioEventEmitter>();
        if (emitter != null) {
            AudioManager.instance.TrackEmitter(emitter);
        }

    }
}