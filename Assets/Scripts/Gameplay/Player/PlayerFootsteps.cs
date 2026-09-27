using FMOD.Studio;
using FMODUnity;
using UnityEngine;

/// <summary>
/// System I made for handling player footsteps. (Now with code comments!)
/// Used to be material based, but now tag based since the build had issues with rooms spawned in at runtime.
/// </summary>
public class PlayerFootsteps : MonoBehaviour {
    [SerializeField] private FootstepData[] footstepData;
    [SerializeField] private LayerMask groundLayer = -1;

    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private CharacterController characterController;

    private VCA footstepVCA;
    private float currentFootstepVolume = -1f;

    private bool isSprinting;
    private bool isCrouching;
    private bool isMoving;

    #region Unity Callbacks

    private void Awake() {
        footstepVCA = RuntimeManager.GetVCA("vca:/FootstepVCA");
    }

    // Just calls UpdateFoosteps() every frame so the script knows what movement state the player is in
    private void Update() {
        UpdateFootsteps();
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Play a footstep sound based on various factors such as the surface tag under the player and their movement state
    /// </summary>
    public void PlayFootstepAudio() {
        var surfaceCollider = GetSurfaceColliderUnderPlayer();
        if (!surfaceCollider) return;

        var footstep = GetFootstepDataForSurface(surfaceCollider);
        if (!footstep) return;

        var eventRef = isSprinting ? footstep.associatedRunEvent : footstep.associatedWalkEvent;
        if (eventRef.IsNull) return;

        AudioManager.PlayOneShot(eventRef, transform.position);
    }
    #endregion

    #region Private Methods
    // Update what kind of footstep to use based on the players movement state (Walking, Sprinting, Crouching)
    private void UpdateFootsteps() {
        isMoving = player.isMoving;
        isSprinting = player.isSprinting;
        isCrouching = player.isCrouching;
        if (!isMoving) return;

        var targetVolume = isCrouching ? 0.3f : 1.0f; // If crouching, set volume to 0.3, otherwise 1 (Full Volume)
        if (Mathf.Approximately(targetVolume, currentFootstepVolume)) return;

        footstepVCA.setVolume(targetVolume);
        currentFootstepVolume = targetVolume;
    }
    #endregion

    #region Helpers :)
    // Shoots a raycast downwards to find the surface tag under the player, assuming they are grounded on something with a collider
    private Collider GetSurfaceColliderUnderPlayer() {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, characterController.height, groundLayer)) {
            return hit.collider; // Returns the collider the raycast hit
        }
        return null; // Otherwise return nothing
    }

    // Returns the FootstepData whose surface tag matches the collider found in GetSurfaceColliderUnderPlayer()
    private FootstepData GetFootstepDataForSurface(Collider surface) {
        foreach (FootstepData data in footstepData) {
            if (!string.IsNullOrEmpty(data.surfaceTag) && surface.CompareTag(data.surfaceTag)) return data; // Returns data that matches the tag the fella is on
        }
        return null; // Otherwise return nothing
    }
    #endregion
}