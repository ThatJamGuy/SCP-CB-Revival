using PrimeTween;
using TMPro;
using UnityEngine;

/// <summary>
/// Global script to handle the little messages that show up in the middle of the screen. Will probably later expand
/// </summary>
public class InfoTextManager : MonoBehaviour {
    public static InfoTextManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI infoText;

    private Tween infoTextTween;

    #region Unity Callbacks
    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    #endregion

    #region Public Methods
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
        
        infoText.text = textToDisplay; // Set the info text to the parameter defined text
        infoText.color = startColor; // Set the color to the start color
        
        infoTextTween = Tween.Delay(displayDuration, () => {
            infoTextTween = Tween.Color(infoText, endColor, fadeDuration);
        });
    }
    #endregion
}