#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;

namespace Verpha.HierarchyDesigner
{
    internal class HierarchyDesigner_Configurable_Presets
    {
        #region Properties
        [System.Serializable]
        public class HierarchyDesigner_Preset
        {
            public string presetName;
            public Color folderTextColor;
            public int folderFontSize;
            public FontStyle folderFontStyle;
            public Color folderColor;
            public HierarchyDesigner_Configurable_Folders.FolderImageType folderImageType;
            public Color separatorTextColor;
            public bool separatorIsGradientBackground;
            public Color separatorBackgroundColor;
            public Gradient separatorBackgroundGradient;
            public FontStyle separatorFontStyle;
            public int separatorFontSize;
            public TextAnchor separatorTextAlignment;
            public HierarchyDesigner_Configurable_Separators.SeparatorImageType separatorBackgroundImageType;
            public Color tagTextColor;
            public FontStyle tagFontStyle;
            public int tagFontSize;
            public TextAnchor tagTextAnchor;
            public Color layerTextColor;
            public FontStyle layerFontStyle;
            public int layerFontSize;
            public TextAnchor layerTextAnchor;
            public Color treeColor;
            public Color hierarchyLineColor;
            public Color lockColor;
            public int lockFontSize;
            public FontStyle lockFontStyle;
            public TextAnchor lockTextAnchor;

            public HierarchyDesigner_Preset(
                string name,
                Color folderTextColor, 
                int folderFontSize, 
                FontStyle folderFontStyle,
                Color folderColor,
                HierarchyDesigner_Configurable_Folders.FolderImageType folderImageType,
                Color separatorTextColor,
                bool separatorIsGradientBackground,
                Color separatorBackgroundColor,
                Gradient separatorBackgroundGradient,
                FontStyle separatorFontStyle,
                int separatorFontSize,
                TextAnchor separatorTextAlignment,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType separatorBackgroundImageType,
                Color tagTextColor,
                FontStyle tagFontStyle,
                int tagFontSize,
                TextAnchor tagTextAnchor,
                Color layerTextColor,
                FontStyle layerFontStyle,
                int layerFontSize,
                TextAnchor layerTextAnchor,
                Color treeColor,
                Color hierarchyLineColor,
                Color lockColor,
                int lockFontSize,
                FontStyle lockFontStyle,
                TextAnchor lockTextAnchor)
            {
                this.presetName = name;
                this.folderTextColor = folderTextColor;
                this.folderFontSize = folderFontSize;
                this.folderFontStyle = folderFontStyle;
                this.folderColor = folderColor;
                this.folderImageType = folderImageType;
                this.separatorTextColor = separatorTextColor;
                this.separatorIsGradientBackground = separatorIsGradientBackground;
                this.separatorBackgroundColor = separatorBackgroundColor;
                this.separatorBackgroundGradient = separatorBackgroundGradient;
                this.separatorFontStyle = separatorFontStyle;
                this.separatorFontSize = separatorFontSize;
                this.separatorTextAlignment = separatorTextAlignment;
                this.separatorBackgroundImageType = separatorBackgroundImageType;
                this.tagTextColor = tagTextColor;
                this.tagFontStyle = tagFontStyle;
                this.tagFontSize = tagFontSize;
                this.tagTextAnchor = tagTextAnchor;
                this.layerTextColor = layerTextColor;
                this.layerFontStyle = layerFontStyle;
                this.layerFontSize = layerFontSize;
                this.layerTextAnchor = layerTextAnchor;
                this.treeColor = treeColor;
                this.hierarchyLineColor = hierarchyLineColor;
                this.lockColor = lockColor;
                this.lockFontSize = lockFontSize;
                this.lockFontStyle = lockFontStyle;
                this.lockTextAnchor = lockTextAnchor;
            }
        }

        private static List<HierarchyDesigner_Preset> presets;
        #endregion

        #region Initialization
        public static void Initialize()
        {
            LoadPresets();
        }

        private static void LoadPresets()
        {
            presets = new()
            {
                AgeOfEnlightenmentPreset(),
                AzureDreamscapePreset(),
                BlackAndGoldPreset(),
                BlackAndWhitePreset(),
                BloodyMaryPreset(),
                BlueHarmonyPreset(),
                DeepOceanPreset(),
                DunesPreset(),
                FreshSwampPreset(),
                FrostyFogPreset(),
                IronCinderPreset(),
                JadeLakePreset(),
                LittleRedPreset(),
                MinimalBlackPreset(),
                MinimalWhitePreset(),
                NaturePreset(),
                NavyBlueLightPreset(),
                OldSchoolPreset(),
                PrettyPinkPreset(),
                PrismaticPreset(),
                RedDawnPreset(),
                StrawberrySalmonPreset(),
                SunflowerPreset(),
                TheTwoRealmsPreset(),
                WildcatsPreset(),
                YoungMonarchPreset(),
            };
        }
        #endregion

        #region Presets
        private static HierarchyDesigner_Preset AgeOfEnlightenmentPreset()
        {
            return new(
                "Age of Enlightenment",
                HierarchyDesigner_Shared_Color.HexToColor("#FFF9F4"),
                11,
                FontStyle.Normal,
                HierarchyDesigner_Shared_Color.HexToColor("#E2DAC1"),
                HierarchyDesigner_Configurable_Folders.FolderImageType.Default,
                HierarchyDesigner_Shared_Color.HexToColor("#464646"),
                false,
                HierarchyDesigner_Shared_Color.HexToColor("#FFF9F4"),
                new Gradient(),
                FontStyle.Normal,
                11,
                TextAnchor.MiddleCenter,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.ClassicI,
                HierarchyDesigner_Shared_Color.HexToColor("#6C6C6C"),
                FontStyle.Italic,
                10,
                TextAnchor.MiddleRight,
                HierarchyDesigner_Shared_Color.HexToColor("#FAF1EA"),
                FontStyle.Italic,
                10,
                TextAnchor.MiddleLeft,
                HierarchyDesigner_Shared_Color.HexToColor("#6C6C6C"),
                HierarchyDesigner_Shared_Color.HexToColor("#FAF1EA80"),
                HierarchyDesigner_Shared_Color.HexToColor("#FAF1EA"),
                10,
                FontStyle.Normal,
                TextAnchor.MiddleCenter
            );
        }

