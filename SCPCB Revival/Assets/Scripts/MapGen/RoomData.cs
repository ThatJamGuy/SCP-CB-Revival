using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "New Room", menuName = "Map Generator/Room Data")]
public class RoomData : ScriptableObject
{
    [Header("Basic Info")]
    public string roomName;
    public RoomType roomType;
    public RoomShape roomShape;

    [Header("Prefab Reference")]
    public AssetReferenceGameObject roomPrefab;

    [Header("Connection Settings")]
    public HashSet<CardinalDirection> entranceDirections = new HashSet<CardinalDirection>();
    [Range(0, 1)] public float spawnWeight = 0.5f;
    public bool canRotate = true;

    [Header("Advanced Settings")]
    [Tooltip("Does the room expand into other cells?")]
    public float connectionBonusWeight = 0.2f;

    [Tooltip("Does the room expand into other cells?")]
    public bool isLarge = false;
    [Tooltip("In which directions and by how much does the room expand?")]
    [ShowIf(nameof(isLarge))] public Vector2[] extendedSize;

    public enum RoomType { Normal, ZoneConnection, MustSpawn, DeadEnd }
    public enum RoomShape { TwoWay, ThreeWay, FourWay, Corner, DeadEnd }
    public enum CardinalDirection { North, East, South, West }

#if UNITY_EDITOR
    [ContextMenu("Auto-Set Directions Based on Shape")]
    private void AutoSetDirections()
    {
        entranceDirections.Clear();

        var shapeDirections = new Dictionary<RoomShape, CardinalDirection[]>
        {
            { RoomShape.TwoWay, new[] { CardinalDirection.North, CardinalDirection.South } },
            { RoomShape.ThreeWay, new[] { CardinalDirection.South, CardinalDirection.East, CardinalDirection.West } },
            { RoomShape.FourWay, new[] { CardinalDirection.North, CardinalDirection.East, CardinalDirection.South, CardinalDirection.West } },
            { RoomShape.Corner, new[] { CardinalDirection.South, CardinalDirection.East } },
            { RoomShape.DeadEnd, new[] { CardinalDirection.South } }
        };

        if (shapeDirections.TryGetValue(roomShape, out var directions))
        {
            foreach (var direction in directions)
            {
                entranceDirections.Add(direction);
            }
        }
    }
#endif

    private void OnValidate()
    {
        if (roomPrefab == null)
        {
            Debug.LogWarning($"{nameof(roomPrefab)} is not assigned in {nameof(RoomData)}: {roomName}");
        }

        if (spawnWeight < 0 || spawnWeight > 1)
        {
            Debug.LogWarning($"{nameof(spawnWeight)} should be between 0 and 1 in {nameof(RoomData)}: {roomName}");
        }

        if (isLarge && (extendedSize == null || extendedSize.Length == 0))
        {
            Debug.LogWarning($"{nameof(extendedSize)} must be assigned for large rooms in {nameof(RoomData)}: {roomName}");
        }
    }
}