using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CanvasInstance : MonoBehaviour {
    public static CanvasInstance instance;

    public GameObject heldItemDisplay;
    public GameObject heldDocumentDisplay;
    public Slider blinkBar;
    public Image blinkBarBackground;
    public Image blinkBarFill;
    public GameObject blinkOverlay;
    public GameObject deathMenu;
    public TextMeshProUGUI deathMenuDeathCauseText;

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
    }
}