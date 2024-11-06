using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FootstepHandler : MonoBehaviour
{
    [Header("Footstep Settings")]
    [SerializeField] private FootstepData[] footstepData;
    [SerializeField] private AudioSource footstepAudioSource;
    [SerializeField] private bool enableHeadbobFootsteps = true;

    private CharacterController characterController;
    private bool isSprinting;
    private bool isMoving;
    private float footstepTimer;
    private float footstepInterval;

    public void Initialize(CharacterController controller)
    {
        characterController = controller;
    }

    public void UpdateFootsteps(bool moving, bool sprinting)
    {
        isMoving = moving;
        isSprinting = sprinting;
        if (enableHeadbobFootsteps || !isMoving) return;

        footstepInterval = isSprinting ? 0.5f : 0.7f;
        footstepTimer += Time.deltaTime;

        if (footstepTimer >= footstepInterval)
        {
            PlayFootstepAudio();
            footstepTimer = 0f;
        }
    }

    private void PlayFootstepAudio()
    {
        Texture currentTexture = GetCurrentTextureUnderPlayer();
        if (currentTexture == null) return;

        FootstepData footstep = GetFootstepDataForTexture(currentTexture);
        if (footstep == null) return;

        AudioClip[] footstepSounds = isSprinting ? footstep.runningFootstepAudio : footstep.walkingFootstepAudio;
        if (footstepSounds.Length == 0) return;

        footstepAudioSource.PlayOneShot(footstepSounds[Random.Range(0, footstepSounds.Length)]);
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
            foreach (Texture tex in data.textures)
            {
                if (tex == texture) return data;
            }
        }
        return null;
    }
}