        private static HierarchyDesigner_Preset AzureDreamscapePreset()
        {
            return new(
                "Azure Dreamscape",
                HierarchyDesigner_Shared_Color.HexToColor("#8E9FD5"),
                11,
                FontStyle.BoldAndItalic,
                HierarchyDesigner_Shared_Color.HexToColor("#318DCB"),
                HierarchyDesigner_Configurable_Folders.FolderImageType.ModernOutline,
                HierarchyDesigner_Shared_Color.HexToColor("#7EBCEF"),
                false,
                HierarchyDesigner_Shared_Color.HexToColor("#3C5A81"),
                new Gradient(),
                FontStyle.BoldAndItalic,
                13,
                TextAnchor.MiddleCenter,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.DefaultFadedSideways,
                HierarchyDesigner_Shared_Color.HexToColor("#8E9FD5"),
                FontStyle.BoldAndItalic,
                9,
                TextAnchor.MiddleRight,
                HierarchyDesigner_Shared_Color.HexToColor("#8E9FD5"),
                FontStyle.BoldAndItalic,
                9,
                TextAnchor.MiddleRight,
                HierarchyDesigner_Shared_Color.HexToColor("#5A5485"),
                HierarchyDesigner_Shared_Color.HexToColor("#8E9FD580"),
                HierarchyDesigner_Shared_Color.HexToColor("#8E9FD5"),
                11,
                FontStyle.BoldAndItalic,
                TextAnchor.MiddleCenter
            );
        }

        private static HierarchyDesigner_Preset BlackAndGoldPreset()
        {
            return new(
                "Black and Gold",
                HierarchyDesigner_Shared_Color.HexToColor("#FFD102"),
                12,
                FontStyle.Bold,
                HierarchyDesigner_Shared_Color.HexToColor("#1C1C1C"),
                HierarchyDesigner_Configurable_Folders.FolderImageType.Default,
                HierarchyDesigner_Shared_Color.HexToColor("#FFD102"),
                false,
                HierarchyDesigner_Shared_Color.HexToColor("#1C1C1C"),
                new Gradient(),
                FontStyle.BoldAndItalic,
                13,
                TextAnchor.MiddleCenter,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.ModernI,
                HierarchyDesigner_Shared_Color.HexToColor("#1C1C1C"),
                FontStyle.BoldAndItalic,
                9,
                TextAnchor.MiddleRight,
                HierarchyDesigner_Shared_Color.HexToColor("#1C1C1C"),
                FontStyle.BoldAndItalic,
                9,
                TextAnchor.MiddleRight,
                HierarchyDesigner_Shared_Color.HexToColor("#FFC402"),
                HierarchyDesigner_Shared_Color.HexToColor("#00000080"),
                HierarchyDesigner_Shared_Color.HexToColor("#FFC402"),
                11,
                FontStyle.BoldAndItalic,
                TextAnchor.MiddleRight
            );
        }

        private static HierarchyDesigner_Preset BlackAndWhitePreset()
        {
            return new(
                "Black and White",
                HierarchyDesigner_Shared_Color.HexToColor("#FFFFFF"),
                12,
                FontStyle.Normal,
                HierarchyDesigner_Shared_Color.HexToColor("#000000"),
                HierarchyDesigner_Configurable_Folders.FolderImageType.Default,
                HierarchyDesigner_Shared_Color.HexToColor("#ffffff"),
                false,
                HierarchyDesigner_Shared_Color.HexToColor("#000000"),
                new Gradient(),
                FontStyle.Bold,
                12,
                TextAnchor.MiddleCenter,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.Default,
                HierarchyDesigner_Shared_Color.HexToColor("#ffffff80"),
                FontStyle.Italic,
                9,
                TextAnchor.MiddleRight,
                HierarchyDesigner_Shared_Color.HexToColor("#ffffff80"),
                FontStyle.Italic,
                9,
                TextAnchor.MiddleLeft,
                HierarchyDesigner_Shared_Color.HexToColor("#FFFFFF"),
                HierarchyDesigner_Shared_Color.HexToColor("#FFFFFF80"),
                HierarchyDesigner_Shared_Color.HexToColor("#FFFFFF"),
                11,
                FontStyle.Bold,
                TextAnchor.MiddleCenter
            );
        }

