#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Sortify
{
    public class SortifyDashboardWindow : EditorWindow
    {
#pragma warning disable CS0414
        private const string LOGO_NAME = "SortifyLogo";
        private const string VERSION = "Free";
        private const string CONSENT_KEY = "Sortify_ConsentGiven";
        private const string DONT_SHOW_KEY = "Sortify_DontShowOnStart";
        private const string SAVE_IN_PROJECT_KEY = "Sortify_SaveInProject";

        private SortifyChangeLog _changeLog;
        private string[] _tabs = { "Welcome", "Features", "Modules", "Settings", "Support" };
        private int _currentTab = 0;
        private bool _isConsentGiven;
        private bool _isSortifyEnabled;
        private bool _isSortifyAttributesEnabled;
        private bool _isSortifyHighlightEnabled;
        private bool _showTerms = false;
        private Vector2 _scrollPosition;
        private List<bool> _showDetails = new List<bool>();
#pragma warning restore CS0414

        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            EditorApplication.update += ShowDashboardOnStartup;
        }

        private static void ShowDashboardOnStartup()
        {
            EditorApplication.update -= ShowDashboardOnStartup;
            if (!EditorPrefs.GetBool(DONT_SHOW_KEY, false))
                ShowWindow();
        }

        [MenuItem("Window/Sortify/Dashboard")]
        public static void ShowWindow()
        {
            var window = GetWindow<SortifyDashboardWindow>();
            GUIContent titleContent = new GUIContent("Sortify", EditorGUIUtility.IconContent("d_SettingsIcon").image);
            window.titleContent = titleContent;
            window.minSize = new Vector2(600, 400);
            window.CheckConsentStatus();
            window.InitializeModuleStatus();
            window.LoadChangeLog();
        }

        private void CheckConsentStatus()
        {
            _isConsentGiven = EditorPrefs.GetBool(CONSENT_KEY, false);
        }

        private void InitializeModuleStatus()
        {
            string defines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup));
            _isSortifyEnabled = defines.Contains("SORTIFY");
            _isSortifyAttributesEnabled = defines.Contains("SORTIFY_ATTRIBUTES");
            _isSortifyHighlightEnabled = defines.Contains("SORTIFY_HIGHLIGHT");
        }

        private void LoadChangeLog()
        {
            string[] guids = AssetDatabase.FindAssets("t:SortifyChangeLog SortifyChangeLog");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _changeLog = AssetDatabase.LoadAssetAtPath<SortifyChangeLog>(path);
                if (_changeLog != null)
                    InitializeShowDetailsList();
            }

            if (_changeLog == null)
                Debug.LogWarning("[Sortify] ChangeLog file 'SortifyChangeLog' not found.");
        }

        private void InitializeShowDetailsList()
        {
            _showDetails.Clear();
            if (_changeLog != null)
            {
                for (int i = 0; i < _changeLog.ChangelogEntries.Length; i++)
                    _showDetails.Add(false);
            }
        }

        private void OnGUI()
        {
            if (!_isConsentGiven)
            {
                DrawConsentScreen();
            }
            else
            {
                if (!_isSortifyEnabled)
                    DrawSortifyCoreMissing();
#if SORTIFY
                DrawDashboard();
#endif
            }
        }

        private void DrawConsentScreen()
        {
            try
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.Space(10);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Data Storage Notice and Terms", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                EditorGUILayout.Space(10);

                DrawTermsContent();

                if (GUILayout.Button("Install", GUILayout.Height(40)))
                {
                    _isConsentGiven = true;
                    EditorPrefs.SetBool(CONSENT_KEY, true);
                    _isSortifyEnabled = true;
                    UpdateDefineSymbols();
                    GUIUtility.ExitGUI();
                }

                EditorGUILayout.EndVertical();
            }
            catch (ExitGUIException)
            {
                throw;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Sortify] Error in DrawConsentScreen: {e}");
            }
        }

        private void DrawTermsContent()
        {
            EditorGUILayout.BeginVertical("box");
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            GUILayout.Label(
                "Sortify stores certain user preferences locally on your device to provide a personalized and efficient experience." +
                "\n\n" +
                "By installing this tool, you agree to the following terms regarding data storage:",
                EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(10);

            GUILayout.Label("Why we save data locally:", EditorStyles.boldLabel);
            GUILayout.Label(
                "Sortify was designed to work seamlessly in team environments. Instead of storing settings in the project files, " +
                "we use a local storage approach to prevent personal preferences, such as colors, favorites, custom components, or user names, from " +
                "being accidentally overwritten during team merges. This helps avoid conflicts and ensures that each team member's setup remains unique and intact.",
                EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(10);

            GUILayout.Label("What data we store:", EditorStyles.boldLabel);
            GUILayout.Label(
                "The data saved locally does not include any sensitive or private information. " +
                "We only store user-specific settings, including color themes, favorite objects, custom component lists, and user names, " +
                "to personalize your Sortify experience. These files are stored as simple .json files, so you can view and modify them directly if needed.",
                EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(10);

            GUILayout.Label("Privacy and data security:", EditorStyles.boldLabel);
            GUILayout.Label(
                "All stored data is strictly limited to local preferences that enhance your interaction with the tool. " +
                "No data is transmitted externally or used beyond the customization of Sortify. You can delete or reset these files anytime " +
                "by navigating to the specified storage location on your device.",
                EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(20);

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void UpdateDefineSymbols()
        {
            var targetGroups = System.Enum.GetValues(typeof(BuildTargetGroup))
                .Cast<BuildTargetGroup>()
                .Where(group =>
                    group != BuildTargetGroup.Unknown &&
                    !IsObsoletePlatform(group));

            foreach (var buildTargetGroup in targetGroups)
            {
                string defines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup));
                HashSet<string> defineSymbols = new HashSet<string>(defines.Split(';'));

                if (_isSortifyEnabled)
                {
                    defineSymbols.Add("SORTIFY");
                }
                else
                {
                    defineSymbols.Remove("SORTIFY");
                }

                if (_isSortifyAttributesEnabled)
                {
                    defineSymbols.Add("SORTIFY_ATTRIBUTES");
                }
                else
                {
                    defineSymbols.Remove("SORTIFY_ATTRIBUTES");
                }

                if (_isSortifyHighlightEnabled)
                {
                    defineSymbols.Add("SORTIFY_HIGHLIGHT");
                }
                else
                {
                    defineSymbols.Remove("SORTIFY_HIGHLIGHT");
                }

                string updatedDefines = string.Join(";", defineSymbols.Where(s => !string.IsNullOrEmpty(s)).ToArray());
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup), updatedDefines);
            }
        }

        private bool IsObsoletePlatform(BuildTargetGroup group)
        {
            var field = typeof(BuildTargetGroup).GetField(group.ToString());
            return field != null && System.Attribute.IsDefined(field, typeof(System.ObsoleteAttribute));
        }

        private void DrawSortifyCoreMissing()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Sortify Core Module Missing", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);

            EditorGUILayout.BeginVertical("box");
            GUILayout.Label(
                "The Core Sortify module is required for the asset to function properly. It seems the module was not activated, " +
                "even though you previously agreed to install the asset. Without this module, Sortify will not work as intended.",
                EditorStyles.wordWrappedLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();

            if (GUILayout.Button("Install Core Module", GUILayout.Height(40)))
            {
                _isSortifyEnabled = true;
                UpdateDefineSymbols();
                Repaint();
            }
            EditorGUILayout.EndVertical();
        }

#if SORTIFY
        private void DrawDashboard()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(position.width * 0.33f), GUILayout.ExpandHeight(true));
            DrawMenuSection();
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MaxWidth(position.width * 0.67f), GUILayout.ExpandHeight(true));
            DrawTabContent();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MaxWidth(position.width), GUILayout.Height(30));
            DrawFooter();
            EditorGUILayout.EndVertical();
        }

        private void DrawMenuSection()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(GetSortifyLogo(), GUILayout.Width(150), GUILayout.Height(150));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUIStyle customButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fixedHeight = 30
            };

            for (int i = 0; i < _tabs.Length; i++)
            {
                GUIContent content = new GUIContent(_tabs[i], GetIconForTab(i));
                if (GUILayout.Button(content, customButtonStyle))
                    _currentTab = i;

                GUILayout.Space(5);
            }

            GUILayout.FlexibleSpace();
            GUILayout.Label($"Version: {VERSION}", EditorStyles.boldLabel);
            GUILayout.Space(1);
        }

        private Texture2D GetSortifyLogo()
        {
            string[] guids = AssetDatabase.FindAssets(LOGO_NAME);
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            return null;
        }
        
        private Texture2D GetIconForTab(int tabIndex)
        {
            switch (tabIndex)
            {
                case 0:
                    return EditorGUIUtility.IconContent("d_UnityEditor.HierarchyWindow").image as Texture2D;
                case 1:
                    return EditorGUIUtility.IconContent("d_Favorite").image as Texture2D;
                case 2:
                    return EditorGUIUtility.IconContent("d_Package Manager").image as Texture2D;
                case 3:
                    return EditorGUIUtility.IconContent("d_SettingsIcon").image as Texture2D;
                case 4:
                    return EditorGUIUtility.IconContent("d_UnityEditor.InspectorWindow").image as Texture2D;
                default:
                    return null;
            }
        }

        private void DrawTabContent()
        {
            switch (_currentTab)
            {
                case 0: DrawWelcomeContent(); break;
                case 1: DrawFeaturesContent(); break;
                case 2: DrawModulesContent(); break;
                case 3: DrawSettingsContent(); break;
                case 4: DrawSupportContent(); break;
            }
        }

        private void DrawWelcomeContent()
        {
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };

            GUILayout.Label("Welcome to Sortify!", titleStyle);
            GUILayout.Space(20);

            GUILayout.Label("Introduction", EditorStyles.boldLabel);
            GUILayout.Label(
                "Welcome to Sortify! This advanced toolset optimizes your Unity workflow by enhancing organization, customization, and efficiency in managing project elements. Sortify provides features like color-coding, custom icons, quick access to components, and custom attributes, all designed to keep your workspace organized and your projects running smoothly.\n\n" +
                "Whether you’re working solo or in a team, Sortify simplifies complex scenes and helps you stay productive with intuitive tools and modular options tailored to your workflow.",
                EditorStyles.wordWrappedLabel);
            GUILayout.Space(20);

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("What's New", EditorStyles.boldLabel);
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandHeight(true));

            if (_changeLog == null || _changeLog.ChangelogEntries == null || _changeLog.ChangelogEntries.Length == 0)
            {
                GUILayout.Label("No changelog entries available.", EditorStyles.wordWrappedLabel);
                GUILayout.EndScrollView();
                GUILayout.EndVertical();
                return;
            }

            for (int i = _changeLog.ChangelogEntries.Length - 1; i >= 0; i--)
            {
                if (i >= _showDetails.Count) continue;

                GUILayout.BeginVertical("box");
                _showDetails[i] = EditorGUILayout.Foldout(_showDetails[i], $"{_changeLog.ChangelogEntries[i].version} - {_changeLog.ChangelogEntries[i].releaseDate.ToShortDateString()}");

                if (_showDetails[i])
                    GUILayout.Label(_changeLog.ChangelogEntries[i].details, EditorStyles.wordWrappedLabel);
                
                GUILayout.EndVertical();
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawFeaturesContent()
        {
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };

            GUILayout.Label("Features", titleStyle);
            GUILayout.Space(20);

            GUIStyle headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold
            };

            GUIStyle customButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fixedHeight = 30
            };

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandHeight(true));

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Free Features", headerStyle);
            GUILayout.Space(8);
            GUILayout.BeginVertical("box");
            GUILayout.Label("• Color customization for easy visual identification of objects in the hierarchy", EditorStyles.wordWrappedLabel);
            GUILayout.Label("• Personal notes for organizing project details directly in the hierarchy", EditorStyles.wordWrappedLabel);
            GUILayout.Label("• Quick access to frequently used components", EditorStyles.wordWrappedLabel);
            GUILayout.Label("• Basic custom attributes for project-specific information", EditorStyles.wordWrappedLabel);
            GUILayout.EndVertical();
            EditorGUILayout.EndVertical();

            GUILayout.Space(10);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Pro Features", headerStyle);
            GUILayout.FlexibleSpace();
