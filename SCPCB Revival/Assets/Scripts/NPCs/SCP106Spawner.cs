using System.Collections;
using UnityEngine;

namespace scpcbr {
    public class SCP106Spawner : MonoBehaviour {
        [Header("Prefabs & References")]
        public GameObject scp106Prefab;
        public GameObject spawnEffectPrefab;

        [Header("Spawn Settings")]
        public float spawnInterval = 120f;
        public float effectDuration = 5f;
        public float spawnRadius = 10f;
        public float maxSpawnHeight = 20f;
        public float forwardOffset = 1.1f;

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

            Vector3 scp106Pos = spawnPos + (Vector3.forward * forwardOffset);
            Instantiate(scp106Prefab, scp106Pos, Quaternion.identity);

            GameManager.instance.scp106Active = true;
        }

        private bool TryGetGroundPosition(out Vector3 groundPos) {
            groundPos = player.position;

            for (int i = 0; i < 10; i++) {
                Vector3 randomOffset = Random.insideUnitCircle * spawnRadius;
                Vector3 candidate = player.position + new Vector3(randomOffset.x, 0, randomOffset.y);
                Vector3 rayStart = candidate + Vector3.up * maxSpawnHeight;

                if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, maxSpawnHeight + groundCheckDistance, groundMask)) {
                    groundPos = hit.point;
                    return true;
                }
            }

            return false;
        }

        private void OnDestroy() {
            if (currentEffect != null) {
                Destroy(currentEffect);
            }
        }
    }
}