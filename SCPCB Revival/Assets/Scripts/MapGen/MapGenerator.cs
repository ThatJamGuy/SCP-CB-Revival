using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;

public class MapGenerator : MonoBehaviour
{
    public MapSettings mapSettings;

    private GridCell[,] grid;
    private List<RoomData> spawnedRooms = new List<RoomData>();
    private Queue<Vector2Int> roomsToProcess = new Queue<Vector2Int>();
    private HashSet<RoomData> remainingMustSpawnRooms = new HashSet<RoomData>();
    private HashSet<string> placedDoorPositions = new HashSet<string>();

    private void Awake()
    {
        CreateWorld();
    }

    public void CreateWorld()
    {
        InitializeGrid();
        InitializeMustSpawnRooms();
        PlaceStartingRoom();
        GenerateMap();
    }

    void InitializeGrid()
    {
        grid = new GridCell[mapSettings.gridSize.x, mapSettings.gridSize.y];

        for (int x = 0; x < mapSettings.gridSize.x; x++)
        {
            for (int y = 0; y < mapSettings.gridSize.y; y++)
            {
                grid[x, y] = new GridCell(new Vector2Int(x, y));
            }
        }

        Debug.Log("Grid initialized with size: " + mapSettings.gridSize);
    }

    [Button("Regenerate Map")]
    void RegenerateMap()
    {
        ClearMap();
        InitializeGrid();
        InitializeMustSpawnRooms();
        PlaceStartingRoom();
        GenerateMap();
    }

    void ClearMap()
    {
        List<GameObject> children = new List<GameObject>();

        foreach (Transform child in transform)
            children.Add(child.gameObject);

        foreach (GameObject child in children)
            DestroyImmediate(child);

        spawnedRooms.Clear();
        grid = null;
        placedDoorPositions.Clear();

        Debug.Log("Map cleared successfully.");
    }

    void InitializeMustSpawnRooms()
    {
        remainingMustSpawnRooms.Clear();
        foreach (RoomData room in mapSettings.zones[0].rooms)
        {
            if (room.mustSpawn && !room.isExitRoom)
            {
                remainingMustSpawnRooms.Add(room);
            }
        }
    }

    void PlaceStartingRoom()
    {
        List<RoomData> availableRooms = mapSettings.zones[0].rooms.FindAll(room =>
            !room.isExitRoom &&
            (room.shape == RoomData.RoomShape.TShapeHallway || room.shape == RoomData.RoomShape.FourWayHallway) &&
            (room.mustSpawn || !spawnedRooms.Contains(room))
        );

        if (availableRooms.Count == 0)
        {
            Debug.LogError("No available rooms to place at the starting coordinates.");
            return;
        }

        RoomData startingRoom = availableRooms[Random.Range(0, availableRooms.Count)];
        spawnedRooms.Add(startingRoom);

        Vector2Int startCoords = mapSettings.startingCoords;
        grid[startCoords.x, startCoords.y].roomData = startingRoom;

        int rotation = GetValidRotation(startingRoom);
        grid[startCoords.x, startCoords.y].rotation = rotation;

        InstantiateRoom(startingRoom, startCoords, rotation);

        Debug.Log($"Placed starting room '{startingRoom.roomName}' at {startCoords} with rotation {rotation} degrees.");
    }

    void GenerateMap()
    {
        // Initial map generation
        Vector2Int startCoords = mapSettings.startingCoords;
        roomsToProcess.Enqueue(startCoords);

        while (roomsToProcess.Count > 0)
        {
            Vector2Int currentCoords = roomsToProcess.Dequeue();
            GridCell currentCell = grid[currentCoords.x, currentCoords.y];

            for (int i = 0; i < 4; i++)
            {
                int rotatedDirection = (i + (currentCell.rotation / 90)) % 4;
                if (currentCell.roomData.entrances[i])
                {
                    Vector2Int adjacentCoords = GetAdjacentCoordinates(currentCoords, rotatedDirection);
                    if (IsWithinGrid(adjacentCoords) && grid[adjacentCoords.x, adjacentCoords.y].roomData == null)
                    {
                        PlaceAdjacentRoom(adjacentCoords, rotatedDirection);
                        roomsToProcess.Enqueue(adjacentCoords);
                    }
                }
            }
        }

        // Place remaining must-spawn rooms
        PlaceRemainingMustSpawnRooms();

        // Fill remaining dead ends
        FillDeadEnds();

        // Verify and fix connectivity
        EnsureFullConnectivity();
    }

