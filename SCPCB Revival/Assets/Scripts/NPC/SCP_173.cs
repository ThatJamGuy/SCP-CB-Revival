using UnityEngine;
using UnityEngine.AI;

public class SCP_173 : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField, Range(1, 3)] private int difficulty = 1;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float stoppingDistance = 1f;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private AudioSource moveSource;

    // Private variables
    private bool isVisible;
    private bool hiddenBehindObstacle;
    private bool isMoving;
    private bool alreadySpotted = false;

    // Private references
    private Renderer objectRenderer;
    private NavMeshAgent agent;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component is missing.");
            enabled = false;
            return;
        }

        moveSpeed *= difficulty;
        agent.speed = moveSpeed;
        agent.stoppingDistance = stoppingDistance;
    }

    void Update()
    {
        HandleVisibilityChecks();
    }

    // This seems to cause the most problems atm D:
    // The player is occasionaly able to detect the SCP-173 even when it's hidden behind an wall (Most notable in spawn room when looking in his direction)
    // Also sometimes will move anyway even when seen by the player, but really at longer distances as if he's trying to catch up to the players radius?
    private void HandleVisibilityChecks()
    {
        CheckObstacleVisibility();
        isVisible = IsObjectVisible();
        moveSource.enabled = isMoving;

        if (!alreadySpotted && isVisible)
        {
            alreadySpotted = true;
            Debug.Log("Player has spotted SCP-173 for the first time.");
        }

        if (alreadySpotted)
        {
            if (!isVisible || hiddenBehindObstacle || player.gameObject.GetComponent<PlayerController>().isBlinking)
            {
                agent.isStopped = false;
                isMoving = true;
                MoveTowardsPlayer();
            }
            else
            {
                agent.ResetPath();
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                isMoving = false;
            }
        }
    }

    private bool IsObjectVisible()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        return GeometryUtility.TestPlanesAABB(planes, objectRenderer.bounds);
    }

    private void CheckObstacleVisibility()
    {
        hiddenBehindObstacle = true;
        Vector3 center = objectRenderer.bounds.center;
        Vector3 extents = objectRenderer.bounds.extents;

        Vector3[] points = {
            center,
            center + new Vector3(extents.x, extents.y, 0),
            center + new Vector3(-extents.x, extents.y, 0),
            center + new Vector3(extents.x, -extents.y, 0),
            center + new Vector3(-extents.x, -extents.y, 0),
            center + new Vector3(0, extents.y, extents.z),
            center + new Vector3(0, -extents.y, extents.z)
        };

        Transform camTransform = Camera.main.transform;
        Vector3 camPos = camTransform.position;

        foreach (Vector3 point in points)
        {
            Vector3 direction = point - camPos;
            if (Physics.Raycast(camPos, direction, out RaycastHit hit, direction.magnitude, obstacleMask))
            {
                if (hit.transform == transform)
                {
                    hiddenBehindObstacle = false;
                    break;
                }
            }
            else
            {
                hiddenBehindObstacle = false;
                break;
            }
        }
    }

    private void MoveTowardsPlayer()
    {
        agent.SetDestination(player.position);
        Vector3 relativePos = player.position - transform.position;
        relativePos.y = 0;
        Quaternion lookRotation = Quaternion.LookRotation(relativePos, Vector3.up);
        transform.rotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0);
    }
}
