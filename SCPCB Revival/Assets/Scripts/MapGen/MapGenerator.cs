using UnityEngine;
using NaughtyAttributes;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;

public class MapGenerator : MonoBehaviour
{
    #region Structs
    private struct PlacedRoom
    {
        public RoomData RoomData;
        public Vector2Int GridPosition;
        public int RotationDegrees;
        public Vector3 WorldPosition;
    }
    #endregion

    #region Serialized Fields
    [Header("Map Settings")]
    [SerializeField] private string seed;
    [Tooltip("If empty, uses current timestamp as seed")]
    [SerializeField] private bool useRandomSeed = true;
    public float cellSize = 20.5f;
    public ZoneData[] zones;

    [Header("Generation Settings")]
    [SerializeField] private bool generateOnStart = false;
    [SerializeField] private bool isGenerating = false;

    [Header("Gizmo Settings")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color baseGridColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);
    [SerializeField] private Color zoneConnectionColor = new Color(1f, 0.5f, 0f, 0.8f);
    #endregion

    #region Private Fields
    // Grid storage for each zone
    private Dictionary<int, Vector3[,]> zoneGrids;
    private Dictionary<int, bool[,]> zoneOccupancy;
    private System.Random random;

    // Track placed rooms and their rotations
    private Dictionary<int, List<PlacedRoom>> placedRooms;
    #endregion

    #region Properties
    public bool IsGenerating => isGenerating;
    #endregion

    #region Unity Callbacks
    private void Start()
    {
        if (generateOnStart)
        {
            StartCoroutine(GenerateMapAsync());
        }
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

            DrawZoneGrid(zoneData, grid);
            DrawZoneLabel(grid[0, 0], zoneData.ZoneID);
            DrawPlacedRooms(zoneId);
        }

