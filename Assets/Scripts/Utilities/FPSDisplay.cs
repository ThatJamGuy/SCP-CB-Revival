using UnityEngine;
using TMPro;

/// <summary>
/// Displays the average framerate the game is running at
/// </summary>
public class FPSDisplay : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI fpsDisplayText;
    [SerializeField] private float pollingTime = 1f;

    private float time;
    private int frameCount;

    #region Unity Callbacks
    private void Update() {
        // Increase the frameCount variable and increase time by deltaTime
        time += Time.deltaTime;
        frameCount++;

        // If the time variable is not greater or equal to the polling time do nothing
        if (!(time >= pollingTime)) return;
        
        // Display the FPS in text and subtract the pollingTime from time as well as set frameCount to 0
        fpsDisplayText.text = $"{Mathf.RoundToInt(frameCount / time)} FPS";
        time -= pollingTime;
        frameCount = 0;
    }
    #endregion
}