        private static HierarchyDesigner_Preset BloodyMaryPreset()
        {
            return new(
                "Bloody Mary",
                HierarchyDesigner_Shared_Color.HexToColor("#FFEEAAF0"),
                11,
                FontStyle.Normal,
                HierarchyDesigner_Shared_Color.HexToColor("#C50515E6"),
                HierarchyDesigner_Configurable_Folders.FolderImageType.ModernIII,
                HierarchyDesigner_Shared_Color.HexToColor("#FFFFFFE1"),
                false,
                HierarchyDesigner_Shared_Color.HexToColor("#CF1625F0"),
                new Gradient(),
                FontStyle.Bold,
                12,
                TextAnchor.UpperCenter,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.DefaultFadedBottom,
                HierarchyDesigner_Shared_Color.HexToColor("#FFEEAA9C"),
                FontStyle.Italic,
                8,
                TextAnchor.MiddleRight,
                HierarchyDesigner_Shared_Color.HexToColor("#FFEEAA9C"),
                FontStyle.Italic,
                8,
                TextAnchor.MiddleCenter,
                HierarchyDesigner_Shared_Color.HexToColor("#FFFFFFC8"),
                HierarchyDesigner_Shared_Color.HexToColor("#00000080"),
                HierarchyDesigner_Shared_Color.HexToColor("#FFFFFFC8"),
                11,
                FontStyle.Normal,
                TextAnchor.UpperCenter
            );
        }

        private static HierarchyDesigner_Preset BlueHarmonyPreset()
        {
            return new(
                "Blue Harmony",
                HierarchyDesigner_Shared_Color.HexToColor("#A5D2FF"),
                11,
                FontStyle.Bold,
                HierarchyDesigner_Shared_Color.HexToColor("#6AB1F8"),
                HierarchyDesigner_Configurable_Folders.FolderImageType.ModernII,
                HierarchyDesigner_Shared_Color.HexToColor("#A5D2FF"),
                false,
                HierarchyDesigner_Shared_Color.HexToColor("#277DEC"),
                new Gradient(),
                FontStyle.Bold,
                12,
                TextAnchor.MiddleCenter,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.ModernII,
                HierarchyDesigner_Shared_Color.HexToColor("#6AB1F8F0"),
                FontStyle.Bold,
                8,
                TextAnchor.MiddleRight,
                HierarchyDesigner_Shared_Color.HexToColor("#A5D2FF"),
                FontStyle.Bold,
                9,
                TextAnchor.MiddleLeft,
                HierarchyDesigner_Shared_Color.HexToColor("#A5D2FF"),
                HierarchyDesigner_Shared_Color.HexToColor("#A5D2FF80"),
                HierarchyDesigner_Shared_Color.HexToColor("#A5D2FF"),
                11,
                FontStyle.Bold,
                TextAnchor.MiddleCenter
            );
        }

        private static HierarchyDesigner_Preset DeepOceanPreset()
        {
            return new(
                "Deep Ocean",
                HierarchyDesigner_Shared_Color.HexToColor("#1E4E8A"),
                12,
                FontStyle.BoldAndItalic,
                HierarchyDesigner_Shared_Color.HexToColor("#1E4E8A"),
                HierarchyDesigner_Configurable_Folders.FolderImageType.ModernIII,
                HierarchyDesigner_Shared_Color.HexToColor("#041F54C8"),
                false,
                HierarchyDesigner_Shared_Color.HexToColor("#041F54"),
                new Gradient(),
                FontStyle.Bold,
                12,
                TextAnchor.LowerRight,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.DefaultFadedRight,
                HierarchyDesigner_Shared_Color.HexToColor("#213864"),
                FontStyle.Bold,
                8,
                TextAnchor.MiddleRight,
                HierarchyDesigner_Shared_Color.HexToColor("#213864"),
                FontStyle.Bold,
                9,
                TextAnchor.MiddleRight,
                HierarchyDesigner_Shared_Color.HexToColor("#213864"),
                HierarchyDesigner_Shared_Color.HexToColor("#21386480"),
                HierarchyDesigner_Shared_Color.HexToColor("#213864"),
                10,
                FontStyle.BoldAndItalic,
                TextAnchor.MiddleRight
            );
        }

        private static HierarchyDesigner_Preset DunesPreset()
        {
            return new(
                "Dunes",
                HierarchyDesigner_Shared_Color.HexToColor("#E7D7C7"),
                12,
                FontStyle.Italic,
                HierarchyDesigner_Shared_Color.HexToColor("#DDC0A4"),
                HierarchyDesigner_Configurable_Folders.FolderImageType.Default,
                HierarchyDesigner_Shared_Color.HexToColor("#E4C6AB"),
                false,
                HierarchyDesigner_Shared_Color.HexToColor("#AB673F"),
                new Gradient(),
                FontStyle.Italic,
                13,
                TextAnchor.MiddleCenter,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.DefaultFadedRight,
                HierarchyDesigner_Shared_Color.HexToColor("#DDC0A4E1"),
                FontStyle.Italic,
                8,
                TextAnchor.MiddleRight,
                HierarchyDesigner_Shared_Color.HexToColor("#DDC0A4E1"),
                FontStyle.Italic,
                8,
                TextAnchor.MiddleCenter,
                HierarchyDesigner_Shared_Color.HexToColor("#DDC0A4E1"),
                HierarchyDesigner_Shared_Color.HexToColor("#DDC0A480"),
                HierarchyDesigner_Shared_Color.HexToColor("#DDC0A4E1"),
                11,
                FontStyle.Italic,
                TextAnchor.MiddleCenter
            );
        }