        // Restore original Gizmos color
        Gizmos.color = originalColor;
    }
    #endregion

    #region Public Methods
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
    #endregion

    #region Private Methods - Initialization
    private void InitializeSeed()
    {
        if (useRandomSeed || string.IsNullOrEmpty(seed))
        {
            seed = DateTime.Now.Ticks.ToString();
        }

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

        zoneGrids = new Dictionary<int, Vector3[,]>();
        zoneOccupancy = new Dictionary<int, bool[,]>();
        Vector3 currentZoneOffset = Vector3.zero;

        for (int i = 0; i < zones.Length; i++)
        {
            ZoneData zone = zones[i];
            Vector2Int gridSize = zone.ZoneGridSize;
            Vector2Int adjustedGridSize = new Vector2Int(gridSize.x, gridSize.y + 1);

            Vector3[,] positionGrid = new Vector3[adjustedGridSize.x, adjustedGridSize.y];
            bool[,] occupancyGrid = new bool[adjustedGridSize.x, adjustedGridSize.y];

            yield return InitializeZoneGrid(positionGrid, occupancyGrid, adjustedGridSize, currentZoneOffset);

            zoneGrids[zone.ZoneID] = positionGrid;
            zoneOccupancy[zone.ZoneID] = occupancyGrid;

            currentZoneOffset.z += (gridSize.y * cellSize) + cellSize;
            yield return null;
        }
    }

    private IEnumerator InitializeZoneGrid(Vector3[,] positionGrid, bool[,] occupancyGrid, Vector2Int gridSize, Vector3 offset)
    {
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                positionGrid[x, y] = offset + new Vector3(x * cellSize, 0, y * cellSize);
                occupancyGrid[x, y] = false;
            }

            if (x % 10 == 0) yield return null;
        }
    }
    #endregion

    #region Private Methods - Room Placement
    private IEnumerator PlaceMustSpawnRoomsAsync()
    {
        foreach (var zone in zones)
        {
            InitializeZoneRoomList(zone.ZoneID);

            if (zone.CreateStartingRoom && zone.ViableStartingRooms.Count > 0)
            {
                PlaceStartingRoom(zone);
                yield return null;
            }

            yield return PlaceMustSpawnRoomsForZone(zone);
        }
    }

    private void InitializeZoneRoomList(int zoneId)
    {
        if (!placedRooms.ContainsKey(zoneId))
        {
            placedRooms[zoneId] = new List<PlacedRoom>();
        }
    }

    private void PlaceStartingRoom(ZoneData zone)
    {
        RoomData startRoom = zone.ViableStartingRooms[GetRandomRange(0, zone.ViableStartingRooms.Count - 1)];
        PlaceRoom(zone, startRoom, zone.StartingCellLocation, GetRandomRotation(startRoom));
    }

    private IEnumerator PlaceMustSpawnRoomsForZone(ZoneData zone)
    {
        var mustSpawnRooms = zone.RoomTable.Where(r => r.roomType == RoomData.RoomType.MustSpawn).ToList();

        foreach (var room in mustSpawnRooms)
        {
            yield return AttemptToPlaceRoom(zone, room);
        }
    }

    private IEnumerator AttemptToPlaceRoom(ZoneData zone, RoomData room)
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
    }

    private IEnumerator PlaceZoneConnectionsAsync()
    {
        foreach (var zone in zones)
        {
            InitializeZoneRoomList(zone.ZoneID);

            Vector2Int gridSize = zone.ZoneGridSize;
            Vector2Int adjustedGridSize = new Vector2Int(gridSize.x, gridSize.y + 1);

            if (zone.ConnectsToNextZone)
            {
                yield return PlaceZoneConnections(zone, adjustedGridSize);
            }
            else if (zone.SurfaceExits != null && zone.SurfaceExits.Count > 0)
            {
                yield return PlaceSurfaceExits(zone, adjustedGridSize);
            }
        }
    }

    private IEnumerator PlaceZoneConnections(ZoneData zone, Vector2Int gridSize)
    {
        int connectionsToPlace = zone.AmountOfConnections;
        List<int> availableColumns = Enumerable.Range(0, gridSize.x).ToList();

        for (int i = 0; i < connectionsToPlace && availableColumns.Count > 0; i++)
        {
            int columnIndex = GetRandomRange(0, availableColumns.Count - 1);
            int column = availableColumns[columnIndex];
            availableColumns.RemoveAt(columnIndex);

            Vector2Int connectionPos = new Vector2Int(column, gridSize.y - 1);
            PlaceRoom(zone, zone.ToNextZoneConnector, connectionPos, 0);
            yield return null;
        }
    }

    private IEnumerator PlaceSurfaceExits(ZoneData zone, Vector2Int gridSize)
    {
        List<int> availableColumns = Enumerable.Range(0, gridSize.x).ToList();

        foreach (var exitRoom in zone.SurfaceExits)
        {
            if (availableColumns.Count == 0) break;

            int columnIndex = GetRandomRange(0, availableColumns.Count - 1);
            int column = availableColumns[columnIndex];
            availableColumns.RemoveAt(columnIndex);

            Vector2Int exitPos = new Vector2Int(column, gridSize.y - 1);
            PlaceRoom(zone, exitRoom, exitPos, 0);
            yield return null;
        }
    }

    private Vector2Int GetRandomGridPosition(ZoneData zone, bool edgeOnly = false)
    {
        if (edgeOnly)
        {
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

        if (occupancyGrid[position.x, position.y])
            return false;

        if (room.isLarge && room.extendedSize != null)
        {
            foreach (var extension in room.extendedSize)
            {
                Vector2Int extendedPos = position + new Vector2Int((int)extension.x, (int)extension.y);

                if (extendedPos.x < 0 || extendedPos.x >= zone.ZoneGridSize.x ||
                    extendedPos.y < 0 || extendedPos.y >= zone.ZoneGridSize.y)
                    return false;

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

        occupancyGrid[gridPosition.x, gridPosition.y] = true;

        if (room.isLarge && room.extendedSize != null)
        {
            foreach (var extension in room.extendedSize)
            {
                Vector2Int extendedPos = gridPosition + new Vector2Int((int)extension.x, (int)extension.y);
                occupancyGrid[extendedPos.x, extendedPos.y] = true;
            }
        }

        placedRooms[zone.ZoneID].Add(new PlacedRoom
        {
            RoomData = room,
            GridPosition = gridPosition,
            RotationDegrees = rotationDegrees,
            WorldPosition = positionGrid[gridPosition.x, gridPosition.y]
        });
    }
    #endregion

    #region Private Methods - Gizmos
    private void DrawZoneGrid(ZoneData zoneData, Vector3[,] grid)
    {
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);

        for (int x = 0; x < rows; x++)
        {
            for (int z = 0; z < cols; z++)
            {
                Vector3 cellCenter = grid[x, z];
                Gizmos.color = baseGridColor;
                DrawCellOutline(cellCenter, cellSize);

                if (zoneData.ConnectsToNextZone && z == cols - 1)
                {
                    Gizmos.color = zoneConnectionColor;
                    DrawCellOutline(cellCenter + new Vector3(0, 0, cellSize), cellSize);
                }
            }
        }
    }

    private void DrawPlacedRooms(int zoneId)
    {
        if (placedRooms == null || !placedRooms.TryGetValue(zoneId, out var zoneRooms)) return;

        foreach (var placedRoom in zoneRooms)
        {
            Gizmos.color = GetRoomTypeColor(placedRoom.RoomData.roomType);
            DrawRoomOutline(placedRoom.WorldPosition, cellSize, placedRoom);

#if UNITY_EDITOR
            Vector3 labelPos = placedRoom.WorldPosition + Vector3.up * 0.5f;
            string roomLabel = $"{placedRoom.RoomData.roomName}\n{placedRoom.RotationDegrees}°";
            UnityEditor.Handles.Label(labelPos, roomLabel);
#endif
        }
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
        Gizmos.DrawWireCube(center + Vector3.up * 0.02f, new Vector3(size, 0, size));

        Vector3 arrowStart = center + Vector3.up * 0.03f;
        Vector3 arrowEnd = arrowStart + Quaternion.Euler(0, placedRoom.RotationDegrees, 0) * Vector3.forward * (size * 0.4f);
        Gizmos.DrawLine(arrowStart, arrowEnd);

        Vector3 right = Quaternion.Euler(0, placedRoom.RotationDegrees + 45, 0) * Vector3.forward * (size * 0.1f);
        Vector3 left = Quaternion.Euler(0, placedRoom.RotationDegrees - 45, 0) * Vector3.forward * (size * 0.1f);
        Gizmos.DrawLine(arrowEnd, arrowEnd - right);
        Gizmos.DrawLine(arrowEnd, arrowEnd - left);

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
    #endregion
}