using UnityEngine;

public class ScreenshotUtility : MonoBehaviour
{
    private string screenshots;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            screenshots = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            ScreenCapture.CaptureScreenshot(screenshots + ".png");
            Debug.Log("Screenshot taken: " + screenshots + ".png");
        }
    }
}