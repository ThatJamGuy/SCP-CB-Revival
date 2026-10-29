using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

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

    private MapTemplate selectedMapTemplate;

    private readonly List<GameObject> spawnedRooms = new();
    private readonly List<GameObject> spawnedDoors = new();
    private readonly Dictionary<Vector2Int, RoomData.Zone> zoneLookup = new();
    private readonly Dictionary<Vector2Int, RoomData> placedMandatoryRooms = new();
    private readonly HashSet<RoomData> usedUniqueRooms = new();
    private readonly List<RoomData> validPoolBuffer = new();
    private readonly List<Vector2Int> validPositionsBuffer = new();
    private readonly Dictionary<Vector2Int, RoomPlacement> placementLookup = new();
    private readonly HashSet<(Vector2Int, Vector2Int)> processedEdges = new();
    private readonly List<(Vector2Int pos, Color color)> gizmoCells = new();

    #region Unity Lifecycle

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start() {
        // Request the current map seed from RevivalSessionEngine, but generate a random one if that doesn't work
        if (RevivalSessionEngine.Instance != null) {
            seed = RevivalSessionEngine.mapSeed;
            useRandomSeed = string.IsNullOrEmpty(seed);
        }

        StartCoroutine(GenerateMapSequence());
    }

    private void OnDrawGizmos() {
        // Draw Layout Grid
        Vector3 cellVolume = new Vector3(cellSize, 0.1f, cellSize);
        for (int i = 0; i < gizmoCells.Count; i++) {
            Gizmos.color = gizmoCells[i].color;
            Gizmos.DrawWireCube(GridToWorld(gizmoCells[i].pos), cellVolume);
        }

        // Draw Mandatory Reservations (Solid Cubes)
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f); // Semi-transparent Orange
        Vector3 reservedVolume = new Vector3(cellSize * 0.5f, 0.5f, cellSize * 0.5f);
        foreach (KeyValuePair<Vector2Int, RoomData> kvp in placedMandatoryRooms) {
            Gizmos.DrawCube(GridToWorld(kvp.Key), reservedVolume);
        }
    }

    private void OnDestroy() {
        if (Instance == this) Instance = null;
        Cleanup();
    }

    #endregion

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

        // 3. Spawn Rooms (fully synchronous now)
        SpawnPlacedRooms();

        // 4. Bake NavMesh
        if (navMeshSurface != null) {
            Debug.Log("Baking NavMesh...");
            navMeshSurface.BuildNavMesh();
            yield return null;
        }

        // 5. Spawn Doors (fully synchronous now)
        SpawnDoors();

        // 6. Find waypoints if applicable
        if (EntitySystem.Instance != null)
            EntitySystem.Instance.FindWaypointsInScene();

        IsGenerationComplete = true;
        Debug.Log("<color=green>Map Generation Sequence Finished!</color>");
    }

    private void SpawnPlacedRooms() {
        RoomPlacement[] placements = selectedMapTemplate.roomPlacements;
        spawnedRooms.Capacity = Mathf.Max(spawnedRooms.Capacity, placements.Length);

        for (int i = 0; i < placements.Length; i++) {
            RoomPlacement placement = placements[i];

            RoomData roomToSpawn = placedMandatoryRooms.TryGetValue(placement.gridPosition, out RoomData mandatory)
                ? mandatory
                : GetRandomValidRoom(placement.requiredShape, GetZoneAtPosition(placement.gridPosition));

            if (roomToSpawn == null || roomToSpawn.roomPrefab == null) continue;

            Vector3 worldPos = GridToWorld(placement.gridPosition);
            float yRotation = placement.rotation;

            // New feature: adding some rotation variation to 2way and 4way rooms for extra cool
            switch (roomToSpawn.roomShape) {
                case RoomData.RoomShape.TwoWay:
                    yRotation += Random.Range(0, 2) * 180f;
                    break;
                case RoomData.RoomShape.FourWay:
                    yRotation += Random.Range(0, 4) * 90f;
                    break;
            }

            Quaternion rotation = Quaternion.Euler(0f, yRotation, 0f);

            spawnedRooms.Add(Instantiate(roomToSpawn.roomPrefab, worldPos, rotation, generatedMapParent));
        }
    }

    private void Cleanup() {
        for (int i = 0; i < spawnedRooms.Count; i++) {
            if (spawnedRooms[i] != null) Destroy(spawnedRooms[i]);
        }
        spawnedRooms.Clear();

        for (int i = 0; i < spawnedDoors.Count; i++) {
            if (spawnedDoors[i] != null) Destroy(spawnedDoors[i]);
        }
        spawnedDoors.Clear();

        placedMandatoryRooms.Clear();
        usedUniqueRooms.Clear();
        zoneLookup.Clear();
        gizmoCells.Clear();
    }

    #region PHASE 1: Grid Mapping

    private void BuildGridData() {
        if (selectedMapTemplate == null) return;

        int currentY = 0;
        ZoneTemplate[] zones = selectedMapTemplate.zones;
        for (int z = 0; z < zones.Length; z++) {
            ZoneTemplate zone = zones[z];

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
        // Same iteration order as before so seeds keep producing the same maps.
        PlaceMandatoryFromArray(roomSet.twoWayRooms);
        PlaceMandatoryFromArray(roomSet.cornerRooms);
        PlaceMandatoryFromArray(roomSet.threeWayRooms);
        PlaceMandatoryFromArray(roomSet.fourWayRooms);
        PlaceMandatoryFromArray(roomSet.deadEndRooms);
    }

    private void PlaceMandatoryFromArray(RoomData[] rooms) {
        if (rooms == null) return;

        for (int i = 0; i < rooms.Length; i++) {
            RoomData room = rooms[i];
            if (room == null || !room.mustSpawn) continue;

            FillValidPositionsForRoom(room);

            if (validPositionsBuffer.Count == 0) {
                Debug.LogWarning($"Could not find valid placement for mandatory room: {room.name}");
                continue;
            }

            Vector2Int selectedPos = validPositionsBuffer[Random.Range(0, validPositionsBuffer.Count)];
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

    private void SpawnDoors() {
        RoomPlacement[] placements = selectedMapTemplate.roomPlacements;

        processedEdges.Clear();
        placementLookup.Clear();

        for (int i = 0; i < placements.Length; i++) {
            placementLookup.TryAdd(placements[i].gridPosition, placements[i]);
        }

        for (int i = 0; i < placements.Length; i++) {
            RoomPlacement placement = placements[i];
            Vector2Int currentPos = placement.gridPosition;
            if (!BaseExits.TryGetValue(placement.requiredShape, out int[] baseAngles)) continue;

            for (int a = 0; a < baseAngles.Length; a++) {
                int localAngle = (baseAngles[a] + placement.rotation) % 360;
                Vector2Int neighborPos = currentPos + AngleToDirection(localAngle);

                var edgeKey = GetEdgeKey(currentPos, neighborPos);
                if (processedEdges.Contains(edgeKey)) continue;

                // Check if neighbor exists and has a matching exit
                if (!placementLookup.TryGetValue(neighborPos, out RoomPlacement neighborPlacement)) continue;

                int oppositeAngle = (localAngle + 180) % 360;
                if (!HasWorldExit(neighborPlacement.requiredShape, neighborPlacement.rotation, oppositeAngle)) continue;

                processedEdges.Add(edgeKey);

                GameObject doorPrefab = GetDoorPrefabForZone(GetZoneAtPosition(currentPos));
                if (doorPrefab == null) continue;

                // Midpoint between the two rooms, rotated to point along the exit axis
                Vector3 doorPosition = (GridToWorld(currentPos) + GridToWorld(neighborPos)) * 0.5f;
                Quaternion doorRotation = Quaternion.Euler(0f, localAngle, 0f);

                spawnedDoors.Add(Instantiate(doorPrefab, doorPosition, doorRotation, generatedDoorParent));
            }
        }
    }

    private GameObject GetDoorPrefabForZone(RoomData.Zone zone) {
        var zones = selectedMapTemplate.zones;
        for (int i = 0; i < zones.Length; i++) {
            if (zones[i].zoneType == zone) return zones[i].zoneDoor;
        }
        return null;
    }
    #endregion

    #region Helpers :)

    private RoomData GetRandomValidRoom(RoomData.RoomShape shape, RoomData.Zone zone) {
        if (shape == RoomData.RoomShape.Checkpoint) return GetCheckpointForZone(zone);

        RoomData[] candidates = roomSet.GetRoomsByShape(shape);
        if (candidates == null) return null;

        validPoolBuffer.Clear();
        for (int i = 0; i < candidates.Length; i++) {
            RoomData r = candidates[i];
            if (r == null) continue;
            if (r.roomZone != zone) continue;
            if (r.isUnique && usedUniqueRooms.Contains(r)) continue;
            validPoolBuffer.Add(r);
        }

        if (validPoolBuffer.Count == 0) return null;

        RoomData selected = validPoolBuffer[Random.Range(0, validPoolBuffer.Count)];
        if (selected.isUnique) usedUniqueRooms.Add(selected);

        return selected;
    }

    private void FillValidPositionsForRoom(RoomData room) {
        validPositionsBuffer.Clear();

        RoomPlacement[] placements = selectedMapTemplate.roomPlacements;
        for (int i = 0; i < placements.Length; i++) {
            RoomPlacement p = placements[i];
            if (p.requiredShape != room.roomShape) continue;
            if (GetZoneAtPosition(p.gridPosition) != room.roomZone) continue;
            if (placedMandatoryRooms.ContainsKey(p.gridPosition)) continue;

            validPositionsBuffer.Add(p.gridPosition);
        }
    }

    private RoomData GetCheckpointForZone(RoomData.Zone zone) {
        ZoneTemplate[] zones = selectedMapTemplate.zones;
        for (int i = 0; i < zones.Length; i++) {
            if (zones[i].zoneType == zone) return zones[i].checkpointRoomVariant;
        }
        return null;
    }

    private RoomData.Zone GetZoneAtPosition(Vector2Int pos) {
        return zoneLookup.TryGetValue(pos, out var zone) ? zone : RoomData.Zone.LCZ;
    }

    private Vector3 GridToWorld(Vector2Int gridPos) {
        return new Vector3(gridPos.x * cellSize + gridOffsetX, 0f, gridPos.y * cellSize);
    }

    #endregion

    #region Door Helpers :)

    private static bool HasWorldExit(RoomData.RoomShape shape, int rotation, int worldAngle) {
        if (!BaseExits.TryGetValue(shape, out int[] angles)) return false;

        for (int i = 0; i < angles.Length; i++) {
            // (Base + Object Rotation) % 360 gives world-space exit direction
            if ((angles[i] + rotation) % 360 == worldAngle) return true;
        }
        return false;
    }

    private static Vector2Int AngleToDirection(int angle) {
        return angle switch {
            0 => new Vector2Int(0, 1),    // North
            90 => new Vector2Int(1, 0),   // East
            180 => new Vector2Int(0, -1), // South
            270 => new Vector2Int(-1, 0), // West
            _ => Vector2Int.zero
        };
    }

    private static (Vector2Int, Vector2Int) GetEdgeKey(Vector2Int a, Vector2Int b) {
        if (a.x < b.x || (a.x == b.x && a.y < b.y)) return (a, b);
        return (b, a);
    }

    #endregion
}