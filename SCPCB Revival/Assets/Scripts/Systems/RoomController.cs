using UnityEngine;

public class RoomController : MonoBehaviour {
    private enum RoomState {
        Disabled,
        Neutral,
        Enabled
    }

    [SerializeField] private GameObject roomMesh;
    [SerializeField] private GameObject lighting;
    [SerializeField] private GameObject other;

    [Header("State Configuration")]
    [SerializeField] private float disabledToNeutralDistance = 30f;
    [SerializeField] private float neutralToEnabledDistance = 15f;

    private RoomState currentState = RoomState.Disabled;
    private Transform playerTransform;
    private float sqrDisabledThreshold;
    private float sqrNeutralThreshold;
    private float lastDistance;

    private void Awake() {
        // Cache squared distances to avoid sqrt calls
        sqrDisabledThreshold = disabledToNeutralDistance * disabledToNeutralDistance;
        sqrNeutralThreshold = neutralToEnabledDistance * neutralToEnabledDistance;

        // Set initial state
        SetState(RoomState.Disabled);
    }

    private void Start() {
        // Find player - adjust this if your player tag is different
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) {
            playerTransform = player.transform;
        } else {
            Debug.LogWarning($"[RoomController] Player not found for room at {transform.position}");
        }
    }

    private void Update() {
        if (playerTransform == null) return;

        // Calculate squared distance (more efficient than sqrt)
        float sqrDistance = Vector3.SqrMagnitude(transform.position - playerTransform.position);
        
        // Update state based on distance
        UpdateState(sqrDistance);
    }

    private void UpdateState(float sqrDistance) {
        RoomState newState = currentState;

        switch (currentState) {
            case RoomState.Disabled:
                // Transition to Neutral if player gets closer
                if (sqrDistance < sqrDisabledThreshold) {
                    newState = RoomState.Neutral;
                }
                break;

            case RoomState.Neutral:
                // Transition to Enabled if player gets very close
                if (sqrDistance < sqrNeutralThreshold) {
                    newState = RoomState.Enabled;
                }
                // Transition back to Disabled if player gets far away
                else if (sqrDistance > sqrDisabledThreshold) {
                    newState = RoomState.Disabled;
                }
                break;

            case RoomState.Enabled:
                // Transition back to Neutral if player moves away
                if (sqrDistance > sqrNeutralThreshold) {
                    newState = RoomState.Neutral;
                }
                break;
        }

        // Only change state if it's different
        if (newState != currentState) {
            SetState(newState);
        }
    }

    private void SetState(RoomState newState) {
        currentState = newState;

        switch (currentState) {
            case RoomState.Disabled:
                // Disable all aspects
                roomMesh.SetActive(false);
                lighting.SetActive(false);
                other.SetActive(false);
                break;

            case RoomState.Neutral:
                // Room mesh and lighting active, other disabled
                roomMesh.SetActive(true);
                lighting.SetActive(true);
                other.SetActive(false);
                break;

            case RoomState.Enabled:
                // Everything active
                roomMesh.SetActive(true);
                lighting.SetActive(true);
                other.SetActive(true);
                break;
        }

        Debug.Log($"[RoomController] Room at {transform.position} changed to {currentState} state");
    }

    #region Public API
    /// <summary>
    /// Manually set the room state (useful for debugging or special cases)
    /// </summary>
    public void SetRoomState(int stateIndex) {
        if (stateIndex >= 0 && stateIndex < System.Enum.GetValues(typeof(RoomState)).Length) {
            SetState((RoomState)stateIndex);
        }
    }

    /// <summary>
    /// Get the current room state as a string
    /// </summary>
    public string GetCurrentState() => currentState.ToString();

    /// <summary>
    /// Manually set distance thresholds (call before game starts for best results)
    /// </summary>
    public void SetDistanceThresholds(float disabledThreshold, float neutralThreshold) {
        disabledToNeutralDistance = disabledThreshold;
        neutralToEnabledDistance = neutralThreshold;
        sqrDisabledThreshold = disabledThreshold * disabledThreshold;
        sqrNeutralThreshold = neutralThreshold * neutralThreshold;
    }
    #endregion
}