using System.Collections;
using UnityEngine;

public class GlobalCameraShake : MonoBehaviour
{
    public static GlobalCameraShake Instance;
    private Transform camTransform;
    private Vector3 originalPosition;
    private float shakeIntensity = 0f;
    private Coroutine shakeCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ShakeCamera(float startIntensity, float endIntensity, float duration)
    {
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(ShakeRoutine(startIntensity, endIntensity, duration));
    }

    private IEnumerator ShakeRoutine(float startIntensity, float endIntensity, float duration)
    {
        camTransform = Camera.main.transform;
        originalPosition = camTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            shakeIntensity = Mathf.Lerp(startIntensity, endIntensity, elapsed / duration);
            ApplyShake();
            elapsed += Time.deltaTime;
            yield return null;
        }

        StartCoroutine(FadeOutShake());
    }

    private IEnumerator FadeOutShake()
    {
        float fadeDuration = 0.5f;
        float startShake = shakeIntensity;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            shakeIntensity = Mathf.Lerp(startShake, 0f, elapsed / fadeDuration);
            ApplyShake();
            elapsed += Time.deltaTime;
            yield return null;
        }
        shakeIntensity = 0f;
        camTransform.localPosition = originalPosition;
    }

    private void ApplyShake()
    {
        if (camTransform == null) return;
        camTransform.localPosition = originalPosition + (Vector3)Random.insideUnitCircle * shakeIntensity;
    }
}