using UnityEngine;
using UnityEngine.InputSystem;

// Giving the main menu some spiiice with the sexy panoramic camera movement
// Recommended to keep the variables as the default as I already tuned them pretty well, but fell free to experiment if necessary
namespace scpcbr {
    public class PanoramicCameraController : MonoBehaviour {
        public float rotationSpeed = 1f, maxHorizontalAngle = 10f, maxVerticalAngle = 5f, returnSpeed = 1f, mouseSensitivity = 5f, smoothTime = 0.3f;

        private Vector2 velocity, mouseInput, smoothInput;
        private Vector3 initialRotation;

        private void Start() {
            initialRotation = transform.eulerAngles;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update() {
            mouseInput = Mouse.current.delta.ReadValue() * mouseSensitivity * rotationSpeed * Time.deltaTime;
            smoothInput = Vector2.SmoothDamp(smoothInput, mouseInput, ref velocity, smoothTime);

            float x = Mathf.Clamp(initialRotation.x - smoothInput.y, initialRotation.x - maxVerticalAngle, initialRotation.x + maxVerticalAngle);
            float y = Mathf.Clamp(initialRotation.y + smoothInput.x, initialRotation.y - maxHorizontalAngle, initialRotation.y + maxHorizontalAngle);

            transform.eulerAngles = new Vector3(x, y, initialRotation.z);

            if (mouseInput.sqrMagnitude < 0.0001f)
                smoothInput = Vector2.Lerp(smoothInput, Vector2.zero, Time.deltaTime * returnSpeed);
        }
    }
}