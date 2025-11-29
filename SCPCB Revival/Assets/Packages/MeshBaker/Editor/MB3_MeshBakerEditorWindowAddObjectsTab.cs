//----------------------------------------------
//            MeshBaker
// Copyright © 2011-2012 Ian Deane
//----------------------------------------------
using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using DigitalOpus.MB.Core;
using UnityEngine.Rendering;


namespace DigitalOpus.MB.MBEditor
{
    public class MB3_MeshBakerEditorWindowAddObjectsTab : MB3_MeshBakerEditorWindowInterface
    {
        public enum OpacityType
        {
            unknown = -1,
            opaque = 0,
            transparent = 1,
            alpha_clip = 2,
            mixed = 3,
        }

        static string[] LODLevelLabels = new string[]
        {
        "All LOD Levels", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"
        };

        static int[] LODLevelValues = new int[]
        {
        -1,0,1,2,3,4,5,6,7,8,9
        };

        static string[] transpOrOpaqueLabels = new string[]
        {
            "Any", "Opaque Only", "Transparent Only", "AlphaClipped Only", "Mixed Transp And Opaque Mats Only"
        };

        static int[] transpOrOpaqueValues = new int[]
        {
            (int) OpacityType.unknown, (int) OpacityType.opaque, (int) OpacityType.transparent, (int) OpacityType.alpha_clip, (int) OpacityType.mixed
        };

        public MB3_MeshBakerRoot _target = null;
        public MonoBehaviour target
        {
            get { return _target; }
            set { _target = (MB3_MeshBakerRoot)value; }
        }

        GameObject targetGO = null;
        GameObject oldTargetGO = null;
        MB3_TextureBaker textureBaker;
        MB3_MeshBaker meshBaker;
        UnityEngine.Object[] targs = new UnityEngine.Object[1];
        SerializedObject serializedObject;

        GUIContent GUIContentRegExpression = new GUIContent("Matches Regular Expression", @"A valid # regular express. Examples:" + "\n\n" +
            @" ([A-Za-z0-9\-]+)(LOD1) matches one or more chars,numbers and hyphen ending with LOD1." + "\n\n" +
            @" (Grass)([A-Za-z0-9\-\(\) ]+) matches the string 'Grass' followed by characters, numbers, hyphen, brackets or space." + "\n\n");

        string helpBoxString = "";
        string regExParseError = "";
        bool onlyStaticObjects = false;
        bool onlyEnabledObjects = false;
        bool excludeMeshesWithOBuvs = true;
        bool excludeMeshesAlreadyAddedToBakers = true;
        int lodLevelToInclude = -1;
        OpacityType transpOrOpaque = OpacityType.unknown;
        int lightmapIndex = -2;
        int[] lightmapValues = new int[64];
        string[] lightmapDisplayValues = new string[64];
        LayerMask layerMask = ~0;
        int[] layerIntValues = new int[32];
        string[] layerDisplayValues = new string[32];
        string searchRegEx = "";
        Material shaderMat = null;
        Material mat = null;

        bool tbFoldout = false;
        bool mbFoldout = false;

        MB3_MeshBakerEditorInternal mbe = new MB3_MeshBakerEditorInternal();
        MB3_TextureBakerEditorInternal tbe = new MB3_TextureBakerEditorInternal();

        public void SetLightmapValuesAndDisplayValues()
        {
            lightmapValues = new int[64];
            lightmapDisplayValues = new string[64];
            lightmapValues[0] = -2;
            lightmapValues[1] = -1;
            lightmapDisplayValues[0] = "don't filter on lightmapping";
            lightmapDisplayValues[1] = "not lightmapped";
            for (int i = 2; i < lightmapDisplayValues.Length; i++)
            {
                lightmapDisplayValues[i] = "" + (i - 2);
                lightmapValues[i] = i - 2;
            }
        }

