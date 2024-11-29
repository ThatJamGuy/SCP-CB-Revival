using UnityEngine;

public class HeadBob : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private FootstepHandler footstepHandler;
    [SerializeField] private float walkBobSpeed = 5f;
    [SerializeField] private float sprintBobSpeed = 10f;
    [SerializeField] private float bobAmount = 0.1f;

    private float defaultYPos;
    private float bobTimer;
    private bool hasPlayedFootstep;

    private void Start()
    {
        defaultYPos = transform.localPosition.y;
    }

    private void Update()
    {
        if (playerController.isMoving)
        {
            float speed = playerController.isSprinting ? sprintBobSpeed : walkBobSpeed;
            bobTimer += Time.deltaTime * speed;

            float bobOffset = Mathf.Sin(bobTimer) * bobAmount;
            transform.localPosition = new Vector3(transform.localPosition.x, defaultYPos + bobOffset, transform.localPosition.z);

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