    void PlaceRemainingMustSpawnRooms()
    {
        if (remainingMustSpawnRooms.Count == 0) return;

        foreach (RoomData mustSpawnRoom in new List<RoomData>(remainingMustSpawnRooms))
        {
            PlaceMustSpawnRoom(mustSpawnRoom);
        }
    }

    void PlaceMustSpawnRoom(RoomData room)
    {
        List<Vector2Int> potentialSpots = new List<Vector2Int>();

        // First try to find empty spots that connect to existing rooms
        for (int x = 0; x < mapSettings.gridSize.x; x++)
        {
            for (int y = 0; y < mapSettings.gridSize.y; y++)
            {
                Vector2Int coords = new Vector2Int(x, y);
                if (!IsPositionOccupied(coords) && CanPlaceRoomAt(room, coords))
                {
                    potentialSpots.Add(coords);
                }
            }
        }

        if (potentialSpots.Count == 0)
        {
            // If no valid spots found, try to force placement by replacing an existing room
            for (int x = 0; x < mapSettings.gridSize.x; x++)
            {
                for (int y = 0; y < mapSettings.gridSize.y; y++)
                {
                    if (grid[x, y].roomData != null && !grid[x, y].roomData.mustSpawn)
                    {
                        Vector2Int coords = new Vector2Int(x, y);
                        if (CanPlaceRoomAt(room, coords))
                        {
                            potentialSpots.Add(coords);
                        }
                    }
                }
            }
        }

        if (potentialSpots.Count > 0)
        {
            Vector2Int selectedSpot = potentialSpots[Random.Range(0, potentialSpots.Count)];

            // If replacing an existing room, clean it up first
            if (grid[selectedSpot.x, selectedSpot.y].roomData != null)
            {
                DestroyImmediate(grid[selectedSpot.x, selectedSpot.y].roomData.roomPrefab);
                spawnedRooms.Remove(grid[selectedSpot.x, selectedSpot.y].roomData);
            }

            int rotation = GetValidRotationForSpot(room, selectedSpot);
            grid[selectedSpot.x, selectedSpot.y].roomData = room;
            grid[selectedSpot.x, selectedSpot.y].rotation = rotation;

            InstantiateRoom(room, selectedSpot, rotation);
            spawnedRooms.Add(room);
            remainingMustSpawnRooms.Remove(room);
        }
        else
        {
            Debug.LogError($"Failed to place must-spawn room: {room.roomName}");
        }
    }

    bool CanPlaceRoomAt(RoomData room, Vector2Int coords)
    {
        // First check if the target position is already occupied
        if (IsPositionOccupied(coords))
            return false;

        // Check if at least one valid rotation exists that connects to existing rooms
        for (int rotation = 0; rotation < 4; rotation++)
        {
            bool hasValidConnection = false;
            bool hasInvalidConnection = false;

            for (int i = 0; i < 4; i++)
            {
                if (!room.entrances[i]) continue;

                int rotatedDirection = (i + rotation) % 4;
                Vector2Int adjacentCoords = GetAdjacentCoordinates(coords, rotatedDirection);

                if (!IsWithinGrid(adjacentCoords))
                {
                    hasInvalidConnection = true;
                    break;
                }

                GridCell adjacentCell = grid[adjacentCoords.x, adjacentCoords.y];
                if (adjacentCell.roomData != null)
                {
                    if (CanRoomsConnect(room, rotation * 90, adjacentCell.roomData, adjacentCell.rotation, rotatedDirection))
                    {
                        hasValidConnection = true;
                    }
                    else
                    {
                        hasInvalidConnection = true;
                        break;
                    }
                }
            }

            if (hasValidConnection && !hasInvalidConnection)
            {
                return true;
            }
        }

        return false;
    }

