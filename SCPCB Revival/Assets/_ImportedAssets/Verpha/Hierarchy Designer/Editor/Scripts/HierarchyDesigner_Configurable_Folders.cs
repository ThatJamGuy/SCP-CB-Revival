#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Verpha.HierarchyDesigner
{
    internal static class HierarchyDesigner_Configurable_Folders
    {
        #region Properties
        [System.Serializable]
        public class HierarchyDesigner_FolderData
        {
            public string Name = "Folder";
            public Color TextColor = HierarchyDesigner_Configurable_DesignSettings.FolderDefaultTextColor;
            public int FontSize = HierarchyDesigner_Configurable_DesignSettings.FolderDefaultFontSize;
            public FontStyle FontStyle = HierarchyDesigner_Configurable_DesignSettings.FolderDefaultFontStyle;
            public Color ImageColor = HierarchyDesigner_Configurable_DesignSettings.FolderDefaultImageColor;
            public FolderImageType ImageType = HierarchyDesigner_Configurable_DesignSettings.FolderDefaultImageType;
        }
        public enum FolderImageType
        { 
            Default, 
            DefaultOutline,
            ModernI,
            ModernII,
            ModernIII,
            ModernOutline,
        }
        private static Dictionary<string, HierarchyDesigner_FolderData> folders = new();
        #endregion

        #region Initialization
        public static void Initialize()
        {
            LoadSettings();
            LoadHierarchyDesignerManagerGameObjectCaches();
        }

        private static void LoadHierarchyDesignerManagerGameObjectCaches()
        {
            Dictionary<int, (Color textColor, int fontSize, FontStyle fontStyle, Color folderColor, FolderImageType folderImageType)> folderCache = new();
            foreach (KeyValuePair<string, HierarchyDesigner_FolderData> folder in folders)
            {
                int instanceID = folder.Key.GetHashCode();
                folderCache[instanceID] = (folder.Value.TextColor, folder.Value.FontSize, folder.Value.FontStyle, folder.Value.ImageColor, folder.Value.ImageType);
            }
            HierarchyDesigner_Manager_GameObject.FolderCache = folderCache;
        }
        #endregion

        #region Methods
        public static void SetFolderData(string folderName, Color textColor, int fontSize, FontStyle fontStyle, Color imageColor, FolderImageType imageType)
        {
            if (folders.TryGetValue(folderName, out HierarchyDesigner_FolderData folderData))
            {
                folderData.TextColor = textColor;
                folderData.FontSize = fontSize;
                folderData.FontStyle = fontStyle;
                folderData.ImageColor = imageColor;
                folderData.ImageType = imageType;
            }
            else
            {
                folders[folderName] = new()
                {
                    Name = folderName,
                    TextColor = textColor,
                    FontSize = fontSize,
                    FontStyle = fontStyle,
                    ImageColor = imageColor,
                    ImageType = imageType
                };
            }
            SaveSettings();
            HierarchyDesigner_Manager_GameObject.ClearFolderCache();
        }

        public static void ApplyChangesToFolders(Dictionary<string, HierarchyDesigner_FolderData> tempFolders, List<string> foldersOrder)
        {
            Dictionary<string, HierarchyDesigner_FolderData> orderedFolders = new();
            foreach (string key in foldersOrder)
            {
                if (tempFolders.TryGetValue(key, out HierarchyDesigner_FolderData folderData))
                {
                    orderedFolders[key] = folderData;
                }
            }
            folders = orderedFolders;
        }

        public static HierarchyDesigner_FolderData GetFolderData(string folderName)
        {
            if (folders.TryGetValue(folderName, out HierarchyDesigner_FolderData folderData))
            {
                return folderData;
            }
            return null;
        }

        public static Dictionary<string, HierarchyDesigner_FolderData> GetAllFoldersData(bool updateData)
        {
            if (updateData) LoadSettings();
            return new(folders);
        }

        public static bool RemoveFolderData(string folderName)
        {
            if (folders.TryGetValue(folderName, out _))
            {
                folders.Remove(folderName);
                SaveSettings();
                HierarchyDesigner_Manager_GameObject.ClearFolderCache();
                return true;
            }
            return false;
        }

        public static Dictionary<string, List<string>> GetFolderImageTypesGrouped()
        {
            return new()
            {
                {
                    "Default", new()
                    {
                        "Default",
                        "Default Outline"
                    }
                },
                {
                    "Modern", new()
                    {
                        "Modern I",
                        "Modern II",
                        "Modern III",
                        "Modern Outline"
                    }
                }
            };
        }

        public static FolderImageType ParseFolderImageType(string displayName)
        {
            return displayName switch
            {
                "Default" => FolderImageType.Default,
                "Default Outline" => FolderImageType.DefaultOutline,
                "Modern I" => FolderImageType.ModernI,
                "Modern II" => FolderImageType.ModernII,
                "Modern III" => FolderImageType.ModernIII,
                "Modern Outline" => FolderImageType.ModernOutline,
                _ => FolderImageType.Default,
            };
        }

        public static string GetFolderImageTypeDisplayName(FolderImageType imageType)
        {
            return imageType switch
            {
                FolderImageType.Default => "Default",
                FolderImageType.DefaultOutline => "Default Outline",
                FolderImageType.ModernI => "Modern I",
                FolderImageType.ModernII => "Modern II",
                FolderImageType.ModernIII => "Modern III",
                FolderImageType.ModernOutline => "Modern Outline",
                _ => imageType.ToString(),
            };
        }
        #endregion

        #region Save and Load
        public static void SaveSettings()
        {
            string dataFilePath = HierarchyDesigner_Shared_File.GetSavedDataFilePath(HierarchyDesigner_Shared_Constants.FolderSettingsTextFileName);
            HierarchyDesigner_Shared_Serializable<HierarchyDesigner_FolderData> serializableList = new(new(folders.Values));
            string json = JsonUtility.ToJson(serializableList, true);
            File.WriteAllText(dataFilePath, json);
            AssetDatabase.Refresh();
        }

        public static void LoadSettings()
        {
            string dataFilePath = HierarchyDesigner_Shared_File.GetSavedDataFilePath(HierarchyDesigner_Shared_Constants.FolderSettingsTextFileName);
            if (File.Exists(dataFilePath))
            {
                string json = File.ReadAllText(dataFilePath);
                HierarchyDesigner_Shared_Serializable<HierarchyDesigner_FolderData> loadedFolders = JsonUtility.FromJson<HierarchyDesigner_Shared_Serializable<HierarchyDesigner_FolderData>>(json);
                folders.Clear();
                foreach (HierarchyDesigner_FolderData folder in loadedFolders.items)
                {
                    folder.ImageType = HierarchyDesigner_Shared_Checker.ParseEnum(folder.ImageType.ToString(), FolderImageType.Default);
                    folders[folder.Name] = folder;
                }
            }
            else
            {
                SetDefaultSettings();
            }
        }

        private static void SetDefaultSettings()
        {
            folders = new();
        }
        #endregion
    }
}
#endif