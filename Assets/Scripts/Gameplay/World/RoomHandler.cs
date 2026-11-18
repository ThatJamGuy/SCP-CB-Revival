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

    private void Update()
    {

        if (roomLights == null || Player.Instance == null) return;

        Vector3 playerPos = Player.Instance.transform.position;
        float activateDistSqr = activateDistance * activateDistance;

        bool shouldBeActive = (transform.position - playerPos).sqrMagnitude <= activateDistSqr;


        foreach (Light light in roomLights)
        {
            light.gameObject.SetActive(shouldBeActive);
        }




        //using a manual distance check over Vector3.Distance saves a single squareRoot call per light source.
        //foreach (Light light in roomLights)
        //{
        //    bool shouldBeActive = (light.transform.position - playerPos).sqrMagnitude <= activateDistSqr;

        //    if (light.gameObject.activeSelf != shouldBeActive)
        //        light.gameObject.SetActive(shouldBeActive);
        //}
    }

    #endregion

    #region Public Methods

    public void TeleportPlayerHere() {
        Player.Instance.transform.position = teleportLocation.position;
    }

    #endregion
}