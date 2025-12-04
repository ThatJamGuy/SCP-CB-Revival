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
    [SerializeField] private float disabledToNeutralDistance = 25f;
    [SerializeField] private float neutralToEnabledDistance = 15f;

    private RoomState currentState = RoomState.Disabled;
    private Transform playerTransform;
    private float sqrDisabledThreshold;
    private float sqrNeutralThreshold;
    private float lastDistance;

    private void Awake() {
        sqrDisabledThreshold = disabledToNeutralDistance * disabledToNeutralDistance;
        sqrNeutralThreshold = neutralToEnabledDistance * neutralToEnabledDistance;

        SetState(RoomState.Disabled);
    }

    private void Start() {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) {
            playerTransform = player.transform;
        } else {
            Debug.LogWarning($"[RoomController] Player not found for room at {transform.position}");
        }
    }

    private void Update() {
        if (playerTransform == null) return;

        float sqrDistance = Vector3.SqrMagnitude(transform.position - playerTransform.position);
        
        UpdateState(sqrDistance);
    }

    private void UpdateState(float sqrDistance) {
        RoomState newState = currentState;

        switch (currentState) {
            case RoomState.Disabled:
                if (sqrDistance < sqrDisabledThreshold) {
                    newState = RoomState.Neutral;
                }
                break;

            case RoomState.Neutral:
                if (sqrDistance < sqrNeutralThreshold) {
                    newState = RoomState.Enabled;
                }
                else if (sqrDistance > sqrDisabledThreshold) {
                    newState = RoomState.Disabled;
                }
                break;

            case RoomState.Enabled:
                if (sqrDistance > sqrNeutralThreshold) {
                    newState = RoomState.Neutral;
                }
                break;
        }

        if (newState != currentState) {
            SetState(newState);
        }
    }

    private void SetState(RoomState newState) {
        currentState = newState;

        switch (currentState) {
            case RoomState.Disabled:
                roomMesh.GetComponent<Renderer>().enabled = false;
                lighting.SetActive(false);
                other.SetActive(false);
                break;

            case RoomState.Neutral:
                roomMesh.GetComponent<Renderer>().enabled = true;
                lighting.SetActive(true);
                other.SetActive(false);
                break;

            case RoomState.Enabled:
                roomMesh.GetComponent<Renderer>().enabled = true;
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