using UnityEngine;

public class PlayerAccessor : MonoBehaviour {
    public static PlayerAccessor instance;

    public bool allowInput = true;

    [Header("Player Status")]
    public bool isMoving;
    public bool isSprinting;
    public bool isCrouching;

    [Header("Player Modifiers")]
    public bool infiniteStamina = false;
    public float staminaDepletionModifier = 0f;

    [Header("References")]
    public Camera playerCamera;

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    private void Update() {
        var im = InputManager.Instance;
        if (im != null && allowInput) {
            isMoving = im.IsMoving;
            isSprinting = im.IsSprinting;
        }
    }

    public void EnablePlayerInputs() {
        allowInput = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void DisablePlayerInputs(bool showMouse) {
        allowInput = false;

        if (showMouse) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void TogglePlayerInputs(bool showMouse) {
        allowInput = !allowInput;
        if (allowInput) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        } else if (showMouse) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public Camera GetPlayerCamera() {
        return playerCamera;
    }
}