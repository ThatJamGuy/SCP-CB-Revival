using UnityEngine;

public enum RoomType
{
    DeadEnd,
    TwoWay,
    ThreeWay,
    FourWay
}

[CreateAssetMenu(fileName = "RoomData", menuName = "MapGen/RoomData", order = 1)]
public class RoomData : ScriptableObject
{
    public GameObject roomPrefab;
    public RoomType roomType;
    public bool isImportant;
}
