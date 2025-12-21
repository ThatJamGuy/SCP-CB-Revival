using UnityEngine;

[CreateAssetMenu(fileName = "NewMapTemplate", menuName = "SCPCBR/Map Template")]
public class MapTemplate : ScriptableObject {
    public ZoneTemplate[] zones;
    public RoomPlacement[] roomPlacements;
}

[System.Serializable]
public class ZoneTemplate {
    public RoomData.Zone zoneType;
    public int zoneWidth = 7;
    public int zoneHeight = 3;
    public RoomData checkpointRoomVariant;
    public Color debugColor = Color.white;
}

[System.Serializable]
public class RoomPlacement {
    public Vector2Int gridPosition;
    public RoomData.RoomShape requiredShape;
    public int rotation;
}