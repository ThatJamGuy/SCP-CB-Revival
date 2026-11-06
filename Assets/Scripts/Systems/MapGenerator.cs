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
    private readonly Dictionary<Vector2Int, float> mandatoryRotations = new();
    private readonly Dictionary<GameObject, Rect> footprintCache = new();
    private readonly List<Rect> occupiedFootprints = new();
    private readonly List<float> rotationOptionsBuffer = new();

    private const float OVERLAP_TOLERANCE = 0.5f;

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
            if (!placedMandatoryRooms.TryGetValue(placement.gridPosition, out RoomData mandatory)) continue;
            if (mandatory.roomPrefab == null) continue;

            float yRotation = mandatoryRotations.TryGetValue(placement.gridPosition, out float chosen) ? chosen : placement.rotation;
            SpawnRoom(mandatory, placement.gridPosition, yRotation);
        }

        for (int i = 0; i < placements.Length; i++) {
            RoomPlacement placement = placements[i];
            if (placedMandatoryRooms.ContainsKey(placement.gridPosition)) continue;

            Vector3 worldPos = GridToWorld(placement.gridPosition);
            FillValidRoomPool(placement.requiredShape, GetZoneAtPosition(placement.gridPosition));
            ShuffleBuffer(validPoolBuffer);

            RoomData bestRoom = null;
            float bestRotation = placement.rotation;
            float bestOverlap = float.MaxValue;

            for (int c = 0; c < validPoolBuffer.Count; c++) {
                RoomData candidate = validPoolBuffer[c];
                if (candidate.roomPrefab == null) continue;

                float overlap = FindBestRotation(candidate, worldPos, placement.rotation, out float rotation);
                if (overlap >= bestOverlap) continue;

                bestRoom = candidate;
                bestRotation = rotation;
                bestOverlap = overlap;
                if (overlap <= 0f) break;
            }

            if (bestRoom == null) continue;

            if (bestOverlap > 0f)
                Debug.LogWarning($"[MapGenerator] No room fits at {placement.gridPosition} without overlapping, using {bestRoom.name} ({bestOverlap:F1}m² overlap).");

            if (bestRoom.isUnique) usedUniqueRooms.Add(bestRoom);
            occupiedFootprints.Add(GetWorldFootprint(bestRoom.roomPrefab, worldPos, bestRotation));
            SpawnRoom(bestRoom, placement.gridPosition, bestRotation);
        }
    }

    private void SpawnRoom(RoomData room, Vector2Int gridPos, float yRotation) {
        Quaternion rotation = Quaternion.Euler(0f, yRotation, 0f);
        spawnedRooms.Add(Instantiate(room.roomPrefab, GridToWorld(gridPos), rotation, generatedMapParent));
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
        mandatoryRotations.Clear();
        occupiedFootprints.Clear();
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

            ShuffleBuffer(validPositionsBuffer);

            Vector2Int selectedPos = validPositionsBuffer[0];
            float selectedRotation = 0f;
            float bestOverlap = float.MaxValue;

            for (int p = 0; p < validPositionsBuffer.Count; p++) {
                Vector2Int pos = validPositionsBuffer[p];
                float overlap = FindBestRotation(room, GridToWorld(pos), GetPlacementRotation(pos), out float rotation);
                if (overlap >= bestOverlap) continue;

                selectedPos = pos;
                selectedRotation = rotation;
                bestOverlap = overlap;
                if (overlap <= 0f) break;
            }

            if (bestOverlap > 0f)
                Debug.LogWarning($"[MapGenerator] Mandatory room {room.name} overlaps another mandatory room ({bestOverlap:F1}m²).");

            placedMandatoryRooms[selectedPos] = room;
            mandatoryRotations[selectedPos] = selectedRotation;
            if (room.roomPrefab != null)
                occupiedFootprints.Add(GetWorldFootprint(room.roomPrefab, GridToWorld(selectedPos), selectedRotation));

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

    private void FillValidRoomPool(RoomData.RoomShape shape, RoomData.Zone zone) {
        validPoolBuffer.Clear();

        if (shape == RoomData.RoomShape.Checkpoint) {
            RoomData checkpoint = GetCheckpointForZone(zone);
            if (checkpoint != null) validPoolBuffer.Add(checkpoint);
            return;
        }

        RoomData[] candidates = roomSet.GetRoomsByShape(shape);
        if (candidates == null) return;

        for (int i = 0; i < candidates.Length; i++) {
            RoomData r = candidates[i];
            if (r == null) continue;
            if (r.roomZone != zone) continue;
            if (r.isUnique && usedUniqueRooms.Contains(r)) continue;
            validPoolBuffer.Add(r);
        }
    }

    private int GetPlacementRotation(Vector2Int gridPos) {
        RoomPlacement[] placements = selectedMapTemplate.roomPlacements;
        for (int i = 0; i < placements.Length; i++) {
            if (placements[i].gridPosition == gridPos) return placements[i].rotation;
        }
        return 0;
    }

    private static void ShuffleBuffer<T>(List<T> list) {
        for (int i = list.Count - 1; i > 0; i--) {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private float FindBestRotation(RoomData room, Vector3 worldPos, int baseRotation, out float bestRotation) {
        rotationOptionsBuffer.Clear();
        rotationOptionsBuffer.Add(baseRotation);

        // New feature: adding some rotation variation to 2way and 4way rooms for extra cool
        switch (room.roomShape) {
            case RoomData.RoomShape.TwoWay:
                rotationOptionsBuffer.Add(baseRotation + 180f);
                break;
            case RoomData.RoomShape.FourWay:
                rotationOptionsBuffer.Add(baseRotation + 90f);
                rotationOptionsBuffer.Add(baseRotation + 180f);
                rotationOptionsBuffer.Add(baseRotation + 270f);
                break;
        }

        ShuffleBuffer(rotationOptionsBuffer);

        bestRotation = rotationOptionsBuffer[0];
        if (room.roomPrefab == null) return 0f;

        float bestOverlap = float.MaxValue;
        for (int i = 0; i < rotationOptionsBuffer.Count; i++) {
            float overlap = GetOverlapArea(GetWorldFootprint(room.roomPrefab, worldPos, rotationOptionsBuffer[i]));
            if (overlap >= bestOverlap) continue;

            bestOverlap = overlap;
            bestRotation = rotationOptionsBuffer[i];
            if (overlap <= 0f) break;
        }

        return bestOverlap;
    }

    private float GetOverlapArea(Rect footprint) {
        float total = 0f;

        for (int i = 0; i < occupiedFootprints.Count; i++) {
            Rect other = occupiedFootprints[i];
            float width = Mathf.Min(footprint.xMax, other.xMax) - Mathf.Max(footprint.xMin, other.xMin);
            float depth = Mathf.Min(footprint.yMax, other.yMax) - Mathf.Max(footprint.yMin, other.yMin);

            if (width <= OVERLAP_TOLERANCE || depth <= OVERLAP_TOLERANCE) continue;
            total += width * depth;
        }

        return total;
    }

    private Rect GetWorldFootprint(GameObject prefab, Vector3 worldPos, float yRotation) {
        Rect local = GetLocalFootprint(prefab);
        Quaternion rotation = Quaternion.Euler(0f, yRotation, 0f);

        Vector3 a = rotation * new Vector3(local.xMin, 0f, local.yMin);
        Vector3 b = rotation * new Vector3(local.xMax, 0f, local.yMin);
        Vector3 c = rotation * new Vector3(local.xMin, 0f, local.yMax);
        Vector3 d = rotation * new Vector3(local.xMax, 0f, local.yMax);

        float minX = Mathf.Min(Mathf.Min(a.x, b.x), Mathf.Min(c.x, d.x));
        float maxX = Mathf.Max(Mathf.Max(a.x, b.x), Mathf.Max(c.x, d.x));
        float minZ = Mathf.Min(Mathf.Min(a.z, b.z), Mathf.Min(c.z, d.z));
        float maxZ = Mathf.Max(Mathf.Max(a.z, b.z), Mathf.Max(c.z, d.z));

        return Rect.MinMaxRect(minX + worldPos.x, minZ + worldPos.z, maxX + worldPos.x, maxZ + worldPos.z);
    }

    private Rect GetLocalFootprint(GameObject prefab) {
        if (footprintCache.TryGetValue(prefab, out Rect cached)) return cached;

        Transform root = prefab.transform;
        Matrix4x4 toRoot = Matrix4x4.Scale(root.localScale) * root.worldToLocalMatrix;

        float minX = float.MaxValue, minZ = float.MaxValue;
        float maxX = float.MinValue, maxZ = float.MinValue;
        bool hasBounds = false;

        MeshFilter[] filters = prefab.GetComponentsInChildren<MeshFilter>(true);
        for (int i = 0; i < filters.Length; i++) {
            MeshFilter filter = filters[i];
            if (filter.sharedMesh == null || !filter.gameObject.activeSelf) continue;
            if (!filter.TryGetComponent(out MeshRenderer meshRenderer) || !meshRenderer.enabled) continue;

            Matrix4x4 matrix = toRoot * filter.transform.localToWorldMatrix;
            Bounds bounds = filter.sharedMesh.bounds;
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;

            for (int corner = 0; corner < 8; corner++) {
                Vector3 point = matrix.MultiplyPoint3x4(new Vector3(
                    (corner & 1) == 0 ? min.x : max.x,
                    (corner & 2) == 0 ? min.y : max.y,
                    (corner & 4) == 0 ? min.z : max.z));

                minX = Mathf.Min(minX, point.x);
                maxX = Mathf.Max(maxX, point.x);
                minZ = Mathf.Min(minZ, point.z);
                maxZ = Mathf.Max(maxZ, point.z);
            }

            hasBounds = true;
        }

        Rect footprint = hasBounds ? Rect.MinMaxRect(minX, minZ, maxX, maxZ) : Rect.zero;
        footprintCache[prefab] = footprint;

        float halfCell = cellSize * 0.5f + OVERLAP_TOLERANCE;
        if (hasBounds && (minX < -halfCell || maxX > halfCell || minZ < -halfCell || maxZ > halfCell))
            Debug.LogWarning($"[MapGenerator] Room prefab {prefab.name} is larger than a grid cell ({footprint.width:F1} x {footprint.height:F1}m, cell {cellSize}m).");

        return footprint;
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