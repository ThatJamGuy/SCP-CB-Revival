using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MapLayoutEditor : EditorWindow {
    private MapTemplate activeTemplate;
    private Vector2 scrollPos;
    private RoomData.RoomShape selectedShape = RoomData.RoomShape.TwoWay;
    private int selectedRotation = 0;

    // Static exit definitions (0=N, 90=E, 180=S, 270=W)
    private static readonly Dictionary<RoomData.RoomShape, int[]> BaseExits = new() {
        { RoomData.RoomShape.TwoWay, new[] { 0, 180 } },
        { RoomData.RoomShape.ThreeWay, new[] { 270, 180, 90 } },
        { RoomData.RoomShape.Corner, new[] { 180, 90 } },
        { RoomData.RoomShape.DeadEnd, new[] { 180 } },
        { RoomData.RoomShape.FourWay, new[] { 0, 90, 180, 270 } },
        { RoomData.RoomShape.Checkpoint, new[] { 0, 180 } }
    };

    [MenuItem("SCP:CBR/Map Editor")]
    public static void ShowWindow() {
        GetWindow<MapLayoutEditor>("Map Editor");
    }

    private void OnInspectorUpdate() {
        Repaint();
    }

    private void OnGUI() {
        DrawToolbar();

        if (activeTemplate == null) {
            EditorGUILayout.HelpBox("Select a Map Template to begin.", MessageType.Info);
            return;
        }

        DrawSettings();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        DrawGrid();
        EditorGUILayout.EndScrollView();

        if (GUI.changed) EditorUtility.SetDirty(activeTemplate);
    }

    private void DrawToolbar() {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        activeTemplate = (MapTemplate)EditorGUILayout.ObjectField(activeTemplate, typeof(MapTemplate), false);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawSettings() {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Paint Settings", EditorStyles.boldLabel);

        selectedShape = (RoomData.RoomShape)EditorGUILayout.EnumPopup("Shape", selectedShape);
        selectedRotation = EditorGUILayout.IntSlider("Rotation", selectedRotation, 0, 270);
        selectedRotation = (selectedRotation / 90) * 90;

        if (GUILayout.Button("Clear All", GUILayout.Height(20))) {
            if (EditorUtility.DisplayDialog("Clear?", "Delete all placements?", "Yes", "No"))
                activeTemplate.roomPlacements = new RoomPlacement[0];
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawGrid() {
        Vector2Int bounds = GetMapBounds();
        float btnSize = 40f;

        GUILayout.BeginVertical();
        for (int y = bounds.y - 1; y >= 0; y--) {
            GUILayout.BeginHorizontal();
            for (int x = 0; x < bounds.x; x++) {
                Vector2Int pos = new Vector2Int(x, y);
                RoomPlacement existing = FindPlacement(pos);

                Color zoneColor = GetZoneColorAtY(y);
                GUI.backgroundColor = existing != null ? Color.green : zoneColor * 0.7f;

                if (GUILayout.Button("", GUILayout.Width(btnSize), GUILayout.Height(btnSize))) {
                    TogglePlacement(pos);
                }

                if (existing != null) {
                    DrawValidatedExits(GUILayoutUtility.GetLastRect(), existing);
                }
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
        GUI.backgroundColor = Color.white;
    }

    private void DrawValidatedExits(Rect rect, RoomPlacement placement) {
        if (!BaseExits.ContainsKey(placement.requiredShape)) return;

        GUIStyle arrowStyle = new GUIStyle(EditorStyles.boldLabel) {
            alignment = TextAnchor.MiddleCenter,
            richText = true,
            fontSize = 12
        };

        foreach (int baseExit in BaseExits[placement.requiredShape]) {
            int finalDir = (baseExit + placement.rotation) % 360;

            // PATH VALIDATION: Check if the exit leads to another room
            bool isConnected = IsDirectionConnected(placement.gridPosition, finalDir);
            string color = isConnected ? "white" : "red";

            string arrow = GetSafeArrow(finalDir);
            GUI.Label(GetArrowRect(rect, finalDir), $"<color={color}>{arrow}</color>", arrowStyle);
        }
    }

    private bool IsDirectionConnected(Vector2Int pos, int angle) {
        Vector2Int neighborPos = pos;
        if (angle == 0) neighborPos.y += 1;
        else if (angle == 90) neighborPos.x += 1;
        else if (angle == 180) neighborPos.y -= 1;
        else if (angle == 270) neighborPos.x -= 1;

        return FindPlacement(neighborPos) != null;
    }

    private string GetSafeArrow(int angle) => angle switch {
        0 => "^",
        90 => ">",
        180 => "v",
        270 => "<",
        _ => "*"
    };

    private Rect GetArrowRect(Rect rect, int angle) {
        float off = 12f;
        Vector2 c = rect.center;
        return angle switch {
            0 => new Rect(c.x - 10, rect.y + 2, 20, 20),
            90 => new Rect(rect.xMax - off - 5, c.y - 10, 20, 20),
            180 => new Rect(c.x - 10, rect.yMax - off - 8, 20, 20),
            270 => new Rect(rect.x + 2, c.y - 10, 20, 20),
            _ => rect
        };
    }

    private Color GetZoneColorAtY(int y) {
        if (activeTemplate == null) return Color.gray;
        int currentY = 0;
        foreach (var zone in activeTemplate.zones) {
            int zoneEnd = currentY + zone.zoneHeight;
            if (y >= currentY && y < zoneEnd) return zone.debugColor;
            currentY = zoneEnd;
            if (zone.checkpointRoomVariant != null) {
                if (y == currentY) return Color.black;
                currentY += 1;
            }
        }
        return Color.gray;
    }

    private void TogglePlacement(Vector2Int pos) {
        List<RoomPlacement> list = new List<RoomPlacement>(activeTemplate.roomPlacements ?? new RoomPlacement[0]);
        int index = list.FindIndex(p => p.gridPosition == pos);
        if (index >= 0) list.RemoveAt(index);
        else list.Add(new RoomPlacement { gridPosition = pos, requiredShape = selectedShape, rotation = selectedRotation });
        activeTemplate.roomPlacements = list.ToArray();
    }

    private Vector2Int GetMapBounds() {
        if (activeTemplate == null) return new Vector2Int(10, 10);
        int maxWidth = 0, totalHeight = 0;
        foreach (var zone in activeTemplate.zones) {
            if (zone.zoneWidth > maxWidth) maxWidth = zone.zoneWidth;
            totalHeight += zone.zoneHeight + (zone.checkpointRoomVariant != null ? 1 : 0);
        }
        return new Vector2Int(maxWidth, totalHeight);
    }

    private RoomPlacement FindPlacement(Vector2Int pos) =>
        System.Array.Find(activeTemplate.roomPlacements ?? new RoomPlacement[0], p => p.gridPosition == pos);
}