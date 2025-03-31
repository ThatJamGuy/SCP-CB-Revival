using UnityEngine;
using NaughtyAttributes;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Zone", menuName = "Map Generator/Zone Data")]
public class ZoneData : ScriptableObject
{
    [Header("Zone Info")]
    [Tooltip("ID of the zone (I.E. 1 - LCZ, 2 - HCZ, 3 - ENZ)")]
    public int zoneID;

    [Tooltip("The size of the zone on the x and y axis from a top down perspective.")]
    public Vector2 zoneGridSize;

    [Tooltip("Rooms that the zone will contain.")]
    public List<RoomData> roomTable = new List<RoomData>();

    [Header("Advanced Settings")]
    [Tooltip("If true, the zone will start with a random viable room at a specific location.")]
    public bool createStartingRoom = false;

    [Tooltip("Rooms that are allowed to be used as the starting room.")]
    [ShowIf(nameof(createStartingRoom))] public List<RoomData> viableStartingRooms = new List<RoomData>();

    [Tooltip("Which cell on the grid that the starting room will spawn at.")]
    [ShowIf(nameof(createStartingRoom))] public Vector2 startingCellLocation;

    private void OnValidate()
    {
        if (zoneID <= 0)
        {
            Debug.LogWarning($"{nameof(zoneID)} should be a positive integer in {nameof(ZoneData)}: {name}");
        }

        if (roomTable == null || roomTable.Count == 0)
        {
            Debug.LogWarning($"{nameof(roomTable)} should not be empty in {nameof(ZoneData)}: {name}");
        }

        if (createStartingRoom)
        {
            if (viableStartingRooms == null || viableStartingRooms.Count == 0)
            {
                Debug.LogWarning($"{nameof(viableStartingRooms)} should not be empty when {nameof(createStartingRoom)} is true in {nameof(ZoneData)}: {name}");
            }

            if (startingCellLocation.x < 0 || startingCellLocation.y < 0)
            {
                Debug.LogWarning($"{nameof(startingCellLocation)} should have non-negative coordinates in {nameof(ZoneData)}: {name}");
            }
        }
    }
}
