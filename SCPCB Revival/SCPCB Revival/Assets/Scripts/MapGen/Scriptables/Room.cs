using UnityEngine;
using UnityEngine.AddressableAssets;
using NaughtyAttributes;

namespace vectorarts.scpcbr {
    [CreateAssetMenu(fileName = "New Room", menuName = "MapGen/Room")]
    public class Room : ScriptableObject {
        public AssetReferenceGameObject roomPrefab;

        [Header("Room Properties")]
        public RoomShape roomShape;
        public RoomType roomType;
        public EntranceDirections entranceDirections;

        [Header("Advanced Settings")]
        public bool isLarge;
        [ShowIf(nameof(isLarge))] public Vector2[] extensionDirections;
        public bool validForStartingRoom;
        [Range(0.1f, 1), MinValue(0.1f), MaxValue(1)] public float roomChance = 0.5f;

        public enum RoomShape { TwoWay, ThreeWay, FourWay, Corner, Endroom }
        public enum RoomType { Normal, Required, CheckpointOrSurfaceExit }

        [System.Serializable]
        public struct EntranceDirections {
            public bool north, east, south, west;
        }
    }
}