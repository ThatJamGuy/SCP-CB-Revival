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

    private const int MUST_SPAWN_PADDING = 1;
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
        CleanupExistingRooms();
        InitializeSeed();
        placedRooms = new Dictionary<int, List<PlacedRoom>>();

        yield return InitializeZoneGridsAsync();
        yield return PlaceMustSpawnRoomsAsync();
        yield return PlaceZoneConnectionsAsync();

        foreach (var zone in zones)
        {
            yield return ConnectRoomsInZone(zone);
        }

        // Instantiate all placed rooms
        InstantiateAllRooms();

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
        // Get a random viable starting room
        RoomData startRoom = zone.ViableStartingRooms[GetRandomRange(0, zone.ViableStartingRooms.Count - 1)];

        // Get all valid rotations that ensure at least one south-facing entrance
        var validRotations = GetRotationsWithSouthernExit(startRoom);

        if (validRotations.Count == 0)
        {
            Debug.LogError($"Starting room {startRoom.roomName} cannot be rotated to have a southern entrance. Please check room configuration.");
            // Fall back to random rotation if no valid rotation found
            PlaceRoom(zone, startRoom, zone.StartingCellLocation, GetRandomRotation(startRoom));
            return;
        }

        // Pick a random rotation from valid options
        int randomIndex = GetRandomRange(0, validRotations.Count - 1);
        int rotation = validRotations[randomIndex];

        PlaceRoom(zone, startRoom, zone.StartingCellLocation, rotation);
    }

    private List<int> GetRotationsWithSouthernExit(RoomData room)
    {
        var validRotations = new List<int>();

        // If room can't rotate, only check the default rotation
        if (!room.canRotate)
        {
            if (room.entranceDirections.Contains(RoomData.CardinalDirection.South))
            {
                validRotations.Add(0);
            }
            return validRotations;
        }

        // Try all possible rotations (0, 90, 180, 270 degrees)
        for (int rot = 0; rot < 4; rot++)
        {
            int rotation = rot * 90;
            if (HasSouthernEntranceAfterRotation(room.entranceDirections, rotation))
            {
                validRotations.Add(rotation);
            }
        }

        return validRotations;
    }

    private bool HasSouthernEntranceAfterRotation(HashSet<RoomData.CardinalDirection> entrances, int rotationDegrees)
    {
        // Calculate how many 90-degree rotations we're doing
        int rotationSteps = ((rotationDegrees % 360) + 360) % 360 / 90;

        foreach (var entrance in entrances)
        {
            // Rotate the entrance direction
            int newDirection = (((int)entrance + rotationSteps) % 4 + 4) % 4;
            if ((RoomData.CardinalDirection)newDirection == RoomData.CardinalDirection.South)
            {
                return true;
            }
        }

        return false;
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

        // Special handling for must-spawn rooms
        bool isMustSpawn = room.roomType == RoomData.RoomType.MustSpawn;
        bool isStartingRoom = zone.CreateStartingRoom && zone.ViableStartingRooms.Contains(room);

        while (!placed && attempts < maxAttempts)
        {
            Vector2Int randomPos = GetRandomGridPosition(zone);
            int rotation;

            // For starting rooms, use existing rotation logic
            if (isStartingRoom)
            {
                rotation = GetRandomRotation(room);
            }
            // For other rooms, ensure no off-grid entrances
            else
            {
                rotation = GetValidRotationForPosition(zone, room, randomPos);

                // Skip this position if no valid rotation found
                if (HasOffGridEntrances(zone, room, randomPos, rotation))
                {
                    attempts++;
                    continue;
                }
            }

            bool isValidPosition = isMustSpawn ?
                IsPositionValidForMustSpawn(zone, room, randomPos) :
                CanPlaceRoomAt(zone, room, randomPos, rotation);

            if (isValidPosition)
            {
                PlaceRoom(zone, room, randomPos, rotation);
                placed = true;
            }

            attempts++;
            if (attempts % 10 == 0) yield return null;
        }

        if (!placed)
        {
            Debug.LogWarning($"Failed to place {(isMustSpawn ? "must-spawn" : "normal")} room {room.roomName} in zone {zone.ZoneID}");
        }
    }

    private bool IsPositionValidForMustSpawn(ZoneData zone, RoomData room, Vector2Int position)
    {
        // Check if position is within padded bounds
        if (position.x < MUST_SPAWN_PADDING ||
            position.x >= zone.ZoneGridSize.x - MUST_SPAWN_PADDING ||
            position.y < MUST_SPAWN_PADDING ||
            position.y >= zone.ZoneGridSize.y - MUST_SPAWN_PADDING)
        {
            return false;
        }

        // For large rooms, check if extended size stays within padded bounds
        if (room.isLarge && room.extendedSize != null)
        {
            foreach (var extension in room.extendedSize)
            {
                Vector2Int extendedPos = position + new Vector2Int((int)extension.x, (int)extension.y);
                if (extendedPos.x < MUST_SPAWN_PADDING ||
                    extendedPos.x >= zone.ZoneGridSize.x - MUST_SPAWN_PADDING ||
                    extendedPos.y < MUST_SPAWN_PADDING ||
                    extendedPos.y >= zone.ZoneGridSize.y - MUST_SPAWN_PADDING)
                {
                    return false;
                }
            }
        }

        // Check if position is not occupied
        return !zoneOccupancy[zone.ZoneID][position.x, position.y];
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

        // For must-spawn rooms, ensure we're getting a position within the padded area
        return new Vector2Int(
            GetRandomRange(MUST_SPAWN_PADDING, zone.ZoneGridSize.x - MUST_SPAWN_PADDING - 1),
            GetRandomRange(MUST_SPAWN_PADDING, zone.ZoneGridSize.y - MUST_SPAWN_PADDING - 1)
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

    private bool HasOffGridEntrances(ZoneData zone, RoomData room, Vector2Int position, int rotation)
    {
        var rotatedEntrances = GetRotatedEntrances(room.entranceDirections, rotation);

        foreach (var entrance in rotatedEntrances)
        {
            Vector2Int entranceOffset = DirectionToOffset(entrance);
            Vector2Int entrancePos = position + entranceOffset;

            // Check if the entrance position is off the grid
            if (!IsValidPosition(zone, entrancePos))
            {
                return true;
            }
        }

        return false;
    }

    private int GetValidRotationForPosition(ZoneData zone, RoomData room, Vector2Int position)
    {
        if (!room.canRotate)
        {
            return 0;
        }

        List<int> validRotations = new List<int>();

        // Try all possible rotations (0, 90, 180, 270 degrees)
        for (int rot = 0; rot < 4; rot++)
        {
            int rotation = rot * 90;
            if (!HasOffGridEntrances(zone, room, position, rotation))
            {
                validRotations.Add(rotation);
            }
        }

        // If we found valid rotations, pick one randomly
        if (validRotations.Count > 0)
        {
            return validRotations[GetRandomRange(0, validRotations.Count - 1)];
        }

        // If no valid rotation found, return 0 (this should be prevented by earlier checks)
        return 0;
    }
    #endregion

    #region Room Connection Methods
    private struct ConnectionPoint
    {
        public Vector2Int Position;
        public RoomData.CardinalDirection Direction;
        public bool IsConnected;

        public ConnectionPoint(Vector2Int pos, RoomData.CardinalDirection dir)
        {
            Position = pos;
            Direction = dir;
            IsConnected = false;
        }
    }

    private List<ConnectionPoint> GetRoomConnectionPoints(PlacedRoom room)
    {
        var points = new List<ConnectionPoint>();
        var rotatedDirections = GetRotatedEntrances(room.RoomData.entranceDirections, room.RotationDegrees);

        foreach (var direction in rotatedDirections)
        {
            Vector2Int offset = DirectionToOffset(direction);
            Vector2Int connectionPos = room.GridPosition + offset;
            points.Add(new ConnectionPoint(connectionPos, direction));
        }

        return points;
    }

    private HashSet<RoomData.CardinalDirection> GetRotatedEntrances(HashSet<RoomData.CardinalDirection> originalEntrances, int rotationDegrees)
    {
        var rotated = new HashSet<RoomData.CardinalDirection>();
        int rotationSteps = ((rotationDegrees % 360) + 360) % 360 / 90;

        foreach (var entrance in originalEntrances)
        {
            int newDirection = (((int)entrance + rotationSteps) % 4 + 4) % 4;
            rotated.Add((RoomData.CardinalDirection)newDirection);
        }

        return rotated;
    }

    private Vector2Int DirectionToOffset(RoomData.CardinalDirection direction)
    {
        return direction switch
        {
            RoomData.CardinalDirection.North => new Vector2Int(0, 1),
            RoomData.CardinalDirection.East => new Vector2Int(1, 0),
            RoomData.CardinalDirection.South => new Vector2Int(0, -1),
            RoomData.CardinalDirection.West => new Vector2Int(-1, 0),
            _ => Vector2Int.zero
        };
    }

    private RoomData.CardinalDirection OppositeDirection(RoomData.CardinalDirection direction)
    {
        return direction switch
        {
            RoomData.CardinalDirection.North => RoomData.CardinalDirection.South,
            RoomData.CardinalDirection.South => RoomData.CardinalDirection.North,
            RoomData.CardinalDirection.East => RoomData.CardinalDirection.West,
            RoomData.CardinalDirection.West => RoomData.CardinalDirection.East,
            _ => direction
        };
    }

    private IEnumerator ConnectRoomsInZone(ZoneData zone)
    {
        if (!placedRooms.TryGetValue(zone.ZoneID, out var zoneRooms))
            yield break;

        var roomsByPosition = new Dictionary<Vector2Int, PlacedRoom>();
        var unconnectedEntrances = new List<(Vector2Int Position, RoomData.CardinalDirection Direction, PlacedRoom SourceRoom)>();

        // First pass: Map rooms and gather unconnected entrances
        foreach (var room in zoneRooms)
        {
            roomsByPosition[room.GridPosition] = room;
            var entrances = GetRoomConnectionPoints(room);

            foreach (var entrance in entrances)
            {
                // Check if this entrance leads to a valid position
                if (IsValidPosition(zone, entrance.Position))
                {
                    unconnectedEntrances.Add((entrance.Position, entrance.Direction, room));
                }
            }
        }
          
        // Second pass: Try to connect unconnected entrances
        foreach (var entrance in unconnectedEntrances)
        {
            if (roomsByPosition.ContainsKey(entrance.Position))
            {
                // Space already occupied, check if connection is valid
                var existingRoom = roomsByPosition[entrance.Position];
                var targetEntrances = GetRotatedEntrances(existingRoom.RoomData.entranceDirections, existingRoom.RotationDegrees);

                if (!targetEntrances.Contains(OppositeDirection(entrance.Direction)))
                {
                    Debug.LogWarning($"Mismatched connection at {entrance.Position}");
                }
            }
            else
            {
                // Try to place a connecting room
                yield return TryPlaceConnectingRoom(zone, entrance.Position, entrance.Direction, roomsByPosition);
            }

            yield return null;
        }
    }

    private bool IsValidPosition(ZoneData zone, Vector2Int position)
    {
        return position.x >= 0 && position.x < zone.ZoneGridSize.x &&
               position.y >= 0 && position.y < zone.ZoneGridSize.y;
    }

    private IEnumerator TryPlaceConnectingRoom(ZoneData zone, Vector2Int position, RoomData.CardinalDirection incomingDirection, Dictionary<Vector2Int, PlacedRoom> roomsByPosition)
    {
        // Get potential connections from adjacent cells
        var requiredConnections = new HashSet<RoomData.CardinalDirection> { OppositeDirection(incomingDirection) };

        // Check all directions for potential connections
        foreach (RoomData.CardinalDirection dir in Enum.GetValues(typeof(RoomData.CardinalDirection)))
        {
            if (dir == incomingDirection) continue;

            Vector2Int adjacentPos = position + DirectionToOffset(dir);
            if (roomsByPosition.TryGetValue(adjacentPos, out var adjacentRoom))
            {
                var adjacentEntrances = GetRotatedEntrances(adjacentRoom.RoomData.entranceDirections, adjacentRoom.RotationDegrees);
                if (adjacentEntrances.Contains(OppositeDirection(dir)))
                {
                    requiredConnections.Add(dir);
                }
            }
        }

        // Find a suitable room that matches our connection requirements
        var suitableRooms = zone.RoomTable
            .Where(r => r.roomType == RoomData.RoomType.Normal &&
                        IsRoomSuitableForConnections(r, requiredConnections) &&
                        !HasOffGridEntrances(zone, r, position, GetBestRotationForConnections(r, requiredConnections)))
            .OrderByDescending(r => r.spawnWeight + r.connectionBonusWeight * requiredConnections.Count)
            .ToList();

        if (suitableRooms.Count > 0)
        {
            // Pick a random room from the top 3 most suitable rooms
            int index = GetRandomRange(0, Mathf.Min(2, suitableRooms.Count - 1));
            var selectedRoom = suitableRooms[index];

            // Find the correct rotation
            int rotation = GetBestRotationForConnections(selectedRoom, requiredConnections);

            if (CanPlaceRoomAt(zone, selectedRoom, position, rotation))
            {
                PlaceRoom(zone, selectedRoom, position, rotation);
                roomsByPosition[position] = placedRooms[zone.ZoneID].Last();
            }
        }

        yield return null;
    }

    private bool IsRoomSuitableForConnections(RoomData room, HashSet<RoomData.CardinalDirection> requiredConnections)
    {
        if (!room.canRotate)
        {
            return requiredConnections.All(dir => room.entranceDirections.Contains(dir));
        }

        // Try all rotations
        for (int rotation = 0; rotation < 360; rotation += 90)
        {
            var rotatedEntrances = GetRotatedEntrances(room.entranceDirections, rotation);
            if (requiredConnections.All(dir => rotatedEntrances.Contains(dir)))
            {
                return true;
            }
        }

        return false;
    }

    private int GetBestRotationForConnections(RoomData room, HashSet<RoomData.CardinalDirection> requiredConnections)
    {
        if (!room.canRotate) return 0;

        for (int rotation = 0; rotation < 360; rotation += 90)
        {
            var rotatedEntrances = GetRotatedEntrances(room.entranceDirections, rotation);
            if (requiredConnections.All(dir => rotatedEntrances.Contains(dir)))
            {
                return rotation;
            }
        }

        return 0;
    }

    private RoomData FindSuitableConnectingRoom(ZoneData zone, params RoomData.CardinalDirection[] requiredDirections)
    {
        return zone.RoomTable
            .Where(r => r.roomType != RoomData.RoomType.ZoneConnection &&
                        r.roomType != RoomData.RoomType.MustSpawn)
            .FirstOrDefault(r => requiredDirections.All(d => r.entranceDirections.Contains(d)));
    }

    private int GetRotationForDirections(RoomData room, params RoomData.CardinalDirection[] requiredDirections)
    {
        if (!room.canRotate) return 0;

        for (int rotation = 0; rotation < 360; rotation += 90)
        {
            var rotatedEntrances = GetRotatedEntrances(room.entranceDirections, rotation);
            if (requiredDirections.All(d => rotatedEntrances.Contains(d)))
            {
                return rotation;
            }
        }

        return 0;
    }
    #endregion

    #region Room Connectivity Validation
    private class RoomNode
    {
        public Vector2Int Position { get; set; }
        public HashSet<Vector2Int> ConnectedTo { get; set; } = new HashSet<Vector2Int>();
        public PlacedRoom Room { get; set; }
        public bool Visited { get; set; } = false;
    }

    private IEnumerator ValidateAndFixConnectivity(ZoneData zone)
    {
        if (!placedRooms.TryGetValue(zone.ZoneID, out var zoneRooms))
            yield break;

        // Build connectivity graph
        Dictionary<Vector2Int, RoomNode> nodes = BuildRoomGraph(zoneRooms);

        // Find starting point (prefer starting room or must-spawn rooms)
        var startNode = FindStartNode(nodes, zone);
        if (startNode == null)
        {
            Debug.LogError($"No valid start node found in zone {zone.ZoneID}");
            yield break;
        }

        // Perform DFS to find unreachable rooms
        HashSet<Vector2Int> reachableRooms = new HashSet<Vector2Int>();
        Stack<RoomNode> pathStack = new Stack<RoomNode>();
        pathStack.Push(startNode);

        while (pathStack.Count > 0)
        {
            var currentNode = pathStack.Pop();
            if (!reachableRooms.Contains(currentNode.Position))
            {
                reachableRooms.Add(currentNode.Position);

                foreach (var connectedPos in currentNode.ConnectedTo)
                {
                    if (nodes.TryGetValue(connectedPos, out var connectedNode) &&
                        !reachableRooms.Contains(connectedPos))
                    {
                        pathStack.Push(connectedNode);
                    }
                }
            }

            if (pathStack.Count % 10 == 0) yield return null;
        }

        // Fix unreachable rooms
        foreach (var node in nodes.Values)
        {
            if (!reachableRooms.Contains(node.Position))
            {
                yield return FixUnreachableRoom(zone, node, nodes, reachableRooms);
            }
        }

        // Final pass - replace dead-end normal rooms with proper dead ends
        yield return OptimizeDeadEnds(zone, nodes);
    }

    private Dictionary<Vector2Int, RoomNode> BuildRoomGraph(List<PlacedRoom> zoneRooms)
    {
        var nodes = new Dictionary<Vector2Int, RoomNode>();

        foreach (var room in zoneRooms)
        {
            var node = new RoomNode
            {
                Position = room.GridPosition,
                Room = room
            };

            // Get all valid connections based on room entrances
            var entrances = GetRoomConnectionPoints(room);
            foreach (var entrance in entrances)
            {
                foreach (var otherRoom in zoneRooms)
                {
                    if (otherRoom.GridPosition == entrance.Position)
                    {
                        var otherEntrances = GetRotatedEntrances(otherRoom.RoomData.entranceDirections, otherRoom.RotationDegrees);
                        if (otherEntrances.Contains(OppositeDirection(entrance.Direction)))
                        {
                            node.ConnectedTo.Add(entrance.Position);
                        }
                    }
                }
            }

            nodes[room.GridPosition] = node;
        }

        return nodes;
    }

    private RoomNode FindStartNode(Dictionary<Vector2Int, RoomNode> nodes, ZoneData zone)
    {
        // First, try to find the starting room if it exists
        if (zone.CreateStartingRoom)
        {
            if (nodes.TryGetValue(zone.StartingCellLocation, out var startNode))
                return startNode;
        }

        // Then, try to find any must-spawn room
        foreach (var node in nodes.Values)
        {
            if (node.Room.RoomData.roomType == RoomData.RoomType.MustSpawn)
                return node;
        }

        // Finally, fall back to any room
        return nodes.Values.FirstOrDefault();
    }

    private IEnumerator FixUnreachableRoom(ZoneData zone, RoomNode unreachableNode,
        Dictionary<Vector2Int, RoomNode> nodes, HashSet<Vector2Int> reachableRooms)
    {
        // If it's a must-spawn room or zone connection, we need to create a path to it
        if (unreachableNode.Room.RoomData.roomType == RoomData.RoomType.MustSpawn ||
            unreachableNode.Room.RoomData.roomType == RoomData.RoomType.ZoneConnection)
        {
            yield return CreatePathToRoom(zone, unreachableNode, nodes, reachableRooms);
        }
        else
        {
            // For normal rooms, we can replace them with dead ends or remove them
            RoomData deadEnd = FindSuitableDeadEnd(zone, unreachableNode);
            if (deadEnd != null)
            {
                ReplaceRoom(zone, unreachableNode.Room, deadEnd);
            }
            else
            {
                // Remove the room if we can't find a suitable dead end
                RemoveRoom(zone, unreachableNode.Room);
            }
        }
    }

    private IEnumerator CreatePathToRoom(ZoneData zone, RoomNode targetNode,
        Dictionary<Vector2Int, RoomNode> nodes, HashSet<Vector2Int> reachableRooms)
    {
        // Find nearest reachable room
        var nearestReachable = FindNearestReachableRoom(targetNode.Position, reachableRooms, nodes);
        if (nearestReachable == null) yield break;

        // Create a path between the rooms
        var path = FindPath(nearestReachable.Position, targetNode.Position, zone.ZoneGridSize);
        foreach (var pos in path)
        {
            if (!nodes.ContainsKey(pos))
            {
                // Place connecting room
                var directions = GetRequiredDirectionsForPath(pos, path);
                var connectingRoom = FindSuitableConnectingRoom(zone, directions.ToArray());
                if (connectingRoom != null)
                {
                    int rotation = GetRotationForDirections(connectingRoom, directions.ToArray());
                    PlaceRoom(zone, connectingRoom, pos, rotation);
                    yield return null;
                }
            }
        }
    }

    private IEnumerator OptimizeDeadEnds(ZoneData zone, Dictionary<Vector2Int, RoomNode> nodes)
    {
        foreach (var node in nodes.Values.ToList()) // Create a copy to allow modification
        {
            if (node.ConnectedTo.Count == 1 &&
                node.Room.RoomData.roomType == RoomData.RoomType.Normal)
            {
                // This is a dead end - replace with proper dead end room if available
                RoomData deadEnd = FindSuitableDeadEnd(zone, node);
                if (deadEnd != null)
                {
                    ReplaceRoom(zone, node.Room, deadEnd);
                    yield return null;
                }
            }
        }
    }

    private RoomData FindSuitableDeadEnd(ZoneData zone, RoomNode node)
    {
        if (node.ConnectedTo.Count == 0) return null;

        // Get the direction to the only connection
        var connectedPos = node.ConnectedTo.First();
        var direction = GetDirectionBetweenPositions(node.Position, connectedPos);

        return zone.RoomTable
            .Where(r => r.roomType == RoomData.RoomType.DeadEnd)
            .FirstOrDefault(r => r.entranceDirections.Contains(direction));
    }

    private void ReplaceRoom(ZoneData zone, PlacedRoom oldRoom, RoomData newRoom)
    {
        // Remove old room
        RemoveRoom(zone, oldRoom);

        // Place new room
        PlaceRoom(zone, newRoom, oldRoom.GridPosition,
            GetRotationForDirections(newRoom, GetConnectedDirections(oldRoom).ToArray()));
    }

    private void RemoveRoom(ZoneData zone, PlacedRoom room)
    {
        var zoneRooms = placedRooms[zone.ZoneID];
        zoneRooms.Remove(room);

        // Clear occupancy
        var occupancyGrid = zoneOccupancy[zone.ZoneID];
        occupancyGrid[room.GridPosition.x, room.GridPosition.y] = false;

        if (room.RoomData.isLarge)
        {
            foreach (var extension in room.RoomData.extendedSize)
            {
                Vector2Int extendedPos = room.GridPosition + new Vector2Int((int)extension.x, (int)extension.y);
                occupancyGrid[extendedPos.x, extendedPos.y] = false;
            }
        }
    }

    private RoomData.CardinalDirection GetDirectionBetweenPositions(Vector2Int from, Vector2Int to)
    {
        Vector2Int diff = to - from;
        if (diff.x > 0) return RoomData.CardinalDirection.East;
        if (diff.x < 0) return RoomData.CardinalDirection.West;
        if (diff.y > 0) return RoomData.CardinalDirection.North;
        return RoomData.CardinalDirection.South;
    }

    private HashSet<RoomData.CardinalDirection> GetConnectedDirections(PlacedRoom room)
    {
        var directions = new HashSet<RoomData.CardinalDirection>();
        var connectionPoints = GetRoomConnectionPoints(room);
        foreach (var point in connectionPoints)
        {
            directions.Add(point.Direction);
        }
        return directions;
    }
    #endregion

    #region Pathfinding Methods
    private List<Vector2Int> FindPath(Vector2Int start, Vector2Int end, Vector2Int gridSize)
    {
        var path = new List<Vector2Int>();
        var openSet = new PriorityQueue<Vector2Int, float>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, float>();
        var fScore = new Dictionary<Vector2Int, float>();

        openSet.Enqueue(start, 0);
        gScore[start] = 0;
        fScore[start] = ManhattanDistance(start, end);

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();

            if (current == end)
            {
                // Reconstruct path
                while (current != start)
                {
                    path.Add(current);
                    current = cameFrom[current];
                }
                path.Add(start);
                path.Reverse();
                return path;
            }

            foreach (var neighbor in GetValidNeighbors(current, gridSize))
            {
                var tentativeGScore = gScore[current] + 1;

                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    float h = ManhattanDistance(neighbor, end);
                    fScore[neighbor] = gScore[neighbor] + h;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                    }
                }
            }
        }

        // No path found, return direct path (might be invalid but will be handled by room placement)
        return GetDirectPath(start, end);
    }

    private List<Vector2Int> GetDirectPath(Vector2Int start, Vector2Int end)
    {
        var path = new List<Vector2Int>();
        var current = start;

        while (current != end)
        {
            path.Add(current);
            var diff = end - current;
            var step = new Vector2Int(
                Math.Sign(diff.x),
                diff.x == 0 ? Math.Sign(diff.y) : 0
            );
            current += step;
        }
        path.Add(end);
        return path;
    }

    private float ManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private IEnumerable<Vector2Int> GetValidNeighbors(Vector2Int pos, Vector2Int gridSize)
    {
        var directions = new[]
        {
        new Vector2Int(0, 1),  // North
        new Vector2Int(1, 0),  // East
        new Vector2Int(0, -1), // South
        new Vector2Int(-1, 0)  // West
    };

        foreach (var dir in directions)
        {
            var neighbor = pos + dir;
            if (IsValidPosition(neighbor, gridSize))
            {
                yield return neighbor;
            }
        }
    }

    private bool IsValidPosition(Vector2Int pos, Vector2Int gridSize)
    {
        return pos.x >= 0 && pos.x < gridSize.x &&
               pos.y >= 0 && pos.y < gridSize.y;
    }

    private HashSet<RoomData.CardinalDirection> GetRequiredDirectionsForPath(Vector2Int position, List<Vector2Int> path)
    {
        var directions = new HashSet<RoomData.CardinalDirection>();
        int index = path.IndexOf(position);

        if (index < 0) return directions;

        // Check previous position
        if (index > 0)
        {
            var prevPos = path[index - 1];
            directions.Add(GetDirectionBetweenPositions(position, prevPos));
        }

        // Check next position
        if (index < path.Count - 1)
        {
            var nextPos = path[index + 1];
            directions.Add(GetDirectionBetweenPositions(position, nextPos));
        }

        return directions;
    }

    private RoomNode FindNearestReachableRoom(Vector2Int position, HashSet<Vector2Int> reachableRooms, Dictionary<Vector2Int, RoomNode> nodes)
    {
        RoomNode nearest = null;
        float minDistance = float.MaxValue;

        foreach (var reachablePos in reachableRooms)
        {
            if (nodes.TryGetValue(reachablePos, out var node))
            {
                float distance = ManhattanDistance(position, reachablePos);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = node;
                }
            }
        }

        return nearest;
    }

    // Priority Queue implementation for pathfinding
    private class PriorityQueue<TItem, TPriority> where TPriority : IComparable<TPriority>
    {
        private readonly List<(TItem item, TPriority priority)> elements = new List<(TItem, TPriority)>();

        public int Count => elements.Count;

        public void Enqueue(TItem item, TPriority priority)
        {
            elements.Add((item, priority));
            elements.Sort((x, y) => x.priority.CompareTo(y.priority));
        }

        public TItem Dequeue()
        {
            var item = elements[0].item;
            elements.RemoveAt(0);
            return item;
        }

        public bool Contains(TItem item)
        {
            return elements.Any(x => x.item.Equals(item));
        }
    }
    #endregion

    #region Private Methods - Room Instantiation
    private async void InstantiateRoom(PlacedRoom placedRoom)
    {
        if (placedRoom.RoomData == null || placedRoom.RoomData.roomPrefab == null)
        {
            Debug.LogError($"Cannot instantiate room: invalid room data or missing prefab reference");
            return;
        }

        try
        {
            // Load and instantiate the room prefab
            GameObject roomInstance = await placedRoom.RoomData.roomPrefab.InstantiateAsync(placedRoom.WorldPosition,
                Quaternion.Euler(0, placedRoom.RotationDegrees, 0), transform).Task;

            if (roomInstance != null)
            {
                roomInstance.name = $"{placedRoom.RoomData.roomName} ({placedRoom.GridPosition.x}, {placedRoom.GridPosition.y})";
            }
            else
            {
                Debug.LogError($"Failed to instantiate room: {placedRoom.RoomData.roomName}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error instantiating room {placedRoom.RoomData.roomName}: {e.Message}");
        }
    }

    private async void InstantiateAllRooms()
    {
        if (placedRooms == null) return;

        foreach (var zoneRooms in placedRooms.Values)
        {
            foreach (var placedRoom in zoneRooms)
            {
                InstantiateRoom(placedRoom);
                await System.Threading.Tasks.Task.Delay(10); // Small delay to prevent freezing
            }
        }
    }

    private void CleanupExistingRooms()
    {
        // Remove all existing room instances
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
            else
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
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
            string roomLabel = $"{placedRoom.RoomData.roomName}\n{placedRoom.RotationDegrees}�";
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

    private class GenerationFrontier
    {
        public Vector2Int Position { get; set; }
        public RoomData.CardinalDirection IncomingDirection { get; set; }
        public float Priority { get; set; }

        public GenerationFrontier(Vector2Int pos, RoomData.CardinalDirection dir, float priority)
        {
            Position = pos;
            IncomingDirection = dir;
            Priority = priority;
        }
    }
}