        public void SetLayerValuesAndDisplayValues()
        {
            layerIntValues = new int[32];
            layerDisplayValues = new string[32];
            for (int i = 0; i < layerDisplayValues.Length; i++)
            {
                if (GetLayerName(i) != "")
                {
                    layerDisplayValues[i] = "" + (i) + ": " + GetLayerName(i);
                }
                else
                {
                    layerDisplayValues[i] = "" + (i);
                }
                layerIntValues[i] = i;
            }
        }


        public void OnEnable()
        {
            SetLightmapValuesAndDisplayValues();
            SetLayerValuesAndDisplayValues();

            if (textureBaker != null)
            {
                serializedObject = new SerializedObject(textureBaker);
                tbe.OnEnable(serializedObject);
            }
            else if (meshBaker != null)
            {
                serializedObject = new SerializedObject(meshBaker);
                mbe.OnEnable(serializedObject);
            }
        }

        public void OnDisable()
        {
            tbe.OnDisable();
            mbe.OnDisable();
        }

        public void drawTabAddObjectsToBakers()
        {
            if (helpBoxString == null) helpBoxString = "";
            EditorGUILayout.HelpBox("To add, select one or more objects in the hierarchy view. Child Game Objects with MeshRender or SkinnedMeshRenderer will be added. Use the fields below to filter what is added." +
                                    "To remove, use the fields below to filter what is removed.\n" + helpBoxString, UnityEditor.MessageType.None);
            target = (MB3_MeshBakerRoot)EditorGUILayout.ObjectField("Target to add objects to", target, typeof(MB3_MeshBakerRoot), true);

            if (target != null)
            {
                targetGO = target.gameObject;
            }
            else
            {
                targetGO = null;
            }

            if (targetGO != oldTargetGO && targetGO != null)
            {
                textureBaker = targetGO.GetComponent<MB3_TextureBaker>();
                meshBaker = targetGO.GetComponent<MB3_MeshBaker>();
                tbe = new MB3_TextureBakerEditorInternal();
                mbe = new MB3_MeshBakerEditorInternal();
                oldTargetGO = targetGO;
                if (textureBaker != null)
                {
                    serializedObject = new SerializedObject(textureBaker);
                    tbe.OnEnable(serializedObject);
                }
                else if (meshBaker != null)
                {
                    serializedObject = new SerializedObject(meshBaker);
                    mbe.OnEnable(serializedObject);
                }
            }


            EditorGUIUtility.labelWidth = 300;
            onlyStaticObjects = EditorGUILayout.Toggle("Only Static Objects", onlyStaticObjects);

            onlyEnabledObjects = EditorGUILayout.Toggle("Only Enabled Objects", onlyEnabledObjects);

            excludeMeshesWithOBuvs = EditorGUILayout.Toggle("Exclude meshes with out-of-bounds UVs", excludeMeshesWithOBuvs);

            excludeMeshesAlreadyAddedToBakers = EditorGUILayout.Toggle("Exclude GameObjects already added to bakers", excludeMeshesAlreadyAddedToBakers);

            lodLevelToInclude = EditorGUILayout.IntPopup("Only include objects on LOD Level", lodLevelToInclude, LODLevelLabels, LODLevelValues);

            mat = (Material)EditorGUILayout.ObjectField("Using Material", mat, typeof(Material), true);
            shaderMat = (Material)EditorGUILayout.ObjectField("Using Shader", shaderMat, typeof(Material), true);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Using Lightmap Index ");
            lightmapIndex = EditorGUILayout.IntPopup(lightmapIndex,
                                                     lightmapDisplayValues,
                                                     lightmapValues);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Using Layers");
            layerMask.value = EditorGUILayout.MaskField(layerMask.value, layerDisplayValues);
            EditorGUILayout.EndHorizontal();

            transpOrOpaque = (OpacityType) EditorGUILayout.IntPopup("Only Include Transparent Or Opaque", (int) transpOrOpaque, transpOrOpaqueLabels, transpOrOpaqueValues);


            if (regExParseError != null && regExParseError.Length > 0)
            {
                EditorGUILayout.HelpBox("Error In Regular Expression:\n" + regExParseError, MessageType.Error);
            }
            searchRegEx = EditorGUILayout.TextField(GUIContentRegExpression, searchRegEx);


            EditorGUILayout.Separator();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Selected Meshes To Target"))
            {
                addSelectedObjects();
            }
            if (GUILayout.Button("Remove Matching Meshes From Target"))
            {
                removeSelectedObjects();
            }
            EditorGUILayout.EndHorizontal();

            if (textureBaker != null)
            {
                MB_EditorUtil.DrawSeparator();
                tbFoldout = EditorGUILayout.Foldout(tbFoldout, "Texture Baker");
                if (tbFoldout)
                {
                    if (targs == null) targs = new UnityEngine.Object[1];
                    targs[0] = textureBaker;
                    tbe.DrawGUI(serializedObject, textureBaker, targs, typeof(MB3_MeshBakerEditorWindow));
                }

            }
            if (meshBaker != null)
            {
                MB_EditorUtil.DrawSeparator();
                mbFoldout = EditorGUILayout.Foldout(mbFoldout, "Mesh Baker");
                if (mbFoldout)
                {
                    if (targs == null) targs = new UnityEngine.Object[1];
                    targs[0] = meshBaker;
                    mbe.DrawGUI(serializedObject, (MB3_MeshBaker)meshBaker, targs, typeof(MB3_MeshBakerEditorWindow));
                }
            }
        }

