using UnityEngine;
using UnityEngine.AI;
using NaughtyAttributes;

public class NPC_Locomotion : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    private Animator animator;

    private static readonly int IsWalking = Animator.StringToHash("isWalking");

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (navMeshAgent != null)
        {
            navMeshAgent.updatePosition = false;
            navMeshAgent.updateRotation = false;
        }
    }

    private void Update()
    {
        if (navMeshAgent == null || animator == null) return;

        bool isMoving = navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance;

        animator.SetBool(IsWalking, isMoving);

        if (isMoving)
        {
            Vector3 direction = navMeshAgent.desiredVelocity.normalized;

            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }

            navMeshAgent.nextPosition = transform.position;
        }
    }

    private void LateUpdate()
    {
        Quaternion targetRotation = animator.rootRotation;
        navMeshAgent.transform.rotation = targetRotation;
    }

    private void OnAnimatorMove()
    {
        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            transform.position += animator.deltaPosition;
        }
    }

    public void WalkToPosition(Transform position)
    {
        if (navMeshAgent == null || animator == null) return;

        navMeshAgent.SetDestination(position.position);
    }
    public void ToggleAgent() => navMeshAgent.enabled = !navMeshAgent.enabled;
}