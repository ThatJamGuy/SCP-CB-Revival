using UnityEngine;
using UnityEngine.AI;

public class NPC_Locomotion : MonoBehaviour {
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private Animator animator;

    private static readonly int IsWalking = Animator.StringToHash("isWalking");

    private void Start() {
        if (navMeshAgent == null) return;
        
        navMeshAgent.updatePosition = false;
        navMeshAgent.updateRotation = false;
    }

    private void Update() {
        if (!navMeshAgent || !animator) return;

        var isMoving = navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance;

        animator.SetBool(IsWalking, isMoving);

        if (!isMoving) return;
        
        var direction = navMeshAgent.desiredVelocity.normalized;

        if (direction.sqrMagnitude > 0.01f) {
            var targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        navMeshAgent.nextPosition = transform.position;
    }

    private void LateUpdate() {
        var targetRotation = animator.rootRotation;
        navMeshAgent.transform.rotation = targetRotation;
    }

    private void OnAnimatorMove() {
        if (navMeshAgent == null || !navMeshAgent.enabled)
            return;

        Vector3 rootMotion = animator.deltaPosition;

        transform.position += new Vector3(rootMotion.x, 0f, rootMotion.z);

        var pos = transform.position;
        pos.y = navMeshAgent.nextPosition.y;

        transform.position = pos;
        navMeshAgent.nextPosition = pos;
    }

    // Tells the AI to relocate to a new position taking in Transform
    public void WalkToPosition(Transform position) {
        if (!navMeshAgent || !animator) return;

        navMeshAgent.SetDestination(position.position);
    }
    
    // Tells the AI to relocate to a new position taking in a Vector3 instead
    public void WalkToPosition(Vector3 position) {
        if (!navMeshAgent || !animator) return;

        navMeshAgent.SetDestination(position);
    }

    // Warps the NPC to a given location 
    public void Warp(Vector3 position) {
        gameObject.transform.position = position;
        navMeshAgent.Warp(position);
    }
    
    public void ToggleAgent() => navMeshAgent.enabled = !navMeshAgent.enabled;
}