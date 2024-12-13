#if UNITY_EDITOR
using System;
using UnityEngine;

namespace Verpha.HierarchyDesigner
{
    internal static class HierarchyDesigner_Shared_Resources
    {
        private static class ResourceNames
        {
            #region Fonts
            internal const string FontBold = "Hierarchy Designer Font Bold";
            internal const string FontRegular = "Hierarchy Designer Font Regular";
            #endregion

            #region Icons
            internal const string IconReset = "Hierarchy Designer Icon Reset";
            internal const string IconTooltip = "Hierarchy Designer Icon Tooltip";
            #endregion

            #region Graphics
            internal const string GraphicsTitleDark = "Hierarchy Designer Graphics Title Dark";
            internal const string GraphicsTitleLight = "Hierarchy Designer Graphics Title Light";
            #endregion

            #region Promotional
            internal const string PromotionalPicEase = "Hierarchy Designer Promotional PicEase";
            internal const string PromotionalPicEaseLite = "Hierarchy Designer Promotional PicEase Lite";
            #endregion
        }

        #region Classes
        internal static class Fonts
        {
            private static readonly Lazy<Font> _bold = new(() => HierarchyDesigner_Shared_Texture.LoadFont(ResourceNames.FontBold));
            public static Font Bold => _bold.Value;

            private static readonly Lazy<Font> _regular = new(() => HierarchyDesigner_Shared_Texture.LoadFont(ResourceNames.FontRegular));
            public static Font Regular => _regular.Value;
        }

        internal static class Icons
        {
            private static readonly Lazy<Texture2D> _resetIcon = new(() => HierarchyDesigner_Shared_Texture.LoadTexture(ResourceNames.IconReset));
            public static Texture2D Reset => _resetIcon.Value;

            private static readonly Lazy<Texture2D> _tooltipIcon = new(() => HierarchyDesigner_Shared_Texture.LoadTexture(ResourceNames.IconTooltip));
            public static Texture2D Tooltip => _tooltipIcon.Value;
        }

        internal static class Graphics
        {
            private static readonly Lazy<Texture2D> _titleDark = new(() => HierarchyDesigner_Shared_Texture.LoadTexture(ResourceNames.GraphicsTitleDark));
            public static Texture2D TitleDark => _titleDark.Value;

            private static readonly Lazy<Texture2D> _titleLight = new(() => HierarchyDesigner_Shared_Texture.LoadTexture(ResourceNames.GraphicsTitleLight));
            public static Texture2D TitleLight => _titleLight.Value;
        }

        internal static class Promotional
        {
            private static readonly Lazy<Texture2D> _picEasePromotionalIcon = new(() => HierarchyDesigner_Shared_Texture.LoadTexture(ResourceNames.PromotionalPicEase));
            public static Texture2D PicEasePromotionalIcon => _picEasePromotionalIcon.Value;

            private static readonly Lazy<Texture2D> _picEaseLitePromotionalIcon = new(() => HierarchyDesigner_Shared_Texture.LoadTexture(ResourceNames.PromotionalPicEaseLite));
            public static Texture2D PicEaseLitePromotionalIcon => _picEaseLitePromotionalIcon.Value;
        }
        #endregion

        #region Old
        #region General
        private static readonly string defaultFontBoldName = "Hierarchy Designer Font Bold";
        private static readonly string defaultFontRegularName = "Hierarchy Designer Font Regular";
        private static readonly string defaultTextureName = "Hierarchy Designer Default Texture";
        private static readonly string lockIconName = "Hierarchy Designer Lock Icon";
        private static readonly string unlockIconName = "Hierarchy Designer Unlock Icon";
        private static readonly string visibilityOnIconName = "Hierarchy Designer Visibility On Icon";
        private static readonly string visibilityOffIconName = "Hierarchy Designer Visibility Off Icon";

