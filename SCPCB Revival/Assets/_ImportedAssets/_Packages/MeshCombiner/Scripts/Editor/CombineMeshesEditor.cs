using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.IO;

namespace LylekGames.Tools
{
    [CustomEditor(typeof(CombineMeshes))]
	public class CombineMeshesEditor : Editor
    {
		CombineMeshes myCombine;

        public void OnEnable()
        {
            myCombine = (CombineMeshes)target;

            myCombine.Start();
        }
        public override void OnInspectorGUI()
        {
			myCombine = (CombineMeshes)target;

            if (myCombine.myMeshFilter.sharedMesh == null)
            {
                DrawDefaultInspector();

                if (GUILayout.Button("NEW Combine Meshes"))
                {
                    myCombine.CombineMultiMaterialMesh();

                    EditorUtility.SetDirty(myCombine.gameObject);
                }
            }
            else
            {
                if (!myCombine.saved)
                {
                    DrawDefaultInspector();

                    if (GUILayout.Button("Save Mesh Data & Clean Up"))
                    {
                        SaveMeshData(true);
                    }

                    if (GUILayout.Button("Save Mesh Data"))
                    {
                        SaveMeshData(false);
                    }

                    EditorGUILayout.Space();

                    if (myCombine.myMeshCollider == null)
                    {
                        if (GUILayout.Button("Generate Collision Data"))
                        {
                            myCombine.CreateMeshCollider();

                            EditorUtility.SetDirty(myCombine.gameObject);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Remove Collision Data"))
                        {
                            DestroyImmediate(myCombine.myMeshCollider);

                            EditorUtility.SetDirty(myCombine.gameObject);
                        }
                    }

                    if (GUILayout.Button("Disable Meshes"))
                    {
                        myCombine.DisableMesh();

                        EditorUtility.SetDirty(myCombine.gameObject);
                    }
                }
                else
                {
                    if (GUILayout.Button("Clean Up"))
                    {
                        CleanUp();
                    }
                    if (GUILayout.Button("Disable Meshes"))
                    {
                        myCombine.DisableMesh();

                        myCombine.saved = false;

                        EditorUtility.SetDirty(myCombine.gameObject);
                    }
                }
            }
        }
        public void SaveMeshData(bool destructive = false)
        {
            #region Correct File Path
            if(myCombine.path.Length < 3) { myCombine.path = "/MeshCombiner/"; }

            if (myCombine.path[0] != '/') { myCombine.path = "/" + myCombine.path; }

            if (myCombine.path[myCombine.path.Length - 1] != '/') { myCombine.path = myCombine.path + "/"; }
            #endregion

            if (!Directory.Exists(Application.dataPath + myCombine.path + "Prefabs/"))
            {
                Directory.CreateDirectory(Application.dataPath + myCombine.path + "Prefabs/");
            }
            if (!Directory.Exists(Application.dataPath + myCombine.path + "MeshData/"))
            {
                Directory.CreateDirectory(Application.dataPath + myCombine.path + "MeshData/");
            }

            AssetDatabase.SaveAssets();

            AssetDatabase.Refresh();

            ///SAVE MESH AND CREATE PREFAB
            SaveMesh(myCombine.myMeshFilter, destructive);
        }
        public void SaveMesh(MeshFilter meshFilter, bool destructive = false)
        {
            string meshPath = "Assets/" + myCombine.path + "MeshData/" + meshFilter.gameObject.name + ".asset";

            string colliderPath = "Assets/" + myCombine.path + "MeshData/" + meshFilter.gameObject.name + "-Collider.asset";

            int randomID = 0;

            int i = 0;

            #region Add File Path Extension
            ///If data path already exists, add an id extension
            while (AssetDatabase.LoadAssetAtPath(meshPath, typeof(Mesh)))
            {
                randomID = Random.Range(0, 999999999);

                meshPath = meshPath.Replace(".asset", randomID.ToString() + ".asset");

                colliderPath = colliderPath.Replace(".asset", randomID.ToString() + ".asset");

                i++; if(i > 8) { Debug.Log("Process Aborted! \n\n See Readme for details."); return; }
            }
            #endregion

            ///SAVE MESH DATA
            AssetDatabase.CreateAsset(meshFilter.sharedMesh, meshPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            ///LOAD REFERENCE TO OUR NEWLY SAVED MESH DATA
            Mesh meshData = (Mesh)AssetDatabase.LoadAssetAtPath(meshPath, typeof(Mesh));
            ///TEMP COLLIDER DATA
            Mesh colliderData = meshData;

            ///Assign mesh data
            meshFilter.sharedMesh = meshData;

            MeshCollider meshCollider = meshFilter.gameObject.GetComponent<MeshCollider>();
            if (meshCollider)
            {
                if (meshCollider.sharedMesh != meshFilter.sharedMesh)
                {
                    ///SAVE COLLIDER DATA
                    AssetDatabase.CreateAsset(meshCollider.sharedMesh, colliderPath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    ///LOAD REFERENCE TO OUR NEWLY SAVED COLLIDER DATA
                    colliderData = (Mesh)AssetDatabase.LoadAssetAtPath(colliderPath, typeof(Mesh));
                }
                ///Assign collider data
                meshCollider.sharedMesh = colliderData;
            }

            if (destructive)
            {
                CleanUp();
            }
            else
            {
                myCombine.saved = true;

                EditorUtility.SetDirty(myCombine.gameObject);
            }

            CombineMeshes.staticPath = myCombine.path;

            Debug.Log("Data saved successfully.");
        }
        public void CleanUp()
        {            
            ///REMOVE OUR CHILD OBJECTS
            int childCount = myCombine.myMeshFilter.gameObject.transform.childCount;

            for (int i = 0; i < childCount; i++)
            {
                DestroyImmediate(myCombine.myMeshFilter.transform.GetChild(0).gameObject);
            }

            EditorUtility.SetDirty(myCombine.gameObject);

            ///REMOVE THE COMBINE SCRIPT FROM OUR PREFAB, WE WON'T BE NEEDING IT ANYMORE
            DestroyImmediate(myCombine);
        }
    }
}
