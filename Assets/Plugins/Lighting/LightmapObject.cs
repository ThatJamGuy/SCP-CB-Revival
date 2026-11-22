using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Lightmapping/LightmapObject")]
public class LightmapObject : MonoBehaviour
{
    public List<LightmapScene> scenes = new List<LightmapScene>();
    public List<LightmapLightScene> lightScenes = new List<LightmapLightScene>();
    public LightmapBakeType mode = LightmapBakeType.Mixed;
    [NonSerialized] public MaterialPropertyBlock propertyBlock;

    public const int MASK = 1 << 8;

    private void Start()
    {
        propertyBlock = new MaterialPropertyBlock();
    }

    public void ShowScene(int index)
    {
        try
        {
            if (GetComponent<Renderer>() != null)
            {
                var scene = scenes[index];
                // /*
                var list = new List<LightmapData>(LightmapSettings.lightmaps);
                int listindex = list.FindIndex(p => p.lightmapColor == scene.color);
                if (listindex == -1)
                {
                    var data = new LightmapData()
                    {
                        lightmapColor = (Texture2D)scene.color,
                        lightmapDir = (Texture2D)scene.dir,
                        shadowMask = (Texture2D)scene.mask,
                    };
                    list.Add(data);
                    listindex = list.Count - 1;
                }
                // LightmapSettings.lightmaps = list.ToArray();
                // */
                var renderer = GetComponent<Renderer>();
                var materials = new List<Material>();
                renderer.GetMaterials(materials);
                for (int i = 0; i < materials.Count; i++)
                {
                    materials[i].SetTexture("_CustomLightmap", scene.color);
                    materials[i].SetVector("_CustomLightmapST", scene.offset);
                    materials[i].SetTexture("_CustomDirLightmap", scene.dir);
                    materials[i].SetMatrix("_CustomMatrix", scene.bakedWorldTransform);
                }
                renderer.SetMaterials(materials);
                // renderer.lightmapIndex = listindex;
                // renderer.lightmapIndex = -1;
                renderer.lightmapScaleOffset = scene.offset;
                if (renderer.gameObject.layer == 0)
                {
                    // renderer.gameObject.layer = LayerMask.NameToLayer("Room");
                    // renderer.renderingLayerMask = MASK;
                }
            }
            if (GetComponent<Light>() != null)
            {
                var scene = lightScenes[index];
                var light = GetComponent<Light>();
                if (mode == 0)
                    mode = LightmapBakeType.Mixed;
                light.bakingOutput = new LightBakingOutput()
                {
                    probeOcclusionLightIndex = -1,//scene.probeOcclusionLightIndex,
                    occlusionMaskChannel = -1,//scene.occlusionMaskChannel,
                    lightmapBakeType = mode,//scene.lightmapBakeType,
                    mixedLightingMode = scene.mixedLightingMode,
                    // isBaked = false,
                    isBaked = scene.isBaked,
                };
                light.shadows = LightShadows.None;
                // light.renderingLayerMask = int.MaxValue & (~MASK);
                if (scene.isBaked && mode == LightmapBakeType.Baked)
                {
                    light.enabled = false;
                }
            }
        }
        catch (ArgumentOutOfRangeException)
        {}
    }
}

[System.Serializable]
public class LightmapScene
{
    public Texture2D color;
    public Texture2D dir;
    public Texture2D mask;
    public Vector4 offset;
    public Matrix4x4 bakedWorldTransform;
}

[System.Serializable]
public class LightmapLightScene
{
    public int probeOcclusionLightIndex;
    public int occlusionMaskChannel;
    public LightmapBakeType lightmapBakeType;
    public MixedLightingMode mixedLightingMode;
    public bool isBaked;
}
