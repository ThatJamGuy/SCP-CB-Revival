using System;
using UnityEngine;

public class PlayerHeadbob : MonoBehaviour {
    [Header("Headbob Settings")]
    [SerializeField] private float crouchBobSpeed = 5f;
    [SerializeField] private float walkBobSpeed = 8f;
    [SerializeField] private float sprintBobSpeed = 13f;
    [SerializeField] private float bobAmount = 0.1f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationStrength = 0.7f;
    [SerializeField] private float maxRotationAngle = 0.7f;
    [SerializeField] private float rotationSpeed = 0.5f;

    [Header("References")] 
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerFootsteps playerFootsteps;

    private Player player;

    private float defaultYPos;
    private float bobTimer;
    private float movementSpeed;
    private bool hasPlayedFootstep;
    private Vector3 defaultRotation;

    #region Unity Callbacks
    private void Start() {
        // Set the player variable to the Instance in the world if available, which is should be
        player = Player.Instance;
        
        // Set default position and rotation for the camera so the headbob can utilize these values
        defaultYPos = transform.localPosition.y;
        defaultRotation = transform.localRotation.eulerAngles;
    }

    private void Update() {
        var bobOffset = Mathf.Sin(bobTimer) * bobAmount;
        var rotationOffset = Mathf.Sin(bobTimer * rotationSpeed) * maxRotationAngle * rotationStrength;
        
        // If the player is sprinting, set the local movement speed to the sprintBobSeed value
        // If not, but the player is crouching, then set the local movements speed to the crouchBobSpeed value
        // If none of those check out, set the movementSpeed to the walkBobSpeed
        if (player.isSprinting)
            movementSpeed = sprintBobSpeed;
        else if (player.isCrouching)
            movementSpeed =  crouchBobSpeed;
        else movementSpeed = walkBobSpeed;
        
        // If the player is moving at all, then increase the bobTimer every second * the current movementSpeed
        if (player.isMoving)
            bobTimer += Time.deltaTime * movementSpeed;
        
        transform.localPosition = new Vector3(
            transform.localPosition.x,
            defaultYPos + bobOffset,
            transform.localPosition.z
        );

        transform.localRotation = Quaternion.Euler(
            defaultRotation.x,
            defaultRotation.y,
            defaultRotation.z + rotationOffset
        );

        // Calculations for playing the footstep sound
        switch (bobOffset) {
            // If bobOffset is less than 0 play the step sound and set the bool to true so it can't spam the sound
            // But if it's 0 or higher than set the bool to false and don't play anything
            case < 0 when !hasPlayedFootstep:
                playerFootsteps.PlayFootstepAudio();
                hasPlayedFootstep = true;
                break;
            case >= 0:
                hasPlayedFootstep = false;
                break;
        }
    }
    #endregion
}