        private static HierarchyDesigner_Preset FreshSwampPreset()
        {
            return new(
                "Fresh Swamp",
                HierarchyDesigner_Shared_Color.HexToColor("#E7DECD"),
                12,
                FontStyle.Normal,
                HierarchyDesigner_Shared_Color.HexToColor("#FBFAF8"),
                HierarchyDesigner_Configurable_Folders.FolderImageType.ModernII,
                HierarchyDesigner_Shared_Color.HexToColor("#FBFAF8"),
                true,
                HierarchyDesigner_Shared_Color.HexToColor("#698F3F"),
#if UNITY_2022_3_OR_NEWER
                HierarchyDesigner_Shared_Color.CreateGradient(GradientMode.PerceptualBlend, ("#698F3F", 255, 0f), ("#804E49", 255, 100f)),
#else
                HierarchyDesigner_Shared_Color.CreateGradient(GradientMode.Blend, ("#698F3F", 255, 0f), ("#804E49", 255, 100f)),
#endif
                FontStyle.Normal,
                13,
                TextAnchor.MiddleCenter,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.ModernII,
                HierarchyDesigner_Shared_Color.HexToColor("#698F3F"),
                FontStyle.BoldAndItalic,
                9,
                TextAnchor.MiddleRight,
                HierarchyDesigner_Shared_Color.HexToColor("#E7DECD"),
                FontStyle.BoldAndItalic,
                9,
                TextAnchor.MiddleLeft,
                HierarchyDesigner_Shared_Color.HexToColor("#698F3F"),
                HierarchyDesigner_Shared_Color.HexToColor("#698F3F80"),
                HierarchyDesigner_Shared_Color.HexToColor("#FBFAF8"),
                11,
                FontStyle.BoldAndItalic,
                TextAnchor.MiddleCenter
            );
        }

        private static HierarchyDesigner_Preset FrostyFogPreset()
        {
            return new(
                "Frosty Fog",
                HierarchyDesigner_Shared_Color.HexToColor("#DBEAEE"),
                12,
                FontStyle.Normal,
                HierarchyDesigner_Shared_Color.HexToColor("#C4E7F3DC"),
                HierarchyDesigner_Configurable_Folders.FolderImageType.Default,
                HierarchyDesigner_Shared_Color.HexToColor("#E2F0F5"),
                true,
                HierarchyDesigner_Shared_Color.HexToColor("#C7E6F1"),
                HierarchyDesigner_Shared_Color.CreateGradient(GradientMode.Blend, ("#A9DDEF", 255, 20f), ("#BDE7F5", 200, 50f), ("#DCF6FF", 120, 90f), ("DBEFF5", 100, 100f)),
                FontStyle.Italic,
                13,
                TextAnchor.MiddleRight,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.DefaultFadedRight,
                HierarchyDesigner_Shared_Color.HexToColor("#ACDEEF"),
                FontStyle.BoldAndItalic,
                10,
                TextAnchor.MiddleRight,
                HierarchyDesigner_Shared_Color.HexToColor("#9FA8AB"),
                FontStyle.Italic,
                11,
                TextAnchor.MiddleCenter,
                HierarchyDesigner_Shared_Color.HexToColor("#CADCE2"),
                HierarchyDesigner_Shared_Color.HexToColor("#C4E5F180"),
                HierarchyDesigner_Shared_Color.HexToColor("#C4E5F1"),
                11,
                FontStyle.BoldAndItalic,
                TextAnchor.MiddleCenter
            );
        }

        private static HierarchyDesigner_Preset IronCinderPreset()
        {
            return new(
                "Iron Cinder",
                HierarchyDesigner_Shared_Color.HexToColor("#C8C8C8"),
                11,
                FontStyle.BoldAndItalic,
                HierarchyDesigner_Shared_Color.HexToColor("#969696"),
                HierarchyDesigner_Configurable_Folders.FolderImageType.ModernIII,
                HierarchyDesigner_Shared_Color.HexToColor("#C8C8C8"),
                true,
                HierarchyDesigner_Shared_Color.HexToColor("#646464"),
#if UNITY_2022_3_OR_NEWER
                HierarchyDesigner_Shared_Color.CreateGradient(GradientMode.PerceptualBlend, ("#191919", 255, 0f), ("#323232", 250, 25f), ("#646464", 250, 50f), ("323232", 250, 75f), ("191919", 250, 100f)),
#else
                HierarchyDesigner_Shared_Color.CreateGradient(GradientMode.Blend, ("#191919", 255, 0f), ("#323232", 250, 25f), ("#646464", 250, 50f), ("323232", 250, 75f), ("191919", 250, 100f)),
#endif
                FontStyle.BoldAndItalic,
                12,
                TextAnchor.MiddleCenter,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.ModernIII,
                HierarchyDesigner_Shared_Color.HexToColor("#C8C8C8"),
                FontStyle.Italic,
                9,
                TextAnchor.MiddleRight,
                HierarchyDesigner_Shared_Color.HexToColor("#969696"),
                FontStyle.Italic,
                9,
                TextAnchor.MiddleCenter,
                HierarchyDesigner_Shared_Color.HexToColor("#646464"),
                HierarchyDesigner_Shared_Color.HexToColor("#19191980"),
                HierarchyDesigner_Shared_Color.HexToColor("#C8C8C8"),
                11,
                FontStyle.BoldAndItalic,
                TextAnchor.MiddleCenter
            );
        }

        private static HierarchyDesigner_Preset JadeLakePreset()
        {
            return new(
                "Jade Lake",
                HierarchyDesigner_Shared_Color.HexToColor("#DBD3D8"),
                11,
                FontStyle.Bold,
                HierarchyDesigner_Shared_Color.HexToColor("#93A7AA"),
                HierarchyDesigner_Configurable_Folders.FolderImageType.ModernIII,
                HierarchyDesigner_Shared_Color.HexToColor("#DBD3D8"),
                false,
                HierarchyDesigner_Shared_Color.HexToColor("#2E5E4E"),
                new Gradient(),
                FontStyle.Bold,
                12,
                TextAnchor.UpperCenter,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.DefaultFadedBottom,
                HierarchyDesigner_Shared_Color.HexToColor("#93A7AA"),
                FontStyle.Italic,
                8,
                TextAnchor.MiddleRight,
                HierarchyDesigner_Shared_Color.HexToColor("#DBD3D8"),
                FontStyle.Italic,
                8,
                TextAnchor.MiddleCenter,
                HierarchyDesigner_Shared_Color.HexToColor("#93A7AA"),
                HierarchyDesigner_Shared_Color.HexToColor("#2E5E4E80"),
                HierarchyDesigner_Shared_Color.HexToColor("#A7B5B9"),
                11,
                FontStyle.Bold,
                TextAnchor.MiddleCenter
            );
        }

