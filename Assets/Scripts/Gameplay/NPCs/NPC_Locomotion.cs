using UnityEngine;
using UnityEngine.AI;

public class NPC_Locomotion : MonoBehaviour {
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private Animator animator;

    private static readonly int IsWalking = Animator.StringToHash("walking_rmotion");

    private Transform cachedTransform;
    private Transform agentTransform;
    private bool isNavMoving;
    private bool scriptedRootMotion;

    /// <summary>Allows animation root motion to move the NPC without a nav path (e.g. scripted walks, turns, steps).</summary>
    public void SetScriptedRootMotion(bool enabled) => scriptedRootMotion = enabled;

    private void Awake() {
        cachedTransform = transform;
        if (navMeshAgent != null) agentTransform = navMeshAgent.transform;
    }

    private void Start() {
        if (navMeshAgent == null) return;

        navMeshAgent.updatePosition = false;
        navMeshAgent.updateRotation = false;
    }

    private void Update() {
        if (!navMeshAgent || !animator) return;

        isNavMoving = navMeshAgent.enabled
            && !navMeshAgent.pathPending
            && navMeshAgent.hasPath
            && navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance;

        animator.SetBool(IsWalking, isNavMoving);

        if (!isNavMoving) return;

        Vector3 direction = navMeshAgent.desiredVelocity;

        // sqrMagnitude check on the raw vector avoids the normalize sqrt
        if (direction.sqrMagnitude > 0.01f) {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            cachedTransform.rotation = Quaternion.Slerp(cachedTransform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        navMeshAgent.nextPosition = cachedTransform.position;
    }

    private void LateUpdate() {
        if (!animator || agentTransform == null) return;

        agentTransform.rotation = animator.rootRotation;
    }

    private void OnAnimatorMove() {
        // Gate: only the nav path or an explicit script opt-in may apply root motion
        if (!isNavMoving && !scriptedRootMotion) return;

        Vector3 delta = animator.deltaPosition;
        Vector3 pos = cachedTransform.position;
        pos.x += delta.x;
        pos.z += delta.z;

        if (navMeshAgent != null && navMeshAgent.enabled) {
            pos.y = navMeshAgent.nextPosition.y;
            cachedTransform.position = pos;
            navMeshAgent.nextPosition = pos;
        } else {
            // Agent toggled off (ToggleAgent): still honor scripted root motion
            cachedTransform.position = pos;
        }
    }
}