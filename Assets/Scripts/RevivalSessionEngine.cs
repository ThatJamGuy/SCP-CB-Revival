using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class RevivalSessionEngine : MonoBehaviour {
    public static RevivalSessionEngine Instance { get; private set; }

    [Header("Menus")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject invMenu;

    [Header("Heads Up Display")]
    [SerializeField] private TextMeshProUGUI infoText;

    private Tween infoTextTween;

    // now hardcoding the menus for more control
    private InputAction pauseKey;
    private InputAction invKey;
    private InputAction consoleKey;

    private bool anyMenuOpen;
    private int openMenuId;

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        pauseKey = InputManager.Instance.GetAction("Player", "Escape");
        invKey = InputManager.Instance.GetAction("Player", "Inventory");
        consoleKey = InputManager.Instance.GetAction("Player", "Console");
    }

    private void Update() {
        // check for currently open menu and close it if applicable; otherwise open the pause menu
        if (pauseKey.WasPressedThisFrame()) {
            ToggleMenu(0, true);
        }
    }

    #region Public Methods

    public void ToggleMenu(int menu, bool forceState) {
        switch (menu) {
            case 0:
                pauseMenu.SetActive(!pauseMenu.activeSelf);
                anyMenuOpen = !anyMenuOpen;

                if (GameManager.Instance.currentDifficulty != 2) GameManager.PauseGame();

                break;
            case 1:
                break;
            case 2:
                break;
        }
    }

    /// <summary>
    /// Displays a heads-up bit of text to the player similar to how Containment Breach did it.
    /// </summary>
    /// <param name="textToDisplay">The text that will appear on screen</param>
    /// <param name="displayDuration">How long the text will be displayed before fading begins</param>
    /// <param name="fadeDuration">How long it takes from the text to fade from full visibility to nothing</param>
    public void NotifyPlayer(string textToDisplay, float displayDuration = 3f, float fadeDuration = 2f) {
        if (!infoText) return; // Don't do anything if the infoText object is missing
        if (infoTextTween.isAlive) infoTextTween.Stop(); // Reset the tween if one is already active

        var startColor = infoText.color; // Create and set startColor to the info text color
        var endColor = startColor; // Create and set endColor to the startColor value
        startColor.a = 1f; // Set the start colors alpha value to 1 (Fully visible)
        endColor.a = 0f; // Set the end colors alpha value to 0 (Not visible)

        infoText.text = textToDisplay;
        infoText.color = startColor;

        infoTextTween = Tween.Delay(displayDuration, () => {
            infoTextTween = Tween.Color(infoText, endColor, fadeDuration);
        });
    }

    #endregion
}