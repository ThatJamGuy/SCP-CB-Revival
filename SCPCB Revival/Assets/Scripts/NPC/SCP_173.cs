using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[RequireComponent(typeof(NavMeshAgent))]
public class SCP_173 : MonoBehaviour
{
    [System.Serializable]
    public enum SCPState { Sleeping, Idle, Chasing }

    [Header("State Settings")]
    [SerializeField] private SCPState currentState = SCPState.Sleeping;
    [SerializeField] private bool ignorePlayer = false;
    [SerializeField] private bool scriptedMode = false;
    [SerializeField, Range(1, 3)] private int difficulty = 1;
    [SerializeField] private float checkInterval = 0.1f; // How often to check visibility

    [Header("Movement Settings")]
    [SerializeField] private float baseSpeed = 5f;
    [SerializeField] private float stoppingDistance = 1f;
    [SerializeField] private float playerEscapeDistance = 50f;

    [Header("Visual Effect Settings")]
    [SerializeField] private float minVignetteIntensity = 0.1f;
    [SerializeField] private float maxVignetteIntensity = 0.8f;
    [SerializeField] private float vignetteRange = 15f;

    [Header("Audio Settings")]
    [SerializeField] private float nearSoundDistance = 5f;
    [SerializeField] private float nearSoundCooldown = 10f;
    [SerializeField] private float killDistance = 3f;
    [SerializeField] private float tensionAudioRange = 15f;
    [SerializeField] private float minTensionVolume = 0.1f;
    [SerializeField] private float maxTensionVolume = 1f;

    [Header("Advanced Visibility")]
    [SerializeField] private int visibilityRayCount = 7; // Number of rays to check
    [SerializeField] private float visibilityThreshold = 0.3f; // Percentage of visible rays needed to be considered visible
    [SerializeField] private LayerMask obstacleMask;

    [Header("Audio")]
    [SerializeField] private AudioClip[] horrorNearSFX;
    [SerializeField] private AudioClip[] horrorFarSFX;
    [SerializeField] private AudioClip killPlayerSound;

    [Header("References")]
    [SerializeField] private AudioSource horrorStingerSource;
    [SerializeField] private AudioSource movementSource;
    [SerializeField] private AudioSource killPlayerSource;
    [SerializeField] private AudioSource tensionSource;
    [SerializeField] private GameObject christmasHat;

    // Private variables
    private Transform player;
    private PlayerController playerController;
    private Renderer objectRenderer;
    private NavMeshAgent agent;
    private Vignette vignette;
    private Camera mainCamera;
    private Transform cameraTransform;
    private float distanceToPlayer;
    private float actualSpeed;

    // Visibility checking
    private Plane[] cameraFrustumPlanes = new Plane[6];
    private bool isVisible;
    private bool hiddenBehindObstacle;
    private bool isMoving;

    // State tracking
    private bool seenForTheFirstTime = false;
    private bool hasPlayedNearSound = false;
    private bool hasKilledPlayer = false;

    private void Awake()
    {
        InitializeComponents();
        SetupVignetteEffect();
        EnableSeasonalContent();
    }

