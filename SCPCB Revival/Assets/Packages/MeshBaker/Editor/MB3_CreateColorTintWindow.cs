//----------------------------------------------
//            MeshBaker
// Copyright © 2011-2024 Ian Deane
//----------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace DigitalOpus.MB.MBEditor
{
    /// <summary>
    /// Component that handles the UI for generating color tint textures for all materials in meshRenderers that belong to the texture baker.
    /// 
    /// The user is able to set a folder for textures to be generated to, set generated texture size, and add or remove color and albedo texture property names
    /// that will be used when searching for materials that are candidates for color tint texture generation.
    /// 
    /// The user can see a list of all available materials under the texture baker and whether color tint textures will be generated for them or not.
    /// Once the user is satisfied with this list they can generate color tint textures which will automatically be assigned to the relevant materials.
    /// 
    /// All the logic for this window is handled in the MB3_ColorTintTextureGenerator class.
    /// </summary>
    namespace DigitalOpus.MB.MBEditor
    {
        public class MB3_CreateColorTintWindow : EditorWindow
        {
            public MB3_TextureBaker textureBaker;
            public MB3_TextureBaker newTextureBaker;
            public string folderPath = "";

            public int texSize = 16;
            [SerializeField]
            public string[] colorTintList = {
                    "_Color", //Built-in RP
                    "_BaseColor", //URP and HDRP
                    "_ColorTint", //Commonly Used
                    };
            public SerializedObject colorTintWindowObject;
            public SerializedProperty colorTintProperty;

            [SerializeField]
            public string[] albedoMapList = {
                    "_MainTex", //Built-in RP
                    "_BaseMap", // URP
                    "_BaseColorMap", //HDRP
                    };

            public SerializedProperty albedoMapProperty;

            private MB3_ColorTintTextureGenerator ctGen;
            private bool doneGeneration = false;
            private bool errorState;
            private string errorMsg;
            private List<MB3_ColorTintTextureGenerator.ColorTintTextureData> ctData;
            private List<string> generatedFiles;
            private Vector2 scrollPos;
            private static GUIContent
                gc_textureBakerSelect = new GUIContent("Texture Baker", "Choose texture baker. Solid color textures will be added to materials used by the \'Objects To Be Combined\'."),
                gc_folderSelectHeader = new GUIContent("Select Folder for Saving Textures:", "Must be a valid folder in the Assets folder."),
                gc_folderSelectButton = new GUIContent("Browse for Output Folder", "Must be a valid folder in the Assets folder."),
                gc_colorPropertyNames = new GUIContent("Color Tint Property names",
                "List of color tint property names that will be seached for in the source object materials."),
                gc_albedoPropertyNames = new GUIContent("Albedo Map Property names",
                "This is the list of texture property names that searched for in the source object materials. If this property is empty, it will be assigned a generated solid color texture."),
                gc_texSizeField = new GUIContent("Set Texture Size in Pixels (must be power of two):", "The height/width size in pixels of the color tint textures that will be generated. This field must be a power of two."),
                gc_findButton = new GUIContent("Find Color Tint Textures", "Search all materials used by 'Objects To Be Combined' for empty albedo texture properties and color tint properties. No assets will be modified. Results will be reported."),
                gc_generateButton = new GUIContent("Generate Color Tint Textures", "Create new solid color textures and assign them to source materials. This will modify source material assets.");



            [MenuItem("Window/Mesh Baker/Generate Color Tint Textures")]
            public static void Open()
            {
                // open window
                MB3_CreateColorTintWindow ctWin = (MB3_CreateColorTintWindow)EditorWindow.GetWindow(typeof(MB3_CreateColorTintWindow));
            }

            private void OnEnable()
            {
                colorTintWindowObject = new SerializedObject(this);
                colorTintProperty = colorTintWindowObject.FindProperty("colorTintList");
                albedoMapProperty = colorTintWindowObject.FindProperty("albedoMapList");
                scrollPos = Vector2.zero;
                errorState = false;
                errorMsg = "";
                ctData = null;
                ctGen = null;
                generatedFiles = new List<string>();
            }

            private void OnGUI()
            {
                //update serialized object
                colorTintWindowObject.Update();

                EditorGUILayout.HelpBox("This window finds materials used by the texture baker list of objects to be combined which have a color tint and no albedo texture. " +
                                        "It will create solid color textures matching the color tint and assign them to the source material's albedo texture. \n\n" +
                                        "Instructions:\n\n" +
                                        "1. Select Texture Baker.\n\n" +
                                        "2. Select folder where color tint textures will be saved.\n\n" +
                                        "3. Add color tint property names or albedo map property names used by source materials.\n\n" +
                                        "4. Set the output texture size. This must be a power of 2. The resulting texture will have height and width equal to \'size\'.\n\n" +
                                        "5. Click \'Find Color Tint Textures\' to generate a dry-run report of affected materials and generated textures.\n\n" +
                                        "6. Click \'Generate Color Tint Textures\' to generate solid color textures and assign them to source materials.", UnityEditor.MessageType.None);
                newTextureBaker = (MB3_TextureBaker)EditorGUILayout.ObjectField(gc_textureBakerSelect, textureBaker, typeof(MB3_TextureBaker), true);

                if (newTextureBaker != null &&
                    (newTextureBaker != textureBaker ||
                     ctGen == null))
                {
                    textureBaker = newTextureBaker;
                    ctGen = new MB3_ColorTintTextureGenerator(textureBaker);
                    ctData = null;
                }

                // get folder to save textures in
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(gc_folderSelectHeader, EditorStyles.boldLabel);
                if (GUILayout.Button(gc_folderSelectButton))
                {
                    errorState = false;
                    folderPath = MB3_ColorTintTextureGenerator.BrowseOutputFolder();
                    if (folderPath == null)
                    {
                        errorState = true;
                        errorMsg = "Selected folder could not be found. Please select a different folder.";
                    }
                }
                EditorGUILayout.LabelField("Folder: " + folderPath);
                EditorGUILayout.EndHorizontal();

                // modifiable arrays for color and albedo property names
                EditorGUILayout.PropertyField(colorTintProperty, gc_colorPropertyNames, true);
                EditorGUILayout.PropertyField(albedoMapProperty, gc_albedoPropertyNames, true);

                // set texture size field
                EditorGUILayout.BeginHorizontal();
                texSize = Mathf.ClosestPowerOfTwo(EditorGUILayout.IntField(gc_texSizeField, texSize));
                EditorGUILayout.EndHorizontal();
                if (texSize < 2)
                {
                    texSize = 2;
                }

                // button for enumerating materials and whether or not they will be updated
                if (GUILayout.Button(gc_findButton))
                {
                    errorState = false;
                    if (ctGen == null)
                    {
                        errorState = true;
                        errorMsg = "Texture baker is not assigned. This operation requires a texture baker reference.";
                    }
                    else
                    {
                        try
                        {
                            generatedFiles.Clear();
                            ctData = ctGen.GenerateColorTintTextures(textureBaker, ctData, folderPath, texSize, colorTintList, albedoMapList, false, out generatedFiles);
                            doneGeneration = false;
                        }
                        catch (Exception e)
                        {
                            errorState = true;
                            Debug.LogError(e.Message);
                            errorMsg = e.Message;
                        }
                    }
                }

                // button to generate new color tint textures and update corresponding materials
                if (GUILayout.Button(gc_generateButton))
                {
                    errorState = false;
                    if (ctGen == null)
                    {
                        errorState = true;
                        errorMsg = "Texture baker is not set. This operation requires a texture baker reference.";
                    }
                    else
                    {
                        bool response = EditorUtility.DisplayDialog("Confirm Color Texture Generation",
                                                                    "Are you sure you want to generate color tint textures?\nThis will modify materials used by the Objects To Be Combined.",
                                                                    "Yes", "No");
                        if (response)
                        {
                            try
                            {
                                EditorUtility.DisplayProgressBar("Generate Color Tint Textures...", "", 0.5f);
                                ctGen.GenerateColorTintTextures(textureBaker, ctData, folderPath, texSize, colorTintList, albedoMapList, true, out generatedFiles);
                                doneGeneration = true;
                            }
                            catch (Exception e)
                            {
                                errorState = true;
                                Debug.LogError(e.Message);
                                errorMsg = e.Message;
                            }
                            finally
                            {
                                EditorUtility.ClearProgressBar();
                            }
                        }
                    }
                }

                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, true);
                {
                    if (errorState)
                    {
                        // if an error occurred during find or generate display it
                        GUIStyle errorStyle = new GUIStyle();
                        errorStyle.normal.textColor = Color.red;
                        EditorGUILayout.LabelField("Error: " + errorMsg, errorStyle);
                    }
                    else if (doneGeneration && generatedFiles != null && generatedFiles.Count > 0)
                    {
                        // display which new files were created
                        EditorGUILayout.LabelField("The following textures were generated in folder", EditorStyles.boldLabel);
                        EditorGUILayout.LabelField(folderPath, EditorStyles.boldLabel);
                        EditorGUILayout.Separator();
                        for (int i = 0; i < generatedFiles.Count; i++)
                        {
                            EditorGUILayout.LabelField(generatedFiles[i]);
                        }
                    }
                    else if (doneGeneration && (generatedFiles == null || generatedFiles.Count == 0))
                    {
                        // notify that no new textures were generated
                        EditorGUILayout.LabelField("No new texture files were generated.", EditorStyles.boldLabel);
                    }
                    else if (ctData != null && ctData.Count > 0)
                    {
                        // display data for each material detected our first pass through GenerateColorTintTextures
                        EditorGUILayout.LabelField("Detected Materials With Color Tints & Empty Albedo Properties:", EditorStyles.boldLabel);
                        for (int i = 0; i < ctData.Count; i++)
                        {
                            EditorGUILayout.Separator();
                            DisplayColorTintData(ctData[i]);
                        }
                    }
                }

                EditorGUILayout.EndScrollView();
                colorTintWindowObject.ApplyModifiedProperties();
            }

            private void DisplayColorTintData(MB3_ColorTintTextureGenerator.ColorTintTextureData ctData)
            {
                StringBuilder sb = new StringBuilder();
                if (ctData.willGenerateColorTexture)
                {
                    // display information about color texture
                    sb.AppendLine("Material: " + ctData.mat + " Found color tint " + ctData.colorProperty + " Color tint texture will be generated.");
                }
                else
                {
                    // explain why color tint texture will not be generated
                    if (ctData.mat != null)
                    {
                        sb.AppendLine("Material " + ctData.mat + " had the following issues:");
                    }
                    for (int i = 0; i < ctData.messages.Count; i++)
                    {
                        sb.AppendLine(ctData.messages[i]);
                    }
                    sb.AppendLine("Color tint texture will not be generated for this material.");
                }
                EditorGUILayout.LabelField(sb.ToString());
            }
        }
    }
}
