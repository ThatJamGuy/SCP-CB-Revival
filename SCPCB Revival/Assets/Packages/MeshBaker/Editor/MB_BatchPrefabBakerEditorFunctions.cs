using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DigitalOpus.MB.Core;
using System.Text.RegularExpressions;


namespace DigitalOpus.MB.MBEditor
{
    public static class MB_BatchPrefabBakerEditorFunctions
    {

        public static int EvalVersionPrefabLimit
        {
            get { return 5; }
        }

        public static void CreateEmptyOutputPrefabs(string outputFolder, MB3_BatchPrefabBaker target)
        {
            string errorMessage;
            bool isValidPath;
            MB3_MeshBakerEditorFunctionsCore.SanitizeAndMakeFullPathRelativeToAssetsFolderAndValidate(outputFolder,
                out errorMessage, out isValidPath);
            if (!isValidPath)
            {
                Debug.LogError(errorMessage);
                return;
            }

            HashSet<GameObject> srcPrefabs = new HashSet<GameObject>();
            if (outputFolder.StartsWith("Assets")) outputFolder = MB_BatchPrefabBakerEditorFunctionsCore.ConvertProjectRelativePathToFullPath(outputFolder);
            int numCreated = 0;
            int numSkippedSrcNull = 0;
            int numSkippedSrcDuplicate = 0;
            int numSkippedAlreadyExisted = 0;
            MB3_BatchPrefabBaker prefabBaker = (MB3_BatchPrefabBaker)target;
            for (int i = 0; i < prefabBaker.prefabRows.Length; i++)
            {
                if (prefabBaker.prefabRows[i].sourcePrefab != null)
                {
                    if (srcPrefabs.Contains(prefabBaker.prefabRows[i].sourcePrefab))
                    {
                        Debug.LogError($"Skipping row {i} because the source prefab was a duplicate of a previous row.");
                        numSkippedSrcDuplicate++;
                    }
                    else
                    {
                        srcPrefabs.Add(prefabBaker.prefabRows[i].sourcePrefab);
                        if (prefabBaker.prefabRows[i].resultPrefab == null)
                        {
                            string outName = outputFolder + "/" + prefabBaker.prefabRows[i].sourcePrefab.name + ".prefab";
                            outName = outName.Replace(Application.dataPath, "");
                            outName = "Assets" + outName;
                            outName = AssetDatabase.GenerateUniqueAssetPath(outName);
                            GameObject go = new GameObject(prefabBaker.prefabRows[i].sourcePrefab.name);
                            prefabBaker.prefabRows[i].resultPrefab = MBVersionEditor.PrefabUtility_CreatePrefab(outName, go);
                            GameObject.DestroyImmediate(go);
                            numCreated++;
                        }
                        else
                        {
                            numSkippedAlreadyExisted++;
                        }
                    }
                }
                else
                {
                    numSkippedSrcNull++;
                }
            }

            string msg = String.Format("Created {0} prefabs. Skipped {1} because source prefab was null. Skipped {2} because source prefab was a duplicate. Skipped {3} because the result prefab was already assigned", numCreated, numSkippedSrcNull, numSkippedSrcDuplicate, numSkippedAlreadyExisted);
            if (numSkippedSrcNull > 0 || numSkippedAlreadyExisted > 0)
            {
                Debug.LogError(msg);
            }
            else
            {
                Debug.Log(msg);
            }
        }

