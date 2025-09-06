using UnityEngine;
using System.Collections;

namespace scpcbr {
    public class EVNT_IntroShakes : MonoBehaviour {
        private GlobalCameraShake cameraShake;

        public AudioSource breachAnnouncement;
        public AudioSource postBreachAlert;

        public void TriggerIntroShakes() {
            cameraShake = GlobalCameraShake.Instance;
            StartCoroutine(IntroShakes());
            StartCoroutine(PostBreachAlert());
        }

        private IEnumerator IntroShakes() {
            breachAnnouncement.Play();
            yield return Wait(12f);
            cameraShake.ShakeCamera(.3f, 0, 5);
            yield return Wait(38f);
            cameraShake.ShakeCamera(.3f, 0, 6);
        }

        private IEnumerator PostBreachAlert() {
            yield return Wait(54f);
            postBreachAlert.Play();
        }

        // --- Utility ---
        IEnumerator Wait(float t) { yield return new WaitForSeconds(t); }
    }
}