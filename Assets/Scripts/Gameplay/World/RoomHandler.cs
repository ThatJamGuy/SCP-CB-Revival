using UnityEngine;

/// <summary> Purpose: Designed to act as runtime room metadata. </summary>
public class RoomHandler : MonoBehaviour {
    [Header("Room Settings")]
    public string roomName;
    public Transform teleportLocation;

    [Header("Optimization")]
    [SerializeField] private Light[] roomLights;
    [SerializeField] private float activateDistance = 25;

    #region Unity Lifecycle

    private void Update() {
        if (roomLights == null) return;

        foreach (Light light in roomLights) {
            float distanceToPlayer = 0;

            if (Player.Instance != null)
                distanceToPlayer = Vector3.Distance(light.transform.position, Player.Instance.transform.position);

            if (distanceToPlayer <= activateDistance) light.gameObject.SetActive(true);
            else light.gameObject.SetActive(false);
        }
    }

    #endregion

    #region Public Methods

    public void TeleportPlayerHere() {
        Player.Instance.transform.position = teleportLocation.position;
    }

    #endregion
}