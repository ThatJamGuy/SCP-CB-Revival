using UnityEngine;
using System.Collections;
using PrimeTween;
using UnityEngine.Rendering;

public class RandomRuntimeIntensity : MonoBehaviour {
    public enum FlickerType { Light, LensFlair }

    public FlickerType flickerType = FlickerType.Light;

    [SerializeField] private float minIntensity = 0.5f;
    [SerializeField] private float maxIntensity = 2.0f;
    [SerializeField] private float changeInterval = 0.01f;

    private float currentIntensity;
    private Light lightComp;
    private LensFlareComponentSRP lensFlareComp;
    private Tween flickerTween;

    private void Awake() {
        lightComp = GetComponent<Light>();
        lensFlareComp = GetComponent<LensFlareComponentSRP>();
        currentIntensity = Random.Range(minIntensity, maxIntensity);
    }

    private void OnEnable() {
        StartCoroutine(FlickerRoutine());
    }

    private void OnDisable() {
        flickerTween.Stop();
    }

    private void Update() {
        switch (flickerType) {
            case FlickerType.Light:
                Light lightComponent = GetComponent<Light>();
                if (lightComponent != null) {
                    lightComponent.intensity = currentIntensity;
                }
                break;
            case FlickerType.LensFlair:
                LensFlareComponentSRP lensFlareComponent = GetComponent<LensFlareComponentSRP>();
                if (lensFlareComponent != null) {
                    lensFlareComponent.intensity = currentIntensity;
                }
                break;
            default:
                break;
        }

    }

    private IEnumerator FlickerRoutine() {
        while (true) {
            float targetIntensity = Random.Range(minIntensity, maxIntensity);
            flickerTween = Tween.Custom(currentIntensity, targetIntensity, changeInterval, value => { currentIntensity = value; ApplyIntensity(); });
            yield return new WaitForSeconds(changeInterval);
        }
    }

    private void ApplyIntensity() {
        if (flickerType == FlickerType.Light && lightComp != null)
            lightComp.intensity = currentIntensity;

        if (flickerType == FlickerType.LensFlair && lensFlareComp != null)
            lensFlareComp.intensity = currentIntensity;
    }
}