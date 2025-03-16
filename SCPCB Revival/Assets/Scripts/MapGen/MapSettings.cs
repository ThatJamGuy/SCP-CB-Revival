using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Map Settings", menuName = "Map Generation/Map Settings")]
public class MapSettings : ScriptableObject
{
    [Header("Grid Configuration")]
    public float cellSize = 20.5f;
    public Vector2Int gridSize = new Vector2Int(10, 10);
    public Vector2Int startingCoords = new Vector2Int(0, 0);

    [Header("Zone Configuration")]
    public List<ZoneConnection> zoneConnections = new List<ZoneConnection>();

    [Tooltip("Zones in generation order")]
    public List<Zone> zones = new List<Zone>();

    [System.Serializable]
    public class Zone
    {
        [Tooltip("All rooms available for this zone")]
        public List<RoomData> rooms = new List<RoomData>();
    }

    [System.Serializable]
    public class ZoneConnection
    {
        public int fromZone;
        public int toZone;
        [Range(1, 4)] public int connectionCount = 1;
    }

    // Helper methods for generator access
    public List<RoomData> GetRequiredRoomsForZone(int zoneIndex)
    {
        return zones[zoneIndex].rooms.FindAll(room => room.mustSpawn);
    }

    public List<RoomData> GetExitRoomsForZone(int zoneIndex)
    {
        return zones[zoneIndex].rooms.FindAll(room => room.isExitRoom);
    }

    public List<RoomData> GetStandardRoomsForZone(int zoneIndex)
    {
        return zones[zoneIndex].rooms.FindAll(room =>
            !room.mustSpawn && !room.isExitRoom
        );
    }
}