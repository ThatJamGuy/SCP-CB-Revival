//----------------------------------------------
//            MeshBaker
// Copyright © 2015-2016 Ian Deane
//----------------------------------------------
using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using DigitalOpus.MB.Core;
using UnityEditor;


namespace DigitalOpus.MB.MBEditor
{

    [CustomEditor(typeof(MB3_BoneWeightCopier))]
    public class MB3_BoneWeightCopierEditor : Editor
    {
        SerializedProperty inputGameObjectProp;
        SerializedProperty outputPrefabProp;
        SerializedProperty radiusProp;
        SerializedProperty seamMeshProp;
        SerializedProperty targetMeshesProp;
        SerializedProperty outputFolderProp;

        GameObject copyOfInput;

        GUIContent gc_inputGameObject = new GUIContent("Input Game Object", "Assign the parent object of all the Skinned Meshes to this field.");
        GUIContent gc_outputPrefab = new GUIContent("Output Prefab", "Provide a GameObject Prefab that the Bone Weight Copier will output to.");
        GUIContent gc_radius = new GUIContent("Radius", "Vertices in each Target Mesh that are within Radius of a vertex in the Seam Mesh will be assigned the same bone weight as the Seam Mesh vertex.");
        GUIContent gc_seamMesh = new GUIContent("Seam Mesh", "The seam mesh should contain vertices for the seams with correct bone weights. Seams are the vertices that are shared by two skinned meshes that overlap. For example there should be a seam between an arm skinned mesh and a hand skinned mesh.");
        GUIContent gc_outputFolder = new GUIContent("Output Folder", "Copies of the Skinned Meshes will be saved to this folder, must be located in the project Assets folder.");

        [MenuItem("GameObject/Create Other/Mesh Baker/Bone Weight Copier", false, 1000)]
        public static GameObject CreateNewMeshBaker()
        {
            GameObject nmb = new GameObject("BoneWeightCopier");
            nmb.transform.position = Vector3.zero;
            nmb.AddComponent<MB3_BoneWeightCopier>();
            return nmb.gameObject;
        }


        void OnEnable()
        {
            radiusProp = serializedObject.FindProperty("radius");
            seamMeshProp = serializedObject.FindProperty("seamMesh");
            //targetMeshesProp = serializedObject.FindProperty("targetMeshes");
            outputFolderProp = serializedObject.FindProperty("outputFolder");
            inputGameObjectProp = serializedObject.FindProperty("inputGameObject");
            outputPrefabProp = serializedObject.FindProperty("outputPrefab");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            MB3_BoneWeightCopier bwc = (MB3_BoneWeightCopier)target;


            EditorGUILayout.HelpBox("== BETA (please report problems) ==\n\n" +
                                    "This tool helps create skinned mesh parts that can be mix and matched to customize characters. " +
                                    "It adjusts the boneWeights, positions, normals and tangents at the joins to have the same values so there are no tears." +
                                    "\n\n" +
                                    "1) Model the skinned meshes and attach them all to the same rig. The rig may have multiple arms, hands, hair etc...\n" +
                                    "2) At every seam (eg. between an arm mesh and hand mesh) there must be a set of vertices duplicated in both meshes.\n" +
                                    "3) Create one additional mesh that contains another copy of all the geometry. This will be called the Seam Mesh\n" +
                                    "4) Attach it to the rig and adjust the bone weights. This will be the master set of bone weights that will be copied to all the other skinned meshes.\n" +
                                    "5) Mark the seam vertices using the UV channel.\n" +
                                    "    verts with UV > (.5,.5) are seam verts\n" +
                                    "    verts with UV < (.5,.5) are ignored\n" +
                                    "6) Import the model into Unity and create an instance of it in the scene\n" +
                                    "7) Assign the Seam Mesh to the Seam Mesh field\n" +
                                    "8) Assign the parent game object of all the skinned meshes to the Input Game Object field\n" +
                                    "9) Adjust the radius\n" +
                                    "10) Choose an output folder and save the meshes\n" +
                                    "11) Click 'Copy Bone Weights From Seam Mesh'\n", MessageType.Info);

            EditorGUILayout.PropertyField(radiusProp, gc_radius);
            EditorGUILayout.PropertyField(seamMeshProp, gc_seamMesh);
            EditorGUILayout.PropertyField(inputGameObjectProp, gc_inputGameObject);
            EditorGUILayout.PropertyField(outputPrefabProp, gc_outputPrefab);
            //if (GUILayout.Button("Get Skinned Meshes From Input Game Object")) {
            //	GetSkinnedMeshesFromGameObject(bwc);
            //}
            //EditorGUILayout.PropertyField(targetMeshesProp,true);

            if (GUILayout.Button("Copy Bone Weights From Seam Mesh"))
            {
                CopyBoneWeightsFromSeamMeshToOtherMeshes(bwc);
            }
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Save Meshes To Project Folder", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(outputFolderProp, gc_outputFolder);
            bool pathIsValid;
            string errorMessage;
            {
                string path = outputFolderProp.stringValue;
                outputFolderProp.stringValue =
                    MB3_MeshBakerEditorFunctionsCore.SanitizeAndMakeFullPathRelativeToAssetsFolderAndValidate(path, out errorMessage,
                        out pathIsValid);
                Debug.Assert(errorMessage != null, "errorMessage should never be null");
            }
            
            if (GUILayout.Button("Browse"))
            {
                string path = EditorUtility.OpenFolderPanel("Browse For Output Folder", "", "");
                path = MB_BatchPrefabBakerEditorFunctionsCore.ConvertFullPathToProjectRelativePath(path);
                outputFolderProp.stringValue = path;
            }
            EditorGUILayout.EndHorizontal();

            if (!pathIsValid)
            {
                EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
            }
            
            if (GUILayout.Button("Set Output Folder To Output Prefab Folder"))
            {
                string path = AssetDatabase.GetAssetPath(bwc.outputPrefab);
                path = new System.IO.FileInfo(path).Directory.FullName.Replace('\\', '/');
                path = MB_BatchPrefabBakerEditorFunctionsCore.ConvertFullPathToProjectRelativePath(path);
                outputFolderProp.stringValue = path;
            }
            serializedObject.ApplyModifiedProperties();
        }

