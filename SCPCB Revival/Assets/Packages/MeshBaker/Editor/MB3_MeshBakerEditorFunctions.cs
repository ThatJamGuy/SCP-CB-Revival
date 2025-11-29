using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DigitalOpus.MB.Core;
using UnityEditor;
using DigitalOpus.MB.MBEditor;


namespace DigitalOpus.MB.MBEditor
{
    public static class MB3_MeshBakerEditorFunctions
    {
        /// <summary>
        /// Used by UnityEditorInspectors for background colors
        /// </summary>
        public static Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];

            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();

            return result;
        }

        public static bool BakeIntoCombined(MB3_MeshBakerCommon mom, out bool createdDummyTextureBakeResults)
        {
            SerializedObject so = null;
            return BakeIntoCombined(mom, out createdDummyTextureBakeResults, ref so);
        }

        /// <summary>
        ///  Bakes a combined mesh.
        /// </summary>
        /// <param name="so">If this is being called from Inspector code then pass in the SerializedObject for the component.
        /// This is necessary for "bake into prefab" which can corrupt the SerializedObject.</param>
        public static bool BakeIntoCombined(MB3_MeshBakerCommon mom, out bool createdDummyTextureBakeResults, ref SerializedObject so)
        {
            MB2_OutputOptions prefabOrSceneObject = mom.meshCombiner.outputOption;
            createdDummyTextureBakeResults = false;

            // Initial Validate
            {
                if (mom.meshCombiner.resultSceneObject != null &&
                    (MBVersionEditor.PrefabUtility_GetPrefabType(mom.meshCombiner.resultSceneObject) == MB_PrefabType.modelPrefabAsset ||
                     MBVersionEditor.PrefabUtility_GetPrefabType(mom.meshCombiner.resultSceneObject) == MB_PrefabType.prefabAsset))
                {
                    Debug.LogWarning("Result Game Object was a project asset not a scene object instance. Clearing this field.");
                    mom.meshCombiner.resultSceneObject = null;
                }

                if (prefabOrSceneObject != MB2_OutputOptions.bakeIntoPrefab && prefabOrSceneObject != MB2_OutputOptions.bakeIntoSceneObject)
                {
                    Debug.LogError("Paramater prefabOrSceneObject must be bakeIntoPrefab or bakeIntoSceneObject");
                    return false;
                }

                if (prefabOrSceneObject == MB2_OutputOptions.bakeIntoPrefab)
                {
                    if (MB3_MeshCombiner.EVAL_VERSION)
                    {
                        Debug.LogError("Cannot BakeIntoPrefab with evaluation version.");
                        return false;
                    }

                    if (mom.resultPrefab == null)
                    {
                        Debug.LogError("Need to set the Combined Mesh Prefab field. Create a prefab asset, drag an empty game object into it, and drag it to the 'Combined Mesh Prefab' field.");
                        return false;
                    }

                    string prefabPth = AssetDatabase.GetAssetPath(mom.resultPrefab);
                    if (prefabPth == null || prefabPth.Length == 0)
                    {
                        Debug.LogError("Could not save result to prefab. Result Prefab value is not a project asset. Is it an instance in the scene?");
                        return false;
                    }
                }
            }

            {
                // Find or create texture bake results
                MB3_TextureBaker tb = mom.GetComponentInParent<MB3_TextureBaker>();
                if (mom.textureBakeResults == null && tb != null)
                {
                    mom.textureBakeResults = tb.textureBakeResults;
                }

                if (mom.textureBakeResults == null)
                {
                    if (_OkToCreateDummyTextureBakeResult(mom))
                    {
                        createdDummyTextureBakeResults = true;
                        List<GameObject> gos = mom.GetObjectsToCombine();
                        if (mom.GetNumObjectsInCombined() > 0)
                        {
                            if (mom.meshCombiner.clearBuffersAfterBake) { mom.ClearMesh(); }
                            else
                            {
                                Debug.LogError("'Texture Bake Result' must be set to add more objects to a combined mesh that already contains objects. Try enabling 'clear buffers after bake'");
                                return false;
                            }
                        }
                        mom.textureBakeResults = MB2_TextureBakeResults.CreateForMaterialsOnRenderer(gos.ToArray(), mom.meshCombiner.GetMaterialsOnTargetRenderer());
                        if (mom.meshCombiner.LOG_LEVEL >= MB2_LogLevel.debug) { Debug.Log("'Texture Bake Result' was not set. Creating a temporary one. Each material will be mapped to a separate submesh."); }
                    }
                }
            }

            // Second level of validation now that TextureBakeResults exists.
            MB2_ValidationLevel vl = Application.isPlaying ? MB2_ValidationLevel.quick : MB2_ValidationLevel.robust;
            if (!MB3_MeshBakerRoot.DoCombinedValidate(mom, MB_ObjsToCombineTypes.sceneObjOnly, new MB3_EditorMethods(), vl))
            {
                return false;
            }

            // Add Delete Game Objects
            bool success;
            try
            {
                if (prefabOrSceneObject == MB2_OutputOptions.bakeIntoSceneObject)
                {
                    success = _BakeIntoCombinedSceneObject(mom, createdDummyTextureBakeResults, ref so);
                }
                else if (prefabOrSceneObject == MB2_OutputOptions.bakeIntoPrefab)
                {
                    success = _BakeIntoCombinedPrefab(mom, createdDummyTextureBakeResults, ref so);
                }
                else
                {
                    Debug.LogError("Should be impossible.");
                    success = false;
                }
            }
            catch
            {
                success = false;
                throw;
            }
            finally
            {
                mom.meshCombiner.Dispose();
            }

            if (createdDummyTextureBakeResults) MB_Utility.Destroy(mom.textureBakeResults);
            return success;
        }

        private static bool _BakeIntoCombinedSceneObject(MB3_MeshBakerCommon mom, bool createdDummyTextureBakeResults, ref SerializedObject so)
        {
            bool success;
            mom.ClearMesh();
            if (mom.AddDeleteGameObjects(mom.GetObjectsToCombine().ToArray(), null, false) &&
                mom.Apply(MB3_MeshBakerEditorFunctionsCore.UnwrapUV2))
            {
                success = true;
                if (createdDummyTextureBakeResults)
                {
                    Debug.Log(String.Format("Successfully baked {0} meshes each material is mapped to its own submesh.", mom.GetObjectsToCombine().Count));
                }
                else
                {
                    Debug.Log(String.Format("Successfully baked {0} meshes", mom.GetObjectsToCombine().Count));
                }
            }
            else
            {
                success = false;
            }

            return success;
        }

        private static bool _BakeIntoCombinedPrefab(MB3_MeshBakerCommon mom, bool createdDummyTextureBakeResults, ref SerializedObject so)
        {
            bool success = false;

            List<Transform> tempPrefabInstanceRoots = null;
            GameObject[] objsToCombine = mom.GetObjectsToCombine().ToArray();
            if (mom.meshCombiner.settings.renderType == MB_RenderType.skinnedMeshRenderer)
            {
                tempPrefabInstanceRoots = new List<Transform>();
                // We are going to move bones of source objs and transforms into our combined mesh prefab so make some duplicates
                // so that we don't destroy a setup.
                MB3_MeshBakerEditorFunctionsCore._DuplicateSrcObjectInstancesAndUnpack(mom.meshCombiner.settings.renderType, objsToCombine, tempPrefabInstanceRoots);
            }
            try
            {
                MB3_EditorMethods editorMethods = new MB3_EditorMethods();
                mom.ClearMesh(editorMethods);
                if (mom.AddDeleteGameObjects(objsToCombine, null, false))
                {
                    success = true;
                    mom.Apply(MB3_MeshBakerEditorFunctionsCore.UnwrapUV2);


                    if (createdDummyTextureBakeResults)
                    {
                        Debug.Log(String.Format("Successfully baked {0} meshes each material is mapped to its own submesh.", mom.GetObjectsToCombine().Count));
                    }
                    else
                    {
                        Debug.Log(String.Format("Successfully baked {0} meshes", mom.GetObjectsToCombine().Count));
                    }

                    string prefabPth = AssetDatabase.GetAssetPath(mom.resultPrefab);
                    if (prefabPth == null || prefabPth.Length == 0)
                    {
                        Debug.LogError("Could not save result to prefab. Result Prefab value is not an Asset.");
                        success = false;
                    }
                    else
                    {
                        string baseName = Path.GetFileNameWithoutExtension(prefabPth);
                        string folderPath = prefabPth.Substring(0, prefabPth.Length - baseName.Length - 7);
                        string newFilename = folderPath + baseName + "-mesh";
                        SaveMeshsToAssetDatabase(mom, folderPath, newFilename);
                        RebuildPrefab(mom, ref so, mom.resultPrefabLeaveInstanceInSceneAfterBake, tempPrefabInstanceRoots, objsToCombine);
                    }
                }
                else
                {
                    success = false;
                }

            }
            catch
            {
                throw;
            }
            finally
            {
                // Clean up temporary created instances. If success was true then they should have been added to a prefab
                // and cleaned up for us.
                if (success == false)
                {
                    if (tempPrefabInstanceRoots != null)
                    {
                        for (int i = 0; i < tempPrefabInstanceRoots.Count; i++)
                        {
                            MB_Utility.Destroy(tempPrefabInstanceRoots[i]);
                        }
                    }
                }
            }

            return success;
        }

        public static bool _OkToCreateDummyTextureBakeResult(MB3_MeshBakerCommon mom)
        {
            List<GameObject> objsToMesh = mom.GetObjectsToCombine();
            if (objsToMesh.Count == 0)
                return false;
            return true;
        }

        public static void SaveMeshsToAssetDatabase(MB3_MeshBakerCommon mom, string folderPath, string newFileNameBase)
        {
            if (MB3_MeshCombiner.EVAL_VERSION) return;
            if (mom is MB3_MeshBaker)
            {
                MB3_MeshBaker mb = (MB3_MeshBaker)mom;
                string newFilename = newFileNameBase + ".asset";
                string ap = AssetDatabase.GetAssetPath(((MB3_MeshCombinerSingle)mb.meshCombiner).GetMesh());
                if (ap == null || ap.Equals(""))
                {
                    Debug.Log("Saving mesh asset to " + newFilename);
                    AssetDatabase.CreateAsset(((MB3_MeshCombinerSingle)mb.meshCombiner).GetMesh(), newFilename);
                }
                else
                {
                    Debug.Log("Mesh is an existing asset at " + ap);
                }
            }
            else if (mom is MB3_MultiMeshBaker)
            {
                MB3_MultiMeshBaker mmb = (MB3_MultiMeshBaker)mom;
                List<MB3_MultiMeshCombiner.CombinedMesh> combiners = ((MB3_MultiMeshCombiner)mmb.meshCombiner).meshCombiners;
                for (int i = 0; i < combiners.Count; i++)
                {
                    string newFilename = newFileNameBase + i + ".asset";
                    Mesh mesh = combiners[i].combinedMesh.GetMesh();
                    string ap = AssetDatabase.GetAssetPath(mesh);
                    if (ap == null || ap.Equals(""))
                    {
                        Debug.Log("Saving mesh asset to " + newFilename);
                        AssetDatabase.CreateAsset(mesh, newFilename);
                    }
                    else
                    {
                        Debug.Log("Mesh is an asset at " + ap);
                    }
                }
            }
            else
            {
                Debug.LogError("Argument was not a MB3_MeshBaker or an MB3_MultiMeshBaker.");
            }
        }

        // The serialized object reference is necessary to work around a nasty unity bug.
        public static GameObject RebuildPrefab(MB3_MeshBakerCommon mom, ref SerializedObject so, bool leaveInstanceInSceneAfterBake, List<Transform> tempPrefabInstanceRoots, GameObject[] objsToCombine)
        {
            if (MB3_MeshCombiner.EVAL_VERSION) return null;

            if (mom.meshCombiner.LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Rebuilding Prefab: " + mom.resultPrefab);
            GameObject prefabRoot = mom.resultPrefab;
            GameObject instanceRootGO = mom.meshCombiner.resultSceneObject;
            /*
            GameObject instanceRootGO = (GameObject)PrefabUtility.InstantiatePrefab(prefabRoot);
            instanceRootGO.transform.position = Vector3.zero;
            instanceRootGO.transform.rotation = Quaternion.identity;
            instanceRootGO.transform.localScale = Vector3.one;

            //remove everything in the prefab.

            MBVersionEditor.UnpackPrefabInstance(instanceRootGO, ref so);
            int numChildren = instanceRootGO.transform.childCount;
            for (int i = numChildren - 1; i >= 0; i--)
            {
                MB_Utility.Destroy(instanceRootGO.transform.GetChild(i).gameObject);
            }

            if (mom is MB3_MeshBaker)
            {
                MB3_MeshBaker mb = (MB3_MeshBaker)mom;
                MB3_MeshCombinerSingle mbs = (MB3_MeshCombinerSingle)mb.meshCombiner;
                MB3_MeshCombinerSingle.BuildPrefabHierarchy(mbs, instanceRootGO, mbs.GetMesh());
            }
            else if (mom is MB3_MultiMeshBaker)
            {
                MB3_MultiMeshBaker mmb = (MB3_MultiMeshBaker)mom;
                MB3_MultiMeshCombiner mbs = (MB3_MultiMeshCombiner)mmb.meshCombiner;
                for (int i = 0; i < mbs.meshCombiners.Count; i++)
                {
                    MB3_MeshCombinerSingle.BuildPrefabHierarchy(mbs.meshCombiners[i].combinedMesh, instanceRootGO, mbs.meshCombiners[i].combinedMesh.GetMesh(), true);
                }
            }
            else
            {
                Debug.LogError("Argument was not a MB3_MeshBaker or an MB3_MultiMeshBaker.");
            }
            */

            if (mom.meshCombiner.settings.renderType == MB_RenderType.skinnedMeshRenderer)
            {
                MB3_MeshBakerEditorFunctionsCore._MoveBonesToCombinedMeshPrefabAndDeleteRenderers(instanceRootGO.transform, tempPrefabInstanceRoots, objsToCombine);
            }

            string prefabPth = AssetDatabase.GetAssetPath(prefabRoot);
            MBVersionEditor.PrefabUtility_ReplacePrefab(instanceRootGO, prefabPth, MB_ReplacePrefabOption.connectToPrefab);
            mom.resultPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPth);
            if (!leaveInstanceInSceneAfterBake)
            {
                Editor.DestroyImmediate(instanceRootGO);
            }

            return instanceRootGO;
        }
    }
}
