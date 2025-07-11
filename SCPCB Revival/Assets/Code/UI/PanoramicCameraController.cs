using UnityEngine;

namespace scpcbr {
    public class PanoramicCameraController : MonoBehaviour {
        public float rotationSpeed = 5f, maxHorizontalAngle = 30f, maxVerticalAngle = 15f, returnSpeed = 2f;
        public float mouseSensitivity = 1f, smoothTime = 0.3f;
        public bool invertX, invertY;

        Vector2 velocity, mouseInput, smoothInput;
        Vector3 initialRotation;

        void Start() {
            initialRotation = transform.eulerAngles;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        void Update() {
            mouseInput.Set(
                Input.GetAxis("Mouse X") * mouseSensitivity * (invertX ? -1 : 1),
                Input.GetAxis("Mouse Y") * mouseSensitivity * (invertY ? -1 : 1)
            );

            smoothInput = Vector2.SmoothDamp(smoothInput, mouseInput, ref velocity, smoothTime);

            float x = Mathf.Clamp(initialRotation.x - smoothInput.y, initialRotation.x - maxVerticalAngle, initialRotation.x + maxVerticalAngle);
            float y = Mathf.Clamp(initialRotation.y + smoothInput.x, initialRotation.y - maxHorizontalAngle, initialRotation.y + maxHorizontalAngle);

            transform.eulerAngles = new Vector3(x, y, initialRotation.z);

            if (mouseInput.sqrMagnitude < 0.0001f)
                smoothInput = Vector2.Lerp(smoothInput, Vector2.zero, Time.deltaTime * returnSpeed);
        }
    }
}