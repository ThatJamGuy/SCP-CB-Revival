using FMODUnity;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SCP_914_Knob : MonoBehaviour, IHoldInteractable {
    [Serializable]
    private class KnobSelectAngles {
        public float angle;
        public SCP_914_Recipe.RefinementMode associatedRefinementMode;
    }

    [Header("SCP-914 Knob Settings")]
    [SerializeField] private float knobRotationSpeed = 100f;
    [SerializeField, Min(0.01f)] private float knobClickDistance = 10f;
    [SerializeField] private float minAngle = -90f;
    [SerializeField] private float maxAngle = 90f;
    [SerializeField] private KnobSelectAngles[] knobSelectAngles;

    [Header("References")]
    [SerializeField] private SCP_914 scp914;
    [SerializeField] private EventReference knobClickSound;
    [SerializeField] private EventReference knobSelectSound;

    private InputAction lookAction;

    private const float INPUT_DEADZONE = 0.001f;

    private bool inUsage;
    private float currentZAngle;
    private float lastClickAngle;

    #region Unity Lifecycle

    private void Start() {
        lookAction = RevivalRuntimeEngine.Instance.GetAction("Player", "Look");

        // Use the signed angle everywhere so negative select angles match the knob's starting rotation
        currentZAngle = Mathf.DeltaAngle(0f, transform.localEulerAngles.z);
        lastClickAngle = currentZAngle;

        // Set the default refinement mode in SCP-914 to whatever the knob rotation is set to right now. Mostly just a consistency thing
        if (TryGetClosestMode(currentZAngle, out SCP_914_Recipe.RefinementMode startMode)) {
            scp914.currentRefinementMode = startMode;
        }
    }

    private void Update() {
        if (!inUsage) return;

        float input = lookAction.ReadValue<Vector2>().x * knobRotationSpeed * Time.deltaTime;
        if (Mathf.Abs(input) < INPUT_DEADZONE) return;

        float newAngle = Mathf.Clamp(currentZAngle + input, minAngle, maxAngle);

        // Pinned against either end of the knob's range, nothing to update
        if (Mathf.Approximately(newAngle, currentZAngle)) return;

        currentZAngle = newAngle;
        transform.localRotation = Quaternion.Euler(0f, 0f, currentZAngle);

        PlayClicks();
        CheckRefinementModeChange();
    }

    #endregion

    #region Private Methods

    // Plays a click for every click-distance the knob has travelled since the last click
    private void PlayClicks() {
        float distanceSinceLastClick = currentZAngle - lastClickAngle;
        int clicks = (int)(Mathf.Abs(distanceSinceLastClick) / knobClickDistance);
        if (clicks == 0) return;

        for (int i = 0; i < clicks; i++) {
            AudioManager.PlayOneShot(knobClickSound, transform.position);
        }

        lastClickAngle += clicks * knobClickDistance * Mathf.Sign(distanceSinceLastClick);
    }

    // Checks whether the knob is now aligned with a known select angle and updates the refinement mode.
    private void CheckRefinementModeChange() {
        if (!TryGetClosestMode(currentZAngle, out SCP_914_Recipe.RefinementMode mode)) return;
        if (scp914.currentRefinementMode == mode) return;

        scp914.currentRefinementMode = mode;
        AudioManager.PlayOneShot(knobSelectSound, transform.position);
    }

    private void StopUsage() {
        Player.Instance.disableLooking = false;
        inUsage = false;
    }

    #endregion

    #region Private Utilities

    // Finds the select angle closest to the given angle, as long as it is within one click distance of it
    private bool TryGetClosestMode(float angle, out SCP_914_Recipe.RefinementMode mode) {
        mode = default;

        if (knobSelectAngles == null) return false;

        bool found = false;
        float bestDistance = float.MaxValue;

        for (var i = 0; i < knobSelectAngles.Length; i++) {
            KnobSelectAngles knobAngle = knobSelectAngles[i];
            float distance = Mathf.Abs(angle - knobAngle.angle);

            if (distance > knobClickDistance || distance >= bestDistance) continue;

            bestDistance = distance;
            mode = knobAngle.associatedRefinementMode;
            found = true;
        }

        return found;
    }

    #endregion

    #region Public Methods

    public void BeginInteract(PlayerInteraction playerInteraction) {
        Player.Instance.disableLooking = true;
        inUsage = true;

        lastClickAngle = currentZAngle;
    }

    public void EndInteract(PlayerInteraction playerInteraction) { StopUsage(); }
    public void ForceStopInteract() { StopUsage(); }

    #endregion
}