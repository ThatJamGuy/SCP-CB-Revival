using System.Collections;
using TMPro;
using UnityEngine;

namespace scpcbr {
    public class InfoTextManager : MonoBehaviour {
        public static InfoTextManager Instance { get; private set; }

        [SerializeField] private TextMeshProUGUI infoText;
        [SerializeField] private float displayDuration = 2f;
        [SerializeField] private float fadeDuration = 2f;

        private Coroutine fadeCoroutine;

        private void Awake() {
            if (Instance == null) {
                Instance = this;
            }
            else {
                Destroy(gameObject);
            }
        }

        public void NotifyPlayer(string notifText) {
            if (infoText == null) return;

            if (fadeCoroutine != null) {
                StopCoroutine(fadeCoroutine);
            }

            infoText.text = notifText;
            infoText.alpha = 1f;
            fadeCoroutine = StartCoroutine(FadeOutText());
        }

        private IEnumerator FadeOutText() {
            yield return new WaitForSeconds(displayDuration);

            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration) {
                elapsedTime += Time.deltaTime;
                infoText.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
                yield return null;
            }

            infoText.alpha = 0f;
        }
    }
}