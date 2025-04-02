using UnityEngine;
using NaughtyAttributes;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "New Zone", menuName = "Map Generator/Zone Data")]
public class ZoneData : ScriptableObject
{
    [Header("Zone Info")]
    [Tooltip("Unique identifier for the zone (e.g., 1 - LCZ, 2 - HCZ, 3 - ENZ). Must be positive.")]
    [SerializeField, Min(1)] private int _zoneID = 1;
    public int ZoneID => _zoneID;

    [Tooltip("The size of the zone in grid cells (x,y). Both values must be positive.")]
    [SerializeField, Min(1)] private Vector2Int _zoneGridSize = Vector2Int.one;
    public Vector2Int ZoneGridSize => _zoneGridSize;

    [Tooltip("List of all possible rooms that can appear in this zone.")]
    [SerializeField] private List<RoomData> _roomTable = new List<RoomData>();
    public IReadOnlyList<RoomData> RoomTable => _roomTable;

    [Header("Zone Connections")]
    [Tooltip("If enabled, this zone will connect to another zone via connection rooms.")]
    [SerializeField] private bool _connectsToNextZone = false;
    public bool ConnectsToNextZone => _connectsToNextZone;

    [Tooltip("ID of the zone this connects to. Must be different from current zone ID.")]
    [ShowIf(nameof(_connectsToNextZone)), Min(1)]
    [SerializeField] private int _nextZoneID = 1;
    public int NextZoneID => _nextZoneID;

    [Tooltip("Number of connection points between zones (1-4).")]
    [ShowIf(nameof(_connectsToNextZone)), Range(1, 4)]
    [SerializeField] private int _amountOfConnections = 2;
    public int AmountOfConnections => _amountOfConnections;

    [Tooltip("Prefab reference for rooms that connect to the next zone.")]
    [ShowIf(nameof(_connectsToNextZone))]
    [SerializeField] private RoomData _toNextZoneConnector;
    public RoomData ToNextZoneConnector => _toNextZoneConnector;

    [Tooltip("Prefab references for surface exit rooms. Each can only be placed once.")]
    [HideIf(nameof(_connectsToNextZone))]
    [SerializeField] private RoomData[] _surfaceExits;
    public IReadOnlyList<RoomData> SurfaceExits => _surfaceExits;

    [Header("Starting Room Settings")]
    [Tooltip("If enabled, the zone will begin with a specific room at a defined location.")]
    [SerializeField] private bool _createStartingRoom = false;
    public bool CreateStartingRoom => _createStartingRoom;

    [Tooltip("Rooms that can be used as the starting room.")]
    [ShowIf(nameof(_createStartingRoom))]
    [SerializeField] private List<RoomData> _viableStartingRooms = new List<RoomData>();
    public IReadOnlyList<RoomData> ViableStartingRooms => _viableStartingRooms;

    [Tooltip("Grid coordinates for the starting room (0-based).")]
    [ShowIf(nameof(_createStartingRoom))]
    [SerializeField] private Vector2Int _startingCellLocation;
    public Vector2Int StartingCellLocation => _startingCellLocation;

    private void OnEnable()
    {
        // Ensure collections are never null
        _roomTable ??= new List<RoomData>();
        _viableStartingRooms ??= new List<RoomData>();
        _surfaceExits ??= Array.Empty<RoomData>();
    }

    private void OnValidate()
    {
        // Zone ID validation
        if (_zoneID <= 0)
        {
            Debug.LogWarning($"{nameof(_zoneID)} must be positive in {nameof(ZoneData)}: {name}");
            _zoneID = 1;
        }

        // Grid size validation
        if (_zoneGridSize.x <= 0 || _zoneGridSize.y <= 0)
        {
            Debug.LogWarning($"{nameof(_zoneGridSize)} must have positive values in {nameof(ZoneData)}: {name}");
            _zoneGridSize = Vector2Int.Max(Vector2Int.one, _zoneGridSize);
        }

        // Room table validation
        if (_roomTable == null || _roomTable.Count == 0)
        {
            Debug.LogWarning($"{nameof(_roomTable)} cannot be empty in {nameof(ZoneData)}: {name}");
        }

        // Zone connection validation
        if (_connectsToNextZone)
        {
            if (_nextZoneID == _zoneID)
            {
                Debug.LogWarning($"{nameof(_nextZoneID)} cannot be the same as {nameof(_zoneID)} in {nameof(ZoneData)}: {name}");
            }

            if (_toNextZoneConnector == null)
            {
                Debug.LogWarning($"{nameof(_toNextZoneConnector)} is required when {nameof(_connectsToNextZone)} is true in {nameof(ZoneData)}: {name}");
            }
        }
        else if (_surfaceExits == null || _surfaceExits.Length == 0)
        {
            Debug.LogWarning($"{nameof(_surfaceExits)} cannot be empty when not connecting to another zone in {nameof(ZoneData)}: {name}");
        }

        // Starting room validation
        if (_createStartingRoom)
        {
            if (_viableStartingRooms == null || _viableStartingRooms.Count == 0)
            {
                Debug.LogWarning($"{nameof(_viableStartingRooms)} cannot be empty when {nameof(_createStartingRoom)} is true in {nameof(ZoneData)}: {name}");
            }

            if (_startingCellLocation.x < 0 || _startingCellLocation.y < 0 ||
                _startingCellLocation.x >= _zoneGridSize.x || _startingCellLocation.y >= _zoneGridSize.y)
            {
                Debug.LogWarning($"{nameof(_startingCellLocation)} must be within zone bounds (0-{_zoneGridSize.x - 1}, 0-{_zoneGridSize.y - 1}) in {nameof(ZoneData)}: {name}");
                _startingCellLocation = new Vector2Int(
                    Mathf.Clamp(_startingCellLocation.x, 0, _zoneGridSize.x - 1),
                    Mathf.Clamp(_startingCellLocation.y, 0, _zoneGridSize.y - 1)
                );
            }
        }

        // Duplicate Room Prevention Validation
        if (_roomTable != null && _roomTable.Count > 0)
        {
            var uniqueRooms = new HashSet<RoomData>(_roomTable);
            if (uniqueRooms.Count != _roomTable.Count)
            {
                Debug.LogWarning($"{nameof(_roomTable)} contains duplicate rooms in {nameof(ZoneData)}: {name}");
            }
        }
    }
}