        public void CopyBoneWeightsFromSeamMeshToOtherMeshes(MB3_BoneWeightCopier bwc)
        {
            if (bwc.seamMesh == null)
            {
                Debug.LogError("The Seam Mesh cannot be null.");
                return;
            }

            if (bwc.inputGameObject == null)
            {
                Debug.LogError("The Input Game Object cannot be null.");
                return;
            }

            UnityEngine.Object pr = (UnityEngine.Object) MBVersionEditor.PrefabUtility_FindPrefabRoot(bwc.seamMesh.gameObject);
            string assetPath = null;
            if (pr != null)
            {
                assetPath = AssetDatabase.GetAssetPath(pr);
            }
            if (assetPath != null)
            {
                ModelImporter mi = (ModelImporter)AssetImporter.GetAtPath(assetPath);
                if (mi != null)
                {
                    if (mi.optimizeGameObjects)
                    {
                        Debug.LogError(string.Format("The seam mesh has 'optimize game objects' checked in the asset importer rig tab. This will result in no bones. Uncheck 'optimize game objects' in the rig tab."));
                        return;
                    }
                }
            }

            {
                // Check that inputGameObject contains all the bones;
                Transform[] bones = bwc.seamMesh.bones;
                for (int i = 0; i < bones.Length; i++)
                {
                    Transform b = bones[i];
                    if (b == null)
                    {
                        Debug.LogError("There are null bones on the seam mesh.");
                        return;
                    } else
                    {
                        if (!IsChildOf(bwc.inputGameObject.transform, b))
                        {
                            Debug.LogError("All bones used by the seam mesh must be children of the Input Game Object. There were some bones that were not a children of the seam mesh.");
                            return;
                        }
                    }
                }
            }

            if (bwc.outputPrefab == null)
            {
                Debug.LogError(string.Format("The output game object must be assigned and must be a prefab of a game object in the project folder."));
                return;
            }
            if (MBVersionEditor.PrefabUtility_GetPrefabType(bwc.outputPrefab) != MB_PrefabType.prefabAsset)
            {
                Debug.LogError("The output game object must be a prefab. Create a prefab in the project and drag an empty game object to it.");
                return;
            }

            //duplicate the source prefab and the meshes
            if (copyOfInput != null)
            {
                DestroyImmediate(copyOfInput);
            }
            copyOfInput = (GameObject)GameObject.Instantiate(bwc.inputGameObject);

            Transform seamMeshCopyTransform = MB_BatchPrefabBakerEditorFunctionsCore.FindCorrespondingTransform(bwc.inputGameObject.transform, bwc.seamMesh.transform, copyOfInput.transform);
            SkinnedMeshRenderer seamMeshCopy = seamMeshCopyTransform.GetComponent<SkinnedMeshRenderer>();

            Transform[] seamMeshBones = seamMeshCopy.bones;
            SkinnedMeshRenderer[] targSkinnedMeshes = copyOfInput.GetComponentsInChildren<SkinnedMeshRenderer>();
            Transform[][] targMeshBones = new Transform[targSkinnedMeshes.Length][];
            Mesh[] targs = new Mesh[targSkinnedMeshes.Length];
            for (int i = 0; i < targSkinnedMeshes.Length; i++)
            {
                if (targSkinnedMeshes[i].sharedMesh == null)
                {
                    Debug.LogError(string.Format("Skinned Mesh {0} does not have a mesh", targSkinnedMeshes[i]));
                    return;
                }
                MB_PrefabType pt = MBVersionEditor.PrefabUtility_GetPrefabType(targSkinnedMeshes[i].gameObject);
                if (pt == MB_PrefabType.modelPrefabAsset)
                {
                    Debug.LogError(string.Format("Target Mesh {0} is an imported model prefab. Can't modify these meshes because changes will be overwritten the next time the model is saved or reimported. Try instantiating the prefab and using skinned meshes from the scene instance.", i));
                    return;
                }

                targMeshBones[i] = targSkinnedMeshes[i].bones;
                targs[i] = (Mesh)GameObject.Instantiate(targSkinnedMeshes[i].sharedMesh);
            }

            Transform[][] newBonesForSMRs = new Transform[targs.Length][];
            MB3_CopyBoneWeights.CopyBoneWeightsFromSeamMeshToOtherMeshes(bwc.radius, seamMeshCopy.sharedMesh, targs, newBonesForSMRs, seamMeshBones, targMeshBones);
            SaveMeshesToOutputFolderAndAssignToSMRs(targs, newBonesForSMRs, targSkinnedMeshes);

            EditorUtility.SetDirty(copyOfInput);


            // TODO tried using 2018 replace prefab but there were errors.
            //MBVersionEditor.ReplacePrefab(copyOfInput, assetPath, MB_ReplacePrefabOption.connectToPrefab | MB_ReplacePrefabOption.nameBased);
            string prefabPth = AssetDatabase.GetAssetPath(bwc.outputPrefab);
            MBVersionEditor.PrefabUtility_ReplacePrefab(copyOfInput, prefabPth, MB_ReplacePrefabOption.connectToPrefab | MB_ReplacePrefabOption.nameBased);

            AssetDatabase.SaveAssets();
            DestroyImmediate(copyOfInput);
        }

