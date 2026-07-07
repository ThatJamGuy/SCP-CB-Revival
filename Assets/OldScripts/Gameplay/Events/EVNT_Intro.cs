using FMODUnity;
using System.Collections;
using UnityEngine;

/// <summary>
/// Gigantic ass script to handle all the intro related things.
/// Persistent so that if the intro is skipped it can still work in the second post-breach primary scene
/// </summary>
public class EVNT_Intro : MonoBehaviour {
    [Header("Settings")]
    [SerializeField] private bool developerMode; // Wheter or not setup should be performed by this script for testing
    [SerializeField] private bool skipIntro; // The nuclear option; Skips everything and immediately triggers post breach stuff (Also changes player spawns)
    [SerializeField] private float defTimeToOpenCell = 30; // Default time until the cell is opened
    [SerializeField] private float paperTimeToOpenCell = 10; // Time until the cell is opened after the orentation leaflet is read

    [Header("Generic References")]
    [SerializeField] private Transform spawnRegular;
    [SerializeField] private Transform spawnSkipIntro; // Keep these two as children probably due to scene switching cases

    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private ItemData orientationLeaflet;
    [SerializeField] private Animator cellDoorAnimator;
    [SerializeField] private StudioEventEmitter doorEmitter;
    [SerializeField] private GameObject cellLight;

    [Header("SFX References")]
    [SerializeField] private EventReference requestDoorEvent;
    [SerializeField] private EventReference cellExitEvent;
    [SerializeField] private EventReference cellExitRefuseEvent;
    [SerializeField] private EventReference escortBeginEvent;
    [SerializeField] private EventReference ulgrinPissedLines;

    [Header("NPC References")]
    [SerializeField] private Actor_Guard agentUlgrin;
    [SerializeField] private Actor_Guard agentThomas;

    // These are normally loaded in via the loading stuff, but that's ofc not possible when starting directly from this scene
    [Header("Developer References")]
    [SerializeField] private GameObject inputManager;
    [SerializeField] private GameObject audioManager;
    [SerializeField] private GameObject gameManager;
    [SerializeField] private GameObject sessionCanvas;
    [SerializeField] private GameObject globalCanvas;

    private const float RQUEST_DOOR_OPEN_TIME = 11f;
    private const float CHECK_IF_LEFT_CELL_TIME = 15f;

    private bool paperHasBeenRead;
    private bool playerStillInCell = true;
    private bool reusableTimerActive;
    private float reusableTimeElapsed;

    #region Unity Callbacks

    private void Awake() {
        // Keep it persistent to work in both pre and post breach scenes
        // Destroy later to not take up resources, likely after the ulgrin franklin post breach event
        // As ambience and whatnot is handled by a post breach script similar to before
        DontDestroyOnLoad(gameObject);

        // Set up developer mode for direct testing within the scene
        if (developerMode) {
            if (inputManager == null || audioManager == null || gameManager == null || sessionCanvas == null || globalCanvas == null) {
                Debug.LogWarning("One or more of the required dev references was not set, so something vill probably break.");
                return;
            }

            if (InputManager.Instance == null) Instantiate(inputManager);
            if (AudioManager.Instance == null) Instantiate(audioManager);
            if (GameManager.Instance == null) Instantiate(gameManager);
            if (CanvasInstance.Instance == null) Instantiate(sessionCanvas);
            if (GlobalCanvasInstance.Instance == null) Instantiate(globalCanvas);
        }
    }

    private void Start() {
        // Normal setup
        if (!skipIntro) {
            // Intro Zone
            if (GameManager.Instance != null) GameManager.Instance.currentZone = 0;

            // Eventually make it to do this later as the waking up animation has yet to be made
            SetupPlayer(spawnRegular);

            StartCoroutine(OpenCellTimerDef());
            reusableTimerActive = true;
        }
        // Skip intro setup
        else {
            SetupPlayer(spawnSkipIntro);
        }
    }

