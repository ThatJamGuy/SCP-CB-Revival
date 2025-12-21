using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[System.Serializable]
public class RoomSet {
    public RoomData[] twoWayRooms;
    public RoomData[] cornerRooms;
    public RoomData[] threeWayRooms;
    public RoomData[] fourWayRooms;
    public RoomData[] deadEndRooms;

    public RoomData[] GetRoomsByShape(RoomData.RoomShape shape) {
        return shape switch {
            RoomData.RoomShape.TwoWay => twoWayRooms,
            RoomData.RoomShape.Corner => cornerRooms,
            RoomData.RoomShape.ThreeWay => threeWayRooms,
            RoomData.RoomShape.FourWay => fourWayRooms,
            RoomData.RoomShape.DeadEnd => deadEndRooms,
            _ => null
        };
    }
}

public class MapGenerator : MonoBehaviour {
    [Header("References")]
    [SerializeField] private RoomSet roomSet;
    [SerializeField] private MapTemplate[] availableMapTemplates;
    [SerializeField] private Transform generatedMapParent;

    [Header("Grid Settings")]
    [SerializeField] private float cellSize = 20.5f;
    [SerializeField] private float gridOffsetX = 0f;

    [Header("Seed Settings")]
    [SerializeField] private string seed = "DefaultSeed";
    [SerializeField] private bool useRandomSeed = true;
    [SerializeField] private int currentSeed;

    // Internal State
    private MapTemplate selectedMapTemplate;
    private readonly List<GameObject> spawnedRooms = new();
    private readonly Dictionary<Vector2Int, RoomData.Zone> zoneLookup = new();
    private readonly Dictionary<Vector2Int, RoomData> placedMandatoryRooms = new();
    private readonly HashSet<RoomData> usedUniqueRooms = new();

    // Debug Data
    private readonly List<(Vector2Int pos, Color color)> gizmoCells = new();

    private void Start() {
        GenerateMap();
    }

    public void GenerateMap() {
        Cleanup();

        // Setup the seed stuff
        if (useRandomSeed) {
            currentSeed = Random.Range(0, 99999);
        }
        else {
            currentSeed = seed.GetHashCode(); // Map gen cant use strings, so convert any user seeds to a number
        }
        Random.InitState(currentSeed);
        Debug.Log($"Generating map with Seed: {currentSeed}");

        if (availableMapTemplates == null || availableMapTemplates.Length == 0) {
            Debug.LogError("No Map Templates assigned to MapGenerator!");
            return;
        }

        selectedMapTemplate = availableMapTemplates[Random.Range(0, availableMapTemplates.Length)];

        BuildGridData();
        PlaceMandatoryRooms();
        SpawnPlacedRooms();
    }

    private void Cleanup() {
        foreach (var room in spawnedRooms) {
            if (room != null) Addressables.ReleaseInstance(room);
        }
        spawnedRooms.Clear();
        placedMandatoryRooms.Clear();
        usedUniqueRooms.Clear();
        zoneLookup.Clear();
        gizmoCells.Clear();
    }

    #region PHASE 1: Grid Mapping
    private void BuildGridData() {
        if (selectedMapTemplate == null) return;

        int currentY = 0;
        foreach (var zone in selectedMapTemplate.zones) {
            // Map Standard Zone Rows
            for (int y = 0; y < zone.zoneHeight; y++) {
                for (int x = 0; x < zone.zoneWidth; x++) {
                    Vector2Int pos = new Vector2Int(x, currentY + y);
                    zoneLookup[pos] = zone.zoneType;
                    gizmoCells.Add((pos, zone.debugColor));
                }
            }
            currentY += zone.zoneHeight;

            // Map Checkpoint Rows
            if (zone.checkpointRoomVariant != null) {
                for (int x = 0; x < zone.zoneWidth; x++) {
                    Vector2Int pos = new Vector2Int(x, currentY);
                    zoneLookup[pos] = zone.zoneType;
                    gizmoCells.Add((pos, Color.black));
                }
                currentY += 1;
            }
        }
    }
    #endregion

    #region PHASE 2: Logic Stuff
    private void PlaceMandatoryRooms() {
        List<RoomData> mandatoryRooms = GetAllRooms().FindAll(r => r.mustSpawn);

        foreach (var room in mandatoryRooms) {
            var validPositions = GetValidPositionsForRoom(room);

            if (validPositions.Count == 0) {
                Debug.LogWarning($"Could not find valid placement for mandatory room: {room.name}");
                continue;
            }

            Vector2Int selectedPos = validPositions[Random.Range(0, validPositions.Count)];
            placedMandatoryRooms[selectedPos] = room;

            if (room.isUnique) usedUniqueRooms.Add(room);
        }
    }
    #endregion

