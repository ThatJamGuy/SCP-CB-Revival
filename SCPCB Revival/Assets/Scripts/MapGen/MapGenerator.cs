using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;

public class MapGenerator : MonoBehaviour
{
    public MapSettings mapSettings;

    private GridCell[,] grid;
    private List<RoomData> spawnedRooms = new List<RoomData>();

    void Start()
    {
        InitializeGrid();
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

        Debug.Log("Map cleared successfully.");
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
        Vector2Int startCoords = mapSettings.startingCoords;
        GridCell startCell = grid[startCoords.x, startCoords.y];

        // Iterate through all entrances of the starting room
        for (int i = 0; i < 4; i++)
        {
            if (startCell.roomData.entrances[i])
            {
                Vector2Int adjacentCoords = GetAdjacentCoordinates(startCoords, i);
                if (IsWithinGrid(adjacentCoords) && grid[adjacentCoords.x, adjacentCoords.y].roomData == null)
                {
                    PlaceAdjacentRoom(adjacentCoords, i);
                }
            }
        }
    }

    void PlaceAdjacentRoom(Vector2Int coords, int entranceDirection)
    {
        List<RoomData> availableRooms = mapSettings.zones[0].rooms.FindAll(room =>
            !room.isExitRoom && // Exclude exit rooms
            (!room.spawnOnce || !spawnedRooms.Contains(room)) // Allow rooms that are not marked as "spawn once" or haven't been spawned yet
        );

        if (availableRooms.Count == 0)
        {
            Debug.LogWarning("No available rooms to place adjacent to the starting room.");
            return;
        }

        // Exclude 4-way rooms if the coordinates are on the edge of the grid
        if (IsOnGridEdge(coords))
        {
            availableRooms = availableRooms.FindAll(room => room.shape != RoomData.RoomShape.FourWayHallway);
        }

        // Prioritize rooms marked as "must spawn"
        List<RoomData> mustSpawnRooms = availableRooms.FindAll(room => room.mustSpawn);
        if (mustSpawnRooms.Count > 0)
        {
            availableRooms = mustSpawnRooms;
        }

        if (availableRooms.Count == 0)
        {
            Debug.LogWarning("No valid rooms to place at the edge of the grid.");
            return;
        }

        RoomData adjacentRoom = availableRooms[Random.Range(0, availableRooms.Count)];
        spawnedRooms.Add(adjacentRoom);

        // Calculate the required rotation for the adjacent room
        int rotation = GetValidRotationForEdge(adjacentRoom, coords, entranceDirection);
        grid[coords.x, coords.y].roomData = adjacentRoom;
        grid[coords.x, coords.y].rotation = rotation;

        InstantiateRoom(adjacentRoom, coords, rotation);

        Debug.Log($"Placed adjacent room '{adjacentRoom.roomName}' at {coords} with rotation {rotation} degrees.");
    }

    bool IsOnGridEdge(Vector2Int coords)
    {
        return coords.x == 0 || coords.x == mapSettings.gridSize.x - 1 ||
               coords.y == 0 || coords.y == mapSettings.gridSize.y - 1;
    }

    int GetValidRotationForEdge(RoomData room, Vector2Int coords, int entranceDirection)
    {
        // The adjacent room's entrance must face the opposite direction of the starting room's exit
        int requiredEntrance = (entranceDirection + 2) % 4; // Opposite direction

        // Try all rotations to find one that aligns the required entrance and doesn't point outside the grid
        for (int rotation = 0; rotation < 4; rotation++)
        {
            int rotatedEntrance = (requiredEntrance - rotation + 4) % 4;
            if (room.entrances[rotatedEntrance])
            {
                // Check if this rotation causes any entrances to point outside the grid
                bool isValidRotation = true;
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
                    }
                }

                if (isValidRotation)
                {
                    return rotation * 90;
                }
            }
        }

        Debug.LogError($"Room {room.roomName} has no valid rotation for adjacent placement without pointing outside the grid!");
        return 0;
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