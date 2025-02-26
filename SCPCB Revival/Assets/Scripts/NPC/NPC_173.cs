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
    public bool ignorePlayer; 
    public bool scriptedMode; 
    public float playerEscapeDistance, minVignetteIntensity, maxVignetteIntensity, vignetteRange;

    [Header("Audio")]
    [SerializeField] private AudioClip[] horrorNearSFX;
    [SerializeField] private AudioClip[] horrorFarSFX;
    [SerializeField] private AudioClip killPlayerSound;

    [Header("References")]
    [SerializeField] private AudioSource horrorStingerSource, movementSource, killPlayerSource, tensionSource;
    [SerializeField] private GameObject christmasHat;

    private PlayerController playerController;
    private Renderer objectRenderer;
    private NavMeshAgent agent;
    private Volume skyAndFogVolume;
    private Vignette vignette;
    private Camera targetCamera;
    private bool seenForTheFirstTime;
    private float distanceToPlayer;
    private bool hasPlayedNearSound = false;
    private bool hasKilledPlayer = false;

    private void Awake()
    {
        playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        targetCamera = Camera.main;
        agent = GetComponent<NavMeshAgent>();
        objectRenderer = GetComponent<Renderer>();
        skyAndFogVolume = GameObject.Find("Sky and Fog Volume").GetComponent<Volume>();

        if (skyAndFogVolume.profile.TryGet(out Vignette v))
            vignette = v;

        christmasHat.SetActive(DateTime.Now.Month == 12);
    }

    private void Update()
    {
        if (ignorePlayer) return;

        distanceToPlayer = Vector3.Distance(transform.position, targetCamera.transform.position);
        bool isVisible = IsPlayerLookingAtNPC();

        HandleFirstEncounter(isVisible);
        HandleStateTransitions(isVisible);
        HandleStateBehavior();
        HandleAudioAndEffects(isVisible);
        HandlePlayerEscape();
    }

    private void HandleFirstEncounter(bool isVisible)
    {
        if (!seenForTheFirstTime && isVisible)
        {
            PlayFirstEncounterSound();
            seenForTheFirstTime = true;
            currentState = SCPState.Idle;

            if (!scriptedMode)
                MusicPlayer.Instance.ChangeMusic(GameManager.Instance.scp173Music);

            Debug.Log("Just saw SCP-173 for the first time.");
        }
    }

    private void HandleStateTransitions(bool isVisible)
    {
        if (currentState == SCPState.Sleeping || scriptedMode) return;

        currentState = (isVisible && !playerController.isBlinking) ? SCPState.Idle : SCPState.Chasing;
    }

    private void HandleStateBehavior()
    {
        switch (currentState)
        {
            case SCPState.Sleeping: break;
            case SCPState.Idle: Idle(); break;
            case SCPState.Chasing: ChasePlayer(); break;
        }
    }

    private void HandleAudioAndEffects(bool isVisible)
    {
        if (isVisible && distanceToPlayer < 5f && !hasPlayedNearSound && agent.velocity.magnitude > 0f)
        {
            PlayHorrorNearSound();
            StartCoroutine(ResetHorrorNearSound());
        }

        if (distanceToPlayer <= 3f && !hasKilledPlayer && !isVisible)
            KillPlayer();

        if (isVisible && !playerController.isBlinking)
            UpdateVignetteEffect();

        tensionSource.enabled = isVisible && distanceToPlayer < vignetteRange;
        tensionSource.volume = Mathf.Lerp(minVignetteIntensity, maxVignetteIntensity, 1 - (distanceToPlayer / vignetteRange));
    }

    private void HandlePlayerEscape()
    {
        if (distanceToPlayer > playerEscapeDistance)
            DespawnNPC();
    }

    private void PlayFirstEncounterSound()
    {
        AudioClip clip = (distanceToPlayer < 10f) ? horrorNearSFX[UnityEngine.Random.Range(0, horrorNearSFX.Length)] : horrorFarSFX[UnityEngine.Random.Range(0, horrorFarSFX.Length)];
        horrorStingerSource.PlayOneShot(clip);
    }

    private void PlayHorrorNearSound()
    {
        horrorStingerSource.PlayOneShot(horrorNearSFX[UnityEngine.Random.Range(0, horrorNearSFX.Length)]);
        hasPlayedNearSound = true;
    }

    private void KillPlayer()
    {
        Debug.Log("Killed player.");
        //playerController.KillPlayer();
        killPlayerSource.PlayOneShot(killPlayerSound);
        hasKilledPlayer = true;

        movementSource.enabled = false;
        currentState = SCPState.Sleeping;
    }

    private void UpdateVignetteEffect()
    {
        if (vignette != null)
        {
            float vignetteIntensity = Mathf.Lerp(maxVignetteIntensity, minVignetteIntensity, distanceToPlayer / vignetteRange);
            vignette.intensity.value = vignetteIntensity;
        }
    }

    private void DespawnNPC()
    {
        currentState = SCPState.Sleeping;
        seenForTheFirstTime = false;
        hasPlayedNearSound = false;
        MusicPlayer.Instance.ChangeMusic(GameManager.Instance.zone1Music);
        Debug.Log("Despawning SCP-173.");
        Destroy(gameObject);
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
    }

    private bool IsPlayerLookingAtNPC()
    {
        if (objectRenderer == null || targetCamera == null) return false;

        Vector3 direction = (objectRenderer.bounds.center - targetCamera.transform.position).normalized;
        if (Physics.Raycast(targetCamera.transform.position, direction, out RaycastHit hit, distanceToPlayer))
        {
            if (hit.collider.gameObject == gameObject)
            {
                Vector3 screenPoint = targetCamera.WorldToViewportPoint(objectRenderer.bounds.center);
                return screenPoint.z > 0 && screenPoint.x >= 0 && screenPoint.x <= 1 && screenPoint.y >= 0 && screenPoint.y <= 1;
            }
        }
        return false;
    }

    IEnumerator ResetHorrorNearSound()
    {
        yield return new WaitForSeconds(10f);
        if (distanceToPlayer > 10f)
            hasPlayedNearSound = false;
    }
}