        public static readonly Font DefaultFontBold = HierarchyDesigner_Shared_Texture.LoadFont(defaultFontBoldName);
        public static readonly Font DefaultFont = HierarchyDesigner_Shared_Texture.LoadFont(defaultFontRegularName);
        public static readonly Texture2D DefaultTexture = HierarchyDesigner_Shared_Texture.LoadTexture(defaultTextureName);
        public static readonly Texture2D LockIcon = HierarchyDesigner_Shared_Texture.LoadTexture(lockIconName);
        public static readonly Texture2D UnlockIcon = HierarchyDesigner_Shared_Texture.LoadTexture(unlockIconName);
        public static readonly Texture2D VisibilityOnIcon = HierarchyDesigner_Shared_Texture.LoadTexture(visibilityOnIconName);
        public static readonly Texture2D VisibilityOffIcon = HierarchyDesigner_Shared_Texture.LoadTexture(visibilityOffIconName);
        #endregion

        #region Tree Branches
        private static readonly string treeBranchIconDefaultIName = "Hierarchy Designer Tree Branch Icon Default I";
        private static readonly string treeBranchIconDefaultLName = "Hierarchy Designer Tree Branch Icon Default L";
        private static readonly string treeBranchIconDefaultTName = "Hierarchy Designer Tree Branch Icon Default T";
        private static readonly string treeBranchIconDefaultTerminalBudName = "Hierarchy Designer Tree Branch Icon Default Terminal Bud";
        private static readonly string treeBranchIconCurvedIName = "Hierarchy Designer Tree Branch Icon Curved I";
        private static readonly string treeBranchIconCurvedLName = "Hierarchy Designer Tree Branch Icon Curved L";
        private static readonly string treeBranchIconCurvedTName = "Hierarchy Designer Tree Branch Icon Curved T";
        private static readonly string treeBranchIconCurvedTerminalBudName = "Hierarchy Designer Tree Branch Icon Curved Terminal Bud";
        private static readonly string treeBranchIconDottedIName = "Hierarchy Designer Tree Branch Icon Dotted I";
        private static readonly string treeBranchIconDottedLName = "Hierarchy Designer Tree Branch Icon Dotted L";
        private static readonly string treeBranchIconDottedTName = "Hierarchy Designer Tree Branch Icon Dotted T";
        private static readonly string treeBranchIconDottedTerminalBudName = "Hierarchy Designer Tree Branch Icon Dotted Terminal Bud";
        private static readonly string treeBranchIconSegmentedIName = "Hierarchy Designer Tree Branch Icon Segmented I";
        private static readonly string treeBranchIconSegmentedLName = "Hierarchy Designer Tree Branch Icon Segmented L";
        private static readonly string treeBranchIconSegmentedTName = "Hierarchy Designer Tree Branch Icon Segmented T";
        private static readonly string treeBranchIconSegmentedTerminalBudName = "Hierarchy Designer Tree Branch Icon Segmented Terminal Bud";

        public static readonly Texture2D TreeBranchIconDefault_I = HierarchyDesigner_Shared_Texture.LoadTexture(treeBranchIconDefaultIName);
        public static readonly Texture2D TreeBranchIconDefault_L = HierarchyDesigner_Shared_Texture.LoadTexture(treeBranchIconDefaultLName);
        public static readonly Texture2D TreeBranchIconDefault_T = HierarchyDesigner_Shared_Texture.LoadTexture(treeBranchIconDefaultTName);
        public static readonly Texture2D TreeBranchIconDefault_TerminalBud = HierarchyDesigner_Shared_Texture.LoadTexture(treeBranchIconDefaultTerminalBudName);
        public static readonly Texture2D TreeBranchIconCurved_I = HierarchyDesigner_Shared_Texture.LoadTexture(treeBranchIconCurvedIName);
        public static readonly Texture2D TreeBranchIconCurved_L = HierarchyDesigner_Shared_Texture.LoadTexture(treeBranchIconCurvedLName);
        public static readonly Texture2D TreeBranchIconCurved_T = HierarchyDesigner_Shared_Texture.LoadTexture(treeBranchIconCurvedTName);
        public static readonly Texture2D TreeBranchIconCurved_TerminalBud = HierarchyDesigner_Shared_Texture.LoadTexture(treeBranchIconCurvedTerminalBudName);
        public static readonly Texture2D TreeBranchIconDotted_I = HierarchyDesigner_Shared_Texture.LoadTexture(treeBranchIconDottedIName);
        public static readonly Texture2D TreeBranchIconDotted_L = HierarchyDesigner_Shared_Texture.LoadTexture(treeBranchIconDottedLName);
        public static readonly Texture2D TreeBranchIconDotted_T = HierarchyDesigner_Shared_Texture.LoadTexture(treeBranchIconDottedTName);
        public static readonly Texture2D TreeBranchIconDotted_TerminalBud = HierarchyDesigner_Shared_Texture.LoadTexture(treeBranchIconDottedTerminalBudName);
        public static readonly Texture2D TreeBranchIconSegmented_I = HierarchyDesigner_Shared_Texture.LoadTexture(treeBranchIconSegmentedIName);
        public static readonly Texture2D TreeBranchIconSegmented_L = HierarchyDesigner_Shared_Texture.LoadTexture(treeBranchIconSegmentedLName);
        public static readonly Texture2D TreeBranchIconSegmented_T = HierarchyDesigner_Shared_Texture.LoadTexture(treeBranchIconSegmentedTName);
        public static readonly Texture2D TreeBranchIconSegmented_TerminalBud = HierarchyDesigner_Shared_Texture.LoadTexture(treeBranchIconSegmentedTerminalBudName);