        private static HierarchyDesigner_Preset LittleRedPreset()
        {
            return new(
                "Little Red",
                HierarchyDesigner_Shared_Color.HexToColor("#FFFFFF"),
                11,
                FontStyle.Bold,
                HierarchyDesigner_Shared_Color.HexToColor("#E02D3C"),
                HierarchyDesigner_Configurable_Folders.FolderImageType.Default,
                HierarchyDesigner_Shared_Color.HexToColor("#FFFFFF"),
                false,
                HierarchyDesigner_Shared_Color.HexToColor("#E02D3CF0"),
                new Gradient(),
                FontStyle.Bold,
                11,
                TextAnchor.MiddleLeft,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.DefaultFadedRight,
                HierarchyDesigner_Shared_Color.HexToColor("#FFFFFF"),
                FontStyle.Bold,
                10,
                TextAnchor.MiddleRight,
                HierarchyDesigner_Shared_Color.HexToColor("#D62E3C"),
                FontStyle.Bold,
                10,
                TextAnchor.MiddleLeft,
                HierarchyDesigner_Shared_Color.HexToColor("#FFFFFF"),
                HierarchyDesigner_Shared_Color.HexToColor("#FFFFFF80"),
                HierarchyDesigner_Shared_Color.HexToColor("#FFFFFF"),
                11,
                FontStyle.Bold,
                TextAnchor.MiddleCenter
            );
        }

        private static HierarchyDesigner_Preset MinimalBlackPreset()
        {
            return new(
                "Minimal Black",
                HierarchyDesigner_Shared_Color.HexToColor("#000000"),
                11,
                FontStyle.Normal,
                HierarchyDesigner_Shared_Color.HexToColor("#000000"),
                HierarchyDesigner_Configurable_Folders.FolderImageType.DefaultOutline,
                HierarchyDesigner_Shared_Color.HexToColor("#646464"),
                false,
                HierarchyDesigner_Shared_Color.HexToColor("#000000"),
                new Gradient(),
                FontStyle.Bold,
                10,
                TextAnchor.MiddleLeft,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.Default,
                HierarchyDesigner_Shared_Color.HexToColor("#000000C8"),
                FontStyle.Italic,
                8,
                TextAnchor.MiddleRight,
                HierarchyDesigner_Shared_Color.HexToColor("#000000C8"),
                FontStyle.Italic,
                8,
                TextAnchor.MiddleCenter,
                HierarchyDesigner_Shared_Color.HexToColor("#000000F0"),
                HierarchyDesigner_Shared_Color.HexToColor("#00000080"),
                HierarchyDesigner_Shared_Color.HexToColor("#000000F0"),
                10,
                FontStyle.Normal,
                TextAnchor.UpperCenter
            );
        }

        private static HierarchyDesigner_Preset MinimalWhitePreset()
        {
            return new(
                "Minimal White",
                HierarchyDesigner_Shared_Color.HexToColor("#FFFFFF"),
                11,
                FontStyle.Normal,
                HierarchyDesigner_Shared_Color.HexToColor("#FFFFFF"),
                HierarchyDesigner_Configurable_Folders.FolderImageType.DefaultOutline,
                HierarchyDesigner_Shared_Color.HexToColor("#9B9B9B"),
                false,
                HierarchyDesigner_Shared_Color.HexToColor("#FFFFFF"),
                new Gradient(),
                FontStyle.Bold,
                10,
                TextAnchor.MiddleLeft,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.Default,
                HierarchyDesigner_Shared_Color.HexToColor("#FFFFFFC8"),
                FontStyle.Italic,
                8,
                TextAnchor.MiddleCenter,
                HierarchyDesigner_Shared_Color.HexToColor("#FFFFFFC8"),
                FontStyle.Italic,
                8,
                TextAnchor.MiddleRight,
                HierarchyDesigner_Shared_Color.HexToColor("#FFFFFFF0"),
                HierarchyDesigner_Shared_Color.HexToColor("#FFFFFF80"),
                HierarchyDesigner_Shared_Color.HexToColor("#FFFFFFF0"),
                10,
                FontStyle.Normal,
                TextAnchor.UpperCenter
            );
        }

        private static HierarchyDesigner_Preset NaturePreset()
        {
            return new(
                "Nature",
                HierarchyDesigner_Shared_Color.HexToColor("#AAD9A5"),
                12,
                FontStyle.Normal,
                HierarchyDesigner_Shared_Color.HexToColor("#DFEAF0"),
                HierarchyDesigner_Configurable_Folders.FolderImageType.Default,
                HierarchyDesigner_Shared_Color.HexToColor("#DFF6CA"),
                false,
                HierarchyDesigner_Shared_Color.HexToColor("#70B879"),
                new Gradient(),
                FontStyle.Normal,
                13,
                TextAnchor.MiddleLeft,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.ModernII,
                HierarchyDesigner_Shared_Color.HexToColor("#AAD9A5C8"),
                FontStyle.Normal,
                9,
                TextAnchor.MiddleCenter,
                HierarchyDesigner_Shared_Color.HexToColor("#AAD9A5C8"),
                FontStyle.Normal,
                9,
                TextAnchor.MiddleCenter,
                HierarchyDesigner_Shared_Color.HexToColor("#BCD8E3"),
                HierarchyDesigner_Shared_Color.HexToColor("#BFDFB180"),
                HierarchyDesigner_Shared_Color.HexToColor("#BFDFB1"),
                11,
                FontStyle.Italic,
                TextAnchor.MiddleCenter
            );
        }