        List<GameObject> GetFilteredList(bool addingObjects)
        {
            List<GameObject> newMomObjs = new List<GameObject>();
            MB3_MeshBakerRoot mom = (MB3_MeshBakerRoot)target;
            if (mom == null)
            {
                Debug.LogError("Must select a target MeshBaker");
                return newMomObjs;
            }

            GameObject dontAddMe = null;
            Renderer r = MB_Utility.GetRenderer(mom.gameObject);
            if (r != null)
            { //make sure that this MeshBaker object is not in list
                dontAddMe = r.gameObject;
            }

            MB3_MeshBakerRoot[] allBakers = (MB3_MeshBakerRoot[])MBVersion.FindSceneObjectsOfType(typeof(MB3_MeshBakerRoot));
            HashSet<GameObject> objectsAlreadyIncludedInBakers = new HashSet<GameObject>();

            if (addingObjects)
            {
                for (int i = 0; i < allBakers.Length; i++)
                {
                    List<GameObject> objsToCombine = allBakers[i].GetObjectsToCombine();
                    for (int j = 0; j < objsToCombine.Count; j++)
                    {
                        if (objsToCombine[j] != null) objectsAlreadyIncludedInBakers.Add(objsToCombine[j]);
                    }
                }
            }

            GameObject[] gos = Selection.gameObjects;
            if (gos.Length == 0)
            {
                Debug.LogWarning("No objects selected in hierarchy view. Nothing added. Try selecting some objects.");
                return newMomObjs;
            }

            List<GameObject> mrs = new List<GameObject>();
            for (int i = 0; i < gos.Length; i++)
            {
                GameObject go = gos[i];
                Renderer[] rs = go.GetComponentsInChildren<Renderer>(true);
                for (int j = 0; j < rs.Length; j++)
                {
                    if (rs[j] is MeshRenderer || rs[j] is SkinnedMeshRenderer)
                    {
                        mrs.Add(rs[j].gameObject);
                    }
                }
            }

            newMomObjs = FilterList(mrs, objectsAlreadyIncludedInBakers, dontAddMe, addingObjects);
            return newMomObjs;
        }

        int GetLODLevelForRenderer(Renderer r)
        {
            if (r != null)
            {
                LODGroup lodGroup = r.GetComponentInParent<LODGroup>();
                if (lodGroup != null)
                {
                    LOD[] lods = lodGroup.GetLODs();
                    for (int lodIdx = 0; lodIdx < lods.Length; lodIdx++)
                    {
                        Renderer[] rs = lods[lodIdx].renderers;
                        for (int j = 0; j < rs.Length; j++)
                        {
                            if (rs[j] == r)
                            {
                                return lodIdx;
                            }
                        }
                    }
                }
            }
            return 0;
        }

