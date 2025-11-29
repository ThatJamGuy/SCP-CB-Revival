using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using DigitalOpus.MB.Core;
using System.Text;
using System.Linq;
using static DigitalOpus.MB.MBEditor.GameObjectFilterInfo;

namespace DigitalOpus.MB.MBEditor
{

    public interface IGroupByFilter
    {

        /// <summary>
        /// 	this name appears in the dropdown list.
        /// </summary>
        /// <returns>The name.</returns>
        string GetName();

        /// <summary>
        /// 	returns a description of the game object for this filter
        ///     eg. renderType=MeshFilter
        /// </summary>
        /// <returns>The description.</returns>
        /// <param name="fi">Fi.</param>
        string GetDescription(GameObjectFilterInfo fi);

        /// <summary>
        ///      For sorting Similar to IComparer.Compare
        /// </summary>
        int Compare(GameObjectFilterInfo a, GameObjectFilterInfo b);
    }

    [Serializable]
    public class GameObjectFilterInfo : IComparable
    {

        public enum TransparentOrOpaque
        {
            NotApplicable = -1,
            Opaque = 0,
            Cutout = 1,
            Fade = 2,
            Transparent = 3,
        }

        public enum URPBlendMode // called SurfaceType in URP
        {
            Opaque = 0,
            Transparent = 1
        }

        public enum HDRPBlendMode // called SurfaceType in HDRP
        {
            Opaque = 0,
            Transparent = 1
        }

        public int[] URPBlendMode_2_StandardBlendMode = new int[]
        {
            (int) TransparentOrOpaque.Opaque, 
            (int) TransparentOrOpaque.Transparent
        };

        public int[] HDRPBlendMode_2_StandardBlendMode = new int[]
        {
            (int) TransparentOrOpaque.Opaque,
            (int) TransparentOrOpaque.Transparent
        };

        public enum MetallicOrSpecular
        {
            NotApplicable = -1,
            Specular = 0,
            Metallic = 1
        }


        public GameObject go;
        public string shaderName = "";
        public Shader[] shaders = null;  //distinct set of shaders used


        public string materialName = "";
        public Material[] materials = null; //disinct set of materials used
        public string standardShaderBlendModesName = "";
        public string metallicOrSpecularName = "";
        public TransparentOrOpaque[] transparencies = null;
        public MetallicOrSpecular[] metallicOrSpeculars = null;
        public bool outOfBoundsUVs = false;
        public bool submeshesOverlap = false;
        public bool alreadyInBakerList = false;
        public string setOfMaps;
        public int numMaterials = 1;
        public int lightmapIndex = -1;
        public int layerIndex = -1;
        public int numVerts = 0;
        public bool isStatic = false;
        public bool isMeshRenderer = true;
        public StringBuilder warning = new StringBuilder();
        public short atlasIndex = 0;

        IGroupByFilter[] filters;


        public int CompareTo(System.Object obj)
        {
            if (obj is GameObjectFilterInfo)
            {
                GameObjectFilterInfo gobj = (GameObjectFilterInfo)obj;
                int gb;

                for (int i = 0; i < filters.Length; i++)
                {
                    if (filters[i] != null)
                    {
                        gb = filters[i].Compare(this, gobj);
                        if (gb != 0) return gb;
                    }
                }
            }
            return 0;
        }

        public GameObjectFilterInfo(GameObject g, HashSet<GameObject> objsAlreadyInBakers, IGroupByFilter[] filts)
        {
            go = g;
            Renderer r = MB_Utility.GetRenderer(g);
            //material = r.sharedMaterial;
            //if (material != null) shader = material.shader;
            HashSet<Material> matsSet = new HashSet<Material>();
            HashSet<Shader> shaderSet = new HashSet<Shader>();
            if (r is SkinnedMeshRenderer) isMeshRenderer = false;
            else isMeshRenderer = true;
            materials = r.sharedMaterials;
            //shaders = new Shader[materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null)
                {
                    matsSet.Add(materials[i]);
                    shaderSet.Add(materials[i].shader);
                }
            }
            materials = new Material[matsSet.Count];
            matsSet.CopyTo(materials);
            shaders = new Shader[shaderSet.Count];
            transparencies = new TransparentOrOpaque[matsSet.Count];
            metallicOrSpeculars = new MetallicOrSpecular[matsSet.Count];

            shaderSet.CopyTo(shaders);

            Array.Sort(materials, new NameComparer());
            Array.Sort(shaders, new NameComparer());