        private static HierarchyDesigner_Preset NavyBlueLightPreset()
        {
            return new(
                "Navy Blue Light",
                HierarchyDesigner_Shared_Color.HexToColor("#AAD6EC"),
                11,
                FontStyle.Bold,
                HierarchyDesigner_Shared_Color.HexToColor("#AAD6EC"),
                HierarchyDesigner_Configurable_Folders.FolderImageType.Default,
                HierarchyDesigner_Shared_Color.HexToColor("#AAD6EC"),
                false,
                HierarchyDesigner_Shared_Color.HexToColor("#113065"),
                new Gradient(),
                FontStyle.Bold,
                12,
                TextAnchor.MiddleCenter,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.ModernII,
                HierarchyDesigner_Shared_Color.HexToColor("#AAD6ECC8"),
                FontStyle.Bold,
                9,
                TextAnchor.MiddleRight,
                HierarchyDesigner_Shared_Color.HexToColor("#AAD6ECC8"),
                FontStyle.Bold,
                9,
                TextAnchor.MiddleCenter,
                HierarchyDesigner_Shared_Color.HexToColor("#AAD6EC"),
                HierarchyDesigner_Shared_Color.HexToColor("#AAD6EC80"),
                HierarchyDesigner_Shared_Color.HexToColor("#AAD6EC"),
                11,
                FontStyle.Bold,
                TextAnchor.MiddleCenter
            );
        }

        private static HierarchyDesigner_Preset OldSchoolPreset()
        {
            return new(
                "Old School",
                HierarchyDesigner_Shared_Color.HexToColor("#1FC742"),
                11,
                FontStyle.Normal,
                HierarchyDesigner_Shared_Color.HexToColor("#686868"),
                HierarchyDesigner_Configurable_Folders.FolderImageType.Default,
                HierarchyDesigner_Shared_Color.HexToColor("#00FF34"),
                false,
                HierarchyDesigner_Shared_Color.HexToColor("#010101"),
                new Gradient(),
                FontStyle.Normal,
                12,
                TextAnchor.MiddleCenter,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.Default,
                HierarchyDesigner_Shared_Color.HexToColor("#1FC742F0"),
                FontStyle.Normal,
                9,
                TextAnchor.MiddleLeft,
                HierarchyDesigner_Shared_Color.HexToColor("#1FC742F0"),
                FontStyle.Normal,
                9,
                TextAnchor.MiddleLeft,
                HierarchyDesigner_Shared_Color.HexToColor("#686868"),
                HierarchyDesigner_Shared_Color.HexToColor("#7D7D7D80"),
                HierarchyDesigner_Shared_Color.HexToColor("#7D7D7D"),
                11,
                FontStyle.Normal,
                TextAnchor.MiddleCenter
            );
        }

        private static HierarchyDesigner_Preset PrettyPinkPreset()
        {
            return new(
                "Pretty Pink",
                HierarchyDesigner_Shared_Color.HexToColor("#FFFFFF"),
                11,
                FontStyle.Bold,
                HierarchyDesigner_Shared_Color.HexToColor("#FF4071"),
                HierarchyDesigner_Configurable_Folders.FolderImageType.ModernIII,
                HierarchyDesigner_Shared_Color.HexToColor("#EFEBE0"),
                false,
                HierarchyDesigner_Shared_Color.HexToColor("#FB4570"),
                new Gradient(),
                FontStyle.Bold,
                12,
                TextAnchor.MiddleLeft,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.ModernII,
                HierarchyDesigner_Shared_Color.HexToColor("#FB4570FA"),
                FontStyle.BoldAndItalic,
                9,
                TextAnchor.MiddleRight,
                HierarchyDesigner_Shared_Color.HexToColor("#FB4570FA"),
                FontStyle.BoldAndItalic,
                9,
                TextAnchor.MiddleLeft,
                HierarchyDesigner_Shared_Color.HexToColor("#FB4570"),
                HierarchyDesigner_Shared_Color.HexToColor("#FB457080"),
                HierarchyDesigner_Shared_Color.HexToColor("#FB4570"),
                11,
                FontStyle.BoldAndItalic,
                TextAnchor.MiddleCenter
            );
        }

        private static HierarchyDesigner_Preset PrismaticPreset()
        {
            return new(
                "Prismatic",
                HierarchyDesigner_Shared_Color.HexToColor("#E5CCE5"),
                11,
                FontStyle.BoldAndItalic,
                HierarchyDesigner_Shared_Color.HexToColor("#A2D5FF"),
                HierarchyDesigner_Configurable_Folders.FolderImageType.ModernIII,
                HierarchyDesigner_Shared_Color.HexToColor("#FFFFFF"),
                true,
                HierarchyDesigner_Shared_Color.HexToColor("#FFFFFF"),
#if UNITY_2022_3_OR_NEWER
                HierarchyDesigner_Shared_Color.CreateGradient(GradientMode.PerceptualBlend, ("#2F7FFF", 155, 0f), ("#72BFAF", 158, 35f), ("E8CEE8", 162, 70f), ("#FFFFFF", 165, 100f)),
#else
                HierarchyDesigner_Shared_Color.CreateGradient(GradientMode.Blend, ("#2F7FFF", 155, 0f), ("#72BFAF", 158, 35f), ("E8CEE8", 162, 70f), ("#FFFFFF", 165, 100f)),
#endif
                FontStyle.BoldAndItalic,
                12,
                TextAnchor.MiddleCenter,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.NeoI,
                HierarchyDesigner_Shared_Color.HexToColor("#9FD3E0"),
                FontStyle.BoldAndItalic,
                10,
                TextAnchor.MiddleRight,
                HierarchyDesigner_Shared_Color.HexToColor("#E09FAD"),
                FontStyle.BoldAndItalic,
                10,
                TextAnchor.MiddleLeft,
                HierarchyDesigner_Shared_Color.HexToColor("#FFFFFF"),
                HierarchyDesigner_Shared_Color.HexToColor("#E09FAD80"),
                HierarchyDesigner_Shared_Color.HexToColor("#FFFFFF"),
                11,
                FontStyle.BoldAndItalic,
                TextAnchor.MiddleCenter
            );
        }

