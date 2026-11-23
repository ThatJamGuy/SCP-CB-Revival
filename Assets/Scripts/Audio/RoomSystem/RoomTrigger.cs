using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A simple trigger that should be scaled to match the exact bounds of the room its representing, Including doorways.
/// see https://github.com/ThatJamGuy/SCP-CB-Revival/pull/32
/// </summary>
public class RoomTrigger : MonoBehaviour
{
    public RoomConfig roomConfig;

    public UnityEvent onRoomEnter;
    public UnityEvent onRoomExit;


    //Using Gizmos to represent room volumes, maybe not the most optimized but it should do for now.
    //Editor bullshitery, this doesnt need to be compiled as its purely designer stuff. 
#if UNITY_EDITOR
    private Collider triggerCollider;

    /// <summary>
    /// Draws a debug cube scaled to the triggers bounds to help vizualize triggers when building room triggers.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!RoomManager.showRoomDebugs || Application.isPlaying) return;      // gizmos across the whole map are expensive at runtime
        if (!triggerCollider && !TryGetComponent(out triggerCollider)) return; // no collider, nothing to draw

        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix; // respects position, rotation and scale for every shape
        Gizmos.color = new Color(0f, 1f, 1f, 0.25f);

        //Draw the appropriate mesh shape based on the collider type.
        switch (triggerCollider)
        {
            case BoxCollider box:
                Gizmos.DrawCube(box.center, box.size);
                break;

            case MeshCollider meshCollider when meshCollider.sharedMesh != null:
                // Matrix is already set, so the mesh is drawn at the transform's pose and scale
                Gizmos.DrawMesh(meshCollider.sharedMesh);
                break;

        }
        Gizmos.matrix = oldMatrix;
    }
#endif
}