            List<string> standardShaderBlendModesNameSet = new List<string>();
            List<string> metallicSpecularNameSet = new List<string>();
            for (int i = 0; i < materials.Length; i++)
            {
                {
                    if (materials[i].renderQueue > (int)UnityEngine.Rendering.RenderQueue.GeometryLast)
                    {
                        transparencies[i] = TransparentOrOpaque.Transparent;
                    }
                    else
                    {
                        transparencies[i] = TransparentOrOpaque.Opaque;
                    }
                }

                if (materials[i].shader.name.StartsWith("Standard"))
                {
                    {
                        if (materials[i].HasProperty("_SpecGlossMap") || materials[i].HasProperty("_SpecColor"))
                        {
                            metallicOrSpeculars[i] = MetallicOrSpecular.Specular;
                        }
                        else
                        {
                            metallicOrSpeculars[i] = MetallicOrSpecular.Metallic;
                        }
                    }

                }
                else if (materials[i].shader.name.StartsWith("Universal Render Pipeline") && materials[i].HasProperty("_Surface"))
                {
                    {
                        // Check URP blend mode (opaque, transparent)
                        int surfaceMode = (int)materials[i].GetFloat("_Surface");
                        if (surfaceMode < 0 || surfaceMode > (int)URPBlendMode.Transparent)
                        {
                            Debug.LogError("Unsupported surface mode, were more surface modes added to the URP?");
                            surfaceMode = Mathf.Clamp(surfaceMode, (int)URPBlendMode.Opaque, (int)URPBlendMode.Transparent);
                        }
                    }

                    {
                        // Check URP workflow (specular, transparent)
                        if (materials[i].HasProperty("_WorkflowMode"))
                        {
                            if (materials[i].GetFloat("_WorkflowMode") == (int)MetallicOrSpecular.Specular)
                            {
                                metallicOrSpeculars[i] = MetallicOrSpecular.Specular;
                            }
                            else
                            {
                                metallicOrSpeculars[i] = MetallicOrSpecular.Metallic;
                            }
                        }
                        else if (materials[i].HasProperty("_Metallic"))
                        {
                            metallicOrSpeculars[i] = MetallicOrSpecular.Metallic;
                        }
                        else
                        {
                            metallicOrSpeculars[i] = MetallicOrSpecular.NotApplicable;
                        }

                    }
                }
                else if (materials[i].shader.name.StartsWith("HDRP") && materials[i].HasProperty("_SurfaceType"))
                {
                    int surfaceMode = (int)materials[i].GetFloat("_SurfaceType");
                    if (surfaceMode < 0 || surfaceMode > (int)HDRPBlendMode.Transparent)
                    {
                        Debug.LogError("Unsupported surface mode, were more surface modes added to the HDRP?");
                        surfaceMode = Mathf.Clamp(surfaceMode, (int)HDRPBlendMode.Opaque, (int)HDRPBlendMode.Transparent);
                    }

                    metallicOrSpeculars[i] = MetallicOrSpecular.Metallic; // HDRP is always metallic
                }
                else
                {
                    metallicOrSpeculars[i] = MetallicOrSpecular.NotApplicable;
                }

                if (!standardShaderBlendModesNameSet.Contains(transparencies[i].ToString()))
                {
                    standardShaderBlendModesNameSet.Add(transparencies[i].ToString());
                }

                if (!metallicSpecularNameSet.Contains(metallicOrSpeculars[i].ToString()))
                {
                    metallicSpecularNameSet.Add(metallicOrSpeculars[i].ToString());
                }

                materialName += (materials[i] == null ? "null" : materials[i].name);
                if (i < materials.Length - 1) materialName += ",";
            }

            standardShaderBlendModesName = "";
            standardShaderBlendModesNameSet.Sort();
            foreach (string modeName in standardShaderBlendModesNameSet)
            {
                standardShaderBlendModesName += modeName + ",";
            }

            metallicOrSpecularName = "";
            metallicSpecularNameSet.Sort();
            foreach (string permutationName in metallicSpecularNameSet)
            {
                metallicOrSpecularName += permutationName + ",";
            }

            for (int i = 0; i < shaders.Length; i++)
            {
                shaderName += (shaders[i] == null ? "null" : shaders[i].name);
                if (i < shaders.Length - 1) shaderName += ",";
            }

            {
                List<string> listOfMaps = new List<string>();
                
                for (int i = 0; i < materials.Length; i++)
                {
                    listOfMaps.Clear();
                    List<ShaderTextureProperty> texPropertyNamesList = new List<ShaderTextureProperty>();
                    List<ShaderTextureProperty> customPropNames_Unused = new List<ShaderTextureProperty>();
                    MBVersion.CollectPropertyNames(texPropertyNamesList,
                                                   MB3_TextureCombinerPipeline.shaderTexPropertyNames,
                                                   customPropNames_Unused,
                                                   materials[i],
                                                   MB2_LogLevel.info);

                    string[] propertyNames = texPropertyNamesList.Select(stp => stp.name).ToArray();

                    foreach (string propName in propertyNames)
                    {
                        if (listOfMaps.Contains(propName))
                        {
                            // Already added, don't add again
                        }
                        else if ( MB3_TextureCombinerPipeline.shaderTexPropertiesToExclude.Contains(propName))
                        {
                            // Don't add
                        }
                        else
                        {
                            if (materials[i].HasProperty(propName) && materials[i].GetTexture(propName) != null)
                            {
                                listOfMaps.Add(propName);
                            }
                        }
                    }
                }

                listOfMaps.Sort();
                if (listOfMaps.Count == 0)
                {
                    setOfMaps = "None";
                }
                else
                {
                    setOfMaps = string.Join(", ", listOfMaps);
                }
            }

            lightmapIndex = r.lightmapIndex;
            Mesh mesh = MB_Utility.GetMesh(g);
            numVerts = 0;
            if (mesh != null)
            {
                numVerts = mesh.vertexCount;
            }
            isStatic = go.isStatic;
            numMaterials = materials.Length;
            warning = new StringBuilder();
            alreadyInBakerList = objsAlreadyInBakers.Contains(g);
            outOfBoundsUVs = false;
            submeshesOverlap = false;
            filters = filts;
            layerIndex = g.layer;
        }

        public string GetDescription(IGroupByFilter[] groupBy, GameObjectFilterInfo fi)
        {
            string desc = "";
            if (groupBy != null)
            {
                for (int i = 0; i < groupBy.Length; i++)
                {
                    desc += groupBy[i].GetDescription(fi) + " ";
                }
                return desc;
            }
            else
            {
                return "todo";
            }
        }

        private class NameComparer : Comparer<UnityEngine.Object>
        {
            public override int Compare(UnityEngine.Object a, UnityEngine.Object b)
            {
                return a.name.CompareTo(b.name);
            }
        }
    }
}



