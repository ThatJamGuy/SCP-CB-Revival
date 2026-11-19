using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

/// <summary>
/// Handles all updates related to the room system.
/// </summary>
public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;
    public RoomConfig currentRoomConfig;

    private void Start()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        Player.Instance.roomListener.OnRoomChanged += UpdateRoomConfig;
        UpdateRoomConfig(Player.Instance.roomListener.activeRoom);

    }

    /// <summary>
    /// Sets the active room config. For multiplayer, make sure each player can manage their own changes to the appropreate systems.
    /// </summary>
    public void UpdateRoomConfig(RoomTrigger roomTrigger)
    {
        currentRoomConfig = roomTrigger.roomConfig;
        AudioManager.Instance.UpdateRoom(roomTrigger);
    }

    [ContextMenu("Force Update Rooms")]
    public void UpdateRegistry()
    {
        Player.Instance.roomListener.OnRoomChanged += UpdateRoomConfig;
        UpdateRoomConfig(Player.Instance.roomListener.activeRoom);
    }
}