    private void InitializeComponents()
    {
        // Get references
        objectRenderer = GetComponent<Renderer>();
        agent = GetComponent<NavMeshAgent>();
        mainCamera = Camera.main;

        if (mainCamera != null)
        {
            cameraTransform = mainCamera.transform;
        }
        else
        {
            Debug.LogError("Main camera not found!");
        }

        // Find player if not set
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerController = playerObj.GetComponent<PlayerController>();
        }
        else
        {
            Debug.LogWarning("Player not found! SCP-173 needs a player to target.");
        }
    }

    private void SetupVignetteEffect()
    {
        Volume skyAndFogVolume = GameObject.Find("Sky and Fog Volume")?.GetComponent<Volume>();
        if (skyAndFogVolume != null && skyAndFogVolume.profile.TryGet(out Vignette v))
        {
            vignette = v;
        }
        else
        {
            Debug.LogWarning("SCP-173: Vignette effect not found in Sky and Fog Volume");
        }
    }

    private void EnableSeasonalContent()
    {
        if (christmasHat != null)
        {
            christmasHat.SetActive(DateTime.Now.Month == 12);
        }
    }

    private void Start()
    {
        // Validate required components
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component is missing on " + gameObject.name);
            enabled = false;
            return;
        }

        // Apply difficulty scaling
        actualSpeed = baseSpeed * difficulty;
        agent.speed = actualSpeed;
        agent.stoppingDistance = stoppingDistance;

        // Start visibility check coroutine for better performance
        StartCoroutine(VisibilityCheckCoroutine());
    }

    // Using a coroutine for visibility checks to reduce performance impact
    private IEnumerator VisibilityCheckCoroutine()
    {
        WaitForSeconds wait = new WaitForSeconds(checkInterval);

        while (enabled)
        {
            if (!ignorePlayer && player != null)
            {
                CheckVisibility();
                HandleStateTransitions();
                HandleStateBehavior();
                HandleAudioAndEffects();
                CheckPlayerDistance();
            }
            yield return wait;
        }
    }

    private void CheckVisibility()
    {
        // Calculate distance to player
        distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check if in frustum (basic visibility)
        GeometryUtility.CalculateFrustumPlanes(mainCamera, cameraFrustumPlanes);
        bool inFrustum = GeometryUtility.TestPlanesAABB(cameraFrustumPlanes, objectRenderer.bounds);

        // Only do the more expensive raycast check if we're in the camera's view frustum
        if (inFrustum)
        {
            CheckObstacleVisibility();
        }
        else
        {
            hiddenBehindObstacle = true;
        }

        // Determine final visibility state
        isVisible = inFrustum && !hiddenBehindObstacle;

        // First time spotted logic
        if (!seenForTheFirstTime && isVisible)
        {
            HandleFirstEncounter();
        }
    }

    private void CheckObstacleVisibility()
    {
        int visiblePoints = 0;
        Vector3 center = objectRenderer.bounds.center;
        Vector3 extents = objectRenderer.bounds.extents;
        Vector3 camPos = cameraTransform.position;

        // Create a set of points around the object's bounds to test visibility
        Vector3[] points = new Vector3[visibilityRayCount];
        points[0] = center; // Center point

        // Add points at the corners and edges of the bounds
        if (visibilityRayCount > 1)
        {
            int index = 1;
            if (index < visibilityRayCount) points[index++] = center + new Vector3(extents.x, extents.y, extents.z);
            if (index < visibilityRayCount) points[index++] = center + new Vector3(-extents.x, extents.y, extents.z);
            if (index < visibilityRayCount) points[index++] = center + new Vector3(extents.x, -extents.y, extents.z);
            if (index < visibilityRayCount) points[index++] = center + new Vector3(-extents.x, -extents.y, extents.z);
            if (index < visibilityRayCount) points[index++] = center + new Vector3(0, 0, extents.z);
            if (index < visibilityRayCount) points[index++] = center + new Vector3(0, extents.y, 0);
        }

        // Cast rays from camera to each point
        foreach (Vector3 point in points)
        {
            Vector3 direction = point - camPos;
            float distance = direction.magnitude;
            Ray ray = new Ray(camPos, direction.normalized);

            if (!Physics.Raycast(ray, out RaycastHit hit, distance, obstacleMask) || hit.transform == transform)
            {
                visiblePoints++;
            }
        }

        // Calculate visibility percentage
        float visibilityPercentage = (float)visiblePoints / visibilityRayCount;
        hiddenBehindObstacle = visibilityPercentage < visibilityThreshold;
    }

    private void HandleFirstEncounter()
    {
        // Play the first encounter sound regardless of scripted mode
        PlayFirstEncounterSound();
        seenForTheFirstTime = true;
        currentState = SCPState.Idle;

        // Only change music if not in scripted mode
        if (!scriptedMode && MusicPlayer.Instance != null && GameManager.Instance != null)
        {
            MusicPlayer.Instance.ChangeMusic(GameManager.Instance.scp173Music);
        }

        Debug.Log("Player encountered SCP-173 for the first time");
    }

    private void HandleStateTransitions()
    {
        if (currentState == SCPState.Sleeping || scriptedMode || hasKilledPlayer)
            return;

        bool playerIsWatching = isVisible && playerController != null && !playerController.isBlinking;
        currentState = playerIsWatching ? SCPState.Idle : SCPState.Chasing;
    }

    private void HandleStateBehavior()
    {
        switch (currentState)
        {
            case SCPState.Sleeping:
                // Do nothing when sleeping
                break;

            case SCPState.Idle:
                IdleState();
                break;

            case SCPState.Chasing:
                ChasePlayerState();
                break;
        }
    }

    private void HandleAudioAndEffects()
    {
        // Play horror sound when close but not seen yet
        if (isVisible &&
            distanceToPlayer < nearSoundDistance &&
            !hasPlayedNearSound &&
            isMoving)
        {
            PlayHorrorNearSound();
        }

        // Kill player when close and not being observed
        if (distanceToPlayer <= killDistance && !hasKilledPlayer && !isVisible && !scriptedMode && currentState == SCPState.Chasing)
        {
            KillPlayer();
        }
        else if(distanceToPlayer <= killDistance && !hasKilledPlayer && playerController.isBlinking && !scriptedMode && currentState == SCPState.Chasing)
        {
            KillPlayer();
        }

        // Update tension audio
        if (tensionSource != null && !scriptedMode)
        {
            bool shouldPlayTension = isVisible && distanceToPlayer < tensionAudioRange;
            tensionSource.enabled = shouldPlayTension;

            if (shouldPlayTension)
            {
                float intensityFactor = 1f - (distanceToPlayer / tensionAudioRange);
                tensionSource.volume = Mathf.Lerp(minTensionVolume, maxTensionVolume, intensityFactor);
            }
        }

        // Update vignette intensity based on distance when player is looking
        if (isVisible && playerController != null && !playerController.isBlinking)
        {
            UpdateVignetteEffect();
        }
    }

    private void CheckPlayerDistance()
    {
        if (distanceToPlayer > playerEscapeDistance && seenForTheFirstTime)
        {
            DespawnNPC();
        }
    }

    private void PlayFirstEncounterSound()
    {
        if (horrorStingerSource == null) return;

        AudioClip[] soundArray = (distanceToPlayer < 10f) ? horrorNearSFX : horrorFarSFX;
        if (soundArray != null && soundArray.Length > 0)
        {
            AudioClip clip = soundArray[UnityEngine.Random.Range(0, soundArray.Length)];
            horrorStingerSource.PlayOneShot(clip);
        }
    }

    private void PlayHorrorNearSound()
    {
        if (horrorStingerSource == null || horrorNearSFX == null || horrorNearSFX.Length == 0) return;

        AudioClip clip = horrorNearSFX[UnityEngine.Random.Range(0, horrorNearSFX.Length)];
        horrorStingerSource.PlayOneShot(clip);
        hasPlayedNearSound = true;

        StartCoroutine(ResetHorrorSoundCooldown());
    }

    private IEnumerator ResetHorrorSoundCooldown()
    {
        yield return new WaitForSeconds(nearSoundCooldown);

        // Only reset if player has moved away
        if (distanceToPlayer > nearSoundDistance)
        {
            hasPlayedNearSound = false;
        }
        else
        {
            // If still in range, check again later
            StartCoroutine(ResetHorrorSoundCooldown());
        }
    }

    private void KillPlayer()
    {
        if (killPlayerSource == null || killPlayerSound == null) return;

        Debug.Log("SCP-173 killed player");
        GameManager.Instance.KillPlayer();

        killPlayerSource.PlayOneShot(killPlayerSound);
        hasKilledPlayer = true;

        if (movementSource != null)
            movementSource.enabled = false;

        currentState = SCPState.Sleeping;
        isMoving = false;
    }

    private void UpdateVignetteEffect()
    {
        if (vignette == null) return;

        float normalizedDistance = Mathf.Clamp01(distanceToPlayer / vignetteRange);
        float vignetteIntensity = Mathf.Lerp(maxVignetteIntensity, minVignetteIntensity, normalizedDistance);
        vignette.intensity.value = vignetteIntensity;
    }

    private void DespawnNPC()
    {
        currentState = SCPState.Sleeping;
        seenForTheFirstTime = false;
        hasPlayedNearSound = false;

        if (MusicPlayer.Instance != null && GameManager.Instance != null)
        {
            MusicPlayer.Instance.ChangeMusic(GameManager.Instance.zone1Music);
        }

        Debug.Log("SCP-173 despawned due to player distance");
        Destroy(gameObject);
    }

    private void ChasePlayerState()
    {
        if (agent == null || player == null) return;

        agent.speed = actualSpeed;
        agent.isStopped = false;

        if (movementSource != null)
            movementSource.enabled = true;

        MoveTowardsPlayer();
        isMoving = true;
    }

    private void IdleState()
    {
        if (agent == null) return;

        agent.speed = 0;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        if (movementSource != null)
            movementSource.enabled = false;

        isMoving = false;
    }

    private void MoveTowardsPlayer()
    {
        if (player == null) return;

        agent.SetDestination(player.position);

        // Smooth rotation towards player (only Y axis)
        Vector3 relativePos = player.position - transform.position;
        relativePos.y = 0;

        if (relativePos != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(relativePos, Vector3.up);
            transform.rotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0);
        }
    }

    // Public methods for external control
    public void SetState(SCPState newState)
    {
        currentState = newState;
    }

    public SCPState GetCurrentState()
    {
        return currentState;
    }

    public void SetDifficulty(int newDifficulty)
    {
        difficulty = Mathf.Clamp(newDifficulty, 1, 3);
        actualSpeed = baseSpeed * difficulty;
        agent.speed = actualSpeed;
    }
}