        private static HierarchyDesigner_Preset RedDawnPreset()
        {
            return new(
                "Red Dawn",
                HierarchyDesigner_Shared_Color.HexToColor("#FE5E2A"),
                11,
                FontStyle.BoldAndItalic,
                HierarchyDesigner_Shared_Color.HexToColor("#DF4148"),
                HierarchyDesigner_Configurable_Folders.FolderImageType.DefaultOutline,
                HierarchyDesigner_Shared_Color.HexToColor("#FF5F2A"),
                false,
                HierarchyDesigner_Shared_Color.HexToColor("#C00531"),
                new Gradient(),
                FontStyle.BoldAndItalic,
                13,
                TextAnchor.MiddleCenter,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.DefaultFadedSideways,
                HierarchyDesigner_Shared_Color.HexToColor("#DF4148F0"),
                FontStyle.BoldAndItalic,
                9,
                TextAnchor.MiddleRight,
                HierarchyDesigner_Shared_Color.HexToColor("#DF4148F0"),
                FontStyle.BoldAndItalic,
                9,
                TextAnchor.MiddleRight,
                HierarchyDesigner_Shared_Color.HexToColor("#DF4148"),
                HierarchyDesigner_Shared_Color.HexToColor("#DF4148B4"),
                HierarchyDesigner_Shared_Color.HexToColor("#DF4148"),
                11,
                FontStyle.Italic,
                TextAnchor.MiddleRight
            );
        }

        private static HierarchyDesigner_Preset StrawberrySalmonPreset()
        {
            return new HierarchyDesigner_Preset(
                "Strawberry Salmon",
                HierarchyDesigner_Shared_Color.HexToColor("#FFC6C6"),
                11,
                FontStyle.Bold,
                HierarchyDesigner_Shared_Color.HexToColor("#FF5574"),
                HierarchyDesigner_Configurable_Folders.FolderImageType.ModernI,
                HierarchyDesigner_Shared_Color.HexToColor("#FAD8D8"),
                false,
                HierarchyDesigner_Shared_Color.HexToColor("#F87474"),
                new Gradient(),
                FontStyle.Bold,
                11,
                TextAnchor.MiddleCenter,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.NeoI,
                HierarchyDesigner_Shared_Color.HexToColor("#D85E74"),
                FontStyle.Italic,
                10,
                TextAnchor.MiddleRight,
                HierarchyDesigner_Shared_Color.HexToColor("#FAD8D8"),
                FontStyle.Italic,
                10,
                TextAnchor.MiddleLeft,
                HierarchyDesigner_Shared_Color.HexToColor("#FAD8D8"),
                HierarchyDesigner_Shared_Color.HexToColor("#FAD8D880"),
                HierarchyDesigner_Shared_Color.HexToColor("#FAD8D8"),
                11,
                FontStyle.Italic,
                TextAnchor.MiddleCenter
            );
        }

        private static HierarchyDesigner_Preset SunflowerPreset()
        {
            return new(
                "Sunflower",
                HierarchyDesigner_Shared_Color.HexToColor("#F8B701"),
                12,
                FontStyle.Bold,
                HierarchyDesigner_Shared_Color.HexToColor("#298AEC"),
                HierarchyDesigner_Configurable_Folders.FolderImageType.ModernI,
                HierarchyDesigner_Shared_Color.HexToColor("#FFC80A"),
                false,
                HierarchyDesigner_Shared_Color.HexToColor("#2A8FF3"),
                new Gradient(),
                FontStyle.Bold,
                13,
                TextAnchor.MiddleCenter,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.ModernI,
                HierarchyDesigner_Shared_Color.HexToColor("#F8B701"),
                FontStyle.BoldAndItalic,
                9,
                TextAnchor.MiddleRight,
                HierarchyDesigner_Shared_Color.HexToColor("#F8B701"),
                FontStyle.BoldAndItalic,
                9,
                TextAnchor.MiddleLeft,
                HierarchyDesigner_Shared_Color.HexToColor("#F8B701"),
                HierarchyDesigner_Shared_Color.HexToColor("#F8B70180"),
                HierarchyDesigner_Shared_Color.HexToColor("#F8B701"),
                10,
                FontStyle.Bold,
                TextAnchor.MiddleCenter
            );
        }

