using UnityEngine;

public class SecurityCam : MonoBehaviour {
    [SerializeField] private Camera cam;
    [SerializeField] private float updateInderval = 0.5f;
    [SerializeField] private float activationDistance = 15f;

    private float nextUpdate;
    private bool isActive;

    private void Start() {
        cam.enabled = false;
        nextUpdate = Time.time + updateInderval;
    }

    private void Update() {
        if (PlayerAccessor.instance != null) { 
            float dist = Vector3.Distance(transform.position, PlayerAccessor.instance.transform.position);
            bool shouldBeActive = dist < activationDistance;

            if (shouldBeActive != isActive) {
                isActive = shouldBeActive;
                cam.enabled = isActive;
            }
        }

        if (Time.time > nextUpdate) {
            cam.Render();
            nextUpdate = Time.time + updateInderval;
        }
    }
}