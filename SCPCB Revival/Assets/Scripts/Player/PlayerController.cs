using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    public float crouchSpeed = 2.5f;
    public float gravity = 9.81f;

    [Header("Look Settings")]
    public float lookSpeed = 2f;
    public float maxLookX = 90f;
    public float minLookX = -90f;

    private CharacterController characterController;
    private Vector3 moveDirection;
    private float currentSpeed;
    private float rotationX;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        currentSpeed = walkSpeed;

        UpdateCursorState();
    }

    void Update()
    {
        if (GameManager.Instance.disablePlayerInputs)
        {
            return;
        }

        Move();
        Look();
    }

    private void Move()
    {
        if (characterController.isGrounded)
        {
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");

            moveDirection = transform.TransformDirection(new Vector3(moveX, 0, moveZ));
            currentSpeed = DetermineCurrentSpeed();
            moveDirection *= currentSpeed;
        }

        moveDirection.y -= gravity * Time.deltaTime;
        characterController.Move(moveDirection * Time.deltaTime);
    }

    private float DetermineCurrentSpeed()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            return sprintSpeed;
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            return crouchSpeed;
        }
        return walkSpeed;
    }

    private void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, minLookX, maxLookX);

        transform.Rotate(0, mouseX, 0);
        Camera.main.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
    }

    private void UpdateCursorState()
    {
        if (GameManager.Instance.disablePlayerInputs)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}