using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace vectorarts.scpcbr {
    [System.Serializable]
    public class RoomInstantiationModule {
        [Header("Instantiation Settings")]
        public Transform roomParent;
        public bool instantiateOnGeneration = true;

        private MapGenerator generator;
        private GridCreationModule gridModule;
        private RoomPlacementModule placementModule;

        private readonly Dictionary<Vector2Int, GameObject> activeRooms = new();
        private readonly Dictionary<string, Queue<GameObject>> roomPool = new();

        public void Initialize(MapGenerator generator, GridCreationModule gridModule, RoomPlacementModule placementModule) {
            this.generator = generator;
            this.gridModule = gridModule;
            this.placementModule = placementModule;
            roomParent ??= generator.transform;
        }

        public async Task InstantiateAllRooms() {
            RecycleAllRooms();

            var placedRooms = placementModule.GetPlacedRooms();
            foreach (var roomData in placedRooms)
                await InstantiateOrReuseRoom(roomData);

            Debug.Log($"Instantiated {activeRooms.Count} rooms (pooled: {roomPool.Count})");
        }

        public async Task InstantiateAllDoors() {
            var doorPlacementTasks = placementModule.GetDoorPlacements()
                .Select(doorData => InstantiateDoor(doorData));
            await Task.WhenAll(doorPlacementTasks); // Await all door instantiation tasks
        }

        private void RecycleAllRooms() {
            foreach (var kvp in activeRooms)
            {
                var room = kvp.Value;
                if (room != null)
                {
                    room.SetActive(false);
                    string key = room.name.Split('_')[0];
                    if (!roomPool.ContainsKey(key))
                        roomPool[key] = new Queue<GameObject>();
                    roomPool[key].Enqueue(room);
                }
            }
            activeRooms.Clear();
        }

        private async Task InstantiateOrReuseRoom(RoomPlacementModule.RoomData roomData) {
            if (roomData.room?.roomPrefab == null)
            {
                Debug.LogError($"Invalid room data at position {roomData.position}");
                return;
            }

            string roomKey = roomData.room.name;
            Vector3 position = gridModule.GetWorldPosition(roomData.position);
            Quaternion rotation = Quaternion.Euler(0, roomData.rotation, 0);
            GameObject roomInstance = null;

            if (roomPool.TryGetValue(roomKey, out var pool) && pool.Count > 0)
            {
                roomInstance = pool.Dequeue();
                roomInstance.transform.SetPositionAndRotation(position, rotation);
                roomInstance.transform.SetParent(roomParent);
                roomInstance.SetActive(true);
            }
            else
            {
                var handle = Addressables.LoadAssetAsync<GameObject>(roomData.room.roomPrefab);
                await handle.Task;

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError($"Failed to load room prefab for {roomKey} at {roomData.position}");
                    return;
                }

                var prefab = handle.Result;
                if (prefab == null) return;

                if (Application.isPlaying)
                {
                    roomInstance = Object.Instantiate(prefab, position, rotation, roomParent);
                }
                else
                {
#if UNITY_EDITOR
                    roomInstance = UnityEditor.PrefabUtility.InstantiatePrefab(prefab, roomParent) as GameObject;
                    if (roomInstance != null)
                    {
                        roomInstance.transform.SetPositionAndRotation(position, rotation);
                    }
#endif
                }

                Addressables.Release(handle);
            }

            if (roomInstance == null) return;

            roomInstance.name = $"{roomKey}_{roomData.position.x}_{roomData.position.y}";
            activeRooms[roomData.position] = roomInstance;

            var metadata = roomInstance.GetComponent<RoomMetadata>() ?? roomInstance.AddComponent<RoomMetadata>();
            metadata.roomData = new RoomMetadata.SerializableRoomData {
                roomName = roomKey,
                position = roomData.position,
                rotation = roomData.rotation,
                zoneID = roomData.zoneID
            };
        }

        private async Task InstantiateDoor(RoomPlacementModule.DoorData doorData) {
            GameObject doorPrefab = null;
            Vector3 position = gridModule.GetWorldPosition(doorData.position);

            if (doorData.isZoneConnector)
            {
                // Zone connector door - get prefab from next zone
                var nextZone = generator.zones.FirstOrDefault(z => z.zoneID == doorData.nextZoneID);
                if (nextZone != null)
                {
                    doorPrefab = nextZone.zoneDoorPrefab;
                    // Position between zones
                    position += new Vector3(doorData.direction.x, 0, doorData.direction.y) * (gridModule.cellSize * 0.75f);
                }
            }
            else
            {
                // Normal zone door
                var zone = generator.zones.FirstOrDefault(z => z.zoneID == doorData.zoneID);
                doorPrefab = zone?.zoneDoorPrefab;
                position += new Vector3(doorData.direction.x, 0, doorData.direction.y) * (gridModule.cellSize / 2);
            }

            if (doorPrefab == null) return;

            Quaternion rotation = Quaternion.LookRotation(new Vector3(doorData.direction.x, 0, doorData.direction.y));

            await Task.Yield(); // Switch back to the main thread
            GameObject doorInstance = Object.Instantiate(doorPrefab, position, rotation, roomParent);
            doorInstance.name = doorData.isZoneConnector
                ? $"ZoneConnector_{doorData.zoneID}_to_{doorData.nextZoneID}"
                : $"Door_{doorData.position.x}_{doorData.position.y}";
        }
    }

    public class RoomMetadata : MonoBehaviour {
        [System.Serializable]
        public class SerializableRoomData {
            public string roomName;
            public Vector2Int position;
            public int rotation;
            public int zoneID;
        }

        public SerializableRoomData roomData;
    }
}