    #region PHASE 3: Room Spawning / Placing
    private void SpawnPlacedRooms() {
        if (selectedMapTemplate.roomPlacements == null) return;

        foreach (var placement in selectedMapTemplate.roomPlacements) {
            RoomData roomToSpawn = null;

            // Priority 1: Mandatory Pre-placed rooms
            if (placedMandatoryRooms.TryGetValue(placement.gridPosition, out RoomData mandatory)) {
                roomToSpawn = mandatory;
            }
            // Priority 2: Random selection based on shape/zone
            else {
                roomToSpawn = GetRandomValidRoom(placement.requiredShape, GetZoneAtPosition(placement.gridPosition));
            }

            if (roomToSpawn == null) continue;

            SpawnRoom(roomToSpawn, placement.gridPosition, placement.rotation);
        }
    }

    private void SpawnRoom(RoomData room, Vector2Int gridPos, int rotationY) {
        if (!room.roomPrefab.RuntimeKeyIsValid()) return;

        Vector3 worldPos = GridToWorld(gridPos);
        Quaternion rotation = Quaternion.Euler(0f, rotationY, 0f);

        var handle = room.roomPrefab.InstantiateAsync(worldPos, rotation, generatedMapParent);
        handle.Completed += (op) => {
            if (op.Status == AsyncOperationStatus.Succeeded) spawnedRooms.Add(op.Result);
        };
    }
    #endregion

    #region Helpers :)
    private RoomData GetRandomValidRoom(RoomData.RoomShape shape, RoomData.Zone zone) {
        if (shape == RoomData.RoomShape.Checkpoint) {
            return GetCheckpointForZone(zone);
        }

        RoomData[] candidates = roomSet.GetRoomsByShape(shape);
        if (candidates == null) return null;

        List<RoomData> validPool = new();
        foreach (var r in candidates) {
            if (r.roomZone != zone) continue;
            if (r.isUnique && usedUniqueRooms.Contains(r)) continue;
            validPool.Add(r);
        }

        if (validPool.Count == 0) return null;

        RoomData selected = validPool[Random.Range(0, validPool.Count)];
        if (selected.isUnique) usedUniqueRooms.Add(selected);

        return selected;
    }

    private List<Vector2Int> GetValidPositionsForRoom(RoomData room) {
        List<Vector2Int> valid = new();
        foreach (var p in selectedMapTemplate.roomPlacements) {
            if (p.requiredShape != room.roomShape) continue;
            if (GetZoneAtPosition(p.gridPosition) != room.roomZone) continue;
            if (placedMandatoryRooms.ContainsKey(p.gridPosition)) continue;

            valid.Add(p.gridPosition);
        }
        return valid;
    }

    private RoomData GetCheckpointForZone(RoomData.Zone zone) {
        foreach (var zT in selectedMapTemplate.zones) {
            if (zT.zoneType == zone) return zT.checkpointRoomVariant;
        }
        return null;
    }

    private RoomData.Zone GetZoneAtPosition(Vector2Int pos) {
        return zoneLookup.TryGetValue(pos, out var zone) ? zone : RoomData.Zone.LCZ;
    }

    private List<RoomData> GetAllRooms() {
        List<RoomData> all = new();
        System.Action<RoomData[]> addIfNotNull = (arr) => { if (arr != null) all.AddRange(arr); };

        addIfNotNull(roomSet.twoWayRooms);
        addIfNotNull(roomSet.cornerRooms);
        addIfNotNull(roomSet.threeWayRooms);
        addIfNotNull(roomSet.fourWayRooms);
        addIfNotNull(roomSet.deadEndRooms);
        return all;
    }

    private Vector3 GridToWorld(Vector2Int gridPos) {
        return new Vector3(gridPos.x * cellSize + gridOffsetX, 0f, gridPos.y * cellSize);
    }

    private void OnDrawGizmos() {
        // Draw Layout Grid
        foreach (var cell in gizmoCells) {
            Gizmos.color = cell.color;
            Vector3 worldPos = GridToWorld(cell.pos);
            Gizmos.DrawWireCube(worldPos, new Vector3(cellSize, 0.1f, cellSize));
        }

        // Draw Mandatory Reservations (Solid Cubes)
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f); // Semi-transparent Orange
        foreach (var kvp in placedMandatoryRooms) {
            Gizmos.DrawCube(GridToWorld(kvp.Key), new Vector3(cellSize * 0.5f, 0.5f, cellSize * 0.5f));
        }
    }
    #endregion
}