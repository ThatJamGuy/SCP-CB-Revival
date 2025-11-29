//            MeshBaker
// Copyright © 2011-2012 Ian Deane
//---------------------------------------------- 
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using DigitalOpus.MB.Core;

using UnityEditor;

namespace DigitalOpus.MB.MBEditor
{

    public class MB_TextureBakerEditorConfigureMultiMaterials
    {
        private static GUIContent gc_moveOneMappingUp = new GUIContent("\u2191", "move this material up to previous mapping."),
                                  gc_moveOneMappingDown = new GUIContent("\u2193", "move this material down to next mapping."),
                                  gc_newCombinedMaterial = new GUIContent("new", "Create a new combined material for this mapping.");

        private static GUIContent[] gc_SourceMatOptions = new GUIContent[]
              {
                  new GUIContent("...", " Source material actions. "),
                  new GUIContent("move up to previous submesh", "move this material up to previous submesh."),
                  new GUIContent("move down to next submesh", "move this material down to next submesh."),
              };

        private static GUIContent[] gc_MappingOptions = new GUIContent[]
              {
                  new GUIContent("...", " Result material actions. "),
                  new GUIContent("move submesh up", "Move this submesh up."),
                  new GUIContent("move submesh down", "Move this submesh down."),
                  new GUIContent("delete submesh", "Delete this submesh."),
                  new GUIContent("insert new submesh", "Insert a new submesh after this submesh."),
                  new GUIContent("create combined material", "Create a combined material for this submesh."),
              };

        private static GUIContent sourceMaterialsLabel = new GUIContent(
            "Source Materials",
            "This list of source materials will be combined into a combined-material for this submesh."
        );
        
        public static void DrawMapping(MB3_TextureBaker momm, MB3_TextureBakerEditorInternal tbEditor, int rowNum)
        {
            EditorGUILayout.Separator();
            if (rowNum % 2 == 1)
            {
                EditorGUILayout.BeginVertical(tbEditor.editorStyles.multipleMaterialBackgroundStyle);
            }
            else
            {
                EditorGUILayout.BeginVertical(tbEditor.editorStyles.multipleMaterialBackgroundStyleDarker);
            }

            string s = "";
            if (rowNum < momm.resultMaterials.Length && momm.resultMaterials[rowNum] != null && momm.resultMaterials[rowNum].combinedMaterial != null) s = momm.resultMaterials[rowNum].combinedMaterial.shader.ToString();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("---------- submesh:" + rowNum + " " + s, EditorStyles.boldLabel);

            int myAction = 0;
            myAction = EditorGUILayout.Popup(myAction, gc_MappingOptions, GUILayout.MaxWidth(60f));
            if (myAction != 0)
            {
                if (myAction == 1) // Move submesh up
                {
                    if (rowNum > 0)
                    {
                        tbEditor.resultMaterials.MoveArrayElement(rowNum, rowNum - 1);
                    }
                }

                if (myAction == 2) // Move submesh down
                {
                    if (rowNum < tbEditor.resultMaterials.arraySize - 1)
                    {
                        tbEditor.resultMaterials.MoveArrayElement(rowNum, rowNum + 1);
                    }
                }

                if (myAction == 3) // Delete submesh
                {
                    tbEditor.resultMaterials.DeleteArrayElementAtIndex(rowNum);
                }

                if (myAction == 4) // Insert new submesh
                {
                    tbEditor.resultMaterials.InsertArrayElementAtIndex(rowNum);
                }

                if (myAction == 5) // Create combined material
                {
                    _CreateCombinedMaterialForSubmesh(momm, tbEditor, rowNum);
                }
            }

            EditorGUILayout.EndHorizontal();
            if (rowNum < tbEditor.resultMaterials.arraySize)
            {
                EditorGUILayout.Separator();
                SerializedProperty resMat = tbEditor.resultMaterials.GetArrayElementAtIndex(rowNum);
                SerializedProperty combinedMatProp = resMat.FindPropertyRelative("combinedMaterial");
                EditorGUILayout.PropertyField(combinedMatProp);
                EditorGUILayout.PropertyField(resMat.FindPropertyRelative("considerMeshUVs"));
                DrawMappingSourceMaterials(resMat, tbEditor, rowNum);
            }
            EditorGUILayout.EndVertical();
        }