        public void SaveMeshesToOutputFolderAndAssignToSMRs(Mesh[] targetMeshes, Transform[][] newBonesForSMRs, SkinnedMeshRenderer[] targetSMRs)
        {
            Debug.Assert(targetMeshes.Length == newBonesForSMRs.Length);
            //validate meshes
            for (int i = 0; i < targetMeshes.Length; i++)
            {
                if (targetMeshes[i] == null)
                {
                    Debug.LogError(string.Format("An error occured processing mesh {0}. Mesh could not be saved.", targetSMRs[i]));
                    continue;
                }
                if (targetSMRs[i] == null)
                {
                    Debug.LogError(string.Format("Target SkinnedMesh {0} is null", i));
                    return;
                }

                if (targetSMRs[i].sharedMesh == null)
                {
                    Debug.LogError(string.Format("Target Mesh {0} does not have a mesh", i));
                    return;
                }
                MB_PrefabType pt = MBVersionEditor.PrefabUtility_GetPrefabType(targetMeshes[i]);
                if (pt == MB_PrefabType.modelPrefabAsset)
                {
                    Debug.LogError(string.Format("Target Mesh {0} is an imported model prefab. Can't modify these meshes because changes will be overwritten the next time the model is saved or reimported. Try instantiating the prefab and using skinned meshes from the scene instance.", i));
                    return;
                }

            }
            //validate output folder
            if (outputFolderProp.stringValue == null)
            {
                Debug.LogError("Output folder must be set");
                return;
            }
            else
            {
                {
                    string errorMessage;
                    bool pathIsValid;
                    outputFolderProp.stringValue =
                        MB3_MeshBakerEditorFunctionsCore.SanitizeAndMakeFullPathRelativeToAssetsFolderAndValidate(
                            outputFolderProp.stringValue, out errorMessage, out pathIsValid);
                    if (!pathIsValid)
                    {
                        Debug.LogError(errorMessage);
                        return;
                    }
                }
            }
            for (int i = 0; i < targetMeshes.Length; i++)
            {
                Mesh m = targetMeshes[i];
                if (m == null)
                {
                    continue;
                }

                string pth = outputFolderProp.stringValue + "/" + targetMeshes[i].name + ".Asset";
                if (pth == null)
                {
                    Debug.LogError("The output folder must be a folder in the project Assets folder.");
                    return;
                }
                Debug.Log("Creating mesh " + m + " path " + pth + " numBindPoses: " + m.bindposes.Length + " targSMRs numBones: " + targetSMRs[i].bones.Length);
                targetSMRs[i].sharedMesh = m;
                if (newBonesForSMRs[i] != null) targetSMRs[i].bones = newBonesForSMRs[i];
                AssetDatabase.CreateAsset(m, pth);
                targetSMRs[i].sharedMesh = m;
                Debug.Log(string.Format("Created mesh at {0}. Updated Skinned Mesh {1} to use created mesh.", pth, targetSMRs[i].name));
            }
            AssetDatabase.SaveAssets();
        }

        bool IsChildOf(Transform parent, Transform child)
        {
            if (parent == null) return false;
            if (child == null) return false;
            Transform t = child.parent;
            do
            {
                if (t == parent || t.parent == parent)
                {
                    return true;
                }
                t = t.parent;
            } while (t != child.root);
            return false;
        }
    }
}
