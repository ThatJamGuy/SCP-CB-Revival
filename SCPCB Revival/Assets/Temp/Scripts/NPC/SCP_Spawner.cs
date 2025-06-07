using NaughtyAttributes;
using System.Collections;
using UnityEngine;

namespace vectorarts.scpcbr {
    public class SCP_Spawner : MonoBehaviour {
        public static SCP_Spawner Instance { get; private set; }

        [Header("SCP Prefabs")]
        [SerializeField] private GameObject scp173Prefab;

        [Header("Spawn Settings")]
        [SerializeField, Tag] private string spawnLocationTag;
        [SerializeField] private float minDistance = 25f;
        [SerializeField, Range(0.1f, 1)] private float spawnChance = 0.5f;
        [SerializeField] private float relocationTime = 30f;

        public bool active173 = false;
        private GameObject[] spawnLocations;
        private Transform playerTransform;
        private GameObject current173Instance;
        private Coroutine relocationCoroutine;

        private void Awake() {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void SetPlayerTransform(Transform player) {
            playerTransform = player;
        }

        public void SpawnSCP(GameObject scp, Transform position) {
            current173Instance = Instantiate(scp, position.position, Quaternion.identity);
            active173 = true;

            if (relocationCoroutine != null)
            {
                StopCoroutine(relocationCoroutine);
            }
            relocationCoroutine = StartCoroutine(RelocationTimer());
        }

        public void CacheSpawnLocations() {
            spawnLocations = GameObject.FindGameObjectsWithTag(spawnLocationTag);
            StartCoroutine(CheckSpawnConditions());
        }

        public void TrySpawnSCP173(Transform playerTransform) {
            if (active173) return;
            if (Random.value > spawnChance) return;

            Transform closestValidLocation = GetClosestValidSpawnLocation(playerTransform);

            if (closestValidLocation != null)
            {
                SpawnSCP(scp173Prefab, closestValidLocation);
            }
        }

        private Transform GetClosestValidSpawnLocation(Transform playerTransform) {
            Transform closestLocation = null;
            float closestDistance = float.MaxValue;

            foreach (GameObject location in spawnLocations)
            {
                float distance = Vector3.Distance(playerTransform.position, location.transform.position);

                if (distance >= minDistance && distance < closestDistance)
                {
                    closestLocation = location.transform;
                    closestDistance = distance;
                }
            }

            return closestLocation;
        }

        private IEnumerator CheckSpawnConditions() {
            while (true)
            {
                yield return new WaitForSeconds(5f);
                if (playerTransform != null)
                {
                    TrySpawnSCP173(playerTransform);
                }
            }
        }

        private IEnumerator RelocationTimer() {
            yield return new WaitForSeconds(relocationTime);

            if (current173Instance != null)
            {
                SCP_173 scp173 = current173Instance.GetComponent<SCP_173>();
                if (scp173 != null && !scp173.isVisible)
                {
                    Transform newLocation = GetClosestValidSpawnLocation(playerTransform);
                    if (newLocation != null)
                    {
                        current173Instance.transform.position = newLocation.position;
                        scp173.Idle();

                        relocationCoroutine = StartCoroutine(RelocationTimer());
                        Debug.Log("SCP-173 relocated to a new spawn point.");
                    }
                }
                else
                {
                    relocationCoroutine = null;
                }
            }
        }

        public void OnSCPDestroyed() {
            active173 = false;
            current173Instance = null;
            if (relocationCoroutine != null)
            {
                StopCoroutine(relocationCoroutine);
                relocationCoroutine = null;
            }
        }
    }
}