using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;   
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;
using Unity.AI.Navigation;

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
    public static MapGenerator instance;

    [Header("References")]
    [SerializeField] private RoomSet roomSet;
    [SerializeField] private MapTemplate[] availableMapTemplates;
    [SerializeField] private Transform generatedMapParent;
    [SerializeField] private Transform generatedDoorParent;
    [SerializeField] private NavMeshSurface navMeshSurface;

    [Header("Grid Settings")]
    [SerializeField] private float cellSize = 20.5f;
    [SerializeField] private float gridOffsetX = 0f;

    [Header("Seed Settings")]
    public string seed = "DefaultSeed";
    public bool useRandomSeed = true;
    public int currentSeed;

    [Header("Status")]
    public bool IsGenerationComplete = false;

    // Internal State
    private MapTemplate selectedMapTemplate;
    private readonly List<GameObject> spawnedRooms = new();
    private readonly Dictionary<Vector2Int, RoomData.Zone> zoneLookup = new();
    private readonly Dictionary<Vector2Int, RoomData> placedMandatoryRooms = new();
    private readonly HashSet<RoomData> usedUniqueRooms = new();

    // Debug Data
    private readonly List<(Vector2Int pos, Color color)> gizmoCells = new();

    private void Awake() {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start() {
        StartCoroutine(GenerateMapSequence());
    }

    public IEnumerator GenerateMapSequence() {
        IsGenerationComplete = false;
        Cleanup();

        // 1. Setup Seed
        if (useRandomSeed) currentSeed = Random.Range(0, 99999);
        else currentSeed = seed.GetHashCode();
        Random.InitState(currentSeed);

        selectedMapTemplate = availableMapTemplates[Random.Range(0, availableMapTemplates.Length)];

        // 2. Build Data
        BuildGridData();
        PlaceMandatoryRooms();

        // 3. Spawn Rooms and Wait
        yield return StartCoroutine(SpawnPlacedRoomsRoutine());

        // 4. Bake NavMesh
        if (navMeshSurface != null) {
            Debug.Log("Baking NavMesh...");
            navMeshSurface.BuildNavMesh();
            yield return null;
        }

        // 5. Spawn Doors
        SpawnDoors();

        IsGenerationComplete = true;
        Debug.Log("<color=green>Map Generation Sequence Finished!</color>");
    }

    private IEnumerator SpawnPlacedRoomsRoutine() {
        List<AsyncOperationHandle<GameObject>> handles = new();

        foreach (var placement in selectedMapTemplate.roomPlacements) {
            RoomData roomToSpawn = placedMandatoryRooms.TryGetValue(placement.gridPosition, out RoomData mandatory)
                ? mandatory
                : GetRandomValidRoom(placement.requiredShape, GetZoneAtPosition(placement.gridPosition));

            if (roomToSpawn == null || !roomToSpawn.roomPrefab.RuntimeKeyIsValid()) continue;

            Vector3 worldPos = GridToWorld(placement.gridPosition);
            Quaternion rotation = Quaternion.Euler(0f, placement.rotation, 0f);

            var handle = roomToSpawn.roomPrefab.InstantiateAsync(worldPos, rotation, generatedMapParent);
            handles.Add(handle);

            handle.Completed += (op) => {
                if (op.Status == AsyncOperationStatus.Succeeded) spawnedRooms.Add(op.Result);
            };
        }

        // Wait until ALL room addressables are finished loading
        while (handles.Count > 0) {
            for (int i = handles.Count - 1; i >= 0; i--) {
                if (handles[i].IsDone) handles.RemoveAt(i);
            }
            yield return null;
        }
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
        SpawnDoors();
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

    #region PHASE 4: Spawn the doors
    private static readonly Dictionary<RoomData.RoomShape, int[]> BaseExits = new() {
        { RoomData.RoomShape.TwoWay, new[] { 0, 180 } },
        { RoomData.RoomShape.ThreeWay, new[] { 270, 180, 90 } },
        { RoomData.RoomShape.Corner, new[] { 180, 90 } },
        { RoomData.RoomShape.DeadEnd, new[] { 180 } },
        { RoomData.RoomShape.FourWay, new[] { 0, 90, 180, 270 } },
        { RoomData.RoomShape.Checkpoint, new[] { 0, 180 } }
    };

    private void SpawnDoors() {
        HashSet<string> processedEdges = new();

        foreach (var placement in selectedMapTemplate.roomPlacements) {
            Vector2Int currentPos = placement.gridPosition;
            List<int> currentExits = GetWorldExits(placement.requiredShape, placement.rotation);

            foreach (int localAngle in currentExits) {
                Vector2Int neighborPos = currentPos + AngleToDirection(localAngle);

                string edgeKey = GetEdgeKey(currentPos, neighborPos);
                if (processedEdges.Contains(edgeKey)) continue;

                // Check if neighbor exists and has a matching exit
                var neighborPlacement = GetPlacementAt(neighborPos);
                if (neighborPlacement != null) {
                    List<int> neighborExits = GetWorldExits(neighborPlacement.requiredShape, neighborPlacement.rotation);
                    int oppositeAngle = (localAngle + 180) % 360;

                    if (neighborExits.Contains(oppositeAngle)) {
                        SpawnDoorBetween(currentPos, neighborPos, localAngle);
                        processedEdges.Add(edgeKey);
                    }
                }
            }
        }
    }

    private void SpawnDoorBetween(Vector2Int posA, Vector2Int posB, int angleFromA) {
        RoomData.Zone zone = GetZoneAtPosition(posA);
        AssetReferenceGameObject doorPrefab = null;

        // Find the door prefab for this zone from the template
        foreach (var zT in selectedMapTemplate.zones) {
            if (zT.zoneType == zone) {
                doorPrefab = zT.zoneDoor;
                break;
            }
        }

        if (doorPrefab == null || !doorPrefab.RuntimeKeyIsValid()) return;

        // Calculate Midpoint
        Vector3 worldPosA = GridToWorld(posA);
        Vector3 worldPosB = GridToWorld(posB);
        Vector3 doorPosition = (worldPosA + worldPosB) / 2f;

        // Rotation: Point the door along the axis of the exit
        Quaternion doorRotation = Quaternion.Euler(0, angleFromA, 0);

        doorPrefab.InstantiateAsync(doorPosition, doorRotation, generatedDoorParent);
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

    #region Door Helpers :)
    private List<int> GetWorldExits(RoomData.RoomShape shape, int rotation) {
        List<int> worldExits = new();
        if (BaseExits.TryGetValue(shape, out int[] angles)) {
            foreach (int angle in angles) {
                // (Base + Object Rotation) % 360 gives world-space exit direction
                worldExits.Add((angle + rotation) % 360);
            }
        }
        return worldExits;
    }

    private Vector2Int AngleToDirection(int angle) {
        return angle switch {
            0 => new Vector2Int(0, 1),    // North
            90 => new Vector2Int(1, 0),   // East
            180 => new Vector2Int(0, -1), // South
            270 => new Vector2Int(-1, 0), // West
            _ => Vector2Int.zero
        };
    }

    private RoomPlacement GetPlacementAt(Vector2Int pos) {
        foreach (var p in selectedMapTemplate.roomPlacements) {
            if (p.gridPosition == pos) return p;
        }
        return null;
    }

    private string GetEdgeKey(Vector2Int a, Vector2Int b) {
        if (a.x < b.x || (a.x == b.x && a.y < b.y)) return $"{a}:{b}";
        return $"{b}:{a}";
    }
    #endregion
}