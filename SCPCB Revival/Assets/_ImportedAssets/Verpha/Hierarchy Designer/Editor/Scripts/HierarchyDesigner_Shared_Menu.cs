#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Verpha.HierarchyDesigner
{
    internal static class HierarchyDesigner_Shared_Menu
    {
        #region Windows
        [MenuItem(HierarchyDesigner_Shared_Constants.AssetLocation, false, HierarchyDesigner_Shared_Constants.MenuPriorityOne)]
        private static void OpenWindow() => HierarchyDesigner_Window_Main.OpenWindow();
        #endregion

        #region Folder
        [MenuItem(HierarchyDesigner_Shared_Constants.GroupFolders + "/Create All Folders", false, HierarchyDesigner_Shared_Constants.MenuPriorityOne)]
        public static void CreateAllFolders()
        {
            foreach (KeyValuePair<string, HierarchyDesigner_Configurable_Folders.HierarchyDesigner_FolderData> folder in HierarchyDesigner_Configurable_Folders.GetAllFoldersData(false))
            {
                HierarchyDesigner_Shared_Operations.CreateFolder(folder.Key, false);
            }
        }

        [MenuItem(HierarchyDesigner_Shared_Constants.GroupFolders + "/Create Default Folder", false, HierarchyDesigner_Shared_Constants.MenuPriorityOne)]
        public static void CreateDefaultFolder()
        {
            HierarchyDesigner_Shared_Operations.CreateFolder("New Folder", true);
        }

        [MenuItem(HierarchyDesigner_Shared_Constants.GroupFolders + "/Create Missing Folders", false, HierarchyDesigner_Shared_Constants.MenuPriorityOne)]
        public static void CreateMissingFolders()
        {
            foreach (KeyValuePair<string, HierarchyDesigner_Configurable_Folders.HierarchyDesigner_FolderData> folder in HierarchyDesigner_Configurable_Folders.GetAllFoldersData(false))
            {
                if (!HierarchyDesigner_Shared_Operations.FolderExists(folder.Key))
                {
                    HierarchyDesigner_Shared_Operations.CreateFolder(folder.Key, false);
                }
            }
        }

        [MenuItem(HierarchyDesigner_Shared_Constants.GroupFolders + "/Transform GameObject into a Folder", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwo)]
        public static void TransformGameObjectIntoAFolder()
        {
            GameObject selectedObject = Selection.activeGameObject;
            if (selectedObject == null)
            {
                Debug.LogWarning("No GameObject selected.");
                return;
            }

            string folderName = selectedObject.name;
            HierarchyDesigner_Configurable_Folders.HierarchyDesigner_FolderData folderData = HierarchyDesigner_Configurable_Folders.GetFolderData(folderName);
            if (folderData == null)
            {
                HierarchyDesigner_Configurable_Folders.SetFolderData(
                    folderName,
                    HierarchyDesigner_Configurable_DesignSettings.FolderDefaultTextColor,
                    HierarchyDesigner_Configurable_DesignSettings.FolderDefaultFontSize,
                    HierarchyDesigner_Configurable_DesignSettings.FolderDefaultFontStyle,
                    HierarchyDesigner_Configurable_DesignSettings.FolderDefaultImageColor,
                    HierarchyDesigner_Configurable_DesignSettings.FolderDefaultImageType
                );
                if (selectedObject.GetComponent<HierarchyDesignerFolder>() == null)
                {
                    selectedObject.AddComponent<HierarchyDesignerFolder>();
                }
                EditorGUIUtility.SetIconForObject(selectedObject, HierarchyDesigner_Shared_Resources.FolderInspectorIcon);
                Debug.Log($"GameObject <color=#73FF7A>'{folderName}'</color> was transformed into a Folder and added to the Folders dictionary.");
            }
            else
            {
                Debug.LogWarning($"GameObject <color=#FF7674>'{folderName}'</color> already exists in the Folders dictionary.");
                return;
            }
        }

        [MenuItem(HierarchyDesigner_Shared_Constants.GroupFolders + "/Transform Folder into a GameObject", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwo + 1)]
        public static void TransformFolderIntoAGameObject()
        {
            GameObject selectedObject = Selection.activeGameObject;
            if (selectedObject == null)
            {
                Debug.LogWarning("No GameObject selected.");
                return;
            }

            HierarchyDesignerFolder folderComponent = selectedObject.GetComponent<HierarchyDesignerFolder>();
            if (folderComponent == null)
            {
                Debug.LogWarning($"GameObject <color=#FF7674>'{selectedObject.name}'</color> is not a Folder.");
                return;
            }

            string folderName = selectedObject.name;
            if (HierarchyDesigner_Configurable_Folders.RemoveFolderData(folderName))
            {
                Object.DestroyImmediate(folderComponent);
                EditorGUIUtility.SetIconForObject(selectedObject, null);
                Debug.Log($"GameObject <color=#73FF7A>'{folderName}'</color> was transformed back into a GameObject and removed from the Folders dictionary.");
            }
            else
            {
                Debug.LogWarning($"Folder data for GameObject <color=#FF7674>'{folderName}'</color> does not exist in the Folders dictionary.");
            }
        }
        #endregion

        #region Separator
        [MenuItem(HierarchyDesigner_Shared_Constants.GroupSeparators + "/Create All Separators", false, HierarchyDesigner_Shared_Constants.MenuPriorityOne)]
        public static void CreateAllSeparators()
        {
            foreach (KeyValuePair<string, HierarchyDesigner_Configurable_Separators.HierarchyDesigner_SeparatorData> separator in HierarchyDesigner_Configurable_Separators.GetAllSeparatorsData(false))
            {
                HierarchyDesigner_Shared_Operations.CreateSeparator(separator.Key);
            }
        }

        [MenuItem(HierarchyDesigner_Shared_Constants.GroupSeparators + "/Create Default Separator", false, HierarchyDesigner_Shared_Constants.MenuPriorityOne)]
        public static void CreateDefaultSeparator()
        {
            HierarchyDesigner_Shared_Operations.CreateSeparator("Separator");
        }

        [MenuItem(HierarchyDesigner_Shared_Constants.GroupSeparators + "/Create Missing Separators", false, HierarchyDesigner_Shared_Constants.MenuPriorityOne)]
        public static void CreateMissingSeparators()
        {
            foreach (KeyValuePair<string, HierarchyDesigner_Configurable_Separators.HierarchyDesigner_SeparatorData> separator in HierarchyDesigner_Configurable_Separators.GetAllSeparatorsData(false))
            {
                if (!HierarchyDesigner_Shared_Operations.SeparatorExists(separator.Key))
                {
                    HierarchyDesigner_Shared_Operations.CreateSeparator(separator.Key);
                }
            }
        }

        [MenuItem(HierarchyDesigner_Shared_Constants.GroupSeparators + "/Transform GameObject into a Separator", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwo)]
        public static void TransformGameObjectIntoASeparator()
        {
            GameObject selectedObject = Selection.activeGameObject;
            if (selectedObject == null)
            {
                Debug.LogWarning("No GameObject selected.");
                return;
            }
            if (selectedObject.GetComponents<Component>().Length > 1)
            {
                Debug.LogWarning("Separators cannot have components because separators are marked as editorOnly, meaning they will not be present in your game's build.");
                return;
            }

            string separatorName = HierarchyDesigner_Shared_Operations.StripPrefix(selectedObject.name);
            HierarchyDesigner_Configurable_Separators.HierarchyDesigner_SeparatorData separatorData = HierarchyDesigner_Configurable_Separators.GetSeparatorData(separatorName);
            if (separatorData == null)
            {
                HierarchyDesigner_Configurable_Separators.SetSeparatorData(
                    separatorName,
                    HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultTextColor,
                    HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultIsGradientBackground,
                    HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultBackgroundColor,
                    HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultBackgroundGradient,
                    HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultFontSize,
                    HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultFontStyle,
                    HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultTextAnchor,
                    HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultImageType
                );
                if (!selectedObject.name.StartsWith(HierarchyDesigner_Shared_Constants.SeparatorPrefix))
                {
                    selectedObject.name = $"{HierarchyDesigner_Shared_Constants.SeparatorPrefix}{selectedObject.name}";
                }
                selectedObject.tag = HierarchyDesigner_Shared_Constants.SeparatorTag;
                selectedObject.SetActive(false);
                EditorGUIUtility.SetIconForObject(selectedObject, HierarchyDesigner_Shared_Resources.SeparatorInspectorIcon);
                Debug.Log($"GameObject <color=#73FF7A>'{separatorName}'</color> was transformed into a Separator and added to the Separators dictionary.");
            }
            else
            {
                Debug.LogWarning($"GameObject <color=#FF7674>'{separatorName}'</color> already exists in the Separators dictionary.");
                return;
            }
            HierarchyDesigner_Shared_Operations.SetSeparatorState(selectedObject, false);
        }

        [MenuItem(HierarchyDesigner_Shared_Constants.GroupSeparators + "/Transform Separator into a GameObject", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwo + 1)]
        public static void TransformSeparatorIntoAGameObject()
        {
            GameObject selectedObject = Selection.activeGameObject;
            if (selectedObject == null)
            {
                Debug.LogWarning("No GameObject selected.");
                return;
            }

            if (!selectedObject.name.StartsWith(HierarchyDesigner_Shared_Constants.SeparatorPrefix) && !selectedObject.CompareTag(HierarchyDesigner_Shared_Constants.SeparatorTag))
            {
                Debug.LogWarning($"GameObject <color=#FF7674>'{selectedObject.name}'</color> is not a Separator.");
                return;
            }

            string separatorName = HierarchyDesigner_Shared_Operations.StripPrefix(selectedObject.name);
            if (HierarchyDesigner_Configurable_Separators.RemoveSeparatorData(separatorName))
            {
                selectedObject.name = separatorName;
                selectedObject.tag = "Untagged";
                selectedObject.SetActive(true);
                HierarchyDesigner_Shared_Operations.SetLockState(selectedObject, true);
                EditorGUIUtility.SetIconForObject(selectedObject, null);
                Debug.Log($"GameObject <color=#73FF7A>'{separatorName}'</color> was transformed back into a GameObject and removed from the Separators dictionary.");
            }
            else
            {
                Debug.LogWarning($"Separator data for GameObject <color=#FF7674>'{separatorName}'</color> does not exist in the Separators dictionary.");
            }
        }
        #endregion

        #region Tools
        #region Activate
        #region General
        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_General + "/Activate Selected GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityEight)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_SelectedGameObjects() => HierarchyDesigner_Shared_Operations.Activate_SelectedGameObjects(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_General + "/Activate All GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_AllGameObjects() => HierarchyDesigner_Shared_Operations.Activate_AllGameObjects(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_General + "/Activate All Parent GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_ParentGameObjects() => HierarchyDesigner_Shared_Operations.Activate_AllParentGameObjects(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_General + "/Activate All Empty GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_EmptyGameObjects() => HierarchyDesigner_Shared_Operations.Activate_AllEmptyGameObjects(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_General + "/Activate All Locked GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_LockedGameObjects() => HierarchyDesigner_Shared_Operations.Activate_AllLockedGameObjects(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_General + "/Activate All Folders", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Folders() => HierarchyDesigner_Shared_Operations.Activate_AllFolders(true);
        #endregion

        #region 2D Objects
        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_2D + "/Activate All Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Sprites() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<SpriteRenderer>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_2D + "/Activate All Sprite Masks", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_SpriteMasks() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<SpriteMask>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_2D_Sprites + "/Activate All 9-Sliced Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_9SlicedSprites() => HierarchyDesigner_Shared_Operations.Activate_All2DSpritesByType("9-Sliced", true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_2D_Sprites + "/Activate All Capsule Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_CapsuleSprites() => HierarchyDesigner_Shared_Operations.Activate_All2DSpritesByType("Capsule", true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_2D_Sprites + "/Activate All Circle Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_CircleSprites() => HierarchyDesigner_Shared_Operations.Activate_All2DSpritesByType("Circle", true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_2D_Sprites + "/Activate All Hexagon Flat-Top Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_HexagonFlatTopSprites() => HierarchyDesigner_Shared_Operations.Activate_All2DSpritesByType("Hexagon Flat-Top", true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_2D_Sprites + "/Activate All Hexagon Pointed-Top Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_HexagonPointedTopSprites() => HierarchyDesigner_Shared_Operations.Activate_All2DSpritesByType("Hexagon Pointed-Top", true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_2D_Sprites + "/Activate All Isometric Diamond Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_IsometricDiamondSprites() => HierarchyDesigner_Shared_Operations.Activate_All2DSpritesByType("Isometric Diamond", true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_2D_Sprites + "/Activate All Square Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_SquareSprites() => HierarchyDesigner_Shared_Operations.Activate_All2DSpritesByType("Square", true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_2D_Sprites + "/Activate All Triangle Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_TriangleSprites() => HierarchyDesigner_Shared_Operations.Activate_All2DSpritesByType("Triangle", true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_2D_Physics + "/Activate All Dynamic Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_PhysicsDynamicSprites() => HierarchyDesigner_Shared_Operations.Activate_AllPhysicsDynamicSprites(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_2D_Physics + "/Activate All Static Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_PhysicsStaticSprites() => HierarchyDesigner_Shared_Operations.Activate_AllPhysicsStaticSprites(true);
        #endregion

        #region 3D Objects
        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_3D + "/Activate All Mesh Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_MeshFilters() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<MeshFilter>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_3D + "/Activate All Mesh Renderers", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_MeshRenderers() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<MeshRenderer>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_3D + "/Activate All Skinned Mesh Renderer", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_SkinnedMeshRenderers() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<SkinnedMeshRenderer>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_3D + "/Activate All Cubes", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_CubesObjects() => HierarchyDesigner_Shared_Operations.Activate_All3DObjectsByType("Cube", true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_3D + "/Activate All Spheres", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_SpheresObjects() => HierarchyDesigner_Shared_Operations.Activate_All3DObjectsByType("Sphere", true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_3D + "/Activate All Capsules", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_CapsulesObjects() => HierarchyDesigner_Shared_Operations.Activate_All3DObjectsByType("Capsule", true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_3D + "/Activate All Cylinders", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_CylindersObjects() => HierarchyDesigner_Shared_Operations.Activate_All3DObjectsByType("Cylinder", true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_3D + "/Activate All Planes", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_PlanesObjects() => HierarchyDesigner_Shared_Operations.Activate_All3DObjectsByType("Plane", true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_3D + "/Activate All Quads", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_QuadsObjects() => HierarchyDesigner_Shared_Operations.Activate_All3DObjectsByType("Quad", true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_3D + "/Activate All Texts - TextMeshPro", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_TextMeshProObjects() => HierarchyDesigner_Shared_Operations.Activate_All3DObjectsByType("TextMeshPro Mesh", true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_3D_Legacy + "/Activate All Text Meshes", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_TextMeshesObjects() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<TextMesh>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_3D + "/Activate All Terrains", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_TerrainsObjects() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Terrain>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_3D + "/Activate All Trees", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_TreesObjects() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Tree>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_3D + "/Activate All Wind Zones", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_WindZonesObjects() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<WindZone>(true);
        #endregion

        #region Audio
        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_Audio + "/Activate All Audio Sources", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_AudioSources() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<AudioSource>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_Audio + "/Activate All Audio Reverb Zones", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_AudioReverbZones() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<AudioReverbZone>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_Audio + "/Activate All Audio Chorus Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_AudioChorusFilters() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<AudioChorusFilter>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_Audio + "/Activate All Audio Distortion Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_AudioDistortionFilters() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<AudioDistortionFilter>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_Audio + "/Activate All Audio Echo Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_AudioEchoFilters() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<AudioEchoFilter>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_Audio + "/Activate All Audio High Pass Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_AudioHighPassFilters() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<AudioHighPassFilter>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_Audio + "/Activate All Audio Listeners", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_AudioListeners() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<AudioListener>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_Audio + "/Activate All Audio Low Pass Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_AudioLowPassFilters() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<AudioLowPassFilter>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_Audio + "/Activate All Audio Reverb Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_AudioReverbFilters() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<AudioReverbFilter>(true);
        #endregion

        #region Effects
        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_Effects + "/Activate All Particle Systems", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_ParticleSystems() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<ParticleSystem>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_Effects + "/Activate All Particle System Force Fields", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_ParticleSystemForceFields() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<ParticleSystemForceField>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_Effects + "/Activate All Trail Renderers", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_TrailRenderers() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<TrailRenderer>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_Effects + "/Activate All Line Renderers", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_LineRenderers() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<LineRenderer>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_Effects + "/Activate All Halos", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Halos() => HierarchyDesigner_Shared_Operations.Activate_AllHalos(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_Effects + "/Activate All Lens Flares", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_LensFlares() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<LensFlare>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_Effects + "/Activate All Projectors", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Projectors() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Projector>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_Effects + "/Activate All Visual Effects", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_VisualEffects() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<UnityEngine.VFX.VisualEffect>(true);
        #endregion

        #region Lights
        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_Light + "/Activate All Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Lights() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Light>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_Light + "/Activate All Directional Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_DirectionalLights() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Light>(true, light => light.type == LightType.Directional);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_Light + "/Activate All Point Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_PointLights() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Light>(true, light => light.type == LightType.Point);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_Light + "/Activate All Spot Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_SpotLights() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Light>(true, light => light.type == LightType.Spot);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_Light + "/Activate All Rectangle Area Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_RectangleAreaLights() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Light>(true, light => light.type == LightType.Rectangle);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_Light + "/Activate All Disc Area Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_DiscAreaLights() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Light>(true, light => light.type == LightType.Disc);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_Light + "/Activate All Reflection Probes", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_ReflectionProbes() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<ReflectionProbe>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_Light + "/Activate All Light Probe Groups", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_LightProbeGroups() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<LightProbeGroup>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_Light + "/Activate All Light Probe Proxy Volumes", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_LightProbeProxyVolumes() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<LightProbeProxyVolume>(true);
        #endregion

        #region Video
        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_Video + "/Activate All Video Players", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_VideoPlayers() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<UnityEngine.Video.VideoPlayer>(true);
        #endregion

        #region UI Toolkit
        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_UIToolkit + "/Activate All UI Documents", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_UIDocuments() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<UnityEngine.UIElements.UIDocument>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_UIToolkit + "/Activate All Panel Event Handlers", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_PanelEventHandlers() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<UnityEngine.UIElements.PanelEventHandler>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_UIToolkit + "/Activate All Panel Raycasters", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_PanelRaycasters() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<UnityEngine.UIElements.PanelRaycaster>(true);
        #endregion

        #region Cameras
        [MenuItem(HierarchyDesigner_Shared_Constants.Tools_Activate + "/Activate All Cameras", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Cameras() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Camera>(true);
        #endregion

        #region UI
        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_UI + "/Activate All Images", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Images() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Image>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_UI + "/Activate All Texts - TextMeshPro", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_TextMeshPro() => HierarchyDesigner_Shared_Operations.Activate_AllTMPComponentIfAvailable<TMPro.TMP_Text>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_UI + "/Activate All Raw Images", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_RawImages() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<RawImage>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_UI + "/Activate All Toggles", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Toggles() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Toggle>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_UI + "/Activate All Sliders", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 5)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Sliders() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Slider>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_UI + "/Activate All Scrollbars", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 6)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Scrollbars() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Scrollbar>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_UI + "/Activate All Scroll Views", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 7)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_ScrollViews() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<ScrollRect>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_UI + "/Activate All Dropdowns - TextMeshPro", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 8)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_DropdownTextMeshPro() => HierarchyDesigner_Shared_Operations.Activate_AllTMPComponentIfAvailable<TMPro.TMP_Dropdown>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_UI + "/Activate All Input Fields - TextMeshPro", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 8)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_InputFieldTextMeshPro() => HierarchyDesigner_Shared_Operations.Activate_AllTMPComponentIfAvailable<TMPro.TMP_InputField>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_UI + "/Activate All Canvases", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 8)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Canvases() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Canvas>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_UI + "/Activate All Event Systems", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 8)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_EventSystems() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<UnityEngine.EventSystems.EventSystem>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_UI_Legacy + "/Activate All Texts", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 8)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Texts() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Text>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_UI_Legacy + "/Activate All Buttons", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Buttons() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Button>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_UI_Legacy + "/Activate All Dropdowns", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Dropdowns() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Dropdown>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_UI_Legacy + "/Activate All Input Fields", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_InputFields() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<InputField>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_UI + "/Activate All Masks", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Masks() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Mask>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_UI + "/Activate All Rect Masks 2D", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_RectMasks2D() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<RectMask2D>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_UI + "/Activate All Selectables", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Selectables() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Selectable>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_UI + "/Activate All Toggle Groups", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_ToggleGroups() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<ToggleGroup>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_UI_Effects + "/Activate All Outlines", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Outlines() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Outline>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_UI_Effects + "/Activate All Positions As UV1", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_PositionsAsUV1() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<PositionAsUV1>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Activate_UI_Effects + "/Activate All Shadows", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Activate)]
        public static void MenuItem_Activate_Shadows() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Shadow>(true);
        #endregion
        #endregion

        #region Deactivate
        #region General
        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_General + "/Deactivate Selected GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityEight)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_SelectedGameObjects() => HierarchyDesigner_Shared_Operations.Activate_SelectedGameObjects(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_General + "/Deactivate All GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_AllGameObjects() => HierarchyDesigner_Shared_Operations.Activate_AllGameObjects(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_General + "/Deactivate All Parent GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_ParentGameObjects() => HierarchyDesigner_Shared_Operations.Activate_AllParentGameObjects(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_General + "/Deactivate All Empty GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_EmptyGameObjects() => HierarchyDesigner_Shared_Operations.Activate_AllEmptyGameObjects(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_General + "/Deactivate All Locked GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_LockedGameObjects() => HierarchyDesigner_Shared_Operations.Activate_AllLockedGameObjects(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_General + "/Deactivate All Folders", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_Folders() => HierarchyDesigner_Shared_Operations.Activate_AllFolders(false);
        #endregion

        #region 2D Objects
        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_2D + "/Deactivate All Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_AllSprites() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<SpriteRenderer>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_2D + "/Deactivate All Sprite Masks", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_SpriteMasks() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<SpriteMask>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_2D_Sprites + "/Deactivate All 9-Sliced Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_9SlicedSprites() => HierarchyDesigner_Shared_Operations.Activate_All2DSpritesByType("9-Sliced", false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_2D_Sprites + "/Deactivate All Capsule Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_CapsuleSprites() => HierarchyDesigner_Shared_Operations.Activate_All2DSpritesByType("Capsule", false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_2D_Sprites + "/Deactivate All Circle Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_CircleSprites() => HierarchyDesigner_Shared_Operations.Activate_All2DSpritesByType("Circle", false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_2D_Sprites + "/Deactivate All Hexagon Flat-Top Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_HexagonFlatTopSprites() => HierarchyDesigner_Shared_Operations.Activate_All2DSpritesByType("Hexagon Flat-Top", false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_2D_Sprites + "/Deactivate All Hexagon Pointed-Top Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_HexagonPointedTopSprites() => HierarchyDesigner_Shared_Operations.Activate_All2DSpritesByType("Hexagon Pointed-Top", false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_2D_Sprites + "/Deactivate All Isometric Diamond Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_IsometricDiamondSprites() => HierarchyDesigner_Shared_Operations.Activate_All2DSpritesByType("Isometric Diamond", false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_2D_Sprites + "/Deactivate All Square Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_SquareSprites() => HierarchyDesigner_Shared_Operations.Activate_All2DSpritesByType("Square", false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_2D_Sprites + "/Deactivate All Triangle Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_TriangleSprites() => HierarchyDesigner_Shared_Operations.Activate_All2DSpritesByType("Triangle", false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_2D_Physics + "/Deactivate All Dynamic Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_PhysicsDynamicSprites() => HierarchyDesigner_Shared_Operations.Activate_AllPhysicsDynamicSprites(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_2D_Physics + "/Deactivate All Static Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_PhysicsStaticSprites() => HierarchyDesigner_Shared_Operations.Activate_AllPhysicsStaticSprites(false);
        #endregion

        #region 3D Objects
        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_3D + "/Deactivate All Mesh Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_MeshFilters() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<MeshFilter>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_3D + "/Deactivate All Mesh Renderers", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_MeshRenderers() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<MeshRenderer>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_3D + "/Deactivate All Skinned Mesh Renderer", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_SkinnedMeshRenderers() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<SkinnedMeshRenderer>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_3D + "/Deactivate All Cubes", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_CubesObjects() => HierarchyDesigner_Shared_Operations.Activate_All3DObjectsByType("Cube", false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_3D + "/Deactivate All Spheres", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_SpheresObjects() => HierarchyDesigner_Shared_Operations.Activate_All3DObjectsByType("Sphere", false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_3D + "/Deactivate All Capsules", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_CapsulesObjects() => HierarchyDesigner_Shared_Operations.Activate_All3DObjectsByType("Capsule", false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_3D + "/Deactivate All Cylinders", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_CylindersObjects() => HierarchyDesigner_Shared_Operations.Activate_All3DObjectsByType("Cylinder", false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_3D + "/Deactivate All Planes", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_PlanesObjects() => HierarchyDesigner_Shared_Operations.Activate_All3DObjectsByType("Plane", false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_3D + "/Deactivate All Quads", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_QuadsObjects() => HierarchyDesigner_Shared_Operations.Activate_All3DObjectsByType("Quad", false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_3D + "/Deactivate All Texts - TextMeshPro", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_TextMeshProObjects() => HierarchyDesigner_Shared_Operations.Activate_All3DObjectsByType("TextMeshPro Mesh", false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_3D_Legacy + "/Deactivate All Text Meshes", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_TextMeshesObjects() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<TextMesh>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_3D + "/Deactivate All Terrains", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_TerrainsObjects() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Terrain>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_3D + "/Deactivate All Trees", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_TreesObjects() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Tree>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_3D + "/Deactivate All Wind Zones", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_WindZonesObjects() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<WindZone>(false);
        #endregion

        #region Audio
        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_Audio + "/Deactivate All Audio Sources", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_AudioSources() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<AudioSource>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_Audio + "/Deactivate All Audio Reverb Zones", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_AudioReverbZones() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<AudioReverbZone>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_Audio + "/Deactivate All Audio Chorus Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_AudioChorusFilters() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<AudioChorusFilter>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_Audio + "/Deactivate All Audio Distortion Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_AudioDistortionFilters() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<AudioDistortionFilter>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_Audio + "/Deactivate All Audio Echo Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_AudioEchoFilters() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<AudioEchoFilter>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_Audio + "/Deactivate All Audio High Pass Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_AudioHighPassFilters() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<AudioHighPassFilter>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_Audio + "/Deactivate All Audio Listeners", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_AudioListeners() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<AudioListener>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_Audio + "/Deactivate All Audio Low Pass Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_AudioLowPassFilters() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<AudioLowPassFilter>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_Audio + "/Deactivate All Audio Reverb Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_AudioReverbFilters() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<AudioReverbFilter>(false);
        #endregion

        #region Effects
        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_Effects + "/Deactivate All Particle Systems", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_ParticleSystems() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<ParticleSystem>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_Effects + "/Deactivate All Particle System Force Fields", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_ParticleSystemForceFields() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<ParticleSystemForceField>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_Effects + "/Deactivate All Trail Renderers", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_TrailRenderers() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<TrailRenderer>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_Effects + "/Deactivate All Line Renderers", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_LineRenderers() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<LineRenderer>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_Effects + "/Deactivate All Halos", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_Halos() => HierarchyDesigner_Shared_Operations.Activate_AllHalos(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_Effects + "/Deactivate All Lens Flares", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_LensFlares() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<LensFlare>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_Effects + "/Deactivate All Projectors", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_Projectors() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Projector>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_Effects + "/Deactivate All Visual Effects", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_VisualEffects() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<UnityEngine.VFX.VisualEffect>(false);
        #endregion

        #region Lights
        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_Light + "/Deactivate All Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_Lights() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Light>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_Light + "/Deactivate All Directional Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_DirectionalLights() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Light>(false, light => light.type == LightType.Directional);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_Light + "/Deactivate All Point Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_PointLights() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Light>(false, light => light.type == LightType.Point);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_Light + "/Deactivate All Spot Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_SpotLights() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Light>(false, light => light.type == LightType.Spot);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_Light + "/Deactivate All Rectangle Area Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_RectangleAreaLights() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Light>(false, light => light.type == LightType.Rectangle);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_Light + "/Deactivate All Disc Area Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_DiscAreaLights() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Light>(false, light => light.type == LightType.Disc);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_Light + "/Deactivate All Reflection Probes", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_ReflectionProbes() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<ReflectionProbe>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_Light + "/Deactivate All Light Probe Groups", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_LightProbeGroups() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<LightProbeGroup>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_Light + "/Deactivate All Light Probe Proxy Volumes", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_LightProbeProxyVolumes() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<LightProbeProxyVolume>(false);
        #endregion

        #region Video
        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_Video + "/Deactivate All Video Players", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_VideoPlayers() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<UnityEngine.Video.VideoPlayer>(false);
        #endregion

        #region UI Toolkit
        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_UIToolkit + "/Deactivate All UI Documents", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_UIDocuments() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<UnityEngine.UIElements.UIDocument>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_UIToolkit + "/Deactivate All Panel Event Handlers", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_PanelEventHandlers() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<UnityEngine.UIElements.PanelEventHandler>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_UIToolkit + "/Deactivate All Panel Raycasters", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_PanelRaycasters() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<UnityEngine.UIElements.PanelRaycaster>(false);
        #endregion

        #region Cameras
        [MenuItem(HierarchyDesigner_Shared_Constants.Tools_Deactivate + "/Deactivate All Cameras", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_Cameras() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Camera>(false);
        #endregion

        #region UI
        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_UI + "/Deactivate All Images", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_Images() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Image>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_UI + "/Deactivate All Texts - TextMeshPro", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_TextMeshPro() => HierarchyDesigner_Shared_Operations.Activate_AllTMPComponentIfAvailable<TMPro.TMP_Text>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_UI + "/Deactivate All Raw Images", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_RawImages() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<RawImage>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_UI + "/Deactivate All Toggles", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_Toggles() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Toggle>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_UI + "/Deactivate All Sliders", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 5)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_Sliders() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Slider>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_UI + "/Deactivate All Scrollbars", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 6)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_Scrollbars() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Scrollbar>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_UI + "/Deactivate All Scroll Views", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 7)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_ScrollViews() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<ScrollRect>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_UI + "/Deactivate All Dropdowns - TextMeshPro", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 8)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_DropdownTextMeshPro() => HierarchyDesigner_Shared_Operations.Activate_AllTMPComponentIfAvailable<TMPro.TMP_Dropdown>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_UI + "/Deactivate All Input Fields - TextMeshPro", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 8)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_InputFieldTextMeshPro() => HierarchyDesigner_Shared_Operations.Activate_AllTMPComponentIfAvailable<TMPro.TMP_InputField>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_UI + "/Deactivate All Canvases", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 8)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_Canvases() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Canvas>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_UI + "/Deactivate All Event Systems", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 8)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_EventSystems() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<UnityEngine.EventSystems.EventSystem>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_UI_Legacy + "/Deactivate All Texts", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 8)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_Texts() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Text>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_UI_Legacy + "/Deactivate All Buttons", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_Buttons() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Button>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_UI_Legacy + "/Deactivate All Dropdowns", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_Dropdowns() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Dropdown>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_UI_Legacy + "/Deactivate All Input Fields", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_InputFields() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<InputField>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_UI + "/Deactivate All Masks", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_Masks() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Mask>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_UI + "/Deactivate All Rect Masks 2D", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_RectMasks2D() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<RectMask2D>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_UI + "/Deactivate All Selectables", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_Selectables() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Selectable>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_UI + "/Deactivate All Toggle Groups", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_ToggleGroups() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<ToggleGroup>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_UI_Effects + "/Deactivate All Outlines", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_Outlines() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Outline>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_UI_Effects + "/Deactivate All Positions As UV1", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_PositionsAsUV1() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<PositionAsUV1>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Deactivate_UI_Effects + "/Deactivate All Shadows", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Deactivate)]
        public static void MenuItem_Deactivate_Shadows() => HierarchyDesigner_Shared_Operations.Activate_AllComponentOfType<Shadow>(false);
        #endregion
        #endregion

        #region Count
        #region General
        [MenuItem(HierarchyDesigner_Shared_Constants.Count_General + "/Count Selected GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_SelectedGameObjects() => HierarchyDesigner_Shared_Operations.Count_SelectedGameObjects();

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_General + "/Count All GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_GameObjects() => HierarchyDesigner_Shared_Operations.Count_AllGameObjects();

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_General + "/Count All Parent GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_ParentGameObjects() => HierarchyDesigner_Shared_Operations.Count_AllParentGameObjects();

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_General + "/Count All Empty GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_EmptyGameObjects() => HierarchyDesigner_Shared_Operations.Count_AllEmptyGameObjects();

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_General + "/Count All Locked GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_LockedGameObjects() => HierarchyDesigner_Shared_Operations.Count_AllLockedGameObjects();

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_General + "/Count All Active GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_ActiveGameObjects() => HierarchyDesigner_Shared_Operations.Count_AllActiveGameObjects();

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_General + "/Count All Inactive GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_InactiveGameObjects() => HierarchyDesigner_Shared_Operations.Count_AllInactiveGameObjects();

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_General + "/Count All Folders", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Folders() => HierarchyDesigner_Shared_Operations.Count_AllFolders();

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_General + "/Count All Separators", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Separators() => HierarchyDesigner_Shared_Operations.Count_AllSeparators();
        #endregion

        #region 2D Objects
        [MenuItem(HierarchyDesigner_Shared_Constants.Count_2D + "/Count All Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Sprites() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<SpriteRenderer>("Sprites");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_2D + "/Count All Sprite Masks", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_SpriteMasks() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<SpriteMask>("Sprite Masks");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_2D_Sprites + "/Count All 9-Sliced Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_9SlicedSprites() => HierarchyDesigner_Shared_Operations.Count_All2DSpritesByType("9-Sliced");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_2D_Sprites + "/Count All Capsule Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_CapsuleSprites() => HierarchyDesigner_Shared_Operations.Count_All2DSpritesByType("Capsule");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_2D_Sprites + "/Count All Circle Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_CircleSprites() => HierarchyDesigner_Shared_Operations.Count_All2DSpritesByType("Circle");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_2D_Sprites + "/Count All Hexagon Flat-Top Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_HexagonFlatTopSprites() => HierarchyDesigner_Shared_Operations.Count_All2DSpritesByType("Hexagon Flat-Top");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_2D_Sprites + "/Count All Hexagon Pointed-Top Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_HexagonPointedTopSprites() => HierarchyDesigner_Shared_Operations.Count_All2DSpritesByType("Hexagon Pointed-Top");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_2D_Sprites + "/Count All Isometric Diamond Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_IsometricDiamondSprites() => HierarchyDesigner_Shared_Operations.Count_All2DSpritesByType("Isometric Diamond");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_2D_Sprites + "/Count All Square Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_SquareSprites() => HierarchyDesigner_Shared_Operations.Count_All2DSpritesByType("Square");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_2D_Sprites + "/Count All Triangle Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_TriangleSprites() => HierarchyDesigner_Shared_Operations.Count_All2DSpritesByType("Triangle");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_2D_Physics + "/Count All Dynamic Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_PhysicsDynamicSprites() => HierarchyDesigner_Shared_Operations.Count_AllPhysicsDynamicSprites();

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_2D_Physics + "/Count All Static Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_PhysicsStaticSprites() => HierarchyDesigner_Shared_Operations.Count_AllPhysicsStaticSprites();
        #endregion

        #region 3D Objects
        [MenuItem(HierarchyDesigner_Shared_Constants.Count_3D + "/Count All Mesh Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_MeshFilters() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<MeshFilter>("Mesh Filters");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_3D + "/Count All Mesh Renderers", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_MeshRenderers() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<MeshRenderer>("Mesh Renderers");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_3D + "/Count All Skinned Mesh Renderer", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_SkinnedMeshRenderers() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<SkinnedMeshRenderer>("Skinned Mesh Renderers");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_3D + "/Count All Cubes", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_CubesObjects() => HierarchyDesigner_Shared_Operations.Count_All3DObjectsByType("Cube");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_3D + "/Count All Spheres", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_SpheresObjects() => HierarchyDesigner_Shared_Operations.Count_All3DObjectsByType("Sphere");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_3D + "/Count All Capsules", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_CapsulesObjects() => HierarchyDesigner_Shared_Operations.Count_All3DObjectsByType("Capsule");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_3D + "/Count All Cylinders", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_CylindersObjects() => HierarchyDesigner_Shared_Operations.Count_All3DObjectsByType("Cylinder");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_3D + "/Count All Planes", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_PlanesObjects() => HierarchyDesigner_Shared_Operations.Count_All3DObjectsByType("Plane");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_3D + "/Count All Quads", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_QuadsObjects() => HierarchyDesigner_Shared_Operations.Count_All3DObjectsByType("Quad");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_3D + "/Count All Texts - TextMeshPro", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_TextMeshProObjects() => HierarchyDesigner_Shared_Operations.Count_All3DObjectsByType("TextMeshPro Mesh");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_3D_Legacy + "/Count All Text Meshes", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_TextMeshesObjects() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<TextMesh>("Text Meshes");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_3D + "/Count All Terrains", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_TerrainsObjects() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<Terrain>("Terrains");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_3D + "/Count All Trees", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_TreesObjects() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<Tree>("Trees");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_3D + "/Count All Wind Zones", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_WindZonesObjects() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<WindZone>("Wind Zones");
        #endregion

        #region Audio
        [MenuItem(HierarchyDesigner_Shared_Constants.Count_Audio + "/Count All Audio Sources", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_CountAllAudioSources() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<AudioSource>("Audio Sources");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_Audio + "/Count All Audio Reverb Zones", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_AudioReverbZones() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<AudioReverbZone>("Audio Reverb Zones");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_Audio + "/Count All Audio Chorus Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_AudioChorusFilters() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<AudioChorusFilter>("Audio Chorus Filters");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_Audio + "/Count All Audio Distortion Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_AudioDistortionFilters() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<AudioDistortionFilter>("Audio Distortion Filters");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_Audio + "/Count All Audio Echo Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_AudioEchoFilters() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<AudioEchoFilter>("Audio Echo Filters");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_Audio + "/Count All Audio High Pass Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_AudioHighPassFilters() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<AudioHighPassFilter>("Audio High Pass Filters");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_Audio + "/Count All Audio Listeners", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_AudioListeners() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<AudioListener>("Audio Listeners");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_Audio + "/Count All Audio Low Pass Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_AudioLowPassFilters() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<AudioLowPassFilter>("Audio Low Pass Filters");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_Audio + "/Count All Audio Reverb Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_AudioReverbFilters() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<AudioReverbFilter>("Audio Reverb Filters");
        #endregion

        #region Effects
        [MenuItem(HierarchyDesigner_Shared_Constants.Count_Effects + "/Count All Particle Systems", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_ParticleSystems() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<ParticleSystem>("Particle Systems");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_Effects + "/Count All Particle System Force Fields", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_ParticleSystemForceFields() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<ParticleSystemForceField>("Particle System Force Fields");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_Effects + "/Count All Trail Renderers", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_TrailRenderers() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<TrailRenderer>("Trail Renderers");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_Effects + "/Count All Line Renderers", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_LineRenderers() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<LineRenderer>("Line Renderers");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_Effects + "/Count All Halos", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Halos() => HierarchyDesigner_Shared_Operations.Count_AllHalos();

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_Effects + "/Count All Lens Flares", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_LensFlares() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<LensFlare>("Lens Flares");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_Effects + "/Count All Projectors", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Projectors() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<Projector>("Projectors");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_Effects + "/Count All Visual Effects", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_VisualEffects() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<UnityEngine.VFX.VisualEffect>("Visual Effects");
        #endregion

        #region Lights
        [MenuItem(HierarchyDesigner_Shared_Constants.Count_Light + "/Count All Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Lights() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<Light>("Lights");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_Light + "/Count All Directional Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_DirectionalLights() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<Light>("Directional Lights", light => light.type == LightType.Directional);

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_Light + "/Count All Point Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_PointLights() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<Light>("Point Lights", light => light.type == LightType.Point);

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_Light + "/Count All Spot Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_SpotLights() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<Light>("Spot Lights", light => light.type == LightType.Spot);

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_Light + "/Count All Rectangle Area Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_RectangleAreaLights() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<Light>("Rectangle Area Lights", light => light.type == LightType.Rectangle);

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_Light + "/Count All Disc Area Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_DiscAreaLights() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<Light>("Disc Area Lights", light => light.type == LightType.Disc);

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_Light + "/Count All Reflection Probes", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_ReflectionProbes() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<ReflectionProbe>("Reflection Probes");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_Light + "/Count All Light Probe Groups", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_LightProbeGroups() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<LightProbeGroup>("Light Probe Groups");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_Light + "/Count All Light Probe Proxy Volumes", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_LightProbeProxyVolumes() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<LightProbeProxyVolume>("Light Probe Proxy Volumes");
        #endregion

        #region Video
        [MenuItem(HierarchyDesigner_Shared_Constants.Count_Video + "/Count All Video Players", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_VideoPlayers() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<UnityEngine.Video.VideoPlayer>("Video Players");
        #endregion

        #region UI Toolkit
        [MenuItem(HierarchyDesigner_Shared_Constants.Count_UIToolkit + "/Count All UI Documents", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_UIDocuments() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<UnityEngine.UIElements.UIDocument>("UI Documents");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_UIToolkit + "/Count All Panel Event Handlers", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_PanelEventHandlers() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<UnityEngine.UIElements.PanelEventHandler>("Panel Event Handlers");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_UIToolkit + "/Count All Panel Raycasters", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_PanelRaycasters() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<UnityEngine.UIElements.PanelRaycaster>("Panel Raycasters");
        #endregion

        #region Cameras
        [MenuItem(HierarchyDesigner_Shared_Constants.Tools_Count + "/Count All Cameras", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Cameras() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<Camera>("Cameras");
        #endregion

        #region UI
        [MenuItem(HierarchyDesigner_Shared_Constants.Count_UI + "/Count All Images", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Images() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<Image>("Images");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_UI + "/Count All Texts - TextMeshPro", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_TextMeshPro() => HierarchyDesigner_Shared_Operations.Count_AllTMPComponentIfAvailable<TMPro.TMP_Text>("Text - TextMeshPro");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_UI + "/Count All Raw Images", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_RawImages() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<RawImage>("Raw Images");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_UI + "/Count All Toggles", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Toggles() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<Toggle>("Toggles");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_UI + "/Count All Sliders", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 5)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Sliders() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<Slider>("Sliders");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_UI + "/Count All Scrollbars", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 6)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Scrollbars() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<Scrollbar>("Scrollbars");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_UI + "/Count All Scroll Views", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 7)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_ScrollViews() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<ScrollRect>("Scroll Views");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_UI + "/Count All Dropdowns - TextMeshPro", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 8)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_DropdownTextMeshPro() => HierarchyDesigner_Shared_Operations.Count_AllTMPComponentIfAvailable<TMPro.TMP_Dropdown>("Dropdowns - TextMeshPro");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_UI + "/Count All Input Fields - TextMeshPro", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 8)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_InputFieldTextMeshPro() => HierarchyDesigner_Shared_Operations.Count_AllTMPComponentIfAvailable<TMPro.TMP_InputField>("Input Fields - TextMeshPro");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_UI + "/Count All Canvases", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 8)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Canvases() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<Canvas>("Canvases");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_UI + "/Count All Event Systems", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 8)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_EventSystems() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<UnityEngine.EventSystems.EventSystem>("Event Systems");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_UI_Legacy + "/Count All Texts", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 8)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Texts() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<Text>("Texts");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_UI_Legacy + "/Count All Buttons", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Buttons() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<Button>("Buttons");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_UI_Legacy + "/Count All Dropdowns", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Dropdowns() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<Dropdown>("Dropdowns");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_UI_Legacy + "/Count All Input Fields", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_InputFields() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<InputField>("Input Fields");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_UI + "/Count All Masks", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Masks() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<Mask>("Masks");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_UI + "/Count All Rect Masks 2D", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_RectMasks2D() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<RectMask2D>("Rect Masks 2D");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_UI + "/Count All Selectables", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Selectables() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<Selectable>("Selectables");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_UI + "/Count All Toggle Groups", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_ToggleGroups() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<ToggleGroup>("Toggle Groups");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_UI_Effects + "/Count All Outlines", false, HierarchyDesigner_Shared_Constants.MenuPriorityFifteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_lOutlines() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<Outline>("Outlines");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_UI_Effects + "/Count All Positions As UV1", false, HierarchyDesigner_Shared_Constants.MenuPriorityFifteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_PositionsAsUV1() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<PositionAsUV1>("Positions As UV1");

        [MenuItem(HierarchyDesigner_Shared_Constants.Count_UI_Effects + "/Count All Shadows", false, HierarchyDesigner_Shared_Constants.MenuPriorityFifteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Count)]
        public static void MenuItem_Count_Shadows() => HierarchyDesigner_Shared_Operations.Count_AllComponentOfType<Shadow>("Shadows");
        #endregion
        #endregion

        #region Lock
        #region General
        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_General + "/Lock Selected GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_SelectedGameObjects() => HierarchyDesigner_Shared_Operations.Lock_SelectedGameObjects(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_General + "/Lock All GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_GameObjects() => HierarchyDesigner_Shared_Operations.Lock_AllGameObjects(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_General + "/Lock All Parent GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_ParentGameObjects() => HierarchyDesigner_Shared_Operations.Lock_AllParentGameObjects(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_General + "/Lock All Empty GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_EmptyGameObjects() => HierarchyDesigner_Shared_Operations.Lock_AllEmptyGameObjects(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_General + "/Lock All Active GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_ActiveGameObjects() => HierarchyDesigner_Shared_Operations.Lock_AllActiveGameObjects(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_General + "/Lock All Inactive GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_InactiveGameObjects() => HierarchyDesigner_Shared_Operations.Lock_AllInactiveGameObjects(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_General + "/Lock All Folders", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Folders() => HierarchyDesigner_Shared_Operations.Lock_AllFolders(true);
        #endregion

        #region 2D Objects
        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_2D + "/Lock All Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Sprites() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<SpriteRenderer>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_2D + "/Lock All Sprite Masks", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_SpriteMasks() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<SpriteMask>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_2D_Sprites + "/Lock All 9-Sliced Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_9SlicedSprites() => HierarchyDesigner_Shared_Operations.Lock_All2DSpritesByType("9-Sliced", true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_2D_Sprites + "/Lock All Capsule Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_CapsuleSprites() => HierarchyDesigner_Shared_Operations.Lock_All2DSpritesByType("Capsule", true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_2D_Sprites + "/Lock All Circle Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_CircleSprites() => HierarchyDesigner_Shared_Operations.Lock_All2DSpritesByType("Circle", true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_2D_Sprites + "/Lock All Hexagon Flat-Top Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_HexagonFlatTopSprites() => HierarchyDesigner_Shared_Operations.Lock_All2DSpritesByType("Hexagon Flat-Top", true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_2D_Sprites + "/Lock All Hexagon Pointed-Top Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_HexagonPointedTopSprites() => HierarchyDesigner_Shared_Operations.Lock_All2DSpritesByType("Hexagon Pointed-Top", true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_2D_Sprites + "/Lock All Isometric Diamond Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_IsometricDiamondSprites() => HierarchyDesigner_Shared_Operations.Lock_All2DSpritesByType("Isometric Diamond", true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_2D_Sprites + "/Lock All Square Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_SquareSprites() => HierarchyDesigner_Shared_Operations.Lock_All2DSpritesByType("Square", true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_2D_Sprites + "/Lock All Triangle Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_TriangleSprites() => HierarchyDesigner_Shared_Operations.Lock_All2DSpritesByType("Triangle", true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_2D_Physics + "/Lock All Dynamic Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_PhysicsDynamicSprites() => HierarchyDesigner_Shared_Operations.Lock_AllPhysicsDynamicSprites(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_2D_Physics + "/Lock All Static Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_PhysicsStaticSprites() => HierarchyDesigner_Shared_Operations.Lock_AllPhysicsStaticSprites(true);
        #endregion

        #region 3D Objects
        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_3D + "/Lock All Mesh Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_MeshFilters() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<MeshFilter>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_3D + "/Lock All Mesh Renderers", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_MeshRenderers() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<MeshRenderer>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_3D + "/Lock All Skinned Mesh Renderer", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_SkinnedMeshRenderers() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<SkinnedMeshRenderer>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_3D + "/Lock All Cubes", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_CubesObjects() => HierarchyDesigner_Shared_Operations.Lock_All3DObjectsByType("Cube", true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_3D + "/Lock All Spheres", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_SpheresObjects() => HierarchyDesigner_Shared_Operations.Lock_All3DObjectsByType("Sphere", true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_3D + "/Lock All Capsules", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_CapsulesObjects() => HierarchyDesigner_Shared_Operations.Lock_All3DObjectsByType("Capsule", true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_3D + "/Lock All Cylinders", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_CylindersObjects() => HierarchyDesigner_Shared_Operations.Lock_All3DObjectsByType("Cylinder", true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_3D + "/Lock All Planes", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_PlanesObjects() => HierarchyDesigner_Shared_Operations.Lock_All3DObjectsByType("Plane", true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_3D + "/Lock All Quads", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_QuadsObjects() => HierarchyDesigner_Shared_Operations.Lock_All3DObjectsByType("Quad", true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_3D + "/Lock All Texts - TextMeshPro", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_TextMeshProObjects() => HierarchyDesigner_Shared_Operations.Lock_All3DObjectsByType("TextMeshPro Mesh", true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_3D_Legacy + "/Lock All Text Meshes", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_TextMeshesObjects() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<TextMesh>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_3D + "/Lock All Terrains", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_TerrainsObjects() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Terrain>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_3D + "/Lock All Trees", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_TreesObjects() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Tree>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_3D + "/Lock All Wind Zones", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_WindZonesObjects() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<WindZone>(true);
        #endregion

        #region Audio
        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_Audio + "/Lock All Audio Sources", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_AudioSources() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<AudioSource>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_Audio + "/Lock All Audio Reverb Zones", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_AudioReverbZones() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<AudioReverbZone>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_Audio + "/Lock All Audio Chorus Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_AudioChorusFilters() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<AudioChorusFilter>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_Audio + "/Lock All Audio Distortion Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_AudioDistortionFilters() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<AudioDistortionFilter>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_Audio + "/Lock All Audio Echo Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_AudioEchoFilters() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<AudioEchoFilter>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_Audio + "/Lock All Audio High Pass Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_AudioHighPassFilters() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<AudioHighPassFilter>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_Audio + "/Lock All Audio Listeners", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_AudioListeners() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<AudioListener>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_Audio + "/Lock All Audio Low Pass Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_AudioLowPassFilters() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<AudioLowPassFilter>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_Audio + "/Lock All Audio Reverb Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_AudioReverbFilters() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<AudioReverbFilter>(true);
        #endregion

        #region Effects
        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_Effects + "/Lock All Particle Systems", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_ParticleSystems() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<ParticleSystem>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_Effects + "/Lock All Particle System Force Fields", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_ParticleSystemForceFields() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<ParticleSystemForceField>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_Effects + "/Lock All Trail Renderers", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_TrailRenderers() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<TrailRenderer>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_Effects + "/Lock All Line Renderers", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_LineRenderers() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<LineRenderer>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_Effects + "/Lock All Halos", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Halos() => HierarchyDesigner_Shared_Operations.Lock_AllHalos(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_Effects + "/Lock All Lens Flares", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_LensFlares() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<LensFlare>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_Effects + "/Lock All Projectors", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Projectors() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Projector>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_Effects + "/Lock All Visual Effects", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_VisualEffects() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<UnityEngine.VFX.VisualEffect>(true);
        #endregion

        #region Lights
        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_Light + "/Lock All Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Lights() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Light>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_Light + "/Lock All Directional Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_DirectionalLights() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Light>(true, light => light.type == LightType.Directional);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_Light + "/Lock All Point Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_PointLights() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Light>(true, light => light.type == LightType.Point);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_Light + "/Lock All Spot Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_SpotLights() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Light>(true, light => light.type == LightType.Spot);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_Light + "/Lock All Rectangle Area Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_RectangleAreaLights() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Light>(true, light => light.type == LightType.Rectangle);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_Light + "/Lock All Disc Area Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_DiscAreaLights() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Light>(true, light => light.type == LightType.Disc);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_Light + "/Lock All Reflection Probes", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_ReflectionProbes() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<ReflectionProbe>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_Light + "/Lock All Light Probe Groups", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_LightProbeGroups() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<LightProbeGroup>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_Light + "/Lock All Light Probe Proxy Volumes", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_LightProbeProxyVolumes() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<LightProbeProxyVolume>(true);
        #endregion

        #region Video
        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_Video + "/Lock All Video Players", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_VideoPlayers() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<UnityEngine.Video.VideoPlayer>(true);
        #endregion

        #region UI Toolkit
        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_UIToolkit + "/Lock All UI Documents", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_UIDocuments() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<UnityEngine.UIElements.UIDocument>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_UIToolkit + "/Lock All Panel Event Handlers", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_PanelEventHandlers() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<UnityEngine.UIElements.PanelEventHandler>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_UIToolkit + "/Lock All Panel Raycasters", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_PanelRaycasters() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<UnityEngine.UIElements.PanelRaycaster>(true);
        #endregion

        #region Cameras
        [MenuItem(HierarchyDesigner_Shared_Constants.Tools_Lock + "/Lock All Cameras", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Cameras() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Camera>(true);
        #endregion

        #region UI
        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_UI + "/Lock All Images", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Images() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Image>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_UI + "/Lock All Texts - TextMeshPro", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_TextMeshPro() => HierarchyDesigner_Shared_Operations.Lock_AllTMPComponentIfAvailable<TMPro.TMP_Text>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_UI + "/Lock All Raw Images", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_RawImages() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<RawImage>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_UI + "/Lock All Toggles", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Toggles() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Toggle>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_UI + "/Lock All Sliders", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 5)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Sliders() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Slider>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_UI + "/Lock All Scrollbars", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 6)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Scrollbars() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Scrollbar>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_UI + "/Lock All Scroll Views", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 7)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_ScrollViews() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<ScrollRect>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_UI + "/Lock All Dropdowns - TextMeshPro", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 8)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_DropdownTextMeshPro() => HierarchyDesigner_Shared_Operations.Lock_AllTMPComponentIfAvailable<TMPro.TMP_Dropdown>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_UI + "/Lock All Input Fields - TextMeshPro", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 8)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_InputFieldTextMeshPro() => HierarchyDesigner_Shared_Operations.Lock_AllTMPComponentIfAvailable<TMPro.TMP_InputField>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_UI + "/Lock All Canvases", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 8)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Canvases() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Canvas>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_UI + "/Lock All Event Systems", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 8)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_EventSystems() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<UnityEngine.EventSystems.EventSystem>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_UI_Legacy + "/Lock All Texts", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 8)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Texts() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Text>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_UI_Legacy + "/Lock All Buttons", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Buttons() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Button>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_UI_Legacy + "/Lock All Dropdowns", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Dropdowns() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Dropdown>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_UI_Legacy + "/Lock All Input Fields", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_InputFields() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<InputField>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_UI + "/Lock All Masks", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Masks() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Mask>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_UI + "/Lock All Rect Masks 2D", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_RectMasks2D() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<RectMask2D>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_UI + "/Lock All Selectables", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Selectables() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Selectable>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_UI + "/Lock All Toggle Groups", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_ToggleGroups() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<ToggleGroup>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_UI_Effects + "/Lock All Outlines", false, HierarchyDesigner_Shared_Constants.MenuPriorityFifteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Outlines() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Outline>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_UI_Effects + "/Lock All Positions As UV1", false, HierarchyDesigner_Shared_Constants.MenuPriorityFifteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_PositionsAsUV1() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<PositionAsUV1>(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Lock_UI_Effects + "/Lock All Shadows", false, HierarchyDesigner_Shared_Constants.MenuPriorityFifteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Lock)]
        public static void MenuItem_Lock_Shadows() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Shadow>(true);
        #endregion
        #endregion

        #region Unlock
        #region General
        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_General + "/Unlock Selected GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityNine)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_SelectedGameObjects() => HierarchyDesigner_Shared_Operations.Lock_SelectedGameObjects(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_General + "/Unlock All GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_GameObjects() => HierarchyDesigner_Shared_Operations.Lock_AllGameObjects(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_General + "/Unlock All Parent GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_ParentGameObjects() => HierarchyDesigner_Shared_Operations.Lock_AllParentGameObjects(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_General + "/Unlock All Empty GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_EmptyGameObjects() => HierarchyDesigner_Shared_Operations.Lock_AllEmptyGameObjects(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_General + "/Unlock All Active GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_ActiveGameObjects() => HierarchyDesigner_Shared_Operations.Lock_AllActiveGameObjects(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_General + "/Unlock All Inactive GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_InactiveGameObjects() => HierarchyDesigner_Shared_Operations.Lock_AllInactiveGameObjects(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_General + "/Unlock All Folders", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_Folders() => HierarchyDesigner_Shared_Operations.Lock_AllFolders(false);
        #endregion

        #region 2D Objects
        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_2D + "/Unlock All Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_UnlockAllSprites() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<SpriteRenderer>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_2D + "/Unlock All Sprite Masks", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_UnlockAllSpriteMasks() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<SpriteMask>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_2D_Sprites + "/Unlock All 9-Sliced Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_UnlockAll9SlicedSprites() => HierarchyDesigner_Shared_Operations.Lock_All2DSpritesByType("9-Sliced", false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_2D_Sprites + "/Unlock All Capsule Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_UnlockAllCapsuleSprites() => HierarchyDesigner_Shared_Operations.Lock_All2DSpritesByType("Capsule", false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_2D_Sprites + "/Unlock All Circle Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_UnlockAllCircleSprites() => HierarchyDesigner_Shared_Operations.Lock_All2DSpritesByType("Circle", false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_2D_Sprites + "/Unlock All Hexagon Flat-Top Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_UnlockAllHexagonFlatTopSprites() => HierarchyDesigner_Shared_Operations.Lock_All2DSpritesByType("Hexagon Flat-Top", false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_2D_Sprites + "/Unlock All Hexagon Pointed-Top Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_UnlockAllHexagonPointedTopSprites() => HierarchyDesigner_Shared_Operations.Lock_All2DSpritesByType("Hexagon Pointed-Top", false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_2D_Sprites + "/Unlock All Isometric Diamond Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_UnlockAllIsometricDiamondSprites() => HierarchyDesigner_Shared_Operations.Lock_All2DSpritesByType("Isometric Diamond", false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_2D_Sprites + "/Unlock All Square Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_UnlockAllSquareSprites() => HierarchyDesigner_Shared_Operations.Lock_All2DSpritesByType("Square", false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_2D_Sprites + "/Unlock All Triangle Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_UnlockAllTriangleSprites() => HierarchyDesigner_Shared_Operations.Lock_All2DSpritesByType("Triangle", false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_2D_Physics + "/Unlock All Dynamic Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_UnlockAllPhysicsDynamicSprites() => HierarchyDesigner_Shared_Operations.Lock_AllPhysicsDynamicSprites(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_2D_Physics + "/Unlock All Static Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_UnlockAllPhysicsStaticSprites() => HierarchyDesigner_Shared_Operations.Lock_AllPhysicsStaticSprites(false);
        #endregion

        #region 3D Objects
        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_3D + "/Unlock All Mesh Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_MeshFilters() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<MeshFilter>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_3D + "/Unlock All Mesh Renderers", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock__UnlockAllMeshRenderers() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<MeshRenderer>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_3D + "/Unlock All Skinned Mesh Renderer", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_SkinnedMeshRenderers() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<SkinnedMeshRenderer>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_3D + "/Unlock All Cubes", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_CubesObjects() => HierarchyDesigner_Shared_Operations.Lock_All3DObjectsByType("Cube", false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_3D + "/Unlock All Spheres", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_SpheresObjects() => HierarchyDesigner_Shared_Operations.Lock_All3DObjectsByType("Sphere", false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_3D + "/Unlock All Capsules", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_CapsulesObjects() => HierarchyDesigner_Shared_Operations.Lock_All3DObjectsByType("Capsule", false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_3D + "/Unlock All Cylinders", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_CylindersObjects() => HierarchyDesigner_Shared_Operations.Lock_All3DObjectsByType("Cylinder", false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_3D + "/Unlock All Planes", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_PlanesObjects() => HierarchyDesigner_Shared_Operations.Lock_All3DObjectsByType("Plane", false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_3D + "/Unlock All Quads", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_QuadsObjects() => HierarchyDesigner_Shared_Operations.Lock_All3DObjectsByType("Quad", false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_3D + "/Unlock All Texts - TextMeshPro", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_TextMeshProObjects() => HierarchyDesigner_Shared_Operations.Lock_All3DObjectsByType("TextMeshPro Mesh", false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_3D_Legacy + "/Unlock All Text Meshes", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_TextMeshesObjects() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<TextMesh>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_3D + "/Unlock All Terrains", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_TerrainsObjects() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Terrain>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_3D + "/Unlock All Trees", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_TreesObjects() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Tree>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_3D + "/Unlock All Wind Zones", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_WindZonesObjects() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<WindZone>(false);
        #endregion

        #region Audio
        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_Audio + "/Unlock All Audio Sources", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_AudioSources() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<AudioSource>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_Audio + "/Unlock All Audio Reverb Zones", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_AudioReverbZones() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<AudioReverbZone>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_Audio + "/Unlock All Audio Chorus Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_AudioChorusFilters() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<AudioChorusFilter>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_Audio + "/Unlock All Audio Distortion Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_AudioDistortionFilters() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<AudioDistortionFilter>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_Audio + "/Unlock All Audio Echo Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_AudioEchoFilters() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<AudioEchoFilter>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_Audio + "/Unlock All Audio High Pass Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_AudioHighPassFilters() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<AudioHighPassFilter>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_Audio + "/Unlock All Audio Listeners", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_AudioListeners() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<AudioListener>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_Audio + "/Unlock All Audio Low Pass Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_AudioLowPassFilters() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<AudioLowPassFilter>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_Audio + "/Unlock All Audio Reverb Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_AudioReverbFilters() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<AudioReverbFilter>(false);
        #endregion

        #region Effects
        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_Effects + "/Unlock All Particle Systems", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_ParticleSystems() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<ParticleSystem>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_Effects + "/Unlock All Particle System Force Fields", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_ParticleSystemForceFields() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<ParticleSystemForceField>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_Effects + "/Unlock All Trail Renderers", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_TrailRenderers() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<TrailRenderer>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_Effects + "/Unlock All Line Renderers", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_LineRenderers() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<LineRenderer>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_Effects + "/Unlock All Halos", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_Halos() => HierarchyDesigner_Shared_Operations.Lock_AllHalos(true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_Effects + "/Unlock All Lens Flares", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_LensFlares() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<LensFlare>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_Effects + "/Unlock All Projectors", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_Projectors() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Projector>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_Effects + "/Unlock All Visual Effects", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_VisualEffects() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<UnityEngine.VFX.VisualEffect>(true);
        #endregion

        #region Lights
        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_Light + "/Unlock All Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_Lights() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Light>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_Light + "/Unlock All Directional Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_DirectionalLights() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Light>(false, light => light.type == LightType.Directional);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_Light + "/Unlock All Point Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_PointLights() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Light>(false, light => light.type == LightType.Point);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_Light + "/Unlock All Spot Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_SpotLights() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Light>(false, light => light.type == LightType.Spot);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_Light + "/Unlock All Rectangle Area Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_RectangleAreaLights() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Light>(false, light => light.type == LightType.Rectangle);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_Light + "/Unlock All Disc Area Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_DiscAreaLights() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Light>(false, light => light.type == LightType.Disc);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_Light + "/Unlock All Reflection Probes", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_ReflectionProbes() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<ReflectionProbe>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_Light + "/Unlock All Light Probe Groups", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_LightProbeGroups() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<LightProbeGroup>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_Light + "/Unlock All Light Probe Proxy Volumes", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_LightProbeProxyVolumes() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<LightProbeProxyVolume>(false);
        #endregion

        #region Video
        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_Video + "/Unlock All Video Players", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_VideoPlayers() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<UnityEngine.Video.VideoPlayer>(false);
        #endregion

        #region UI Toolkit
        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_UIToolkit + "/Unlock All UI Documents", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_UIDocuments() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<UnityEngine.UIElements.UIDocument>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_UIToolkit + "/Unlock All Panel Event Handlers", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_PanelEventHandlers() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<UnityEngine.UIElements.PanelEventHandler>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_UIToolkit + "/Unlock All Panel Raycasters", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_PanelRaycasters() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<UnityEngine.UIElements.PanelRaycaster>(false);
        #endregion

        #region Cameras
        [MenuItem(HierarchyDesigner_Shared_Constants.Tools_Unlock + "/Unlock All Cameras", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_AllCameras() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Camera>(false);
        #endregion

        #region UI
        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_UI + "/Unlock All Images", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_Images() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Image>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_UI + "/Unlock All Texts - TextMeshPro", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_TextMeshPro() => HierarchyDesigner_Shared_Operations.Lock_AllTMPComponentIfAvailable<TMPro.TMP_Text>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_UI + "/Unlock All Raw Images", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_RawImages() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<RawImage>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_UI + "/Unlock All Toggles", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_Toggles() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Toggle>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_UI + "/Unlock All Sliders", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 5)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_Sliders() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Slider>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_UI + "/Unlock All Scrollbars", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 6)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_Scrollbars() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Scrollbar>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_UI + "/Unlock All Scroll Views", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 7)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_ScrollViews() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<ScrollRect>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_UI + "/Unlock All Dropdowns - TextMeshPro", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 8)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_DropdownTextMeshPro() => HierarchyDesigner_Shared_Operations.Lock_AllTMPComponentIfAvailable<TMPro.TMP_Dropdown>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_UI + "/Unlock All Input Fields - TextMeshPro", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 8)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_InputFieldTextMeshPro() => HierarchyDesigner_Shared_Operations.Lock_AllTMPComponentIfAvailable<TMPro.TMP_InputField>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_UI + "/Unlock All Canvases", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 8)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_Canvases() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Canvas>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_UI + "/Unlock All Event Systems", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 8)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_EventSystems() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<UnityEngine.EventSystems.EventSystem>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_UI_Legacy + "/Unlock All Texts", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 8)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_Texts() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Text>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_UI_Legacy + "/Unlock All Buttons", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_Buttons() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Button>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_UI_Legacy + "/Unlock All Dropdowns", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_Dropdowns() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Dropdown>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_UI_Legacy + "/Unlock All Input Fields", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_InputFields() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<InputField>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_UI + "/Unlock All Masks", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_Masks() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Mask>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_UI + "/Unlock All Rect Masks 2D", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_RectMasks2D() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<RectMask2D>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_UI + "/Unlock All Selectables", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_Selectables() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Selectable>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_UI + "/Unlock All Toggle Groups", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_ToggleGroups() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<ToggleGroup>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_UI_Effects + "/Unlock All Outlines", false, HierarchyDesigner_Shared_Constants.MenuPriorityFifteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_Outlines() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Outline>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_UI_Effects + "/Unlock All Positions As UV1", false, HierarchyDesigner_Shared_Constants.MenuPriorityFifteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_PositionsAsUV1() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<PositionAsUV1>(false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Unlock_UI_Effects + "/Unlock All Shadows", false, HierarchyDesigner_Shared_Constants.MenuPriorityFifteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Unlock)]
        public static void MenuItem_Unlock_Shadows() => HierarchyDesigner_Shared_Operations.Lock_AllComponentOfType<Shadow>(false);
        #endregion
        #endregion

        #region Rename
        [MenuItem(HierarchyDesigner_Shared_Constants.Tools_Rename + "/Rename Selected GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Rename)]
        public static void MenuItem_Rename_SelectedGameObjectst() => HierarchyDesigner_Shared_Operations.Rename_SelectedGameObjects("rename", false);

        [MenuItem(HierarchyDesigner_Shared_Constants.Tools_Rename + "/Rename Selected GameObjects with Auto-Indexing", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Rename)]
        public static void MenuItem_Rename_SelectedGameObjectsWithAutoIndex() => HierarchyDesigner_Shared_Operations.Rename_SelectedGameObjects("rename with automatic indexing", true);

        [MenuItem(HierarchyDesigner_Shared_Constants.Tools_Rename + "/Open Rename Tool Window", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Rename)]
        public static void MenuItem_Rename_OpenWindow() => HierarchyDesigner_Shared_Operations.Rename_OpenRenameToolWindow();
        #endregion

        #region Select
        #region General
        [MenuItem(HierarchyDesigner_Shared_Constants.Select_General + "/Select All GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_GameObjects() => HierarchyDesigner_Shared_Operations.Select_AllGameObjects();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_General + "/Select All Parent GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_ParentGameObjects() => HierarchyDesigner_Shared_Operations.Select_AllParentGameObjects();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_General + "/Select All Empty GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_EmptyGameObjects() => HierarchyDesigner_Shared_Operations.Select_AllEmptyGameObjects();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_General + "/Select All Locked GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_LockedGameObjects() => HierarchyDesigner_Shared_Operations.Select_AllLockedGameObjects();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_General + "/Select All Active GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_ActiveGameObjects() => HierarchyDesigner_Shared_Operations.Select_AllActiveGameObjects();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_General + "/Select All Inactive GameObjects", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_InactiveGameObjects() => HierarchyDesigner_Shared_Operations.Select_AllInactiveGameObjects();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_General + "/Select All Folders", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Folders() => HierarchyDesigner_Shared_Operations.Select_AllFolders();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_General + "/Select All Separators", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Separators() => HierarchyDesigner_Shared_Operations.Select_AllSeparators();
        #endregion

        #region 2D Objects
        [MenuItem(HierarchyDesigner_Shared_Constants.Select_2D + "/Select All Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Sprites() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<SpriteRenderer>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_2D + "/Select All Sprite Masks", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_SpriteMasks() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<SpriteMask>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_2D_Sprites + "/Select All 9-Sliced Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_9SlicedSprites() => HierarchyDesigner_Shared_Operations.Select_All2DSpritesByType("9-Sliced");

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_2D_Sprites + "/Select All Capsule Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_CapsuleSprites() => HierarchyDesigner_Shared_Operations.Select_All2DSpritesByType("Capsule");

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_2D_Sprites + "/Select All Circle Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_CircleSprites() => HierarchyDesigner_Shared_Operations.Select_All2DSpritesByType("Circle");

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_2D_Sprites + "/Select All Hexagon Flat-Top Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_HexagonFlatTopSprites() => HierarchyDesigner_Shared_Operations.Select_All2DSpritesByType("Hexagon Flat-Top");

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_2D_Sprites + "/Select All Hexagon Pointed-Top Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_HexagonPointedTopSprites() => HierarchyDesigner_Shared_Operations.Select_All2DSpritesByType("Hexagon Pointed-Top");

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_2D_Sprites + "/Select All Isometric Diamond Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_IsometricDiamondSprites() => HierarchyDesigner_Shared_Operations.Select_All2DSpritesByType("Isometric Diamond");

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_2D_Sprites + "/Select All Square Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_SquareSprites() => HierarchyDesigner_Shared_Operations.Select_All2DSpritesByType("Square");

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_2D_Sprites + "/Select All Triangle Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_TriangleSprites() => HierarchyDesigner_Shared_Operations.Select_All2DSpritesByType("Triangle");

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_2D_Physics + "/Select All Dynamic Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_PhysicsDynamicSprites() => HierarchyDesigner_Shared_Operations.Select_AllPhysicsDynamicSprites();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_2D_Physics + "/Select All Static Sprites", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_PhysicsStaticSprites() => HierarchyDesigner_Shared_Operations.Select_AllPhysicsStaticSprites();
        #endregion

        #region 3D Objects
        [MenuItem(HierarchyDesigner_Shared_Constants.Select_3D + "/Select All Mesh Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_MeshFilters() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<MeshFilter>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_3D + "/Select All Mesh Renderers", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_MeshRenderers() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<MeshRenderer>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_3D + "/Select All Skinned Mesh Renderer", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_SkinnedMeshRenderers() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<SkinnedMeshRenderer>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_3D + "/Select All Cubes", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_CubesObjects() => HierarchyDesigner_Shared_Operations.Select_All3DObjectsByType("Cube");

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_3D + "/Select All Spheres", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 1)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_SpheresObjects() => HierarchyDesigner_Shared_Operations.Select_All3DObjectsByType("Sphere");

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_3D + "/Select All Capsules", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_CapsulesObjects() => HierarchyDesigner_Shared_Operations.Select_All3DObjectsByType("Capsule");

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_3D + "/Select All Cylinders", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_CylindersObjects() => HierarchyDesigner_Shared_Operations.Select_All3DObjectsByType("Cylinder");

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_3D + "/Select All Planes", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_PlanesObjects() => HierarchyDesigner_Shared_Operations.Select_All3DObjectsByType("Plane");

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_3D + "/Select All Quads", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_QuadsObjects() => HierarchyDesigner_Shared_Operations.Select_All3DObjectsByType("Quad");

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_3D + "/Select All Texts - TextMeshPro", false, HierarchyDesigner_Shared_Constants.MenuPriorityFifteen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_TextMeshProObjects() => HierarchyDesigner_Shared_Operations.Select_All3DObjectsByType("TextMeshPro Mesh");

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_3D_Legacy + "/Select All Text Meshes", false, HierarchyDesigner_Shared_Constants.MenuPriorityFifteen + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_TextMeshesObjects() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<TextMesh>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_3D + "/Select All Terrains", false, HierarchyDesigner_Shared_Constants.MenuPrioritySixteen + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_TerrainsObjects() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<Terrain>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_3D + "/Select All Trees", false, HierarchyDesigner_Shared_Constants.MenuPrioritySixteen + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_TreesObjects() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<Tree>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_3D + "/Select All Wind Zones", false, HierarchyDesigner_Shared_Constants.MenuPrioritySixteen + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_WindZonesObjects() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<WindZone>();
        #endregion

        #region Audio
        [MenuItem(HierarchyDesigner_Shared_Constants.Select_Audio + "/Select All Audio Sources", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_AudioSources() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<AudioSource>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_Audio + "/Select All Audio Reverb Zones", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_AudioReverbZones() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<AudioReverbZone>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_Audio + "/Select All Audio Chorus Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_AudioChorusFilters() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<AudioChorusFilter>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_Audio + "/Select All Audio Distortion Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_AudioDistortionFilters() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<AudioDistortionFilter>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_Audio + "/Select All Audio Echo Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_AudioEchoFilters() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<AudioEchoFilter>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_Audio + "/Select All Audio High Pass Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_AudioHighPassFilters() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<AudioHighPassFilter>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_Audio + "/Select All Audio Listeners", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_AudioListeners() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<AudioListener>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_Audio + "/Select All Audio Low Pass Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_AudioLowPassFilters() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<AudioLowPassFilter>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_Audio + "/Select All Audio Reverb Filters", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_AudioReverbFilters() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<AudioReverbFilter>();
        #endregion

        #region Effects
        [MenuItem(HierarchyDesigner_Shared_Constants.Select_Effects + "/Select All Particle Systems", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_ParticleSystems() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<ParticleSystem>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_Effects + "/Select All Particle System Force Fields", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_ParticleSystemForceFields() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<ParticleSystemForceField>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_Effects + "/Select All Trail Renderers", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_TrailRenderers() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<TrailRenderer>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_Effects + "/Select All Line Renderers", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_LineRenderers() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<LineRenderer>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_Effects + "/Select All Halos", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Halos() => HierarchyDesigner_Shared_Operations.Select_AllHalos();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_Effects + "/Select All Lens Flares", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_LensFlares() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<LensFlare>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_Effects + "/Select All Projectors", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_AllProjectors() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<Projector>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_Effects + "/Select All Visual Effects", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_VisualEffects() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<UnityEngine.VFX.VisualEffect>();
        #endregion

        #region Lights
        [MenuItem(HierarchyDesigner_Shared_Constants.Select_Light + "/Select All Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Lights() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<Light>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_Light + "/Select All Directional Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_DirectionalLights() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<Light>(light => light.type == LightType.Directional);

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_Light + "/Select All Point Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_PointLights() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<Light>(light => light.type == LightType.Point);

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_Light + "/Select All Spot Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_SpotLights() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<Light>(light => light.type == LightType.Spot);

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_Light + "/Select All Rectangle Area Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_RectangleAreaLights() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<Light>(light => light.type == LightType.Rectangle);

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_Light + "/Select All Disc Area Lights", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_DiscAreaLights() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<Light>(light => light.type == LightType.Disc);

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_Light + "/Select All Reflection Probes", false, HierarchyDesigner_Shared_Constants.MenuPriorityFifteen + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_ReflectionProbes() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<ReflectionProbe>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_Light + "/Select All Light Probe Groups", false, HierarchyDesigner_Shared_Constants.MenuPriorityFifteen + 5)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_LightProbeGroups() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<LightProbeGroup>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_Light + "/Select All Light Probe Proxy Volumes", false, HierarchyDesigner_Shared_Constants.MenuPriorityFifteen + 5)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_LightProbeProxyVolumes() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<LightProbeProxyVolume>();
        #endregion

        #region Video
        [MenuItem(HierarchyDesigner_Shared_Constants.Select_Video + "/Select All Video Players", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 2)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_VideoPlayers() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<UnityEngine.Video.VideoPlayer>();
        #endregion

        #region UI Toolkit
        [MenuItem(HierarchyDesigner_Shared_Constants.Select_UIToolkit + "/Select All UI Documents", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_UIDocuments() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<UnityEngine.UIElements.UIDocument>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_UIToolkit + "/Select All Panel Event Handlers", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_PanelEventHandlers() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<UnityEngine.UIElements.PanelEventHandler>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_UIToolkit + "/Select All Panel Raycasters", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 3)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_PanelRaycasters() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<UnityEngine.UIElements.PanelRaycaster>();
        #endregion

        #region Cameras
        [MenuItem(HierarchyDesigner_Shared_Constants.Tools_Select + "/Select All Cameras", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Cameras() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<Camera>();
        #endregion

        #region UI
        [MenuItem(HierarchyDesigner_Shared_Constants.Select_UI + "/Select All Images", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Images() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<Image>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_UI + "/Select All Texts - TextMeshPro", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 4)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_TextMeshPro() => HierarchyDesigner_Shared_Operations.Select_AllTMPComponentIfAvailable<TMPro.TMP_Text>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_UI + "/Select All Raw Images", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen + 5)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_RawImages() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<RawImage>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_UI + "/Select All Toggles", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 5)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Toggles() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<Toggle>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_UI + "/Select All Sliders", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 6)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Sliders() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<Slider>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_UI + "/Select All Scrollbars", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 7)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Scrollbars() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<Scrollbar>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_UI + "/Select All Scroll Views", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 8)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_ScrollViews() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<ScrollRect>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_UI + "/Select All Dropdowns - TextMeshPro", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_DropdownTextMeshPro() => HierarchyDesigner_Shared_Operations.Select_AllTMPComponentIfAvailable<TMPro.TMP_Dropdown>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_UI + "/Select All Input Fields - TextMeshPro", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_InputFieldTextMeshPro() => HierarchyDesigner_Shared_Operations.Select_AllTMPComponentIfAvailable<TMPro.TMP_InputField>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_UI + "/Select All Canvases", false, HierarchyDesigner_Shared_Constants.MenuPriorityFifteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Canvases() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<Canvas>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_UI + "/Select All Event Systems", false, HierarchyDesigner_Shared_Constants.MenuPriorityFifteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_EventSystems() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<UnityEngine.EventSystems.EventSystem>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_UI_Legacy + "/Select All Texts", false, HierarchyDesigner_Shared_Constants.MenuPrioritySixteen + 9)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Texts() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<Text>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_UI_Legacy + "/Select All Buttons", false, HierarchyDesigner_Shared_Constants.MenuPrioritySixteen + 10)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Buttons() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<Button>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_UI_Legacy + "/Select All Dropdowns", false, HierarchyDesigner_Shared_Constants.MenuPrioritySixteen + 10)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Dropdowns() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<Dropdown>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_UI_Legacy + "/Select All Input Fields", false, HierarchyDesigner_Shared_Constants.MenuPrioritySixteen + 10)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_InputFields() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<InputField>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_UI + "/Select All Masks", false, HierarchyDesigner_Shared_Constants.MenuPrioritySeventeen + 10)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Masks() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<Mask>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_UI + "/Select All Rect Masks 2D", false, HierarchyDesigner_Shared_Constants.MenuPrioritySeventeen + 10)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_RectMasks2D() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<RectMask2D>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_UI + "/Select All Selectables", false, HierarchyDesigner_Shared_Constants.MenuPrioritySeventeen + 10)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Selectables() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<Selectable>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_UI + "/Select All Toggle Groups", false, HierarchyDesigner_Shared_Constants.MenuPrioritySeventeen + 10)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_ToggleGroups() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<ToggleGroup>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_UI_Effects + "/Select All Outlines", false, HierarchyDesigner_Shared_Constants.MenuPriorityEigtheen + 10)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Outlines() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<Outline>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_UI_Effects + "/Select All Positions As UV1", false, HierarchyDesigner_Shared_Constants.MenuPriorityEigtheen + 10)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_PositionsAsUV1() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<PositionAsUV1>();

        [MenuItem(HierarchyDesigner_Shared_Constants.Select_UI_Effects + "/Select All Shadows", false, HierarchyDesigner_Shared_Constants.MenuPriorityEigtheen + 10)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Select)]
        public static void MenuItem_Select_Shadows() => HierarchyDesigner_Shared_Operations.Select_AllComponentOfType<Shadow>();
        #endregion
        #endregion

        #region Sort
        [MenuItem(HierarchyDesigner_Shared_Constants.Tools_Sort + "/Sort Alphabetically Ascending", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Sort)]
        public static void MenuItem_Sort_AlphabeticallyAscending() => HierarchyDesigner_Shared_Operations.Sort_GameObjectChildren(HierarchyDesigner_Shared_Operations.AlphanumericComparison, "sort its children alphabetically ascending");

        [MenuItem(HierarchyDesigner_Shared_Constants.Tools_Sort + "/Sort Alphabetically Descending", false, HierarchyDesigner_Shared_Constants.MenuPriorityTen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Sort)]
        public static void MenuItem_Sort_AlphabeticallyDescending() => HierarchyDesigner_Shared_Operations.Sort_GameObjectChildren((a, b) => -HierarchyDesigner_Shared_Operations.AlphanumericComparison(a, b), "sort its children alphabetically descending");

        [MenuItem(HierarchyDesigner_Shared_Constants.Tools_Sort + "/Sort Components Amount Ascending", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Sort)]
        public static void MenuItem_Sort_ComponentsAmountAscending() => HierarchyDesigner_Shared_Operations.Sort_GameObjectChildren((a, b) => a.GetComponents<Component>().Length.CompareTo(b.GetComponents<Component>().Length), "sort its children by components amount ascending");

        [MenuItem(HierarchyDesigner_Shared_Constants.Tools_Sort + "/Sort Components Amount Descending", false, HierarchyDesigner_Shared_Constants.MenuPriorityEleven)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Sort)]
        public static void MenuItem_Sort_ComponentsAmountDescending() => HierarchyDesigner_Shared_Operations.Sort_GameObjectChildren((a, b) => b.GetComponents<Component>().Length.CompareTo(a.GetComponents<Component>().Length), "sort its children by components amount descending");

        [MenuItem(HierarchyDesigner_Shared_Constants.Tools_Sort + "/Sort Length Ascending", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Sort)]
        public static void MenuItem_Sort_LengthAscending() => HierarchyDesigner_Shared_Operations.Sort_GameObjectChildren((a, b) => a.name.Length.CompareTo(b.name.Length), "sort its children by length ascending");

        [MenuItem(HierarchyDesigner_Shared_Constants.Tools_Sort + "/Sort Length Descending", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwelve)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Sort)]
        public static void MenuItem_Sort_LengthDescending() => HierarchyDesigner_Shared_Operations.Sort_GameObjectChildren((a, b) => b.name.Length.CompareTo(a.name.Length), "sort its children by length descending");

        [MenuItem(HierarchyDesigner_Shared_Constants.Tools_Sort + "/Sort Tag Alphabetically Ascending", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Sort)]
        public static void MenuItem_Sort_TagAlphabeticallyAscending() => HierarchyDesigner_Shared_Operations.Sort_GameObjectChildrenByTag(true, "sort its children by tag ascending");

        [MenuItem(HierarchyDesigner_Shared_Constants.Tools_Sort + "/Sort Tag Alphabetically Descending", false, HierarchyDesigner_Shared_Constants.MenuPriorityThirteen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Sort)]
        public static void MenuItem_Sort_TagAlphabeticallyDescending() => HierarchyDesigner_Shared_Operations.Sort_GameObjectChildrenByTag(false, "sort its children by tag descending");

        [MenuItem(HierarchyDesigner_Shared_Constants.Tools_Sort + "/Sort Tag List Order Ascending", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Sort)]
        public static void MenuItem_Sort_TagListOrderAscending() => HierarchyDesigner_Shared_Operations.Sort_GameObjectChildrenByTagListOrder(true, "sort its children by tag list order ascending");

        [MenuItem(HierarchyDesigner_Shared_Constants.Tools_Sort + "/Sort Tag List Order Descending", false, HierarchyDesigner_Shared_Constants.MenuPriorityFourteen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Sort)]
        public static void MenuItem_Sort_TagListOrderDescending() => HierarchyDesigner_Shared_Operations.Sort_GameObjectChildrenByTagListOrder(false, "sort its children by tag list order descending");

        [MenuItem(HierarchyDesigner_Shared_Constants.Tools_Sort + "/Sort Layer Alphabetically Ascending", false, HierarchyDesigner_Shared_Constants.MenuPriorityFifteen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Sort)]
        public static void MenuItem_Sort_LayerAlphabeticallyAscending() => HierarchyDesigner_Shared_Operations.Sort_GameObjectChildrenByLayer(true, "sort its children by layer alphabetically ascending");

        [MenuItem(HierarchyDesigner_Shared_Constants.Tools_Sort + "/Sort Layer Alphabetically Descending", false, HierarchyDesigner_Shared_Constants.MenuPriorityFifteen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Sort)]
        public static void MenuItem_Sort_LayerAlphabeticallyDescending() => HierarchyDesigner_Shared_Operations.Sort_GameObjectChildrenByLayer(false, "sort its children by layer alphabetically descending");

        [MenuItem(HierarchyDesigner_Shared_Constants.Tools_Sort + "/Sort Layer List Order Ascending", false, HierarchyDesigner_Shared_Constants.MenuPrioritySixteen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Sort)]
        public static void MenuItem_Sort_LayerListOrderAscending() => HierarchyDesigner_Shared_Operations.Sort_GameObjectChildrenByLayerListOrder(true, "sort its children by layer list order ascending");

        [MenuItem(HierarchyDesigner_Shared_Constants.Tools_Sort + "/Sort Layer List Order Descending", false, HierarchyDesigner_Shared_Constants.MenuPrioritySixteen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Sort)]
        public static void MenuItem_Sort_LayerListOrderDescending() => HierarchyDesigner_Shared_Operations.Sort_GameObjectChildrenByLayerListOrder(false, "sort its children by layer list order descending");

        [MenuItem(HierarchyDesigner_Shared_Constants.Tools_Sort + "/Sort Randomly", false, HierarchyDesigner_Shared_Constants.MenuPrioritySeventeen)]
        [HierarchyDesigner_Shared_Attributes(HierarchyDesigner_Attribute_Tools.Sort)]
        public static void MenuItem_Sort_Randomly() => HierarchyDesigner_Shared_Operations.Sort_GameObjectChildrenRandomly("sort its children randomly");
        #endregion
        #endregion

        #region GameObject
        [MenuItem(HierarchyDesigner_Shared_Constants.GroupRefresh + "/Refresh All GameObjects' Data", false, HierarchyDesigner_Shared_Constants.MenuPriorityNineteen)]
        public static void RefreshAllGameObjectsData() => HierarchyDesigner_Shared_Operations.RefreshAllGameObjectsData();

        [MenuItem(HierarchyDesigner_Shared_Constants.GroupRefresh + "/Refresh Selected GameObject's Data", false, HierarchyDesigner_Shared_Constants.MenuPriorityNineteen)]
        public static void RefreshSelectedGameObjectsData() => HierarchyDesigner_Shared_Operations.RefreshSelectedGameObjectsData();

        [MenuItem(HierarchyDesigner_Shared_Constants.GroupRefresh + "/Refresh Selected Main Icon", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwenty)]
        public static void RefreshSelectedMainIcon() => HierarchyDesigner_Shared_Operations.RefreshMainIconForSelectedGameObject();

        [MenuItem(HierarchyDesigner_Shared_Constants.GroupRefresh + "/Refresh Selected Component Icons", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwenty + 1)]
        public static void RefreshSelectedComponentIcons() => HierarchyDesigner_Shared_Operations.RefreshComponentIconsForSelectedGameObjects();

        [MenuItem(HierarchyDesigner_Shared_Constants.GroupRefresh + "/Refresh Selected Hierarchy Tree Icon", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwenty + 1)]
        public static void RefreshSelectedHierarchyTreeIcon() => HierarchyDesigner_Shared_Operations.RefreshHierarchyTreeIconForSelectedGameObjects();

        [MenuItem(HierarchyDesigner_Shared_Constants.GroupRefresh + "/Refresh Selected Tag", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwenty + 1)]
        public static void RefreshSelectedTag() => HierarchyDesigner_Shared_Operations.RefreshTagForSelectedGameObjects();

        [MenuItem(HierarchyDesigner_Shared_Constants.GroupRefresh + "/Refresh Selected Layer", false, HierarchyDesigner_Shared_Constants.MenuPriorityTwenty + 2)]
        public static void RefreshSelectedLayer() => HierarchyDesigner_Shared_Operations.RefreshLayerForSelectedGameObjects();
        #endregion
    }
}
#endif