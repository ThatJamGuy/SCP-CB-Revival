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
    }

    void ClearMap()
    {
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }
        spawnedRooms.Clear();
        grid = null;
    }

    void PlaceStartingRoom()
    {
        List<RoomData> availableRooms = mapSettings.zones[0].rooms.FindAll(room =>
            !room.isExitRoom && room.shape != RoomData.RoomShape.EndRoom && (room.mustSpawn || !spawnedRooms.Contains(room))
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
