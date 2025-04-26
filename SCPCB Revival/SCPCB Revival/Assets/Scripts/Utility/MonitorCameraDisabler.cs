using UnityEngine;

public class MonitorCameraDisabler : MonoBehaviour
{
    public Camera cameraWithMonitor;
    public bool showDebugBounds = false;

    private Bounds objectBounds;
    private Renderer objectRenderer;
    private Collider objectCollider;

    private void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        objectCollider = GetComponent<Collider>();

        if (objectRenderer != null)
            objectBounds = objectRenderer.bounds;
        else if (objectCollider != null)
            objectBounds = objectCollider.bounds;
    }

    private void Update()
    {
        if (cameraWithMonitor == null || Camera.main == null) return;

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        bool isVisible = GeometryUtility.TestPlanesAABB(planes, objectBounds);
        cameraWithMonitor.enabled = isVisible;
    }

    private void OnDrawGizmos()
    {
        if (showDebugBounds)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(objectBounds.center, objectBounds.size);
        }
    }
}
