using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace vectorarts.scpcbr {
    [System.Serializable]
    public class RoomPlacementModule {
        public float deadEndChance = 0.15f;
        public int maxPlacementAttempts = 8;
        public int optimizationPasses = 6;

        private MapGenerator generator;
        private GridCreationModule gridModule;
        private readonly Dictionary<Vector2Int, RoomData> placedRooms = new();
        private readonly Queue<RoomData> roomsToProcess = new();
        private readonly List<DoorData> doorPlacements = new();

        public void Initialize(MapGenerator gen, GridCreationModule grid) {
            generator = gen;
            gridModule = grid;
            placedRooms.Clear();
            roomsToProcess.Clear();
            doorPlacements.Clear();
        }

        public void GenerateRooms() {
            placedRooms.Clear();
            roomsToProcess.Clear();
            doorPlacements.Clear();

            // First pass - generate normal rooms
            foreach (var zone in generator.zones)
            {
                if (zone.utilizeStartingRoom) PlaceStartingRoom(zone);
                else PlaceRandomStartingRoom(zone);
                ProcessRoomQueue();
            }

            // Second pass - place special rooms and ensure connectivity
            foreach (var zone in generator.zones)
            {
                PlaceSpecialRoomsInConnectorAreas(zone);
                EnsureRequiredRoomsArePlaced(zone.zoneID);
                ProcessRoomQueue();

                // Final check for connectivity
                EnsureZoneConnectivity(zone.zoneID);
            }
        }

        private void EnsureZoneConnectivity(int zoneID) {
            var zoneCells = gridModule.GetCellsInZone(zoneID);
            if (zoneCells == null) return;

            // Find all rooms that aren't connected to main path
            var unconnectedRooms = zoneCells
                .Where(c => c.occupied && !IsConnectedToStart(c.position, zoneID))
                .ToList();

            foreach (var cell in unconnectedRooms)
            {
                // Try to connect this room to nearest connected room
                ConnectToMainPath(cell.position, zoneID);
            }
        }

        private bool IsConnectedToStart(Vector2Int position, int zoneID) {
            // Simple flood fill to check connectivity
            var visited = new HashSet<Vector2Int>();
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(position);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (visited.Contains(current)) continue;
                visited.Add(current);

                // If we reached starting room, it's connected
                if (placedRooms.TryGetValue(current, out var room) &&
                    room.room.validForStartingRoom)
                {
                    return true;
                }

                // Check all connected neighbors
                foreach (var dir in new[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left })
                {
                    var neighbor = current + dir;
                    if (placedRooms.TryGetValue(neighbor, out var neighborRoom) &&
                        neighborRoom.zoneID == zoneID)
                    {
                        // Check if there's a door between them
                        if (doorPlacements.Any(d =>
                            (d.position == current && d.direction == dir) ||
                            (d.position == neighbor && d.direction == -dir)))
                        {
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }
            return false;
        }

        private void ConnectToMainPath(Vector2Int position, int zoneID) {
            // Find nearest connected room
            var connectedRooms = placedRooms
                .Where(r => r.Value.zoneID == zoneID && IsConnectedToStart(r.Key, zoneID))
                .OrderBy(r => Vector2Int.Distance(position, r.Key))
                .ToList();

            if (!connectedRooms.Any()) return;

            var targetRoom = connectedRooms.First();
            var path = FindPathBetweenRooms(position, targetRoom.Key, zoneID);

            // Add doors along the path
            for (int i = 0; i < path.Count - 1; i++)
            {
                var currentPos = path[i];
                var nextPos = path[i + 1];

                // Skip if current position doesn't have a room
                if (!placedRooms.ContainsKey(currentPos)) continue;

                var dir = nextPos - currentPos;
                doorPlacements.Add(new DoorData {
                    position = currentPos,
                    direction = dir,
                    zoneID = zoneID,
                    room = placedRooms[currentPos].room
                });
            }
        }

        private List<Vector2Int> FindPathBetweenRooms(Vector2Int start, Vector2Int end, int zoneID) {
            // Simple breadth-first search implementation
            var queue = new Queue<Vector2Int>();
            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            var visited = new HashSet<Vector2Int>();

            // Only start pathfinding if both positions have rooms
            if (!placedRooms.ContainsKey(start) || !placedRooms.ContainsKey(end))
            {
                return new List<Vector2Int> { start, end };
            }

            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (current == end)
                {
                    return ReconstructPath(cameFrom, current);
                }

                foreach (var dir in new[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left })
                {
                    var neighbor = current + dir;

                    // Skip if already visited
                    if (visited.Contains(neighbor)) continue;

                    // Skip if not in grid or not in same zone
                    if (!gridModule.IsPositionWithinGrid(neighbor)) continue;

                    // Skip if position is occupied by another zone's room (unless it's our target)
                    if (neighbor != end &&
                        placedRooms.TryGetValue(neighbor, out var neighborRoom) &&
                        neighborRoom.zoneID != zoneID)
                    {
                        continue;
                    }

                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }

            // If no path found, return direct path
            return new List<Vector2Int> { start, end };
        }

        private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current) {
            var path = new List<Vector2Int> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Insert(0, current);
            }
            return path;
        }


        private void PlaceSpecialRoomsInConnectorAreas(Zone zone) {
            // Get all connector cells for this zone
            var connectorCells = gridModule.GetCellsInZone(zone.zoneID)
                .Where(c => c.isConnector)
                .OrderBy(_ => Random.value) // Randomize order for placement
                .ToList();

            if (!connectorCells.Any()) return;

            if (zone.connectsToNextZone)
            {
                // Place zone connector rooms
                if (zone.zoneConnectorRoom == null)
                {
                    Debug.LogError($"Zone {zone.zoneID} is marked to connect but has no connector room assigned");
                    return;
                }

                // Place the specified number of connector rooms (default is 2)
                int connectorsToPlace = Mathf.Min(zone.numberOfZoneConnectors, connectorCells.Count);
                for (int i = 0; i < connectorsToPlace; i++)
                {
                    var cell = connectorCells[i];
                    PlaceZoneConnectorRoom(cell.position, zone);
                }
            }
            else if (zone.surfaceExitRooms != null && zone.surfaceExitRooms.Length > 0)
            {
                // Place surface exit rooms - one of each unique type
                int exitsToPlace = Mathf.Min(zone.surfaceExitRooms.Length, connectorCells.Count);
                for (int i = 0; i < exitsToPlace; i++)
                {
                    var cell = connectorCells[i];
                    PlaceSurfaceExitRoom(cell.position, zone, zone.surfaceExitRooms[i]);
                }
            }
        }

        private void PlaceZoneConnectorRoom(Vector2Int position, Zone zone) {
            var cell = gridModule.GetCell(position);
            if (cell == null || !cell.isConnector || cell.occupied) return;

            // Zone connector rooms should face both current and next zone
            int rotation = GetRotationWithTwoWayEntrance(zone.zoneConnectorRoom);
            if (rotation == -1)
            {
                Debug.LogError($"Zone connector room {zone.zoneConnectorRoom.name} needs both north and south entrances");
                return;
            }

            PlaceRoom(position, zone.zoneConnectorRoom, rotation, zone.zoneID);

            // Connect to previous room in current zone
            var southPos = position + Vector2Int.down;
            if (placedRooms.TryGetValue(southPos, out var southRoom))
            {
                doorPlacements.Add(new DoorData {
                    position = southPos,
                    direction = Vector2Int.up,
                    zoneID = zone.zoneID,
                    room = southRoom.room
                });
            }

            // Mark connection to next zone
            doorPlacements.Add(new DoorData {
                position = position,
                direction = Vector2Int.up,
                zoneID = zone.zoneID,
                room = zone.zoneConnectorRoom,
                isZoneConnector = true,
                nextZoneID = zone.nextZoneID
            });
        }

        private int GetRotationWithTwoWayEntrance(Room room) {
            for (int i = 0; i < 4; i++)
            {
                var entrances = RotateEntrances(room.entranceDirections, i * 90);
                if (entrances.north && entrances.south) return i * 90;
            }
            return -1;
        }

        private void PlaceSurfaceExitRoom(Vector2Int position, Zone zone, Room exitRoom) {
            var cell = gridModule.GetCell(position);
            if (cell == null || !cell.isConnector || cell.occupied) return;

            // Surface exits should face inward (south entrance)
            int rotation = GetRotationWithSouthEntrance(exitRoom);
            if (rotation == -1)
            {
                Debug.LogError($"Surface exit room {exitRoom.name} has no south entrance");
                return;
            }

            PlaceRoom(position, exitRoom, rotation, zone.zoneID);

            // Connect to previous room in current zone
            var southPos = position + Vector2Int.down;
            if (placedRooms.TryGetValue(southPos, out var southRoom))
            {
                doorPlacements.Add(new DoorData {
                    position = southPos,
                    direction = Vector2Int.up,
                    zoneID = zone.zoneID,
                    room = southRoom.room
                });
            }
        }

        public IEnumerable<RoomData> GetPlacedRooms() => placedRooms.Values;
        public IEnumerable<DoorData> GetDoorPlacements() => doorPlacements;
        public bool TryGetRoomAtPosition(Vector2Int pos, out RoomData data) => placedRooms.TryGetValue(pos, out data);

        private void PlaceStartingRoom(Zone zone) {
            var cellPos = new Vector2Int((int)zone.startingRoomCellPosition.x, (int)zone.startingRoomCellPosition.y + (gridModule.zoneStartY.TryGetValue(zone.zoneID, out int y) ? y : 0));
            var rooms = zone.rooms.normalRooms.Where(r => r.validForStartingRoom).ToList();
            if (rooms.Count == 0) rooms = zone.rooms.normalRooms.ToList();
            if (rooms.Count == 0) { Debug.LogError($"No rooms for zone {zone.zoneID}"); return; }

            var room = rooms.Random();
            var rot = GetRotationWithSouthEntrance(room);
            if (rot == -1) { Debug.LogError($"No south entrance for {room.name} in zone {zone.zoneID}"); return; }

            PlaceRoom(cellPos, room, rot, zone.zoneID);
        }

        private void PlaceRandomStartingRoom(Zone zone) {
            var cells = gridModule.GetCellsInZone(zone.zoneID)?.Where(c => !c.isConnector && !c.occupied).ToList();
            if (cells == null || cells.Count == 0) { Debug.LogError($"No valid cells in zone {zone.zoneID}"); return; }

            var cell = cells.Random(Mathf.Min(cells.Count, gridModule.perZoneGridWidth * 3));
            var rooms = zone.rooms.normalRooms.Where(r => r.validForStartingRoom).ToList();
            if (rooms.Count == 0) rooms = zone.rooms.normalRooms.ToList();
            if (rooms.Count == 0) { Debug.LogError($"No rooms for zone {zone.zoneID}"); return; }

            var room = rooms.Random();
            var rot = GetRotationWithSouthEntrance(room);
            if (rot == -1) { Debug.LogError($"No south entrance for {room.name} in zone {zone.zoneID}"); return; }

            PlaceRoom(cell.position, room, rot, zone.zoneID);
        }

        private int GetRotationWithSouthEntrance(Room room) {
            for (int i = 0; i < 4; i++) if (RotateEntrances(room.entranceDirections, i * 90).south) return i * 90;
            return -1;
        }

        private void PlaceRoom(Vector2Int pos, Room room, int rot, int zoneID) {
            var cell = gridModule.GetCell(pos);
            if (cell == null || cell.occupied) return;
            cell.occupied = true;

            var entrances = RotateEntrances(room.entranceDirections, rot);
            placedRooms[pos] = new RoomData {
                position = pos,
                room = room,
                rotation = rot,
                zoneID = zoneID,
                entranceDirections = entrances
            };
            roomsToProcess.Enqueue(placedRooms[pos]);

            if (room.isLarge && room.extensionDirections != null)
                foreach (var d in room.extensionDirections)
                {
                    var extPos = pos + RotateDirection(d, rot);
                    var extCell = gridModule.GetCell(extPos);
                    if (extCell != null && !extCell.occupied) extCell.occupied = true;
                }
        }

        private void ProcessRoomQueue() {
            while (roomsToProcess.Count > 0)
            {
                var room = roomsToProcess.Dequeue();
                Connect(room, Vector2Int.up, room.entranceDirections.north);
                Connect(room, Vector2Int.right, room.entranceDirections.east);
                Connect(room, Vector2Int.down, room.entranceDirections.south);
                Connect(room, Vector2Int.left, room.entranceDirections.west);
            }
        }

        private void Connect(RoomData source, Vector2Int dir, bool hasEntrance) {
            if (!hasEntrance) return;
            var targetPos = source.position + dir;
            var cell = gridModule.GetCell(targetPos);
            if (cell == null || cell.occupied || cell.zoneID != source.zoneID) return;

            doorPlacements.Add(new DoorData { position = source.position, direction = dir, zoneID = source.zoneID, room = source.room });

            Vector2Int opp = -dir;
            bool n = opp == Vector2Int.up, e = opp == Vector2Int.right, s = opp == Vector2Int.down, w = opp == Vector2Int.left;
            if (Random.value < deadEndChance)
            {
                var deadEnds = GetPotentialRooms(source.zoneID, n, e, s, w)
                    .Where(r => r.roomShape == Room.RoomShape.Endroom || CountEntrances(r.entranceDirections) == 1).ToList();
                if (deadEnds.Count > 0) { PlaceRoomWithRetry(targetPos, source.zoneID, n, e, s, w, deadEnds); return; }
            }
            PlaceRoomWithRetry(targetPos, source.zoneID, n, e, s, w);
        }

        private void PlaceRoomWithRetry(Vector2Int pos, int zoneID, bool n, bool e, bool s, bool w, List<Room> specificRooms = null) {
            for (int a = 0; a < maxPlacementAttempts; a++)
            {
                var rooms = (specificRooms ?? GetPotentialRooms(zoneID, n, e, s, w));
                if (rooms.Count == 0) return;
                var room = rooms.Random();
                var rots = GetValidRotations(room, pos, n, e, s, w);
                if (rots.Count > 0) { PlaceRoom(pos, room, rots.Random(), zoneID); return; }
            }
        }

        private List<int> GetValidRotations(Room room, Vector2Int pos, bool n, bool e, bool s, bool w) =>
            Enumerable.Range(0, 4).Select(i => i * 90)
                .Where(rot => {
                    var entrances = RotateEntrances(room.entranceDirections, rot);
                    return ((n && entrances.north) || (e && entrances.east) || (s && entrances.south) || (w && entrances.west))
                        && AreEntrancesInBounds(pos, entrances);
                }).ToList();

        private List<Room> GetPotentialRooms(int zoneID, bool n, bool e, bool s, bool w) {
            var zone = generator.zones.FirstOrDefault(z => z.zoneID == zoneID);
            if (zone == null) return new();

            // First check if there are any unplaced required rooms that match the entrance requirements
            var unplacedRequired = zone.rooms.requiredRooms
                .Where(r => !placedRooms.Values.Any(p => p.room == r && p.zoneID == zoneID) && IsCompatible(r, n, e, s, w))
                .ToList();

            // If we have unplaced required rooms that fit here, prioritize them with a random chance
            if (unplacedRequired.Count > 0 && Random.value < 0.5f)
            {
                return unplacedRequired;
            }

            // Otherwise proceed with normal room selection
            var normal = zone.rooms.normalRooms.Where(r => IsCompatible(r, n, e, s, w))
                .OrderByDescending(r => CountEntrances(r.entranceDirections));

            return normal.Concat(unplacedRequired).ToList();
        }

        private void EnsureRequiredRoomsArePlaced(int zoneID) {
            var zone = generator.zones.FirstOrDefault(z => z.zoneID == zoneID);
            if (zone == null || zone.rooms.requiredRooms.Length == 0) return;

            foreach (var requiredRoom in zone.rooms.requiredRooms)
            {
                // Skip if already placed
                if (placedRooms.Values.Any(r => r.room == requiredRoom && r.zoneID == zoneID)) continue;

                // Find all possible positions where this room could fit
                var possiblePositions = gridModule.GetCellsInZone(zoneID)
                    .Where(c => !c.occupied && !c.isConnector)
                    .OrderBy(_ => Random.value) // Randomize order to vary placement
                    .ToList();

                foreach (var cell in possiblePositions)
                {
                    // Check what entrances would be needed at this position
                    bool needsNorth = false, needsEast = false, needsSouth = false, needsWest = false;
                    var neighbors = new[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

                    foreach (var dir in neighbors)
                    {
                        var neighborPos = cell.position + dir;
                        if (placedRooms.TryGetValue(neighborPos, out var neighbor))
                        {
                            if (dir == Vector2Int.up) needsSouth = neighbor.entranceDirections.north;
                            if (dir == Vector2Int.right) needsWest = neighbor.entranceDirections.east;
                            if (dir == Vector2Int.down) needsNorth = neighbor.entranceDirections.south;
                            if (dir == Vector2Int.left) needsEast = neighbor.entranceDirections.west;
                        }
                    }

                    // Try all possible rotations
                    var validRotations = GetValidRotations(requiredRoom, cell.position, needsNorth, needsEast, needsSouth, needsWest);
                    if (validRotations.Count > 0)
                    {
                        PlaceRoom(cell.position, requiredRoom, validRotations.Random(), zoneID);
                        break;
                    }
                }
            }
        }

        private bool IsCompatible(Room room, bool n, bool e, bool s, bool w) =>
            Enumerable.Range(0, 4).Any(rot => {
                var eDir = RotateEntrances(room.entranceDirections, rot * 90);
                return (n && eDir.north) || (e && eDir.east) || (s && eDir.south) || (w && eDir.west);
            });

        private bool AreEntrancesInBounds(Vector2Int pos, Room.EntranceDirections entrances) {
            var dirs = new[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
            var checks = new[] { entrances.north, entrances.east, entrances.south, entrances.west };
            for (int i = 0; i < 4; i++)
            {
                if (!checks[i]) continue;
                var neighbor = pos + dirs[i];
                if (!gridModule.IsPositionWithinGrid(neighbor)) return false;
                if (placedRooms.TryGetValue(neighbor, out var nRoom))
                {
                    var required = -dirs[i];
                    if ((required == Vector2Int.up && !nRoom.entranceDirections.north) ||
                        (required == Vector2Int.right && !nRoom.entranceDirections.east) ||
                        (required == Vector2Int.down && !nRoom.entranceDirections.south) ||
                        (required == Vector2Int.left && !nRoom.entranceDirections.west)) return false;
                }
            }
            return true;
        }

        private int CountEntrances(Room.EntranceDirections e) => (e.north ? 1 : 0) + (e.east ? 1 : 0) + (e.south ? 1 : 0) + (e.west ? 1 : 0);

        private Room.EntranceDirections RotateEntrances(Room.EntranceDirections e, int deg) {
            int s = (deg / 90) % 4;
            return s switch {
                1 => new() { north = e.west, east = e.north, south = e.east, west = e.south },
                2 => new() { north = e.south, east = e.west, south = e.north, west = e.east },
                3 => new() { north = e.east, east = e.south, south = e.west, west = e.north },
                _ => e
            };
        }

        private Vector2Int RotateDirection(Vector2 d, int deg) {
            var v = new Vector2Int(Mathf.RoundToInt(d.x), Mathf.RoundToInt(d.y));
            return (deg / 90 % 4) switch {
                1 => new(-v.y, v.x),
                2 => new(-v.x, -v.y),
                3 => new(v.y, -v.x),
                _ => v
            };
        }

        [System.Serializable]
        public class RoomData {
            public Vector2Int position;
            public Room room;
            public int rotation;
            public int zoneID;
            public Room.EntranceDirections entranceDirections;
        }

        [System.Serializable]
        public class DoorData {
            public Vector2Int position;
            public Vector2Int direction;
            public int zoneID;
            public Room room;
            public bool isZoneConnector;
            public int nextZoneID; // Added for zone connections
        }
    }

    static class ListExtensions {
        public static T Random<T>(this List<T> list, int max = int.MaxValue) => list[UnityEngine.Random.Range(0, Mathf.Min(list.Count, max))];
    }
}