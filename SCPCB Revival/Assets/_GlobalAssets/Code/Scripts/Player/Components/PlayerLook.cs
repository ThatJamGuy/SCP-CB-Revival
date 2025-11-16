using UnityEngine;

public class PlayerLook : MonoBehaviour {
    [Header("Look Settings")]
    [SerializeField] private float sensitivity = 100f;

    [Header("References")]
    [SerializeField] private Transform playerBody;

    private float xRotation;

    #region Default Methods
    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update() {
        if (!PlayerAccessor.instance.allowInput) return;

        Vector2 input = InputManager.Instance.Look * sensitivity * Time.deltaTime;
        xRotation -= input.y;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * input.x);
    }
    #endregion
}