        public static Texture2D GetTreeBranchIconI(HierarchyDesigner_Configurable_DesignSettings.TreeBranchImageType imageType)
        {
            return imageType switch
            {
                HierarchyDesigner_Configurable_DesignSettings.TreeBranchImageType.Curved => TreeBranchIconCurved_I,
                HierarchyDesigner_Configurable_DesignSettings.TreeBranchImageType.Dotted => TreeBranchIconDotted_I,
                HierarchyDesigner_Configurable_DesignSettings.TreeBranchImageType.Segmented => TreeBranchIconSegmented_I,
                _ => TreeBranchIconDefault_I,
            };
        }

        public static Texture2D GetTreeBranchIconL(HierarchyDesigner_Configurable_DesignSettings.TreeBranchImageType imageType)
        {
            return imageType switch
            {
                HierarchyDesigner_Configurable_DesignSettings.TreeBranchImageType.Curved => TreeBranchIconCurved_L,
                HierarchyDesigner_Configurable_DesignSettings.TreeBranchImageType.Dotted => TreeBranchIconDotted_L,
                HierarchyDesigner_Configurable_DesignSettings.TreeBranchImageType.Segmented => TreeBranchIconSegmented_L,
                _ => TreeBranchIconDefault_L,
            };
        }

        public static Texture2D GetTreeBranchIconT(HierarchyDesigner_Configurable_DesignSettings.TreeBranchImageType imageType)
        {
            return imageType switch
            {
                HierarchyDesigner_Configurable_DesignSettings.TreeBranchImageType.Curved => TreeBranchIconCurved_T,
                HierarchyDesigner_Configurable_DesignSettings.TreeBranchImageType.Dotted => TreeBranchIconDotted_T,
                HierarchyDesigner_Configurable_DesignSettings.TreeBranchImageType.Segmented => TreeBranchIconSegmented_T,
                _ => TreeBranchIconDefault_T,
            };
        }

        public static Texture2D GetTreeBranchIconTerminalBud(HierarchyDesigner_Configurable_DesignSettings.TreeBranchImageType imageType)
        {
            return imageType switch
            {
                HierarchyDesigner_Configurable_DesignSettings.TreeBranchImageType.Curved => TreeBranchIconCurved_TerminalBud,
                HierarchyDesigner_Configurable_DesignSettings.TreeBranchImageType.Dotted => TreeBranchIconDotted_TerminalBud,
                HierarchyDesigner_Configurable_DesignSettings.TreeBranchImageType.Segmented => TreeBranchIconSegmented_TerminalBud,
                _ => TreeBranchIconDefault_TerminalBud,
            };
        }
        #endregion

        #region Folder Images
        private static readonly string folderDefaultIconName = "Hierarchy Designer Folder Icon Default";
        private static readonly string folderDefaultOutlineIconName = "Hierarchy Designer Folder Icon Default Outline";
        private static readonly string folderModernIIconName = "Hierarchy Designer Folder Icon Modern I";
        private static readonly string folderModernIIIconName = "Hierarchy Designer Folder Icon Modern II";
        private static readonly string folderModernIIIIconName = "Hierarchy Designer Folder Icon Modern III";
        private static readonly string folderModernOutlineIconName = "Hierarchy Designer Folder Icon Modern Outline";
        private static readonly string folderInspectorIconName = "Hierarchy Designer Folder Icon Inspector";