    int GetValidRotationForSpot(RoomData room, Vector2Int coords)
    {
        for (int rotation = 0; rotation < 4; rotation++)
        {
            bool isValid = true;
            bool hasConnection = false;

            for (int i = 0; i < 4; i++)
            {
                if (!room.entrances[i]) continue;

                int rotatedDirection = (i + rotation) % 4;
                Vector2Int adjacentCoords = GetAdjacentCoordinates(coords, rotatedDirection);

                if (!IsWithinGrid(adjacentCoords))
                {
                    isValid = false;
                    break;
                }

                GridCell adjacentCell = grid[adjacentCoords.x, adjacentCoords.y];
                if (adjacentCell.roomData != null)
                {
                    if (CanRoomsConnect(room, rotation * 90, adjacentCell.roomData, adjacentCell.rotation, rotatedDirection))
                    {
                        hasConnection = true;
                    }
                    else
                    {
                        isValid = false;
                        break;
                    }
                }
            }

            if (isValid && hasConnection)
            {
                return rotation * 90;
            }
        }

        return 0;
    }

    void EnsureFullConnectivity()
    {
        HashSet<Vector2Int> connectedRooms = new HashSet<Vector2Int>();
        FindConnectedRooms(mapSettings.startingCoords, connectedRooms);

        // Find all rooms that should be connected
        HashSet<Vector2Int> allRooms = new HashSet<Vector2Int>();
        for (int x = 0; x < mapSettings.gridSize.x; x++)
        {
            for (int y = 0; y < mapSettings.gridSize.y; y++)
            {
                if (grid[x, y].roomData != null)
                {
                    allRooms.Add(new Vector2Int(x, y));
                }
            }
        }

        // Find isolated rooms
        allRooms.ExceptWith(connectedRooms);
        if (allRooms.Count > 0)
        {
            ConnectIsolatedRooms(connectedRooms, allRooms);
        }
    }

    void FindConnectedRooms(Vector2Int start, HashSet<Vector2Int> connected)
    {
        if (!IsWithinGrid(start) || grid[start.x, start.y].roomData == null || connected.Contains(start))
            return;

        connected.Add(start);
        GridCell cell = grid[start.x, start.y];

        for (int i = 0; i < 4; i++)
        {
            int rotatedDirection = (i + (cell.rotation / 90)) % 4;
            if (cell.roomData.entrances[i])
            {
                Vector2Int adjacent = GetAdjacentCoordinates(start, rotatedDirection);
                if (IsWithinGrid(adjacent) && grid[adjacent.x, adjacent.y].roomData != null)
                {
                    FindConnectedRooms(adjacent, connected);
                }
            }
        }
    }

    void ConnectIsolatedRooms(HashSet<Vector2Int> connected, HashSet<Vector2Int> isolated)
    {
        foreach (Vector2Int isolatedPos in isolated)
        {
            Vector2Int? bestConnection = FindBestConnectionPoint(isolatedPos, connected);
            if (bestConnection.HasValue)
            {
                CreateConnectionBetween(isolatedPos, bestConnection.Value);
            }
        }
    }

    Vector2Int? FindBestConnectionPoint(Vector2Int from, HashSet<Vector2Int> connected)
    {
        float shortestDistance = float.MaxValue;
        Vector2Int? bestTarget = null;

        foreach (Vector2Int target in connected)
        {
            float distance = Vector2Int.Distance(from, target);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                bestTarget = target;
            }
        }

