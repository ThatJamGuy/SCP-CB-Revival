using UnityEngine;

public class GlobalSettingsManager : MonoBehaviour {
    public static GlobalSettingsManager instance;

    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }
}