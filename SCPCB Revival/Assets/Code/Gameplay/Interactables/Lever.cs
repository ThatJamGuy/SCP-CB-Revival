using UnityEngine;
using UnityEngine.Events;

namespace scpcbr {
    public class Lever : MonoBehaviour {
        [Header("Lever Handle Settings")]
        [SerializeField] private GameObject leverHandle;
        [SerializeField] private float minRotation = -50f;
        [SerializeField] private float maxRotation = 50f;
        [SerializeField] private float rotationSpeed = 100f;

        [Header("Lever State Settings")]
        [SerializeField] private float leverOffAngle = -50f;
        [SerializeField] private float leverOnAngle = 50f;
        [SerializeField] private float leverOnOffThreshold = 5f;

        [Header("Lever Audio Settings")]
        [SerializeField] private AudioClip leverFlipSound;
        [SerializeField] private AudioClip leverFlippedSound;

        [Header("Lever Events")]
        public UnityEvent OnLeverTurnedOn;
        public UnityEvent OnLeverTurnedOff;

        [Space]

        public bool turnedOn = false;

        private bool isBeingUsed = false;
        private bool lastState = false;
        private Quaternion initialRotation;

        private AudioSource audioSource;

        private void Start() {
            initialRotation = leverHandle.transform.localRotation;
            audioSource = GetComponent<AudioSource>();
        }

        private void Update() {
            if (isBeingUsed) {
                HandleLeverMovement();
            }

            UpdateLeverState();
        }

        public void UseLever(bool use) {
            isBeingUsed = use;

            if (isBeingUsed && !audioSource.isPlaying) {
                audioSource.clip = leverFlipSound;
                audioSource.Play();
            }
        }

        private void HandleLeverMovement() {
            float mouseMovement = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

            float currentXRotation = leverHandle.transform.localEulerAngles.x;
            if (currentXRotation > 180f) currentXRotation -= 360f;

            float newAngle = Mathf.Clamp(currentXRotation + mouseMovement, minRotation, maxRotation);
            leverHandle.transform.localEulerAngles = new Vector3(newAngle, 0f, 0f);
        }

        private void UpdateLeverState() {
            float currentXRotation = leverHandle.transform.localEulerAngles.x;
            if (currentXRotation > 180f) currentXRotation -= 360f;

            if (Mathf.Abs(currentXRotation - leverOnAngle) <= leverOnOffThreshold) {
                turnedOn = true;

                if (lastState != turnedOn) {
                    audioSource.clip = leverFlippedSound;
                    audioSource.Play();

                    OnLeverTurnedOn.Invoke();
                }
            }
            else if (Mathf.Abs(currentXRotation - leverOffAngle) <= leverOnOffThreshold) {
                turnedOn = false;

                if (lastState != turnedOn) {
                    audioSource.clip = leverFlippedSound;
                    audioSource.Play();

                    OnLeverTurnedOff.Invoke();
                }
            }

            lastState = turnedOn;
        }

        public void ResetLever() {
            leverHandle.transform.localRotation = initialRotation;
            turnedOn = false;
        }
    }
}