using UnityEngine;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MapGenerator : MonoBehaviour
{
    [Header("Map Configuration")]
    public MapSettings mapSettings;

    [Header("Debug Visualization")]
    public bool showGrid = true;
    public bool showRoomShapes = true;
    public bool showZones = true;
    public bool showConnections = true;

    [Header("Debug Colors")]
    public Color gridColor = Color.gray;
    public Color requiredRoomColor = Color.red;
    public Color exitRoomColor = Color.green;
    public Color standardRoomColor = Color.blue;
    public Color connectionColor = Color.yellow;

    [Header("Generated Data")]
    [SerializeField] private List<DebugRoom> debugRooms = new List<DebugRoom>();
    [SerializeField] private List<DebugConnection> debugConnections = new List<DebugConnection>();

    [System.Serializable]
    public class DebugRoom
    {
        public Vector2Int gridPosition;
        public RoomData roomData;
        public int zoneIndex;
        public bool isRequired;
        public bool isExit;

        public DebugRoom(Vector2Int pos, RoomData data, int zone, bool required, bool exit)
        {
            gridPosition = pos;
            roomData = data;
            zoneIndex = zone;
            isRequired = required;
            isExit = exit;
        }
    }

    [System.Serializable]
    public class DebugConnection
    {
        public Vector2Int fromPosition;
        public Vector2Int toPosition;
        public int fromZone;
        public int toZone;

        public DebugConnection(Vector2Int from, Vector2Int to, int zoneFrom, int zoneTo)
        {
            fromPosition = from;
            toPosition = to;
            fromZone = zoneFrom;
            toZone = zoneTo;
        }
    }

    [ContextMenu("Generate Debug Map")]
    public void GenerateDebugMap()
    {
        debugRooms.Clear();
        debugConnections.Clear();

        if (mapSettings == null)
        {
            Debug.LogError("Map Settings not assigned!");
            return;
        }

        // Start by placing required rooms for each zone
        Dictionary<int, List<Vector2Int>> zonePositions = new Dictionary<int, List<Vector2Int>>();

        for (int zoneIndex = 0; zoneIndex < mapSettings.zones.Count; zoneIndex++)
        {
            zonePositions[zoneIndex] = new List<Vector2Int>();
            PlaceRequiredRooms(zoneIndex, zonePositions);
        }

        // Place zone connections
        CreateZoneConnections(zonePositions);

        // Fill remaining space with standard rooms
        for (int zoneIndex = 0; zoneIndex < mapSettings.zones.Count; zoneIndex++)
        {
            FillZoneWithStandardRooms(zoneIndex, zonePositions);
        }

        Debug.Log($"Generated debug map with {debugRooms.Count} rooms and {debugConnections.Count} connections");
    }

    private void PlaceRequiredRooms(int zoneIndex, Dictionary<int, List<Vector2Int>> zonePositions)
    {
        List<RoomData> requiredRooms = mapSettings.GetRequiredRoomsForZone(zoneIndex);
        List<RoomData> exitRooms = mapSettings.GetExitRoomsForZone(zoneIndex);

        // Start zone in different areas of the grid
        Vector2Int startPos = GetZoneStartPosition(zoneIndex);

        // Place required rooms
        foreach (var room in requiredRooms)
        {
            Vector2Int pos = FindValidPosition(startPos, zonePositions);
            debugRooms.Add(new DebugRoom(pos, room, zoneIndex, true, false));
            zonePositions[zoneIndex].Add(pos);

            // If it's a large room, reserve additional grid cells
            if (room.isLargeRoom)
            {
                ReserveSpaceForLargeRoom(pos, room.expansionAmount, zonePositions[zoneIndex]);
            }
        }

        // Place exit rooms near the edges
        foreach (var room in exitRooms)
        {
            Vector2Int pos = FindEdgePosition(zonePositions);
            debugRooms.Add(new DebugRoom(pos, room, zoneIndex, false, true));
            zonePositions[zoneIndex].Add(pos);

            if (room.isLargeRoom)
            {
                ReserveSpaceForLargeRoom(pos, room.expansionAmount, zonePositions[zoneIndex]);
            }
        }
    }

    private void CreateZoneConnections(Dictionary<int, List<Vector2Int>> zonePositions)
    {
        foreach (var connection in mapSettings.zoneConnections)
        {
            if (!zonePositions.ContainsKey(connection.fromZone) || !zonePositions.ContainsKey(connection.toZone))
                continue;

            for (int i = 0; i < connection.connectionCount; i++)
            {
                if (zonePositions[connection.fromZone].Count == 0 || zonePositions[connection.toZone].Count == 0)
                    continue;

                // Find closest rooms between zones
                Vector2Int fromPos = zonePositions[connection.fromZone][0];
                Vector2Int toPos = zonePositions[connection.toZone][0];

                float closestDistance = float.MaxValue;

                foreach (var pos1 in zonePositions[connection.fromZone])
                {
                    foreach (var pos2 in zonePositions[connection.toZone])
                    {
                        float dist = Vector2Int.Distance(pos1, pos2);
                        if (dist < closestDistance)
                        {
                            closestDistance = dist;
                            fromPos = pos1;
                            toPos = pos2;
                        }
                    }
                }

                // Create a path between zones using A*
                List<Vector2Int> path = FindPath(fromPos, toPos, zonePositions);

                if (path.Count > 0)
                {
                    debugConnections.Add(new DebugConnection(fromPos, toPos, connection.fromZone, connection.toZone));

                    // Create hallway rooms along the path
                    for (int j = 1; j < path.Count - 1; j++)
                    {
                        Vector2Int current = path[j];

                        // Skip if position is already occupied
                        if (IsPositionOccupied(current, zonePositions))
                            continue;

                        // Determine appropriate hallway type based on connections
                        RoomData.RoomShape hallwayShape = DetermineHallwayShape(path, j);

                        // Create temporary hallway room data
                        RoomData hallwayRoom = CreateTemporaryHallwayRoom(hallwayShape);

                        // Add hallway to debug rooms
                        debugRooms.Add(new DebugRoom(current, hallwayRoom, -1, false, false));

                        // Mark position as occupied in both zones
                        zonePositions[connection.fromZone].Add(current);
                        zonePositions[connection.toZone].Add(current);
                    }
                }
            }
        }
    }

    private void FillZoneWithStandardRooms(int zoneIndex, Dictionary<int, List<Vector2Int>> zonePositions)
    {
        List<RoomData> standardRooms = mapSettings.GetStandardRoomsForZone(zoneIndex);
        if (standardRooms.Count == 0) return;

        // Get a list of all positions occupied by any zone
        HashSet<Vector2Int> allOccupiedPositions = new HashSet<Vector2Int>();
        foreach (var zone in zonePositions)
        {
            foreach (var pos in zone.Value)
            {
                allOccupiedPositions.Add(pos);
            }
        }

        // Find valid positions adjacent to existing zone rooms
        List<Vector2Int> validExpansionPositions = new List<Vector2Int>();
        foreach (var pos in zonePositions[zoneIndex])
        {
            foreach (var dir in GetAdjacentDirections())
            {
                Vector2Int newPos = pos + dir;
                if (IsWithinGrid(newPos) && !allOccupiedPositions.Contains(newPos))
                {
                    validExpansionPositions.Add(newPos);
                }
            }
        }

        // Randomly select positions to place standard rooms
        System.Random random = new System.Random(System.DateTime.Now.Millisecond);
        validExpansionPositions = validExpansionPositions.OrderBy(x => random.Next()).ToList();

        // Calculate how many standard rooms to place (between 30-70% of available positions)
        int roomCount = Mathf.Min(
            standardRooms.Count,
            Mathf.FloorToInt(validExpansionPositions.Count * (0.3f + (0.4f * zoneIndex / mapSettings.zones.Count)))
        );

        // Place standard rooms
        for (int i = 0; i < roomCount && i < validExpansionPositions.Count; i++)
        {
            Vector2Int pos = validExpansionPositions[i];

            // Select a random room
            RoomData room = standardRooms[random.Next(standardRooms.Count)];

            // If it's a "spawn once" room, remove it from the list
            if (room.spawnOnce)
            {
                standardRooms.Remove(room);
            }

            // Add to debug rooms
            debugRooms.Add(new DebugRoom(pos, room, zoneIndex, false, false));
            zonePositions[zoneIndex].Add(pos);
            allOccupiedPositions.Add(pos);

            // Handle large rooms
            if (room.isLargeRoom)
            {
                ReserveSpaceForLargeRoom(pos, room.expansionAmount, zonePositions[zoneIndex]);

                // Also add to all occupied positions
                for (int x = 0; x < room.expansionAmount.x; x++)
                {
                    for (int y = 0; y < room.expansionAmount.y; y++)
                    {
                        allOccupiedPositions.Add(new Vector2Int(pos.x + x, pos.y + y));
                    }
                }
            }
        }
    }

    // A* Pathfinding algorithm
    private List<Vector2Int> FindPath(Vector2Int start, Vector2Int end, Dictionary<int, List<Vector2Int>> zonePositions)
    {
        var openSet = new List<PathNode>();
        var closedSet = new HashSet<Vector2Int>();
        var startNode = new PathNode(start, 0, Vector2Int.Distance(start, end), null);

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            // Find node with lowest f-cost
            PathNode currentNode = openSet.OrderBy(n => n.fCost).First();
            openSet.Remove(currentNode);
            closedSet.Add(currentNode.position);

            // Check if reached the end
            if (currentNode.position == end)
            {
                return ReconstructPath(currentNode);
            }

            // Check neighbors
            foreach (var dir in GetAdjacentDirections())
            {
                Vector2Int neighborPos = currentNode.position + dir;

                if (!IsWithinGrid(neighborPos) || closedSet.Contains(neighborPos))
                    continue;

                // Calculate costs
                float gCost = currentNode.gCost + 1;
                float hCost = Vector2Int.Distance(neighborPos, end);

                // Create neighbor node
                PathNode neighborNode = new PathNode(neighborPos, gCost, hCost, currentNode);

                // Check if already in open set with better path
                var existingNode = openSet.FirstOrDefault(n => n.position == neighborPos);
                if (existingNode != null && existingNode.gCost <= gCost)
                    continue;

                // Add to open set
                if (existingNode != null)
                    openSet.Remove(existingNode);

                openSet.Add(neighborNode);
            }
        }

        // No path found
        return new List<Vector2Int>();
    }

    private List<Vector2Int> ReconstructPath(PathNode endNode)
    {
        var path = new List<Vector2Int>();
        PathNode currentNode = endNode;

        while (currentNode != null)
        {
            path.Add(currentNode.position);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }

    private class PathNode
    {
        public Vector2Int position;
        public float gCost; // Cost from start
        public float hCost; // Heuristic cost to end
        public float fCost => gCost + hCost;
        public PathNode parent;

        public PathNode(Vector2Int pos, float g, float h, PathNode parent)
        {
            position = pos;
            gCost = g;
            hCost = h;
            this.parent = parent;
        }
    }

    private Vector2Int FindValidPosition(Vector2Int startPos, Dictionary<int, List<Vector2Int>> zonePositions)
    {
        // Start at the given position
        Vector2Int pos = startPos;

        // Check if position is already occupied by any zone
        while (IsPositionOccupied(pos, zonePositions))
        {
            // Move to a random adjacent position
            pos += GetAdjacentDirections()[Random.Range(0, 4)];

            // Keep within grid bounds
            pos.x = Mathf.Clamp(pos.x, 0, mapSettings.gridSize.x - 1);
            pos.y = Mathf.Clamp(pos.y, 0, mapSettings.gridSize.y - 1);
        }

        return pos;
    }

    private Vector2Int FindEdgePosition(Dictionary<int, List<Vector2Int>> zonePositions)
    {
        // Try to find a position at the edge of the grid
        for (int attempts = 0; attempts < 100; attempts++)
        {
            Vector2Int pos;
            int edge = Random.Range(0, 4);

            switch (edge)
            {
                case 0: // Top edge
                    pos = new Vector2Int(Random.Range(0, mapSettings.gridSize.x), 0);
                    break;
                case 1: // Right edge
                    pos = new Vector2Int(mapSettings.gridSize.x - 1, Random.Range(0, mapSettings.gridSize.y));
                    break;
                case 2: // Bottom edge
                    pos = new Vector2Int(Random.Range(0, mapSettings.gridSize.x), mapSettings.gridSize.y - 1);
                    break;
                default: // Left edge
                    pos = new Vector2Int(0, Random.Range(0, mapSettings.gridSize.y));
                    break;
            }

            if (!IsPositionOccupied(pos, zonePositions))
            {
                return pos;
            }
        }

        // If all edge positions are occupied, find any valid position
        return FindValidPosition(new Vector2Int(Random.Range(0, mapSettings.gridSize.x),
                                              Random.Range(0, mapSettings.gridSize.y)),
                               zonePositions);
    }

    private bool IsPositionOccupied(Vector2Int pos, Dictionary<int, List<Vector2Int>> zonePositions)
    {
        foreach (var zone in zonePositions)
        {
            if (zone.Value.Contains(pos))
                return true;
        }
        return false;
    }

    private bool IsWithinGrid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < mapSettings.gridSize.x &&
               pos.y >= 0 && pos.y < mapSettings.gridSize.y;
    }

    private Vector2Int GetZoneStartPosition(int zoneIndex)
    {
        // Divide the grid into regions for each zone
        int totalZones = mapSettings.zones.Count;
        float zonePercent = (float)zoneIndex / totalZones;

        // Create a diagonal distribution of zone start positions
        int x = Mathf.FloorToInt(zonePercent * mapSettings.gridSize.x);
        int y = Mathf.FloorToInt(zonePercent * mapSettings.gridSize.y);

        return new Vector2Int(x, y);
    }

    private Vector2Int[] GetAdjacentDirections()
    {
        return new Vector2Int[]
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };
    }

    private void ReserveSpaceForLargeRoom(Vector2Int pos, Vector2Int expansionAmount, List<Vector2Int> occupiedPositions)
    {
        for (int x = 0; x < expansionAmount.x; x++)
        {
            for (int y = 0; y < expansionAmount.y; y++)
            {
                if (x == 0 && y == 0) continue; // Skip the original position

                Vector2Int expandedPos = new Vector2Int(pos.x + x, pos.y + y);
                if (IsWithinGrid(expandedPos))
                {
                    occupiedPositions.Add(expandedPos);
                }
            }
        }
    }

    private RoomData.RoomShape DetermineHallwayShape(List<Vector2Int> path, int index)
    {
        if (path.Count <= 2 || index <= 0 || index >= path.Count - 1)
            return RoomData.RoomShape.Hallway;

        Vector2Int prev = path[index - 1];
        Vector2Int current = path[index];
        Vector2Int next = path[index + 1];

        Vector2Int dirToPrev = prev - current;
        Vector2Int dirToNext = next - current;

        // Check if we're going straight
        if (dirToPrev == -dirToNext)
            return RoomData.RoomShape.Hallway;

        // Check for L-shape (90-degree turn)
        if (dirToPrev.x != 0 && dirToNext.y != 0 || dirToPrev.y != 0 && dirToNext.x != 0)
            return RoomData.RoomShape.LShapeHallway;

        // Check if it's a junction with multiple paths (simplified)
        if (index > 1 && index < path.Count - 2)
        {
            Vector2Int prevPrev = path[index - 2];
            Vector2Int nextNext = path[index + 2];

            if (prevPrev != prev && nextNext != next)
                return RoomData.RoomShape.TShapeHallway;
        }

        // Default to four-way for complex intersections
        return RoomData.RoomShape.FourWayHallway;
    }

    private RoomData CreateTemporaryHallwayRoom(RoomData.RoomShape shape)
    {
        // Create a temporary room data for the hallway
        RoomData hallway = ScriptableObject.CreateInstance<RoomData>();
        hallway.roomName = "Hallway_" + shape.ToString();
        hallway.shape = shape;
        hallway.isLargeRoom = false;
        hallway.expansionAmount = Vector2Int.one;
        hallway.isExitRoom = false;
        hallway.mustSpawn = false;
        hallway.spawnOnce = false;

        return hallway;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (mapSettings == null) return;

        float cellSize = mapSettings.cellSize;

        // Draw grid
        if (showGrid)
        {
            Gizmos.color = gridColor;
            for (int x = 0; x <= mapSettings.gridSize.x; x++)
            {
                Gizmos.DrawLine(
                    new Vector3(x * cellSize, 0, 0),
                    new Vector3(x * cellSize, 0, mapSettings.gridSize.y * cellSize)
                );
            }

            for (int y = 0; y <= mapSettings.gridSize.y; y++)
            {
                Gizmos.DrawLine(
                    new Vector3(0, 0, y * cellSize),
                    new Vector3(mapSettings.gridSize.x * cellSize, 0, y * cellSize)
                );
            }
        }

        // Draw rooms
        if (showRoomShapes && debugRooms.Count > 0)
        {
            foreach (var room in debugRooms)
            {
                // Determine color based on room type
                if (room.isRequired)
                    Gizmos.color = requiredRoomColor;
                else if (room.isExit)
                    Gizmos.color = exitRoomColor;
                else
                    Gizmos.color = standardRoomColor;

                Vector3 position = new Vector3(
                    room.gridPosition.x * cellSize + cellSize * 0.5f,
                    0,
                    room.gridPosition.y * cellSize + cellSize * 0.5f
                );

                // Draw based on room shape
                switch (room.roomData.shape)
                {
                    case RoomData.RoomShape.EndRoom:
                        DrawEndRoom(position, cellSize);
                        break;
                    case RoomData.RoomShape.Hallway:
                        DrawHallway(position, cellSize);
                        break;
                    case RoomData.RoomShape.TShapeHallway:
                        DrawTShapeHallway(position, cellSize);
                        break;
                    case RoomData.RoomShape.FourWayHallway:
                        DrawFourWayHallway(position, cellSize);
                        break;
                    case RoomData.RoomShape.LShapeHallway:
                        DrawLShapeHallway(position, cellSize);
                        break;
                }

                // Draw zone number
                if (showZones && room.zoneIndex >= 0)
                {
                    Handles.Label(position + Vector3.up * 2, "Z" + room.zoneIndex.ToString());
                }

                // Handle large rooms
                if (room.roomData.isLargeRoom)
                {
                    Vector3 expandedSize = new Vector3(
                        room.roomData.expansionAmount.x * cellSize,
                        0.1f,
                        room.roomData.expansionAmount.y * cellSize
                    );

                    Gizmos.DrawWireCube(
                        position + new Vector3(
                            (room.roomData.expansionAmount.x - 1) * cellSize * 0.5f,
                            0,
                            (room.roomData.expansionAmount.y - 1) * cellSize * 0.5f
                        ),
                        expandedSize
                    );
                }
            }
        }

        // Draw connections
        if (showConnections && debugConnections.Count > 0)
        {
            Gizmos.color = connectionColor;

            foreach (var connection in debugConnections)
            {
                Vector3 from = new Vector3(
                    connection.fromPosition.x * cellSize + cellSize * 0.5f,
                    1,
                    connection.fromPosition.y * cellSize + cellSize * 0.5f
                );

                Vector3 to = new Vector3(
                    connection.toPosition.x * cellSize + cellSize * 0.5f,
                    1,
                    connection.toPosition.y * cellSize + cellSize * 0.5f
                );

                Gizmos.DrawLine(from, to);

                // Draw connection labels
                Vector3 midpoint = (from + to) * 0.5f;
                Handles.Label(midpoint + Vector3.up,
                           $"Z{connection.fromZone}→Z{connection.toZone}");
            }
        }
    }

    private void DrawEndRoom(Vector3 position, float cellSize)
    {
        float size = cellSize * 0.8f;
        Gizmos.DrawCube(position, new Vector3(size, 0.1f, size));
    }

    private void DrawHallway(Vector3 position, float cellSize)
    {
        float width = cellSize * 0.4f;
        float length = cellSize * 0.8f;
        Gizmos.DrawCube(position, new Vector3(width, 0.1f, length));
    }

    private void DrawTShapeHallway(Vector3 position, float cellSize)
    {
        float size = cellSize * 0.8f;
        float width = cellSize * 0.4f;

        // Horizontal part
        Gizmos.DrawCube(position, new Vector3(size, 0.1f, width));

        // Vertical part (only one direction)
        Gizmos.DrawCube(
            position + new Vector3(0, 0, size * 0.3f),
            new Vector3(width, 0.1f, size * 0.6f)
        );
    }

    private void DrawFourWayHallway(Vector3 position, float cellSize)
    {
        float size = cellSize * 0.8f;
        float width = cellSize * 0.4f;

        // Horizontal part
        Gizmos.DrawCube(position, new Vector3(size, 0.1f, width));

        // Vertical part
        Gizmos.DrawCube(position, new Vector3(width, 0.1f, size));
    }

    private void DrawLShapeHallway(Vector3 position, float cellSize)
    {
        float size = cellSize * 0.4f;
        float offset = cellSize * 0.2f;

        // Horizontal part
        Gizmos.DrawCube(
            position + new Vector3(offset, 0, 0),
            new Vector3(size, 0.1f, size)
        );

        // Vertical part
        Gizmos.DrawCube(
            position + new Vector3(0, 0, offset),
            new Vector3(size, 0.1f, size)
        );
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(MapGenerator))]
public class MapGenerationDebuggerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MapGenerator debugger = (MapGenerator)target;

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate Debug Map"))
        {
            debugger.GenerateDebugMap();
        }
    }
}
#endif