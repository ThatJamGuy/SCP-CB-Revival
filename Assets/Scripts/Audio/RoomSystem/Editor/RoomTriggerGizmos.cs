using UnityEditor;
using UnityEngine;

public static class RoomTriggerGizmos
{
    /// <summary>
    /// Draws a debug cube scaled to the triggers bounds to help vizualize triggers when building room triggers.
    /// </summary>
    [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected)]
    private static void Draw(RoomTrigger trigger, GizmoType gizmoType)
    {
        if (!RoomDebugSettings.ShowRoomDebugs || Application.isPlaying) return;
        if (!trigger.TryGetComponent(out Collider col)) return;

        //Respect scale, position and rotation of the gameobject
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = trigger.transform.localToWorldMatrix;
        Gizmos.color = new Color(0f, 1f, 1f, 0.25f);

        //Draw the appropriate mesh shape based on the collider type.
        switch (col)
        {
            case BoxCollider box:
                Gizmos.DrawCube(box.center, box.size);
                break;
            case MeshCollider mc when mc.sharedMesh != null:
                Gizmos.DrawMesh(mc.sharedMesh);
                break;
        }

        Gizmos.matrix = oldMatrix;
    }
}