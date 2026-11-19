using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Tracks and updates movement between RoomTriggers. Place on any object that needs to move between rooms.
/// </summary>
public class RoomListener : MonoBehaviour
{
    public RoomTrigger activeRoom; //the room the object is currently in.
    private const int ROOM_LAYER = 2; //configure to be the index of whatever layer Ambience Zones are on.
    private const int ROOM_MASK = 1 << ROOM_LAYER; 

    private readonly Collider[] overlapBuffer = new Collider[16]; // Reusable buffer (physics query fallback safety)

    public Action<RoomTrigger> OnRoomChanged;

    /// <summary>
    /// Very simple layer check to quickly ignore objects that arent rooms before commiting to more expensive checks
    /// </summary>
    private bool IsRoom(Collider col)
    {
        return (ROOM_MASK & (1 << col.gameObject.layer)) != 0;
    }

    private void OnEnable()
    {
        //force a manual update to make sure we ALWAYS start in a room regardles of unity's physics system.
        ManualUpdate();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsRoom(other)) return;

        ManualUpdate();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsRoom(other)) return;
        ManualUpdate();
    }

    private void SetActiveRoom(RoomTrigger newRoom)
    {
        if (newRoom == activeRoom)
            return;

        activeRoom = newRoom;
        OnRoomChanged?.Invoke(newRoom);
    }


    /// <summary>
    /// Updates the listeners current room. Since we're only calling it in the trigger events its not massively expensive.
    /// We solve for overlapping AmbienceZones by prioritizing the first found room currently. 
    /// </summary>
    [ContextMenu("Force Update Rooms")]
    public void ManualUpdate()
    {
        RoomTrigger found = null;

        int count = Physics.OverlapSphereNonAlloc(transform.position, 0.1f, overlapBuffer, ROOM_MASK, QueryTriggerInteraction.Collide);

        for (int i = 0; i < count; i++)
        {
            if (overlapBuffer[i].TryGetComponent(out found))
                break;
        }

        SetActiveRoom(found);
    }
}
