using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Globally accessable class that scripts can use to grab stuff on the canvas without direct references
/// This allows the Player to still function without the canvas present and vice versa
/// </summary>
public class CanvasInstance : MonoBehaviour {
    public static CanvasInstance Instance;

    // The different things that need to be accessed from external scripts
    [Header("Single Objects")]
    public Image interactIcon;
    public RectTransform canvasRectTransform;
    public GameObject heldItemDisplay;
    public GameObject heldDocumentDisplay;
    public Slider blinkBar;
    public Image blinkBarBackground;
    public Image blinkBarFill;
    public GameObject blinkOverlay;
    public GameObject deathMenu;
    public TextMeshProUGUI deathMenuDeathCauseText;
    public GameObject introScreenStuff;
    
    [Header("Pause Menu")]
    public GameObject controllerTooltips;
    public Button resumeButton;

    private void Awake() {
        // Ensure that only of one these exist to prevent issues
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}