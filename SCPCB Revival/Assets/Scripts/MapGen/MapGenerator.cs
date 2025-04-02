using UnityEngine;
using NaughtyAttributes;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Settings")]
    [SerializeField] private string seed;
    [Tooltip("If empty, uses current timestamp as seed")]
    [SerializeField] private bool useRandomSeed = true;
    public float cellSize = 20.5f;
    public ZoneData[] zones;

    [Header("Generation Settings")]
    [SerializeField] private bool generateOnStart = false;
    [SerializeField] private bool isGenerating = false;
    public bool IsGenerating => isGenerating;

    [Header("Gizmo Settings")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color baseGridColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);
    [SerializeField] private Color zoneConnectionColor = new Color(1f, 0.5f, 0f, 0.8f);

    // Grid storage for each zone
    private Dictionary<int, Vector3[,]> zoneGrids;
    private Dictionary<int, bool[,]> zoneOccupancy;
    private System.Random random;

    // Track placed rooms and their rotations
    private Dictionary<int, List<PlacedRoom>> placedRooms;

    private struct PlacedRoom
    {
        public RoomData RoomData;
        public Vector2Int GridPosition;
        public int RotationDegrees;
        public Vector3 WorldPosition;
    }

    private void Start()
    {
        if (generateOnStart)
        {
            StartCoroutine(GenerateMapAsync());
        }
    }

    [Button("Generate Map")]
    public void GenerateMap()
    {
        StartCoroutine(GenerateMapAsync());
    }

    public IEnumerator GenerateMapAsync()
    {
        if (isGenerating)
        {
            Debug.LogWarning("Map generation already in progress!");
            yield break;
        }

        isGenerating = true;
        InitializeSeed();
        placedRooms = new Dictionary<int, List<PlacedRoom>>();

        yield return InitializeZoneGridsAsync();
        yield return PlaceMustSpawnRoomsAsync();
        yield return PlaceZoneConnectionsAsync();

        isGenerating = false;
    }

    private void InitializeSeed()
    {
        if (useRandomSeed || string.IsNullOrEmpty(seed))
        {
            seed = DateTime.Now.Ticks.ToString();
        }

        // Create deterministic random number generator
        random = new System.Random(seed.GetHashCode());
        Debug.Log($"Generating map with seed: {seed}");
    }

    private IEnumerator InitializeZoneGridsAsync()
    {
        if (zones == null || zones.Length == 0)
        {
            Debug.LogError("No zones defined in MapGenerator!");
            yield break;
        }

        // Initialize dictionaries
        zoneGrids = new Dictionary<int, Vector3[,]>();
        zoneOccupancy = new Dictionary<int, bool[,]>();

        Vector3 currentZoneOffset = Vector3.zero;

        // Process each zone
        for (int i = 0; i < zones.Length; i++)
        {
            ZoneData zone = zones[i];
            Vector2Int gridSize = zone.ZoneGridSize;

            // Adjust grid size to include the connection area
            Vector2Int adjustedGridSize = new Vector2Int(gridSize.x, gridSize.y + 1);

            // Create position grid and occupancy grid for this zone
            Vector3[,] positionGrid = new Vector3[adjustedGridSize.x, adjustedGridSize.y];
            bool[,] occupancyGrid = new bool[adjustedGridSize.x, adjustedGridSize.y];

            // Calculate grid positions with gaps between zones
            for (int x = 0; x < adjustedGridSize.x; x++)
            {
                for (int y = 0; y < adjustedGridSize.y; y++)
                {
                    // Calculate world position for each cell
                    positionGrid[x, y] = currentZoneOffset + new Vector3(x * cellSize, 0, y * cellSize);
                    occupancyGrid[x, y] = false; // Initialize as unoccupied
                }

                // Yield every few rows to prevent frame drops
                if (x % 10 == 0)
                {
                    yield return null;
                }
            }

            // Store grids in dictionaries
            zoneGrids[zone.ZoneID] = positionGrid;
            zoneOccupancy[zone.ZoneID] = occupancyGrid;

            // Update offset for next zone (including gap)
            currentZoneOffset.z += (gridSize.y * cellSize) + cellSize; // Add extra cellSize as gap

            // Yield after each zone
            yield return null;
        }
    }

    private IEnumerator PlaceMustSpawnRoomsAsync()
    {
        foreach (var zone in zones)
        {
            if (!placedRooms.ContainsKey(zone.ZoneID))
            {
                placedRooms[zone.ZoneID] = new List<PlacedRoom>();
            }

            // First place the starting room if specified
            if (zone.CreateStartingRoom && zone.ViableStartingRooms.Count > 0)
            {
                RoomData startRoom = zone.ViableStartingRooms[GetRandomRange(0, zone.ViableStartingRooms.Count - 1)];
                PlaceRoom(zone, startRoom, zone.StartingCellLocation, GetRandomRotation(startRoom));
                yield return null;
            }

            // Then place must-spawn rooms
            var mustSpawnRooms = zone.RoomTable.Where(r => r.roomType == RoomData.RoomType.MustSpawn).ToList();
            foreach (var room in mustSpawnRooms)
            {
                bool placed = false;
                int attempts = 0;
                const int maxAttempts = 100;

                while (!placed && attempts < maxAttempts)
                {
                    Vector2Int randomPos = GetRandomGridPosition(zone);
                    int rotation = GetRandomRotation(room);

                    if (CanPlaceRoomAt(zone, room, randomPos, rotation))
                    {
                        PlaceRoom(zone, room, randomPos, rotation);
                        placed = true;
                    }

                    attempts++;
                    if (attempts % 10 == 0) yield return null;
                }

                if (!placed)
                {
                    Debug.LogWarning($"Failed to place must-spawn room {room.roomName} in zone {zone.ZoneID}");
                }

                yield return null;
            }
        }
    }

    private IEnumerator PlaceZoneConnectionsAsync()
    {
        foreach (var zone in zones)
        {
            if (!placedRooms.ContainsKey(zone.ZoneID))
            {
                placedRooms[zone.ZoneID] = new List<PlacedRoom>();
            }

            Vector2Int gridSize = zone.ZoneGridSize;
            Vector2Int adjustedGridSize = new Vector2Int(gridSize.x, gridSize.y + 1);

            if (zone.ConnectsToNextZone)
            {
                // Place zone connections in the last row of the adjusted grid (inside the connection zone)
                int connectionsToPlace = zone.AmountOfConnections;
                List<int> availableColumns = Enumerable.Range(0, adjustedGridSize.x).ToList();

                for (int i = 0; i < connectionsToPlace && availableColumns.Count > 0; i++)
                {
                    // Pick random column for connection
                    int columnIndex = GetRandomRange(0, availableColumns.Count - 1);
                    int column = availableColumns[columnIndex];
                    availableColumns.RemoveAt(columnIndex);

                    // Place in the last row of the adjusted grid (inside the connection zone)
                    Vector2Int connectionPos = new Vector2Int(column, adjustedGridSize.y - 1);
                    PlaceRoom(zone, zone.ToNextZoneConnector, connectionPos, 0); // Face towards next zone
                    yield return null;
                }
            }
            else if (zone.SurfaceExits != null && zone.SurfaceExits.Count > 0)
            {
                // Place surface exits in the last row of the adjusted grid similar to zone connections
                List<int> availableColumns = Enumerable.Range(0, adjustedGridSize.x).ToList();

                foreach (var exitRoom in zone.SurfaceExits)
                {
                    if (availableColumns.Count == 0) break;

                    // Pick random column for surface exit
                    int columnIndex = GetRandomRange(0, availableColumns.Count - 1);
                    int column = availableColumns[columnIndex];
                    availableColumns.RemoveAt(columnIndex);

                    Vector2Int exitPos = new Vector2Int(column, adjustedGridSize.y - 1);
                    PlaceRoom(zone, exitRoom, exitPos, 0); // Face outward
                    yield return null;
                }
            }
        }
    }

    private Vector2Int GetRandomGridPosition(ZoneData zone, bool edgeOnly = false)
    {
        if (edgeOnly)
        {
            // Return a random edge position
            bool useXEdge = GetRandomValue() > 0.5f;
            if (useXEdge)
            {
                int x = GetRandomValue() > 0.5f ? 0 : zone.ZoneGridSize.x - 1;
                return new Vector2Int(x, GetRandomRange(0, zone.ZoneGridSize.y - 1));
            }
            else
            {
                int y = GetRandomValue() > 0.5f ? 0 : zone.ZoneGridSize.y - 1;
                return new Vector2Int(GetRandomRange(0, zone.ZoneGridSize.x - 1), y);
            }
        }

        return new Vector2Int(
            GetRandomRange(0, zone.ZoneGridSize.x - 1),
            GetRandomRange(0, zone.ZoneGridSize.y - 1)
        );
    }

    private int GetRandomRotation(RoomData room)
    {
        if (!room.canRotate) return 0;
        return GetRandomRange(0, 3) * 90; // 0, 90, 180, or 270 degrees
    }

    private bool CanPlaceRoomAt(ZoneData zone, RoomData room, Vector2Int position, int rotation)
    {
        if (!zoneOccupancy.TryGetValue(zone.ZoneID, out var occupancyGrid))
            return false;

        // Check if the base cell is available
        if (occupancyGrid[position.x, position.y])
            return false;

        // If the room is large, check extended size
        if (room.isLarge && room.extendedSize != null)
        {
            foreach (var extension in room.extendedSize)
            {
                Vector2Int extendedPos = position + new Vector2Int((int)extension.x, (int)extension.y);

                // Check if position is within grid bounds
                if (extendedPos.x < 0 || extendedPos.x >= zone.ZoneGridSize.x ||
                    extendedPos.y < 0 || extendedPos.y >= zone.ZoneGridSize.y)
                    return false;

                // Check if position is occupied
                if (occupancyGrid[extendedPos.x, extendedPos.y])
                    return false;
            }
        }

        return true;
    }

    private void PlaceRoom(ZoneData zone, RoomData room, Vector2Int gridPosition, int rotationDegrees)
    {
        var occupancyGrid = zoneOccupancy[zone.ZoneID];
        var positionGrid = zoneGrids[zone.ZoneID];

        // Mark the base cell as occupied
        occupancyGrid[gridPosition.x, gridPosition.y] = true;

        // Mark extended cells for large rooms
        if (room.isLarge && room.extendedSize != null)
        {
            foreach (var extension in room.extendedSize)
            {
                Vector2Int extendedPos = gridPosition + new Vector2Int((int)extension.x, (int)extension.y);
                occupancyGrid[extendedPos.x, extendedPos.y] = true;
            }
        }

        // Record the placed room
        placedRooms[zone.ZoneID].Add(new PlacedRoom
        {
            RoomData = room,
            GridPosition = gridPosition,
            RotationDegrees = rotationDegrees,
            WorldPosition = positionGrid[gridPosition.x, gridPosition.y]
        });
    }

    // Helper method to get a random value
    public float GetRandomValue()
    {
        return (float)random.NextDouble();
    }

    // Helper method to get a random range
    public float GetRandomRange(float min, float max)
    {
        return min + (float)(random.NextDouble() * (max - min));
    }

    // Helper method to get a random int range (inclusive)
    public int GetRandomRange(int min, int max)
    {
        return random.Next(min, max + 1);
    }

    // Get the current seed
    public string GetCurrentSeed()
    {
        return seed;
    }

    // Set a specific seed
    public void SetSeed(string newSeed)
    {
        seed = newSeed;
        useRandomSeed = false;
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos || zoneGrids == null) return;

        // Store original Gizmos color
        Color originalColor = Gizmos.color;

        foreach (var zoneEntry in zoneGrids)
        {
            int zoneId = zoneEntry.Key;
            Vector3[,] grid = zoneEntry.Value;
            ZoneData zoneData = zones.Length > zoneId - 1 ? zones[zoneId - 1] : null;

            if (zoneData == null) continue;

            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);

            // Draw cells
            for (int x = 0; x < rows; x++)
            {
                for (int z = 0; z < cols; z++)
                {
                    Vector3 cellCenter = grid[x, z];

                    // Draw cell outline
                    Gizmos.color = baseGridColor;
                    DrawCellOutline(cellCenter, cellSize);

                    // If this is a zone connection area, highlight it
                    if (zoneData.ConnectsToNextZone && z == cols - 1)
                    {
                        Gizmos.color = zoneConnectionColor;
                        DrawCellOutline(cellCenter + new Vector3(0, 0, cellSize), cellSize);
                    }
                }
            }

            // Draw zone label
            DrawZoneLabel(grid[0, 0], zoneData.ZoneID);

            // Draw placed rooms
            if (placedRooms != null && placedRooms.TryGetValue(zoneId, out var zoneRooms))
            {
                foreach (var placedRoom in zoneRooms)
                {
                    // Choose color based on room type
                    Gizmos.color = GetRoomTypeColor(placedRoom.RoomData.roomType);

                    // Draw room outline
                    Vector3 roomCenter = placedRoom.WorldPosition;
                    DrawRoomOutline(roomCenter, cellSize, placedRoom);

                    // Draw room name and rotation
#if UNITY_EDITOR
                    Vector3 labelPos = roomCenter + Vector3.up * 0.5f;
                    string roomLabel = $"{placedRoom.RoomData.roomName}\n{placedRoom.RotationDegrees}°";
                    UnityEditor.Handles.Label(labelPos, roomLabel);
#endif
                }
            }
        }

        // Restore original Gizmos color
        Gizmos.color = originalColor;
    }

    private Color GetRoomTypeColor(RoomData.RoomType roomType)
    {
        switch (roomType)
        {
            case RoomData.RoomType.MustSpawn:
                return new Color(1f, 0f, 0f, 0.8f); // Red
            case RoomData.RoomType.ZoneConnection:
                return new Color(1f, 0.5f, 0f, 0.8f); // Orange
            case RoomData.RoomType.DeadEnd:
                return new Color(0.5f, 0f, 0.5f, 0.8f); // Purple
            default:
                return new Color(0f, 1f, 0f, 0.8f); // Green
        }
    }

    private void DrawRoomOutline(Vector3 center, float size, PlacedRoom placedRoom)
    {
        // Draw base cell
        Gizmos.DrawWireCube(center + Vector3.up * 0.02f, new Vector3(size, 0, size));

        // Draw direction arrow
        Vector3 arrowStart = center + Vector3.up * 0.03f;
        Vector3 arrowEnd = arrowStart + Quaternion.Euler(0, placedRoom.RotationDegrees, 0) * Vector3.forward * (size * 0.4f);
        Gizmos.DrawLine(arrowStart, arrowEnd);

        // Draw arrow head
        Vector3 right = Quaternion.Euler(0, placedRoom.RotationDegrees + 45, 0) * Vector3.forward * (size * 0.1f);
        Vector3 left = Quaternion.Euler(0, placedRoom.RotationDegrees - 45, 0) * Vector3.forward * (size * 0.1f);
        Gizmos.DrawLine(arrowEnd, arrowEnd - right);
        Gizmos.DrawLine(arrowEnd, arrowEnd - left);

        // If room is large, draw extended size
        if (placedRoom.RoomData.isLarge && placedRoom.RoomData.extendedSize != null)
        {
            foreach (var extension in placedRoom.RoomData.extendedSize)
            {
                Vector3 extendedCenter = center + new Vector3(extension.x * size, 0, extension.y * size);
                Gizmos.DrawWireCube(extendedCenter + Vector3.up * 0.02f, new Vector3(size, 0, size));
            }
        }
    }

    private void DrawCellOutline(Vector3 center, float size)
    {
        Vector3 halfSize = new Vector3(size * 0.5f, 0, size * 0.5f);

        Vector3 heightOffset = new Vector3(0, 0.01f, 0);
        Gizmos.DrawWireCube(center + heightOffset, new Vector3(size, 0, size));
    }

    private void DrawZoneLabel(Vector3 zoneStart, int zoneId)
    {
#if UNITY_EDITOR
        Vector3 labelPosition = zoneStart + new Vector3(0, 1f, 0);
        UnityEditor.Handles.Label(labelPosition, $"Zone {zoneId}");
#endif
    }
}