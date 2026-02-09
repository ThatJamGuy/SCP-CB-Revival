using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EntitySystem : MonoBehaviour {
    public static EntitySystem instance;
    public enum EntityType { SCP173, SCP106 };

    [Header("SCP-173 Settings")]
    [SerializeField] private GameObject scp173Prefab;
    [SerializeField] private float scp173RelocateDelay = 10f;
    [SerializeField] private float scp173MinSpawnDistance = 20f;
    [SerializeField] private float scp173NavRadius = 1f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundCheckDistance = 50f;

    #region Unity Callbacks
    private void Awake() {
        if (instance == null) { 
            instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    private void Start() {
        StartCoroutine(SCP173SpawnRoutine());
    }
    #endregion

    #region Public Methods
    public void MoveEntity(EntityType entityType, Transform newPos) {
        switch (entityType) { 
            case EntityType.SCP173:
                SCP_173 active173 = FindFirstObjectByType<SCP_173>();

                active173.transform.position = newPos.transform.position;
                active173.GetComponent<NavMeshAgent>().Warp(newPos.transform.position);
                StartCoroutine(SleepAfterTeleport(active173.GetComponent<NavMeshAgent>()));
                break;
            case EntityType.SCP106:
                break;
            default:
                break;
        }
    }

    private IEnumerator SleepAfterTeleport(NavMeshAgent agentToStop) {
        agentToStop.isStopped = true;
        yield return new WaitForSeconds(0.7f);
        agentToStop.isStopped = false;

    }
    #endregion

    #region Coroutines
    private IEnumerator SCP173SpawnRoutine() {
        while (true) {
            yield return new WaitForSeconds(scp173RelocateDelay);
            if (PlayerAccessor.instance != null && !PlayerAccessor.instance.isDead && !GameManager.instance.scp173ChasingPlayer) {
                if (TryGetValidSpawnPosition(out Vector3 spawnPos)) {
                    SCP_173 existing173 = FindFirstObjectByType<SCP_173>();

                    if (existing173 == null) {
                        Instantiate(scp173Prefab, spawnPos, Quaternion.identity);
                        //scp173Active = true;
                    }
                    else {
                        existing173.transform.position = spawnPos;
                        NavMeshAgent agent = existing173.GetComponent<NavMeshAgent>();
                        if (agent != null) {
                            agent.Warp(spawnPos);
                        }
                    }
                }
                else {
                    Debug.Log("<color=yellow>[EntitySystem]:</color> No valid On-Nav position found for SCP-173 during routine spawn check.");
                }
            }
        }
    }
    #endregion

    #region Helpers :)
    private bool TryGetValidSpawnPosition(out Vector3 spawnPos) {
        spawnPos = Vector3.zero;

        for (int i = 0; i < 20; i++) {
            Vector2 randomCircle = Random.insideUnitCircle.normalized;
            float distance = Random.Range(scp173MinSpawnDistance, 40);
            Vector3 offset = new Vector3(randomCircle.x, 0, randomCircle.y) * distance;
            Vector3 candidate = PlayerAccessor.instance.transform.position + offset;
            Vector3 rayStart = candidate + Vector3.up * 1;

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 1 + groundCheckDistance, groundMask)) {
                if (IsNavMeshRadiusValid(hit.point, scp173NavRadius)) {
                    spawnPos = hit.point;
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
    #endregion
}