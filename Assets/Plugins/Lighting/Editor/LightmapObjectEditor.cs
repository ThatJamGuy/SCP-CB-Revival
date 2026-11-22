#if false
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LightmapObject))]
[CanEditMultipleObjects]
public class LightmapObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Space(10f);
        if (GUILayout.Button("Save Materials"))
        {
            var lmObj = target as LightmapObject;
            Save(lmObj);
        }
        if (GUILayout.Button("Load Materials"))
        {
            var lmObj = target as LightmapObject;
            Load(lmObj);
        }
    }

    public static void Save(LightmapObject lmObj)
    {
        if (lmObj.TryGetComponent(out MeshRenderer mr))
        {
            Undo.RecordObject(mr, "Save Materials");
            Undo.RecordObject(lmObj, "Save Materials");
            lmObj.rendererMaterials.Clear();
            foreach (var mat in mr.sharedMaterials)
            {
                var m = new AssetReferenceT<Material>(AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(mat)).ToString());
                lmObj.rendererMaterials.Add(m);
            }
            var mats = new Material[mr.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = Resources.Load<Material>("PlaceHolder");
            }
            mr.sharedMaterials = mats;
        }
    }

    public static void Load(LightmapObject lmObj)
    {
        if (lmObj.TryGetComponent(out MeshRenderer mr))
        {
            Undo.RecordObject(mr, "Load Materials");
            Undo.RecordObject(lmObj, "Load Materials");
            var mats = new Material[mr.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                if (i >= lmObj.rendererMaterials.Count)
                    break;
                mats[i] = lmObj.rendererMaterials[i].editorAsset;
            }
            mr.sharedMaterials = mats;
        }
    }

    private void OnSceneGUI()
    {
        // foreach (LightmapObject lmObj in targets)
        var lmObj = target as LightmapObject;
        {
            if (lmObj.TryGetComponent(out MeshFilter mf))
            {
                for (int i = 0; i < lmObj.rendererMaterials.Count; i++)
                {
                    // lmObj.rendererMaterials[i].editorAsset.SetPass(0);
                    // Graphics.DrawMeshNow(mf.sharedMesh, Matrix4x4.TRS(lmObj.transform.position, lmObj.transform.rotation, lmObj.transform.lossyScale), i);
                }
            }
        }
    }
}
#endif