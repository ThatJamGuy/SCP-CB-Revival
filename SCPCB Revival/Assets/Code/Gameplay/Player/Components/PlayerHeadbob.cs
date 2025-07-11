using UnityEngine;

namespace scpcbr {
    public class PlayerHeadbob : MonoBehaviour {
        [Header("References")]
        [SerializeField] private PlayerBase playerBase;
        [SerializeField] private PlayerFootsteps playerFootsteps;

        [Header("Headbob Settings")]
        [SerializeField] private float walkBobSpeed = 8f;
        [SerializeField] private float sprintBobSpeed = 13f;
        [SerializeField] private float bobAmount = 0.1f;

        [Header("Rotation Settings")]
        [SerializeField] private float rotationStrength = 0.7f;
        [SerializeField] private float maxRotationAngle = 0.7f;
        [SerializeField] private float rotationSpeed = 0.5f;

        private float defaultYPos;
        private float bobTimer;
        private bool hasPlayedFootstep;
        private Vector3 defaultRotation;

        #region Default Methods
        private void Start() {
            defaultYPos = transform.localPosition.y;
            defaultRotation = transform.localRotation.eulerAngles;

            if (GlobalCameraShake.Instance != null)
                GlobalCameraShake.Instance.RegisterCamera(GetComponentInChildren<Camera>().transform);
        }

        private void Update() {
            float movementSpeed = playerBase.isSprinting ? sprintBobSpeed : walkBobSpeed;

            if (playerBase.isCrouching)
                movementSpeed = walkBobSpeed;

            if (playerBase.isMoving) {
                bobTimer += Time.deltaTime * movementSpeed;

                float bobOffset = Mathf.Sin(bobTimer) * bobAmount;
                transform.localPosition = new Vector3(
                    transform.localPosition.x,
                    defaultYPos + bobOffset,
                    transform.localPosition.z
                );

                float rotationOffset = Mathf.Sin(bobTimer * rotationSpeed) * maxRotationAngle * rotationStrength;

                transform.localRotation = Quaternion.Euler(
                    defaultRotation.x,
                    defaultRotation.y,
                    defaultRotation.z + rotationOffset
                );

                if (bobOffset < 0 && !hasPlayedFootstep) {
                    playerFootsteps.PlayFootstepAudio();
                    hasPlayedFootstep = true;
                }
                else if (bobOffset >= 0) {
                    hasPlayedFootstep = false;
                }
            }
        }
        #endregion
    }
}