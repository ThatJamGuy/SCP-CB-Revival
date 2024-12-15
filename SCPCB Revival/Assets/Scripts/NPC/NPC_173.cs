using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPC_173 : MonoBehaviour
{
    enum SCPState { Sleeping, Idle, Chasing }
    SCPState currentState = SCPState.Sleeping;

    [Header("Settings")]
    public bool ignorePlayer;
    public float playerEscapeDistance;

    [Header("Audio")]
    [SerializeField] private AudioClip[] horrorNearSFX;
    [SerializeField] private AudioClip[] horrorFarSFX;
    [SerializeField] private AudioClip killPlayerSound;

    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private LayerMask obstructionMask;
    [SerializeField] private AudioSource horrorStingerSource;
    [SerializeField] private AudioSource movementSource;
    [SerializeField] private AudioSource killPlayerSource;
    [SerializeField] private GameObject christmasHat;
    public bool isVisible { get; private set; }

    private Renderer objectRenderer;
    private NavMeshAgent agent;
    private bool seenForTheFirstTime;
    private float distanceToPlayer;
    private bool hasPlayedNearSound = false;
    private bool hasKilledPlayer = false;

    private void Awake()
    {
        targetCamera ??= Camera.main;
        objectRenderer = GetComponent<Renderer>();
        agent = GetComponent<NavMeshAgent>();

        // Check if the month is December and enable holiday spririt!
        if (DateTime.Now.Month == 12)
        {
            christmasHat.SetActive(true);
        }
        else
        {
            christmasHat.SetActive(false);
        }
    }

    private void Update()
    {
        if(ignorePlayer) return;

        isVisible = IsObjectVisible();
        
        if (!seenForTheFirstTime && isVisible)
        {
            distanceToPlayer = Vector3.Distance(transform.position, targetCamera.transform.position);

            if (distanceToPlayer < 10f)
                horrorStingerSource.PlayOneShot(horrorNearSFX[UnityEngine.Random.Range(0, horrorNearSFX.Length)]);
            else
                horrorStingerSource.PlayOneShot(horrorFarSFX[UnityEngine.Random.Range(0, horrorFarSFX.Length)]);
            
            seenForTheFirstTime = true;
            currentState = SCPState.Idle;

            MusicPlayer.Instance.ChangeMusic(GameManager.Instance.scp173Music);

            Debug.Log("Just saw SCP-173 for the first time.");
        }

        if(currentState == SCPState.Sleeping) return;

        distanceToPlayer = Vector3.Distance(transform.position, targetCamera.transform.position);

        if(isVisible && !playerController.isBlinking)
            currentState = SCPState.Idle;
        else
            currentState = SCPState.Chasing;

        switch (currentState)
        {
            case SCPState.Sleeping:
                break;
            case SCPState.Idle:
                Idle();
                break;
            case SCPState.Chasing:
                ChasePlayer();
                break;
        }

        if (isVisible && distanceToPlayer < 5f && !hasPlayedNearSound && agent.velocity.magnitude > 0f)
        {
            horrorStingerSource.PlayOneShot(horrorNearSFX[UnityEngine.Random.Range(0, horrorNearSFX.Length)]);
            hasPlayedNearSound = true;

            StartCoroutine(ResetHorrorNearSound());
        }

        if(distanceToPlayer <= 3f && !hasKilledPlayer && !isVisible)
        {
            Debug.Log("Killed player.");
            playerController.KillPlayer();
            killPlayerSource.clip = killPlayerSound;
            killPlayerSource.Play();
            hasKilledPlayer = true;

            movementSource.enabled = false;
            currentState = SCPState.Sleeping;
        }

        if(distanceToPlayer > playerEscapeDistance)
        {
            currentState = SCPState.Sleeping;
            seenForTheFirstTime = false;
            hasPlayedNearSound = false;
            MusicPlayer.Instance.ChangeMusic(GameManager.Instance.zone1Music);
            Debug.Log("Despawning SCSP-173.");
            Destroy(gameObject);
        }
    }

    private void ChasePlayer() 
    { 
        agent.isStopped = false;
        movementSource.enabled = true;
        agent.SetDestination(playerController.transform.position);
    }

    public void Idle()
    {
        agent.isStopped = true;
        movementSource.enabled = false;
        agent.SetDestination(transform.position);
    }

    private bool IsObjectVisible()
    {
        if (objectRenderer == null || targetCamera == null) return false;

        var planes = GeometryUtility.CalculateFrustumPlanes(targetCamera);
        if (!GeometryUtility.TestPlanesAABB(planes, objectRenderer.bounds)) return false;

        Vector3 screenPoint = targetCamera.WorldToScreenPoint(objectRenderer.bounds.center);
        if (screenPoint.z <= 0) return false;

        if (Physics.Raycast(targetCamera.ScreenPointToRay(screenPoint), out RaycastHit hit, Mathf.Infinity, obstructionMask))
        {
            return hit.transform == transform;
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        if(!Application.isPlaying) return;

        Gizmos.color = isVisible ? Color.green : Color.red;
        Gizmos.DrawWireCube(objectRenderer.bounds.center, objectRenderer.bounds.size);
    }

    IEnumerator ResetHorrorNearSound()
    {
        yield return new WaitForSeconds(10f);

        if(distanceToPlayer > 10f)
            hasPlayedNearSound = false;
    }
}