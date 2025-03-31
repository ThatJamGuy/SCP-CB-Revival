using UnityEngine;
using NaughtyAttributes;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Settings")]
    public string seed;
    public ZoneData[] zones;

    [Button("Generate Map")]
    public void GenerateMap()
    {

    }
}