        public static readonly Texture2D FolderDefaultIcon = HierarchyDesigner_Shared_Texture.LoadTexture(folderDefaultIconName);
        public static readonly Texture2D FolderDefaultOutlineIcon = HierarchyDesigner_Shared_Texture.LoadTexture(folderDefaultOutlineIconName);
        public static readonly Texture2D FolderModernIIcon = HierarchyDesigner_Shared_Texture.LoadTexture(folderModernIIconName);
        public static readonly Texture2D FolderModernIIIcon = HierarchyDesigner_Shared_Texture.LoadTexture(folderModernIIIconName);
        public static readonly Texture2D FolderModernIIIIcon = HierarchyDesigner_Shared_Texture.LoadTexture(folderModernIIIIconName);
        public static readonly Texture2D FolderModernOutlineIcon = HierarchyDesigner_Shared_Texture.LoadTexture(folderModernOutlineIconName);
        public static readonly Texture2D FolderInspectorIcon = HierarchyDesigner_Shared_Texture.LoadTexture(folderInspectorIconName);

        public static Texture2D FolderImageType(HierarchyDesigner_Configurable_Folders.FolderImageType folderImageType)
        {
            return folderImageType switch
            {
                HierarchyDesigner_Configurable_Folders.FolderImageType.DefaultOutline => FolderDefaultOutlineIcon,
                HierarchyDesigner_Configurable_Folders.FolderImageType.ModernI => FolderModernIIcon,
                HierarchyDesigner_Configurable_Folders.FolderImageType.ModernII => FolderModernIIIcon,
                HierarchyDesigner_Configurable_Folders.FolderImageType.ModernIII => FolderModernIIIIcon,
                HierarchyDesigner_Configurable_Folders.FolderImageType.ModernOutline => FolderModernOutlineIcon,
                _ => FolderDefaultIcon,
            };
        }
        #endregion

        #region Separator Images
        private static readonly string separatorBackgroundImageDefaultName = "Hierarchy Designer Separator Background Image Default";
        private static readonly string separatorBackgroundImageDefaultFadedBottomName = "Hierarchy Designer Separator Background Image Default Faded Bottom";
        private static readonly string separatorBackgroundImageDefaultFadedLeftName = "Hierarchy Designer Separator Background Image Default Faded Left";
        private static readonly string separatorBackgroundImageDefaultFadedSidewaysName = "Hierarchy Designer Separator Background Image Default Faded Sideways";
        private static readonly string separatorBackgroundImageDefaultFadedRightName = "Hierarchy Designer Separator Background Image Default Faded Right";
        private static readonly string separatorBackgroundImageDefaultFadedTopName = "Hierarchy Designer Separator Background Image Default Faded Top";
        private static readonly string separatorBackgroundImageClassicIName = "Hierarchy Designer Separator Background Image Classic I";
        private static readonly string separatorBackgroundImageClassicIIName = "Hierarchy Designer Separator Background Image Classic II";
        private static readonly string separatorBackgroundImageModernIName = "Hierarchy Designer Separator Background Image Modern I";
        private static readonly string separatorBackgroundImageModernIIName = "Hierarchy Designer Separator Background Image Modern II";
        private static readonly string separatorBackgroundImageModernIIIName = "Hierarchy Designer Separator Background Image Modern III";
        private static readonly string separatorBackgroundImageNeoIName = "Hierarchy Designer Separator Background Image Neo I";
        private static readonly string separatorBackgroundImageNeoIIName = "Hierarchy Designer Separator Background Image Neo II";
        private static readonly string separatorBackgroundImageNextGenIName = "Hierarchy Designer Separator Background Image Next-Gen I";
        private static readonly string separatorBackgroundImageNextGenIIName = "Hierarchy Designer Separator Background Image Next-Gen II";
        private static readonly string separatorInspectorIconName = "Hierarchy Designer Separator Icon Inspector";

