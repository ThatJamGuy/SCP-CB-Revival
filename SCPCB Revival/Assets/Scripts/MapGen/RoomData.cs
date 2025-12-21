using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "RoomData", menuName = "SCPCBR/Room Data")]
public class RoomData : ScriptableObject {
    public enum RoomShape { TwoWay, ThreeWay, FourWay, Corner, DeadEnd, Checkpoint }
    public enum Zone { LCZ, HCZ, EZ }

    [Header("Room Settings")]
    public RoomShape roomShape;
    public Zone roomZone;
    public bool isUnique;
    public bool mustSpawn;

    [Header("Room References")]
    public AssetReferenceGameObject roomPrefab;
}