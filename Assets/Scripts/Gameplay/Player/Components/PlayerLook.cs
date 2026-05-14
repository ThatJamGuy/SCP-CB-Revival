using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Script to manage the player looking behavior, and only the looking behavior
/// </summary>
public class PlayerLook : MonoBehaviour {
    [Header("Look Settings")]
    [SerializeField] private float sensitivity = 25f;
    [SerializeField] private float mouseSmoothing = 95f;
    [SerializeField] private float maxLookAngle = 85f;

    [Header("References")]
    [SerializeField] private Transform playerBody;

    private InputAction lookAction;
    private Vector2 smoothedInput;
    
    private float xRotation;
    private bool cantFunction;

    #region Unity Callbacks

    private void Awake() {
        // Set the mouse sens and mouse smooth values according to the values in settings.json
        sensitivity = Player.Instance.settingsData.mouseSensitivity;
        mouseSmoothing = Player.Instance.settingsData.mouseSmoothing;
    }
    
    private void Start() {
        // If there is no InputManager available at the start, disallow functionality and print a warning in console
        if (InputManager.Instance == null) {
            cantFunction = true;
            Debug.Log("<color=red>[PlayerLook]</color> InputManager was not found, so looking will now work for now.");

            return;
        }
        
        // If the check passes, get the look action from the available InputManager
        lookAction = InputManager.Instance.GetAction("Player", "Look");
        
        // Lock the cursor and set it to invisible for gameplay by default
        Player.SetCursorState(false);
    }

    private void Update() {
        if (cantFunction || Player.Instance.disableInput) return;
        
        // Take the raw Vector2 (X, Y) input of the look action and multiply it by the mouse sensitivity
        // Then run the rawInput through ApplyMouseSmoothing to see if it needs to apply any smoothing at all
        var rawInput = lookAction.ReadValue<Vector2>() * sensitivity;
        var input = ApplyMouseSmoothing(rawInput) * Time.deltaTime;
        
        // Clamp the xRotation - input.y (Up and Down Mouse Movement), positive and negative maxLookAngle
        // Finally set the localRotation of this transform (CameraRoot object) to xRotation on the X only
        xRotation = Mathf.Clamp(xRotation - input.y, -maxLookAngle, maxLookAngle);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        
        // Rotate the playerBody GameObject on the Vector3.up Axis (Y Axis) multiplied by input.x (Look action X)
        playerBody.Rotate(Vector3.up * input.x);
    }
    #endregion
    
    #region Helpers
    private Vector2 ApplyMouseSmoothing(Vector2 rawInput) {
        // If mouseSmoothing is 0 consider mouse smoothing to be disabled and return the rawInput
        if (mouseSmoothing <= 0) return rawInput;
        
        // Define and set a lerpFactor from 1 (Start) to 0.03 (End) by mouseSmoothing divided by 100
        // Then set smoothedInput to a value interpolated between smoothedInput and rawInput by lerpFactor
        var lerpFactor = Mathf.Lerp(1f, 0.03f, mouseSmoothing / 100f);
        smoothedInput = Vector2.Lerp(smoothedInput, rawInput, lerpFactor);
        
        // Return the smoothedInput to the main looking functionality 
        return smoothedInput;
    }
    #endregion
}