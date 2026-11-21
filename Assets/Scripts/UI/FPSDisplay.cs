using System.Collections;
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

    private Coroutine avgFrameCoroutine;

    private float time;
    private int frameCount;

    #region Unity Lifecycle

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
        fpsDisplayText.SetText("{0} FPS", Mathf.RoundToInt(frameCount / time));
        time -= pollingTime;
        frameCount = 0;
    }

    #endregion

    #region Private Coroutines

    private IEnumerator AvgFrameTestRoutine(float testDuration) {
        int startFrame = Time.frameCount;
        float startTime = Time.realtimeSinceStartup;

        Debug.Log($"<color=#00ffff>The frametest has begun...</color>");

        yield return new WaitForSecondsRealtime(testDuration);

        int frames = Time.frameCount - startFrame;
        float elapsed = Time.realtimeSinceStartup - startTime;
        float avgFps = frames / elapsed;
        float avgMs = elapsed * 1000f / frames;

        Debug.Log($"<color=#00ffff>Average FPS over {elapsed:F2}s: {avgFps:F1} ({avgMs:F2} ms/frame, {frames} frames)</color>");

        avgFrameCoroutine = null;
    }

    #endregion

    #region Public Methods

    public void TestAverageFramerate(float testDuration = 0) {
        if (testDuration <= 0) testDuration = 5;
        if (avgFrameCoroutine != null) StopCoroutine(avgFrameCoroutine);

        avgFrameCoroutine = StartCoroutine(AvgFrameTestRoutine(testDuration));
    }

    #endregion
}