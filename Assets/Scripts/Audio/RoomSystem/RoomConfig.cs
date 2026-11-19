using UnityEngine;

/// <summary>
/// Configuration for any given rooms runtime events, such as updating reverb, invoking room events and updating post processing.
/// This system is not apart of map generation, and thus is detatched from RoomData.cs
/// </summary>
[CreateAssetMenu(fileName = "Room Config", menuName = "SCP:CBR/Audio/Room Config")]
public class RoomConfig : ScriptableObject
{
    public ReverbEnum reverbType;
}
