using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class NPC_173 : MonoBehaviour {
    [SerializeField] private bool canRoam = true;
    [SerializeField] private float dooropenChance = 0.3f;
    [SerializeField] private AudioSource movementSource;

    private bool isVisible, isChasing = true;
    private float distanceFromPlayer;
    private NavMeshAgent agent;
    private PlayerController player;
    private Camera playerCam;

    private void Start() {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        playerCam = GameObject.FindWithTag("PlayerCam").GetComponent<Camera>();
        StartCoroutine(ChasePlayerStep());
        StartCoroutine(CheckForDoor());
    }

    private void Update() {
        if (movementSource) movementSource.enabled = agent.velocity.magnitude > 0.1f;
        if (player) distanceFromPlayer = Vector3.Distance(transform.position, player.transform.position);
    }

    private IEnumerator ChasePlayerStep() {
        while (isChasing) {
            if (!isVisible) {
                yield return new WaitForSeconds(0.1f);
                agent.SetDestination(player.transform.position);
            }
            if (distanceFromPlayer > 10f) {
                isChasing = false;
                StartCoroutine(FreeRoam());
                Debug.Log("NPC-173 stopped chasing the player, now free roaming.");
            }
        }
    }

    private IEnumerator FreeRoam() {
        while (canRoam && !isChasing) {
            if (!isVisible) {
                yield return new WaitForSeconds(1f);
                agent.SetDestination(RandomNavSphere(transform.position, 10f, -1));
            }
        }
    }

    private static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask) {
        Vector3 randDirection = Random.insideUnitSphere * dist + origin;
        NavMesh.SamplePosition(randDirection, out var navHit, dist, layermask);
        return navHit.position;
    }

    private IEnumerator CheckForDoor() {
        while (true) {
            yield return new WaitForSeconds(10f);
            var colliders = Physics.OverlapSphere(transform.position, 5f);
            Door closestDoor = null;
            float minDist = Mathf.Infinity;
            foreach (var col in colliders) {
                if (col.TryGetComponent(out Door door)) {
                    float dist = Vector3.Distance(transform.position, col.transform.position);
                    if (dist < minDist) {
                        minDist = dist;
                        closestDoor = door;
                    }
                }
            }
            if (closestDoor && Random.value < dooropenChance) {
                closestDoor.OpenDoor();
                Debug.Log($"NPC-173 opened a door: {closestDoor.name}");
            }
        }
    }
}