using UnityEngine;

[CreateAssetMenu(fileName = "New Room Data", menuName = "Map Generation/Room Data")]
public class RoomData : ScriptableObject
{
    [Header("Basic Information")]
    public string roomName;
    public RoomShape shape;
    public GameObject roomPrefab;

    [Header("Size Configuration")]
    public bool isLargeRoom;
    public Vector2Int expansionAmount; // X = horizontal expansion, Y = vertical expansion

    [Header("Special Properties")]
    public bool isExitRoom;
    public bool mustSpawn;
    public bool spawnOnce;

    public enum RoomShape
    {
        EndRoom,
        Hallway,
        TShapeHallway,
        FourWayHallway,
        LShapeHallway
    }

    // Add custom validation if needed in a separate editor script
}