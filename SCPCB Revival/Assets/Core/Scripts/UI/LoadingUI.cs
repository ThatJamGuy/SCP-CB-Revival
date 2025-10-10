using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingUI : MonoBehaviour {
    [SerializeField] Slider progressBar;
    [SerializeField] TextMeshProUGUI statusText;
    [SerializeField] float smoothSpeed = 2.5f;
    [SerializeField] TextMeshProUGUI continuePrompt;

    float displayed;
    float target;

    void Start() {
        if (progressBar) { progressBar.minValue = 0; progressBar.maxValue = 1; progressBar.value = 0; }
        if (continuePrompt) continuePrompt.gameObject.SetActive(false);
    }

    void Update() {
        displayed = Mathf.MoveTowards(displayed, target, Time.unscaledDeltaTime * smoothSpeed);
        if (progressBar) progressBar.value = displayed;
        if (statusText) statusText.text = $"Loading - {Mathf.RoundToInt(displayed * 100)} %";
    }

    public void SetTargetProgress(float v) => target = v;
    public void SetImmediate(float v) { target = displayed = v; if (progressBar) progressBar.value = v; if (statusText) statusText.text = $"Loading - {Mathf.RoundToInt(v * 100)} %"; }

    public void ShowContinuePrompt(bool show) {
        if (continuePrompt) continuePrompt.gameObject.SetActive(show);
    }
}