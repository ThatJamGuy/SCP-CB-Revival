using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Globally accessable class that scripts can use to grab stuff on the canvas without direct references
/// This allows the Player to still function without the canvas present and vice versa
/// </summary>
public class CanvasInstance : MonoBehaviour {
    public static CanvasInstance Instance { get; private set; }

    [Header("Single Objects")]
    public Image interactIcon;
    public RectTransform canvasRectTransform;
    public Animator HUD_QuickSave;
    public Animator HUD_AchievementPopup;
    public TextMeshProUGUI achievementName;
    public TextMeshProUGUI achievementDesc;
    public Image achievementIcon;
    public GameObject deathMenu;
    public TextMeshProUGUI deathMenuDeathCauseText;
    
    [Header("Pause Menu")]
    public GameObject controllerTooltips;
    public Button resumeButton;

    [Header("Canvas References")] 
    public Canvas screensCanvas;

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}