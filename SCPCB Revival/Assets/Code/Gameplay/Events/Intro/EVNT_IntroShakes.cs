using UnityEngine;
using System.Collections;

namespace scpcbr {
    public class EVNT_IntroShakes : MonoBehaviour {
        private GlobalCameraShake cameraShake;

        public AudioSource breachAnnouncement;

        public void TriggerIntroShakes() {
            cameraShake = GlobalCameraShake.Instance;
            StartCoroutine(IntroShakes());
        }

        private IEnumerator IntroShakes() {
            breachAnnouncement.Play();
            yield return Wait(11.6f);
            cameraShake.ShakeCamera(.3f, 0, 5);
        }

        // --- Utility ---
        IEnumerator Wait(float t) { yield return new WaitForSeconds(t); }
    }
}