        public static readonly Texture2D SeparatorBackgroundImageDefault = HierarchyDesigner_Shared_Texture.LoadTexture(separatorBackgroundImageDefaultName);
        public static readonly Texture2D SeparatorBackgroundImageDefaultFadedBottom = HierarchyDesigner_Shared_Texture.LoadTexture(separatorBackgroundImageDefaultFadedBottomName);
        public static readonly Texture2D SeparatorBackgroundImageDefaultFadedLeft = HierarchyDesigner_Shared_Texture.LoadTexture(separatorBackgroundImageDefaultFadedLeftName);
        public static readonly Texture2D SeparatorBackgroundImageDefaultFadedSideways = HierarchyDesigner_Shared_Texture.LoadTexture(separatorBackgroundImageDefaultFadedSidewaysName);
        public static readonly Texture2D SeparatorBackgroundImageDefaultFadedRight = HierarchyDesigner_Shared_Texture.LoadTexture(separatorBackgroundImageDefaultFadedRightName);
        public static readonly Texture2D SeparatorBackgroundImageDefaultFadedTop = HierarchyDesigner_Shared_Texture.LoadTexture(separatorBackgroundImageDefaultFadedTopName);
        public static readonly Texture2D SeparatorBackgroundImageClassicI = HierarchyDesigner_Shared_Texture.LoadTexture(separatorBackgroundImageClassicIName);
        public static readonly Texture2D SeparatorBackgroundImageClassicII = HierarchyDesigner_Shared_Texture.LoadTexture(separatorBackgroundImageClassicIIName);
        public static readonly Texture2D SeparatorBackgroundImageModernI = HierarchyDesigner_Shared_Texture.LoadTexture(separatorBackgroundImageModernIName);
        public static readonly Texture2D SeparatorBackgroundImageModernII = HierarchyDesigner_Shared_Texture.LoadTexture(separatorBackgroundImageModernIIName);
        public static readonly Texture2D SeparatorBackgroundImageModernIII = HierarchyDesigner_Shared_Texture.LoadTexture(separatorBackgroundImageModernIIIName);
        public static readonly Texture2D SeparatorBackgroundImageNeoI = HierarchyDesigner_Shared_Texture.LoadTexture(separatorBackgroundImageNeoIName);
        public static readonly Texture2D SeparatorBackgroundImageNeoII = HierarchyDesigner_Shared_Texture.LoadTexture(separatorBackgroundImageNeoIIName);
        public static readonly Texture2D SeparatorBackgroundImageNextGenI = HierarchyDesigner_Shared_Texture.LoadTexture(separatorBackgroundImageNextGenIName);
        public static readonly Texture2D SeparatorBackgroundImageNextGenII = HierarchyDesigner_Shared_Texture.LoadTexture(separatorBackgroundImageNextGenIIName);
        public static readonly Texture2D SeparatorInspectorIcon = HierarchyDesigner_Shared_Texture.LoadTexture(separatorInspectorIconName);

        public static Texture2D SeparatorImageType(HierarchyDesigner_Configurable_Separators.SeparatorImageType separatorImageType)
        {
            return separatorImageType switch
            {
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.DefaultFadedBottom => SeparatorBackgroundImageDefaultFadedBottom,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.DefaultFadedLeft => SeparatorBackgroundImageDefaultFadedLeft,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.DefaultFadedSideways => SeparatorBackgroundImageDefaultFadedSideways,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.DefaultFadedRight => SeparatorBackgroundImageDefaultFadedRight,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.DefaultFadedTop => SeparatorBackgroundImageDefaultFadedTop,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.ClassicI => SeparatorBackgroundImageClassicI,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.ClassicII => SeparatorBackgroundImageClassicII,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.ModernI => SeparatorBackgroundImageModernI,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.ModernII => SeparatorBackgroundImageModernII,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.ModernIII => SeparatorBackgroundImageModernIII,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.NeoI => SeparatorBackgroundImageNeoI,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.NeoII => SeparatorBackgroundImageNeoII,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.NextGenI => SeparatorBackgroundImageNextGenI,
                HierarchyDesigner_Configurable_Separators.SeparatorImageType.NextGenII => SeparatorBackgroundImageNextGenII,
                _ => SeparatorBackgroundImageDefault,
            };
        }
        #endregion
        #endregion
    }
}
#endif