using UnityEngine;

/// <summary>
/// Player instance class that will carry all publicly available information about the player.
/// </summary>
public class Player : MonoBehaviour {
    public static Player Instance;
    
    [Header("Movements Settings")]
    public float walkSpeed = 3f;
    public float sprintSpeed = 7f;
    public float crouchSpeed = 1f;

    [Header("Player States")] 
    public bool disableInput;
    public bool isMoving;
    public bool isSprinting;
    public bool isCrouching;
    public bool isDead;
    
    [Header("Player Status")]
    [Range(0, 1)] public float stamina = 1f;

    [HideInInspector] public SettingsData settingsData;
    
    private static bool _desiredCursorVisible;

    #region Unity Callbacks
    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        settingsData = DataSaver.Load<SettingsData>("settings.json");
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
    // Re-apply cursor state when switching back to KBM so menus restore correctly
    private static void OnDeviceChanged(bool usingController) {
        if (!usingController) ApplyCursorState();
    }
    
    // Apply cursor state only when on KBM; skip silently on controller
    private static void ApplyCursorState() {
        if (InputManager.Instance.UsingController) return;
        Cursor.lockState = _desiredCursorVisible ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = _desiredCursorVisible;
    }
    #endregion

    public static void SetCursorState(bool visibleAndUnlocked) {
        _desiredCursorVisible = visibleAndUnlocked;
        ApplyCursorState();
    }
}