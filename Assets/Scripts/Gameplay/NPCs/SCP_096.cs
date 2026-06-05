using System;
using UnityEngine;
using UnityEngine.AI;

public class SCP_096 : MonoBehaviour, IRoamingSCP {
    public enum State { Puppet, Roaming, Raging, Chasing }
    public State currentState = State.Roaming;
    
    [Header("Navigation Settings")]
    [SerializeField] private float roamRadius = 15f;
    [SerializeField] private float minIdlePauseTime = 5f;
    [SerializeField] private float maxIdlePauseTime = 10f;
    [SerializeField] private float reachedDestinationThreshold = 0.5f;
    
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    
    private static readonly int WalkingHash = Animator.StringToHash("walking");
    
    private float idleWaitTimer;
    private bool isCurrentlyIdling;
    
    #region Unity Callbacks

    private void Start() {
        EntitySystem.Instance.RegisterEntity(this);
    }

    private void Update() {
        switch (currentState) {
            case State.Puppet:
                break;
            case State.Roaming:
                PerformRoamingActivities();
                break;
            case State.Raging:
                // Raging
                break;
            case State.Chasing:
                // Chasing
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    #endregion
    
    #region Private Methods

    private void PerformRoamingActivities() {
        if (isCurrentlyIdling) {
            idleWaitTimer -= Time.deltaTime;
            
            if (idleWaitTimer <= 0f) {
                if (!TryGetNavMeshPoint(out Vector3 destination))
                    return;

                isCurrentlyIdling = false;
                WalkTo(destination);
            }

            return;
        }
        
        // Check if destination is reached then begin idle wait
        if (!agent.pathPending && agent.remainingDistance <= reachedDestinationThreshold)
            BeginIdling();
        
        animator.SetBool(WalkingHash, !isCurrentlyIdling && agent.velocity.sqrMagnitude > 0.01f);
    }
    
    private void BeginIdling() {
        isCurrentlyIdling = true;
        idleWaitTimer = UnityEngine.Random.Range(minIdlePauseTime, maxIdlePauseTime);
        animator.SetBool(WalkingHash, false);
    }
    
    private bool TryGetNavMeshPoint(out Vector3 result) {
        // Sample random points until a valid NavMesh position is found
        for (var i = 0; i < 10; i++) {
            var randomPoint = transform.position + UnityEngine.Random.insideUnitSphere * roamRadius;
            randomPoint.y = transform.position.y;
 
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, roamRadius, NavMesh.AllAreas)) {
                result = hit.position;
                return true;
            }
        }
 
        result = transform.position;
        return false;
    }
    #endregion
    
    #region Public Methods

    public void WalkTo(Vector3 position) {
        agent.SetDestination(position);
    }
    
    public void Teleport(Vector3 position) {
        gameObject.transform.position = position;
        agent.Warp(position);
    }
    #endregion
}