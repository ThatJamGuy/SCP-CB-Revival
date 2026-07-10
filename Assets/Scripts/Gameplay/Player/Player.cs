using PrimeTween;
using System.Collections;
using UnityEngine;

/// <summary>
/// Player instance class that will carry all publicly available information about the player.
/// </summary>
public class Player : MonoBehaviour {
    public static Player Instance { get; private set; }

    [Header("Movements Settings")]
    public float walkSpeed = 3f;
    public float sprintSpeed = 7f;
    public float crouchSpeed = 1f;

    [Header("Player Modifiers")]
    public float staminaDepletionModifier;
    public float blinkDepletionModifier;
    public float bloodLossModifier;

    [Header("Player States")]
    public bool disableInput;
    public bool disableLooking;
    public bool isMoving;
    public bool isSprinting;
    public bool isCrouching;
    public static bool isBlinking { get; set; }
    public bool isDead;

    public GameObject cameraRoot;
    public Camera playerCamera;

    private Sequence currentDeathTween;
    private Vector3 cameraStartPos;
    private Quaternion cameraStartRot;

    [HideInInspector] public SettingsData settingsData;

    private static bool desiredCursorVisible { get; set; }

    #region Unity Callbacks

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        settingsData = DataSaver.Load<SettingsData>("settings.json");

        cameraStartPos = cameraRoot.transform.localPosition;
        cameraStartRot = cameraRoot.transform.localRotation;
    }

    private void OnEnable() {
        if (InputManager.Instance != null)
            InputManager.Instance.OnInputDeviceChanged += OnDeviceChanged;
    }

    private void OnDisable() {
        if (InputManager.Instance != null)
            InputManager.Instance.OnInputDeviceChanged -= OnDeviceChanged;
    }

    #endregion

    #region Private Methods

    private void PlayFallForwardAnim(float animDuration = 0.5f) {
        currentDeathTween = Sequence.Create()

        // Forward falling
        .Group(Tween.LocalPosition(
            cameraRoot.transform, cameraStartPos + new Vector3(0f, -2f, 2f), animDuration, Ease.OutCubic))

        // Forward rotation
        .Group(Tween.LocalRotation(
            cameraRoot.transform, Quaternion.Euler(80f, 0f, 0f), animDuration, Ease.OutCubic))

        // Slight delayed Y axis angle
        .Group(Tween.LocalRotation(
            cameraRoot.transform, Quaternion.Euler(29f, 40f, 39f), 0.1f, Ease.OutCubic, startDelay: animDuration - 0.1f));
    }

    private void PlayFallForwardNeckAnim(float animDuration = 0.5f) {
        cameraRoot.transform.eulerAngles = new Vector3(0f, 180f, 0f);

        currentDeathTween = Sequence.Create()

        // Forward falling
        .Group(Tween.LocalPosition(
            cameraRoot.transform, cameraStartPos + new Vector3(0f, -2f, 2f), animDuration, Ease.Linear))

        // Forward rotation
        .Group(Tween.LocalRotation(
            cameraRoot.transform, Quaternion.Euler(-70f, -180f, 7f), animDuration, Ease.OutCubic));
    }

    private void PlayFallBackwardAnim(float animDuration = 0.5f) {
        currentDeathTween = Sequence.Create()

        // Backward falling
        .Group(Tween.LocalPosition(
            cameraRoot.transform, cameraStartPos + new Vector3(0f, -2f, -2f), animDuration, Ease.OutCubic))

        // Backward rotation
        .Group(Tween.LocalRotation(
            cameraRoot.transform, Quaternion.Euler(-90f, 0f, 0f), animDuration, Ease.OutCubic));
    }

    private IEnumerator DisplayDeathMessageCoroutine(float delayTime, string causeOfDeath) {
        yield return new WaitForSeconds(delayTime);
        GameManager.Instance.ShowDeathScreen(causeOfDeath);
    }

    // Re-apply cursor state when switching back to KBM so menus restore correctly
    private static void OnDeviceChanged(bool usingController) {
        if (!usingController) ApplyCursorState();
    }

    // Apply cursor state only when on KBM; skip silently on controller
    private static void ApplyCursorState() {
        if (InputManager.Instance.UsingController) return;
        Cursor.lockState = desiredCursorVisible ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = desiredCursorVisible;
    }
    #endregion

    #region Public Methods

    public static void SetCursorState(bool visibleAndUnlocked) {
        desiredCursorVisible = visibleAndUnlocked;
        ApplyCursorState();
    }

    public void KillPlayer(int killType, float animDuration, float deathMessageDelay, string causeOfDeath) {
        if (isDead) return;

        disableInput = true;
        disableLooking = true;
        isMoving = false;
        isDead = true;

        currentDeathTween.Stop();

        // Animate the player camera based on killType
        switch (killType) {
            case 0:
                // Nothing (Scripted animation deaths or just no animation)
                break;
            case 1:
                PlayFallForwardAnim(animDuration);
                break;
            case 2:
                PlayFallForwardNeckAnim(animDuration);
                break;
            case 3:
                PlayFallBackwardAnim(animDuration);
                break;
        }

        StartCoroutine(DisplayDeathMessageCoroutine(deathMessageDelay, causeOfDeath));
    }

    #endregion
}