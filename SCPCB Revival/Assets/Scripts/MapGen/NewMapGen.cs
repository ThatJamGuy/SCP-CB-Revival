using UnityEngine;

[System.Serializable]
public class MapSize
{
    public int xSize, ySize;
}

public class NewMapGen : MonoBehaviour
{
    public MapSize mapSize;
    public string mapGenSeed;

    public float roomsize;

    public GameObject cont173;

    public bool creationDone;

    private GameObject mapParent;

    private void MapStart()
    {
        mapParent = new GameObject("Generated Map");
    }

    [ContextMenu("Create Map")]
    public void CreateMap()
    {
        MapStart();

        Debug.Log("Generating map...");
    }
}