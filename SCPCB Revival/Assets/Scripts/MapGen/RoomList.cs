using UnityEngine;

[CreateAssetMenu(fileName = "RoomList", menuName = "Map Generation/RoomList")]
public class RoomList : ScriptableObject
{
    [Header("General Room Lists")]
    public RoomData[] lightRooms;
    public RoomData[] heavyRooms;
    public RoomData[] entranceRooms;

    [Header("Checkpoint Rooms")]
    public RoomData lcz_hcz_Checkpoint;
    public RoomData hcz_ent_Checkpoint;
}
