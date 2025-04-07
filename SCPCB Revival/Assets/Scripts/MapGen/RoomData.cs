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
    [Space(5)]
    [SerializeField, BoxGroup("Entrances")] private bool hasNorthEntrance;
    [SerializeField, BoxGroup("Entrances")] private bool hasEastEntrance;
    [SerializeField, BoxGroup("Entrances")] private bool hasSouthEntrance;
    [SerializeField, BoxGroup("Entrances")] private bool hasWestEntrance;

    [Space(10)]
    [Range(0, 1)] public float spawnWeight = 0.5f;
    public bool canRotate = true;

    [Header("Advanced Settings")]
    [Tooltip("Additional weight added when connecting to other rooms")]
    public float connectionBonusWeight = 0.2f;

    [Space(5)]
    [Tooltip("Does the room expand into other cells?")]
    public bool isLarge = false;
    [Tooltip("In which directions and by how much does the room expand?")]
    [ShowIf(nameof(isLarge))] public Vector2[] extendedSize;

    public enum RoomType { Normal, ZoneConnection, MustSpawn, DeadEnd }
    public enum RoomShape { TwoWay, ThreeWay, FourWay, Corner, DeadEnd }
    public enum CardinalDirection { North, East, South, West }

    // Cache the HashSet and rebuild it when needed
    private HashSet<CardinalDirection> _entranceDirections;
    public HashSet<CardinalDirection> entranceDirections
    {
        get
        {
            if (_entranceDirections == null)
            {
                RebuildEntranceDirections();
            }
            return _entranceDirections;
        }
    }

    private void RebuildEntranceDirections()
    {
        _entranceDirections = new HashSet<CardinalDirection>();

        if (hasNorthEntrance) _entranceDirections.Add(CardinalDirection.North);
        if (hasEastEntrance) _entranceDirections.Add(CardinalDirection.East);
        if (hasSouthEntrance) _entranceDirections.Add(CardinalDirection.South);
        if (hasWestEntrance) _entranceDirections.Add(CardinalDirection.West);
    }

    private void OnValidate()
    {
        // Force rebuild of entrance directions
        RebuildEntranceDirections();

        // Validate shape matches entrances
        ValidateShapeAndEntrances();

        // Other validations
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

    private void ValidateShapeAndEntrances()
    {
        int entranceCount = (hasNorthEntrance ? 1 : 0) + (hasEastEntrance ? 1 : 0) +
                           (hasSouthEntrance ? 1 : 0) + (hasWestEntrance ? 1 : 0);

        RoomShape expectedShape = entranceCount switch
        {
            1 => RoomShape.DeadEnd,
            2 => hasNorthEntrance && hasSouthEntrance ? RoomShape.TwoWay :
                 hasEastEntrance && hasWestEntrance ? RoomShape.TwoWay : RoomShape.Corner,
            3 => RoomShape.ThreeWay,
            4 => RoomShape.FourWay,
            _ => roomShape
        };

        if (roomShape != expectedShape)
        {
            Debug.LogWarning($"Room {roomName}: Entrance configuration suggests {expectedShape} but shape is set to {roomShape}");
        }
    }

#if UNITY_EDITOR
    [Button("Show Current Directions")]
    private void ShowCurrentDirections()
    {
        var directions = new System.Collections.Generic.List<string>();
        if (hasNorthEntrance) directions.Add("North");
        if (hasEastEntrance) directions.Add("East");
        if (hasSouthEntrance) directions.Add("South");
        if (hasWestEntrance) directions.Add("West");

        string directionString = string.Join(", ", directions);
        Debug.Log($"[{roomName}] Current entrance directions: {directionString}");
    }
#endif
}