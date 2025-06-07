using NaughtyAttributes;
using UnityEngine;

namespace vectorarts.scpcbr
{
    [CreateAssetMenu(fileName = "New Zone", menuName = "MapGen/Zone")]
    public class Zone : ScriptableObject
    {
        public int zoneID;
        public GameObject zoneDoorPrefab;
        public Rooms rooms;

        [Header("Room Settings")]
        public bool utilizeStartingRoom;
        [ShowIf(nameof(utilizeStartingRoom))] public Vector2 startingRoomCellPosition;

        [Header("Zone Connections")]
        public bool connectsToNextZone;
        [ShowIf(nameof(connectsToNextZone))] public Room zoneConnectorRoom;
        [ShowIf(nameof(connectsToNextZone))] public int numberOfZoneConnectors = 2;
        [ShowIf(nameof(connectsToNextZone))] public int nextZoneID;
        [HideIf(nameof(connectsToNextZone))] public Room[] surfaceExitRooms;

        [System.Serializable]
        public struct Rooms {
            public Room[] normalRooms;
            public Room[] requiredRooms;
        }
    }
}