        return bestTarget;
    }

    void CreateConnectionBetween(Vector2Int from, Vector2Int to)
    {
        // Calculate direction
        Vector2Int direction = new Vector2Int(
            Mathf.Clamp(to.x - from.x, -1, 1),
            Mathf.Clamp(to.y - from.y, -1, 1)
        );

        Vector2Int current = from;
        while (current != to)
        {
            // Replace or modify room to create connection
            if (grid[current.x, current.y].roomData == null)
            {
                PlaceConnectingRoom(current, direction);
            }
            else
            {
                EnsureRoomHasConnection(current, direction);
            }
            current += direction;
        }
    }

    void PlaceConnectingRoom(Vector2Int coords, Vector2Int direction)
    {
        // Check if position is already occupied
        if (IsPositionOccupied(coords)) return;

        List<RoomData> hallways = mapSettings.zones[0].rooms.FindAll(room =>
            room.shape == RoomData.RoomShape.Hallway &&
            (!room.spawnOnce || !spawnedRooms.Contains(room))
        );

        if (hallways.Count > 0)
        {
            RoomData hallway = hallways[Random.Range(0, hallways.Count)];
            int rotation = GetRotationForDirection(direction);

            grid[coords.x, coords.y].roomData = hallway;
            grid[coords.x, coords.y].rotation = rotation;

            InstantiateRoom(hallway, coords, rotation);
            spawnedRooms.Add(hallway);
        }
    }

    int GetRotationForDirection(Vector2Int direction)
    {
        if (direction.x != 0)
            return 90; // East-West orientation
        return 0;     // North-South orientation
    }

    void EnsureRoomHasConnection(Vector2Int coords, Vector2Int direction)
    {
        GridCell cell = grid[coords.x, coords.y];
        int requiredDirection = DirectionToIndex(direction);

        if (!cell.roomData.entrances[requiredDirection])
        {
            // Replace with a room that has the required connection
            List<RoomData> suitableRooms = mapSettings.zones[0].rooms.FindAll(room =>
                room.entrances[requiredDirection] &&
                (!room.spawnOnce || !spawnedRooms.Contains(room))
            );

            if (suitableRooms.Count > 0)
            {
                RoomData newRoom = suitableRooms[Random.Range(0, suitableRooms.Count)];
                DestroyImmediate(grid[coords.x, coords.y].roomData.roomPrefab);

                grid[coords.x, coords.y].roomData = newRoom;
                InstantiateRoom(newRoom, coords, cell.rotation);
                spawnedRooms.Add(newRoom);
            }
        }
    }

    int DirectionToIndex(Vector2Int direction)
    {
        if (direction.y > 0) return 0;      // North
        if (direction.x > 0) return 1;      // East
        if (direction.y < 0) return 2;      // South
        if (direction.x < 0) return 3;      // West
        return 0;
    }

    void FillDeadEnds()
    {
        for (int x = 0; x < mapSettings.gridSize.x; x++)
        {
            for (int y = 0; y < mapSettings.gridSize.y; y++)
            {
                GridCell cell = grid[x, y];
                if (cell.roomData != null)
                {
                    CheckAndFillDeadEnds(new Vector2Int(x, y));
                }
            }
        }
    }

    void CheckAndFillDeadEnds(Vector2Int coords)
    {
        GridCell cell = grid[coords.x, coords.y];
        for (int i = 0; i < 4; i++)
        {
            int rotatedDirection = (i + (cell.rotation / 90)) % 4;
            if (cell.roomData.entrances[i])
            {
                Vector2Int adjacentCoords = GetAdjacentCoordinates(coords, rotatedDirection);
                if (IsWithinGrid(adjacentCoords) && grid[adjacentCoords.x, adjacentCoords.y].roomData == null)
                {
                    // Check if this position can actually fit a dead end
                    bool canPlaceDeadEnd = true;
                    for (int dir = 0; dir < 4; dir++)
                    {
                        if (dir == (rotatedDirection + 2) % 4) continue; // Skip the direction we're connecting from

                        Vector2Int neighborCoords = GetAdjacentCoordinates(adjacentCoords, dir);
                        if (IsWithinGrid(neighborCoords) && grid[neighborCoords.x, neighborCoords.y].roomData != null)
                        {
                            canPlaceDeadEnd = false;
                            break;
                        }
                    }

                    if (canPlaceDeadEnd)
                    {
                        PlaceDeadEnd(adjacentCoords, rotatedDirection);
                    }
                }
            }
        }
    }

    void PlaceDeadEnd(Vector2Int coords, int entranceDirection)
    {
        // First verify that this position is still empty (could have been filled by another dead end)
        if (grid[coords.x, coords.y].roomData != null) return;

        List<RoomData> deadEnds = mapSettings.zones[0].rooms.FindAll(room =>
            room.shape == RoomData.RoomShape.EndRoom &&
            (!room.spawnOnce || !spawnedRooms.Contains(room))
        );

        if (deadEnds.Count > 0)
        {
            RoomData deadEnd = deadEnds[Random.Range(0, deadEnds.Count)];

            // Verify the dead end can connect in this direction
            int rotation = ((entranceDirection + 2) % 4) * 90; // Rotate to face the connecting room
            int deadEndEntranceIndex = (4 + (entranceDirection - rotation / 90)) % 4;

            if (deadEnd.entrances[deadEndEntranceIndex])
            {
                spawnedRooms.Add(deadEnd);
                grid[coords.x, coords.y].roomData = deadEnd;
                grid[coords.x, coords.y].rotation = rotation;

                InstantiateRoom(deadEnd, coords, rotation);
            }
        }
    }

    bool CanRoomsConnect(RoomData room1, int rotation1, RoomData room2, int rotation2, int direction)
    {
        // Check if room1 has an entrance in the specified direction
        int room1EntranceIndex = (4 + (direction - rotation1 / 90)) % 4;
        if (!room1.entrances[room1EntranceIndex]) return false;

        // Check if room2 has an entrance in the opposite direction
        int room2EntranceIndex = (4 + ((direction + 2) % 4 - rotation2 / 90)) % 4;
        return room2.entrances[room2EntranceIndex];
    }

    void PlaceAdjacentRoom(Vector2Int coords, int entranceDirection)
    {
        // Check if position is already occupied
        if (IsPositionOccupied(coords))
        {
            Debug.LogWarning($"Attempted to place room at occupied position {coords}");
            return;
        }

        // Get the room that we're connecting from
        Vector2Int sourceCoords = GetAdjacentCoordinates(coords, (entranceDirection + 2) % 4);
        GridCell sourceCell = grid[sourceCoords.x, sourceCoords.y];

        List<RoomData> availableRooms = mapSettings.zones[0].rooms.FindAll(room =>
            !room.isExitRoom &&
            (!room.spawnOnce || !spawnedRooms.Contains(room))
        );

        if (availableRooms.Count == 0)
        {
            Debug.LogWarning("No available rooms to place adjacent to the room.");
            return;
        }

        // Filter rooms based on valid connections
        availableRooms = availableRooms.FindAll(room =>
        {
            int rotation = GetValidRotationForConnection(room, coords, entranceDirection, sourceCell.roomData, sourceCell.rotation);
            return rotation != -1;
        });

        if (IsOnGridEdge(coords))
        {
            availableRooms = availableRooms.FindAll(room => room.shape != RoomData.RoomShape.FourWayHallway);
        }

        // Randomly decide whether to try placing a must-spawn room (25% chance)
        bool tryMustSpawn = Random.value < 0.25f && remainingMustSpawnRooms.Count > 0;
        RoomData selectedRoom;

        if (tryMustSpawn)
        {
            // Try to find a must-spawn room that can fit here and respects spawnOnce
            List<RoomData> validMustSpawnRooms = availableRooms.FindAll(room =>
                remainingMustSpawnRooms.Contains(room) &&
                (!room.spawnOnce || !spawnedRooms.Contains(room)));

            if (validMustSpawnRooms.Count > 0)
            {
                selectedRoom = validMustSpawnRooms[Random.Range(0, validMustSpawnRooms.Count)];
                remainingMustSpawnRooms.Remove(selectedRoom);
            }
            else
            {
                selectedRoom = availableRooms[Random.Range(0, availableRooms.Count)];
            }
        }
        else
        {
            selectedRoom = availableRooms[Random.Range(0, availableRooms.Count)];
        }

        spawnedRooms.Add(selectedRoom);

        // If this was a must-spawn room that we just placed, remove it from remaining must-spawns
        if (selectedRoom.mustSpawn)
        {
            remainingMustSpawnRooms.Remove(selectedRoom);
        }

        grid[coords.x, coords.y].roomData = selectedRoom;
        grid[coords.x, coords.y].rotation = GetValidRotationForConnection(selectedRoom, coords, entranceDirection, sourceCell.roomData, sourceCell.rotation);

        InstantiateRoom(selectedRoom, coords, grid[coords.x, coords.y].rotation);
    }

    int GetValidRotationForConnection(RoomData room, Vector2Int coords, int entranceDirection, RoomData sourceRoom, int sourceRotation)
    {
        for (int rotation = 0; rotation < 4; rotation++)
        {
            if (!CanRoomsConnect(sourceRoom, sourceRotation, room, rotation * 90, entranceDirection))
                continue;

            bool isValidRotation = true;

            // Check if this rotation causes any entrances to point outside the grid
            for (int i = 0; i < 4; i++)
            {
                if (room.entrances[i])
                {
                    Vector2Int adjacentCoords = GetAdjacentCoordinates(coords, (i + rotation) % 4);

                    if (!IsWithinGrid(adjacentCoords))
                    {
                        isValidRotation = false;
                        break;
                    }

                    // If adjacent cell has a room, verify connection is valid
                    if (IsWithinGrid(adjacentCoords) && grid[adjacentCoords.x, adjacentCoords.y].roomData != null)
                    {
                        GridCell adjacentCell = grid[adjacentCoords.x, adjacentCoords.y];
                        if (!CanRoomsConnect(room, rotation * 90, adjacentCell.roomData, adjacentCell.rotation, (i + rotation) % 4))
                        {
                            isValidRotation = false;
                            break;
                        }
                    }
                }
            }

            if (isValidRotation)
                return rotation * 90;
        }

        return -1; // No valid rotation found
    }

    Vector2Int GetAdjacentCoordinates(Vector2Int coords, int direction)
    {
        switch (direction)
        {
            case 0: return new Vector2Int(coords.x, coords.y + 1); // North
            case 1: return new Vector2Int(coords.x + 1, coords.y); // East
            case 2: return new Vector2Int(coords.x, coords.y - 1); // South
            case 3: return new Vector2Int(coords.x - 1, coords.y); // West
            default: return coords;
        }
    }

    bool IsWithinGrid(Vector2Int coords)
    {
        return coords.x >= 0 && coords.x < mapSettings.gridSize.x &&
               coords.y >= 0 && coords.y < mapSettings.gridSize.y;
    }

    bool IsOnGridEdge(Vector2Int coords)
    {
        return coords.x == 0 || coords.x == mapSettings.gridSize.x - 1 ||
               coords.y == 0 || coords.y == mapSettings.gridSize.y - 1;
    }

    bool IsPositionOccupied(Vector2Int coords)
    {
        if (!IsWithinGrid(coords)) return true;
        return grid[coords.x, coords.y].roomData != null;
    }

    int GetValidRotation(RoomData room)
    {
        // Try all possible rotations to find one with a south entrance
        for (int rotation = 0; rotation < 4; rotation++)
        {
            // Check if current rotation has a south entrance
            // South is at index 2, rotate counter-clockwise
            int southIndex = (2 - rotation + 4) % 4;
            if (room.entrances[southIndex])
            {
                return rotation * 90;
            }
        }

        Debug.LogError($"Room {room.roomName} has no possible rotation with a south entrance!");
        return 0;
    }

    void InstantiateRoom(RoomData room, Vector2Int coords, int rotation)
    {
        Vector3 position = new Vector3(coords.x * mapSettings.cellSize, 0, coords.y * mapSettings.cellSize);
        Quaternion rot = Quaternion.Euler(0, rotation, 0);
        Instantiate(room.roomPrefab, position, rot, transform);

        // Place doors for this room
        PlaceDoorsForRoom(room, coords, rotation);
    }

    void PlaceDoorsForRoom(RoomData room, Vector2Int coords, int rotation)
    {
        if (mapSettings.doorPrefab == null) return;

        for (int i = 0; i < 4; i++)
        {
            if (!room.entrances[i]) continue;

            int worldDir = (i + (rotation / 90)) % 4;
            Vector2Int adjacentCoords = GetAdjacentCoordinates(coords, worldDir);

            // Skip if adjacent cell is outside grid or has no room
            if (!IsWithinGrid(adjacentCoords) || grid[adjacentCoords.x, adjacentCoords.y].roomData == null)
                continue;

            // Create a unique key for this door position
            string doorKey = GetDoorPositionKey(coords, adjacentCoords);
            if (placedDoorPositions.Contains(doorKey))
                continue;

            // Calculate door position (halfway between rooms)
            Vector3 roomPos = new Vector3(coords.x * mapSettings.cellSize, 0, coords.y * mapSettings.cellSize);
            Vector3 adjacentPos = new Vector3(adjacentCoords.x * mapSettings.cellSize, 0, adjacentCoords.y * mapSettings.cellSize);
            Vector3 doorPosition = Vector3.Lerp(roomPos, adjacentPos, 0.5f);

            // Calculate door rotation
            float doorRotation = worldDir * 90f;
            Quaternion doorRot = Quaternion.Euler(0, doorRotation, 0);

            // Instantiate the door
            Instantiate(mapSettings.doorPrefab, doorPosition, doorRot, transform);
            placedDoorPositions.Add(doorKey);
        }
    }

    string GetDoorPositionKey(Vector2Int pos1, Vector2Int pos2)
    {
        // Create a consistent key regardless of order
        return pos1.x < pos2.x || (pos1.x == pos2.x && pos1.y < pos2.y)
            ? $"{pos1.x},{pos1.y}-{pos2.x},{pos2.y}"
            : $"{pos2.x},{pos2.y}-{pos1.x},{pos1.y}";
    }

    void OnDrawGizmos()
    {
        if (grid == null) return;

        for (int x = 0; x < mapSettings.gridSize.x; x++)
        {
            for (int y = 0; y < mapSettings.gridSize.y; y++)
            {
                Vector3 cellCenter = new Vector3(x * mapSettings.cellSize, 0, y * mapSettings.cellSize);
                Gizmos.DrawWireCube(cellCenter, new Vector3(mapSettings.cellSize, 0.1f, mapSettings.cellSize));

                GridCell cell = grid[x, y];
                if (cell.roomData != null)
                {
                    DrawRoomGizmo(cell, cellCenter);
                }
            }
        }
    }

    void DrawRoomGizmo(GridCell cell, Vector3 cellCenter)
    {
        Gizmos.color = Color.blue;
        Quaternion rotation = Quaternion.Euler(0, cell.rotation, 0);
        Gizmos.matrix = Matrix4x4.TRS(cellCenter, rotation, Vector3.one);

        // Draw lines based on room shape
        switch (cell.roomData.shape)
        {
            case RoomData.RoomShape.Hallway:
                Gizmos.DrawLine(Vector3.forward * -mapSettings.cellSize / 2, Vector3.forward * mapSettings.cellSize / 2);
                break;
            case RoomData.RoomShape.TShapeHallway:
                Gizmos.DrawLine(Vector3.forward * -mapSettings.cellSize / 2, Vector3.forward * mapSettings.cellSize / 2);
                Gizmos.DrawLine(Vector3.zero, Vector3.right * mapSettings.cellSize / 2);
                break;
            case RoomData.RoomShape.FourWayHallway:
                Gizmos.DrawLine(Vector3.forward * -mapSettings.cellSize / 2, Vector3.forward * mapSettings.cellSize / 2);
                Gizmos.DrawLine(Vector3.right * -mapSettings.cellSize / 2, Vector3.right * mapSettings.cellSize / 2);
                break;
            case RoomData.RoomShape.LShapeHallway:
                Gizmos.DrawLine(Vector3.forward * -mapSettings.cellSize / 2, Vector3.zero);
                Gizmos.DrawLine(Vector3.zero, Vector3.right * mapSettings.cellSize / 2);
                break;
        }

        Gizmos.matrix = Matrix4x4.identity;
    }

    public class GridCell
    {
        public Vector2Int coordinates;
        public RoomData roomData;
        public int rotation;

        public GridCell(Vector2Int coordinates)
        {
            this.coordinates = coordinates;
            this.roomData = null;
            this.rotation = 0;
        }
    }
}