using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float gravity = 9.81f;

    [Header("Look Settings")]
    [SerializeField] private float lookSpeed = 2f;
    [SerializeField] private float maxLookX = 90f;
    [SerializeField] private float minLookX = -90f;

    private CharacterController characterController;
    private Vector3 moveDirection;
    private float currentSpeed;
    private float rotationX;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        currentSpeed = walkSpeed;
        UpdateCursorState();
    }

    private void Update()
    {
        if (GameManager.Instance.disablePlayerInputs) return;
        HandleMovement();
        HandleLook();
    }

    private void HandleMovement()
    {
        if (characterController.isGrounded)
        {
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");

            moveDirection = transform.TransformDirection(new Vector3(moveX, 0, moveZ)) * DetermineCurrentSpeed();
        }

        moveDirection.y -= gravity * Time.deltaTime;
        characterController.Move(moveDirection * Time.deltaTime);
    }

    private float DetermineCurrentSpeed()
    {
        if (Input.GetKey(KeyCode.LeftShift)) return sprintSpeed;
        if (Input.GetKey(KeyCode.LeftControl)) return crouchSpeed;
        return walkSpeed;
    }

    private void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

        rotationX = Mathf.Clamp(rotationX - mouseY, minLookX, maxLookX);
        transform.Rotate(0, mouseX, 0);
        Camera.main.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
    }

    private void UpdateCursorState()
    {
        Cursor.lockState = GameManager.Instance.disablePlayerInputs ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = GameManager.Instance.disablePlayerInputs;
    }
}