        int GetLayerValue(GameObject go)
        {
            return go.layer;
        }

        string GetLayerName(int layerNum)
        {
            string layerName = LayerMask.LayerToName(layerNum);
            return "" + layerName;
        }

        List<GameObject> FilterList(List<GameObject> mrss,
                    HashSet<GameObject> objectsAlreadyIncludedInBakers,
                    GameObject dontAddMe,
                    bool addingObjects)
        {
            int numInSelection = 0;
            int numStaticExcluded = 0;
            int numEnabledExcluded = 0;
            int numLightmapExcluded = 0;
            int numLayerExcluded = 0;
            int numLodLevelExcluded = 0;
            int numOBuvExcluded = 0;
            int numMatExcluded = 0;
            int numShaderExcluded = 0;
            int numRegExExcluded = 0;
            int numAlreadyIncludedExcluded = 0;
            int numTranspOrOpaqueExluded = 0;
            System.Text.RegularExpressions.Regex regex = null;
            if (searchRegEx != null && searchRegEx.Length > 0)
            {

                try
                {
                    regex = new System.Text.RegularExpressions.Regex(searchRegEx);
                    regExParseError = "";
                }
                catch (Exception ex)
                {
                    regExParseError = ex.Message;
                }
            }

            Dictionary<int, MB_Utility.MeshAnalysisResult> meshAnalysisResultsCache = new Dictionary<int, MB_Utility.MeshAnalysisResult>(); //cache results
            List<GameObject> newMomObjs = new List<GameObject>();
            for (int j = 0; j < mrss.Count; j++)
            {
                if (mrss[j] == null)
                {
                    continue;
                }
                Renderer mrs = mrss[j].GetComponent<Renderer>();
                int layerValue = GetLayerValue(mrss[j]);

                if (mrs is MeshRenderer || mrs is SkinnedMeshRenderer)
                {
                    if (mrs.GetComponent<TextMesh>() != null)
                    {
                        continue; //don't add TextMeshes
                    }

                    numInSelection++;
                    if (!newMomObjs.Contains(mrs.gameObject))
                    {
                        bool addMe = true;
                        if (!mrs.gameObject.isStatic && onlyStaticObjects)
                        {
                            numStaticExcluded++;
                            addMe = false;
                            continue;
                        }

                        if ((!mrs.enabled || !mrs.gameObject.activeInHierarchy) && onlyEnabledObjects)
                        {
                            numEnabledExcluded++;
                            addMe = false;
                            continue;
                        }

                        if (lightmapIndex != -2)
                        {
                            if (mrs.lightmapIndex != lightmapIndex)
                            {
                                numLightmapExcluded++;
                                addMe = false;
                                continue;
                            }
                        }

                        if (layerMask != 0)
                        {
                            int layerBit = 1 << layerValue;
                            //Debug.Log("LayerBit: " + layerBit);
                            if ((layerMask & layerBit) == 0)
                            {
                                numLayerExcluded++;
                                addMe = false;
                                continue;
                            }
                        }
                        else
                        {
                            numLayerExcluded++;
                            addMe = false;
                            continue;
                        }

                        if (lodLevelToInclude == -1)
                        {
                            // not filtering on LODLevel
                        }
                        else
                        {
                            if (GetLODLevelForRenderer(mrs) != lodLevelToInclude)
                            {
                                numLodLevelExcluded++;
                                addMe = false;
                                continue;
                            }
                        }

                        if (transpOrOpaque == OpacityType.unknown)
                        {
                            // not filtering on transp or opaque
                        }
                        else
                        {
                            if (!RendererTransparentOrOpaqueMatches(mrs, transpOrOpaque))
                            {
                                numTranspOrOpaqueExluded++;
                                addMe = false;
                                continue;
                            }
                        }

                        // only do this check when adding objects. If removing objects shouldn't do it
                        if (addingObjects &&
                            excludeMeshesAlreadyAddedToBakers &&
                            objectsAlreadyIncludedInBakers.Contains(mrs.gameObject))
                        {
                            numAlreadyIncludedExcluded++;
                            addMe = false;
                            continue;
                        }

                        Mesh mm = MB_Utility.GetMesh(mrs.gameObject);
                        if (mm != null)
                        {
                            MB_Utility.MeshAnalysisResult mar;
                            if (!meshAnalysisResultsCache.TryGetValue(mm.GetInstanceID(), out mar))
                            {
                                MB_Utility.hasOutOfBoundsUVs(mm, ref mar);
                                meshAnalysisResultsCache.Add(mm.GetInstanceID(), mar);
                            }
                            if (mar.hasOutOfBoundsUVs && excludeMeshesWithOBuvs)
                            {
                                numOBuvExcluded++;
                                addMe = false;
                                continue;
                            }
                        }

                        if (shaderMat != null)
                        {
                            Material[] nMats = mrs.sharedMaterials;
                            bool usesShader = false;
                            foreach (Material nMat in nMats)
                            {
                                if (nMat != null && nMat.shader == shaderMat.shader)
                                {
                                    usesShader = true;
                                }
                            }
                            if (!usesShader)
                            {
                                numShaderExcluded++;
                                addMe = false;
                                continue;
                            }
                        }

                        if (mat != null)
                        {
                            Material[] nMats = mrs.sharedMaterials;
                            bool usesMat = false;
                            foreach (Material nMat in nMats)
                            {
                                if (nMat == mat)
                                {
                                    usesMat = true;
                                }
                            }
                            if (!usesMat)
                            {
                                numMatExcluded++;
                                addMe = false;
                                continue;
                            }
                        }

                        if (regex != null)
                        {
                            if (!regex.IsMatch(mrs.gameObject.name))
                            {
                                numRegExExcluded++;
                                addMe = false;
                                continue;
                            }
                        }

                        if (addMe && mrs.gameObject != dontAddMe)
                        {
                            if (!newMomObjs.Contains(mrs.gameObject))
                            {
                                newMomObjs.Add(mrs.gameObject);
                            }
                        }
                    }
                }
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            //sb.AppendFormat("Total objects in selection {0}\n", numInSelection);
            //Debug.Log( "Total objects in selection " + numInSelection);
            if (numStaticExcluded > 0)
            {
                sb.AppendFormat("   {0} objects were excluded because they were not static\n", numStaticExcluded);
                Debug.Log(numStaticExcluded + " objects were excluded because they were not static\n");
            }
            if (numEnabledExcluded > 0)
            {
                sb.AppendFormat("   {0} objects were excluded because they were disabled\n", numEnabledExcluded);
                Debug.Log(numEnabledExcluded + " objects were excluded because they were disabled\n");
            }
            if (numOBuvExcluded > 0)
            {
                sb.AppendFormat("   {0} objects were excluded because they had out of bounds uvs\n", numOBuvExcluded);
                Debug.Log(numOBuvExcluded + " objects were excluded because they had out of bounds uvs\n");
            }
            if (numLightmapExcluded > 0)
            {
                sb.AppendFormat("   {0} objects were excluded because they did not match lightmap filter.\n", numLightmapExcluded);
                Debug.Log(numLightmapExcluded + " objects did not match lightmap filter.\n");
            }
            if (numLayerExcluded > 0)
            {
                sb.AppendFormat("   {0} objects were excluded because they did not use the selected layers. \n",
                numLayerExcluded);
                Debug.Log(numLayerExcluded + " objects were excluded because they did not use the selected layers");
            }
            if (numLodLevelExcluded > 0)
            {
                sb.AppendFormat("   {0} objects were excluded because they did not match the selected LOD level filter.\n", numLodLevelExcluded);
                Debug.Log(numLodLevelExcluded + " objects did not match LOD level filter.\n");
            }
            if (numShaderExcluded > 0)
            {
                sb.AppendFormat("   {0} objects were excluded because they did not use the selected shader.\n", numShaderExcluded);
                Debug.Log(numShaderExcluded + " objects were excluded because they did not use the selected shader.\n");
            }
            if (numMatExcluded > 0)
            {
                sb.AppendFormat("   {0} objects were excluded because they did not use the selected material.\n", numMatExcluded);
                Debug.Log(numMatExcluded + " objects were excluded because they did not use the selected material.\n");
            }

            if (numTranspOrOpaqueExluded > 0)
            {
                sb.AppendFormat("   {0} objects were excluded because they the didn't match transparent or opaque filter.\n", numTranspOrOpaqueExluded);
                Debug.LogFormat("   {0} objects were excluded because they the didn't match transparent or opaque filter.\n", numTranspOrOpaqueExluded);
            }

            if (numRegExExcluded > 0)
            {
                sb.AppendFormat("   {0} objects were excluded because they did not match the regular expression.\n", numRegExExcluded);
                Debug.Log(numRegExExcluded + " objects were excluded because they did not match the regular expression.\n");
            }
            if (numAlreadyIncludedExcluded > 0)
            {
                sb.AppendFormat("   {0} objects were excluded because they did were already included in other bakers.\n", numAlreadyIncludedExcluded);
                Debug.Log(numAlreadyIncludedExcluded + " objects were excluded because they did were already included in other bakers.\n");
            }

            helpBoxString = sb.ToString();
            return newMomObjs;
        }



        private bool RendererTransparentOrOpaqueMatches(Renderer mrs, OpacityType transpOrOpaque)
        {
            MBVersion.PipelineType currentRenderPipeline = MBVersion.DetectPipeline();
            OpacityType isMatTranspOrOpaque = OpacityType.unknown;

            if (mrs == null)
            {
                return false;
            }
            for (int i = 0; i < mrs.sharedMaterials.Length; i++)
            {
                Material m = mrs.sharedMaterials[i];
                if (m == null)
                {
                    continue;
                }

                OpacityType isCurMatTranspOrOpaque;
                if (currentRenderPipeline == MBVersion.PipelineType.URP)
                {
                    isCurMatTranspOrOpaque = _GetOpaqueOrTransparentURP(m);
                }
                else if (currentRenderPipeline == MBVersion.PipelineType.HDRP)
                {
                    isCurMatTranspOrOpaque = _GetOpaqueOrTransparentHDRP(m);
                }
                else
                {
                    // Defualt or unknown pipeline
                    isCurMatTranspOrOpaque = _GetOpaqueOrTransparentBuiltInOrUnknownRP(m);
                }

                if (isMatTranspOrOpaque == OpacityType.unknown)
                {
                    isMatTranspOrOpaque = isCurMatTranspOrOpaque;
                }
                else if (isMatTranspOrOpaque != isCurMatTranspOrOpaque)
                {
                    // was previously set to something different. Renderer has mixed materials.
                    isMatTranspOrOpaque = OpacityType.mixed;
                    break;
                }
            }

            if (isMatTranspOrOpaque == OpacityType.unknown)
            {
                return false;
            }
            else
            {
                return isMatTranspOrOpaque == transpOrOpaque;
            }
        }

        private OpacityType _GetOpaqueOrTransparentBuiltInOrUnknownRP(Material m)
        {
            bool isTransparent = m.renderQueue >= (int)RenderQueue.Transparent;
            bool isAlphaClipped = m.renderQueue == (int)RenderQueue.AlphaTest;
        
            if (isAlphaClipped)
            {
                return OpacityType.alpha_clip;
            }
            else if (isTransparent)
            {
                return OpacityType.transparent;
            }
            else
            {
                return OpacityType.opaque;
            }
        }

        private OpacityType _GetOpaqueOrTransparentHDRP(Material m)
        {
            if (m.HasProperty("_SurfaceType"))
            {
                float surfaceType = m.GetFloat("_SurfaceType");
                if (surfaceType == 0)
                {
                    if (m.HasProperty("_AlphaCutoffEnable") && m.GetFloat("_AlphaCutoffEnable") == 1)
                    {
                        return OpacityType.alpha_clip;
                    }
                    else
                    {
                        return OpacityType.opaque;
                    }
                }
                else if (surfaceType == 1)
                {

                    return OpacityType.transparent;
                }
            }

            return _GetOpaqueOrTransparentBuiltInOrUnknownRP(m);
        }
        
        private OpacityType _GetOpaqueOrTransparentURP(Material m)
        {
            if (m.HasProperty("_Surface"))
            {
                float surfaceType = m.GetFloat("_Surface");
                if (surfaceType == 0)
                {
                    if (m.HasProperty("_AlphaClip") && m.GetFloat("_AlphaClip") == 1)
                    {
                        return OpacityType.alpha_clip;
                    }
                    else
                    {
                        return OpacityType.opaque;
                    }
                }
                else if (surfaceType == 1)
                {
                    return OpacityType.transparent;
                }
            }

            return _GetOpaqueOrTransparentBuiltInOrUnknownRP(m);
        }

        void removeSelectedObjects()
        {
            MB3_MeshBakerRoot mom = (MB3_MeshBakerRoot)target;
            if (mom == null)
            {
                Debug.LogError("Must select a target MeshBaker");
                return;
            }
            List<GameObject> objsToCombine = mom.GetObjectsToCombine();
            GameObject dontAddMe = null;
            Renderer r = MB_Utility.GetRenderer(mom.gameObject);
            if (r != null)
            { //make sure that this MeshBaker object is not in list
                dontAddMe = r.gameObject;
            }

            List<GameObject> objsSelectedMatchingFilter = GetFilteredList(addingObjects: false);
            Debug.Log("Matching filter " + objsSelectedMatchingFilter.Count);
            List<GameObject> objsToRemove = new List<GameObject>();
            for (int i = 0; i < objsSelectedMatchingFilter.Count; i++)
            {
                if (objsToCombine.Contains(objsSelectedMatchingFilter[i]))
                {
                    objsToRemove.Add(objsSelectedMatchingFilter[i]);
                }
            }

            MBVersionEditor.RegisterUndo(mom, "Remove Objects");
            for (int i = 0; i < objsToRemove.Count; i++)
            {
                objsToCombine.Remove(objsToRemove[i]);
            }

            SerializedObject so = new SerializedObject(mom);
            so.SetIsDifferentCacheDirty();
            Debug.Log("Removed " + objsToRemove.Count + " objects from " + mom.name);
            helpBoxString += String.Format("\nRemoved {0} objects from {1}", objsToRemove.Count, mom.name);
        }

        void addSelectedObjects()
        {
            MB3_MeshBakerRoot mom = (MB3_MeshBakerRoot)target;
            if (mom == null)
            {
                Debug.LogError("Must select a target MeshBaker or TextureBaker to add objects to");
                return;
            }

            List<GameObject> newMomObjs = GetFilteredList(addingObjects: true);
            MBVersionEditor.RegisterUndo(mom, "Add Objects");
            List<GameObject> momObjs = mom.GetObjectsToCombine();
            int numAdded = 0;
            int numAlreadyInList = 0;
            for (int i = 0; i < newMomObjs.Count; i++)
            {
                if (!momObjs.Contains(newMomObjs[i]))
                {
                    momObjs.Add(newMomObjs[i]);
                    numAdded++;
                }
                else
                {
                    numAlreadyInList++;
                }
            }

            SerializedObject so = new SerializedObject(mom);
            so.SetIsDifferentCacheDirty();
            if (numAlreadyInList > 0)
            {
                Debug.Log(String.Format("Skipped adding {0} objects to Target because these objects had already been added to this Target. ", numAlreadyInList));
            }

            if (numAdded == 0)
            {
                Debug.LogWarning("Added 0 objects. Make sure some or all objects are selected in the hierarchy view. Also check the 'Only Static Objects', 'Using Material' and 'Using Shader' settings");
            }
            else
            {
                Debug.Log(string.Format("Added {0} objects to {1}. ", numAdded, mom.name));
            }

            helpBoxString += String.Format("\nAdded {0} objects to {1}", numAdded, mom.name);
        }
    }
}