        public static void PopulatePrefabRowsFromTextureBaker(MB3_BatchPrefabBaker prefabBaker)
        {
            MB3_TextureBaker texBaker = prefabBaker.GetComponent<MB3_TextureBaker>();
            List<GameObject> newPrefabs = new List<GameObject>();
            List<GameObject> gos = texBaker.GetObjectsToCombine();
            for (int i = 0; i < gos.Count; i++)
            {
                GameObject go = (GameObject)MBVersionEditor.PrefabUtility_FindPrefabRoot(gos[i]);
                UnityEngine.Object obj = MBVersionEditor.PrefabUtility_GetCorrespondingObjectFromSource(go);

                if (obj != null && obj is GameObject)
                {
                    if (!newPrefabs.Contains((GameObject)obj)) newPrefabs.Add((GameObject)obj);
                }
                else
                {
                    Debug.LogWarning(String.Format("Object {0} did not have a prefab", gos[i]));
                }

            }

            // Remove prefabs that are already in the list of batch prefab baker's prefabs.
            {
                List<GameObject> tmpNewPrefabs = new List<GameObject>();
                for (int i = 0; i < newPrefabs.Count; i++)
                {
                    bool found = false;
                    for (int j = 0; j < prefabBaker.prefabRows.Length; j++)
                    {
                        if (prefabBaker.prefabRows[j].sourcePrefab == newPrefabs[i])
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        tmpNewPrefabs.Add(newPrefabs[i]);
                    }
                }

                newPrefabs = tmpNewPrefabs;
            }

            List<MB3_BatchPrefabBaker.MB3_PrefabBakerRow> newRows = new List<MB3_BatchPrefabBaker.MB3_PrefabBakerRow>();
            if (prefabBaker.prefabRows == null) prefabBaker.prefabRows = new MB3_BatchPrefabBaker.MB3_PrefabBakerRow[0];
            newRows.AddRange(prefabBaker.prefabRows);
            for (int i = 0; i < newPrefabs.Count; i++)
            {
                MB3_BatchPrefabBaker.MB3_PrefabBakerRow row = new MB3_BatchPrefabBaker.MB3_PrefabBakerRow();
                row.sourcePrefab = newPrefabs[i];
                newRows.Add(row);
            }


            Undo.RecordObject(prefabBaker, "Populate prefab rows");
            prefabBaker.prefabRows = newRows.ToArray();
        }


