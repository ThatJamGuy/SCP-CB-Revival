using PrimeTween;
using TMPro;
using UnityEngine;

public class InfoTextManager : MonoBehaviour {
    public static InfoTextManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float fadeDuration = 2f;

    private Tween fadeTween;

    private void Awake() {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    //private void OnEnable() {
    //    DevConsole.Instance.Add<string>("notify_player", str => NotifyPlayer(str));
    //}

    /// <summary>
    /// Displays a heads up bit of text to the player similar to how Containment Breach did it.
    /// </summary>
    /// <param name="textToDisplay"></param>
    public void NotifyPlayer(string textToDisplay) {
        if (!infoText) return;

        infoText.text = textToDisplay;
        Color startColor = infoText.color;
        startColor.a = 1f;
        infoText.color = startColor;
        Color endColor = startColor;
        endColor.a = 0f;
        fadeTween = Tween.Delay(displayDuration, () => {
            fadeTween = Tween.Color(infoText, endColor, fadeDuration);
        });
    }
}