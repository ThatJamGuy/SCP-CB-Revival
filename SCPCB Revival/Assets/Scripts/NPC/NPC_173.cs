using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[RequireComponent(typeof(NavMeshAgent))]
public class NPC_173 : MonoBehaviour
{
    enum SCPState { Sleeping, Idle, Chasing }
    SCPState currentState = SCPState.Sleeping;

    [Header("Settings")] 
    public bool ignorePlayer; public bool scriptedMode; public float playerEscapeDistance, minVignetteIntensity, maxVignetteIntensity, vignetteRange;

    [Header("Audio")]
    [SerializeField] private AudioClip[] horrorNearSFX;
    [SerializeField] private AudioClip[] horrorFarSFX;
    [SerializeField] private AudioClip killPlayerSound;

    [Header("References")]
    [SerializeField] private LayerMask obstructionMask;
    [SerializeField] private AudioSource horrorStingerSource, movementSource, killPlayerSource, tensionSource;
    [SerializeField] private GameObject christmasHat;
    public bool isVisible { get; private set; }

    private PlayerController playerController;
    private Renderer objectRenderer;
    private NavMeshAgent agent;
    private Volume skyAndFogVolume;
    private Camera targetCamera;
    private bool seenForTheFirstTime;
    private float distanceToPlayer;
    private bool hasPlayedNearSound = false;
    private bool hasKilledPlayer = false;

    private void Awake()
    {
        playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        targetCamera ??= Camera.main;
        objectRenderer = GetComponent<Renderer>();
        agent = GetComponent<NavMeshAgent>();

        skyAndFogVolume = GameObject.Find("Sky and Fog Volume").GetComponent<Volume>();

        // Check if the month is December and enable holiday spririt!
        christmasHat.SetActive(DateTime.Now.Month == 12);
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

            if(!scriptedMode)
                MusicPlayer.Instance.ChangeMusic(GameManager.Instance.scp173Music);

            Debug.Log("Just saw SCP-173 for the first time.");
        }

        if(currentState == SCPState.Sleeping || scriptedMode) return;

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

        if (isVisible && distanceToPlayer < 5f && !hasPlayedNearSound && agent.velocity.magnitude > 0f && isVisible)
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

        if (isVisible && !playerController.isBlinking)
        {
            if (skyAndFogVolume.profile.TryGet(out Vignette vignette))
            {
                float vignetteIntensity = Mathf.Lerp(maxVignetteIntensity, minVignetteIntensity, distanceToPlayer / vignetteRange);
                vignette.intensity.value = vignetteIntensity;
            }
        }

        tensionSource.enabled = isVisible && distanceToPlayer < vignetteRange;
        tensionSource.volume = Mathf.Lerp(minVignetteIntensity, maxVignetteIntensity, 1 - (distanceToPlayer / vignetteRange));

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
        agent.speed = 350;
        agent.isStopped = false;
        movementSource.enabled = true;
        agent.SetDestination(playerController.transform.position);
    }

    public void Idle()
    {
        agent.speed = 0;
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