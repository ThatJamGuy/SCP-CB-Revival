using UnityEngine;
using FMODUnity;

public class AudioManager : MonoBehaviour {
    public static AudioManager instance { get; private set; }

    private void Awake() {
        if (instance != null) {
            Debug.LogError("Found more than one Audio Manager in the scene.");
        }
        instance = this;
    }

    public void PlaySound(EventReference sound, Vector3 worldPos) {
        RuntimeManager.PlayOneShot(sound, worldPos);
    }
}