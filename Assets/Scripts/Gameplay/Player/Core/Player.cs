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
    public bool isDead;
    
    [Header("Player Status")]
    [Range(0, 1)] public float stamina = 1f;

    [HideInInspector] public SettingsData settingsData;

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        settingsData = DataSaver.Load<SettingsData>("settings.json");
    }

    public static void SetCursorState(bool cursorVisibleAndUnlocked) {
        Cursor.lockState = cursorVisibleAndUnlocked ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = cursorVisibleAndUnlocked;
    }
}