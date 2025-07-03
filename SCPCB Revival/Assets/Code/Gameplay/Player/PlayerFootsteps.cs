using UnityEngine;

public class PlayerFootsteps : MonoBehaviour {
    [Header("Referencess")]
    [SerializeField] private PlayerBase playerBase;
    [SerializeField] private FootstepData[] footstepData;

    private CharacterController characterController;
    private AudioSource footstepAudioSource;

    private bool isSprinting;
    private bool isCrouching;
    private bool isMoving;

    #region Default Methods
    private void Start() {
        characterController = playerBase.GetComponent<CharacterController>();
        footstepAudioSource = GetComponent<AudioSource>();
    }
    #endregion

    #region Public Methods
    public void UpdateFootsteps() {
        isMoving = playerBase.isMoving;
        isSprinting = playerBase.isSprinting;
        isCrouching = playerBase.isCrouching;
        if (!isMoving) return;

        if (isCrouching)
            footstepAudioSource.volume = 0.1f;
        else 
            footstepAudioSource.volume = 0.5f;
    }

    public void PlayFootstepAudio() {
        Texture currentTexture = GetCurrentTextureUnderPlayer();
        if (currentTexture == null) return;

        FootstepData footstep = GetFootstepDataForTexture(currentTexture);
        if (footstep == null) return;

        AudioClip[] footstepSounds = isSprinting ? footstep.runningFootstepAudio : footstep.walkingFootstepAudio;
        if (footstepSounds.Length == 0) return;

        footstepAudioSource.PlayOneShot(footstepSounds[Random.Range(0, footstepSounds.Length)]);
    }
    #endregion

    #region Private Methods
    private Texture GetCurrentTextureUnderPlayer() {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, characterController.height + 1.0f)) {
            MeshCollider meshCollider = hit.collider as MeshCollider;
            if (meshCollider != null && meshCollider.sharedMesh != null) {
                int submeshIndex = GetSubmeshIndex(meshCollider.sharedMesh, hit.triangleIndex);
                if (submeshIndex != -1) {
                    Material material = meshCollider.GetComponent<Renderer>()?.materials[submeshIndex];
                    return material?.mainTexture;
                }
            }
        }
        return null;
    }

    private int GetSubmeshIndex(Mesh mesh, int triangleIndex) {
        int triangleCount = 0;
        for (int i = 0; i < mesh.subMeshCount; i++) {
            int subMeshTriangleCount = mesh.GetTriangles(i).Length / 3;
            if (triangleIndex < triangleCount + subMeshTriangleCount) return i;
            triangleCount += subMeshTriangleCount;
        }
        return -1;
    }

    private FootstepData GetFootstepDataForTexture(Texture texture) {
        foreach (FootstepData data in footstepData) {
            foreach (Texture tex in data.textures) {
                if (tex == texture) return data;
            }
        }
        return null;
    }
    #endregion
}