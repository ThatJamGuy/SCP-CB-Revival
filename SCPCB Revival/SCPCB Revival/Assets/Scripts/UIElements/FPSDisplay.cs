using TMPro;
using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI fpsText;

    private bool fpsCounterEnabled = false;

    private float pollingTime = 1f;
    private float time;
    private int frameCount;

    private void Start()
    {
        fpsCounterEnabled = PlayerSettings.Instance.enableFPSCounter;
        fpsText.enabled = fpsCounterEnabled;
    }

    private void Update()
    {
        time += Time.deltaTime;

        frameCount++;

        if (time >= pollingTime)
        {
            int frameRate = Mathf.RoundToInt(frameCount / time);
            fpsText.text = frameRate.ToString() + " FPS";

            time -= pollingTime;
            frameCount = 0;
        }
    }
}