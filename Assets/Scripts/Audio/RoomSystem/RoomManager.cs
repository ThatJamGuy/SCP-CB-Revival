using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Handles all updates related to the room system.
/// </summary>
public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;


    public RoomConfig currentRoomConfig;


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }


    /// <summary>
    /// Sets the active room config. For multiplayer, make sure each player can manage their own changes to the appropreate systems.
    /// </summary>
    public void UpdateRoomConfig(RoomTrigger roomTrigger)
    {
        currentRoomConfig = roomTrigger.roomConfig;
        AudioManager.Instance.UpdateRoom(roomTrigger);
        PostProcessingManager.Instance.UpdateRoom(roomTrigger);
    }

    /// <summary>
    /// Subscribes the roommanager to the players listener event.
    /// </summary>
    [ContextMenu("Force Update Rooms")]
    public void UpdateRegistry()
    {
        Player.Instance.roomListener.OnRoomChanged += UpdateRoomConfig;
        Player.Instance.roomListener.ManualUpdate();
    }
}