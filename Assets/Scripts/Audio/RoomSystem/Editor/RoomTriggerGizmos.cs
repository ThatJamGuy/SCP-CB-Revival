using UnityEditor;
using UnityEngine;

public static class RoomTriggerGizmos
{
    [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected)]
    private static void Draw(RoomTrigger trigger, GizmoType gizmoType)
    {
        if (!RoomDebugSettings.ShowRoomDebugs || Application.isPlaying) return;
        if (!trigger.TryGetComponent(out Collider col)) return;

        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = trigger.transform.localToWorldMatrix;
        Gizmos.color = new Color(0f, 1f, 1f, 0.25f);

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