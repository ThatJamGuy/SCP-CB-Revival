using FMOD.Studio;
using FMODUnity;
using UnityEngine;

/// <summary>
/// System I made for handling player footsteps. (Now with code comments!)
/// Used to be material based, but now tag based since the build had issues with rooms spawned in at runtime.
/// </summary>
public class PlayerFootsteps : MonoBehaviour {
    [SerializeField] private FootstepData[] footstepData;
    [SerializeField] private PlayerAccessor playerAccessor;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private LayerMask groundLayer = -1;

    private VCA footstepVCA;
    private PlayerMovement playerMovement;

    private bool isSprinting;
    private bool isCrouching;
    private bool isMoving;

    #region Unity Callbacks
    // Immediately set some stuff needed for the script
    private void Awake() {
        footstepVCA = RuntimeManager.GetVCA("vca:/FootstepVCA");
        playerMovement = GetComponentInParent<PlayerMovement>();
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
        string surfaceTag = GetSurfaceTagUnderPlayer();
        if (string.IsNullOrEmpty(surfaceTag)) return;

        FootstepData footstep = GetFootstepDataForTag(surfaceTag);
        if (footstep == null) return;

        EventReference eventRef = isSprinting ? footstep.assocatedRunEvent : footstep.assocatedWalkEvent;
        if (eventRef.IsNull) return;

        AudioManager.instance.PlaySound(eventRef, transform.position);
    }
    #endregion

    #region Private Methods
    // Update what kind of footstep to use based on the players movement state (Walking, Sprinting, Crouching)
    private void UpdateFootsteps() {
        isMoving = playerAccessor.isMoving;
        isSprinting = playerMovement != null ? playerMovement.IsActuallySprinting : false;
        isCrouching = playerAccessor.isCrouching;
        if (!isMoving) return;

        footstepVCA.setVolume(isCrouching ? 0.3f : 1.0f); // If crouching, set volume to 0.3, otherwise 1 (Full Volume)
    }
    #endregion

    #region Helpers :)
    // Shoots a raycast downwards to find the surface tag under the player, assuming they are grounded on something with a collider
    private string GetSurfaceTagUnderPlayer() {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, characterController.height, groundLayer)) {
            return hit.collider.tag; // Returns the tag of the collider the raycast hit
        }
        return null; // Otherwise return nothing
    }

    // Returns the FootstepData associated with the given surface tag found in the previous method GetSurfaceTagUnderPlayer()
    private FootstepData GetFootstepDataForTag(string tag) {
        foreach (FootstepData data in footstepData) {
            if (data.surfaceTag == tag) return data; // Returns data that matches the tag the fella is on
        }
        return null; // Otherwise return nothing
    }
    #endregion
}