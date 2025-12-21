using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class PlayerFootsteps : MonoBehaviour {
    [SerializeField] private FootstepData[] footstepData;
    [SerializeField] private PlayerAccessor playerAccessor;
    [SerializeField] private CharacterController characterController;

    private VCA footstepVCA;
    private PlayerMovement playerMovement;

    private bool isSprinting;
    private bool isCrouching;
    private bool isMoving;

    #region Unity Methods
    private void Awake() {
        footstepVCA = RuntimeManager.GetVCA("vca:/FootstepVCA");
        playerMovement = GetComponentInParent<PlayerMovement>();
    }

    private void Update() {
        UpdateFootsteps();
    }
    #endregion

    #region Public Methods
    public void UpdateFootsteps() {
        isMoving = playerAccessor.isMoving;
        isSprinting = playerMovement != null ? playerMovement.IsActuallySprinting : false;
        isCrouching = playerAccessor.isCrouching;
        if (!isMoving) return;

        if (isCrouching)
            footstepVCA.setVolume(0.3f);
        else
            footstepVCA.setVolume(1.0f);
    }

    public void PlayFootstepAudio() {
        Texture currentTexture = GetCurrentTextureUnderPlayer();
        if (currentTexture == null) return;

        FootstepData footstep = GetFootstepDataForTexture(currentTexture);
        if (footstep == null) return;

        EventReference eventRef = isSprinting ? footstep.assocatedRunEvent : footstep.assocatedWalkEvent;
        if (eventRef.IsNull) return;

        AudioManager.instance.PlaySound(eventRef, transform.position);
    }
    #endregion

    #region Private Methods
    private Texture GetCurrentTextureUnderPlayer() {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, characterController.height + 1.0f)) {
            if (hit.triangleIndex == -1) return null;

            MeshCollider meshCollider = hit.collider as MeshCollider;
            if (meshCollider != null && meshCollider.sharedMesh != null) {
                int submeshIndex = GetSubmeshIndex(meshCollider.sharedMesh, hit.triangleIndex);

                if (submeshIndex != -1) {
                    Renderer renderer = meshCollider.GetComponent<Renderer>();
                    if (renderer != null) {
                        Material[] materials = renderer.sharedMaterials;
                        if (submeshIndex < materials.Length) {
                            return materials[submeshIndex]?.mainTexture;
                        }
                    }
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