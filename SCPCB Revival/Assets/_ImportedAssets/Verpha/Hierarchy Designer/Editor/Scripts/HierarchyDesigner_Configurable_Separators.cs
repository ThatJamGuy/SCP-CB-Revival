#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Verpha.HierarchyDesigner
{
    internal static class HierarchyDesigner_Configurable_Separators
    {
        #region Properties
        [System.Serializable]
        public class HierarchyDesigner_SeparatorData
        {
            public string Name = "Separator";
            public Color TextColor = HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultTextColor;
            public bool IsGradientBackground = HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultIsGradientBackground;
            public Color BackgroundColor = HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultBackgroundColor;
            public Gradient BackgroundGradient;
            public int FontSize = HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultFontSize;
            public FontStyle FontStyle = HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultFontStyle;
            public TextAnchor TextAnchor = HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultTextAnchor;
            public SeparatorImageType ImageType = HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultImageType;

            public HierarchyDesigner_SeparatorData()
            {
                BackgroundGradient = HierarchyDesigner_Shared_Color.CopyGradient(HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultBackgroundGradient);
            }
        }
        public enum SeparatorImageType
        {
            Default,
            DefaultFadedTop,
            DefaultFadedLeft,
            DefaultFadedRight,
            DefaultFadedBottom,
            DefaultFadedSideways,
            ClassicI,
            ClassicII,
            ModernI,
            ModernII,
            ModernIII,
            NeoI,
            NeoII,
            NextGenI,
            NextGenII,
        }
        private static Dictionary<string, HierarchyDesigner_SeparatorData> separators = new();
        #endregion

        #region Initialization
        public static void Initialize()
        {
            LoadSettings();
            LoadHierarchyDesignerManagerGameObjectCaches();
        }

        private static void LoadHierarchyDesignerManagerGameObjectCaches()
        {
            Dictionary<int, (Color textColor, bool isGradientBackground, Color backgroundColor, Gradient backgroundGradient, int fontSize, FontStyle fontStyle, TextAnchor textAnchor, SeparatorImageType separatorImageType)> separatorCache = new();
            foreach (KeyValuePair<string, HierarchyDesigner_SeparatorData> separator in separators)
            {
                int instanceID = separator.Key.GetHashCode();
                separatorCache[instanceID] = (separator.Value.TextColor, separator.Value.IsGradientBackground, separator.Value.BackgroundColor, separator.Value.BackgroundGradient, separator.Value.FontSize, separator.Value.FontStyle, separator.Value.TextAnchor, separator.Value.ImageType);
            }
            HierarchyDesigner_Manager_GameObject.SeparatorCache = separatorCache;
        }
        #endregion

        #region Methods
        public static void SetSeparatorData(string separatorName, Color textColor, bool isGradientBackground, Color backgroundColor, Gradient backgroundGradient, int fontSize, FontStyle fontStyle, TextAnchor textAnchor, SeparatorImageType imageType)
        {
            separatorName = HierarchyDesigner_Shared_Operations.StripPrefix(separatorName);
            if (separators.TryGetValue(separatorName, out HierarchyDesigner_SeparatorData separatorData))
            {
                separatorData.TextColor = textColor;
                separatorData.IsGradientBackground = isGradientBackground;
                separatorData.BackgroundColor = backgroundColor;
                separatorData.BackgroundGradient = HierarchyDesigner_Shared_Color.CopyGradient(backgroundGradient);
                separatorData.FontSize = fontSize;
                separatorData.FontStyle = fontStyle;
                separatorData.TextAnchor = textAnchor;
                separatorData.ImageType = imageType;
            }
            else
            {
                separators[separatorName] = new()
                {
                    Name = separatorName,
                    TextColor = textColor,
                    IsGradientBackground = isGradientBackground,
                    BackgroundColor = backgroundColor,
                    BackgroundGradient = HierarchyDesigner_Shared_Color.CopyGradient(backgroundGradient),
                    FontSize = fontSize,
                    FontStyle = fontStyle,
                    TextAnchor = textAnchor,
                    ImageType = imageType
                };
            }
            SaveSettings();
            HierarchyDesigner_Manager_GameObject.ClearSeparatorCache();
        }

        public static void ApplyChangesToSeparators(Dictionary<string, HierarchyDesigner_SeparatorData> tempSeparators, List<string> separatorsOrder)
        {
            Dictionary<string, HierarchyDesigner_SeparatorData> orderedSeparators = new();
            foreach (string key in separatorsOrder)
            {
                if (tempSeparators.TryGetValue(key, out HierarchyDesigner_SeparatorData separatorData))
                {
                    orderedSeparators[key] = separatorData;
                }
            }
            separators = orderedSeparators;
        }

        public static HierarchyDesigner_SeparatorData GetSeparatorData(string separatorName)
        {
            separatorName = HierarchyDesigner_Shared_Operations.StripPrefix(separatorName);
            if (separators.TryGetValue(separatorName, out HierarchyDesigner_SeparatorData separatorData))
            {
                return separatorData;
            }
            return null;
        }

        public static Dictionary<string, HierarchyDesigner_SeparatorData> GetAllSeparatorsData(bool updateData)
        {
            if (updateData) LoadSettings();
            return new(separators);
        }

        public static bool RemoveSeparatorData(string separatorName)
        {
            separatorName = HierarchyDesigner_Shared_Operations.StripPrefix(separatorName);
            if (separators.TryGetValue(separatorName, out _))
            {
                separators.Remove(separatorName);
                SaveSettings();
                HierarchyDesigner_Manager_GameObject.ClearSeparatorCache();
                return true;
            }
            return false;
        }

        public static Dictionary<string, List<string>> GetSeparatorImageTypesGrouped()
        {
            return new()
            {
                {
                    "Default", new()
                    {
                        "Default",
                        "Default Faded Top",
                        "Default Faded Left",
                        "Default Faded Right",
                        "Default Faded Bottom",
                        "Default Faded Sideways"
                    }
                },
                {
                    "Classic", new()
                    {
                        "Classic I",
                        "Classic II",
                    }
                },
                {
                    "Modern", new()
                    {
                        "Modern I",
                        "Modern II",
                        "Modern III"
                    }
                },
                {
                    "Neo", new()
                    {
                        "Neo I",
                        "Neo II"
                    } 
                },
                {
                    "Next-Gen", new()
                    {
                        "Next-Gen I",
                        "Next-Gen II"

                    }
                }
            };
        }

        public static SeparatorImageType ParseSeparatorImageType(string displayName)
        {
            return displayName switch
            {
                "Default" => SeparatorImageType.Default,
                "Default Faded Top" => SeparatorImageType.DefaultFadedTop,
                "Default Faded Left" => SeparatorImageType.DefaultFadedLeft,
                "Default Faded Right" => SeparatorImageType.DefaultFadedRight,
                "Default Faded Bottom" => SeparatorImageType.DefaultFadedBottom,
                "Default Faded Sideways" => SeparatorImageType.DefaultFadedSideways,
                "Classic I" => SeparatorImageType.ClassicI,
                "Classic II" => SeparatorImageType.ClassicII,
                "Modern I" => SeparatorImageType.ModernI,
                "Modern II" => SeparatorImageType.ModernII,
                "Modern III" => SeparatorImageType.ModernIII,
                "Neo I" => SeparatorImageType.NeoI,
                "Neo II" => SeparatorImageType.NeoII,
                "Next-Gen I" => SeparatorImageType.NextGenI,
                "Next-Gen II" => SeparatorImageType.NextGenII,
                _ => SeparatorImageType.Default,
            };
        }

        public static string GetSeparatorImageTypeDisplayName(SeparatorImageType imageType)
        {
            return imageType switch
            {
                SeparatorImageType.Default => "Default",
                SeparatorImageType.DefaultFadedTop => "Default Faded Top",
                SeparatorImageType.DefaultFadedLeft => "Default Faded Left",
                SeparatorImageType.DefaultFadedRight => "Default Faded Right",
                SeparatorImageType.DefaultFadedBottom => "Default Faded Bottom",
                SeparatorImageType.DefaultFadedSideways => "Default Faded Sideways",
                SeparatorImageType.ClassicI => "Classic I",
                SeparatorImageType.ClassicII => "Classic II",
                SeparatorImageType.ModernI => "Modern I",
                SeparatorImageType.ModernII => "Modern II",
                SeparatorImageType.ModernIII => "Modern III",
                SeparatorImageType.NeoI => "Neo I",
                SeparatorImageType.NeoII => "Neo II",
                SeparatorImageType.NextGenI => "Next-Gen I",
                SeparatorImageType.NextGenII => "Next-Gen II",
                _ => imageType.ToString(),
            };
        }
        #endregion

        #region Save and Load
        public static void SaveSettings()
        {
            string dataFilePath = HierarchyDesigner_Shared_File.GetSavedDataFilePath(HierarchyDesigner_Shared_Constants.SeparatorSettingsTextFileName);
            HierarchyDesigner_Shared_Serializable<HierarchyDesigner_SeparatorData> serializableList = new(new(separators.Values));
            string json = JsonUtility.ToJson(serializableList, true);
            File.WriteAllText(dataFilePath, json);
            AssetDatabase.Refresh();
        }

        public static void LoadSettings()
        {
            string dataFilePath = HierarchyDesigner_Shared_File.GetSavedDataFilePath(HierarchyDesigner_Shared_Constants.SeparatorSettingsTextFileName);
            if (File.Exists(dataFilePath))
            {
                string json = File.ReadAllText(dataFilePath);
                HierarchyDesigner_Shared_Serializable<HierarchyDesigner_SeparatorData> loadedSeparators = JsonUtility.FromJson<HierarchyDesigner_Shared_Serializable<HierarchyDesigner_SeparatorData>>(json);
                separators.Clear();
                foreach (HierarchyDesigner_SeparatorData separator in loadedSeparators.items)
                {
                    separator.ImageType = HierarchyDesigner_Shared_Checker.ParseEnum(separator.ImageType.ToString(), SeparatorImageType.Default);
                    separator.BackgroundGradient = HierarchyDesigner_Shared_Color.CopyGradient(separator.BackgroundGradient);
                    separators[separator.Name] = separator;
                }
            }
            else
            {
                SetDefaultSettings();
            }
        }

        private static void SetDefaultSettings()
        {
            separators = new();
        }
        #endregion
    }
}
#endif