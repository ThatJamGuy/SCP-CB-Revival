using scpcbr;
using UnityEngine;

public class PlayerAccessor : MonoBehaviour {
    public static PlayerAccessor instance;

    public bool allowInput = true;

    [Header("Player Status")]
    public bool isMoving;
    public bool isSprinting;
    public bool isCrouching;
    public bool isBlinking;
    public bool isDead;

    [Header("Player Modifiers")]
    public bool infiniteStamina = false;
    public bool infiniteBlink = false;
    public float staminaDepletionModifier = 0f;
    public float blinkDepletionModifier = 0f;

    [Header("References")]
    public Camera playerCamera;
    public Transform playerCameraRoot;

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    private void Start() {
        if (GlobalCameraShake.instance != null && playerCamera != null) {
            GlobalCameraShake.instance.RegisterCamera(playerCameraRoot);
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