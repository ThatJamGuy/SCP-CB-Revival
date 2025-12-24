using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace scpcbr {
    public class SCP106Spawner : MonoBehaviour {
        [Header("Prefabs & References")]
        public GameObject scp106Prefab;
        public GameObject spawnEffectPrefab;

        [Header("Spawn Settings")]
        public float spawnInterval = 120f;
        public float effectDuration = 10f;
        public float spawnRadius = 3f;
        public float maxSpawnHeight = 1f;
        public float navMeshCheckRadius = 5f;

        [Header("Ground Detection")]
        public LayerMask groundMask;
        public float groundCheckDistance = 50f;

        private Transform player;
        private GameObject currentEffect;

        private void Start() {
            if (!FindPlayer()) return;
            StartCoroutine(SpawnRoutine());
        }

        private bool FindPlayer() {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) {
                player = playerObj.transform;
                return true;
            }
            Debug.LogError("SCP106Spawner: Player not found");
            return false;
        }

        private IEnumerator SpawnRoutine() {
            while (true) {
                yield return new WaitForSeconds(spawnInterval);
                if (player != null && !GameManager.instance.scp106Active && !PlayerAccessor.instance.isDead) {
                    yield return StartCoroutine(SpawnSequence());
                }
            }
        }

        private IEnumerator SpawnSequence() {
            if (!TryGetGroundPosition(out Vector3 spawnPos)) {
                Debug.LogWarning("SCP106Spawner: No valid ground position found");
                yield break;
            }

            currentEffect = Instantiate(spawnEffectPrefab, spawnPos, Quaternion.identity);
            yield return new WaitForSeconds(effectDuration);

            Vector3 scp106Pos = spawnPos;
            Instantiate(scp106Prefab, scp106Pos, PlayerAccessor.instance.transform.rotation);

            GameManager.instance.scp106Active = true;
        }

        private bool TryGetGroundPosition(out Vector3 groundPos) {
            groundPos = player.position;

            for (int i = 0; i < 10; i++) {
                Vector3 randomOffset = Random.insideUnitCircle * spawnRadius;
                Vector3 candidate = player.position + new Vector3(randomOffset.x, 0, randomOffset.y);
                Vector3 rayStart = candidate + Vector3.up * maxSpawnHeight;

                if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, maxSpawnHeight + groundCheckDistance, groundMask)) {
                    if (IsNavMeshRadiusValid(hit.point, navMeshCheckRadius)) {
                        groundPos = hit.point;
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsNavMeshRadiusValid(Vector3 position, float radius) {
            int samples = 8;
            float angleStep = 360f / samples;

            for (int i = 0; i < samples; i++) {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 checkPos = position + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);

                if (!NavMesh.SamplePosition(checkPos, out NavMeshHit hit, 0.5f, NavMesh.AllAreas)) {
                    return false;
                }
            }

            return NavMesh.SamplePosition(position, out NavMeshHit centerHit, 0.5f, NavMesh.AllAreas);
        }

        private void OnDestroy() {
            if (currentEffect != null) {
                Destroy(currentEffect);
            }
        }
    }
}