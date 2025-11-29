//----------------------------------------------
//            MeshBaker
// Copyright © 2011-2024 Ian Deane
//----------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;
using DigitalOpus.MB.Core;


namespace DigitalOpus.MB.MBEditor
{
    /// <summary>
    /// MB3_ColorTintTextureGenerator handles all the logic for the MB3_CreateCOlorTintWindow class.
    /// 
    /// Provides functionality for detecting materials which are candidates for color tint texture generation and for performing this generation.
    /// 
    /// Makes use of the ColorTintTextureData structure which is also defined in this file to pass information to the MB3_CreateColorTintWindow class.
    /// </summary>
    public class MB3_ColorTintTextureGenerator
    {
        public struct ColorTintTextureData
        {
            public Material mat;
            public bool willGenerateColorTexture;
            public bool hasColorProperty;
            public bool hasTextureProperty;
            public Color colorProperty;
            public Texture textureProperty;
            public List<string> messages;
        }


        private MB3_TextureBaker textureBaker;
        private Dictionary<Color, string> color2TexMap;

        public MB3_ColorTintTextureGenerator(MB3_TextureBaker tb)
        {
            textureBaker = tb;
            color2TexMap = new Dictionary<Color, string>();
        }

        public List<ColorTintTextureData> GenerateColorTintTextures(MB3_TextureBaker tb, List<ColorTintTextureData> ctData,
                                                                    string path, int texSize,
                                                                    string[] colorProps, string[] albedoProps,
                                                                    bool applyChanges, out List<string> fileNames)
        {
            //  validate inputs
            if (textureBaker == null)
            {
                string errorMsg = "Texture baker is not set. This operation requires a texture baker reference.";
                throw new Exception(errorMsg);
            }
            else if (tb != textureBaker)
            {
                string errorMsg = "Selected texture baker has changed since material list was found.\nPlease find color tint textures again before attempting to generate.";
                throw new Exception(errorMsg);
            }
            if (applyChanges && (path == null || path == ""))
            {
                string errorMsg = "Folder path must be selected before color tint textures can be generated.";
                throw new Exception(errorMsg);
            }
            if (applyChanges && ctData == null)
            {
                string errorMsg = "Color tint textures must be found before color tint textures can be generated. Please click \'Find Color Tint Textures\' first.\nThis error may occur if texture baker is changed after color tint textures are found.";
                throw new Exception(errorMsg);
            }
            if (!MB3_MeshBakerRoot.ValidateTextureBakerGameObjects(textureBaker,
                                                                textureBaker.GetObjectsToCombine(),
                                                                MB2_ValidationLevel.quick))
            {
                string errorMsg = "Something went wrong when validating the game objects belonging to the texture baker." +
                            "Double check that your texture baker is set up correctly.";
                throw new Exception(errorMsg);
            }
            // check if we have at least one colorprop and one albedoprop to search for
            if (colorProps.Length <= 0)
            {
                string errorMsg = "No color properties provided." +
                "There must be at least one color property present in the array.";
                throw new Exception(errorMsg);
            }
            else
            {
                for (int i = 0; i < colorProps.Length; i++)
                {
                    if (colorProps[i] == "")
                    {
                        string errorMsg = "One or more of your color properties is empty." +
                        "Please ensure all properties have an assigned string.";
                        throw new Exception(errorMsg);
                    }
                }
            }
            if (albedoProps.Length <= 0)
            {
                string errorMsg = "No albedo properties provided." +
                "There must be at least one albedo property present in the array.";
                throw new Exception(errorMsg);
            }
            else
            {
                for (int i = 0; i < albedoProps.Length; i++)
                {
                    if (albedoProps[i] == "")
                    {
                        string errorMsg = "One or more of your albedo properties is empty." +
                        "Please ensure all properties have an assigned string.";
                        throw new Exception(errorMsg);
                    }
                }
            }
            List<GameObject> objsToCombine = textureBaker.GetObjectsToCombine();
            // check that objects have been added to texture baker
            if (objsToCombine.Count == 0)
            {
                string errorMsg = "No game objects have been added to the selected Texture Baker.";
                throw new Exception(errorMsg);
            }

            // handle initializations
            List<ColorTintTextureData> dataList = new List<ColorTintTextureData>();
            fileNames = new List<string>();
            color2TexMap.Clear();
            // retrieve materials from ObjectsToCombine
            List<Material> matsToCombine = new List<Material>();
            for (int i = 0; i < objsToCombine.Count; i++)
            {
                matsToCombine.AddRange(objsToCombine[i].GetComponent<Renderer>().sharedMaterials.ToList());
            }
            // ensure our material list does not contain duplicates
            matsToCombine = matsToCombine.Distinct().ToList();
            //do validation on materials
            for (int i = matsToCombine.Count - 1; i >= 0; i--)
            {
                ColorTintTextureData data = new ColorTintTextureData
                {
                    messages = new List<string>(),
                    willGenerateColorTexture = true,
                    mat = matsToCombine[i]
                };
                Material mat = matsToCombine[i];

                // check if material is null 
                if (mat == null)
                {
                    data.willGenerateColorTexture = false;
                    data.messages.Add("Material was null.");
                    matsToCombine.Remove(mat);
                }
                else if (!AssetDatabase.GetAssetPath(mat).Contains("Assets"))
                {
                    data.willGenerateColorTexture = false;
                    data.messages.Add("Material was not in the \'Assets\' folder: " + mat);
                    matsToCombine.Remove(mat);
                }
                else
                {
                    // check if material has no albedo texture or color tint (remove from list)
                    data.hasColorProperty = false;
                    data.hasTextureProperty = false;
                    data.colorProperty = GetColorFromMaterial(mat, colorProps, out data.hasColorProperty);
                    data.textureProperty = GetTextureFromMaterial(mat, albedoProps, out data.hasTextureProperty);

                    if (!data.hasColorProperty)
                    {
                        data.willGenerateColorTexture = false;
                        data.messages.Add("Color property could not be detected.");
                    }
                    if (!data.hasTextureProperty)
                    {
                        data.willGenerateColorTexture = false;
                        data.messages.Add("Texture property could not be detected.");
                    }
                    else
                    {
                        // if material has texture property field the value of that field must be null or mat will be removed
                        if (data.textureProperty != null)
                        {
                            data.willGenerateColorTexture = false;
                            data.messages.Add("Material " + mat + "already has albedo texture in detected texture property.");
                        }
                    }
                    if (!data.willGenerateColorTexture)
                    {
                        matsToCombine.Remove(mat);
                    }
                }
                dataList.Add(data);
            }

            if (applyChanges)
            {
                // get unique colors
                List<Color> colorsOnMats = new List<Color>();
                Undo.RecordObjects(matsToCombine.ToArray(), "Color Tint Texture Generation");
                for (int i = 0; i < matsToCombine.Count; i++)
                {
                    Material mat = matsToCombine[i];
                    if (mat != null)
                    {
                        Color tintColor;
                        bool foundColor;
                        tintColor = GetColorFromMaterial(mat, colorProps, out foundColor);
                        // at this point we should be able to assume that the material has a color associated with one of our property names
                        Debug.Assert(foundColor, "Expected material " + mat + " to have a detectable color property but none was found.");
                        colorsOnMats.Add(tintColor);
                    }
                }
                colorsOnMats = colorsOnMats.Distinct().ToList();
                for (int i = 0; i < colorsOnMats.Count; i++)
                {
                    Color tintColor = colorsOnMats[i];
                    Texture2D tintTexture;
                    if (!color2TexMap.ContainsKey(tintColor))
                    {
                        string filePath = BuildAndSaveColorTintTexture(path, "ColorTintTexture" + i, texSize, tintColor, out tintTexture);
                        fileNames.Add(filePath);
                        color2TexMap.Add(tintColor, filePath);
                    }
                }
                AssignTintTexturesToMaterials(matsToCombine, colorProps, albedoProps);
                return dataList;
            }
            else
            {
                return dataList;
            }
        }

