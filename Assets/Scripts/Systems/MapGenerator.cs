using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
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
    public static MapGenerator Instance { get; private set; }

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
    private readonly Dictionary<AssetReferenceGameObject, AsyncOperationHandle<GameObject>> loadedRoomAssets = new();
    private readonly List<GameObject> spawnedDoors = new();
    private readonly Dictionary<AssetReferenceGameObject, AsyncOperationHandle<GameObject>> loadedDoorAssets = new();
    private readonly Dictionary<Vector2Int, RoomData.Zone> zoneLookup = new();
    private readonly Dictionary<Vector2Int, RoomData> placedMandatoryRooms = new();
    private readonly HashSet<RoomData> usedUniqueRooms = new();

    // Debug Data
    private readonly List<(Vector2Int pos, Color color)> gizmoCells = new();

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start() {
        if (DataSaver.DataFileExists("save.json")) {
            var saveData = DataSaver.Load<SaveData>("save.json");
            seed = saveData.currentMapSeed;

            if (string.IsNullOrEmpty(seed))
                useRandomSeed = true;
            else
                useRandomSeed = false;
        }

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

        // 5. Spawn Doors and Wait
        yield return StartCoroutine(SpawnDoorsRoutine());

        IsGenerationComplete = true;
        Debug.Log("<color=green>Map Generation Sequence Finished!</color>");
    }

    private IEnumerator SpawnPlacedRoomsRoutine() {
        // Decide which room goes in each placement up front, so we know exactly
        // which addressable prefabs actually need to be loaded.
        List<(RoomPlacement placement, RoomData room)> placementRooms = new();
        HashSet<AssetReferenceGameObject> prefabsToLoad = new();

        foreach (var placement in selectedMapTemplate.roomPlacements) {
            RoomData roomToSpawn = placedMandatoryRooms.TryGetValue(placement.gridPosition, out RoomData mandatory)
                ? mandatory
                : GetRandomValidRoom(placement.requiredShape, GetZoneAtPosition(placement.gridPosition));

            if (roomToSpawn == null || !roomToSpawn.roomPrefab.RuntimeKeyIsValid()) continue;

            placementRooms.Add((placement, roomToSpawn));
            prefabsToLoad.Add(roomToSpawn.roomPrefab);
        }

        // Load every distinct prefab once - this is the only part that stays async,
        // since Addressables may need to pull the bundle from disk or network.
        List<AsyncOperationHandle<GameObject>> loadHandles = new(prefabsToLoad.Count);
        foreach (var prefabRef in prefabsToLoad) {
            var handle = prefabRef.LoadAssetAsync();
            loadedRoomAssets[prefabRef] = handle;
            loadHandles.Add(handle);
        }

        foreach (var handle in loadHandles) {
            yield return handle;
        }

        // Every prefab is resident in memory now, so placing a room is just a plain,
        // synchronous Instantiate - no need to spread it across frames like InstantiateAsync does.
        foreach (var (placement, room) in placementRooms) {
            AsyncOperationHandle<GameObject> handle = loadedRoomAssets[room.roomPrefab];
            if (handle.Status != AsyncOperationStatus.Succeeded) {
                Debug.LogWarning($"Failed to load addressable prefab for room: {room.name}");
                continue;
            }

            Vector3 worldPos = GridToWorld(placement.gridPosition);
            Quaternion rotation = Quaternion.Euler(0f, placement.rotation, 0f);

            GameObject instance = Instantiate(handle.Result, worldPos, rotation, generatedMapParent);
            spawnedRooms.Add(instance);
        }
    }

    private void Cleanup() {
        foreach (var room in spawnedRooms) {
            if (room != null) Destroy(room);
        }
        spawnedRooms.Clear();

        foreach (var door in spawnedDoors) {
            if (door != null) Destroy(door);
        }
        spawnedDoors.Clear();

        // Release every addressable load handle now that its instances are gone -
        // this is what actually decrements Addressables' ref count for the bundle.
        foreach (var handle in loadedRoomAssets.Values) {
            Addressables.Release(handle);
        }
        loadedRoomAssets.Clear();

        foreach (var handle in loadedDoorAssets.Values) {
            Addressables.Release(handle);
        }
        loadedDoorAssets.Clear();

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

    #region PHASE 3: Spawn the doors
    private static readonly Dictionary<RoomData.RoomShape, int[]> BaseExits = new() {
        { RoomData.RoomShape.TwoWay, new[] { 0, 180 } },
        { RoomData.RoomShape.ThreeWay, new[] { 270, 180, 90 } },
        { RoomData.RoomShape.Corner, new[] { 180, 90 } },
        { RoomData.RoomShape.DeadEnd, new[] { 180 } },
        { RoomData.RoomShape.FourWay, new[] { 0, 90, 180, 270 } },
        { RoomData.RoomShape.Checkpoint, new[] { 0, 180 } }
    };

    private IEnumerator SpawnDoorsRoutine() {
        // Walk every placement's exits and work out where a door is actually needed,
        // same adjacency logic as before - just collecting instead of spawning immediately.
        HashSet<string> processedEdges = new();
        List<(Vector3 position, Quaternion rotation, AssetReferenceGameObject prefab)> doorsToPlace = new();
        HashSet<AssetReferenceGameObject> prefabsToLoad = new();

        foreach (var placement in selectedMapTemplate.roomPlacements) {
            Vector2Int currentPos = placement.gridPosition;
            List<int> currentExits = GetWorldExits(placement.requiredShape, placement.rotation);

            foreach (int localAngle in currentExits) {
                Vector2Int neighborPos = currentPos + AngleToDirection(localAngle);

                string edgeKey = GetEdgeKey(currentPos, neighborPos);
                if (processedEdges.Contains(edgeKey)) continue;

                // Check if neighbor exists and has a matching exit
                var neighborPlacement = GetPlacementAt(neighborPos);
                if (neighborPlacement == null) continue;

                List<int> neighborExits = GetWorldExits(neighborPlacement.requiredShape, neighborPlacement.rotation);
                int oppositeAngle = (localAngle + 180) % 360;
                if (!neighborExits.Contains(oppositeAngle)) continue;

                processedEdges.Add(edgeKey);

                AssetReferenceGameObject doorPrefab = GetDoorPrefabForZone(GetZoneAtPosition(currentPos));
                if (doorPrefab == null || !doorPrefab.RuntimeKeyIsValid()) continue;

                // Midpoint between the two rooms, rotated to point along the exit axis
                Vector3 doorPosition = (GridToWorld(currentPos) + GridToWorld(neighborPos)) / 2f;
                Quaternion doorRotation = Quaternion.Euler(0f, localAngle, 0f);

                doorsToPlace.Add((doorPosition, doorRotation, doorPrefab));
                prefabsToLoad.Add(doorPrefab);
            }
        }

        // Load every distinct door prefab once - the only async part.
        List<AsyncOperationHandle<GameObject>> loadHandles = new(prefabsToLoad.Count);
        foreach (var prefabRef in prefabsToLoad) {
            var handle = prefabRef.LoadAssetAsync();
            loadedDoorAssets[prefabRef] = handle;
            loadHandles.Add(handle);
        }

        foreach (var handle in loadHandles) {
            yield return handle;
        }

        // Every door prefab is resident in memory now, so place them with plain,
        // synchronous Instantiate calls.
        foreach (var (position, rotation, prefab) in doorsToPlace) {
            AsyncOperationHandle<GameObject> handle = loadedDoorAssets[prefab];
            if (handle.Status != AsyncOperationStatus.Succeeded) {
                Debug.LogWarning("Failed to load addressable door prefab.");
                continue;
            }

            GameObject instance = Instantiate(handle.Result, position, rotation, generatedDoorParent);
            spawnedDoors.Add(instance);
        }
    }

    private AssetReferenceGameObject GetDoorPrefabForZone(RoomData.Zone zone) {
        foreach (var zT in selectedMapTemplate.zones) {
            if (zT.zoneType == zone) return zT.zoneDoor;
        }
        return null;
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
            if (r == null) continue;
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

        void AddValidRooms(RoomData[] arr) {
            if (arr == null) return;
            foreach (var room in arr) {
                if (room != null) all.Add(room);
            }
        }

        AddValidRooms(roomSet.twoWayRooms);
        AddValidRooms(roomSet.cornerRooms);
        AddValidRooms(roomSet.threeWayRooms);
        AddValidRooms(roomSet.fourWayRooms);
        AddValidRooms(roomSet.deadEndRooms);
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