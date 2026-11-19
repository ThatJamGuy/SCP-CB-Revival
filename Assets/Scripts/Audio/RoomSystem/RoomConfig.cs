using FMODUnity;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Configuration for any given rooms runtime events, such as updating reverb, invoking room events and updating post processing.
/// This system is not apart of map generation, and thus is detatched from RoomData.cs
/// </summary>
[CreateAssetMenu(fileName = "Room Config", menuName = "SCP:CBR/Audio/Room Config")]
public class RoomConfig : ScriptableObject
{
    [Header("Sound Scape")]
    [Tooltip("Sets the rooms Reverb Type")] 
    public ReverbEnum reverbType;

    [Tooltip("Sets what ambience will play when inside the room")] 
    public EventReference ambienceSoundScape;

    [Space(5), Header("Post Processing")]

    [Tooltip("What tint should the room be? Colours are HDR so they match unity's settings."), ColorUsage(false, true)] 
    public Color colorFilter;

    [Tooltip("How saturated should this room be? Values are intentionally clamped low as any changes should be subtle."), Range(-6, 6)] 
    public float saturationValue;
}
