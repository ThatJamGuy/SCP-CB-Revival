using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class SCP_2490 : MonoBehaviour  {
    private static readonly int IsChasing = Animator.StringToHash("isChasing");
    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;

    private void Start() {
        StartCoroutine(WanderRoutineA());
    }
    
    private void Update() {
        //agent.SetDestination(Player.Instance.transform.position);
    }

    private IEnumerator WanderRoutineA() {
        yield return new WaitForSeconds(5);
        agent.speed = 0.9f;
        agent.SetDestination(Player.Instance.transform.position);
        animator.SetBool(IsWalking, true);
        yield return new WaitForSeconds(15);
        Destroy(gameObject);
    }
}