        private static HierarchyDesigner_Preset TheTwoRealmsPreset()
        {
            return new(
                "The Two Realms",
                HierarchyDesigner_Shared_Color.HexToColor("#01BAEF"),
                12,
                FontStyle.Normal,
                HierarchyDesigner_Shared_Color.HexToColor("#0CBABA"),
                HierarchyDesigner_Configurable_Folders.FolderImageType.ModernI,
                HierarchyDesigner_Shared_Color.HexToColor("#150811"),
                true,
                HierarchyDesigner_Shared_Color.HexToColor("#26081C"),
                HierarchyDesigner_Shared_Color.CreateGradient(GradientMode.Blend, ("#150811", 255, 0f), ("#26081C", 255, 20f), ("#380036", 150, 50f), ("0CBABA", 200, 75f), ("01BAEF", 255, 100f)),
                FontStyle.Bold,
                12,
                TextAnchor.UpperCenter,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.NeoI,
                HierarchyDesigner_Shared_Color.HexToColor("#380036"),
                FontStyle.BoldAndItalic,
                9,
                TextAnchor.MiddleRight,
                HierarchyDesigner_Shared_Color.HexToColor("#01BAEF"),
                FontStyle.BoldAndItalic,
                9,
                TextAnchor.MiddleLeft,
                HierarchyDesigner_Shared_Color.HexToColor("#150811"),
                HierarchyDesigner_Shared_Color.HexToColor("#15081180"),
                HierarchyDesigner_Shared_Color.HexToColor("#01BAEF"),
                11,
                FontStyle.Bold,
                TextAnchor.MiddleCenter
            );
        }

        private static HierarchyDesigner_Preset WildcatsPreset()
        {
            return new(
                "Wildcats",
                HierarchyDesigner_Shared_Color.HexToColor("#FFFFFF"),
                11,
                FontStyle.Bold,
                HierarchyDesigner_Shared_Color.HexToColor("#FFCF28"),
                HierarchyDesigner_Configurable_Folders.FolderImageType.Default,
                HierarchyDesigner_Shared_Color.HexToColor("#FFCF28"),
                false,
                HierarchyDesigner_Shared_Color.HexToColor("#1D5098"),
                new Gradient(),
                FontStyle.Bold,
                13,
                TextAnchor.MiddleCenter,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.DefaultFadedSideways,
                HierarchyDesigner_Shared_Color.HexToColor("#FFFFFF"),
                FontStyle.Bold,
                9,
                TextAnchor.MiddleRight,
                HierarchyDesigner_Shared_Color.HexToColor("#FFCF28"),
                FontStyle.BoldAndItalic,
                10,
                TextAnchor.MiddleLeft,
                HierarchyDesigner_Shared_Color.HexToColor("#FFFFFF"),
                HierarchyDesigner_Shared_Color.HexToColor("#1D509880"),
                HierarchyDesigner_Shared_Color.HexToColor("#F8B701"),
                11,
                FontStyle.BoldAndItalic,
                TextAnchor.UpperCenter
            );
        }

        private static HierarchyDesigner_Preset YoungMonarchPreset()
        {
            return new(
                "Young Monarch",
                HierarchyDesigner_Shared_Color.HexToColor("#EAEAEA"),
                12,
                FontStyle.Normal,
                HierarchyDesigner_Shared_Color.HexToColor("#4F6D7A"),
                HierarchyDesigner_Configurable_Folders.FolderImageType.Default,
                HierarchyDesigner_Shared_Color.HexToColor("#EAEAEA"),
                false,
                HierarchyDesigner_Shared_Color.HexToColor("#DD6E42"),
                new Gradient(),
                FontStyle.Bold,
                12,
                TextAnchor.UpperCenter,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.NeoII,
                HierarchyDesigner_Shared_Color.HexToColor("#E8DAB2"),
                FontStyle.BoldAndItalic,
                9,
                TextAnchor.MiddleRight,
                HierarchyDesigner_Shared_Color.HexToColor("#C0D6DF"),
                FontStyle.BoldAndItalic,
                9,
                TextAnchor.MiddleLeft,
                HierarchyDesigner_Shared_Color.HexToColor("#4F6D7A"),
                HierarchyDesigner_Shared_Color.HexToColor("#EAEAEA80"),
                HierarchyDesigner_Shared_Color.HexToColor("#E8DAB2"),
                11,
                FontStyle.Bold,
                TextAnchor.MiddleCenter
            );
        }
        #endregion

        #region Methods
        public static List<HierarchyDesigner_Preset> Presets
        {
            get
            {
                if (presets == null) LoadPresets();
                return presets;
            }
        }

        public static string[] GetPresetNames()
        {
            List<HierarchyDesigner_Preset> presetList = Presets;
            string[] presetNames = new string[presetList.Count];
            for (int i = 0; i < presetList.Count; i++)
            {
                presetNames[i] = presetList[i].presetName;
            }
            return presetNames;
        }

        public static Dictionary<string, List<string>> GetPresetNamesGrouped()
        {
            List<HierarchyDesigner_Preset> presetList = Presets;
            Dictionary<string, List<string>> groupedPresets = new()
            {
                { "A-E", new() },
                { "F-J", new() },
                { "K-O", new() },
                { "P-T", new() },
                { "U-Z", new() }
            };

            foreach (HierarchyDesigner_Preset preset in presetList)
            {
                char firstChar = preset.presetName.ToUpper()[0];
                if (firstChar >= 'A' && firstChar <= 'E') { groupedPresets["A-E"].Add(preset.presetName); }
                else if (firstChar >= 'F' && firstChar <= 'J') { groupedPresets["F-J"].Add(preset.presetName); }
                else if (firstChar >= 'K' && firstChar <= 'O') { groupedPresets["K-O"].Add(preset.presetName); }
                else if (firstChar >= 'P' && firstChar <= 'T') { groupedPresets["P-T"].Add(preset.presetName); }
                else if (firstChar >= 'U' && firstChar <= 'Z') { groupedPresets["U-Z"].Add(preset.presetName); }
            }
            return groupedPresets;
        }
        #endregion
    }
}
#endif