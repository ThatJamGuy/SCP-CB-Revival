using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalCameraShake : MonoBehaviour {
    public static GlobalCameraShake Instance { get; private set; }

    [Header("Shake Feel")]
    [Tooltip("How fast the shake oscillates. Higher = frantic, lower = a slow sway.")]
    [SerializeField] private float noiseFrequency = 15f;
    [Tooltip("Max rotational shake in degrees, applied around the camera's local Z axis.")]
    [SerializeField] private float maxRotationDegrees = 3f;

    private readonly List<Transform> camTransforms = new List<Transform>();
    private readonly Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();
    private readonly Dictionary<Transform, Quaternion> originalRotations = new Dictionary<Transform, Quaternion>();

    private float shakeIntensity;
    private float noiseSeed;
    private Coroutine activeRoutine;

    #region Default Methods

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    #endregion

    #region Public Methods

    public void RegisterCamera(Transform cam) {
        if (camTransforms.Contains(cam)) return;
        camTransforms.Add(cam);
        originalPositions[cam] = cam.localPosition;
        originalRotations[cam] = cam.localRotation;
    }

    public void UnregisterCamera(Transform cam) {
        camTransforms.Remove(cam);
        originalPositions.Remove(cam);
        originalRotations.Remove(cam);
    }

    public void ShakeCamera(float startIntensity, float endIntensity, float duration) {
        if (activeRoutine != null) StopCoroutine(activeRoutine);

        foreach (var cam in camTransforms) {
            if (cam == null) continue;
            originalPositions[cam] = cam.localPosition;
            originalRotations[cam] = cam.localRotation;
        }

        noiseSeed = Random.value * 1000f;
        activeRoutine = StartCoroutine(ShakeRoutine(startIntensity, endIntensity, duration));
    }

    #endregion

    #region Private Methods

    private void ApplyShake() {
        float t = Time.time * noiseFrequency;
        float offsetX = (Mathf.PerlinNoise(noiseSeed, t) - 0.5f) * 2f;
        float offsetY = (Mathf.PerlinNoise(noiseSeed + 1f, t) - 0.5f) * 2f;
        float offsetZ = (Mathf.PerlinNoise(noiseSeed + 2f, t) - 0.5f) * 2f;

        foreach (var cam in camTransforms) {
            if (cam == null || !originalPositions.ContainsKey(cam)) continue;

            Vector3 posOffset = new Vector3(offsetX, offsetY, 0f) * shakeIntensity;
            cam.localPosition = originalPositions[cam] + posOffset;

            float rotOffset = offsetZ * shakeIntensity * maxRotationDegrees;
            cam.localRotation = originalRotations[cam] * Quaternion.Euler(0f, 0f, rotOffset);
        }
    }

    #endregion

    #region Private Coroutines

    private IEnumerator ShakeRoutine(float startIntensity, float endIntensity, float duration) {
        float elapsed = 0f;

        while (elapsed < duration) {
            shakeIntensity = Mathf.Lerp(startIntensity, endIntensity, elapsed / duration);
            ApplyShake();
            elapsed += Time.deltaTime;
            yield return null;
        }

        activeRoutine = StartCoroutine(FadeOutShake());
    }

    private IEnumerator FadeOutShake() {
        const float fadeDuration = 0.5f;
        float startShake = shakeIntensity;
        float elapsed = 0f;

        while (elapsed < fadeDuration) {
            shakeIntensity = Mathf.Lerp(startShake, 0f, elapsed / fadeDuration);
            ApplyShake();
            elapsed += Time.deltaTime;
            yield return null;
        }

        shakeIntensity = 0f;
        foreach (var cam in camTransforms) {
            if (cam == null) continue;
            if (originalPositions.ContainsKey(cam)) cam.localPosition = originalPositions[cam];
            if (originalRotations.ContainsKey(cam)) cam.localRotation = originalRotations[cam];
        }
        activeRoutine = null;
    }

    #endregion
}