    private void Update() {
        // Handle reusable timer aspects
        if (reusableTimerActive) {
            reusableTimeElapsed += Time.deltaTime;
        }

        // Check if the orentation doc was read
        if (!paperHasBeenRead && !skipIntro) {
            if (InventorySystem.Instance != null && InventorySystem.Instance.currentlyHeldItem == orientationLeaflet) {
                OnReadUpOriPaper();
                paperHasBeenRead = true;
            }
        }
    }

    #endregion

    #region Private Methods

    // May later move this to public methods, will be triggered doing the loading process
    private void SetupPlayer(Transform posToSpawn) {
        Instantiate(playerPrefab, posToSpawn);
    }

    private void ResetReusableTimer() {
        reusableTimerActive = false;
        reusableTimeElapsed = 0;
    }

    private void OnReadUpOriPaper() {
        // If the paper was picked up within the range of the paperTimeToOpenCell then ignore since it would make the number bigger
        if (reusableTimeElapsed < paperTimeToOpenCell) return;

        StopAllCoroutines();
        ResetReusableTimer();

        StartCoroutine(OpenCellTimerPaper());
    }

    #endregion

    #region Private Coroutines

    #region Sequence 1: Cell Door

    private IEnumerator OpenCellTimerDef() {
        yield return new WaitForSeconds(defTimeToOpenCell);

        AudioManager.PlayOneShot(requestDoorEvent, agentUlgrin.voiceSource.position);

        yield return new WaitForSeconds(RQUEST_DOOR_OPEN_TIME);

        cellDoorAnimator.Play("CellDoor_Open");
        doorEmitter.Play();

        yield return new WaitForSeconds(1f);

        MusicManager.Instance.SetTrack(MusicManager.MusicTrack.Intro);
        StartCoroutine(CheckExitCellCoroutine());
    }

    private IEnumerator OpenCellTimerPaper() {
        yield return new WaitForSeconds(paperTimeToOpenCell);

        AudioManager.PlayOneShot(requestDoorEvent, agentUlgrin.voiceSource.position);

        yield return new WaitForSeconds(RQUEST_DOOR_OPEN_TIME);

        cellDoorAnimator.Play("CellDoor_Open");
        doorEmitter.Play();

        yield return new WaitForSeconds(1f);

        MusicManager.Instance.SetTrack(MusicManager.MusicTrack.Intro);
        StartCoroutine(CheckExitCellCoroutine());
    }

    #endregion

    #region Sequence 2: Escort Begin

    private IEnumerator CheckExitCellCoroutine() {
        AudioManager.PlayOneShot(cellExitEvent, agentUlgrin.voiceSource.position);

        yield return new WaitForSeconds(CHECK_IF_LEFT_CELL_TIME);

        if (playerStillInCell) AudioManager.PlayOneShot(cellExitRefuseEvent, agentUlgrin.voiceSource.position);

        yield return new WaitForSeconds(CHECK_IF_LEFT_CELL_TIME);

        if (playerStillInCell) AudioManager.PlayOneShot(ulgrinPissedLines, agentUlgrin.voiceSource.position, "ulgrinPissedLevel", 0);

        yield return new WaitForSeconds(CHECK_IF_LEFT_CELL_TIME - 9);

        if (playerStillInCell) {
            cellDoorAnimator.Play("CellDoor_Close");
            doorEmitter.Play();

            yield return new WaitForSeconds(1);

            MusicManager.Instance.SetTrack(MusicManager.MusicTrack.SCP_096, 0);
            cellLight.SetActive(false);

        } else {
            AudioManager.PlayOneShot(ulgrinPissedLines, agentUlgrin.voiceSource.position, "ulgrinPissedLevel", 1);
        }
    }

    #endregion

    #endregion

    #region public methods

    public void OnPlayerExitCell() {
        playerStillInCell = false;
    }

    #endregion
}