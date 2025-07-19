using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class NPC_RootMotionAgent : MonoBehaviour {
    private Animator animator;
    private NavMeshAgent agent;
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private float minDeltaTime = 0.0001f;

    private void Awake() {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        agent.updateRotation = false;
    }

    private void Update() {
        if (Time.timeScale < minDeltaTime) return;

        bool isWalking = IsAgentMoving();
        animator.SetBool(IsMoving, isWalking);

        if (isWalking) {
            RotateToward(agent.desiredVelocity.normalized);
        }
    }

    private void OnAnimatorMove() {
        if (Time.timeScale < minDeltaTime) return;

        Vector3 delta = animator.deltaPosition;
        float deltaTime = Mathf.Max(Time.deltaTime, minDeltaTime);

        transform.position += delta;
        agent.nextPosition = transform.position;

        if (IsAgentMoving()) {
            agent.velocity = delta / deltaTime;
        }
        else {
            agent.velocity = Vector3.zero;
        }
    }

    private void LateUpdate() {
        if (Time.timeScale < minDeltaTime) return;
        transform.rotation = animator.rootRotation;
    }

    public void WalkToPosition(Vector3 destination) {
        if (agent.enabled) agent.SetDestination(destination);
    }

    public void ToggleAgent() => agent.enabled = !agent.enabled;

    private bool IsAgentMoving() {
        return agent.enabled && !agent.pathPending && agent.remainingDistance > agent.stoppingDistance;
    }

    private void RotateToward(Vector3 direction) {
        if (direction.sqrMagnitude < 0.01f) return;
        Quaternion target = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * 10f);
    }
}