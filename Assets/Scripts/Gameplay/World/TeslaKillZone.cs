using UnityEngine;

public class TeslaKillZone : MonoBehaviour {
    [SerializeField] private TeslaController teslaController;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            if (teslaController != null) {
                Player.Instance.KillPlayer(1, 0.5f, 3f, "Subject D-9341 killed by the Tesla Gate at [REDACTED].");
            }
        }

        if (!other.CompareTag("NPC")) return;
        
        if (other.TryGetComponent(out SCP_106_New scp106)) {
            scp106.DespawnTesla();
        }
    }
}