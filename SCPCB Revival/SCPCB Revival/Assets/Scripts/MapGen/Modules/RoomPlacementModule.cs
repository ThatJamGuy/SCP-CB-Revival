using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace vectorarts.scpcbr {
    [System.Serializable]
    public class RoomPlacementModule {
        [Header("Generation Settings")]
        public float deadEndChance = 0.15f;
        [Tooltip("Number of attempts to find a suitable room before giving up")]
        public int maxPlacementAttempts = 8;
        [Tooltip("Number of optimization passes to fix mismatched rooms")]
        public int optimizationPasses = 6;

        private MapGenerator generator;
        private GridCreationModule gridModule;
        private Dictionary<Vector2Int, RoomData> placedRooms = new();
        private Queue<RoomData> roomsToProcess = new();
        private List<DoorData> doorPlacements = new();

        public void Initialize(MapGenerator generator, GridCreationModule gridModule) {
            this.generator = generator;
            this.gridModule = gridModule;
            placedRooms.Clear();
            roomsToProcess.Clear();
            doorPlacements.Clear();
        }

        public void GenerateRooms() {
            placedRooms.Clear();
            roomsToProcess.Clear();
            doorPlacements.Clear();
            foreach (var zone in generator.zones)
            {
                if (zone.utilizeStartingRoom) PlaceStartingRoom(zone);
                else PlaceRandomStartingRoom(zone);
                ProcessRoomQueue();
            }
        }

        public IEnumerable<RoomData> GetPlacedRooms() => placedRooms.Values;
        public IEnumerable<DoorData> GetDoorPlacements() => doorPlacements;
        public bool TryGetRoomAtPosition(Vector2Int position, out RoomData roomData) => placedRooms.TryGetValue(position, out roomData);

        private void PlaceStartingRoom(Zone zone) {
            var cellPos = new Vector2Int((int)zone.startingRoomCellPosition.x, (int)zone.startingRoomCellPosition.y + (gridModule.zoneStartY.TryGetValue(zone.zoneID, out int startY) ? startY : 0));
            var startingRooms = zone.rooms.normalRooms.Where(r => r.validForStartingRoom).ToList();
            if (startingRooms.Count == 0)
            {
                Debug.LogWarning($"No valid starting rooms for zone {zone.zoneID}, using first normal room");
                startingRooms = zone.rooms.normalRooms.ToList();
            }
            if (startingRooms.Count == 0)
            {
                Debug.LogError($"No rooms available for zone {zone.zoneID}");
                return;
            }

            Room selectedRoom = startingRooms[Random.Range(0, startingRooms.Count)];
            int rotation = GetRotationWithSouthEntrance(selectedRoom);
            if (rotation == -1)
            {
                Debug.LogError($"No valid rotation with south entrance for room {selectedRoom.name} in zone {zone.zoneID}");
                return;
            }

            PlaceRoom(cellPos, selectedRoom, rotation, zone.zoneID);
        }

        private void PlaceRandomStartingRoom(Zone zone) {
            var cells = gridModule.GetCellsInZone(zone.zoneID);
            if (cells == null || cells.Count == 0)
            {
                Debug.LogError($"No cells for zone {zone.zoneID}");
                return;
            }

            var validCells = cells.Where(c => !c.isConnector && !c.occupied).ToList();
            if (validCells.Count == 0)
            {
                Debug.LogError($"No valid cells for zone {zone.zoneID}");
                return;
            }

            var cell = validCells[Random.Range(0, Mathf.Min(validCells.Count, gridModule.perZoneGridWidth * 3))];
            var startingRooms = zone.rooms.normalRooms.Where(r => r.validForStartingRoom).ToList();
            if (startingRooms.Count == 0) startingRooms = zone.rooms.normalRooms.ToList();
            if (startingRooms.Count == 0)
            {
                Debug.LogError($"No rooms available for zone {zone.zoneID}");
                return;
            }

            Room selectedRoom = startingRooms[Random.Range(0, startingRooms.Count)];
            int rotation = GetRotationWithSouthEntrance(selectedRoom);
            if (rotation == -1)
            {
                Debug.LogError($"No valid rotation with south entrance for room {selectedRoom.name} in zone {zone.zoneID}");
                return;
            }

            PlaceRoom(cell.position, selectedRoom, rotation, zone.zoneID);
        }

        private int GetRotationWithSouthEntrance(Room room) {
            for (int i = 0; i < 4; i++)
            {
                var rotated = RotateEntranceDirections(room.entranceDirections, i * 90);
                if (rotated.south) return i * 90;
            }
            return -1;
        }

        private void PlaceRoom(Vector2Int position, Room room, int rotation, int zoneID) {
            var cell = gridModule.GetCell(position);
            if (cell == null || cell.occupied)
            {
                Debug.LogWarning($"Cannot place room at {position}: cell is null or occupied");
                return;
            }

            cell.occupied = true;
            var entranceDirections = RotateEntranceDirections(room.entranceDirections, rotation);
            var roomData = new RoomData { position = position, room = room, rotation = rotation, zoneID = zoneID, entranceDirections = entranceDirections };
            placedRooms[position] = roomData;
            roomsToProcess.Enqueue(roomData);
            Debug.Log($"Placed {room.name} at {position} with rotation {rotation}° in zone {zoneID}");

            if (room.isLarge && room.extensionDirections != null)
            {
                foreach (var dir in room.extensionDirections)
                {
                    var extensionPos = position + RotateDirection(dir, rotation);
                    var extensionCell = gridModule.GetCell(extensionPos);
                    if (extensionCell != null && !extensionCell.occupied)
                    {
                        extensionCell.occupied = true;
                        Debug.Log($"Marked extension cell at {extensionPos} as occupied");
                    }
                }
            }
        }

        private void ProcessRoomQueue() {
            while (roomsToProcess.Count > 0)
            {
                var room = roomsToProcess.Dequeue();
                TryConnectRoom(room, Vector2Int.up, room.entranceDirections.north);
                TryConnectRoom(room, Vector2Int.right, room.entranceDirections.east);
                TryConnectRoom(room, Vector2Int.down, room.entranceDirections.south);
                TryConnectRoom(room, Vector2Int.left, room.entranceDirections.west);
            }
        }

        private void TryConnectRoom(RoomData source, Vector2Int dir, bool hasEntrance) {
            if (!hasEntrance) return;

            var targetPos = source.position + dir;
            var targetCell = gridModule.GetCell(targetPos);
            if (targetCell == null || targetCell.occupied || targetCell.zoneID != source.zoneID) return;

            // Add door placement data
            doorPlacements.Add(new DoorData {
                position = source.position,
                direction = dir,
                zoneID = source.zoneID,
                room = source.room,
                isZoneConnector = false
            });

            Vector2Int opposite = -dir;
            bool needsNorth = opposite == Vector2Int.up;
            bool needsEast = opposite == Vector2Int.right;
            bool needsSouth = opposite == Vector2Int.down;
            bool needsWest = opposite == Vector2Int.left;

            // Check if this connection would create a dead end that wasn't intended
            if (Random.value < deadEndChance)
            {
                var potentialRooms = GetPotentialRooms(source.zoneID, needsNorth, needsEast, needsSouth, needsWest);
                var deadEnds = potentialRooms.Where(r => r.roomShape == Room.RoomShape.Endroom || CountEntrances(r.entranceDirections) == 1).ToList();

                if (deadEnds.Count > 0)
                {
                    PlaceRoomWithRetry(targetPos, source.zoneID, needsNorth, needsEast, needsSouth, needsWest, deadEnds);
                    return;
                }
            }

            PlaceRoomWithRetry(targetPos, source.zoneID, needsNorth, needsEast, needsSouth, needsWest);
        }

        private void PlaceRoomWithRetry(Vector2Int position, int zoneID, bool needsNorth, bool needsEast, bool needsSouth, bool needsWest, List<Room> specificRooms = null) {
            int attempts = 0;
            while (attempts < maxPlacementAttempts)
            {
                attempts++;

                var potentialRooms = specificRooms ?? GetPotentialRooms(zoneID, needsNorth, needsEast, needsSouth, needsWest);
                if (potentialRooms.Count == 0) return;

                Room selectedRoom = potentialRooms[Random.Range(0, potentialRooms.Count)];
                var validRotations = GetValidRotations(selectedRoom, position, needsNorth, needsEast, needsSouth, needsWest);

                if (validRotations.Count > 0)
                {
                    int selectedRotation = validRotations[Random.Range(0, validRotations.Count)];
                    PlaceRoom(position, selectedRoom, selectedRotation, zoneID);
                    return;
                }
            }
        }

        private void OptimizeRoomPlacements() {
            for (int i = 0; i < optimizationPasses; i++)
            {
                bool madeImprovements = false;

                // Get all rooms that have mismatched entrances
                var problematicRooms = placedRooms.Values
                    .Where(room => HasMismatchedEntrances(room))
                    .OrderByDescending(room => CountMismatchedEntrances(room))
                    .ToList();

                foreach (var roomData in problematicRooms)
                {
                    // Try to find a better replacement
                    if (TryFindBetterReplacement(roomData))
                    {
                        madeImprovements = true;
                    }
                }

                // Early exit if no improvements were made
                if (!madeImprovements) break;
            }
        }

        private bool HasMismatchedEntrances(RoomData roomData) {
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
            bool[] entrances = { roomData.entranceDirections.north, roomData.entranceDirections.east,
                               roomData.entranceDirections.south, roomData.entranceDirections.west };

            for (int i = 0; i < 4; i++)
            {
                if (!entrances[i]) continue;

                Vector2Int neighborPos = roomData.position + directions[i];
                if (placedRooms.TryGetValue(neighborPos, out var neighbor))
                {
                    Vector2Int requiredEntrance = -directions[i];
                    bool hasMatchingEntrance =
                        (requiredEntrance == Vector2Int.up && neighbor.entranceDirections.north) ||
                        (requiredEntrance == Vector2Int.right && neighbor.entranceDirections.east) ||
                        (requiredEntrance == Vector2Int.down && neighbor.entranceDirections.south) ||
                        (requiredEntrance == Vector2Int.left && neighbor.entranceDirections.west);

                    if (!hasMatchingEntrance) return true;
                }
            }
            return false;
        }

        private int CountMismatchedEntrances(RoomData roomData) {
            int count = 0;
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
            bool[] entrances = { roomData.entranceDirections.north, roomData.entranceDirections.east,
                               roomData.entranceDirections.south, roomData.entranceDirections.west };

            for (int i = 0; i < 4; i++)
            {
                if (!entrances[i]) continue;

                Vector2Int neighborPos = roomData.position + directions[i];
                if (placedRooms.TryGetValue(neighborPos, out var neighbor))
                {
                    Vector2Int requiredEntrance = -directions[i];
                    bool hasMatchingEntrance =
                        (requiredEntrance == Vector2Int.up && neighbor.entranceDirections.north) ||
                        (requiredEntrance == Vector2Int.right && neighbor.entranceDirections.east) ||
                        (requiredEntrance == Vector2Int.down && neighbor.entranceDirections.south) ||
                        (requiredEntrance == Vector2Int.left && neighbor.entranceDirections.west);

                    if (!hasMatchingEntrance) count++;
                }
            }
            return count;
        }

        private bool TryFindBetterReplacement(RoomData originalRoom) {
            // Determine required entrances based on neighbors
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
            bool[] requiredEntrances = new bool[4];

            for (int i = 0; i < 4; i++)
            {
                Vector2Int neighborPos = originalRoom.position + directions[i];
                if (placedRooms.TryGetValue(neighborPos, out var neighbor))
                {
                    requiredEntrances[i] = true;
                }
            }

            // Get potential replacement rooms
            var potentialRooms = GetPotentialRooms(originalRoom.zoneID,
                requiredEntrances[0], requiredEntrances[1],
                requiredEntrances[2], requiredEntrances[3]);

            // Remove the current room from potential replacements
            potentialRooms.Remove(originalRoom.room);

            if (potentialRooms.Count == 0) return false;

            // Try to find a room that better matches the required entrances
            foreach (var room in potentialRooms.OrderByDescending(r => r.entranceDirections.GetHashCode()))
            {
                var validRotations = GetValidRotations(room, originalRoom.position,
                    requiredEntrances[0], requiredEntrances[1],
                    requiredEntrances[2], requiredEntrances[3]);

                foreach (var rotation in validRotations)
                {
                    var rotatedEntrances = RotateEntranceDirections(room.entranceDirections, rotation);
                    if (CheckEntranceCompatibility(originalRoom.position, rotatedEntrances))
                    {
                        // Found a better match - replace the room
                        ReplaceRoom(originalRoom, room, rotation);
                        return true;
                    }
                }
            }

            return false;
        }

        private bool CheckEntranceCompatibility(Vector2Int position, Room.EntranceDirections entrances) {
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
            bool[] checks = { entrances.north, entrances.east, entrances.south, entrances.west };

            for (int i = 0; i < 4; i++)
            {
                if (!checks[i]) continue;

                Vector2Int neighborPos = position + directions[i];
                if (placedRooms.TryGetValue(neighborPos, out var neighbor))
                {
                    Vector2Int requiredEntrance = -directions[i];
                    bool hasMatchingEntrance =
                        (requiredEntrance == Vector2Int.up && neighbor.entranceDirections.north) ||
                        (requiredEntrance == Vector2Int.right && neighbor.entranceDirections.east) ||
                        (requiredEntrance == Vector2Int.down && neighbor.entranceDirections.south) ||
                        (requiredEntrance == Vector2Int.left && neighbor.entranceDirections.west);

                    if (!hasMatchingEntrance) return false;
                }
            }
            return true;
        }

        private void ReplaceRoom(RoomData originalRoom, Room newRoom, int newRotation) {
            // Remove the old room
            placedRooms.Remove(originalRoom.position);
            gridModule.GetCell(originalRoom.position).occupied = false;

            // Clear any extension cells
            if (originalRoom.room.isLarge && originalRoom.room.extensionDirections != null)
            {
                foreach (var dir in originalRoom.room.extensionDirections)
                {
                    var extensionPos = originalRoom.position + RotateDirection(dir, originalRoom.rotation);
                    var extensionCell = gridModule.GetCell(extensionPos);
                    if (extensionCell != null) extensionCell.occupied = false;
                }
            }

            // Place the new room
            PlaceRoom(originalRoom.position, newRoom, newRotation, originalRoom.zoneID);
        }

        private List<int> GetValidRotations(Room room, Vector2Int position, bool needsNorth, bool needsEast, bool needsSouth, bool needsWest) {
            return Enumerable.Range(0, 4)
                .Select(rot => rot * 90)
                .Where(rot => {
                    var rotatedEntrances = RotateEntranceDirections(room.entranceDirections, rot);
                    return ((needsNorth && rotatedEntrances.north) ||
                           (needsEast && rotatedEntrances.east) ||
                           (needsSouth && rotatedEntrances.south) ||
                           (needsWest && rotatedEntrances.west)) &&
                           AreEntrancesInBounds(position, rotatedEntrances);
                })
                .ToList();
        }

        private List<Room> GetPotentialRooms(int zoneID, bool needsNorth, bool needsEast, bool needsSouth, bool needsWest) {
            var zone = generator.zones.FirstOrDefault(z => z.zoneID == zoneID);
            if (zone == null)
            {
                Debug.LogError($"Zone {zoneID} not found");
                return new();
            }

            // Prioritize rooms that match the required entrance count
            var normal = zone.rooms.normalRooms
                .Where(r => CheckRoomCompatibility(r, needsNorth, needsEast, needsSouth, needsWest))
                .OrderByDescending(r => CountEntrances(r.entranceDirections));

            var required = zone.rooms.requiredRooms
                .Where(r => !IsRequiredRoomPlaced(r, zoneID) &&
                       CheckRoomCompatibility(r, needsNorth, needsEast, needsSouth, needsWest));

            return normal.Concat(required).ToList();
        }

        private bool IsRequiredRoomPlaced(Room room, int zoneID) => placedRooms.Values.Any(r => r.room == room && r.zoneID == zoneID);

        private bool CheckRoomCompatibility(Room room, bool needsNorth, bool needsEast, bool needsSouth, bool needsWest) {
            for (int rot = 0; rot < 4; rot++)
            {
                var e = RotateEntranceDirections(room.entranceDirections, rot * 90);
                if ((needsNorth && e.north) || (needsEast && e.east) || (needsSouth && e.south) || (needsWest && e.west)) return true;
            }
            return false;
        }

        private bool AreEntrancesInBounds(Vector2Int position, Room.EntranceDirections entrances) {
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
            bool[] checks = { entrances.north, entrances.east, entrances.south, entrances.west };

            for (int i = 0; i < 4; i++)
            {
                if (!checks[i]) continue;
                Vector2Int neighbor = position + directions[i];
                if (!gridModule.IsPositionWithinGrid(neighbor)) return false;

                // Check if neighbor is already occupied by a room that doesn't have a matching entrance
                if (placedRooms.TryGetValue(neighbor, out var neighborRoom))
                {
                    Vector2Int requiredEntrance = -directions[i];
                    bool entranceRequired =
                        (requiredEntrance == Vector2Int.up && !neighborRoom.entranceDirections.north) ||
                        (requiredEntrance == Vector2Int.right && !neighborRoom.entranceDirections.east) ||
                        (requiredEntrance == Vector2Int.down && !neighborRoom.entranceDirections.south) ||
                        (requiredEntrance == Vector2Int.left && !neighborRoom.entranceDirections.west);

                    if (entranceRequired) return false;
                }
            }
            return true;
        }

        private int CountEntrances(Room.EntranceDirections dir) {
            int c = 0;
            if (dir.north) c++;
            if (dir.east) c++;
            if (dir.south) c++;
            if (dir.west) c++;
            return c;
        }

        private Room.EntranceDirections RotateEntranceDirections(Room.EntranceDirections original, int degrees) {
            int steps = (degrees / 90) % 4;
            return steps switch {
                1 => new Room.EntranceDirections { north = original.west, east = original.north, south = original.east, west = original.south },
                2 => new Room.EntranceDirections { north = original.south, east = original.west, south = original.north, west = original.east },
                3 => new Room.EntranceDirections { north = original.east, east = original.south, south = original.west, west = original.north },
                _ => new Room.EntranceDirections { north = original.north, east = original.east, south = original.south, west = original.west }
            };
        }

        private Vector2Int RotateDirection(Vector2 dir, int degrees) {
            var v = new Vector2Int(Mathf.RoundToInt(dir.x), Mathf.RoundToInt(dir.y));
            return (degrees / 90 % 4) switch {
                1 => new Vector2Int(-v.y, v.x),
                2 => new Vector2Int(-v.x, -v.y),
                3 => new Vector2Int(v.y, -v.x),
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
            public Vector2Int direction; // The direction the door faces (outward from the room)
            public int zoneID;
            public Room room; // The room this door belongs to
            public bool isZoneConnector;
        }
    }
}