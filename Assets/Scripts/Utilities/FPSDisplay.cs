using TMPro;
using UnityEngine;

/// <summary>
/// Displays the average framerate the game is running at
/// </summary>
public class FPSDisplay : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI fpsDisplayText;
    [SerializeField] private float pollingTime = 1f;

    [Header("FPS Levels")]
    [SerializeField] private Color fpsGoodColor = Color.green;
    [SerializeField] private int fpsIffyMaxValue = 100;
    [SerializeField] private Color fpsIffyColor = Color.orange;
    [SerializeField] private int fpsBadMaxValue = 59;
    [SerializeField] private Color fpsBadColor = Color.red;

    private float time;
    private int frameCount;

    #region Unity Callbacks

    private void Update() {
        // Increase the frameCount variable and increase time by deltaTime
        time += Time.deltaTime;
        frameCount++;

        // If the time variable is not greater or equal to the polling time do nothing
        if (!(time >= pollingTime)) return;

        // Set fps display color based on defined values
        if (frameCount >= fpsIffyMaxValue) fpsDisplayText.color = fpsGoodColor;
        if (frameCount < fpsIffyMaxValue && frameCount > fpsBadMaxValue) fpsDisplayText.color = fpsIffyColor;
        if (frameCount <= fpsBadMaxValue) fpsDisplayText.color = fpsBadColor;

        // Display the FPS in text and subtract the pollingTime from time as well as set frameCount to 0
        fpsDisplayText.text = $"{Mathf.RoundToInt(frameCount / time)} FPS";
        time -= pollingTime;
        frameCount = 0;
    }

    #endregion
}