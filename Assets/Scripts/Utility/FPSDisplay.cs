using UnityEngine;
using TMPro;

/// <summary>
/// Displays the average framerate the game is running at. Also recieves realtime calls from the OptionsMenu to enable/disable itself mid-game.
/// </summary>
public class FPSDisplay : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI fpsText;
    [SerializeField] private float pollingTime = 1f;

    private float time;
    private int frameCount;

    #region Unity Callbacks
    private void OnEnable() {
        OptionsMenu.OnSettingsChanged += HandleFpsToggle;
        HandleFpsToggle(SaveSystem.Load<SettingsData>("settings.json").fpsCounter);
    }

    private void OnDisable() {
        OptionsMenu.OnSettingsChanged -= HandleFpsToggle;
    }

    private void Update() {
        // Give up if the FPS Text isn't active
        if (!fpsText.gameObject.activeSelf) return;

        // Increase the framecount
        time += Time.deltaTime;
        frameCount++;

        // Display the FPS if the time reaches the polling time or above it
        if (time >= pollingTime) {
            fpsText.text = $"{Mathf.RoundToInt(frameCount / time)} FPS";
            time -= pollingTime;
            frameCount = 0;
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Shows or hides the FPS display based on the specified value.
    /// </summary>
    /// <param name="enabled">true to show the FPS display; false to hide it.</param>
    private void HandleFpsToggle(bool enabled) => fpsText.gameObject.SetActive(enabled);
    #endregion
}