using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Globally accessable class that scripts can use to grab stuff on the canvas without direct references
/// This allows the Player to still function without the canvas present and vice versa
/// </summary>
public class CanvasInstance : MonoBehaviour {
    public static CanvasInstance Instance { get; private set; }

    [Header("Single Objects")]
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

    [Header("HUD")]
    public GameObject revivalBarStyleParent;
    public GameObject legacyBarStyleParent;
    public Slider revivalBlinkSlider;
    public Slider revivalSprintSlider;
    public Slider legacyBlinkSlider;
    public Slider legacySprintSlider;
    public Sprite revivalInteractIcon;
    public Sprite legacyInteractIcon;

    public Image interactIcon;
    [HideInInspector] public Slider currBlinkSlider;
    [HideInInspector] public Slider currSprintSlider;

    [Header("Canvas References")]
    public Canvas screensCanvas;

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start() {
        // Set up which HUD Designs to use so other scripts don't have to think about it
        switch (SettingsManager.settingsData.hudDesign) {
            case 0:
                revivalBarStyleParent.SetActive(true);
                legacyBarStyleParent.SetActive(false);

                currBlinkSlider = revivalBlinkSlider;
                currSprintSlider = revivalSprintSlider;
                interactIcon.sprite = revivalInteractIcon;
                break;
            case 1:
                revivalBarStyleParent.SetActive(false);
                legacyBarStyleParent.SetActive(true);

                currBlinkSlider = legacyBlinkSlider;
                currSprintSlider = legacySprintSlider;
                interactIcon.sprite = legacyInteractIcon;
                break;
        }
    }
}