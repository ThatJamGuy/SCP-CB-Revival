using UnityEngine;

public class TeslaKillZone : MonoBehaviour {
    [SerializeField] private TeslaController teslaController;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            if (teslaController != null) {
                teslaController.KillPlayer();
            }
        }
        if (other.CompareTag("NPC")) {
            if (other.GetComponent<SCP_106_New>()) {
                other.GetComponent<SCP_106_New>().DespawnTesla();
            }
        }
    }
}