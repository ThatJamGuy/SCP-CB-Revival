using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LightmapPrefab))]
[CanEditMultipleObjects]
public class LightmapPrefabEditor : Editor {
    private static int scene = 0;
    private static int progressId;
    private static int count;
    private static LightmapPrefab prefab;
    private static List<KeyValuePair<LightmapPrefab, int>> queue = new List<KeyValuePair<LightmapPrefab, int>>();

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        GUILayout.Space(10f);
        prefab = target as LightmapPrefab;
        scene = EditorGUILayout.IntField("Scene Id", scene);

        if (GUILayout.Button("Calc. Bounds")) {
            foreach (LightmapPrefab item2 in targets) {
                Undo.RecordObject(item2, "Lightmap");
                item2.bounds = new Bounds(item2.transform.localPosition, Vector3.zero);
                var objs = item2.GetComponentsInChildren<LightmapObject>(true);
                foreach (var item3 in objs) {
                    item2.bounds.Encapsulate(item2.transform.InverseTransformPoint(item3.transform.position));
                    if (item3.TryGetComponent(out Renderer render)) {
                        item2.bounds.Encapsulate(render.localBounds);
                    }
                }
            }
        }

        if ((PrefabUtility.IsPartOfPrefabInstance(prefab.gameObject) || PrefabUtility.IsPartOfPrefabAsset(prefab.gameObject)) && GUILayout.Button("Bake")) {
            count = 1;
            progressId = Progress.Start("Bake Rooms");
            BakeScene();
        }
        if ((PrefabUtility.IsPartOfPrefabInstance(prefab.gameObject) || PrefabUtility.IsPartOfPrefabAsset(prefab.gameObject)) && GUILayout.Button("Save Bake")) {
            try {
                AssetDatabase.StartAssetEditing();
                var objs = prefab.GetComponentsInChildren<LightmapObject>(true);
                foreach (var item in objs) {
                    PrefabUtility.ApplyObjectOverride(item, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefab), InteractionMode.AutomatedAction);
                }
            } finally {
                AssetDatabase.StopAssetEditing();
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }
    }

    [MenuItem("Tools/Bake All")]
    public static void BakeAll() {
        var arr = Selection.gameObjects;
        foreach (var item in arr) {
            foreach (var item2 in arr)
                item2.SetActive(false);
            item.SetActive(true);
            var prefab2 = item.GetComponent<LightmapPrefab>();
            for (int i = 0; i < prefab2.scenes.Count; i++) {
                queue.Add(new(prefab2, i));
                prefab = prefab2;
                scene = i;
                // BakeScene(scene);
            }
        }
        count = queue.Count;
        progressId = Progress.Start("Bake Rooms");
        BakeScene();
    }

    public static void BakeScene() {
        if (scene >= prefab.scenes.Count || scene < 0) {
            Progress.Remove(progressId);
            EditorUtility.DisplayDialog("Error", "Scene index is invalid. Make sure to add at least one scene!", "OK");
            return;
        }
        prefab.gameObject.SetActive(true);
        foreach (var lmObj in prefab.GetComponentsInChildren<LightmapObject>(true)) {
            // LightmapObjectEditor.Load(lmObj);
        }
        // prefab = target as LightmapPrefab;
        foreach (var item in prefab.scenes) {
            foreach (var obj in item.objects) {
                obj.SetActive(false);
            }
        }
        foreach (var obj in prefab.scenes[scene].objects) {
            obj.SetActive(true);
        }
        Progress.Report(progressId, count - queue.Count, count);
        Progress.RegisterCancelCallback(progressId, () => {
            Lightmapping.Cancel();
            return true;
        });
        Lightmapping.bakeCompleted += OnBakeDone;
        Lightmapping.BakeAsync();
    }

    private static void OnBakeDone() {
        Lightmapping.bakeCompleted -= OnBakeDone;
        // var prefab = target as LightmapPrefab;
        // string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefab.gameObject);
        string path = $"Assets/GFX/Lightmaps/{prefab.name}";
        // string path = AssetDatabase.GetAssetPath(prefab.gameObject);
        var objs = prefab.GetComponentsInChildren<LightmapObject>(true);
        try {
            AssetDatabase.StartAssetEditing();
            foreach (var obj in objs) {
                var renderer = obj.GetComponent<Renderer>();
                var light = obj.GetComponent<Light>();
                if (renderer != null && renderer.lightmapIndex != -1) {
                    var data = LightmapSettings.lightmaps[renderer.lightmapIndex];
                    string colorPath = $"{path}_{renderer.lightmapIndex}_lightmapColor_{scene}.exr";
                    string dirPath = string.Empty;
                    if (data.lightmapDir != null)
                        dirPath = $"{path}_{renderer.lightmapIndex}_lightmapDir_{scene}.exr";
                    string maskPath = string.Empty;
                    if (data.shadowMask != null)
                        maskPath = $"{path}_{renderer.lightmapIndex}_shadowMask_{scene}.exr";
                    if (File.Exists(colorPath))
                        AssetDatabase.DeleteAsset(colorPath);
                    if (File.Exists(dirPath))
                        AssetDatabase.DeleteAsset(dirPath);
                    if (File.Exists(maskPath))
                        AssetDatabase.DeleteAsset(maskPath);
                    AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(data.lightmapColor), colorPath);
                    if (data.lightmapDir != null) {
                        AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(data.lightmapDir), dirPath);
                    }
                    if (data.shadowMask != null) {
                        AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(data.shadowMask), maskPath);
                    }
                    // AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);
                }
                if (light != null) {
                    Undo.RecordObject(obj, "Lightmap");
                    var sceneData = new LightmapLightScene() {
                        probeOcclusionLightIndex = light.bakingOutput.probeOcclusionLightIndex,
                        occlusionMaskChannel = light.bakingOutput.occlusionMaskChannel,
                        lightmapBakeType = light.bakingOutput.lightmapBakeType,
                        mixedLightingMode = light.bakingOutput.mixedLightingMode,
                        isBaked = light.bakingOutput.isBaked,
                    };
                    obj.mode = light.lightmapBakeType;
                    if (scene >= obj.lightScenes.Count) {
                        obj.lightScenes.Add(sceneData);
                    } else {
                        obj.lightScenes[scene] = sceneData;
                    }
                    Undo.FlushUndoRecordObjects();
                    EditorUtility.SetDirty(obj);
                }
            }
        } finally {
            AssetDatabase.StopAssetEditing();
        }
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        foreach (var obj in objs) {
            Undo.RecordObject(obj, "Lightmap");
            var renderer = obj.GetComponent<Renderer>();
            var light = obj.GetComponent<Light>();
            if (renderer != null && renderer.lightmapIndex != -1) {
                var data = LightmapSettings.lightmaps[renderer.lightmapIndex];
                string colorPath = $"{path}_{renderer.lightmapIndex}_lightmapColor_{scene}.exr";
                string dirPath = string.Empty;
                if (data.lightmapDir != null)
                    dirPath = $"{path}_{renderer.lightmapIndex}_lightmapDir_{scene}.exr";
                string maskPath = string.Empty;
                if (data.shadowMask != null)
                    maskPath = $"{path}_{renderer.lightmapIndex}_shadowMask_{scene}.exr";
                ((TextureImporter)AssetImporter.GetAtPath(colorPath)).textureType = TextureImporterType.Default;
                AssetImporter.GetAtPath(colorPath).SaveAndReimport();
                var sceneData = new LightmapScene() {
                    /*
                    color = (Texture2D)AssetDatabase.LoadAssetAtPath<Texture2D>(colorPath),
                    dir = string.IsNullOrEmpty(dirPath) ? null : (Texture2D)AssetDatabase.LoadAssetAtPath<Texture2D>(dirPath),
                    mask = string.IsNullOrEmpty(maskPath) ? null : (Texture2D)AssetDatabase.LoadAssetAtPath<Texture2D>(maskPath),
                    offset = renderer.lightmapScaleOffset,
                    */
                    color = (Texture2D)AssetDatabase.LoadMainAssetAtPath(colorPath),
                    dir = string.IsNullOrEmpty(dirPath) ? null : (Texture2D)AssetDatabase.LoadMainAssetAtPath(dirPath),
                    mask = string.IsNullOrEmpty(maskPath) ? null : (Texture2D)AssetDatabase.LoadMainAssetAtPath(maskPath),
                    offset = renderer.lightmapScaleOffset,
                    bakedWorldTransform = renderer.transform.worldToLocalMatrix,
                    // color = data.lightmapColor,
                    // dir = data.lightmapDir,
                    // mask = data.shadowMask,
                    // offset = renderer.lightmapScaleOffset,
                };
                if (scene >= obj.scenes.Count) {
                    obj.scenes.Add(sceneData);
                } else {
                    obj.scenes[scene] = sceneData;
                }
            }
            Undo.FlushUndoRecordObjects();
            EditorUtility.SetDirty(obj);
            AssetDatabase.SaveAssets();
        }
        if (queue.Count > 0) {
            Progress.Report(progressId, count - queue.Count, count);
            prefab.gameObject.SetActive(false);
            prefab = queue[0].Key;
            prefab.gameObject.SetActive(true);
            scene = queue[0].Value;
            queue.RemoveAt(0);
            BakeScene();
        } else {
            Progress.Remove(progressId);
        }
    }
}
