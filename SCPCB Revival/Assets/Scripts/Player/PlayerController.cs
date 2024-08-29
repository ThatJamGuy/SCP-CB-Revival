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

    [Header("Headbob Settings")]
    [SerializeField] private float headbobFrequency = 1.5f;
    [SerializeField] private float headbobAmplitude = 0.1f;
    [SerializeField] private float headbobRotationFrequency = 1.5f;
    [SerializeField] private float headbobRotationAmplitude = 0.1f;
    [SerializeField] private float runningHeadbobMultiplier = 1.5f;

    [Header("Footstep Settings")]
    [SerializeField] private FootstepData[] footstepData;
    [SerializeField] private AudioSource footstepAudioSource;

    private CharacterController characterController;
    private Vector3 moveDirection;
    private float rotationX;
    private bool isMoving;
    private bool isSprinting;
    private float headbobTimer;
    private Vector3 initialCameraPosition;
    private Quaternion initialCameraRotation;
    private bool footstepPlayed;
    private float previousHeadbobOffset = float.MaxValue;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        UpdateCursorState();
        initialCameraPosition = Camera.main.transform.localPosition;
        initialCameraRotation = Camera.main.transform.localRotation;
    }

    private void Update()
    {
        if (GameManager.Instance.disablePlayerInputs) return;

        HandleMovement();
        HandleLook();
        HandleHeadbob();
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

    // HEADBOBBING - Made a goofy fix for the sudden snappiness from switching from walking to running
    // and vice versa. It's really weird though and probably shouldn't be shipped in the final game :skull:

    private float transitionProgress = 0.9f;
    private const float transitionSpeed = 0.1f; // Adjust this value for faster or slower transitions

    private void HandleHeadbob()
    {
        if (isMoving)
        {
            headbobTimer += Time.deltaTime;

            float targetFrequencyMultiplier = isSprinting ? runningHeadbobMultiplier : 1.0f;
            float targetAmplitudeMultiplier = isSprinting ? runningHeadbobMultiplier : 1.0f;

            if (isSprinting)
            {
                transitionProgress = Mathf.Clamp01(transitionProgress + Time.deltaTime * transitionSpeed);
            }
            else
            {
                transitionProgress = Mathf.Clamp01(transitionProgress - Time.deltaTime * transitionSpeed);
            }

            float currentFrequencyMultiplier = Mathf.Lerp(1.0f, runningHeadbobMultiplier, transitionProgress);
            float currentAmplitudeMultiplier = Mathf.Lerp(1.0f, runningHeadbobMultiplier, transitionProgress);

            float currentHeadbobOffset = Mathf.Sin(headbobTimer * headbobFrequency * currentFrequencyMultiplier) * headbobAmplitude * currentAmplitudeMultiplier;

            Vector3 newPosition = initialCameraPosition + new Vector3(0, currentHeadbobOffset, 0);
            Quaternion newRotation = CalculateHeadbobRotation(currentFrequencyMultiplier, currentAmplitudeMultiplier);

            Camera.main.transform.localPosition = newPosition;
            Camera.main.transform.localRotation = Quaternion.Euler(rotationX, 0, 0) * newRotation;

            if (currentHeadbobOffset > previousHeadbobOffset && previousHeadbobOffset < 0 && !footstepPlayed)
            {
                PlayFootstepAudio();
                footstepPlayed = true;
            }

            if (currentHeadbobOffset >= 0) footstepPlayed = false;
            previousHeadbobOffset = currentHeadbobOffset;
        }
        else
        {
            headbobTimer = 0;
            transitionProgress = 0.9f;
        }
    }

    // TODO: FIX THE HEADBOB AT SOME POINT. IM JUST LEAVING IT LIKE THIS BECAUSE IM LAZY AND DONT
    // WANT TO SPEND TOO MUCH TIME ON THE DAMN HEADBOBBING.


    private Quaternion CalculateHeadbobRotation(float frequencyMultiplier, float amplitudeMultiplier)
    {
        float headbobRotation = Mathf.Sin(headbobTimer * headbobRotationFrequency * frequencyMultiplier) * headbobRotationAmplitude * amplitudeMultiplier;
        return initialCameraRotation * Quaternion.Euler(0, 0, headbobRotation);
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