        static void _CreateCombinedMaterialForSubmesh(MB3_TextureBaker momm, MB3_TextureBakerEditorInternal tbEditor, int rowNum)
        {
            string newMatPath = EditorUtility.SaveFilePanelInProject("Asset name", "", "mat", "Enter a name for the baked material");
            if (newMatPath != null && newMatPath.Length > 4)
            {
                Material newMat = null;
                {
                    if (momm.resultMaterials[rowNum].sourceMaterials.Count > 0)
                    {
                        Material sourceMat = momm.resultMaterials[rowNum].sourceMaterials[0];
                        if (sourceMat != null)
                        {
                            newMat = new Material(sourceMat);
                            MB3_TextureBaker.ConfigureNewMaterialToMatchOld(newMat, sourceMat);
                        }
                    }
                }

                if (newMat == null)
                {
                    newMat = new Material(Shader.Find("Diffuse"));
                }

                AssetDatabase.CreateAsset(newMat, newMatPath);
                var assetRef = (Material)AssetDatabase.LoadAssetAtPath(newMatPath, typeof(Material));
                SerializedProperty resMat = tbEditor.resultMaterials.GetArrayElementAtIndex(rowNum);
                SerializedProperty combinedMatProp = resMat.FindPropertyRelative("combinedMaterial");
                combinedMatProp.objectReferenceValue = assetRef;
            }
        }

