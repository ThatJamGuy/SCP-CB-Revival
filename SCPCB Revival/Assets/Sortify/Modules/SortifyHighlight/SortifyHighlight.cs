#if UNITY_EDITOR && SORTIFY_HIGHLIGHT
using UnityEditor;
using UnityEngine;

namespace Sortify
{
    [InitializeOnLoad]
    public static class SortifyHighlight
    {
        private static GameObject _hoveredObject = null;
        private static Color _highlightColor = new Color(0.5f, 0.8f, 1f, 0.6f);
        private static int _lastHierarchyMouseOverFrame = -1;

        static SortifyHighlight()
        {
            Initialzie();
        }

        public static void Initialzie()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
            SceneView.duringSceneGui += OnSceneGUI;
            RepaintAllSceneViews();
        }

        private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
        {
            Event currentEvent = Event.current;
            if (_lastHierarchyMouseOverFrame != Time.frameCount)
            {
                _hoveredObject = null;
                _lastHierarchyMouseOverFrame = Time.frameCount;
            }

            if (selectionRect.Contains(currentEvent.mousePosition))
            {
                GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
                if (obj != null)
                {
                    if (_hoveredObject != obj)
                    {
                        _hoveredObject = obj;
                        RepaintAllSceneViews();
                    }
                }
            }
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (_hoveredObject != null)
            {
                Renderer renderer = _hoveredObject.GetComponent<Renderer>();
                Collider collider = _hoveredObject.GetComponent<Collider>();
                Bounds bounds = new Bounds();

                if (renderer != null)
                {
                    bounds = renderer.bounds;
                }
                else if (collider != null)
                {
                    bounds = collider.bounds;
                }
                else
                {
                    bounds.center = _hoveredObject.transform.position;
                    bounds.size = Vector3.one;
                }

                Handles.color = _highlightColor;
                Handles.DrawWireCube(bounds.center, bounds.size);

                Color faceColor = new Color(_highlightColor.r, _highlightColor.g, _highlightColor.b, 0.1f);
                Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
                DrawSolidBox(bounds, faceColor);
            }
        }

        private static void DrawSolidBox(Bounds bounds, Color color)
        {
            Vector3[] verts = new Vector3[8];

            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;

            verts[0] = center + new Vector3(-extents.x, -extents.y, -extents.z);
            verts[1] = center + new Vector3(-extents.x, -extents.y, extents.z);
            verts[2] = center + new Vector3(-extents.x, extents.y, -extents.z);
            verts[3] = center + new Vector3(-extents.x, extents.y, extents.z);
            verts[4] = center + new Vector3(extents.x, -extents.y, -extents.z);
            verts[5] = center + new Vector3(extents.x, -extents.y, extents.z);
            verts[6] = center + new Vector3(extents.x, extents.y, -extents.z);
            verts[7] = center + new Vector3(extents.x, extents.y, extents.z);

            Handles.DrawSolidRectangleWithOutline(new Vector3[] { verts[0], verts[1], verts[3], verts[2] }, color, Color.clear);
            Handles.DrawSolidRectangleWithOutline(new Vector3[] { verts[4], verts[5], verts[7], verts[6] }, color, Color.clear);
            Handles.DrawSolidRectangleWithOutline(new Vector3[] { verts[0], verts[1], verts[5], verts[4] }, color, Color.clear);
            Handles.DrawSolidRectangleWithOutline(new Vector3[] { verts[2], verts[3], verts[7], verts[6] }, color, Color.clear);
            Handles.DrawSolidRectangleWithOutline(new Vector3[] { verts[0], verts[2], verts[6], verts[4] }, color, Color.clear);
            Handles.DrawSolidRectangleWithOutline(new Vector3[] { verts[1], verts[3], verts[7], verts[5] }, color, Color.clear);
        }

        private static void RepaintAllSceneViews()
        {
            SceneView.RepaintAll();
        }
    }
}
#endif