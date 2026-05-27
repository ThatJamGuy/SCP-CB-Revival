using UnityEngine;
using FMODUnity;

/// <summary>
/// Script attached to the physical representations of items scattered around the world. Handles pickup logic
/// </summary>

[AddComponentMenu("SCP:CBR/World Item")]
public class WorldItem : MonoBehaviour, IInteractable {
    [SerializeField] private EventReference pickupSound;
    
    public void Interact(PlayerInteraction playerInteraction) {
        AudioManager.PlayOneShot(pickupSound, transform.position);
        Destroy(gameObject); // Temporary thing
    }
}