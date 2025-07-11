using System.Collections;
using UnityEngine;

namespace scpcbr {
    public class SCP106Spawner : MonoBehaviour {
        [Header("Prefabs & References")]
        public GameObject scp106Prefab;
        public GameObject spawnEffectPrefab;
        public float spawnInterval = 120f;
        public float effectDuration = 5f;
        public float spawnRadius = 10f;
        public LayerMask groundMask;

        private Transform player;
        private void Start() {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
            else
                Debug.LogError("Player not found for SCP106Spawner.");

            StartCoroutine(SpawnRoutine());
        }

        private IEnumerator SpawnRoutine() {
            yield return new WaitForSeconds(spawnInterval);
            if (player != null)
                yield return StartCoroutine(SpawnSequence());
        }

        private IEnumerator SpawnSequence() {
            float forwardOffset = 1.1f;
            Vector3 spawnPos = GetGroundPositionNearPlayer();
            Vector3 offsetSpawnPos = spawnPos + (Vector3.forward * forwardOffset);
            GameObject effect = Instantiate(spawnEffectPrefab, spawnPos, Quaternion.identity);
            yield return new WaitForSeconds(effectDuration);
            Instantiate(scp106Prefab, offsetSpawnPos, Quaternion.identity);
        }

        private Vector3 GetGroundPositionNearPlayer() {
            Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
            randomOffset.y = 0;
            Vector3 candidate = player.position + randomOffset;
            RaycastHit hit;
            if (Physics.Raycast(candidate + Vector3.up * 10f, Vector3.down, out hit, 20f, groundMask))
                return hit.point;
            return player.position;
        }
    }
}