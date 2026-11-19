using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

/// <summary>
/// Handles all updates related to the room system.
/// </summary>
public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;
    private RoomConfig currentRoomConfig;

    private void Awake()
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
        AudioManager.Instance.UpdateReverb(roomTrigger);
    }
}
