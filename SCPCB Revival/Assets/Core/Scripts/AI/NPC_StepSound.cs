using UnityEngine;

namespace scpcbr {
    public class NPC_StepSound : MonoBehaviour {
        [SerializeField] private bool enableStepSounds = true;
        [SerializeField] private bool enableDynamicStepSounds = false;
        [SerializeField] private AudioClip[] stepSounds;
        [SerializeField] private AudioSource audioSource;
        
        [Header("Dynamic Footstep System")]
        [SerializeField] private FootstepData[] footstepData;
        [SerializeField] private bool isRunning = false;

        private CharacterController characterController;

        private void Start() {
            characterController = GetComponent<CharacterController>();
        }

        public void Step() {
            if (!enableStepSounds) return;

            if (enableDynamicStepSounds && footstepData != null && footstepData.Length > 0) {
                PlayDynamicFootstep();
            }
            else if (stepSounds != null && stepSounds.Length > 0) {
                PlayBasicFootstep();
            }
        }

        private void PlayBasicFootstep() {
            int randomIndex = Random.Range(0, stepSounds.Length);
            audioSource.clip = stepSounds[randomIndex];
            audioSource.Play();
        }

        private void PlayDynamicFootstep() {
            Texture currentTexture = GetCurrentTextureUnderNPC();
            if (currentTexture == null) return;

            FootstepData footstep = GetFootstepDataForTexture(currentTexture);
            if (footstep == null) return;

            AudioClip[] footstepSounds = isRunning ? footstep.runningFootstepAudio : footstep.walkingFootstepAudio;
            if (footstepSounds.Length == 0) return;

            audioSource.PlayOneShot(footstepSounds[Random.Range(0, footstepSounds.Length)]);
        }

        private Texture GetCurrentTextureUnderNPC() {
            float rayDistance = characterController != null ? characterController.height + 1.0f : 2.0f;
            
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, rayDistance)) {
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

        public void SetRunning(bool running) {
            isRunning = running;
        }
    }
}