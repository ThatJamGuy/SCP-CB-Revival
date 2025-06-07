using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class NPC_173 : MonoBehaviour {

    [SerializeField] private bool canRoam = true;
    [SerializeField] private float dooropenChance = 0.3f;
    [SerializeField] private AudioSource movementSource;

    private bool isVisible = false;
    private bool isChasing = false;

    private float distanceFromPlayer;

    private NavMeshAgent agent;
    private PlayerController player;
    private Camera playerCam;

    private void Start() {
        isChasing = true;

        IntitializeComponents();
        StartCoroutine(ChasePlayerStep());
        StartCoroutine(CheckForDoor());
    }

    private void IntitializeComponents() {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        playerCam = GameObject.FindWithTag("PlayerCam").GetComponent<Camera>();
    }

    private void Update() {
        // Sets movement source to enabled if the agent is moving
        if (movementSource != null) {
            movementSource.enabled = agent.velocity.magnitude > 0.1f;
        }

        // Check distance from player
        if (player != null) {
            distanceFromPlayer = Vector3.Distance(transform.position, player.transform.position);
        }

        // Test for visibility
        Vector3 viewPos = playerCam.WorldToViewportPoint(transform.position);
        if (viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1 && viewPos.z > 0) {
            Debug.Log("Object is in the camera's viewport");
            isVisible = true;
        }
        else {
            Debug.Log("Object is not in the camera's viewport");
            isVisible = false;
        }
    }

    #region Free Roam Logic
    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask) {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return navHit.position;
    }

    private IEnumerator FreeRoam() {
        while (canRoam && !isChasing) {
            if (!isVisible) {
                yield return new WaitForSeconds(1);
                Vector3 newPos = RandomNavSphere(transform.position, 10, -1);
                agent.SetDestination(newPos);
            }
        }
    }
    #endregion

    #region Chase Player Logic
    private IEnumerator ChasePlayerStep() {
        while (isChasing) {
            if (!isVisible) {
                yield return new WaitForSeconds(0.1f);
                agent.SetDestination(player.transform.position);
            }

            if(distanceFromPlayer > 10f) {
                isChasing = false;
                StartCoroutine(FreeRoam());
                Debug.Log("NPC-173 stopped chasing the player, now free roaming.");
            }
        }
    }
    #endregion

    #region Door Open Logic
    private void AttemptOpenDoor(Door closestDoor) {
        float chance = Random.Range(0f, 1f);
        if (chance < dooropenChance) {
            closestDoor.OpenDoor();
            Debug.Log("NPC-173 opened a door: " + closestDoor.name);
        }
    }

    private IEnumerator CheckForDoor() {
        while (true) { 
            yield return new WaitForSeconds(10f);
            var colliders = Physics.OverlapSphere(transform.position, 5f);
            Door closestDoor = null;
            float minDist = Mathf.Infinity;
            foreach (var col in colliders) {
                var door = col.GetComponent<Door>();
                if (door) {
                    float dist = Vector3.Distance(transform.position, col.transform.position);
                    if (dist < minDist) {
                        minDist = dist;
                        closestDoor = door;
                    }
                }
            }
            if (closestDoor) AttemptOpenDoor(closestDoor);
        }
    }
    #endregion
}