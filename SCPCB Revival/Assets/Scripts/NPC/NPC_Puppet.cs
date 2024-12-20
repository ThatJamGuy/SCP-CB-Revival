using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.AI;

public class NPC_Puppet : MonoBehaviour
{
    [Header("Speech")]
    [SerializeField] private bool doesSpeech;
    [SerializeField, ShowIf(nameof(doesSpeech))] private AudioSource speechSource;

    [Header("Head IK")]
    [SerializeField] private bool useHeadIK;
    [SerializeField, ShowIf(nameof(useHeadIK))] private IK_HeadTracking headTracking;
    [SerializeField, ShowIf(nameof(useHeadIK))] private bool targetMainCamera;

    [Header("Animation")]
    [SerializeField] private bool useAnimations;
    [SerializeField, ShowIf(nameof(useAnimations))] private Animator animator;
    [SerializeField, ShowIf(nameof(useAnimations))] private bool uniqueAnimSpeeds;
    [SerializeField, ShowIf(nameof(uniqueAnimSpeeds))] private float minAnimSpeed = 0.5f, maxAnimSpeed = 1;

    [Header("Movement")]
    [SerializeField] private bool nodeMovement;
    [SerializeField, ShowIf(nameof(nodeMovement))] private Transform[] nodes;

    private NavMeshAgent navMeshAgent;
    private IK_PointOfInterest mainCameraPointOfInterest;

    private void Start()
    {
        if(useHeadIK && targetMainCamera) {
            mainCameraPointOfInterest = Camera.main.GetComponent<IK_PointOfInterest>();
            if (mainCameraPointOfInterest != null) {
                headTracking.POIs.Add(mainCameraPointOfInterest);
            }
        }

        if (nodeMovement)
            navMeshAgent = GetComponent<NavMeshAgent>();

        if(useAnimations) animator.speed = uniqueAnimSpeeds ? Random.Range(minAnimSpeed, maxAnimSpeed) : 1f;
    }

    private void Update()
    {
        if (useAnimations && !uniqueAnimSpeeds)
            animator.speed = 1f;
        
        if (useHeadIK && headTracking)
            headTracking.enabled = useHeadIK;

        if(!targetMainCamera && mainCameraPointOfInterest != null && headTracking.POIs.Contains(mainCameraPointOfInterest)) {
            headTracking.POIs.Remove(mainCameraPointOfInterest);
        }
        if (targetMainCamera && mainCameraPointOfInterest != null && !headTracking.POIs.Contains(mainCameraPointOfInterest)) {
            headTracking.POIs.Add(mainCameraPointOfInterest);
        }
    }

    public void PlayAnimation(string animationName)
    {
        animator.Play(animationName);
    }

    public void MoveToNodes()
    {
        if (nodes.Length == 0 || !navMeshAgent)
            return;

        StartCoroutine(MoveThroughNodes());
    }

    public void Say(AudioClip clip)
    {
        if (!doesSpeech || !speechSource)
        {
            Debug.LogWarning($"{gameObject.name} is not set to use speech or missing a speech source.");
            return;
        }

        speechSource.clip = clip;
        speechSource.Play();
    }

    private IEnumerator MoveThroughNodes()
    {
        foreach (var node in nodes)
        {
            navMeshAgent.SetDestination(node.position);

            while (navMeshAgent.pathPending || navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
                yield return null;
        }

        navMeshAgent.isStopped = true;
    }
}