using UnityEngine;

public class AmbientZones : MonoBehaviour {
    public Collider area;

    private GameObject player;
    private AudioSource audioSource;
    private bool wasInside;

    #region Default Methods
    private void Start() {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update() {
        if (player == null || area == null) return;

        Vector3 closestPoint = area.ClosestPoint(player.transform.position);
        transform.position = closestPoint;

        bool isInside = area.bounds.Contains(player.transform.position);
        
        if (isInside != wasInside) {
            if (isInside) {
                audioSource.spatialBlend = 0;
            } else {
                audioSource.spatialBlend = 1;
            }
            wasInside = isInside;
        }
    }
    #endregion

    #region Public Methods
    // Registers the player GameObject to this ambient zone so that it dynamically works with the zone even upon late placement in the scene.
    public void RegisterPlayer(GameObject playerObj) {
        player = playerObj;
    }
    #endregion
}