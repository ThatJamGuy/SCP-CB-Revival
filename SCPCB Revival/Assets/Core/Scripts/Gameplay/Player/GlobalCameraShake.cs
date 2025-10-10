using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace scpcbr {
    public class GlobalCameraShake : MonoBehaviour {
        public static GlobalCameraShake Instance;

        private List<Transform> camTransforms = new List<Transform>();
        private Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();
        private float shakeIntensity = 0f;
        private Coroutine shakeCoroutine;

        #region Default Methods
        private void Awake() {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }
        #endregion

        #region Public Methods
        public void RegisterCamera(Transform cam) {
            if (!camTransforms.Contains(cam)) {
                camTransforms.Add(cam);
                originalPositions[cam] = cam.localPosition;
            }
        }

        public void UnregisterCamera(Transform cam) {
            if (camTransforms.Contains(cam)) {
                camTransforms.Remove(cam);
                originalPositions.Remove(cam);
            }
        }

        public void ShakeCamera(float startIntensity, float endIntensity, float duration) {
            Debug.Log("Shake triggered at " + Time.time);
            if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
            shakeCoroutine = StartCoroutine(ShakeRoutine(startIntensity, endIntensity, duration));
        }
        #endregion

        #region Private Methods
        private void ApplyShake() {
            foreach (var cam in camTransforms) {
                if (cam == null || !originalPositions.ContainsKey(cam)) continue;
                cam.localPosition = originalPositions[cam] + (Vector3)Random.insideUnitCircle * shakeIntensity;
            }
        }
        #endregion

        #region Coroutines
        private IEnumerator ShakeRoutine(float startIntensity, float endIntensity, float duration) {
            foreach (var cam in camTransforms) {
                if (cam != null)
                    originalPositions[cam] = cam.localPosition;
            }
            float elapsed = 0f;

            while (elapsed < duration) {
                shakeIntensity = Mathf.Lerp(startIntensity, endIntensity, elapsed / duration);
                ApplyShake();
                elapsed += Time.deltaTime;
                yield return null;
            }

            StartCoroutine(FadeOutShake());
        }

        private IEnumerator FadeOutShake() {
            float fadeDuration = 0.5f;
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
                if (cam != null && originalPositions.ContainsKey(cam))
                    cam.localPosition = originalPositions[cam];
            }
        }
        #endregion
    }
}