        public static void DrawMappingSourceMaterials(SerializedProperty resMat, MB3_TextureBakerEditorInternal tbEditor, int rowNum)
        {
            EditorGUILayout.LabelField(sourceMaterialsLabel);
            SerializedProperty sourceMats = resMat.FindPropertyRelative("sourceMaterials");
            bool showListSize = true;
            EditorGUI.indentLevel += 1;

            sourceMats.isExpanded = true;
            if (sourceMats.isExpanded)
            {
                if (showListSize)
                {
                    EditorGUILayout.PropertyField(sourceMats.FindPropertyRelative("Array.size"));
                }
                for (int i = 0; i < sourceMats.arraySize; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(sourceMats.GetArrayElementAtIndex(i));

                    // ========================
                    {
                        int myAction = 0;
                        myAction = EditorGUILayout.Popup(myAction, gc_SourceMatOptions, GUILayout.MaxWidth(60f));
                        if (myAction != 0)
                        {
                            if (myAction == 1) // move source mat up one mapping
                            {
                                if (rowNum > 0)
                                {
                                    SerializedProperty resMatUp = tbEditor.resultMaterials.GetArrayElementAtIndex(rowNum - 1);
                                    SerializedProperty sourceMatsUp = resMatUp.FindPropertyRelative("sourceMaterials");
                                    int idx = sourceMatsUp.arraySize;
                                    sourceMatsUp.InsertArrayElementAtIndex(idx);
                                    sourceMatsUp.GetArrayElementAtIndex(idx).objectReferenceValue = sourceMats.GetArrayElementAtIndex(i).objectReferenceValue;
                                    sourceMats.DeleteArrayElementAtIndex(i);
                                }
                            }

                            if (myAction == 2) // move source mat down one mapping
                            {
                                if (rowNum < tbEditor.resultMaterials.arraySize - 1)
                                {
                                    SerializedProperty resMatDown = tbEditor.resultMaterials.GetArrayElementAtIndex(rowNum + 1);
                                    SerializedProperty sourceMatsDown = resMatDown.FindPropertyRelative("sourceMaterials");

                                    sourceMatsDown.InsertArrayElementAtIndex(0);
                                    sourceMatsDown.GetArrayElementAtIndex(0).objectReferenceValue = sourceMats.GetArrayElementAtIndex(i).objectReferenceValue;
                                    sourceMats.DeleteArrayElementAtIndex(i);
                                }
                            }
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUI.indentLevel -= 1;
        }

        public static void DrawMultipleMaterialsMappings(MB3_TextureBaker momm, SerializedObject textureBaker, MB3_TextureBakerEditorInternal tbEditor)
        {
            EditorGUILayout.BeginVertical(tbEditor.editorStyles.multipleMaterialBackgroundStyle);
            EditorGUILayout.LabelField("Source Material To Combined Mapping", EditorStyles.boldLabel);

            float oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 300;
            EditorGUILayout.PropertyField(tbEditor.doMultiMaterialIfOBUVs, MB3_TextureBakerEditorInternal.gc_DoMultiMaterialSplitAtlasesIfOBUVs);
            EditorGUILayout.PropertyField(tbEditor.doMultiMaterialSplitAtlasesIfTooBig, MB3_TextureBakerEditorInternal.gc_DoMultiMaterialSplitAtlasesIfTooBig);
            EditorGUIUtility.labelWidth = oldLabelWidth;


            if (GUILayout.Button(MB3_TextureBakerEditorInternal.configAtlasMultiMatsFromObjsContent))
            {
                MB_TextureBakerEditorConfigureMultiMaterials.ConfigureMutiMaterialsFromObjsToCombine(momm, tbEditor.resultMaterials, textureBaker);
            }

            EditorGUILayout.BeginHorizontal();
            tbEditor.resultMaterialsFoldout = EditorGUILayout.Foldout(tbEditor.resultMaterialsFoldout, MB3_TextureBakerEditorInternal.combinedMaterialsGUIContent);

            if (GUILayout.Button(MB3_TextureBakerEditorInternal.insertContent, EditorStyles.miniButtonLeft, MB3_TextureBakerEditorInternal.buttonWidth))
            {
                if (tbEditor.resultMaterials.arraySize == 0)
                {
                    momm.resultMaterials = new MB_MultiMaterial[1];
                    momm.resultMaterials[0] = new MB_MultiMaterial();
                    momm.resultMaterials[0].considerMeshUVs = momm.fixOutOfBoundsUVs;
                }
                else
                {
                    int idx = tbEditor.resultMaterials.arraySize - 1;
                    tbEditor.resultMaterials.InsertArrayElementAtIndex(idx);
                    tbEditor.resultMaterials.GetArrayElementAtIndex(idx + 1).FindPropertyRelative("considerMeshUVs").boolValue = momm.fixOutOfBoundsUVs;
                }
            }
            if (GUILayout.Button(MB3_TextureBakerEditorInternal.deleteContent, EditorStyles.miniButtonRight, MB3_TextureBakerEditorInternal.buttonWidth)
                && tbEditor.resultMaterials.arraySize > 0)
            {
                tbEditor.resultMaterials.DeleteArrayElementAtIndex(tbEditor.resultMaterials.arraySize - 1);
            }
            EditorGUILayout.EndHorizontal();
            if (tbEditor.resultMaterialsFoldout)
            {
                for (int i = 0; i < tbEditor.resultMaterials.arraySize; i++)
                {
                    DrawMapping(momm, tbEditor, i);
                }
            }

            EditorGUILayout.EndVertical();
        }


        /* tried to see if the MultiMaterialConfig could be done using the GroupBy filters. Saddly it didn't work */
        public static void ConfigureMutiMaterialsFromObjsToCombine2(MB3_TextureBaker mom, SerializedProperty resultMaterials, SerializedObject textureBaker)
        {
            if (mom.GetObjectsToCombine().Count == 0)
            {
                Debug.LogError("You need to add some objects to combine before building the multi material list.");
                return;
            }
            if (resultMaterials.arraySize > 0)
            {
                Debug.LogError("You already have some source to combined material mappings configured. You must remove these before doing this operation.");
                return;
            }
            if (mom.textureBakeResults == null)
            {
                Debug.LogError("Texture Bake Result asset must be set before using this operation.");
                return;
            }

            //validate that the objects to be combined are valid
            for (int i = 0; i < mom.GetObjectsToCombine().Count; i++)
            {
                GameObject go = mom.GetObjectsToCombine()[i];
                if (go == null)
                {
                    Debug.LogError("Null object in list of objects to combine at position " + i);
                    return;
                }
                Renderer r = go.GetComponent<Renderer>();
                if (r == null || (!(r is MeshRenderer) && !(r is SkinnedMeshRenderer)))
                {
                    Debug.LogError("GameObject at position " + i + " in list of objects to combine did not have a renderer");
                    return;
                }
                if (r.sharedMaterial == null)
                {
                    Debug.LogError("GameObject at position " + i + " in list of objects to combine has a null material");
                    return;
                }
            }

            IGroupByFilter[] filters = new IGroupByFilter[3];
            filters[0] = new GroupByOutOfBoundsUVs();
            filters[1] = new GroupByShader();
            filters[2] = new MB3_GroupByTransparency();

            List<GameObjectFilterInfo> gameObjects = new List<GameObjectFilterInfo>();
            HashSet<GameObject> objectsAlreadyIncludedInBakers = new HashSet<GameObject>();
            for (int i = 0; i < mom.GetObjectsToCombine().Count; i++)
            {
                GameObjectFilterInfo goaw = new GameObjectFilterInfo(mom.GetObjectsToCombine()[i], objectsAlreadyIncludedInBakers, filters);
                if (goaw.materials.Length > 0) //don't consider renderers with no materials
                {
                    gameObjects.Add(goaw);
                }
            }

            //analyse meshes
            Dictionary<int, MB_Utility.MeshAnalysisResult> meshAnalysisResultCache = new Dictionary<int, MB_Utility.MeshAnalysisResult>();
            int totalVerts = 0;
            for (int i = 0; i < gameObjects.Count; i++)
            {
                //string rpt = String.Format("Processing {0} [{1} of {2}]", gameObjects[i].go.name, i, gameObjects.Count);
                //EditorUtility.DisplayProgressBar("Analysing Scene", rpt + " A", .6f);
                Mesh mm = MB_Utility.GetMesh(gameObjects[i].go);
                int nVerts = 0;
                if (mm != null)
                {
                    nVerts += mm.vertexCount;
                    MB_Utility.MeshAnalysisResult mar;
                    if (!meshAnalysisResultCache.TryGetValue(mm.GetInstanceID(), out mar))
                    {

                        //EditorUtility.DisplayProgressBar("Analysing Scene", rpt + " Check Out Of Bounds UVs", .6f);
                        MB_Utility.hasOutOfBoundsUVs(mm, ref mar);
                        //Rect dummy = mar.uvRect;
                        MB_Utility.doSubmeshesShareVertsOrTris(mm, ref mar);
                        meshAnalysisResultCache.Add(mm.GetInstanceID(), mar);
                    }
                    if (mar.hasOutOfBoundsUVs)
                    {
                        int w = (int)mar.uvRect.width;
                        int h = (int)mar.uvRect.height;
                        gameObjects[i].outOfBoundsUVs = true;
                        gameObjects[i].warning.AppendLine(" [WARNING: has uvs outside the range (0,1) tex is tiled " + w + "x" + h + " times]");
                    }
                    if (mar.hasOverlappingSubmeshVerts)
                    {
                        gameObjects[i].submeshesOverlap = true;
                        gameObjects[i].warning.AppendLine(" [WARNING: Submeshes share verts or triangles. 'Multiple Combined Materials' feature may not work.]");
                    }
                }
                totalVerts += nVerts;
                //EditorUtility.DisplayProgressBar("Analysing Scene", rpt + " Validate OBuvs Multi Material", .6f);
                Renderer mr = gameObjects[i].go.GetComponent<Renderer>();
                if (!MB_Utility.AreAllSharedMaterialsDistinct(mr.sharedMaterials))
                {
                    gameObjects[i].warning.AppendLine(" [WARNING: Object uses same material on multiple submeshes. This may produce poor results when used with multiple materials or Consider Mesh UVs.]");
                }
            }

            List<GameObjectFilterInfo> objsNotAddedToBaker = new List<GameObjectFilterInfo>();

            Dictionary<GameObjectFilterInfo, List<List<GameObjectFilterInfo>>> gs2bakeGroupMap = MB3_MeshBakerEditorWindowAnalyseSceneTab.sortIntoBakeGroups3(gameObjects, objsNotAddedToBaker, filters, false, mom.maxAtlasSize);

            mom.resultMaterials = new MB_MultiMaterial[gs2bakeGroupMap.Keys.Count];
            string pth = AssetDatabase.GetAssetPath(mom.textureBakeResults);
            string baseName = Path.GetFileNameWithoutExtension(pth);
            string folderPath = pth.Substring(0, pth.Length - baseName.Length - 6);
            int k = 0;
            foreach (GameObjectFilterInfo m in gs2bakeGroupMap.Keys)
            {
                MB_MultiMaterial mm = mom.resultMaterials[k] = new MB_MultiMaterial();
                mm.sourceMaterials = new List<Material>();
                mm.sourceMaterials.Add(m.materials[0]);
                string matName = folderPath + baseName + "-mat" + k + ".mat";
                Material newMat = new Material(Shader.Find("Diffuse"));
                MB3_TextureBaker.ConfigureNewMaterialToMatchOld(newMat, m.materials[0]);
                AssetDatabase.CreateAsset(newMat, matName);
                mm.combinedMaterial = (Material)AssetDatabase.LoadAssetAtPath(matName, typeof(Material));
                k++;
            }
            MBVersionEditor.UpdateIfDirtyOrScript(textureBaker);
        }


        //posibilities
        //  using fixOutOfBoundsUVs or not 
        //  
        public static void ConfigureMutiMaterialsFromObjsToCombine(MB3_TextureBaker mom, SerializedProperty resultMaterials, SerializedObject textureBaker)
        {
            if (mom.GetObjectsToCombine().Count == 0)
            {
                Debug.LogError("You need to add some objects to combine before building the multi material list.");
                return;
            }
            if (resultMaterials.arraySize > 0)
            {
                Debug.LogError("You already have some source to combined material mappings configured. You must remove these before doing this operation.");
                return;
            }
            if (mom.textureBakeResults == null)
            {
                Debug.LogError("Texture Bake Result asset must be set before using this operation.");
                return;
            }

            Dictionary<MB3_TextureBakerEditorInternal.MultiMatSubmeshInfo, List<List<Material>>> shader2Material_map = new Dictionary<MB3_TextureBakerEditorInternal.MultiMatSubmeshInfo, List<List<Material>>>();
            Dictionary<Material, Mesh> obUVobject2mesh_map = new Dictionary<Material, Mesh>();

            //validate that the objects to be combined are valid
            for (int goIdx = 0; goIdx < mom.GetObjectsToCombine().Count; goIdx++)
            {
                GameObject go = mom.GetObjectsToCombine()[goIdx];
                if (go == null)
                {
                    Debug.LogError("Null object in list of objects to combine at position " + goIdx);
                    return;
                }
                Renderer r = go.GetComponent<Renderer>();
                if (r == null || (!(r is MeshRenderer) && !(r is SkinnedMeshRenderer)))
                {
                    Debug.LogError("GameObject at position " + goIdx + " in list of objects to combine did not have a renderer");
                    return;
                }
                for (int matIdx = 0; matIdx < r.sharedMaterials.Length; matIdx++)
                {
                    if (r.sharedMaterials[matIdx] == null)
                    {
                        Debug.LogError("GameObject " + go + " at position " + goIdx + " in list of objects to combine has one or more null materials");
                        return;
                    }
                }
            }

            //first pass put any meshes with obUVs on their own submesh if not fixing OB uvs
            if (mom.doMultiMaterialSplitAtlasesIfOBUVs)
            {
                for (int goIdx = 0; goIdx < mom.GetObjectsToCombine().Count; goIdx++)
                {
                    GameObject go = mom.GetObjectsToCombine()[goIdx];
                    Mesh m = MB_Utility.GetMesh(go);
                    MB_Utility.MeshAnalysisResult dummyMar = new MB_Utility.MeshAnalysisResult();
                    Renderer r = go.GetComponent<Renderer>();
                    for (int matIdx = 0; matIdx < r.sharedMaterials.Length; matIdx++)
                    {
                        if (MB_Utility.hasOutOfBoundsUVs(m, ref dummyMar, matIdx))
                        {
                            if (!obUVobject2mesh_map.ContainsKey(r.sharedMaterials[matIdx]))
                            {
                                Debug.LogWarning("Object " + go + " submesh " + matIdx + " uses UVs outside the range 0,0..1,1 to generate tiling. This object has been mapped to its own submesh in the combined mesh. It can share a submesh with other objects that use different materials if you use the Consider Mesh UVs feature which will bake the tiling");
                                obUVobject2mesh_map.Add(r.sharedMaterials[matIdx], m);
                            }
                        }
                    }
                }
            }

            //second pass  put other materials without OB uvs in a shader to material map
            for (int goIdx = 0; goIdx < mom.GetObjectsToCombine().Count; goIdx++)
            {
                Renderer r = mom.GetObjectsToCombine()[goIdx].GetComponent<Renderer>();
                for (int matIdx = 0; matIdx < r.sharedMaterials.Length; matIdx++)
                {
                    if (!obUVobject2mesh_map.ContainsKey(r.sharedMaterials[matIdx]))
                    { //if not already added
                        if (r.sharedMaterials[matIdx] == null) continue;
                        List<List<Material>> binsOfMatsThatUseShader = null;
                        MB3_TextureBakerEditorInternal.MultiMatSubmeshInfo newKey = new MB3_TextureBakerEditorInternal.MultiMatSubmeshInfo(r.sharedMaterials[matIdx].shader, r.sharedMaterials[matIdx]);
                        if (!shader2Material_map.TryGetValue(newKey, out binsOfMatsThatUseShader))
                        {
                            binsOfMatsThatUseShader = new List<List<Material>>();
                            binsOfMatsThatUseShader.Add(new List<Material>());
                            shader2Material_map.Add(newKey, binsOfMatsThatUseShader);
                        }
                        if (!binsOfMatsThatUseShader[0].Contains(r.sharedMaterials[matIdx])) binsOfMatsThatUseShader[0].Add(r.sharedMaterials[matIdx]);
                    }
                }
            }

            int numResMats = shader2Material_map.Count;
            //third pass for each shader grouping check how big the atlas would be and group into bins that would fit in an atlas
            if (mom.doMultiMaterialSplitAtlasesIfTooBig)
            {
                if (mom.packingAlgorithm == MB2_PackingAlgorithmEnum.UnitysPackTextures)
                {
                    Debug.LogWarning("Unity texture packer does not support splitting atlases if too big. Atlases will not be split.");
                }
                else
                {
                    numResMats = 0;
                    foreach (MB3_TextureBakerEditorInternal.MultiMatSubmeshInfo sh in shader2Material_map.Keys)
                    {
                        List<List<Material>> binsOfMatsThatUseShader = shader2Material_map[sh];
                        List<Material> allMatsThatUserShader = binsOfMatsThatUseShader[0];//at this point everything is in the same list
                        binsOfMatsThatUseShader.RemoveAt(0);
                        MB3_TextureCombiner combiner = mom.CreateAndConfigureTextureCombiner();
                        combiner.saveAtlasesAsAssets = false;
                        if (allMatsThatUserShader.Count > 1) combiner.fixOutOfBoundsUVs = mom.fixOutOfBoundsUVs;
                        else combiner.fixOutOfBoundsUVs = false;

                        // Do the texture pack
                        List<AtlasPackingResult> packingResults = new List<AtlasPackingResult>();
                        Material tempMat = new Material(sh.shader);
                        MB_AtlasesAndRects atlasesAndRects = new MB_AtlasesAndRects();
                        combiner.CombineTexturesIntoAtlases(null, atlasesAndRects, tempMat, mom.GetObjectsToCombine(), allMatsThatUserShader, mom.texturePropNamesToIgnore, null, packingResults,
                            onlyPackRects: true, splitAtlasWhenPackingIfTooBig: true);
                        for (int aprIdx = 0; aprIdx < packingResults.Count; aprIdx++)
                        {

                            List<MB_MaterialAndUVRect> matsData = (List<MB_MaterialAndUVRect>)packingResults[aprIdx].data;
                            List<Material> mats = new List<Material>();
                            for (int mdIdx = 0; mdIdx < matsData.Count; mdIdx++)
                            {
                                Material mat = matsData[mdIdx].material;
                                if (!mats.Contains(mat))
                                {
                                    mats.Add(mat);
                                }
                            }
                            binsOfMatsThatUseShader.Add(mats);
                        }
                        numResMats += binsOfMatsThatUseShader.Count;
                    }
                }
            }

            //build the result materials
            if (shader2Material_map.Count == 0 && obUVobject2mesh_map.Count == 0) Debug.LogError("Found no materials in list of objects to combine");
            mom.resultMaterials = new MB_MultiMaterial[numResMats + obUVobject2mesh_map.Count];
            string pth = AssetDatabase.GetAssetPath(mom.textureBakeResults);
            string baseName = Path.GetFileNameWithoutExtension(pth);
            string folderPath = pth.Substring(0, pth.Length - baseName.Length - 6);
            int k = 0;
            foreach (MB3_TextureBakerEditorInternal.MultiMatSubmeshInfo sh in shader2Material_map.Keys)
            {
                foreach (List<Material> matsThatUse in shader2Material_map[sh])
                {
                    MB_MultiMaterial mm = mom.resultMaterials[k] = new MB_MultiMaterial();
                    mm.sourceMaterials = matsThatUse;
                    if (mm.sourceMaterials.Count == 1)
                    {
                        mm.considerMeshUVs = false;
                    }
                    else
                    {
                        mm.considerMeshUVs = mom.fixOutOfBoundsUVs;
                    }
                    string matName = folderPath + baseName + "-mat" + k + ".mat";
                    Material newMat = new Material(Shader.Find("Diffuse"));
                    if (matsThatUse.Count > 0 && matsThatUse[0] != null)
                    {
                        MB3_TextureBaker.ConfigureNewMaterialToMatchOld(newMat, matsThatUse[0]);
                    }
                    AssetDatabase.CreateAsset(newMat, matName);
                    mm.combinedMaterial = (Material)AssetDatabase.LoadAssetAtPath(matName, typeof(Material));
                    k++;
                }
            }
            foreach (Material m in obUVobject2mesh_map.Keys)
            {
                MB_MultiMaterial mm = mom.resultMaterials[k] = new MB_MultiMaterial();
                mm.sourceMaterials = new List<Material>();
                mm.sourceMaterials.Add(m);
                mm.considerMeshUVs = false;
                string matName = folderPath + baseName + "-mat" + k + ".mat";
                Material newMat = new Material(Shader.Find("Diffuse"));
                MB3_TextureBaker.ConfigureNewMaterialToMatchOld(newMat, m);
                AssetDatabase.CreateAsset(newMat, matName);
                mm.combinedMaterial = (Material)AssetDatabase.LoadAssetAtPath(matName, typeof(Material));
                k++;
            }
            MBVersionEditor.UpdateIfDirtyOrScript(textureBaker);
        }

    }
}
