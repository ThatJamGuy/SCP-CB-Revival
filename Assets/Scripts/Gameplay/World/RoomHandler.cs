using UnityEngine;

/// <summary> Purpose: Designed to act as runtime room metadata. </summary>
public class RoomHandler : MonoBehaviour
{
    [Header("Room Settings")]
    public string roomName;
    public Transform teleportLocation;

    [Header("Optimization")]
    [SerializeField] private Light[] roomLights;
    [SerializeField] private float activateDistance = 25;
    private float activationDistanceSqr;


    private bool roomState = true;   // for the room-wide mode
    private Transform transformCache; //Cache the rooms transform to never have to call it again


    [SerializeField, Tooltip("Only enable if the room is obscenely large, such as 106's containment room.")] private bool usePerLightCalculation = false;
    #region Unity Lifecycle

    private void Awake()
    {
        transformCache = this.transform;
        activationDistanceSqr = activateDistance * activateDistance;
    }

    private void Update()
    {
        if (roomLights == null || Player.Instance == null) return;

        Vector3 playerPos = Player.Instance.transform.position;


        //Cull all lights in the room if the player is too far from the rooms center
        if (!usePerLightCalculation)
        {
            bool shouldBeActive = (transformCache.position - playerPos).sqrMagnitude <= activationDistanceSqr;

            if (shouldBeActive == roomState)
                return;

            roomState = shouldBeActive;

            foreach (Light light in roomLights)
            {
                if (light != null)
                    light.gameObject.SetActive(shouldBeActive);
            }
        }

        //Cull lights individually based on the players distance from the light.
        else
        {
            foreach (Light light in roomLights)
            {
                if (light == null) continue;

                bool shouldBeActive = (light.transform.position - playerPos).sqrMagnitude <= activationDistanceSqr;

                if (light.gameObject.activeSelf != shouldBeActive)
                    light.gameObject.SetActive(shouldBeActive);
            }
        }
    }


    #endregion

    #region Public Methods

    public void TeleportPlayerHere()
    {
        Player.Instance.transform.position = teleportLocation.position;
    }

    #endregion
}