        private string BuildAndSaveColorTintTexture(string path, string fileName, int texSize,
                                                Color tintColor, out Texture2D tintTexture)
        {
            // create size by size texture filled with 'color' pixels
            bool linear = false;
            switch (PlayerSettings.colorSpace)
            {
                case ColorSpace.Uninitialized:
                    string errorMsg = "Project color space is not initialized. Could not generate 2D textures";
                    throw new Exception(errorMsg);
                case ColorSpace.Gamma:
                    linear = false;
                    break;
                case ColorSpace.Linear:
                    linear = true;
                    break;
            }
            tintTexture = new Texture2D(texSize, texSize, TextureFormat.RGBA32, true, linear);
            Color[] pixels = new Color[texSize * texSize];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = tintColor;
            }
            tintTexture.SetPixels(pixels);
            tintTexture.Apply();

            // save texture as file in 'path' folder
            string filePath = path + "/" + fileName + ".asset";
            filePath = AssetDatabase.GenerateUniqueAssetPath(filePath);

            // Write the PNG texture data to a file
            AssetDatabase.CreateAsset(tintTexture, filePath);
            return filePath;
        }

        private void AssignTintTexturesToMaterials(List<Material> mats, string[] colorProps, string[] albedoProps)
        {
            // visit each source material
            for (int i = 0; i < mats.Count; i++)
            {
                Material mat = mats[i];
                if (mat != null)
                {
                    // get color tint
                    bool foundColor;
                    Color tintColor = GetColorFromMaterial(mat, colorProps, out foundColor);
                    Debug.Assert(foundColor, "Expected material " + mat + " to have a detectable color property but none was found.");
                    // feed color tint into dictionary and assign corresponding texture to material
                    string texturePath = color2TexMap[tintColor];
                    Texture2D textureToAssign = (Texture2D)AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D));
                    bool foundTexture;
                    SetMaterialTexture(mat, albedoProps, textureToAssign, out foundTexture);
                    Debug.Assert(foundTexture, "Expected material " + mat + " to have detectable texture property but none was found.");
                    SetMaterialColorToDefault(mat, colorProps, out foundColor);
                    Debug.Assert(foundColor, "Material " + mat + " had detectable material property when fetching color but not when setting color.");
                }
            }
        }

        private Color GetColorFromMaterial(Material mat, string[] colorProperties, out bool foundColor)
        {
            Color color = new Color();
            for (int i = 0; i < colorProperties.Length; i++)
            {
                string prop = colorProperties[i];
                if (mat.HasProperty(prop))
                {
                    color = mat.GetColor(prop);
                    foundColor = true;
                    return color;
                }
            }
            foundColor = false;
            return color;
        }

        private void SetMaterialColorToDefault(Material mat, string[] colorProps, out bool foundColor)
        {
            foundColor = false;
            for (int i = 0; i < colorProps.Length; i++)
            {
                if (mat.HasProperty(colorProps[i]))
                {
                    Color currentColor = mat.GetColor(colorProps[i]);
                    if (currentColor != Color.white)
                    {
                        mat.SetColor(colorProps[i], Color.white);
                    }
                    foundColor = true;
                }
            }
        }

        private void SetMaterialTexture(Material mat, string[] albedoProps, Texture2D tex, out bool foundTexture)
        {
            foundTexture = false;
            for (int i = 0; i < albedoProps.Length; i++)
            {
                if (mat.HasProperty(albedoProps[i]))
                {
                    mat.SetTexture(albedoProps[i], tex);
                    foundTexture = true;
                }
            }
        }

        private Texture GetTextureFromMaterial(Material mat, string[] texProperties, out bool foundTex)
        {
            Texture tex = null;
            for (int i = 0; i < texProperties.Length; i++)
            {
                string prop = texProperties[i];
                if (mat.HasProperty(prop))
                {
                    tex = mat.GetTexture(prop);
                    foundTex = true;
                    return tex;
                }
            }
            foundTex = false;
            return tex;
        }

        public static string BrowseOutputFolder()
        {
            string errorMsg;
            bool success;
            string path = EditorUtility.OpenFolderPanel("Browse for Output Folder", "", "");
            path = MB3_MeshBakerEditorFunctionsCore.SanitizeAndMakeFullPathRelativeToAssetsFolderAndValidate(path, out errorMsg, out success);
            if (success)
            {
                return path;
            }
            else
            {
                Debug.LogError(errorMsg);
                return null;
            }
        }
    }
}
