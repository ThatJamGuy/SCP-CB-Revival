using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Script to handle some of that fun dynamic camera movement in the main menu.
/// I recommend keeping the values as I already have them, as I personally find them best suited for subtle but noticable
/// </summary>
public class PanoramicCameraController : MonoBehaviour {
    [SerializeField] private float rotationSpeed = 1f, maxHorizontalAngle = 10f, maxVerticalAngle = 5f, returnSpeed = 1f, mouseSensitivity = 5f, smoothTime = 0.3f;

    private Vector2 velocity, mouseInput, smoothInput;
    private Vector3 initialRotation;
    private Vector3 defaultPosition;

    #region Unity Callbacks

    private void Start() {
        initialRotation = transform.eulerAngles;
        defaultPosition = transform.localPosition;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update() {
        // Determine finalized mouse input
        mouseInput = Mouse.current.delta.ReadValue() * mouseSensitivity * rotationSpeed * Time.deltaTime;
        smoothInput = Vector2.SmoothDamp(smoothInput, mouseInput, ref velocity, smoothTime);

        // Clamp the camera rotation
        float x = Mathf.Clamp(initialRotation.x - smoothInput.y, initialRotation.x - maxVerticalAngle, initialRotation.x + maxVerticalAngle);
        float y = Mathf.Clamp(initialRotation.y + smoothInput.x, initialRotation.y - maxHorizontalAngle, initialRotation.y + maxHorizontalAngle);

        // Set the rotation of the camera
        transform.eulerAngles = new Vector3(x, y, initialRotation.z);

        // Return to default position if the mouse stops moving
        if (mouseInput.sqrMagnitude < 0.0001f) smoothInput = Vector2.Lerp(smoothInput, Vector2.zero, Time.deltaTime * returnSpeed);

        // New method for implementing camera shake to cameras is now in here too
        // Define a Vector3 for the shake offest, constantly updates itself via RevivalRuntimeEngine
        Vector3 shake = RevivalRuntimeEngine.ShakeOffset;

        // Apply shake offset on all the axes
        transform.localPosition = new Vector3(
            defaultPosition.x + shake.x,
            defaultPosition.y + shake.y,
            defaultPosition.z + shake.z
        );
    }

    #endregion
}