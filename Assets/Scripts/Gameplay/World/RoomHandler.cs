using UnityEngine;

/// <summary> Purpose: Designed to act as runtime room metadata. </summary>
public class RoomHandler : MonoBehaviour {
    [Header("Room Settings")]
    public string roomName;
    public Transform teleportLocation;

    [Header("Optimization")]
    [SerializeField] private Light[] roomLights;
    [SerializeField] private float activateDistance = 25;
    [SerializeField, Tooltip("Only enable if the room is obscenely large, such as 106's containment room.")] private bool usePerLightCalculation = false;

    #region Unity Lifecycle

    private void Update() {
        if (roomLights == null || Player.Instance == null) return;

        Vector3 playerPos = Player.Instance.transform.position;
        float activateDistSqr = activateDistance * activateDistance;

        //Cull all lights in the room if the player is too far from the rooms center
        if (!usePerLightCalculation) {
            bool shouldBeActive = (transform.position - playerPos).sqrMagnitude <= activateDistSqr;

            foreach (Light light in roomLights) {
                if (light == null) return;
                light.gameObject.SetActive(shouldBeActive);
            }
        }

        //Cull lights individually based on the players distance from the light.
        else {
            foreach (Light light in roomLights) {
                if (light == null) return;

                bool shouldBeActive = (light.transform.position - playerPos).sqrMagnitude <= activateDistSqr;
                light.gameObject.SetActive(shouldBeActive);
            }

        }
    }

    #endregion

    #region Public Methods

    public void TeleportPlayerHere() {
        Player.Instance.transform.position = teleportLocation.position;
    }

    #endregion
}