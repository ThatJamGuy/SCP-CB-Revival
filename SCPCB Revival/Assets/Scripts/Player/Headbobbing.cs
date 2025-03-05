using UnityEngine;

public class HeadBob : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private FootstepHandler footstepHandler;
    [SerializeField] private float walkBobSpeed = 5f;
    [SerializeField] private float sprintBobSpeed = 10f;
    [SerializeField] private float bobAmount = 0.1f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationStrength = 5f;
    [SerializeField] private float maxRotationAngle = 5f;
    [SerializeField] private float rotationSpeed = 2f;

    private float defaultYPos;
    private float bobTimer;
    private bool hasPlayedFootstep;
    private Vector3 defaultRotation;

    private void Start()
    {
        defaultYPos = transform.localPosition.y;
        defaultRotation = transform.localRotation.eulerAngles;
    }

    private void Update()
    {
        if (GameManager.Instance.disablePlayerInputs) return;

        float movementSpeed = playerController.isSprinting ? sprintBobSpeed : walkBobSpeed;

        if (playerController.isMoving)
        {
            bobTimer += Time.deltaTime * movementSpeed;

            // Position Bob
            float bobOffset = Mathf.Sin(bobTimer) * bobAmount;
            transform.localPosition = new Vector3(
                transform.localPosition.x,
                defaultYPos + bobOffset,
                transform.localPosition.z
            );

            // Rotation Bob (SCP-like left and right rotation)
            float rotationOffset = Mathf.Sin(bobTimer * rotationSpeed) * maxRotationAngle * rotationStrength;

            transform.localRotation = Quaternion.Euler(
                defaultRotation.x,
                defaultRotation.y,
                defaultRotation.z + rotationOffset
            );

            // Footstep Logic
            if (bobOffset < 0 && !hasPlayedFootstep)
            {
                footstepHandler.PlayFootstepAudio();
                hasPlayedFootstep = true;
            }
            else if (bobOffset >= 0)
            {
                hasPlayedFootstep = false;
            }
        }
    }
}