using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [System.Serializable]
    public struct FootstepData
    {
        public Texture texture;
        public AudioClip[] walkingFootstepAudio;
        public AudioClip[] runningFootstepAudio;
    }

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float gravity = 9.81f;

    [Header("Look Settings")]
    [SerializeField] private float lookSpeed = 2f;
    [SerializeField] private float maxLookX = 90f;
    [SerializeField] private float minLookX = -90f;

    [Header("Footstep Settings")]
    [SerializeField] private FootstepData[] footstepData;
    [SerializeField] private AudioSource footstepAudioSource;

    [Header("Toggles")]
    [SerializeField] private bool enableHeadbobFootsteps = true;

    private CharacterController characterController;
    private Vector3 moveDirection;
    private float rotationX;
    private bool isMoving;
    private bool isSprinting;
    private bool footstepPlayed;
    private float footstepTimer;
    private float footstepInterval;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        UpdateCursorState();
        footstepTimer = 0f;
    }

    private void Update()
    {
        if (GameManager.Instance.disablePlayerInputs) return;

        HandleMovement();
        HandleLook();
        HandleFootsteps();
    }

    private void HandleMovement()
    {
        if (characterController.isGrounded)
        {
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");

            moveDirection = transform.TransformDirection(new Vector3(moveX, 0, moveZ)) * DetermineCurrentSpeed();
            isMoving = moveX != 0 || moveZ != 0;
            isSprinting = Input.GetKey(KeyCode.LeftShift) && isMoving;
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

    private void HandleFootsteps()
    {
        // Handle footsteps if headbob footsteps are disabled
        if (!enableHeadbobFootsteps && isMoving)
        {
            // Set footstep interval based on speed
            footstepInterval = isSprinting ? 0.5f : 0.7f;

            // Update footstep timer
            footstepTimer += Time.deltaTime;

            if (footstepTimer >= footstepInterval)
            {
                PlayFootstepAudio();
                footstepTimer = 0f;
            }
        }
    }

    private void PlayFootstepAudio()
    {
        Texture currentTexture = GetCurrentTextureUnderPlayer();
        if (currentTexture == null) return;

        FootstepData footstep = GetFootstepDataForTexture(currentTexture);
        if (footstep.texture == null) return;

        AudioClip[] footstepSounds = isSprinting ? footstep.runningFootstepAudio : footstep.walkingFootstepAudio;
        if (footstepSounds.Length == 0) return;

        AudioClip footstepSound = footstepSounds[Random.Range(0, footstepSounds.Length)];
        footstepAudioSource.PlayOneShot(footstepSound);
    }

    private Texture GetCurrentTextureUnderPlayer()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, characterController.height + 1.0f))
        {
            MeshCollider meshCollider = hit.collider as MeshCollider;
            if (meshCollider != null && meshCollider.sharedMesh != null)
            {
                int submeshIndex = GetSubmeshIndex(meshCollider.sharedMesh, hit.triangleIndex);
                if (submeshIndex != -1)
                {
                    Material material = meshCollider.GetComponent<Renderer>()?.materials[submeshIndex];
                    return material?.mainTexture;
                }
            }
        }
        return null;
    }

    private int GetSubmeshIndex(Mesh mesh, int triangleIndex)
    {
        int triangleCount = 0;
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            int subMeshTriangleCount = mesh.GetTriangles(i).Length / 3;
            if (triangleIndex < triangleCount + subMeshTriangleCount) return i;
            triangleCount += subMeshTriangleCount;
        }
        return -1;
    }

    private FootstepData GetFootstepDataForTexture(Texture texture)
    {
        foreach (FootstepData data in footstepData)
        {
            if (data.texture == texture) return data;
        }
        return new FootstepData();
    }

    private void UpdateCursorState()
    {
        bool disablePlayerInputs = GameManager.Instance.disablePlayerInputs;
        Cursor.lockState = disablePlayerInputs ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = disablePlayerInputs;
    }
}