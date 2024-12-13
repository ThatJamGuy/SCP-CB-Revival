#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Verpha.HierarchyDesigner
{
    internal class HierarchyDesigner_Window_Main : EditorWindow
    {
        #region Properties
        #region General
        public enum CurrentWindow { Home, About, Folders, Separators, Tools, Presets, GeneralSettings, DesignSettings, ShortcutSettings, AdvancedSettings }
        private static CurrentWindow currentWindow;
        private static string cachedCurrentWindowLabel;
        private Vector2 headerButtonsScroll;
        private Dictionary<string, Action> utilitiesMenuItems;
        private Dictionary<string, Action> configurationsMenuItems;
        private const int primaryButtonsHeight = 30;
        private const int secondaryButtonsHeight = 25;
        private const int defaultMarginSpacing = 5;
        private const float moveItemInListButtonWidth = 25;
        private const float createButtonWidth = 52;
        private const float removeButtonWidth = 60;
        private readonly int[] fontSizeOptions = new int[15];
        #endregion

        #region Home
        private Vector2 homeScroll;
        private string patchNotes = string.Empty;
        private const float titleAspectRatio = 1f;
        private const float titleWidthPercentage = 1f;
        private const float titleMinWidth = 256f;
        private const float titleMaxWidth = 512f;
        private const float titleMinHeight = 128f;
        private const float titleMaxHeight = 400f;
        #endregion

        #region About
        private Vector2 aboutSummaryScroll;
        private Vector2 aboutPatchNotesScroll;
        private Vector2 aboutMyOtherAssetsScroll;

        private const string folderText =
            "Folders are great for organizing multiple GameObjects of the same or similar type (e.g., static environment objects, reflection probes, and so on).\n\n" +
            "Folders have a script called 'Hierarchy Designer Folder' with a main variable, 'Flatten Folder.'" +
            "If 'Flatten Folder' is true, in the Flatten Event (Awake or Start method), the folder will FREE all GameObject children, and once that is complete, it will destroy the folder.\n";

        private const string separatorText =
            "Separators are visual dividers; they are meant to organize your scenes and provide clarity.\n\n" +
            "Separators are editor-only and will NOT be included in your game's build. Therefore, do not use them as GameObject parents; instead, use folders.\n";

        private const string additionalNotesText =
            "Hierarchy Designer is currently in development, and more features and improvements are coming soon.\n\n" +
            "Hierarchy Designer is an Editor-Only tool (with the exception of the Hierarchy Designer Folder script) and will not affect your build or game.\n" +
            "Like most editor tool, it will slightly affect performance (EDITOR ONLY). Disabling features you don't use or setting their update values to 'smart' will greatly improve performance, especially in larger scenes.\n\n" +
            "If you have any questions or would like to report a bug, you may email me at: VerphaSuporte@outlook.com.\n\nIf you like Hierarchy Designer, please rate it on the Store.";
        #endregion

        #region Folders
        private Vector2 folderMainScroll;
        private Vector2 foldersListScroll;
        private const float folderCreationLabelWidth = 110;
        private Dictionary<string, HierarchyDesigner_Configurable_Folders.HierarchyDesigner_FolderData> tempFolders;
        private List<string> foldersOrder;
        private string newFolderName = "";
        private Color newFolderTextColor = HierarchyDesigner_Configurable_DesignSettings.FolderDefaultTextColor;
        private int newFolderFontSize = HierarchyDesigner_Configurable_DesignSettings.FolderDefaultFontSize;
        private FontStyle newFolderFontStyle = HierarchyDesigner_Configurable_DesignSettings.FolderDefaultFontStyle;
        private Color newFolderIconColor = HierarchyDesigner_Configurable_DesignSettings.FolderDefaultImageColor;
        private HierarchyDesigner_Configurable_Folders.FolderImageType newFolderImageType = HierarchyDesigner_Configurable_DesignSettings.FolderDefaultImageType;
        private bool folderHasModifiedChanges = false;
        private Color tempFolderGlobalTextColor = Color.white;
        private int tempFolderGlobalFontSize = 12;
        private FontStyle tempFolderGlobalFontStyle = FontStyle.Normal;
        private Color tempFolderGlobalIconColor = Color.white;
        private HierarchyDesigner_Configurable_Folders.FolderImageType tempGlobalFolderImageType = HierarchyDesigner_Configurable_Folders.FolderImageType.Default;
        #endregion

        #region Separators
        private Vector2 separatorMainScroll;
        private Vector2 separatorsListScroll;
        private const float separatorCreationLabelWidth = 160;
        private Dictionary<string, HierarchyDesigner_Configurable_Separators.HierarchyDesigner_SeparatorData> tempSeparators;
        private List<string> separatorsOrder;
        private bool separatorHasModifiedChanges = false;
        private string newSeparatorName = "";
        private Color newSeparatorTextColor = HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultTextColor;
        private bool newSeparatorIsGradient = HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultIsGradientBackground;
        private Color newSeparatorBackgroundColor = HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultBackgroundColor;
        private Gradient newSeparatorBackgroundGradient = HierarchyDesigner_Shared_Color.CopyGradient(HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultBackgroundGradient);
        private int newSeparatorFontSize = HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultFontSize;
        private FontStyle newSeparatorFontStyle = HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultFontStyle;
        private TextAnchor newSeparatorTextAnchor = HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultTextAnchor;
        private HierarchyDesigner_Configurable_Separators.SeparatorImageType newSeparatorImageType = HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultImageType;
        private Color tempSeparatorGlobalTextColor = Color.white;
        private bool tempSeparatorGlobalIsGradient = false;
        private Color tempSeparatorGlobalBackgroundColor = Color.gray;
        private Gradient tempSeparatorGlobalBackgroundGradient = new();
        private int tempSeparatorGlobalFontSize = 12;
        private FontStyle tempSeparatorGlobalFontStyle = FontStyle.Normal;
        private TextAnchor tempSeparatorGlobalTextAnchor = TextAnchor.MiddleCenter;
        private HierarchyDesigner_Configurable_Separators.SeparatorImageType tempSeparatorGlobalImageType = HierarchyDesigner_Configurable_Separators.SeparatorImageType.Default;
        #endregion

        #region Tools
        private Vector2 toolsMainScroll;
        private const float labelWidth = 80;
        private HierarchyDesigner_Attribute_Tools selectedCategory = HierarchyDesigner_Attribute_Tools.Activate;
        private int selectedActionIndex = 0;
        private readonly List<string> availableActionNames = new();
        private readonly List<MethodInfo> availableActionMethods = new();
        private static readonly Dictionary<HierarchyDesigner_Attribute_Tools, List<(string Name, MethodInfo Method)>> cachedActions = new();
        private static bool cacheInitialized = false;
        #endregion

        #region Presets
        private Vector2 presetsMainScroll;
        private const float presetslabelWidth = 130;
        private const float presetsToggleLabelWidth = 205;
        private int selectedPresetIndex = 0;
        private string[] presetNames;
        private bool applyToFolders = true;
        private bool applyToSeparators = true;
        private bool applyToTag = true;
        private bool applyToLayer = true;
        private bool applyToTree = true;
        private bool applyToLines = true;
        private bool applyToFolderDefaultValues = true;
        private bool applyToSeparatorDefaultValues = true;
        private bool applyToLock = true;
        #endregion

        #region General Settings
        private Vector2 generalSettingsMainScroll;
        private const float enumPopupLabelWidth = 190;
        private const float generalSettingsMainToggleLabelWidth = 360;
        private const float generalSettingsFilterToggleLabelWidth = 300;
        private const float maskFieldLabelWidth = 145;
        private HierarchyDesigner_Configurable_GeneralSettings.HierarchyLayoutMode tempLayoutMode;
        private HierarchyDesigner_Configurable_GeneralSettings.HierarchyTreeMode tempTreeMode;
        private bool tempEnableGameObjectMainIcon;
        private bool tempEnableGameObjectComponentIcons;
        private bool tempEnableHierarchyTree;
        private bool tempEnableGameObjectTag;
        private bool tempEnableGameObjectLayer;
        private bool tempEnableHierarchyRows;
        private bool tempEnableHierarchyLines;
        private bool tempEnableHierarchyButtons;
        private bool tempEnableMajorShortcuts;
        private bool tempDisableHierarchyDesignerDuringPlayMode;
        private bool tempExcludeFolderProperties;
        private bool tempExcludeTransformForGameObjectComponentIcons;
        private bool tempExcludeCanvasRendererForGameObjectComponentIcons;
        private int tempMaximumComponentIconsAmount;
        private List<string> tempExcludedTags;
        private List<string> tempExcludedLayers;
        private static bool generalSettingsHasModifiedChanges = false;
        #endregion

        #region Design Settings
        private Vector2 designSettingsMainScroll;
        private const float designSettingslabelWidth = 260;
        private float tempComponentIconsSize;
        private int tempComponentIconsOffset;
        private float tempComponentIconsSpacing;
        private Color tempHierarchyTreeColor;
        private HierarchyDesigner_Configurable_DesignSettings.TreeBranchImageType tempTreeBranchImageType_I;
        private HierarchyDesigner_Configurable_DesignSettings.TreeBranchImageType tempTreeBranchImageType_L;
        private HierarchyDesigner_Configurable_DesignSettings.TreeBranchImageType tempTreeBranchImageType_T;
        private HierarchyDesigner_Configurable_DesignSettings.TreeBranchImageType tempTreeBranchImageType_TerminalBud;
        private Color tempTagColor;
        private TextAnchor tempTagTextAnchor;
        private FontStyle tempTagFontStyle;
        private int tempTagFontSize;
        private Color tempLayerColor;
        private TextAnchor tempLayerTextAnchor;
        private FontStyle tempLayerFontStyle;
        private int tempLayerFontSize;
        private int tempTagLayerOffset;
        private int tempTagLayerSpacing;
        private Color tempHierarchyLineColor;
        private int tempHierarchyLineThickness;
        private Color tempFolderDefaultTextColor;
        private int tempFolderDefaultFontSize;
        private FontStyle tempFolderDefaultFontStyle;
        private Color tempFolderDefaultImageColor;
        private HierarchyDesigner_Configurable_Folders.FolderImageType tempFolderDefaultImageType;
        private Color tempSeparatorDefaultTextColor;
        private bool tempSeparatorDefaultIsGradientBackground;
        private Color tempSeparatorDefaultBackgroundColor;
        private Gradient tempSeparatorDefaultBackgroundGradient;
        private int tempSeparatorDefaultFontSize;
        private FontStyle tempSeparatorDefaultFontStyle;
        private TextAnchor tempSeparatorDefaultTextAnchor;
        private HierarchyDesigner_Configurable_Separators.SeparatorImageType tempSeparatorDefaultImageType;
        private int tempSeparatorLeftSideTextAnchorOffset;
        private int tempSeparatorRightSideTextAnchorOffset;
        private Color tempLockColor;
        private TextAnchor tempLockTextAnchor;
        private FontStyle tempLockFontStyle;
        private int tempLockFontSize;
        private static bool designSettingsHasModifiedChanges = false;
        #endregion

        #region Shortcut Settings
        private Vector2 shortcutSettingsMainScroll;
        private Vector2 minorShortcutSettingsScroll;
        private const float majorShortcutEnumToggleLabelWidth = 340;
        private const float minorShortcutCommandLabelWidth = 200;
        private const float minorShortcutLabelWidth = 400;
        private readonly List<string> minorShortcutIdentifiers = new()
        {
            "Hierarchy Designer/Open Hierarchy Designer Window",
            "Hierarchy Designer/Open Folder Panel",
            "Hierarchy Designer/Open Separator Panel",
            "Hierarchy Designer/Open Tools Panel",
            "Hierarchy Designer/Open Presets Panel",
            "Hierarchy Designer/Open General Settings Panel",
            "Hierarchy Designer/Open Design Settings Panel",
            "Hierarchy Designer/Open Shortcut Settings Panel",
            "Hierarchy Designer/Open Advanced Settings Panel",
            "Hierarchy Designer/Open Rename Tool Window",
            "Hierarchy Designer/Create All Folders",
            "Hierarchy Designer/Create Default Folder",
            "Hierarchy Designer/Create Missing Folders",
            "Hierarchy Designer/Create All Separators",
            "Hierarchy Designer/Create Default Separator",
            "Hierarchy Designer/Create Missing Separators",
            "Hierarchy Designer/Refresh All GameObjects' Data",
            "Hierarchy Designer/Refresh Selected GameObject's Data",
            "Hierarchy Designer/Refresh Selected Main Icon",
            "Hierarchy Designer/Refresh Selected Component Icons",
            "Hierarchy Designer/Refresh Selected Hierarchy Tree Icon",
            "Hierarchy Designer/Refresh Selected Tag",
            "Hierarchy Designer/Refresh Selected Layer",
        };
        private KeyCode tempToggleGameObjectActiveStateKeyCode;
        private KeyCode tempToggleLockStateKeyCode;
        private KeyCode tempChangeTagLayerKeyCode;
        private KeyCode tempRenameSelectedGameObjectsKeyCode;
        private static bool shortcutSettingsHasModifiedChanges = false;
        #endregion

        #region Advanced Settings
        private Vector2 advancedSettingsMainScroll;
        private const float advancedSettingsEnumPopupLabelWidth = 250;
        private const float advancedSettingsToggleLabelWidth = 480;
        private HierarchyDesigner_Configurable_AdvancedSettings.HierarchyDesignerLocation tempHierarchyLocation;
        private HierarchyDesigner_Configurable_AdvancedSettings.UpdateMode tempMainIconUpdateMode;
        private HierarchyDesigner_Configurable_AdvancedSettings.UpdateMode tempComponentsIconsUpdateMode;
        private HierarchyDesigner_Configurable_AdvancedSettings.UpdateMode tempHierarchyTreeUpdateMode;
        private HierarchyDesigner_Configurable_AdvancedSettings.UpdateMode tempTagUpdateMode;
        private HierarchyDesigner_Configurable_AdvancedSettings.UpdateMode tempLayerUpdateMode;
        private bool tempEnableDynamicBackgroundForGameObjectMainIcon;
        private bool tempEnablePreciseRectForDynamicBackgroundForGameObjectMainIcon;
        private bool tempEnableCustomizationForGameObjectComponentIcons;
        private bool tempEnableTooltipOnComponentIconHovered;
        private bool tempEnableActiveStateEffectForComponentIcons;
        private bool tempDisableComponentIconsForInactiveGameObjects;
        private bool tempIncludeEditorUtilitiesForHierarchyDesignerRuntimeFolder;
        private bool tempIncludeBackgroundImageForGradientBackground;
        private bool tempExcludeFoldersFromCountSelectToolCalculations;
        private bool tempExcludeSeparatorsFromCountSelectToolCalculations;
        private static bool advancedSettingsHasModifiedChanges = false;
        #endregion
        #endregion

        #region Initialization
        public static void OpenWindow()
        {
            HierarchyDesigner_Window_Main editorWindow = GetWindow<HierarchyDesigner_Window_Main>(HierarchyDesigner_Shared_Constants.AssetName);
            editorWindow.minSize = new(500, 400);
            UpdateCurrentWindowLabel();
        }

        private void OnEnable()
        {
            InitializeMenus();
            InitializeFontSizeOptions();
            LoadSessionData();
            LoadFolderData();
            LoadSeparatorData();
            LoadTools();
            LoadPresets();
            LoadGeneralSettingsData();
            LoadDesignSettingsData();
            LoadShortcutSettingsData();
            LoadAdvancedSettingsData();
        }

        private void InitializeMenus()
        {
            if (utilitiesMenuItems == null)
            {
                utilitiesMenuItems = new()
                {
                    { "Tools", () => { SelectToolsWindow(); } },
                    { "Presets", () => { SelectPresetsWindow(); } }
                };
            }

            if (configurationsMenuItems == null)
            {
                configurationsMenuItems = new()
                {
                    { "General Settings", () => { SelectGeneralSettingsWindow(); } },
                    { "Design Settings", () => { SelectDesignSettingsWindow(); } },
                    { "Shortcut Settings", () => { SelectShortcutSettingsWindow(); } },
                    { "Advanced Settings", () => { SelectAdvancedSettingsWindow(); } }
                };
            }
        }

        private void InitializeFontSizeOptions()
        {
            for (int i = 0; i < fontSizeOptions.Length; i++)
            {
                fontSizeOptions[i] = 7 + i;
            }
        }

        private void LoadSessionData()
        {
            if (!HierarchyDesigner_Manager_Session.instance.IsPatchNotesLoaded)
            {
                patchNotes = HierarchyDesigner_Shared_File.GetPatchNotesData();
                HierarchyDesigner_Manager_Session.instance.PatchNotesContent = patchNotes;
                HierarchyDesigner_Manager_Session.instance.IsPatchNotesLoaded = true;
            }
            else
            {
                patchNotes = HierarchyDesigner_Manager_Session.instance.PatchNotesContent;
            }

            currentWindow = HierarchyDesigner_Manager_Session.instance.currentWindow;
        }
        #endregion

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.PrimaryPanelStyle);

            if (currentWindow != CurrentWindow.Home)
            {
                EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.HeaderPanelStyle);
                headerButtonsScroll = EditorGUILayout.BeginScrollView(headerButtonsScroll, GUI.skin.horizontalScrollbar, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Hierarchy <color=#{(HierarchyDesigner_Manager_Editor.IsProSkin ? "67F758" : "50C044")}>Designer</color>", HierarchyDesigner_Shared_GUI.HeaderLabelLeftStyle, GUILayout.Width(220));
                GUILayout.FlexibleSpace();
                DrawHeaderButtons();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndScrollView();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"▷ {cachedCurrentWindowLabel}", HierarchyDesigner_Shared_GUI.TabLabelStyle);
                GUILayout.FlexibleSpace();
                GUILayout.Label($"{HierarchyDesigner_Shared_Constants.AssetVersion}", HierarchyDesigner_Shared_GUI.VersionLabelHeaderStyle, GUILayout.Height(20));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }

            #region Body
            switch (currentWindow)
            {
                case CurrentWindow.Home:
                    DrawHomeTab();
                    break;
                case CurrentWindow.About:
                    DrawAboutPanel();
                    break;
                case CurrentWindow.Folders:
                    DrawFoldersTab();
                    break;
                case CurrentWindow.Separators:
                    DrawSeparatorsTab();
                    break;
                case CurrentWindow.Tools:
                    DrawToolsTab();
                    break;
                case CurrentWindow.Presets:
                    DrawPresetsTab();
                    break;
                case CurrentWindow.GeneralSettings:
                    DrawGeneralSettingsTab();
                    break;
                case CurrentWindow.DesignSettings:
                    DrawDesignSettingsTab();
                    break;
                case CurrentWindow.ShortcutSettings:
                    DrawShortcutSettingsTab();
                    break;
                case CurrentWindow.AdvancedSettings:
                    DrawAdvancedSettingsTab();
                    break;
            }
            #endregion

            EditorGUILayout.EndVertical();
        }

        #region Methods
        #region General
        private void DrawHeaderButtons()
        {
            if (GUILayout.Button("FOLDERS", HierarchyDesigner_Shared_GUI.HeaderButtonStyle, GUILayout.Height(primaryButtonsHeight)))
            {
                SelectFoldersWindow();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("SEPARATORS", HierarchyDesigner_Shared_GUI.HeaderButtonStyle, GUILayout.Height(primaryButtonsHeight)))
            {
                SelectSeparatorsWindow();
            }

            
            GUILayout.Label("│", HierarchyDesigner_Shared_GUI.DivisorLabelStyle, GUILayout.Width(15), GUILayout.Height(primaryButtonsHeight));

            if (GUILayout.Button("HOME", HierarchyDesigner_Shared_GUI.HeaderButtonStyle, GUILayout.Height(primaryButtonsHeight)))
            {
                SelectHomeWindow();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("ABOUT", HierarchyDesigner_Shared_GUI.HeaderButtonStyle, GUILayout.Height(primaryButtonsHeight)))
            {
                SelectAboutWindow();
            }

            GUILayout.Label("│", HierarchyDesigner_Shared_GUI.DivisorLabelStyle, GUILayout.Width(15), GUILayout.Height(primaryButtonsHeight));

            HierarchyDesigner_Shared_GUI.DrawPopupButton("UTILITIES ▾", HierarchyDesigner_Shared_GUI.HeaderButtonStyle, primaryButtonsHeight, utilitiesMenuItems);
            
            GUILayout.Space(8);

            HierarchyDesigner_Shared_GUI.DrawPopupButton("CONFIGURATIONS ▾", HierarchyDesigner_Shared_GUI.HeaderButtonStyle, primaryButtonsHeight, configurationsMenuItems);
        }

        private void SelectFoldersWindow()
        {
            SwitchWindow(CurrentWindow.Folders);
        }

        private void SelectSeparatorsWindow()
        {
            SwitchWindow(CurrentWindow.Separators);
        }

        private void SelectHomeWindow()
        {
            SwitchWindow(CurrentWindow.Home);
        }

        private void SelectAboutWindow()
        {
            SwitchWindow(CurrentWindow.About);
        }

        private void SelectToolsWindow()
        {
            SwitchWindow(CurrentWindow.Tools);
        }

        private void SelectPresetsWindow()
        {
            SwitchWindow(CurrentWindow.Presets);
        }

        private void SelectGeneralSettingsWindow()
        {
            SwitchWindow(CurrentWindow.GeneralSettings);
        }

        private void SelectDesignSettingsWindow()
        {
            SwitchWindow(CurrentWindow.DesignSettings);
        }

        private void SelectShortcutSettingsWindow()
        {
            SwitchWindow(CurrentWindow.ShortcutSettings);
        }

        private void SelectAdvancedSettingsWindow()
        {
            SwitchWindow(CurrentWindow.AdvancedSettings);
        }

        public static void SwitchWindow(CurrentWindow newWindow, Action extraAction = null)
        {
            if (currentWindow == newWindow) return;

            extraAction?.Invoke();
            currentWindow = newWindow;
            HierarchyDesigner_Manager_Session.instance.currentWindow = currentWindow;
            UpdateCurrentWindowLabel();
        }

        private static void UpdateCurrentWindowLabel()
        {
            string name = currentWindow.ToString();
            string correctedName = System.Text.RegularExpressions.Regex.Replace(name, "([a-z])([A-Z])", "$1 $2");
            cachedCurrentWindowLabel = correctedName.ToUpper();
        }
        #endregion

        #region Home
        private void DrawHomeTab()
        {
            homeScroll = EditorGUILayout.BeginScrollView(homeScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUILayout.FlexibleSpace();

            DrawTitle();
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            DrawButtons();
            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Space(40);
            DrawAssetVersion();

            EditorGUILayout.EndScrollView();
        }

        private void DrawTitle()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            float labelWidth = position.width * titleWidthPercentage;
            float labelHeight = labelWidth * titleAspectRatio;

            labelWidth = Mathf.Clamp(labelWidth, titleMinWidth, titleMaxWidth);
            labelHeight = Mathf.Clamp(labelHeight, titleMinHeight, titleMaxHeight);

            GUILayout.Label(HierarchyDesigner_Manager_Editor.IsProSkin ? HierarchyDesigner_Shared_Resources.Graphics.TitleDark : HierarchyDesigner_Shared_Resources.Graphics.TitleLight, HierarchyDesigner_Shared_GUI.TitleLabelStyle, GUILayout.Width(labelWidth), GUILayout.Height(labelHeight));

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawButtons()
        {
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("FOLDERS", HierarchyDesigner_Shared_GUI.PrimaryButtonStyle, GUILayout.Height(primaryButtonsHeight)))
            {
                SwitchWindow(CurrentWindow.Folders);
            }

            GUILayout.Space(15);

            if (GUILayout.Button("SEPARATORS", HierarchyDesigner_Shared_GUI.PrimaryButtonStyle, GUILayout.Height(primaryButtonsHeight)))
            {
                SwitchWindow(CurrentWindow.Separators);
            }

            GUILayout.Space(15);

            if (GUILayout.Button("HOME", HierarchyDesigner_Shared_GUI.PrimaryButtonStyle, GUILayout.Height(primaryButtonsHeight)))
            {
                SwitchWindow(CurrentWindow.Home);
            }

            GUILayout.Space(15);

            if (GUILayout.Button("ABOUT", HierarchyDesigner_Shared_GUI.PrimaryButtonStyle, GUILayout.Height(primaryButtonsHeight)))
            {
                SwitchWindow(CurrentWindow.About);
            }

            GUILayout.Space(13);

            HierarchyDesigner_Shared_GUI.DrawPopupButton("UTILITIES ▾", HierarchyDesigner_Shared_GUI.PrimaryButtonStyle, primaryButtonsHeight, utilitiesMenuItems);

            GUILayout.Space(9);

            HierarchyDesigner_Shared_GUI.DrawPopupButton("CONFIGURATIONS ▾", HierarchyDesigner_Shared_GUI.PrimaryButtonStyle, primaryButtonsHeight, configurationsMenuItems);

            GUILayout.FlexibleSpace();
        }

        private void DrawAssetVersion()
        {
            GUILayout.Label($"{HierarchyDesigner_Shared_Constants.AssetVersion}", HierarchyDesigner_Shared_GUI.FooterLabelStyle, GUILayout.Height(20));
        }
        #endregion

        #region About
        private void DrawAboutPanel()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            DrawSummary();
            EditorGUILayout.BeginVertical();
            DrawPatchNotes();
            DrawMyOtherAssets();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawSummary()
        {
            EditorGUILayout.BeginHorizontal(HierarchyDesigner_Shared_GUI.SecondaryPanelStyle);
            aboutSummaryScroll = EditorGUILayout.BeginScrollView(aboutSummaryScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            GUILayout.Label("Features Breakdown", HierarchyDesigner_Shared_GUI.FieldsCategoryLabelStyle);

            GUILayout.Label("Folders", HierarchyDesigner_Shared_GUI.MiniBoldLabelStyle);
            GUILayout.Label(folderText, HierarchyDesigner_Shared_GUI.RegularLabelStyle);

            GUILayout.Label("Separators", HierarchyDesigner_Shared_GUI.MiniBoldLabelStyle);
            GUILayout.Label(separatorText, HierarchyDesigner_Shared_GUI.RegularLabelStyle);

            GUILayout.Label("Additional Notes", HierarchyDesigner_Shared_GUI.MiniBoldLabelStyle);
            GUILayout.Label(additionalNotesText, HierarchyDesigner_Shared_GUI.RegularLabelStyle);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPatchNotes()
        {
            EditorGUILayout.BeginHorizontal(HierarchyDesigner_Shared_GUI.SecondaryPanelStyle);
            aboutPatchNotesScroll = EditorGUILayout.BeginScrollView(aboutPatchNotesScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUILayout.Label("Patch Notes", HierarchyDesigner_Shared_GUI.FieldsCategoryLabelStyle);
            GUILayout.Label(patchNotes, HierarchyDesigner_Shared_GUI.RegularLabelStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawMyOtherAssets()
        {
            EditorGUILayout.BeginHorizontal(HierarchyDesigner_Shared_GUI.SecondaryPanelStyle);
            aboutMyOtherAssetsScroll = EditorGUILayout.BeginScrollView(aboutMyOtherAssetsScroll, GUILayout.MinHeight(170), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUILayout.Label("My Other Assets", HierarchyDesigner_Shared_GUI.FieldsCategoryCenterLabelStyle);

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();

            #region PicEase
            EditorGUILayout.BeginVertical(GUILayout.Width(200));
            GUILayout.Label("PicEase", HierarchyDesigner_Shared_GUI.MiniBoldLabelCenterStyle);
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(string.Empty, HierarchyDesigner_Shared_GUI.PromotionalPicEaseStyle))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/tools/utilities/picease-297051");
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
            GUILayout.Label("An image editor and screenshot tool.", HierarchyDesigner_Shared_GUI.RegularLabelCenterStyle);
            EditorGUILayout.EndVertical();
            #endregion

            GUILayout.Space(20);

            #region PicEase Lite
            EditorGUILayout.BeginVertical(GUILayout.Width(200));
            GUILayout.Label("PicEase Lite", HierarchyDesigner_Shared_GUI.MiniBoldLabelCenterStyle);
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(string.Empty, HierarchyDesigner_Shared_GUI.PromotionalPicEaseLiteStyle))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/tools/utilities/picease-lite-302896");
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
            GUILayout.Label("The free, simplified version of PicEase.", HierarchyDesigner_Shared_GUI.RegularLabelCenterStyle);
            EditorGUILayout.EndVertical();
            #endregion

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
        }
        #endregion

        #region Folders
        private void DrawFoldersTab()
        {
            #region Body
            folderMainScroll = EditorGUILayout.BeginScrollView(folderMainScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            DrawFoldersCreationFields();
            if (tempFolders.Count > 0)
            {
                DrawFoldersGlobalFields();
                DrawFoldersList();
            }
            else
            {
                EditorGUILayout.LabelField("No folders found. Please create a new folder.", HierarchyDesigner_Shared_GUI.UnassignedLabelStyle);
            }
            EditorGUILayout.EndScrollView();
            #endregion

            #region Footer
            if (GUILayout.Button("Update and Save Folders", GUILayout.Height(primaryButtonsHeight)))
            {
                UpdateAndSaveFoldersData();
            }
            #endregion
        }

        private void DrawFoldersCreationFields()
        {
            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Folder Creation", HierarchyDesigner_Shared_GUI.FieldsCategoryLabelStyle);
            GUILayout.Space(defaultMarginSpacing);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name", HierarchyDesigner_Shared_GUI.LayoutLabelStyle, GUILayout.Width(folderCreationLabelWidth));
            newFolderName = EditorGUILayout.TextField(newFolderName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Text Color", HierarchyDesigner_Shared_GUI.LayoutLabelStyle, GUILayout.Width(folderCreationLabelWidth));
            newFolderTextColor = EditorGUILayout.ColorField(newFolderTextColor);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            string[] newFontSizeOptionsStrings = Array.ConvertAll(fontSizeOptions, x => x.ToString());
            int newFontSizeIndex = Array.IndexOf(fontSizeOptions, newFolderFontSize);
            EditorGUILayout.LabelField("Font Size", HierarchyDesigner_Shared_GUI.LayoutLabelStyle, GUILayout.Width(folderCreationLabelWidth));
            newFolderFontSize = fontSizeOptions[EditorGUILayout.Popup(newFontSizeIndex, newFontSizeOptionsStrings)];
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Font Style", HierarchyDesigner_Shared_GUI.LayoutLabelStyle, GUILayout.Width(folderCreationLabelWidth));
            newFolderFontStyle = (FontStyle)EditorGUILayout.EnumPopup(newFolderFontStyle);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Image Color", HierarchyDesigner_Shared_GUI.LayoutLabelStyle, GUILayout.Width(folderCreationLabelWidth));
            newFolderIconColor = EditorGUILayout.ColorField(newFolderIconColor);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Image Type", HierarchyDesigner_Shared_GUI.LayoutLabelStyle, GUILayout.Width(folderCreationLabelWidth));
            if (GUILayout.Button(HierarchyDesigner_Configurable_Folders.GetFolderImageTypeDisplayName(newFolderImageType), EditorStyles.popup))
            {
                ShowFolderImageTypePopup();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            if (GUILayout.Button("Create Folder", GUILayout.Height(secondaryButtonsHeight)))
            {
                if (IsFolderNameValid(newFolderName))
                {
                    CreateFolder(newFolderName, newFolderTextColor, newFolderFontSize, newFolderFontStyle, newFolderIconColor, newFolderImageType);
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Folder Name", "Folder name is either duplicate or invalid.", "OK");
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawFoldersGlobalFields()
        {
            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Folders' Global Fields", HierarchyDesigner_Shared_GUI.FieldsCategoryLabelStyle);
            GUILayout.Space(defaultMarginSpacing);

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            tempFolderGlobalTextColor = EditorGUILayout.ColorField(tempFolderGlobalTextColor, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
            if (EditorGUI.EndChangeCheck()) { UpdateGlobalFolderTextColor(tempFolderGlobalTextColor); }
            EditorGUI.BeginChangeCheck();
            string[] tempFontSizeOptionsStrings = Array.ConvertAll(fontSizeOptions, x => x.ToString());
            int tempFontSizeIndex = Array.IndexOf(fontSizeOptions, tempFolderGlobalFontSize);
            tempFolderGlobalFontSize = fontSizeOptions[EditorGUILayout.Popup(tempFontSizeIndex, tempFontSizeOptionsStrings, GUILayout.Width(50))];
            if (EditorGUI.EndChangeCheck()) { UpdateGlobalFolderFontSize(tempFolderGlobalFontSize); }
            EditorGUI.BeginChangeCheck();
            tempFolderGlobalFontStyle = (FontStyle)EditorGUILayout.EnumPopup(tempFolderGlobalFontStyle, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
            if (EditorGUI.EndChangeCheck()) { UpdateGlobalFolderFontStyle(tempFolderGlobalFontStyle); }
            EditorGUI.BeginChangeCheck();
            tempFolderGlobalIconColor = EditorGUILayout.ColorField(tempFolderGlobalIconColor, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
            if (EditorGUI.EndChangeCheck()) { UpdateGlobalFolderIconColor(tempFolderGlobalIconColor); }
            if (GUILayout.Button(HierarchyDesigner_Configurable_Folders.GetFolderImageTypeDisplayName(tempGlobalFolderImageType), EditorStyles.popup, GUILayout.MinWidth(125), GUILayout.ExpandWidth(true))) { ShowFolderImageTypePopupGlobal(); }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawFoldersList()
        {
            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Folders' List", HierarchyDesigner_Shared_GUI.FieldsCategoryLabelStyle);
            GUILayout.Space(defaultMarginSpacing);

            foldersListScroll = EditorGUILayout.BeginScrollView(foldersListScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            int index = 1;
            for (int i = 0; i < foldersOrder.Count; i++)
            {
                string key = foldersOrder[i];
                DrawFolders(index, key, tempFolders[key], i, foldersOrder.Count);
                index++;
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

        }

        private void UpdateAndSaveFoldersData()
        {
            HierarchyDesigner_Configurable_Folders.ApplyChangesToFolders(tempFolders, foldersOrder);
            HierarchyDesigner_Configurable_Folders.SaveSettings();
            HierarchyDesigner_Manager_GameObject.ClearFolderCache();
            folderHasModifiedChanges = false;
        }

        private void LoadFolderData()
        {
            tempFolders = HierarchyDesigner_Configurable_Folders.GetAllFoldersData(true);
            foldersOrder = new List<string>(tempFolders.Keys);
        }

        private void LoadFolderCreationFields()
        {
            newFolderTextColor = HierarchyDesigner_Configurable_DesignSettings.FolderDefaultTextColor;
            newFolderFontSize = HierarchyDesigner_Configurable_DesignSettings.FolderDefaultFontSize;
            newFolderFontStyle = HierarchyDesigner_Configurable_DesignSettings.FolderDefaultFontStyle;
            newFolderIconColor = HierarchyDesigner_Configurable_DesignSettings.FolderDefaultImageColor;
            HierarchyDesigner_Configurable_Folders.FolderImageType newFolderImageType = HierarchyDesigner_Configurable_DesignSettings.FolderDefaultImageType;
        }

        private bool IsFolderNameValid(string folderName)
        {
            return !string.IsNullOrEmpty(folderName) && !tempFolders.TryGetValue(folderName, out _);
        }

        private void CreateFolder(string folderName, Color textColor, int fontSize, FontStyle fontStyle, Color ImageColor, HierarchyDesigner_Configurable_Folders.FolderImageType imageType)
        {
            HierarchyDesigner_Configurable_Folders.HierarchyDesigner_FolderData newFolderData = new HierarchyDesigner_Configurable_Folders.HierarchyDesigner_FolderData
            {
                Name = folderName,
                TextColor = textColor,
                FontSize = fontSize,
                FontStyle = fontStyle,
                ImageColor = ImageColor,
                ImageType = imageType
            };
            tempFolders[folderName] = newFolderData;
            foldersOrder.Add(folderName);
            newFolderName = "";
            newFolderTextColor = HierarchyDesigner_Configurable_DesignSettings.FolderDefaultTextColor;
            newFolderFontSize = HierarchyDesigner_Configurable_DesignSettings.FolderDefaultFontSize;
            newFolderFontStyle = HierarchyDesigner_Configurable_DesignSettings.FolderDefaultFontStyle;
            newFolderIconColor = HierarchyDesigner_Configurable_DesignSettings.FolderDefaultImageColor;
            newFolderImageType = HierarchyDesigner_Configurable_DesignSettings.FolderDefaultImageType;
            folderHasModifiedChanges = true;
            GUI.FocusControl(null);
        }

        private void DrawFolders(int index, string key, HierarchyDesigner_Configurable_Folders.HierarchyDesigner_FolderData folderData, int position, int totalItems)
        {
            float folderLabelWidth = HierarchyDesigner_Shared_GUI.CalculateMaxLabelWidth(tempFolders.Keys);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{index}) {folderData.Name}", HierarchyDesigner_Shared_GUI.LayoutLabelStyle, GUILayout.Width(folderLabelWidth));
            EditorGUI.BeginChangeCheck();
            folderData.TextColor = EditorGUILayout.ColorField(folderData.TextColor, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
            string[] fontSizeOptionsStrings = Array.ConvertAll(fontSizeOptions, x => x.ToString());
            int fontSizeIndex = Array.IndexOf(fontSizeOptions, folderData.FontSize);
            if (fontSizeIndex == -1) { fontSizeIndex = 5; }
            folderData.FontSize = fontSizeOptions[EditorGUILayout.Popup(fontSizeIndex, fontSizeOptionsStrings, GUILayout.Width(50))];
            folderData.FontStyle = (FontStyle)EditorGUILayout.EnumPopup(folderData.FontStyle, GUILayout.Width(110));
            folderData.ImageColor = EditorGUILayout.ColorField(folderData.ImageColor, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
            if (GUILayout.Button(HierarchyDesigner_Configurable_Folders.GetFolderImageTypeDisplayName(folderData.ImageType), EditorStyles.popup, GUILayout.MinWidth(125), GUILayout.ExpandWidth(true))) { ShowFolderImageTypePopupForFolder(folderData); }
            if (EditorGUI.EndChangeCheck()) { folderHasModifiedChanges = true; }

            if (GUILayout.Button("↑", GUILayout.Width(moveItemInListButtonWidth)) && position > 0)
            {
                MoveFolder(position, position - 1);
            }
            if (GUILayout.Button("↓", GUILayout.Width(moveItemInListButtonWidth)) && position < totalItems - 1)
            {
                MoveFolder(position, position + 1);
            }
            if (GUILayout.Button("Create", GUILayout.Width(createButtonWidth)))
            {
                CreateFolderGameObject(folderData);
            }
            if (GUILayout.Button("Remove", GUILayout.Width(removeButtonWidth)))
            {
                RemoveFolder(key);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void MoveFolder(int indexA, int indexB)
        {
            string keyA = foldersOrder[indexA];
            string keyB = foldersOrder[indexB];

            foldersOrder[indexA] = keyB;
            foldersOrder[indexB] = keyA;
            folderHasModifiedChanges = true;
        }

        private void CreateFolderGameObject(HierarchyDesigner_Configurable_Folders.HierarchyDesigner_FolderData folderData)
        {
            GameObject folder = new GameObject(folderData.Name);
            folder.AddComponent<HierarchyDesignerFolder>();
            Undo.RegisterCreatedObjectUndo(folder, $"Create {folderData.Name}");

            Texture2D inspectorIcon = HierarchyDesigner_Shared_Resources.FolderInspectorIcon;
            if (inspectorIcon != null)
            {
                EditorGUIUtility.SetIconForObject(folder, inspectorIcon);
            }
        }

        private void RemoveFolder(string folderName)
        {
            if (tempFolders.TryGetValue(folderName, out _))
            {
                tempFolders.Remove(folderName);
                foldersOrder.Remove(folderName);
                folderHasModifiedChanges = true;
                GUIUtility.ExitGUI();
            }
        }

        private void ShowFolderImageTypePopup()
        {
            GenericMenu menu = new GenericMenu();
            Dictionary<string, List<string>> groupedTypes = HierarchyDesigner_Configurable_Folders.GetFolderImageTypesGrouped();
            foreach (KeyValuePair<string, List<string>> group in groupedTypes)
            {
                foreach (string typeName in group.Value)
                {
                    menu.AddItem(new GUIContent($"{group.Key}/{typeName}"), typeName == HierarchyDesigner_Configurable_Folders.GetFolderImageTypeDisplayName(newFolderImageType), OnFolderImageTypeSelected, typeName);
                }
            }
            menu.ShowAsContext();
        }

        private void ShowFolderImageTypePopupGlobal()
        {
            GenericMenu menu = new GenericMenu();
            Dictionary<string, List<string>> groupedTypes = HierarchyDesigner_Configurable_Folders.GetFolderImageTypesGrouped();
            foreach (KeyValuePair<string, List<string>> group in groupedTypes)
            {
                foreach (string typeName in group.Value)
                {
                    menu.AddItem(new GUIContent($"{group.Key}/{typeName}"), typeName == HierarchyDesigner_Configurable_Folders.GetFolderImageTypeDisplayName(tempGlobalFolderImageType), OnFolderImageTypeGlobalSelected, typeName);
                }
            }
            menu.ShowAsContext();
        }

        private void ShowFolderImageTypePopupForFolder(HierarchyDesigner_Configurable_Folders.HierarchyDesigner_FolderData folderData)
        {
            GenericMenu menu = new GenericMenu();
            Dictionary<string, List<string>> groupedTypes = HierarchyDesigner_Configurable_Folders.GetFolderImageTypesGrouped();
            foreach (KeyValuePair<string, List<string>> group in groupedTypes)
            {
                foreach (string typeName in group.Value)
                {
                    menu.AddItem(new GUIContent($"{group.Key}/{typeName}"), typeName == HierarchyDesigner_Configurable_Folders.GetFolderImageTypeDisplayName(folderData.ImageType), OnFolderImageTypeForFolderSelected, new KeyValuePair<HierarchyDesigner_Configurable_Folders.HierarchyDesigner_FolderData, string>(folderData, typeName));
                }
            }
            menu.ShowAsContext();
        }

        private void OnFolderImageTypeSelected(object imageTypeObj)
        {
            string typeName = (string)imageTypeObj;
            newFolderImageType = HierarchyDesigner_Configurable_Folders.ParseFolderImageType(typeName);
        }

        private void OnFolderImageTypeGlobalSelected(object imageTypeObj)
        {
            string typeName = (string)imageTypeObj;
            tempGlobalFolderImageType = HierarchyDesigner_Configurable_Folders.ParseFolderImageType(typeName);
            UpdateGlobalFolderImageType(tempGlobalFolderImageType);
        }

        private void OnFolderImageTypeForFolderSelected(object folderDataAndTypeObj)
        {
            KeyValuePair<HierarchyDesigner_Configurable_Folders.HierarchyDesigner_FolderData, string> folderDataAndType = (KeyValuePair<HierarchyDesigner_Configurable_Folders.HierarchyDesigner_FolderData, string>)folderDataAndTypeObj;
            folderDataAndType.Key.ImageType = HierarchyDesigner_Configurable_Folders.ParseFolderImageType(folderDataAndType.Value);
        }

        private void UpdateGlobalFolderTextColor(Color color)
        {
            foreach (HierarchyDesigner_Configurable_Folders.HierarchyDesigner_FolderData folder in tempFolders.Values)
            {
                folder.TextColor = color;
            }
            folderHasModifiedChanges = true;
        }

        private void UpdateGlobalFolderFontSize(int size)
        {
            foreach (HierarchyDesigner_Configurable_Folders.HierarchyDesigner_FolderData folder in tempFolders.Values)
            {
                folder.FontSize = size;
            }
            folderHasModifiedChanges = true;
        }

        private void UpdateGlobalFolderFontStyle(FontStyle style)
        {
            foreach (HierarchyDesigner_Configurable_Folders.HierarchyDesigner_FolderData folder in tempFolders.Values)
            {
                folder.FontStyle = style;
            }
            folderHasModifiedChanges = true;
        }

        private void UpdateGlobalFolderIconColor(Color color)
        {
            foreach (HierarchyDesigner_Configurable_Folders.HierarchyDesigner_FolderData folder in tempFolders.Values)
            {
                folder.ImageColor = color;
            }
            folderHasModifiedChanges = true;
        }

        private void UpdateGlobalFolderImageType(HierarchyDesigner_Configurable_Folders.FolderImageType imageType)
        {
            foreach (HierarchyDesigner_Configurable_Folders.HierarchyDesigner_FolderData folder in tempFolders.Values)
            {
                folder.ImageType = imageType;
            }
            folderHasModifiedChanges = true;
        }
        #endregion

        #region Separators
        private void DrawSeparatorsTab()
        {
            #region Body
            separatorMainScroll = EditorGUILayout.BeginScrollView(separatorMainScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            DrawSeparatorsCreationFields();

            if (tempSeparators.Count > 0)
            {
                DrawSeparatorsGlobalFields();
                DrawSeparatorsList();
            }
            else
            {
                EditorGUILayout.LabelField("No separators found. Please create a new separator.", HierarchyDesigner_Shared_GUI.UnassignedLabelStyle);
            }
            EditorGUILayout.EndScrollView();
            #endregion

            #region Footer
            if (GUILayout.Button("Update and Save Separators", GUILayout.Height(primaryButtonsHeight)))
            {
                UpdateAndSaveSeparatorsData();
            }
            #endregion
        }

        private void DrawSeparatorsCreationFields()
        {
            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Separator Creation", HierarchyDesigner_Shared_GUI.FieldsCategoryLabelStyle);
            GUILayout.Space(defaultMarginSpacing);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name", HierarchyDesigner_Shared_GUI.LayoutLabelStyle, GUILayout.Width(separatorCreationLabelWidth));
            newSeparatorName = EditorGUILayout.TextField(newSeparatorName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Text Color", HierarchyDesigner_Shared_GUI.LayoutLabelStyle, GUILayout.Width(separatorCreationLabelWidth));
            newSeparatorTextColor = EditorGUILayout.ColorField(newSeparatorTextColor);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Is Gradient", HierarchyDesigner_Shared_GUI.LayoutLabelStyle, GUILayout.Width(separatorCreationLabelWidth));
            newSeparatorIsGradient = EditorGUILayout.Toggle(newSeparatorIsGradient);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (newSeparatorIsGradient)
            {
                EditorGUILayout.LabelField("Background Gradient", HierarchyDesigner_Shared_GUI.LayoutLabelStyle, GUILayout.Width(separatorCreationLabelWidth));
                newSeparatorBackgroundGradient = EditorGUILayout.GradientField(newSeparatorBackgroundGradient) != null ? newSeparatorBackgroundGradient : new Gradient();
            }
            else
            {
                EditorGUILayout.LabelField("Background Color", HierarchyDesigner_Shared_GUI.LayoutLabelStyle, GUILayout.Width(separatorCreationLabelWidth));
                newSeparatorBackgroundColor = EditorGUILayout.ColorField(newSeparatorBackgroundColor);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            string[] newFontSizeOptionsStrings = Array.ConvertAll(fontSizeOptions, x => x.ToString());
            int newFontSizeIndex = Array.IndexOf(fontSizeOptions, newSeparatorFontSize);
            EditorGUILayout.LabelField("Font Size", HierarchyDesigner_Shared_GUI.LayoutLabelStyle, GUILayout.Width(separatorCreationLabelWidth));
            newSeparatorFontSize = fontSizeOptions[EditorGUILayout.Popup(newFontSizeIndex, newFontSizeOptionsStrings)];
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Font Style", HierarchyDesigner_Shared_GUI.LayoutLabelStyle, GUILayout.Width(separatorCreationLabelWidth));
            newSeparatorFontStyle = (FontStyle)EditorGUILayout.EnumPopup(newSeparatorFontStyle);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Text Anchor", HierarchyDesigner_Shared_GUI.LayoutLabelStyle, GUILayout.Width(separatorCreationLabelWidth));
            newSeparatorTextAnchor = (TextAnchor)EditorGUILayout.EnumPopup(newSeparatorTextAnchor);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Background Type", HierarchyDesigner_Shared_GUI.LayoutLabelStyle, GUILayout.Width(separatorCreationLabelWidth));
            if (GUILayout.Button(HierarchyDesigner_Configurable_Separators.GetSeparatorImageTypeDisplayName(newSeparatorImageType), EditorStyles.popup))
            {
                ShowSeparatorImageTypePopup();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            if (GUILayout.Button("Create Separator", GUILayout.Height(secondaryButtonsHeight)))
            {
                if (IsSeparatorNameValid(newSeparatorName))
                {
                    CreateSeparator(newSeparatorName, newSeparatorTextColor, newSeparatorIsGradient, newSeparatorBackgroundColor, newSeparatorBackgroundGradient, newSeparatorFontSize, newSeparatorFontStyle, newSeparatorTextAnchor, newSeparatorImageType);
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Separator Name", "Separator name is either duplicate or invalid.", "OK");
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawSeparatorsGlobalFields()
        {
            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Separators' Global Fields", HierarchyDesigner_Shared_GUI.FieldsCategoryLabelStyle);
            GUILayout.Space(defaultMarginSpacing);

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            tempSeparatorGlobalTextColor = EditorGUILayout.ColorField(tempSeparatorGlobalTextColor, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
            if (EditorGUI.EndChangeCheck()) { UpdateGlobalSeparatorTextColor(tempSeparatorGlobalTextColor); }
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Space(defaultMarginSpacing);
            tempSeparatorGlobalIsGradient = EditorGUILayout.Toggle(tempSeparatorGlobalIsGradient, GUILayout.Width(18));
            if (EditorGUI.EndChangeCheck()) { UpdateGlobalSeparatorIsGradientBackground(tempSeparatorGlobalIsGradient); }
            EditorGUI.BeginChangeCheck();
            tempSeparatorGlobalBackgroundColor = EditorGUILayout.ColorField(tempSeparatorGlobalBackgroundColor, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
            if (EditorGUI.EndChangeCheck()) { UpdateGlobalSeparatorBackgroundColor(tempSeparatorGlobalBackgroundColor); }
            EditorGUI.BeginChangeCheck();
            tempSeparatorGlobalBackgroundGradient = EditorGUILayout.GradientField(tempSeparatorGlobalBackgroundGradient, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
            if (EditorGUI.EndChangeCheck()) { UpdateGlobalSeparatorGradientBackground(tempSeparatorGlobalBackgroundGradient); }
            EditorGUI.BeginChangeCheck();
            string[] tempFontSizeOptionsStrings = Array.ConvertAll(fontSizeOptions, x => x.ToString());
            int tempFontSizeIndex = Array.IndexOf(fontSizeOptions, tempSeparatorGlobalFontSize);
            tempSeparatorGlobalFontSize = fontSizeOptions[EditorGUILayout.Popup(tempFontSizeIndex, tempFontSizeOptionsStrings, GUILayout.Width(50))];
            if (EditorGUI.EndChangeCheck()) { UpdateGlobalSeparatorFontSize(tempSeparatorGlobalFontSize); }
            EditorGUI.BeginChangeCheck();
            tempSeparatorGlobalFontStyle = (FontStyle)EditorGUILayout.EnumPopup(tempSeparatorGlobalFontStyle, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
            if (EditorGUI.EndChangeCheck()) { UpdateGlobalSeparatorFontStyle(tempSeparatorGlobalFontStyle); }
            EditorGUI.BeginChangeCheck();
            tempSeparatorGlobalTextAnchor = (TextAnchor)EditorGUILayout.EnumPopup(tempSeparatorGlobalTextAnchor, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
            if (EditorGUI.EndChangeCheck()) { UpdateGlobalSeparatorTextAnchor(tempSeparatorGlobalTextAnchor); }
            if (GUILayout.Button(HierarchyDesigner_Configurable_Separators.GetSeparatorImageTypeDisplayName(tempSeparatorGlobalImageType), EditorStyles.popup, GUILayout.MinWidth(150), GUILayout.ExpandWidth(true))) { ShowSeparatorImageTypePopupGlobal(); }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawSeparatorsList()
        {
            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Separators' List", HierarchyDesigner_Shared_GUI.FieldsCategoryLabelStyle);
            GUILayout.Space(defaultMarginSpacing);

            separatorsListScroll = EditorGUILayout.BeginScrollView(separatorsListScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            int index = 1;
            for (int i = 0; i < separatorsOrder.Count; i++)
            {
                string key = separatorsOrder[i];
                DrawSeparators(index, key, tempSeparators[key], i, separatorsOrder.Count);
                index++;
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void UpdateAndSaveSeparatorsData()
        {
            HierarchyDesigner_Configurable_Separators.ApplyChangesToSeparators(tempSeparators, separatorsOrder);
            HierarchyDesigner_Configurable_Separators.SaveSettings();
            HierarchyDesigner_Manager_GameObject.ClearSeparatorCache();
            separatorHasModifiedChanges = false;
        }

        private void LoadSeparatorData()
        {
            tempSeparators = HierarchyDesigner_Configurable_Separators.GetAllSeparatorsData(true);
            separatorsOrder = new List<string>(tempSeparators.Keys);
        }

        private void LoadSeparatorsCreationFields()
        {
            newSeparatorName = "";
            newSeparatorTextColor = HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultTextColor;
            newSeparatorIsGradient = HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultIsGradientBackground;
            newSeparatorBackgroundColor = HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultBackgroundColor;
            newSeparatorBackgroundGradient = HierarchyDesigner_Shared_Color.CopyGradient(HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultBackgroundGradient);
            newSeparatorFontSize = HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultFontSize;
            newSeparatorFontStyle = HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultFontStyle;
            newSeparatorTextAnchor = HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultTextAnchor;
            _ = HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultImageType;
        }

        private bool IsSeparatorNameValid(string separatorName)
        {
            return !string.IsNullOrEmpty(separatorName) && !tempSeparators.TryGetValue(separatorName, out _);
        }

        private void CreateSeparator(string separatorName, Color textColor, bool isGradient, Color backgroundColor, Gradient backgroundGradient, int fontSize, FontStyle fontStyle, TextAnchor textAnchor, HierarchyDesigner_Configurable_Separators.SeparatorImageType imageType)
        {
            HierarchyDesigner_Configurable_Separators.HierarchyDesigner_SeparatorData newSeparatorData = new()
            {
                Name = separatorName,
                TextColor = textColor,
                IsGradientBackground = isGradient,
                BackgroundColor = backgroundColor,
                BackgroundGradient = HierarchyDesigner_Shared_Color.CopyGradient(backgroundGradient),
                FontSize = fontSize,
                FontStyle = fontStyle,
                TextAnchor = textAnchor,
                ImageType = imageType,

            };
            tempSeparators[separatorName] = newSeparatorData;
            separatorsOrder.Add(separatorName);
            LoadSeparatorsCreationFields();
            separatorHasModifiedChanges = true;
            GUI.FocusControl(null);
        }

        private void DrawSeparators(int index, string key, HierarchyDesigner_Configurable_Separators.HierarchyDesigner_SeparatorData separatorData, int position, int totalItems)
        {
            float separatorLabelWidth = HierarchyDesigner_Shared_GUI.CalculateMaxLabelWidth(tempSeparators.Keys);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{index}) {separatorData.Name}", HierarchyDesigner_Shared_GUI.LayoutLabelStyle, GUILayout.Width(separatorLabelWidth));
            EditorGUI.BeginChangeCheck();
            separatorData.TextColor = EditorGUILayout.ColorField(separatorData.TextColor, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
            GUILayout.Space(defaultMarginSpacing);
            separatorData.IsGradientBackground = EditorGUILayout.Toggle(separatorData.IsGradientBackground, GUILayout.Width(18));
            if (separatorData.IsGradientBackground) { separatorData.BackgroundGradient = EditorGUILayout.GradientField(separatorData.BackgroundGradient, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true)) ?? new Gradient(); }
            else { separatorData.BackgroundColor = EditorGUILayout.ColorField(separatorData.BackgroundColor, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true)); }
            string[] fontSizeOptionsStrings = Array.ConvertAll(fontSizeOptions, x => x.ToString());
            int fontSizeIndex = Array.IndexOf(fontSizeOptions, separatorData.FontSize);
            if (fontSizeIndex == -1) { fontSizeIndex = 5; }
            separatorData.FontSize = fontSizeOptions[EditorGUILayout.Popup(fontSizeIndex, fontSizeOptionsStrings, GUILayout.Width(50))];
            separatorData.FontStyle = (FontStyle)EditorGUILayout.EnumPopup(separatorData.FontStyle, GUILayout.Width(100));
            separatorData.TextAnchor = (TextAnchor)EditorGUILayout.EnumPopup(separatorData.TextAnchor, GUILayout.Width(115));
            if (GUILayout.Button(HierarchyDesigner_Configurable_Separators.GetSeparatorImageTypeDisplayName(separatorData.ImageType), EditorStyles.popup, GUILayout.Width(150))) { ShowSeparatorImageTypePopupForSeparator(separatorData); }
            if (EditorGUI.EndChangeCheck()) { separatorHasModifiedChanges = true; }

            if (GUILayout.Button("↑", GUILayout.Width(moveItemInListButtonWidth)) && position > 0)
            {
                MoveSeparator(position, position - 1);
            }
            if (GUILayout.Button("↓", GUILayout.Width(moveItemInListButtonWidth)) && position < totalItems - 1)
            {
                MoveSeparator(position, position + 1);
            }
            if (GUILayout.Button("Create", GUILayout.Width(createButtonWidth)))
            {
                CreateSeparatorGameObject(separatorData);
            }
            if (GUILayout.Button("Remove", GUILayout.Width(removeButtonWidth)))
            {
                RemoveSeparator(key);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void MoveSeparator(int indexA, int indexB)
        {
            string keyA = separatorsOrder[indexA];
            string keyB = separatorsOrder[indexB];

            separatorsOrder[indexA] = keyB;
            separatorsOrder[indexB] = keyA;
            separatorHasModifiedChanges = true;
        }

        private void CreateSeparatorGameObject(HierarchyDesigner_Configurable_Separators.HierarchyDesigner_SeparatorData separatorData)
        {
            GameObject separator = new($"{HierarchyDesigner_Shared_Constants.SeparatorPrefix}{separatorData.Name}");
            separator.tag = HierarchyDesigner_Shared_Constants.SeparatorTag;
            HierarchyDesigner_Shared_Operations.SetSeparatorState(separator, false);
            separator.SetActive(false);
            Undo.RegisterCreatedObjectUndo(separator, $"Create {separatorData.Name}");

            Texture2D inspectorIcon = HierarchyDesigner_Shared_Resources.SeparatorInspectorIcon;
            if (inspectorIcon != null)
            {
                EditorGUIUtility.SetIconForObject(separator, inspectorIcon);
            }
        }

        private void RemoveSeparator(string separatorName)
        {
            if (tempSeparators.TryGetValue(separatorName, out _))
            {
                tempSeparators.Remove(separatorName);
                separatorsOrder.Remove(separatorName);
                separatorHasModifiedChanges = true;
                GUIUtility.ExitGUI();
            }
        }

        private void ShowSeparatorImageTypePopup()
        {
            GenericMenu menu = new();
            Dictionary<string, List<string>> groupedTypes = HierarchyDesigner_Configurable_Separators.GetSeparatorImageTypesGrouped();
            foreach (KeyValuePair<string, List<string>> group in groupedTypes)
            {
                foreach (string typeName in group.Value)
                {
                    menu.AddItem(new GUIContent($"{group.Key}/{typeName}"), typeName == HierarchyDesigner_Configurable_Separators.GetSeparatorImageTypeDisplayName(newSeparatorImageType), OnSeparatorImageTypeSelected, typeName);
                }
            }
            menu.ShowAsContext();
        }

        private void ShowSeparatorImageTypePopupGlobal()
        {
            GenericMenu menu = new();
            Dictionary<string, List<string>> groupedTypes = HierarchyDesigner_Configurable_Separators.GetSeparatorImageTypesGrouped();
            foreach (KeyValuePair<string, List<string>> group in groupedTypes)
            {
                foreach (string typeName in group.Value)
                {
                    menu.AddItem(new GUIContent($"{group.Key}/{typeName}"), typeName == HierarchyDesigner_Configurable_Separators.GetSeparatorImageTypeDisplayName(tempSeparatorGlobalImageType), OnSeparatorImageTypeGlobalSelected, typeName);
                }
            }
            menu.ShowAsContext();
        }

        private void ShowSeparatorImageTypePopupForSeparator(HierarchyDesigner_Configurable_Separators.HierarchyDesigner_SeparatorData separatorData)
        {
            GenericMenu menu = new();
            Dictionary<string, List<string>> groupedTypes = HierarchyDesigner_Configurable_Separators.GetSeparatorImageTypesGrouped();
            foreach (KeyValuePair<string, List<string>> group in groupedTypes)
            {
                foreach (string typeName in group.Value)
                {
                    menu.AddItem(new GUIContent($"{group.Key}/{typeName}"), typeName == HierarchyDesigner_Configurable_Separators.GetSeparatorImageTypeDisplayName(separatorData.ImageType), OnSeparatorImageTypeForSeparatorSelected, new KeyValuePair<HierarchyDesigner_Configurable_Separators.HierarchyDesigner_SeparatorData, string>(separatorData, typeName));
                }
            }
            menu.ShowAsContext();
        }

        private void OnSeparatorImageTypeSelected(object imageTypeObj)
        {
            string typeName = (string)imageTypeObj;
            newSeparatorImageType = HierarchyDesigner_Configurable_Separators.ParseSeparatorImageType(typeName);
        }

        private void OnSeparatorImageTypeGlobalSelected(object imageTypeObj)
        {
            string typeName = (string)imageTypeObj;
            tempSeparatorGlobalImageType = HierarchyDesigner_Configurable_Separators.ParseSeparatorImageType(typeName);
            UpdateGlobalSeparatorImageType(tempSeparatorGlobalImageType);
        }

        private void OnSeparatorImageTypeForSeparatorSelected(object separatorDataAndTypeObj)
        {
            KeyValuePair<HierarchyDesigner_Configurable_Separators.HierarchyDesigner_SeparatorData, string> separatorDataAndType = (KeyValuePair<HierarchyDesigner_Configurable_Separators.HierarchyDesigner_SeparatorData, string>)separatorDataAndTypeObj;
            separatorDataAndType.Key.ImageType = HierarchyDesigner_Configurable_Separators.ParseSeparatorImageType(separatorDataAndType.Value);
        }

        private void UpdateGlobalSeparatorTextColor(Color color)
        {
            foreach (HierarchyDesigner_Configurable_Separators.HierarchyDesigner_SeparatorData separator in tempSeparators.Values)
            {
                separator.TextColor = color;
            }
            separatorHasModifiedChanges = true;
        }

        private void UpdateGlobalSeparatorIsGradientBackground(bool isGradientBackground)
        {
            foreach (HierarchyDesigner_Configurable_Separators.HierarchyDesigner_SeparatorData separator in tempSeparators.Values)
            {
                separator.IsGradientBackground = isGradientBackground;
            }
            separatorHasModifiedChanges = true;
        }

        private void UpdateGlobalSeparatorBackgroundColor(Color color)
        {
            foreach (HierarchyDesigner_Configurable_Separators.HierarchyDesigner_SeparatorData separator in tempSeparators.Values)
            {
                separator.BackgroundColor = color;
            }
            separatorHasModifiedChanges = true;
        }

        private void UpdateGlobalSeparatorGradientBackground(Gradient gradientBackground)
        {
            foreach (HierarchyDesigner_Configurable_Separators.HierarchyDesigner_SeparatorData separator in tempSeparators.Values)
            {
                separator.BackgroundGradient = HierarchyDesigner_Shared_Color.CopyGradient(gradientBackground);
            }
            separatorHasModifiedChanges = true;
        }

        private void UpdateGlobalSeparatorFontSize(int size)
        {
            foreach (HierarchyDesigner_Configurable_Separators.HierarchyDesigner_SeparatorData separator in tempSeparators.Values)
            {
                separator.FontSize = size;
            }
            separatorHasModifiedChanges = true;
        }

        private void UpdateGlobalSeparatorFontStyle(FontStyle style)
        {
            foreach (HierarchyDesigner_Configurable_Separators.HierarchyDesigner_SeparatorData separator in tempSeparators.Values)
            {
                separator.FontStyle = style;
            }
            separatorHasModifiedChanges = true;
        }

        private void UpdateGlobalSeparatorTextAnchor(TextAnchor anchor)
        {
            foreach (HierarchyDesigner_Configurable_Separators.HierarchyDesigner_SeparatorData separator in tempSeparators.Values)
            {
                separator.TextAnchor = anchor;
            }
            separatorHasModifiedChanges = true;
        }

        private void UpdateGlobalSeparatorImageType(HierarchyDesigner_Configurable_Separators.SeparatorImageType imageType)
        {
            foreach (HierarchyDesigner_Configurable_Separators.HierarchyDesigner_SeparatorData separator in tempSeparators.Values)
            {
                separator.ImageType = imageType;
            }
            separatorHasModifiedChanges = true;
        }
        #endregion

        #region Tools
        private void DrawToolsTab()
        {
            toolsMainScroll = EditorGUILayout.BeginScrollView(toolsMainScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            DrawToolsCategory();

            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.SecondaryPanelStyle);
            DrawToolsActions();
            EditorGUILayout.Space(defaultMarginSpacing);

            if (GUILayout.Button("Apply Action", GUILayout.Height(primaryButtonsHeight)))
            {
                if (availableActionMethods.Count > selectedActionIndex && selectedActionIndex >= 0)
                {
                    MethodInfo methodToInvoke = availableActionMethods[selectedActionIndex];
                    methodToInvoke?.Invoke(null, null);
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
        }

        private void DrawToolsCategory()
        {
            HierarchyDesigner_Attribute_Tools previousCategory = selectedCategory;

            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Category Selection", HierarchyDesigner_Shared_GUI.FieldsCategoryLabelStyle, GUILayout.Width(labelWidth));
            GUILayout.Space(2);
            selectedCategory = HierarchyDesigner_Shared_GUI.DrawEnumPopup("Selected Category", 145, selectedCategory, HierarchyDesigner_Attribute_Tools.Activate);
            if (previousCategory != selectedCategory) UpdateAvailableActions();
            EditorGUILayout.EndVertical();
        }

        private void DrawToolsActions()
        {
           
            if (availableActionNames.Count == 0) 
            {
                GUILayout.Label("No tools available for this category.", HierarchyDesigner_Shared_GUI.UnassignedLabelStyle); 
            }
            else
            {
                EditorGUILayout.LabelField("Action Selection", HierarchyDesigner_Shared_GUI.FieldsCategoryLabelStyle, GUILayout.Width(labelWidth));
                GUILayout.Space(defaultMarginSpacing);
                selectedActionIndex = EditorGUILayout.Popup(selectedActionIndex, availableActionNames.ToArray());
            }
        }

        private void LoadTools()
        {
            if (!cacheInitialized)
            {
                InitializeActionCache();
                cacheInitialized = true;
            }
            UpdateAvailableActions();
        }

        private void InitializeActionCache()
        {
            MethodInfo[] methods = typeof(HierarchyDesigner_Shared_Menu).GetMethods(BindingFlags.Public | BindingFlags.Static);
            foreach (MethodInfo method in methods)
            {
                object[] toolAttributes = method.GetCustomAttributes(typeof(HierarchyDesigner_Shared_Attributes), false);
                for (int i = 0; i < toolAttributes.Length; i++)
                {
                    if (toolAttributes[i] is HierarchyDesigner_Shared_Attributes toolAttribute)
                    {
                        object[] menuItemAttributes = method.GetCustomAttributes(typeof(MenuItem), true);
                        for (int j = 0; j < menuItemAttributes.Length; j++)
                        {
                            MenuItem menuItemAttribute = menuItemAttributes[j] as MenuItem;
                            if (menuItemAttribute != null)
                            {
                                string rawActionName = menuItemAttribute.menuItem;
                                string actionName = ExtractActionsFromCategories(rawActionName, toolAttribute.Category);
                                if (!string.IsNullOrEmpty(actionName))
                                {
                                    if (!cachedActions.TryGetValue(toolAttribute.Category, out List<(string Name, MethodInfo Method)> actionsList))
                                    {
                                        actionsList = new();
                                        cachedActions[toolAttribute.Category] = actionsList;
                                    }
                                    actionsList.Add((actionName, method));
                                }
                            }
                        }
                    }
                }
            }
        }

        private string ExtractActionsFromCategories(string menuItemPath, HierarchyDesigner_Attribute_Tools category)
        {
            if (string.IsNullOrEmpty(menuItemPath)) return null;

            string[] parts = menuItemPath.Split('/');
            if (parts.Length < 2) return null;

            return category switch
            {
                HierarchyDesigner_Attribute_Tools.Rename or HierarchyDesigner_Attribute_Tools.Sort => parts[^1],
                HierarchyDesigner_Attribute_Tools.Activate or HierarchyDesigner_Attribute_Tools.Deactivate or HierarchyDesigner_Attribute_Tools.Count or HierarchyDesigner_Attribute_Tools.Lock or HierarchyDesigner_Attribute_Tools.Unlock or HierarchyDesigner_Attribute_Tools.Select => (parts.Length > 4) ? string.Join("/", parts, 4, parts.Length - 4) : null,
                _ => (parts.Length >= 2) ? $"{parts[^2]}/{parts[^1]}" : null,
            };
        }

        private void UpdateAvailableActions()
        {
            availableActionNames.Clear();
            availableActionMethods.Clear();
            if (cachedActions.TryGetValue(selectedCategory, out List<(string Name, MethodInfo Method)> actions))
            {
                foreach ((string Name, MethodInfo Method) in actions)
                {
                    availableActionNames.Add(Name);
                    availableActionMethods.Add(Method);
                }
            }
            selectedActionIndex = 0;
        }
        #endregion

        #region Presets
        private void DrawPresetsTab()
        {
            #region Body
            presetsMainScroll = EditorGUILayout.BeginScrollView(presetsMainScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            DrawPresetsList();
            DrawPresetsFeaturesFields();
            EditorGUILayout.EndScrollView();
            #endregion

            #region Footer
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Enable All Features", GUILayout.Height(secondaryButtonsHeight)))
            {
                EnableAllFeatures(true);
            }
            if (GUILayout.Button("Disable All Features", GUILayout.Height(secondaryButtonsHeight)))
            {
                EnableAllFeatures(false);
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Confirm and Apply Preset", GUILayout.Height(primaryButtonsHeight)))
            {
                ApplySelectedPreset();
            }
            EditorGUILayout.EndVertical();
            #endregion
        }

        private void DrawPresetsList()
        {
            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Preset Selection", HierarchyDesigner_Shared_GUI.FieldsCategoryLabelStyle);
            GUILayout.Space(defaultMarginSpacing);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Selected Preset", HierarchyDesigner_Shared_GUI.LayoutLabelStyle, GUILayout.Width(presetslabelWidth));
            if (GUILayout.Button(presetNames[selectedPresetIndex], EditorStyles.popup))
            {
                ShowPresetPopup();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawPresetsFeaturesFields()
        {
            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Apply Preset To:", HierarchyDesigner_Shared_GUI.FieldsCategoryLabelStyle, GUILayout.Width(presetslabelWidth));
            GUILayout.Space(defaultMarginSpacing);

            applyToFolders = HierarchyDesigner_Shared_GUI.DrawToggle("Folders", presetsToggleLabelWidth, applyToFolders, true, true, "Applies the folder values from the currently selected preset (i.e., Text Color, Font Size, Font Style, Image Color, and Image Type) to all folders in your folder list.");
            applyToSeparators = HierarchyDesigner_Shared_GUI.DrawToggle("Separators", presetsToggleLabelWidth, applyToSeparators, true, true, "Applies the separator values from the currently selected preset (i.e., Text Color, Is Gradient Background, Background Color, Background Gradient, Font Size, Font Style, Text Alignment and Background Image Type) to all separators in your separators list.");
            applyToTag = HierarchyDesigner_Shared_GUI.DrawToggle("GameObject's Tag", presetsToggleLabelWidth, applyToTag, true, true, "Applies the tag values from the currently selected preset (i.e., Color, Font Style, Font Size, and Text Anchor) to the GameObject's Tag.");
            applyToLayer = HierarchyDesigner_Shared_GUI.DrawToggle("GameObject's Layer", presetsToggleLabelWidth, applyToLayer, true, true, "Applies the layer values from the currently selected preset (i.e., Color, Font Style, Font Size, and Text Anchor) to the GameObject's Layer.");
            applyToTree = HierarchyDesigner_Shared_GUI.DrawToggle("Hierarchy Tree", presetsToggleLabelWidth, applyToTree, true, true, "Applies the tree values from the currently selected preset (i.e., Color) to the Hierarchy Tree.");
            applyToLines = HierarchyDesigner_Shared_GUI.DrawToggle("Hierarchy Lines", presetsToggleLabelWidth, applyToLines, true, true, "Applies the line values from the currently selected preset (i.e., Color) to the Hierarchy Lines.");
            applyToFolderDefaultValues = HierarchyDesigner_Shared_GUI.DrawToggle("Folder Default Values", presetsToggleLabelWidth, applyToFolderDefaultValues, true, true, "Applies the default folder values from the currently selected preset (i.e., Text Color, Font Size, Font Style, Image Color, and Image Type) to folders that are not present in your folders list, as well as to the folder creation fields.");
            applyToSeparatorDefaultValues = HierarchyDesigner_Shared_GUI.DrawToggle("Separator Default Values", presetsToggleLabelWidth, applyToSeparatorDefaultValues, true, true, "Applies the default separator values from the currently selected preset (i.e., Text Color, Is Gradient Background, Background Color, Background Gradient, Font Size, Font Style, Text Alignment and Background Image Type) to separators that are not present in your separators list, as well as to the separator creation fields.");
            applyToLock = HierarchyDesigner_Shared_GUI.DrawToggle("Lock Label", presetsToggleLabelWidth, applyToLock, true, true, "Applies the lock label values from the currently selected preset (i.e., Color, Font Size, Font Style and Text Anchor) to the Lock Label.");
            EditorGUILayout.EndVertical();
        }

        private void LoadPresets()
        {
            presetNames = HierarchyDesigner_Configurable_Presets.GetPresetNames();
        }

        private void ShowPresetPopup()
        {
            GenericMenu menu = new();
            Dictionary<string, List<string>> groupedPresets = HierarchyDesigner_Configurable_Presets.GetPresetNamesGrouped();

            foreach (KeyValuePair<string, List<string>> group in groupedPresets)
            {
                foreach (string presetName in group.Value)
                {
                    menu.AddItem(new($"{group.Key}/{presetName}"), presetName == presetNames[selectedPresetIndex], OnPresetSelected, presetName);
                }
            }

            menu.ShowAsContext();
        }

        private void OnPresetSelected(object presetNameObj)
        {
            string presetName = (string)presetNameObj;
            selectedPresetIndex = Array.IndexOf(presetNames, presetName);
        }

        private void ApplySelectedPreset()
        {
            if (selectedPresetIndex < 0 || selectedPresetIndex >= presetNames.Length) return;

            HierarchyDesigner_Configurable_Presets.HierarchyDesigner_Preset selectedPreset = HierarchyDesigner_Configurable_Presets.Presets[selectedPresetIndex];

            string message = "Are you sure you want to override your current values for: ";
            List<string> changesList = new();
            if (applyToFolders) changesList.Add("Folders");
            if (applyToSeparators) changesList.Add("Separators");
            if (applyToTag) changesList.Add("GameObject's Tag");
            if (applyToLayer) changesList.Add("GameObject's Layer");
            if (applyToTree) changesList.Add("Hierarchy Tree");
            if (applyToLines) changesList.Add("Hierarchy Lines");
            if (applyToFolderDefaultValues) changesList.Add("Folder Default Values");
            if (applyToSeparatorDefaultValues) changesList.Add("Separator Default Values");
            if (applyToLock) changesList.Add("Lock Label");
            message += string.Join(", ", changesList) + "?\n\n*If you select 'confirm' all values will be overridden and saved.*";

            if (EditorUtility.DisplayDialog("Confirm Preset Application", message, "Confirm", "Cancel"))
            {
                if (applyToFolders)
                {
                    HierarchyDesigner_Shared_Operations.ApplyPresetToFolders(selectedPreset);
                }
                if (applyToSeparators)
                {
                    HierarchyDesigner_Shared_Operations.ApplyPresetToSeparators(selectedPreset);
                }
                bool shouldSaveDesignSettings = false;
                if (applyToTag)
                {
                    HierarchyDesigner_Shared_Operations.ApplyPresetToTag(selectedPreset);
                    shouldSaveDesignSettings = true;
                }
                if (applyToLayer)
                {
                    HierarchyDesigner_Shared_Operations.ApplyPresetToLayer(selectedPreset);
                    shouldSaveDesignSettings = true;
                }
                if (applyToTree)
                {
                    HierarchyDesigner_Shared_Operations.ApplyPresetToTree(selectedPreset);
                    shouldSaveDesignSettings = true;
                }
                if (applyToLines)
                {
                    HierarchyDesigner_Shared_Operations.ApplyPresetToLines(selectedPreset);
                    shouldSaveDesignSettings = true;
                }
                if (applyToFolderDefaultValues)
                {
                    HierarchyDesigner_Shared_Operations.ApplyPresetToDefaultFolderValues(selectedPreset);
                    shouldSaveDesignSettings = true;
                }
                if (applyToSeparatorDefaultValues)
                {
                    HierarchyDesigner_Shared_Operations.ApplyPresetToDefaultSeparatorValues(selectedPreset);
                    shouldSaveDesignSettings = true;
                }
                if (applyToLock)
                {
                    HierarchyDesigner_Shared_Operations.ApplyPresetToLockLabel(selectedPreset);
                    shouldSaveDesignSettings = true;
                }
                if (shouldSaveDesignSettings)
                {
                    HierarchyDesigner_Configurable_DesignSettings.SaveSettings();
                    LoadDesignSettingsData();
                    LoadFolderCreationFields();
                    LoadSeparatorsCreationFields();
                }
                EditorApplication.RepaintHierarchyWindow();
            }
        }

        private void EnableAllFeatures(bool enable)
        {
            applyToFolders = enable;
            applyToSeparators = enable;
            applyToTag = enable;
            applyToLayer = enable;
            applyToTree = enable;
            applyToLines = enable;
            applyToFolderDefaultValues = enable;
            applyToSeparatorDefaultValues = enable;
            applyToLock = enable;
        }
        #endregion

        #region General Settings
        private void DrawGeneralSettingsTab()
        {
            #region Body
            generalSettingsMainScroll = EditorGUILayout.BeginScrollView(generalSettingsMainScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            DrawGeneralSettingsCoreFeatures();
            DrawGeneralSettingsMainFeatures();
            DrawGeneralSettingsFilteringFeatures();
            EditorGUILayout.EndScrollView();
            #endregion

            #region Footer
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Enable All Features", GUILayout.Height(secondaryButtonsHeight)))
            {
                EnableAllGeneralSettingsFeatures(true);
            }
            if (GUILayout.Button("Disable All Features", GUILayout.Height(secondaryButtonsHeight)))
            {
                EnableAllGeneralSettingsFeatures(false);
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Update and Save General Settings", GUILayout.Height(primaryButtonsHeight)))
            {
                UpdateAndSaveGeneralSettingsData();
            }
            #endregion
        }

        private void DrawGeneralSettingsCoreFeatures()
        {
            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Core Features", HierarchyDesigner_Shared_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempLayoutMode = HierarchyDesigner_Shared_GUI.DrawEnumPopup("Hierarchy Layout Mode", enumPopupLabelWidth, tempLayoutMode, HierarchyDesigner_Configurable_GeneralSettings.HierarchyLayoutMode.Split, true, "The layout of your Hierarchy window:\n\n• Consecutive: Elements are displayed after the GameObject's name and are drawn one after the other.\n\n• Docked: Elements are docked to the right side of your Hierarchy window.\n\n• Split: Elements are divided into two parts (consecutive and docked).");
            tempTreeMode = HierarchyDesigner_Shared_GUI.DrawEnumPopup("Hierarchy Tree Mode", enumPopupLabelWidth, tempTreeMode, HierarchyDesigner_Configurable_GeneralSettings.HierarchyTreeMode.Default, true, "The mode of the Hierarchy Tree feature:\n\n• Minimal: Uses the default tree branch and tree branch Type I for all parent-child relationships.\n\n• Default: Uses all four branch types (i.e., Type I, Type L, Type T, and Type T-Bud) for parent-child relationships.");
            if (EditorGUI.EndChangeCheck()) { generalSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void DrawGeneralSettingsMainFeatures()
        {
            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Main Features", HierarchyDesigner_Shared_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempEnableGameObjectMainIcon = HierarchyDesigner_Shared_GUI.DrawToggle("Enable GameObject's Main Icon", generalSettingsMainToggleLabelWidth, tempEnableGameObjectMainIcon, true, true, "Displays the main icon for GameObjects. Main icons are determined based on the order of components in a GameObject (i.e., default Unity behavior; usually, the second or third component becomes the main icon).\n\nNote: You can modify the main icon of a GameObject by moving the desired component to the second or third position.");
            tempEnableGameObjectComponentIcons = HierarchyDesigner_Shared_GUI.DrawToggle("Enable GameObject's Component Icons", generalSettingsMainToggleLabelWidth, tempEnableGameObjectComponentIcons, true, true, "Displays all components of the GameObject in the Hierarchy window.");
            tempEnableGameObjectTag = HierarchyDesigner_Shared_GUI.DrawToggle("Enable GameObject's Tag", generalSettingsMainToggleLabelWidth, tempEnableGameObjectTag, true, true, "Displays the tag of the GameObject in the Hierarchy window.");
            tempEnableGameObjectLayer = HierarchyDesigner_Shared_GUI.DrawToggle("Enable GameObject's Layer", generalSettingsMainToggleLabelWidth, tempEnableGameObjectLayer, true, true, "Displays the layer of the GameObject in the Hierarchy window.");
            tempEnableHierarchyTree = HierarchyDesigner_Shared_GUI.DrawToggle("Enable Hierarchy Tree", generalSettingsMainToggleLabelWidth, tempEnableHierarchyTree, true, true, "Displays the parent-child relationship of GameObjects in the Hierarchy window through branch icons.");
            tempEnableHierarchyRows = HierarchyDesigner_Shared_GUI.DrawToggle("Enable Hierarchy Rows", generalSettingsMainToggleLabelWidth, tempEnableHierarchyRows, true, true, "Draws background rows for alternating GameObjects in the Hierarchy window.");
            tempEnableHierarchyLines = HierarchyDesigner_Shared_GUI.DrawToggle("Enable Hierarchy Lines", generalSettingsMainToggleLabelWidth, tempEnableHierarchyLines, true, true, "Draws horizontal lines under each GameObject in the Hierarchy window.");
            tempEnableHierarchyButtons = HierarchyDesigner_Shared_GUI.DrawToggle("Enable Hierarchy Buttons", generalSettingsMainToggleLabelWidth, tempEnableHierarchyButtons, true, true, "Displays utility buttons (i.e., Active State and Lock State) for each GameObject in the Hierarchy window.");
            tempEnableMajorShortcuts = HierarchyDesigner_Shared_GUI.DrawToggle("Enable Major Shortcuts", generalSettingsMainToggleLabelWidth, tempEnableMajorShortcuts, true, true, "Allows major shortcuts (i.e., Toggle GameObject Active State and Lock State, Chage Selected Tag and Layer; and Rename Selected GameObjects) to be executed.\n\nNote: Disabling this feature improves performance as it will not check for input while interacting with the Hierarchy window.");
            tempDisableHierarchyDesignerDuringPlayMode = HierarchyDesigner_Shared_GUI.DrawToggle("Disable Hierarchy Designer During PlayMode", generalSettingsMainToggleLabelWidth, tempDisableHierarchyDesignerDuringPlayMode, true, true, "Disables the majority of Hierarchy Designer's features while in Play mode.\n\nNote: It is recommended to disable this feature only when debugging specific aspects of your game where performance is not a concern.");
            if (EditorGUI.EndChangeCheck()) { generalSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void DrawGeneralSettingsFilteringFeatures()
        {
            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Filtering Features", HierarchyDesigner_Shared_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempExcludeFolderProperties = HierarchyDesigner_Shared_GUI.DrawToggle("Exclude Folder Properties", generalSettingsFilterToggleLabelWidth, tempExcludeFolderProperties, true, true, "Excludes certain main features (i.e., Component Icons, Tag and Layer) for folder GameObjects.");
            tempExcludeTransformForGameObjectComponentIcons = HierarchyDesigner_Shared_GUI.DrawToggle("Exclude Transform Component", generalSettingsFilterToggleLabelWidth, tempExcludeTransformForGameObjectComponentIcons, true, true, "Excludes the Transform and Rect Transform components from the GameObject's Component Icons display.");
            tempExcludeCanvasRendererForGameObjectComponentIcons = HierarchyDesigner_Shared_GUI.DrawToggle("Exclude Canvas Renderer Component", generalSettingsFilterToggleLabelWidth, tempExcludeCanvasRendererForGameObjectComponentIcons, true, true, "Excludes the Canvas Renderer component from the GameObject's Component Icons display.");
            tempMaximumComponentIconsAmount = HierarchyDesigner_Shared_GUI.DrawIntSlider("Maximum Component Icons Amount", generalSettingsFilterToggleLabelWidth, tempMaximumComponentIconsAmount, 10 , 1, 25, true, "Limits how many Component Icons are allowed to be displayed for each GameObject.");
            if (EditorGUI.EndChangeCheck()) { generalSettingsHasModifiedChanges = true; }
            GUILayout.Space(defaultMarginSpacing);

            #region Tag
            string[] tags = UnityEditorInternal.InternalEditorUtility.tags;
            int tagMask = GetMaskFromList(tempExcludedTags, tags);
            EditorGUI.BeginChangeCheck();
            tagMask = HierarchyDesigner_Shared_GUI.DrawMaskField("Excluded Tags", maskFieldLabelWidth, tagMask, 1, tags, true, "Excludes selected tags from being displayed in the GameObject's Tag\nfeature.");
            if (EditorGUI.EndChangeCheck()) { generalSettingsHasModifiedChanges = true; }
            tempExcludedTags = GetListFromMask(tagMask, tags);
            #endregion

            #region Layer
            string[] layers = UnityEditorInternal.InternalEditorUtility.layers;
            int layerMask = GetMaskFromList(tempExcludedLayers, layers);
            layerMask = HierarchyDesigner_Shared_GUI.DrawMaskField("Excluded Layers", maskFieldLabelWidth, layerMask, 1, layers, true, "Excludes selected layers from being displayed in the GameObject's Layer\nfeature.");
            EditorGUI.BeginChangeCheck();
            tempExcludedLayers = GetListFromMask(layerMask, layers);
            if (EditorGUI.EndChangeCheck()) { generalSettingsHasModifiedChanges = true; }
            #endregion
            EditorGUILayout.EndVertical();
        }

        private void UpdateAndSaveGeneralSettingsData()
        {
            HierarchyDesigner_Configurable_GeneralSettings.LayoutMode = tempLayoutMode;
            HierarchyDesigner_Configurable_GeneralSettings.TreeMode = tempTreeMode;
            HierarchyDesigner_Configurable_GeneralSettings.EnableGameObjectMainIcon = tempEnableGameObjectMainIcon;
            HierarchyDesigner_Configurable_GeneralSettings.EnableGameObjectComponentIcons = tempEnableGameObjectComponentIcons;
            HierarchyDesigner_Configurable_GeneralSettings.EnableHierarchyTree = tempEnableHierarchyTree;
            HierarchyDesigner_Configurable_GeneralSettings.EnableGameObjectTag = tempEnableGameObjectTag;
            HierarchyDesigner_Configurable_GeneralSettings.EnableGameObjectLayer = tempEnableGameObjectLayer;
            HierarchyDesigner_Configurable_GeneralSettings.EnableHierarchyRows = tempEnableHierarchyRows;
            HierarchyDesigner_Configurable_GeneralSettings.EnableHierarchyLines = tempEnableHierarchyLines;
            HierarchyDesigner_Configurable_GeneralSettings.EnableHierarchyButtons = tempEnableHierarchyButtons;
            HierarchyDesigner_Configurable_GeneralSettings.EnableMajorShortcuts = tempEnableMajorShortcuts;
            HierarchyDesigner_Configurable_GeneralSettings.DisableHierarchyDesignerDuringPlayMode = tempDisableHierarchyDesignerDuringPlayMode;
            HierarchyDesigner_Configurable_GeneralSettings.ExcludeFolderProperties = tempExcludeFolderProperties;
            HierarchyDesigner_Configurable_GeneralSettings.ExcludeTransformForGameObjectComponentIcons = tempExcludeTransformForGameObjectComponentIcons;
            HierarchyDesigner_Configurable_GeneralSettings.ExcludeCanvasRendererForGameObjectComponentIcons = tempExcludeCanvasRendererForGameObjectComponentIcons;
            HierarchyDesigner_Configurable_GeneralSettings.ExcludedTags = tempExcludedTags;
            HierarchyDesigner_Configurable_GeneralSettings.ExcludedLayers = tempExcludedLayers;
            HierarchyDesigner_Configurable_GeneralSettings.MaximumComponentIconsAmount = tempMaximumComponentIconsAmount;
            HierarchyDesigner_Configurable_GeneralSettings.SaveSettings();
            generalSettingsHasModifiedChanges = false;
        }

        private void LoadGeneralSettingsData()
        {
            tempLayoutMode = HierarchyDesigner_Configurable_GeneralSettings.LayoutMode;
            tempTreeMode = HierarchyDesigner_Configurable_GeneralSettings.TreeMode;
            tempEnableGameObjectMainIcon = HierarchyDesigner_Configurable_GeneralSettings.EnableGameObjectMainIcon;
            tempEnableGameObjectComponentIcons = HierarchyDesigner_Configurable_GeneralSettings.EnableGameObjectComponentIcons;
            tempEnableGameObjectTag = HierarchyDesigner_Configurable_GeneralSettings.EnableGameObjectTag;
            tempEnableGameObjectLayer = HierarchyDesigner_Configurable_GeneralSettings.EnableGameObjectLayer;
            tempEnableHierarchyTree = HierarchyDesigner_Configurable_GeneralSettings.EnableHierarchyTree;
            tempEnableHierarchyRows = HierarchyDesigner_Configurable_GeneralSettings.EnableHierarchyRows;
            tempEnableHierarchyLines = HierarchyDesigner_Configurable_GeneralSettings.EnableHierarchyLines;
            tempEnableHierarchyButtons = HierarchyDesigner_Configurable_GeneralSettings.EnableHierarchyButtons;
            tempEnableMajorShortcuts = HierarchyDesigner_Configurable_GeneralSettings.EnableMajorShortcuts;
            tempDisableHierarchyDesignerDuringPlayMode = HierarchyDesigner_Configurable_GeneralSettings.DisableHierarchyDesignerDuringPlayMode;
            tempExcludeFolderProperties = HierarchyDesigner_Configurable_GeneralSettings.ExcludeFolderProperties;
            tempExcludeTransformForGameObjectComponentIcons = HierarchyDesigner_Configurable_GeneralSettings.ExcludeTransformForGameObjectComponentIcons;
            tempExcludeCanvasRendererForGameObjectComponentIcons = HierarchyDesigner_Configurable_GeneralSettings.ExcludeCanvasRendererForGameObjectComponentIcons;
            tempExcludedTags = HierarchyDesigner_Configurable_GeneralSettings.ExcludedTags;
            tempExcludedLayers = HierarchyDesigner_Configurable_GeneralSettings.ExcludedLayers;
            tempMaximumComponentIconsAmount = HierarchyDesigner_Configurable_GeneralSettings.MaximumComponentIconsAmount;
        }

        private void EnableAllGeneralSettingsFeatures(bool enable)
        {
            tempEnableGameObjectMainIcon = enable;
            tempEnableGameObjectComponentIcons = enable;
            tempEnableGameObjectTag = enable;
            tempEnableGameObjectLayer = enable;
            tempEnableHierarchyTree = enable;
            tempEnableHierarchyRows = enable;
            tempEnableHierarchyLines = enable;
            tempEnableHierarchyButtons = enable;
            tempEnableMajorShortcuts = enable;
            tempDisableHierarchyDesignerDuringPlayMode = enable;
            tempExcludeFolderProperties = enable;
            tempExcludeTransformForGameObjectComponentIcons = enable;
            tempExcludeCanvasRendererForGameObjectComponentIcons = enable;
            generalSettingsHasModifiedChanges = true;
        }

        private int GetMaskFromList(List<string> list, string[] allItems)
        {
            int mask = 0;
            for (int i = 0; i < allItems.Length; i++)
            {
                if (list.Contains(allItems[i]))
                {
                    mask |= 1 << i;
                }
            }
            return mask;
        }

        private List<string> GetListFromMask(int mask, string[] allItems)
        {
            List<string> list = new();
            for (int i = 0; i < allItems.Length; i++)
            {
                if ((mask & (1 << i)) != 0)
                {
                    list.Add(allItems[i]);
                }
            }
            return list;
        }
        #endregion

        #region Design Settings
        private void DrawDesignSettingsTab()
        {
            #region Body
            designSettingsMainScroll = EditorGUILayout.BeginScrollView(designSettingsMainScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            DrawDesignSettingsComponentIcons();
            DrawDesignSettingsTag();
            DrawDesignSettingsLayer();
            DrawDesignSettingsTagAndLayer();
            DrawDesignSettingsHierarchyTree();
            DrawDesignSettingsHierarchyLines();
            DrawDesignSettingsFolder();
            DrawDesignSettingsSeparator();
            DrawDesignSettingsLock();
            EditorGUILayout.EndScrollView();
            #endregion

            #region Footer
            if (GUILayout.Button("Update and Save Design Settings", GUILayout.Height(primaryButtonsHeight)))
            {
                UpdateAndSaveDesignSettingsData();
            }
            #endregion
        }

        private void DrawDesignSettingsComponentIcons()
        {
            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Component Icons", HierarchyDesigner_Shared_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempComponentIconsSize = HierarchyDesigner_Shared_GUI.DrawFloatSlider("Component Icons Size", designSettingslabelWidth, tempComponentIconsSize, 1.0f, 0.5f, 1.0f, true, "The size of component icons, where a value of 1 represents 100%.");
            tempComponentIconsOffset = HierarchyDesigner_Shared_GUI.DrawIntSlider("Component Icons Offset", designSettingslabelWidth, tempComponentIconsOffset, 21, 15, 30, true, "The horizontal offset position of component icons relative to their initial position, based on the Hierarchy Layout Mode.");
            tempComponentIconsSpacing = HierarchyDesigner_Shared_GUI.DrawFloatSlider("Component Icons Spacing", designSettingslabelWidth, tempComponentIconsSpacing, 2f, 0.0f, 10.0f, true, "The spacing between each component icon.");
            if (EditorGUI.EndChangeCheck()) { designSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void DrawDesignSettingsTag()
        {
            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Tag", HierarchyDesigner_Shared_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempTagColor = HierarchyDesigner_Shared_GUI.DrawColorField("Tag Color", designSettingslabelWidth, "#808080", tempTagColor, true, "The color of the GameObject's tag label.");
            tempTagFontSize = HierarchyDesigner_Shared_GUI.DrawIntSlider("Tag Font Size", designSettingslabelWidth, tempTagFontSize, 10, 7, 21, true, "The font size of the GameObject's tag label.");
            tempTagFontStyle = HierarchyDesigner_Shared_GUI.DrawEnumPopup("Tag Font Style", designSettingslabelWidth, tempTagFontStyle, FontStyle.BoldAndItalic, true, "The font style of the GameObject's tag label.");
            tempTagTextAnchor = HierarchyDesigner_Shared_GUI.DrawEnumPopup("Tag Text Anchor", designSettingslabelWidth, tempTagTextAnchor, TextAnchor.MiddleRight, true, "The text anchor of the GameObject's tag label.");
            if (EditorGUI.EndChangeCheck()) { designSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void DrawDesignSettingsLayer()
        {
            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Layer", HierarchyDesigner_Shared_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempLayerColor = HierarchyDesigner_Shared_GUI.DrawColorField("Layer Color", designSettingslabelWidth, "#808080", tempLayerColor, true, "The color of the GameObject's layer.");
            tempLayerFontSize = HierarchyDesigner_Shared_GUI.DrawIntSlider("Layer Font Size", designSettingslabelWidth, tempLayerFontSize, 10, 7, 21, true, "The font size of the GameObject's layer.");
            tempLayerFontStyle = HierarchyDesigner_Shared_GUI.DrawEnumPopup("Layer Font Style", designSettingslabelWidth, tempLayerFontStyle, FontStyle.BoldAndItalic, true, "The font style of the GameObject's layer.");
            tempLayerTextAnchor = HierarchyDesigner_Shared_GUI.DrawEnumPopup("Layer Text Anchor", designSettingslabelWidth, tempLayerTextAnchor, TextAnchor.MiddleLeft, true, "The text anchor of the GameObject's\nlayer.");
            if (EditorGUI.EndChangeCheck()) { designSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void DrawDesignSettingsTagAndLayer()
        {
            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Tag and Layer", HierarchyDesigner_Shared_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempTagLayerOffset = HierarchyDesigner_Shared_GUI.DrawIntSlider("Tag and Layer Offset", designSettingslabelWidth, tempTagLayerOffset, 5, 0, 20, true, "The horizontal offset position of the tag and layer labels relative to their initial position, based on the Hierarchy Layout Mode.");
            tempTagLayerSpacing = HierarchyDesigner_Shared_GUI.DrawIntSlider("Tag and Layer Spacing", designSettingslabelWidth, tempTagLayerSpacing, 5, 0, 20, true, "The spacing between the tag and layer labels.");
            if (EditorGUI.EndChangeCheck()) { designSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void DrawDesignSettingsHierarchyTree()
        {
            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Hierarchy Tree", HierarchyDesigner_Shared_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempHierarchyTreeColor = HierarchyDesigner_Shared_GUI.DrawColorField("Tree Color", designSettingslabelWidth, "#FFFFFF", tempHierarchyTreeColor, true, "The color of the Hierarchy Tree branches.");
            tempTreeBranchImageType_I = HierarchyDesigner_Shared_GUI.DrawEnumPopup("Tree Branch Image Type I", designSettingslabelWidth, tempTreeBranchImageType_I, HierarchyDesigner_Configurable_DesignSettings.TreeBranchImageType.Default, true, "The branch icon of the Hierarchy Tree's Branch Type I.");
            tempTreeBranchImageType_L = HierarchyDesigner_Shared_GUI.DrawEnumPopup("Tree Branch Image Type L", designSettingslabelWidth, tempTreeBranchImageType_L, HierarchyDesigner_Configurable_DesignSettings.TreeBranchImageType.Default, true, "The branch icon of the Hierarchy Tree's Branch Type L.");
            tempTreeBranchImageType_T = HierarchyDesigner_Shared_GUI.DrawEnumPopup("Tree Branch Image Type T", designSettingslabelWidth, tempTreeBranchImageType_T, HierarchyDesigner_Configurable_DesignSettings.TreeBranchImageType.Default, true, "The branch icon of the Hierarchy Tree's Branch Type T.");
            tempTreeBranchImageType_TerminalBud = HierarchyDesigner_Shared_GUI.DrawEnumPopup("Tree Branch Image Type T-Bud", designSettingslabelWidth, tempTreeBranchImageType_TerminalBud, HierarchyDesigner_Configurable_DesignSettings.TreeBranchImageType.Default, true, "The branch icon of the Hierarchy Tree's Branch Type T-Bud.");
            if (EditorGUI.EndChangeCheck()) { designSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void DrawDesignSettingsHierarchyLines()
        {
            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Hierarchy Lines", HierarchyDesigner_Shared_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempHierarchyLineColor = HierarchyDesigner_Shared_GUI.DrawColorField("Lines Color", designSettingslabelWidth, "#00000080", tempHierarchyLineColor, true, "The color of the Hierarchy Lines.");
            tempHierarchyLineThickness = HierarchyDesigner_Shared_GUI.DrawIntSlider("Lines Thickness", designSettingslabelWidth, tempHierarchyLineThickness, 1, 1, 3, true, "The thickness of the Hierarchy Lines.");
            if (EditorGUI.EndChangeCheck()) { designSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void DrawDesignSettingsFolder()
        {
            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Folder", HierarchyDesigner_Shared_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempFolderDefaultTextColor = HierarchyDesigner_Shared_GUI.DrawColorField("Default Text Color", designSettingslabelWidth, "#FFFFFF", tempFolderDefaultTextColor, true, "The text color for folders that are not present in your folder list, as well as the default text color value in the folder creation field.");
            tempFolderDefaultFontSize = HierarchyDesigner_Shared_GUI.DrawIntSlider("Default Font Size", designSettingslabelWidth, tempFolderDefaultFontSize, 12, 7, 21, true, "The font size for folders that are not present in your folder list, as well as the default font size value in the folder creation field.");
            tempFolderDefaultFontStyle = HierarchyDesigner_Shared_GUI.DrawEnumPopup("Default Font Style", designSettingslabelWidth, tempFolderDefaultFontStyle, FontStyle.Normal, true, "The font style for folders that are not present in your folder list, as well as the default font style value in the folder creation field.");
            tempFolderDefaultImageColor = HierarchyDesigner_Shared_GUI.DrawColorField("Default Image Color", designSettingslabelWidth, "#FFFFFF", tempFolderDefaultImageColor, true, "The image color for folders that are not present in your folder list, as well as the default image color value in the folder creation field.");
            tempFolderDefaultImageType = HierarchyDesigner_Shared_GUI.DrawEnumPopup("Default Image Type", designSettingslabelWidth, tempFolderDefaultImageType, HierarchyDesigner_Configurable_Folders.FolderImageType.Default, true, "The image type for folders that are not present in your folder list, as well as the default image type value in the folder creation field.");
            if (EditorGUI.EndChangeCheck()) { designSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void DrawDesignSettingsSeparator()
        {
            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Separator", HierarchyDesigner_Shared_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempSeparatorDefaultTextColor = HierarchyDesigner_Shared_GUI.DrawColorField("Default Text Color", designSettingslabelWidth, "#FFFFFF", tempSeparatorDefaultTextColor, true, "The text color for separators that are not present in your separators list, as well as the default text color value in the separator creation field.");
            tempSeparatorDefaultIsGradientBackground = HierarchyDesigner_Shared_GUI.DrawToggle("Default Is Gradient Background", designSettingslabelWidth, tempSeparatorDefaultIsGradientBackground, false, true, "The is gradient background for separators that are not present in your separators list, as well as the default is gradient background value in the separator creation field.");
            tempSeparatorDefaultBackgroundColor = HierarchyDesigner_Shared_GUI.DrawColorField("Default Background Color", designSettingslabelWidth, "#808080", tempSeparatorDefaultBackgroundColor, true, "The background color for separators that are not present in your separators list, as well as the default background color value in the separator creation field.");
            tempSeparatorDefaultBackgroundGradient = HierarchyDesigner_Shared_GUI.DrawGradientField("Default Background Gradient", designSettingslabelWidth, new(), tempSeparatorDefaultBackgroundGradient ?? new(), true, "The background gradient for separators that are not present in your separators list, as well as the default background gradient value in the separator creation field.");
            tempSeparatorDefaultFontSize = HierarchyDesigner_Shared_GUI.DrawIntSlider("Default Font Size", designSettingslabelWidth, tempSeparatorDefaultFontSize, 12, 7, 21, true, "The font size for separators that are not present in your separators list, as well as the default font size value in the separator creation field.");
            tempSeparatorDefaultFontStyle = HierarchyDesigner_Shared_GUI.DrawEnumPopup("Default Font Style", designSettingslabelWidth, tempSeparatorDefaultFontStyle, FontStyle.Normal, true, "The font style for separators that are not present in your separators list, as well as the default font style value in the separator creation field.");
            tempSeparatorDefaultTextAnchor = HierarchyDesigner_Shared_GUI.DrawEnumPopup("Default Text Anchor", designSettingslabelWidth, tempSeparatorDefaultTextAnchor, TextAnchor.MiddleCenter, true, "The text anchor for separators that are not present in your separators list, as well as the default text anchor value in the separator creation field.");
            tempSeparatorDefaultImageType = HierarchyDesigner_Shared_GUI.DrawEnumPopup("Default Image Type", designSettingslabelWidth, tempSeparatorDefaultImageType, HierarchyDesigner_Configurable_Separators.SeparatorImageType.Default, true, "The image type for separators that are not present in your separators list, as well as the default image type value in the separator creation field.");
            tempSeparatorLeftSideTextAnchorOffset = HierarchyDesigner_Shared_GUI.DrawIntSlider("Left Side Text Anchor Offset", designSettingslabelWidth, tempSeparatorLeftSideTextAnchorOffset, 3, 0, 33, true, "The horizontal left-side offset for separators with the following text anchor values: Upper Left, Middle Left, and Lower Left.");
            tempSeparatorRightSideTextAnchorOffset = HierarchyDesigner_Shared_GUI.DrawIntSlider("Right Side Text Anchor Offset", designSettingslabelWidth, tempSeparatorRightSideTextAnchorOffset, 36, 33, 66, true, "The horizontal right-side offset for separators with the following text anchor values: Upper Right, Middle Right, and Lower Right.");
            if (EditorGUI.EndChangeCheck()) { designSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void DrawDesignSettingsLock()
        {
            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Lock Label", HierarchyDesigner_Shared_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempLockColor = HierarchyDesigner_Shared_GUI.DrawColorField("Text Color", designSettingslabelWidth, "#FFFFFF", tempLockColor, true, "The color of the Lock label.");
            tempLockFontSize = HierarchyDesigner_Shared_GUI.DrawIntSlider("Font Size", designSettingslabelWidth, tempLockFontSize, 11, 7, 21, true, "The font size of the Lock label.");
            tempLockFontStyle = HierarchyDesigner_Shared_GUI.DrawEnumPopup("Font Style", designSettingslabelWidth, tempLockFontStyle, FontStyle.BoldAndItalic, true, "The font style of the Lock label.");
            tempLockTextAnchor = HierarchyDesigner_Shared_GUI.DrawEnumPopup("Text Anchor", designSettingslabelWidth, tempLockTextAnchor, TextAnchor.MiddleCenter, true, "The text anchor of the Lock label.");
            if (EditorGUI.EndChangeCheck()) { designSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void UpdateAndSaveDesignSettingsData()
        {
            HierarchyDesigner_Configurable_DesignSettings.ComponentIconsSize = tempComponentIconsSize;
            HierarchyDesigner_Configurable_DesignSettings.ComponentIconsOffset = tempComponentIconsOffset;
            HierarchyDesigner_Configurable_DesignSettings.ComponentIconsSpacing = tempComponentIconsSpacing;
            HierarchyDesigner_Configurable_DesignSettings.HierarchyTreeColor = tempHierarchyTreeColor;
            HierarchyDesigner_Configurable_DesignSettings.TreeBranchImageType_I = tempTreeBranchImageType_I;
            HierarchyDesigner_Configurable_DesignSettings.TreeBranchImageType_L = tempTreeBranchImageType_L;
            HierarchyDesigner_Configurable_DesignSettings.TreeBranchImageType_T = tempTreeBranchImageType_T;
            HierarchyDesigner_Configurable_DesignSettings.TreeBranchImageType_TerminalBud = tempTreeBranchImageType_TerminalBud;
            HierarchyDesigner_Configurable_DesignSettings.TagColor = tempTagColor;
            HierarchyDesigner_Configurable_DesignSettings.TagTextAnchor = tempTagTextAnchor;
            HierarchyDesigner_Configurable_DesignSettings.TagFontStyle = tempTagFontStyle;
            HierarchyDesigner_Configurable_DesignSettings.TagFontSize = tempTagFontSize;
            HierarchyDesigner_Configurable_DesignSettings.LayerColor = tempLayerColor;
            HierarchyDesigner_Configurable_DesignSettings.LayerTextAnchor = tempLayerTextAnchor;
            HierarchyDesigner_Configurable_DesignSettings.LayerFontStyle = tempLayerFontStyle;
            HierarchyDesigner_Configurable_DesignSettings.LayerFontSize = tempLayerFontSize;
            HierarchyDesigner_Configurable_DesignSettings.TagLayerOffset = tempTagLayerOffset;
            HierarchyDesigner_Configurable_DesignSettings.TagLayerSpacing = tempTagLayerSpacing;
            HierarchyDesigner_Configurable_DesignSettings.HierarchyLineColor = tempHierarchyLineColor;
            HierarchyDesigner_Configurable_DesignSettings.HierarchyLineThickness = tempHierarchyLineThickness;
            HierarchyDesigner_Configurable_DesignSettings.FolderDefaultTextColor = tempFolderDefaultTextColor;
            HierarchyDesigner_Configurable_DesignSettings.FolderDefaultFontSize = tempFolderDefaultFontSize;
            HierarchyDesigner_Configurable_DesignSettings.FolderDefaultFontStyle = tempFolderDefaultFontStyle;
            HierarchyDesigner_Configurable_DesignSettings.FolderDefaultImageColor = tempFolderDefaultImageColor;
            HierarchyDesigner_Configurable_DesignSettings.FolderDefaultImageType = tempFolderDefaultImageType;
            HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultTextColor = tempSeparatorDefaultTextColor;
            HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultIsGradientBackground = tempSeparatorDefaultIsGradientBackground;
            HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultBackgroundColor = tempSeparatorDefaultBackgroundColor;
            HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultBackgroundGradient = tempSeparatorDefaultBackgroundGradient;
            HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultFontSize = tempSeparatorDefaultFontSize;
            HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultFontStyle = tempSeparatorDefaultFontStyle;
            HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultTextAnchor = tempSeparatorDefaultTextAnchor;
            HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultImageType = tempSeparatorDefaultImageType;
            HierarchyDesigner_Configurable_DesignSettings.SeparatorLeftSideTextAnchorOffset = tempSeparatorLeftSideTextAnchorOffset;
            HierarchyDesigner_Configurable_DesignSettings.SeparatorRightSideTextAnchorOffset = tempSeparatorRightSideTextAnchorOffset;
            HierarchyDesigner_Configurable_DesignSettings.LockColor = tempLockColor;
            HierarchyDesigner_Configurable_DesignSettings.LockTextAnchor = tempLockTextAnchor;
            HierarchyDesigner_Configurable_DesignSettings.LockFontStyle = tempLockFontStyle;
            HierarchyDesigner_Configurable_DesignSettings.LockFontSize = tempLockFontSize;
            HierarchyDesigner_Configurable_DesignSettings.SaveSettings();
            designSettingsHasModifiedChanges = false;
        }

        private void LoadDesignSettingsData()
        {
            tempComponentIconsSize = HierarchyDesigner_Configurable_DesignSettings.ComponentIconsSize;
            tempComponentIconsOffset = HierarchyDesigner_Configurable_DesignSettings.ComponentIconsOffset;
            tempComponentIconsSpacing = HierarchyDesigner_Configurable_DesignSettings.ComponentIconsSpacing;
            tempHierarchyTreeColor = HierarchyDesigner_Configurable_DesignSettings.HierarchyTreeColor;
            tempTreeBranchImageType_I = HierarchyDesigner_Configurable_DesignSettings.TreeBranchImageType_I;
            tempTreeBranchImageType_L = HierarchyDesigner_Configurable_DesignSettings.TreeBranchImageType_L;
            tempTreeBranchImageType_T = HierarchyDesigner_Configurable_DesignSettings.TreeBranchImageType_T;
            tempTreeBranchImageType_TerminalBud = HierarchyDesigner_Configurable_DesignSettings.TreeBranchImageType_TerminalBud;
            tempTagColor = HierarchyDesigner_Configurable_DesignSettings.TagColor;
            tempTagTextAnchor = HierarchyDesigner_Configurable_DesignSettings.TagTextAnchor;
            tempTagFontStyle = HierarchyDesigner_Configurable_DesignSettings.TagFontStyle;
            tempTagFontSize = HierarchyDesigner_Configurable_DesignSettings.TagFontSize;
            tempLayerColor = HierarchyDesigner_Configurable_DesignSettings.LayerColor;
            tempLayerTextAnchor = HierarchyDesigner_Configurable_DesignSettings.LayerTextAnchor;
            tempLayerFontStyle = HierarchyDesigner_Configurable_DesignSettings.LayerFontStyle;
            tempLayerFontSize = HierarchyDesigner_Configurable_DesignSettings.LayerFontSize;
            tempTagLayerOffset = HierarchyDesigner_Configurable_DesignSettings.TagLayerOffset;
            tempTagLayerSpacing = HierarchyDesigner_Configurable_DesignSettings.TagLayerSpacing;
            tempHierarchyLineColor = HierarchyDesigner_Configurable_DesignSettings.HierarchyLineColor;
            tempHierarchyLineThickness = HierarchyDesigner_Configurable_DesignSettings.HierarchyLineThickness;
            tempFolderDefaultTextColor = HierarchyDesigner_Configurable_DesignSettings.FolderDefaultTextColor;
            tempFolderDefaultFontSize = HierarchyDesigner_Configurable_DesignSettings.FolderDefaultFontSize;
            tempFolderDefaultFontStyle = HierarchyDesigner_Configurable_DesignSettings.FolderDefaultFontStyle;
            tempFolderDefaultImageColor = HierarchyDesigner_Configurable_DesignSettings.FolderDefaultImageColor;
            tempFolderDefaultImageType = HierarchyDesigner_Configurable_DesignSettings.FolderDefaultImageType;
            tempSeparatorDefaultTextColor = HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultTextColor;
            tempSeparatorDefaultIsGradientBackground = HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultIsGradientBackground;
            tempSeparatorDefaultBackgroundColor = HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultBackgroundColor;
            tempSeparatorDefaultBackgroundGradient = HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultBackgroundGradient;
            tempSeparatorDefaultFontSize = HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultFontSize;
            tempSeparatorDefaultFontStyle = HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultFontStyle;
            tempSeparatorDefaultTextAnchor = HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultTextAnchor;
            tempSeparatorDefaultImageType = HierarchyDesigner_Configurable_DesignSettings.SeparatorDefaultImageType;
            tempSeparatorLeftSideTextAnchorOffset = HierarchyDesigner_Configurable_DesignSettings.SeparatorLeftSideTextAnchorOffset;
            tempSeparatorRightSideTextAnchorOffset = HierarchyDesigner_Configurable_DesignSettings.SeparatorRightSideTextAnchorOffset;
            tempLockColor = HierarchyDesigner_Configurable_DesignSettings.LockColor;
            tempLockTextAnchor = HierarchyDesigner_Configurable_DesignSettings.LockTextAnchor;
            tempLockFontStyle = HierarchyDesigner_Configurable_DesignSettings.LockFontStyle;
            tempLockFontSize = HierarchyDesigner_Configurable_DesignSettings.LockFontSize;
        }
        #endregion

        #region Shortcut Settings
        private void DrawShortcutSettingsTab()
        {
            #region Body
            shortcutSettingsMainScroll = EditorGUILayout.BeginScrollView(shortcutSettingsMainScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            DrawMajorShortcuts();
            DrawMinorShortcuts();
            EditorGUILayout.EndScrollView();
            #endregion

            #region Footer
            EditorGUILayout.BeginVertical();
            if (GUILayout.Button("Open Shortcut Manager", GUILayout.Height(primaryButtonsHeight)))
            {
                EditorApplication.ExecuteMenuItem("Edit/Shortcuts...");
            }
            if (GUILayout.Button("Update and Save Major Shortcut Settings", GUILayout.Height(primaryButtonsHeight)))
            {
                UpdateAndSaveShortcutSettingsData();
            }
            EditorGUILayout.EndVertical();
            #endregion
        }

        private void DrawMajorShortcuts()
        {
            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Major Shortcuts", HierarchyDesigner_Shared_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempToggleGameObjectActiveStateKeyCode = HierarchyDesigner_Shared_GUI.DrawEnumPopup("Toggle GameObject Active State Key Code", majorShortcutEnumToggleLabelWidth, tempToggleGameObjectActiveStateKeyCode, KeyCode.Mouse2, true, "The key code to toggle the active state of the hovered GameObject or selected GameObjects. This input must be entered within the Hierarchy window, as it is only detected while interacting with the Hierarchy.");
            tempToggleLockStateKeyCode = HierarchyDesigner_Shared_GUI.DrawEnumPopup("Toggle GameObject Lock State Key Code", majorShortcutEnumToggleLabelWidth, tempToggleLockStateKeyCode, KeyCode.F1, true, "The key code to toggle the lock state of the hovered GameObject or selected GameObjects.\n\nNote: The Hierarchy window must be focused for this to work.");
            tempChangeTagLayerKeyCode = HierarchyDesigner_Shared_GUI.DrawEnumPopup("Change Selected Tag, Layer Key Code", majorShortcutEnumToggleLabelWidth, tempChangeTagLayerKeyCode, KeyCode.F2, true, "The key code to change the current tag or layer of a GameObject. Hover over the tag or layer and press the key code to apply\n\nNote: The Hierarchy window must be focused for this to work.");
            tempRenameSelectedGameObjectsKeyCode = HierarchyDesigner_Shared_GUI.DrawEnumPopup("Rename Selected GameObjects Key Code", majorShortcutEnumToggleLabelWidth, tempRenameSelectedGameObjectsKeyCode, KeyCode.F3, true, "The key code to rename the selected GameObject(s).\n\nNote: The Hierarchy window must be focused for this to work.");
            if (EditorGUI.EndChangeCheck()) { shortcutSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void DrawMinorShortcuts()
        {
            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Minor Shortcuts", HierarchyDesigner_Shared_GUI.FieldsCategoryLabelStyle);
            GUILayout.Space(defaultMarginSpacing);

            minorShortcutSettingsScroll = EditorGUILayout.BeginScrollView(minorShortcutSettingsScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            foreach (string shortcutId in minorShortcutIdentifiers)
            {
                ShortcutBinding currentBinding = ShortcutManager.instance.GetShortcutBinding(shortcutId);
                string[] parts = shortcutId.Split('/');
                string commandName = parts[^1];

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(commandName + ":", HierarchyDesigner_Shared_GUI.LayoutLabelStyle, GUILayout.Width(minorShortcutCommandLabelWidth));

                bool hasKeyCombination = false;
                foreach (KeyCombination kc in currentBinding.keyCombinationSequence)
                {
                    if (!hasKeyCombination)
                    {
                        hasKeyCombination = true;
                        GUILayout.Label(kc.ToString(), HierarchyDesigner_Shared_GUI.AssignedLabelStyle, GUILayout.MinWidth(minorShortcutLabelWidth));
                    }
                    else
                    {
                        GUILayout.Label(" + " + kc.ToString(), HierarchyDesigner_Shared_GUI.AssignedLabelStyle, GUILayout.MinWidth(minorShortcutLabelWidth));
                    }
                }
                if (!hasKeyCombination)
                {
                    GUILayout.Label("unassigned shortcut", HierarchyDesigner_Shared_GUI.UnassignedLabelStyle, GUILayout.MinWidth(minorShortcutLabelWidth));
                }

                EditorGUILayout.EndHorizontal();
                GUILayout.Space(4);
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            EditorGUILayout.HelpBox("To modify minor shortcuts, please go to: Edit/Shortcuts.../Hierarchy Designer.\nYou can click the button below for quick access, then in the category section, search for Hierarchy Designer.", MessageType.Info);
        }

        private void UpdateAndSaveShortcutSettingsData()
        {
            HierarchyDesigner_Configurable_ShortcutSettings.ToggleGameObjectActiveStateKeyCode = tempToggleGameObjectActiveStateKeyCode;
            HierarchyDesigner_Configurable_ShortcutSettings.ToggleLockStateKeyCode = tempToggleLockStateKeyCode;
            HierarchyDesigner_Configurable_ShortcutSettings.ChangeTagLayerKeyCode = tempChangeTagLayerKeyCode;
            HierarchyDesigner_Configurable_ShortcutSettings.RenameSelectedGameObjectsKeyCode = tempRenameSelectedGameObjectsKeyCode;
            HierarchyDesigner_Configurable_ShortcutSettings.SaveSettings();
            shortcutSettingsHasModifiedChanges = false;
        }

        private void LoadShortcutSettingsData()
        {
            tempToggleGameObjectActiveStateKeyCode = HierarchyDesigner_Configurable_ShortcutSettings.ToggleGameObjectActiveStateKeyCode;
            tempToggleLockStateKeyCode = HierarchyDesigner_Configurable_ShortcutSettings.ToggleLockStateKeyCode;
            tempChangeTagLayerKeyCode = HierarchyDesigner_Configurable_ShortcutSettings.ChangeTagLayerKeyCode;
            tempRenameSelectedGameObjectsKeyCode = HierarchyDesigner_Configurable_ShortcutSettings.RenameSelectedGameObjectsKeyCode;
        }
        #endregion

        #region Advanced Settings
        private void DrawAdvancedSettingsTab()
        {
            #region Body
            advancedSettingsMainScroll = EditorGUILayout.BeginScrollView(advancedSettingsMainScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            DrawAdvancedCoreFeatures();
            DrawAdvancedMainIconFeatures();
            DrawAdvancedComponentIconsFeatures();
            DrawAdvancedFolderFeatures();
            DrawAdvancedSeparatorFeatures();
            DrawAdvancedHierarchyToolsFeatures();
            EditorGUILayout.EndScrollView();
            #endregion

            #region Footer
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Enable All Features", GUILayout.Height(secondaryButtonsHeight)))
            {
                EnableAllAdvancedSettingsFeatures(true);
            }
            if (GUILayout.Button("Disable All Features", GUILayout.Height(secondaryButtonsHeight)))
            {
                EnableAllAdvancedSettingsFeatures(false);
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Update and Save Advanced Settings", GUILayout.Height(primaryButtonsHeight)))
            {
                UpdateAndSaveAdvancedSettingsData();
            }
            #endregion
        }

        private void DrawAdvancedCoreFeatures()
        {
            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Core Features", HierarchyDesigner_Shared_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempHierarchyLocation = HierarchyDesigner_Shared_GUI.DrawEnumPopup("Hierarchy Designer Location", advancedSettingsEnumPopupLabelWidth, tempHierarchyLocation, HierarchyDesigner_Configurable_AdvancedSettings.HierarchyDesignerLocation.Tools, true, "The location of Hierarchy Designer in the top menu bar (e.g., Tools/Hierarchy Designer, Plugins/Hierarchy Designer, etc.).\n\nNote: Modifying this setting will trigger a script recompilation.");
            tempMainIconUpdateMode = HierarchyDesigner_Shared_GUI.DrawEnumPopup("Main Icon Update Mode", advancedSettingsEnumPopupLabelWidth, tempMainIconUpdateMode, HierarchyDesigner_Configurable_AdvancedSettings.UpdateMode.Dynamic, true, "The update mode for the Main Icon feature:\n\nDynamic: Checks for changes dynamically during Hierarchy events.\n\nSmart: Checks periodically, such as during scene open/reload or script recompilation.\n\nNote: In Smart mode, you can manually check for changes by refreshing through the context menus or using minor shortcuts.");
            tempComponentsIconsUpdateMode = HierarchyDesigner_Shared_GUI.DrawEnumPopup("Component Icons Update Mode", advancedSettingsEnumPopupLabelWidth, tempComponentsIconsUpdateMode, HierarchyDesigner_Configurable_AdvancedSettings.UpdateMode.Dynamic, true, "The update mode for the Component Icons feature:\n\nDynamic: Checks for changes dynamically during Hierarchy events.\n\nSmart: Checks periodically, such as during scene open/reload or script recompilation.\n\nNote: In Smart mode, you can manually check for changes by refreshing through the context menus or using minor shortcuts.");
            tempHierarchyTreeUpdateMode = HierarchyDesigner_Shared_GUI.DrawEnumPopup("Hierarchy Tree Update Mode", advancedSettingsEnumPopupLabelWidth, tempHierarchyTreeUpdateMode, HierarchyDesigner_Configurable_AdvancedSettings.UpdateMode.Dynamic, true, "The update mode for the Hierarchy Tree feature:\n\nDynamic: Checks for changes dynamically during Hierarchy events.\n\nSmart: Checks periodically, such as during scene open/reload or script recompilation.\n\nNote: In Smart mode, you can manually check for changes by refreshing through the context menus or using minor shortcuts.");
            tempTagUpdateMode = HierarchyDesigner_Shared_GUI.DrawEnumPopup("Tag Update Mode", advancedSettingsEnumPopupLabelWidth, tempTagUpdateMode, HierarchyDesigner_Configurable_AdvancedSettings.UpdateMode.Dynamic, true, "The update mode for the Tag feature:\n\nDynamic: Checks for changes dynamically during Hierarchy events.\n\nSmart: Checks periodically, such as during scene open/reload or script recompilation.\n\nNote: In Smart mode, you can manually check for changes by refreshing through the context menus or using minor shortcuts.");
            tempLayerUpdateMode = HierarchyDesigner_Shared_GUI.DrawEnumPopup("Layer Update Mode", advancedSettingsEnumPopupLabelWidth, tempLayerUpdateMode, HierarchyDesigner_Configurable_AdvancedSettings.UpdateMode.Dynamic, true, "The update mode for the Layer feature:\n\nDynamic: Checks for changes dynamically during Hierarchy events.\n\nSmart: Checks periodically, such as during scene open/reload or script recompilation.\n\nNote: In Smart mode, you can manually check for changes by refreshing through the context menus or using minor shortcuts.");
            if (EditorGUI.EndChangeCheck()) { advancedSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void DrawAdvancedMainIconFeatures()
        {
            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("GameObject's Main Icon", HierarchyDesigner_Shared_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempEnableDynamicBackgroundForGameObjectMainIcon = HierarchyDesigner_Shared_GUI.DrawToggle("Enable Dynamic Background", advancedSettingsToggleLabelWidth, tempEnableDynamicBackgroundForGameObjectMainIcon, true, true, "The background of the main icon will match the background color of the Hierarchy window (i.e., Editor Light, Dark Mode, GameObject Selected, Focused, Unfocused).");
            tempEnablePreciseRectForDynamicBackgroundForGameObjectMainIcon = HierarchyDesigner_Shared_GUI.DrawToggle("Enable Precise Rect For Dynamic Background", advancedSettingsToggleLabelWidth, tempEnablePreciseRectForDynamicBackgroundForGameObjectMainIcon, true, true, "Uses precise rect calculations for pointer/mouse detection utilized by the Dynamic Background feature.");
            if (EditorGUI.EndChangeCheck()) { advancedSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void DrawAdvancedComponentIconsFeatures()
        {
            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("GameObject's Component Icons", HierarchyDesigner_Shared_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempEnableCustomizationForGameObjectComponentIcons = HierarchyDesigner_Shared_GUI.DrawToggle("Enable Design Customization For Component Icons", advancedSettingsToggleLabelWidth, tempEnableCustomizationForGameObjectComponentIcons, true, true, "Enables calculation of component icon design settings (e.g., Component Icon Size, Offset, and Spacing).\n\nNote: If you are using the default values, you may turn this off to reduce extra calculations in the component icon logic.");
            tempEnableTooltipOnComponentIconHovered = HierarchyDesigner_Shared_GUI.DrawToggle("Enable Tooltip For Component Icons", advancedSettingsToggleLabelWidth, tempEnableTooltipOnComponentIconHovered, true, true, "Displays the component name when hovering over the component icon.");
            tempEnableActiveStateEffectForComponentIcons = HierarchyDesigner_Shared_GUI.DrawToggle("Enable Active State Effect For Component Icons", advancedSettingsToggleLabelWidth, tempEnableActiveStateEffectForComponentIcons, true, true, "Displays which components are disabled for a given object.");
            tempDisableComponentIconsForInactiveGameObjects = HierarchyDesigner_Shared_GUI.DrawToggle("Disable Component Icons For Inactive GameObjects", advancedSettingsToggleLabelWidth, tempDisableComponentIconsForInactiveGameObjects, true, true, "Hides component icons for inactive GameObjects.");
            if (EditorGUI.EndChangeCheck()) { advancedSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void DrawAdvancedFolderFeatures()
        {
            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Folders", HierarchyDesigner_Shared_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempIncludeEditorUtilitiesForHierarchyDesignerRuntimeFolder = HierarchyDesigner_Shared_GUI.DrawToggle("Include Editor Utilities For Hierarchy Designer's Folder", advancedSettingsToggleLabelWidth, tempIncludeEditorUtilitiesForHierarchyDesignerRuntimeFolder, true, true, "Enables editor-only utilities (e.g., Toggle Active State, Delete, Children List, etc.) in the inspector window for Folder GameObjects.");
            if (EditorGUI.EndChangeCheck()) { advancedSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void DrawAdvancedSeparatorFeatures()
        {
            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Separators", HierarchyDesigner_Shared_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempIncludeBackgroundImageForGradientBackground = HierarchyDesigner_Shared_GUI.DrawToggle("Include Background Image For Gradient Background", advancedSettingsToggleLabelWidth, tempIncludeBackgroundImageForGradientBackground, true, true, "Includes the Background Image Type for Separators that uses a gradient background. The background image type will be used first, followed by the gradient placed on top.");
            if (EditorGUI.EndChangeCheck()) { advancedSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void DrawAdvancedHierarchyToolsFeatures()
        {
            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Hierarchy Tools", HierarchyDesigner_Shared_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempExcludeFoldersFromCountSelectToolCalculations = HierarchyDesigner_Shared_GUI.DrawToggle("Exclude Folders From Count-Select Tool Calculations", advancedSettingsToggleLabelWidth, tempExcludeFoldersFromCountSelectToolCalculations, true, true, "Excludes Folder GameObjects from Count and Select tool calculations.");
            tempExcludeSeparatorsFromCountSelectToolCalculations = HierarchyDesigner_Shared_GUI.DrawToggle("Exclude Separators From Count-Select Tool Calculations", advancedSettingsToggleLabelWidth, tempExcludeSeparatorsFromCountSelectToolCalculations, true, true, "Excludes Separator GameObjects from Count and Select tool calculations.");
            if (EditorGUI.EndChangeCheck()) { advancedSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void UpdateAndSaveAdvancedSettingsData()
        {
            bool hierarchyLocationChanged = tempHierarchyLocation != HierarchyDesigner_Configurable_AdvancedSettings.HierarchyLocation;

            HierarchyDesigner_Configurable_AdvancedSettings.HierarchyLocation = tempHierarchyLocation;
            HierarchyDesigner_Configurable_AdvancedSettings.MainIconUpdateMode = tempMainIconUpdateMode;
            HierarchyDesigner_Configurable_AdvancedSettings.ComponentsIconsUpdateMode = tempComponentsIconsUpdateMode;
            HierarchyDesigner_Configurable_AdvancedSettings.HierarchyTreeUpdateMode = tempHierarchyTreeUpdateMode;
            HierarchyDesigner_Configurable_AdvancedSettings.TagUpdateMode = tempTagUpdateMode;
            HierarchyDesigner_Configurable_AdvancedSettings.LayerUpdateMode = tempLayerUpdateMode;
            HierarchyDesigner_Configurable_AdvancedSettings.EnableDynamicBackgroundForGameObjectMainIcon = tempEnableDynamicBackgroundForGameObjectMainIcon;
            HierarchyDesigner_Configurable_AdvancedSettings.EnablePreciseRectForDynamicBackgroundForGameObjectMainIcon = tempEnablePreciseRectForDynamicBackgroundForGameObjectMainIcon;
            HierarchyDesigner_Configurable_AdvancedSettings.EnableCustomizationForGameObjectComponentIcons = tempEnableCustomizationForGameObjectComponentIcons;
            HierarchyDesigner_Configurable_AdvancedSettings.EnableTooltipOnComponentIconHovered = tempEnableTooltipOnComponentIconHovered;
            HierarchyDesigner_Configurable_AdvancedSettings.EnableActiveStateEffectForComponentIcons = tempEnableActiveStateEffectForComponentIcons;
            HierarchyDesigner_Configurable_AdvancedSettings.DisableComponentIconsForInactiveGameObjects = tempDisableComponentIconsForInactiveGameObjects;
            HierarchyDesigner_Configurable_AdvancedSettings.IncludeEditorUtilitiesForHierarchyDesignerRuntimeFolder = tempIncludeEditorUtilitiesForHierarchyDesignerRuntimeFolder;
            HierarchyDesigner_Configurable_AdvancedSettings.IncludeBackgroundImageForGradientBackground = tempIncludeBackgroundImageForGradientBackground;
            HierarchyDesigner_Configurable_AdvancedSettings.ExcludeFoldersFromCountSelectToolCalculations = tempExcludeFoldersFromCountSelectToolCalculations;
            HierarchyDesigner_Configurable_AdvancedSettings.ExcludeSeparatorsFromCountSelectToolCalculations = tempExcludeSeparatorsFromCountSelectToolCalculations;
            HierarchyDesigner_Configurable_AdvancedSettings.SaveSettings();
            advancedSettingsHasModifiedChanges = false;

            if (hierarchyLocationChanged)
            {
                HierarchyDesigner_Configurable_AdvancedSettings.GenerateConstantsFile(tempHierarchyLocation);
            }
        }

        private void LoadAdvancedSettingsData()
        {
            tempHierarchyLocation = HierarchyDesigner_Configurable_AdvancedSettings.HierarchyLocation;
            tempMainIconUpdateMode = HierarchyDesigner_Configurable_AdvancedSettings.MainIconUpdateMode;
            tempComponentsIconsUpdateMode = HierarchyDesigner_Configurable_AdvancedSettings.ComponentsIconsUpdateMode;
            tempHierarchyTreeUpdateMode = HierarchyDesigner_Configurable_AdvancedSettings.HierarchyTreeUpdateMode;
            tempTagUpdateMode = HierarchyDesigner_Configurable_AdvancedSettings.TagUpdateMode;
            tempLayerUpdateMode = HierarchyDesigner_Configurable_AdvancedSettings.LayerUpdateMode;
            tempEnableDynamicBackgroundForGameObjectMainIcon = HierarchyDesigner_Configurable_AdvancedSettings.EnableDynamicBackgroundForGameObjectMainIcon;
            tempEnablePreciseRectForDynamicBackgroundForGameObjectMainIcon = HierarchyDesigner_Configurable_AdvancedSettings.EnablePreciseRectForDynamicBackgroundForGameObjectMainIcon;
            tempEnableCustomizationForGameObjectComponentIcons = HierarchyDesigner_Configurable_AdvancedSettings.EnableCustomizationForGameObjectComponentIcons;
            tempEnableTooltipOnComponentIconHovered = HierarchyDesigner_Configurable_AdvancedSettings.EnableTooltipOnComponentIconHovered;
            tempEnableActiveStateEffectForComponentIcons = HierarchyDesigner_Configurable_AdvancedSettings.EnableActiveStateEffectForComponentIcons;
            tempDisableComponentIconsForInactiveGameObjects = HierarchyDesigner_Configurable_AdvancedSettings.DisableComponentIconsForInactiveGameObjects;
            tempIncludeEditorUtilitiesForHierarchyDesignerRuntimeFolder = HierarchyDesigner_Configurable_AdvancedSettings.IncludeEditorUtilitiesForHierarchyDesignerRuntimeFolder;
            tempIncludeBackgroundImageForGradientBackground = HierarchyDesigner_Configurable_AdvancedSettings.IncludeBackgroundImageForGradientBackground;
            tempExcludeFoldersFromCountSelectToolCalculations = HierarchyDesigner_Configurable_AdvancedSettings.ExcludeFoldersFromCountSelectToolCalculations;
            tempExcludeSeparatorsFromCountSelectToolCalculations = HierarchyDesigner_Configurable_AdvancedSettings.ExcludeSeparatorsFromCountSelectToolCalculations;
        }

        private void EnableAllAdvancedSettingsFeatures(bool enable)
        {
            tempEnableDynamicBackgroundForGameObjectMainIcon = enable;
            tempEnablePreciseRectForDynamicBackgroundForGameObjectMainIcon = enable;
            tempEnableCustomizationForGameObjectComponentIcons = enable;
            tempEnableTooltipOnComponentIconHovered = enable;
            tempEnableActiveStateEffectForComponentIcons = enable;
            tempDisableComponentIconsForInactiveGameObjects = enable;
            tempIncludeEditorUtilitiesForHierarchyDesignerRuntimeFolder = enable;
            tempIncludeBackgroundImageForGradientBackground = enable;
            tempExcludeFoldersFromCountSelectToolCalculations = enable;
            tempExcludeSeparatorsFromCountSelectToolCalculations = enable;
            advancedSettingsHasModifiedChanges = true;
        }
        #endregion
        #endregion

        private void OnDestroy()
        {
            string message = "The following settings have been modified: ";
            List<string> modifiedSettingsList = new();

            if (folderHasModifiedChanges) modifiedSettingsList.Add("Folders");
            if (separatorHasModifiedChanges) modifiedSettingsList.Add("Separators");
            if (generalSettingsHasModifiedChanges) modifiedSettingsList.Add("General Settings");
            if (designSettingsHasModifiedChanges) modifiedSettingsList.Add("Design Settings");
            if (shortcutSettingsHasModifiedChanges) modifiedSettingsList.Add("Shortcut Settings");
            if (advancedSettingsHasModifiedChanges) modifiedSettingsList.Add("Advanced Settings");

            if (modifiedSettingsList.Count > 0)
            {
                message += string.Join(", ", modifiedSettingsList) + ".\n\nWould you like to save the changes?";
                bool shouldSave = EditorUtility.DisplayDialog("Data Has Been Modified!", message, "Save", "Don't Save");

                if (shouldSave)
                {
                    if (folderHasModifiedChanges) UpdateAndSaveFoldersData();
                    if (separatorHasModifiedChanges) UpdateAndSaveSeparatorsData();
                    if (generalSettingsHasModifiedChanges) UpdateAndSaveGeneralSettingsData();
                    if (designSettingsHasModifiedChanges) UpdateAndSaveDesignSettingsData();
                    if (shortcutSettingsHasModifiedChanges) UpdateAndSaveShortcutSettingsData();
                    if (advancedSettingsHasModifiedChanges) UpdateAndSaveAdvancedSettingsData();
                }
            }

            folderHasModifiedChanges = false;
            separatorHasModifiedChanges = false;
            generalSettingsHasModifiedChanges = false;
            designSettingsHasModifiedChanges = false;
            shortcutSettingsHasModifiedChanges = false;
            advancedSettingsHasModifiedChanges = false;

            HierarchyDesigner_Manager_Session.instance.currentWindow = currentWindow;
        }
    }
}
#endif