using System;
using UnityEngine;
using UnityEngine.InputSystem;
using FMODUnity;

/// <summary>
/// Script attached to the knob thing for SCP-914. Handles the knob SFX and logic for changing the refinement mode
/// </summary>
public class SCP_914_Knob : MonoBehaviour, IHoldInteractable {
    [Serializable]
    private class KnobSelectAngles {
        public float angle;
        public RefineryRecipeTable.RefinementMode associatedRefinementMode;
    }
    
    [Header("SCP-914 Knob Settings")]
    [SerializeField] private float knobRotationSpeed = 100f;
    [SerializeField] private float knobClickDistance;
    [SerializeField] private KnobSelectAngles[] knobSelectAngles;

    [Header("References")] 
    [SerializeField] private SCP_914 scp914;
    [SerializeField] private EventReference knobClickSound;
    [SerializeField] private EventReference knobSelectSound;

    private InputAction lookAction;

    private Quaternion oldRotation;
    
    private bool inUsage;
    private float currentZAngle;
    private float lastClickAngle;

    #region Unity Callbacks
    
    private void Start() {
        lookAction = InputManager.Instance.GetAction("Player", "Look");
        
        // Set the default refinement mode in SCP-914 to whatever the knob rotation is set to right now (MUST BE EXACT)
        foreach (var knobAngle in knobSelectAngles) {
            if (Mathf.Approximately(gameObject.transform.localEulerAngles.z, knobAngle.angle)) {
                scp914.currentRefinementMode = knobAngle.associatedRefinementMode;
            }
        }
        
        currentZAngle = NormalizeAngle(transform.localEulerAngles.z);
        lastClickAngle = transform.localEulerAngles.z;
    }

    private void Update() {
        if (!inUsage) return;
        
        CheckRefinementModeChange();
        
        var input = lookAction.ReadValue<Vector2>().x * knobRotationSpeed * Time.deltaTime;
        
        if (Mathf.Abs(input) < 0.001f) return;
        
        var distanceSinceLastClick = currentZAngle - lastClickAngle;
        
        currentZAngle = Mathf.Clamp(currentZAngle + input, -90f, 90f);
        transform.rotation = Quaternion.Euler(0, 0, currentZAngle);
        
        while (Mathf.Abs(distanceSinceLastClick) >= knobClickDistance) {
            AudioManager.PlayOneShot(knobClickSound, transform.position);
            
            lastClickAngle += knobClickDistance * Mathf.Sign(distanceSinceLastClick);
            distanceSinceLastClick = currentZAngle - lastClickAngle;
        }
    }
    
    #endregion
    
    #region Private Methods
    
    /// <summary>
    /// Checks whether the knob is now aligned with a known select angle and updates the refinement mode.
    /// </summary>
    private void CheckRefinementModeChange() {
        foreach (var knobAngle in knobSelectAngles) {
            if (!(Mathf.Abs(currentZAngle - knobAngle.angle) <= knobClickDistance)) continue;
            if (scp914.currentRefinementMode == knobAngle.associatedRefinementMode) continue;
            
            scp914.currentRefinementMode = knobAngle.associatedRefinementMode;
            AudioManager.PlayOneShot(knobSelectSound, transform.position);
        }
    }
    
    /// <summary>
    /// Converts a wrapped Euler angle into a signed angle.
    /// Example: 350 becomes -10.
    /// </summary>
    private static float NormalizeAngle(float angle) {
        angle %= 360f;

        if (angle > 180f)
            angle -= 360f;

        return angle;
    }
    
    #endregion
    
    #region Public Methods
    
    public void BeginInteract(PlayerInteraction playerInteraction) {
        Player.Instance.disableLooking = true;
        inUsage = true;
        
        lastClickAngle = currentZAngle;
    }

    public void EndInteract(PlayerInteraction playerInteraction) {
        Player.Instance.disableLooking = false;
        inUsage = false;
    }
    
    public void ForceStopInteract() {
        Player.Instance.disableLooking = false;
        inUsage = false;
    }
    
    #endregion
}