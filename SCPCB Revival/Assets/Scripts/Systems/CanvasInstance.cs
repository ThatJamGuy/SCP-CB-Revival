using UnityEngine;

public class CanvasInstance : MonoBehaviour {
    public static CanvasInstance instance;

    public GameObject heldItemDisplay;
    public GameObject heldDocumentDisplay;

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
    }
}