        public static void BakePrefabs(MB3_BatchPrefabBaker pb, bool doReplaceTargetPrefab)
        {
            if (pb.LOG_LEVEL >= MB2_LogLevel.info) Debug.Log("Batch baking prefabs");

            if (MB3_MeshCombiner.EVAL_VERSION)
            {
                int numPrefabsLimit = EvalVersionPrefabLimit;
                if (pb.prefabRows.Length > numPrefabsLimit)
                {
                    Debug.LogError("The free version of mesh baker is limited to batch baking " + numPrefabsLimit +
                        " prefabs. The full version has no limit on the number of prefabs that can be baked. Delete the extra prefab rows before baking.");
                    return;
                }
            }

            if (Application.isPlaying)
            {
                Debug.LogError("The BatchPrefabBaker cannot be run in play mode.");
                return;
            }

            MB3_MeshBaker mb = pb.GetComponent<MB3_MeshBaker>();
            mb.UpgradeToCurrentVersionIfNecessary();
            if (mb == null)
            {
                Debug.LogError("Prefab baker needs to be attached to a Game Object with a MB3_MeshBaker component.");
                return;
            }

            if (mb.textureBakeResults == null)
            {
                Debug.LogError("Texture Bake Results is not set");
                return;
            }

            int numResultMats = mb.textureBakeResults.NumResultMaterials();
            for (int i = 0; i < numResultMats; i++)
            {
                if (mb.textureBakeResults.GetCombinedMaterialForSubmesh(i) == null)
                {
                    Debug.LogError("The texture bake result had a null result material on submesh " + i + ". Try re-baking textures");
                    return;
                }
            }

            if (mb.meshCombiner.outputOption != MB2_OutputOptions.bakeMeshAssetsInPlace)
            {
                mb.meshCombiner.outputOption = MB2_OutputOptions.bakeMeshAssetsInPlace;
            }

            MB2_TextureBakeResults tbr = mb.textureBakeResults;

            HashSet<Mesh> sourceMeshes = new HashSet<Mesh>();
            HashSet<Mesh> allResultMeshes = new HashSet<Mesh>();

            //validate prefabs
            for (int i = 0; i < pb.prefabRows.Length; i++)
            {
                if (pb.prefabRows[i] == null || pb.prefabRows[i].sourcePrefab == null)
                {
                    Debug.LogError("Source Prefab on row " + i + " is not set.");
                    return;
                }
                if (pb.prefabRows[i].resultPrefab == null)
                {
                    Debug.LogError("Result Prefab on row " + i + " is not set.");
                    return;
                }
                for (int j = i + 1; j < pb.prefabRows.Length; j++)
                {
                    if (pb.prefabRows[i].sourcePrefab == pb.prefabRows[j].sourcePrefab)
                    {
                        Debug.LogError("Rows " + i + " and " + j + " contain the same source prefab");
                        return;
                    }
                }
                for (int j = 0; j < pb.prefabRows.Length; j++)
                {
                    if (pb.prefabRows[i].sourcePrefab == pb.prefabRows[j].resultPrefab)
                    {
                        Debug.LogError("Row " + i + " source prefab is the same as row " + j + " result prefab");
                        return;
                    }
                }
                if (MBVersionEditor.PrefabUtility_GetPrefabType(pb.prefabRows[i].sourcePrefab) != MB_PrefabType.modelPrefabAsset &&
                    MBVersionEditor.PrefabUtility_GetPrefabType(pb.prefabRows[i].sourcePrefab) != MB_PrefabType.prefabAsset)
                {
                    Debug.LogError("Row " + i + " source prefab is not a prefab asset ");
                    return;
                }
                if (MBVersionEditor.PrefabUtility_GetPrefabType(pb.prefabRows[i].resultPrefab) != MB_PrefabType.modelPrefabAsset &&
                    MBVersionEditor.PrefabUtility_GetPrefabType(pb.prefabRows[i].resultPrefab) != MB_PrefabType.prefabAsset)
                {
                    Debug.LogError("Row " + i + " result prefab is not a prefab asset");
                    return;
                }

                GameObject so = (GameObject)GameObject.Instantiate(pb.prefabRows[i].sourcePrefab);
                GameObject ro = (GameObject)GameObject.Instantiate(pb.prefabRows[i].resultPrefab);
                Renderer[] rs = (Renderer[])so.GetComponentsInChildren<Renderer>(true);

                for (int j = 0; j < rs.Length; j++)
                {
                    if (MB_BatchPrefabBakerEditorFunctionsCore.IsGoodToBake(rs[j], tbr))
                    {
                        sourceMeshes.Add(MB_Utility.GetMesh(rs[j].gameObject));
                    }
                }
                rs = ro.GetComponentsInChildren<Renderer>(true);

                for (int j = 0; j < rs.Length; j++)
                {
                    Renderer r = rs[j];
                    if (r is MeshRenderer || r is SkinnedMeshRenderer)
                    {
                        Mesh m = MB_Utility.GetMesh(r.gameObject);
                        if (m != null)
                        {
                            allResultMeshes.Add(m);
                        }
                    }
                }

                GameObject.DestroyImmediate(so); //todo should cache these and have a proper cleanup at end
                GameObject.DestroyImmediate(ro);
            }

            sourceMeshes.IntersectWith(allResultMeshes);
            HashSet<Mesh> sourceMeshesThatAreUsedByResult = sourceMeshes;
            if (sourceMeshesThatAreUsedByResult.Count > 0)
            {
                foreach (Mesh m in sourceMeshesThatAreUsedByResult)
                {
                    Debug.LogWarning("Mesh " + m + " is used by both the source and result prefabs. New meshes will be created.");
                }
                //return;
            }

            List<MB_BatchPrefabBakerEditorFunctionsCore.UnityTransform> unityTransforms = new List<MB_BatchPrefabBakerEditorFunctionsCore.UnityTransform>();
            // Bake the meshes using the meshBaker component one prefab at a time
            // Version check
            for (int prefabIdx = 0; prefabIdx < pb.prefabRows.Length; prefabIdx++)
            {
                if (doReplaceTargetPrefab)
                {
                    MB_BatchPrefabBakerEditorFunctionsCore._ProcessPrefabRowReplaceTargetPrefab(pb.LOG_LEVEL, pb.prefabRows[prefabIdx].sourcePrefab, pb.prefabRows[prefabIdx].resultPrefab, tbr, unityTransforms, (MB3_MeshCombinerSingle) mb.meshCombiner);
                }
                else
                {
                    MB_BatchPrefabBakerEditorFunctionsCore._ProcessPrefabRowOnlyMeshesAndMaterials(pb.LOG_LEVEL, pb.prefabRows[prefabIdx].sourcePrefab, pb.prefabRows[prefabIdx].resultPrefab, tbr, unityTransforms, (MB3_MeshCombinerSingle) mb.meshCombiner);
                }
            }

            AssetDatabase.Refresh();
            mb.ClearMesh();
        }
    }
}