#pragma warning disable CS0162
            if (VERSION == "Free")
            {
                if (GUILayout.Button("Upgrade to Pro", customButtonStyle))
                    Application.OpenURL("https://assetstore.unity.com/packages/tools/utilities/sortify-pro-301291");
            }
            else
            {
                GUILayout.Space(8);
            }
#pragma warning restore CS0162
            EditorGUILayout.EndHorizontal();
            GUILayout.BeginVertical("box");
            GUILayout.Label("• Includes all Free Features", EditorStyles.wordWrappedLabel);
            GUILayout.Label("• Add objects to a Favorites list for quick access", EditorStyles.wordWrappedLabel);
            GUILayout.Label("• Open a dedicated Inspector window for selected objects directly from the hierarchy", EditorStyles.wordWrappedLabel);
            GUILayout.Label("• Access a full range of custom attributes for advanced project management", EditorStyles.wordWrappedLabel);
            GUILayout.Label("• Customize object icons within the hierarchy for clearer organization", EditorStyles.wordWrappedLabel);
            GUILayout.Label("• Free access to all future feature updates", EditorStyles.wordWrappedLabel);
            GUILayout.EndVertical();
            EditorGUILayout.EndVertical();
            
            GUILayout.EndScrollView();
        }

        private void DrawModulesContent()
        {
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };

            GUILayout.Label("Modules", titleStyle);
            GUILayout.Space(20);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Sortify", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Always enabled to activate core hierarchy features.", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
            EditorGUILayout.LabelField("Enabled", EditorStyles.boldLabel, GUILayout.Width(80));
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);
            DrawToggle("Enable Custom Attributes", ref _isSortifyAttributesEnabled,
                "Enables custom attributes in the Inspector, allowing you to display additional metadata for serialized fields.");

            GUILayout.Space(5);
            DrawToggle("Enable Sortify Highlight", ref _isSortifyHighlightEnabled,
                "Highlights objects in the scene when hovered over in the Hierarchy for improved visibility and interaction.");

            EditorGUILayout.EndVertical();
            GUILayout.Space(10);

            if (GUILayout.Button("Apply Changes", GUILayout.Height(30)))
                UpdateDefineSymbols();
        }

        private void DrawSettingsContent()
        {
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };

            GUILayout.Label("Settings", titleStyle);
            GUILayout.Space(20);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            bool showComponentIcons = SortifyUserDataManager.GetUserSetting("ShowComponentIcons", true);
            DrawToggle("Show Component Icons", ref showComponentIcons,
                "Enabling this setting will display component icons next to each object in the hierarchy.");

            if (showComponentIcons != SortifyUserDataManager.GetUserSetting("ShowComponentIcons", true))
                SortifyUserDataManager.SaveUserSetting("ShowComponentIcons", showComponentIcons);

            GUILayout.Space(5);
            bool showStyleSection = SortifyUserDataManager.GetUserSetting("ShowStyleSection", true);
            DrawToggle("Show Style Section", ref showStyleSection,
                "Enabling this setting will display the Style section in the hierarchy, allowing you to assign custom styles to objects for better visual organization.");

            if (showStyleSection != SortifyUserDataManager.GetUserSetting("ShowStyleSection", true))
                SortifyUserDataManager.SaveUserSetting("ShowStyleSection", showStyleSection);

            GUILayout.Space(5);
            bool showColorsSection = SortifyUserDataManager.GetUserSetting("ShowColorsSection", true);
            DrawToggle("Show Colors Section", ref showColorsSection,
                "Enabling this setting will display the Colors section in the hierarchy, allowing you to assign custom colors to objects.");

            if (showColorsSection != SortifyUserDataManager.GetUserSetting("ShowColorsSection", true))
                SortifyUserDataManager.SaveUserSetting("ShowColorsSection", showColorsSection);

            GUILayout.Space(5);
            bool showNotesSection = SortifyUserDataManager.GetUserSetting("ShowNotesSection", true);
            DrawToggle("Show Notes Section", ref showNotesSection,
                "Enabling this setting will display the Notes section in the hierarchy, allowing you to add personal notes to objects.");

            if (showNotesSection != SortifyUserDataManager.GetUserSetting("ShowNotesSection", true))
                SortifyUserDataManager.SaveUserSetting("ShowNotesSection", showNotesSection);

            GUILayout.Space(5);
            bool showAddComponentSection = SortifyUserDataManager.GetUserSetting("ShowAddComponentSection", true);
            DrawToggle("Show Add Component Section", ref showAddComponentSection,
                "Enabling this setting will display the Add Component section in the hierarchy, allowing you to quickly add components to objects.");

            if (showAddComponentSection != SortifyUserDataManager.GetUserSetting("ShowAddComponentSection", true))
                SortifyUserDataManager.SaveUserSetting("ShowAddComponentSection", showAddComponentSection);

            GUILayout.Space(5);
            bool showDefaultComponents = SortifyUserDataManager.GetUserSetting("ShowDefaultComponents", true);
            DrawToggle("Show Default Components", ref showDefaultComponents,
                "Enabling this setting will display the default component icons in the Add Component section.");

            if (showDefaultComponents != SortifyUserDataManager.GetUserSetting("ShowDefaultComponents", true))
                SortifyUserDataManager.SaveUserSetting("ShowDefaultComponents", showDefaultComponents);

            GUILayout.Space(5);
            bool saveInProject = EditorPrefs.GetBool(SAVE_IN_PROJECT_KEY, false);
            DrawToggle("Save in Project", ref saveInProject,
                "If enabled, Sortify save data will be stored in the project folder. If disabled, data is stored locally.");

            if (saveInProject != EditorPrefs.GetBool(SAVE_IN_PROJECT_KEY, false))
            {
                EditorPrefs.SetBool(SAVE_IN_PROJECT_KEY, saveInProject);
                SortifyFileManager.MigrateData(saveInProject);
                SortifyComponentManager.LoadComponentListIfNeeded(true);
                SortifyObjectManager.LoadDataIfNeeded(true);
                SortifyUserDataManager.LoadUserDataIfNeeded(true);
                SortifyInitializer.Refresh();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawToggle(string label, ref bool isEnabled, string description)
        {
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(description, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();

            if (GUILayout.Button(isEnabled ? "Enabled" : "Disabled", GUILayout.Width(80)))
                isEnabled = !isEnabled;

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSupportContent()
        {
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };

            GUILayout.Label("Support", titleStyle);
            GUILayout.Space(20);

            EditorGUILayout.LabelField("Need assistance or have feedback? Feel free to reach out! Join the community, read the documentation, or contact me directly " +
                "— I'm here to ensure you have the best experience with Sortify.", EditorStyles.wordWrappedLabel);
            GUILayout.Space(10);

            GUIStyle buttonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fixedHeight = 30,
                fontStyle = FontStyle.Bold,
                margin = new RectOffset(5, 5, 5, 5),
            };

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Join my Discord", buttonStyle))
                Application.OpenURL("https://discord.gg/AuCXbbqyHu");

            if (GUILayout.Button("Follow me on X", buttonStyle))
                Application.OpenURL("https://x.com/paw_mularczyk");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Watch on YouTube", buttonStyle))
                Application.OpenURL("https://www.youtube.com/@erzonn");

            if (GUILayout.Button("Contact Support via Email", buttonStyle))
                Application.OpenURL("https://erzonnn.github.io/pawel-mularczyk/");

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);

            if (GUILayout.Button("Read Documentation", buttonStyle))
                Application.OpenURL("https://docs.google.com/document/d/1TOQ_n5r09TzylZfCMhUBAC6kIPDyt-j9wYeLwS5i96I/edit?usp=sharing");

            EditorGUILayout.EndVertical();
            GUILayout.Space(10);

            if (GUILayout.Button(_showTerms ? "Hide Terms" : "Show Terms", buttonStyle))
                _showTerms = !_showTerms;

            if (_showTerms)
            {
                GUILayout.Space(5);
                DrawTermsContent();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawFooter()
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();

            bool dontShowOnStart = EditorPrefs.GetBool(DONT_SHOW_KEY, false);
            bool newDontShowOnStart = EditorGUILayout.Toggle("Don't show on start", dontShowOnStart);
            if (newDontShowOnStart != dontShowOnStart)
                EditorPrefs.SetBool(DONT_SHOW_KEY, newDontShowOnStart);

            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("2024 Sortify. All Rights Reserved.", EditorStyles.centeredGreyMiniLabel);
            GUILayout.FlexibleSpace();

            Texture2D favoriteIcon = EditorGUIUtility.IconContent("d_Favorite").image as Texture2D;
            if (favoriteIcon != null)
            {
                if (GUILayout.Button(new GUIContent(favoriteIcon, "Leave a Review"), EditorStyles.label, GUILayout.Width(80)))
                    Application.OpenURL("https://assetstore.unity.com/packages/tools/utilities/sortify-301537");

                Rect buttonRect = GUILayoutUtility.GetLastRect();
                float iconWidth = 16f;
                for (int i = 0; i < 5; i++)
                    GUI.DrawTexture(new Rect(buttonRect.x + i * iconWidth, buttonRect.y, iconWidth, iconWidth), favoriteIcon);
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }
#endif
    }
}
#endif
