using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Lightmapping/LightmapPrefab")]
public class LightmapPrefab : MonoBehaviour
{
    public static List<LightmapPrefab> prefabs = new List<LightmapPrefab>();
    public List<LightmapPrefabScene> scenes = new List<LightmapPrefabScene>();
    private bool hasRun = false;
    public Bounds bounds;

    private void Start()
    {
        if (!hasRun)
            ShowScene(0);
        prefabs.Add(this);
    }

    private void OnDestroy()
    {
        prefabs.Remove(this);
    }

    public void ShowScene(int index)
    {
        hasRun = true;
        LightmapSettings.lightmapsMode = LightmapsMode.NonDirectional;
        foreach (var item in scenes)
        {
            foreach (var obj in item.objects)
            {
                obj.SetActive(false);
            }
        }
        foreach (var obj in scenes[index].objects)
        {
            obj.SetActive(true);
        }
        foreach (var obj in GetComponentsInChildren<LightmapObject>(true))
        {
            obj.ShowScene(index);
        }
    }
}

[System.Serializable]
public class LightmapPrefabScene
{
    public List<GameObject> objects = new List<GameObject>();
}
