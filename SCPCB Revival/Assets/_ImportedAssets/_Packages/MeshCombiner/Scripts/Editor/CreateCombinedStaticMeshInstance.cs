using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LylekGames.Tools
{
	public class CreateCombinedStaticMeshInstance
    {
        [MenuItem("Tools/Combine/Static Meshes")]
        private static void CombineStaticMeshesNewCollider()
        {
            //MY OBJECT
            Object selectedObject = Selection.activeObject;
            GameObject myObject = (GameObject)selectedObject;

            CombineMeshes myCombine = myObject.GetComponent<CombineMeshes>();

            MeshFilter meshFilter = myObject.GetComponent<MeshFilter>();

            if (meshFilter != null && myCombine == null)
            {
                Debug.Log("The active object already contains mesh data. Creating empty gameObject...");

                GameObject parent = new GameObject(myObject.name);
                parent.transform.position = myObject.transform.position;
                parent.transform.rotation = myObject.transform.rotation;

                myObject.transform.SetParent(parent.transform);

                myCombine = parent.AddComponent<CombineMeshes>();

                Selection.activeObject = parent;

                Debug.Log("The object you had selected is now a child of the currently active gameObject.");
            }
            else
            {
                myCombine = myObject.GetComponent<CombineMeshes>();

                if(!myCombine)
                {
                    myCombine = myObject.AddComponent<CombineMeshes>();
                }
            }
           
            myCombine.Start();

            myCombine.CombineMultiMaterialMesh();

            EditorUtility.SetDirty(myCombine.gameObject);
        }
    }
}