using UnityEngine;
using TMPro;

public class FPSDisplay : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI fpsText;

    private float pollingTime = 1f;
    private float time;
    private int frameCount;

    private void OnEnable() {
        if (OptionsMenu.instance != null) {
            OptionsMenu.instance.OnSettingsChanged += HandleFpsToggle;
            HandleFpsToggle(PlayerPrefs.GetInt("opt_fps_counter", 0) == 1);
        }
    }

    private void OnDisable() {
        if (OptionsMenu.instance != null) {
            OptionsMenu.instance.OnSettingsChanged -= HandleFpsToggle;
        }
    }

    private void HandleFpsToggle(bool enabled) {
        fpsText.gameObject.SetActive(enabled);
    }

    private void Start() {
        if (PlayerPrefs.GetInt("opt_fps_counter", 0) == 0) {
            fpsText.gameObject.SetActive(false);
        }
    }

    private void Update() {
        time += Time.deltaTime;
        frameCount++;

        if (time >= pollingTime) {
            int frameRate = Mathf.RoundToInt(frameCount / time);
            fpsText.text = frameRate.ToString() + " FPS";

            time -= pollingTime;
            frameCount = 0;
        }
    }
}