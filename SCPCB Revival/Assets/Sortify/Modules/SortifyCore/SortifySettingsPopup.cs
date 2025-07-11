#if UNITY_EDITOR && SORTIFY
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Sortify
{
    public class SortifySettingsPopup : PopupWindowContent
    {
        private const int _toolbarButtonSize = 16;
        private const int _colorButtonSize = 22;
        private const int _componentButtonSize = 32;
        private const int _topPadding = 6;
        private const int _padding = 4;
        private const int _customIconsSize = 28;

        private GameObject _targetObj;
        private Color _selectedColor;
        private SortifyObjectManager.StyleType _selectedStyle;
        private Vector2 _commentsScrollPosition;
        private Vector2 _componentsScrollPosition;
        private string _objID;
        private string _newUserName = "";
        private string _newComment = "New Comment:";
        private bool _commentsFoldout = false;
        private bool _addedNote = false;
        private bool _isRemovingComponent = false;
        private bool _selectedShowComponents;

        private static readonly Color[] _predefinedColors = new Color[]
        {
            new Color(0.15f, 0.15f, 0.15f), new Color(0.5f, 0f, 1f), new Color(0.298f, 0.710f, 1f),
            new Color(0f, 0.5f, 0.5f), new Color(0.75f, 0.25f, 0.25f), new Color(0.25f, 0.75f, 0.25f),
            new Color(0.25f, 0.25f, 0.75f), new Color(0.75f, 0.75f, 0.25f), new Color(0.5f, 0.5f, 0.8f),
            new Color(0.8f, 0.6f, 0.2f)
        };

        private static readonly string[] _customIcons = new string[]
        {
            "_Popup", "d_Asset Store", "d_CloudConnect", "d_AssemblyLock", "d_Folder Icon",
            "d_Favorite", "AvatarMask On Icon", "d_UnityEditor.InspectorWindow", "d_console.warnicon", "d_console.erroricon",
            "d_ViewToolOrbit", "d_UnityEditor.Timeline.TimelineWindow", "d_UnityEditor.SceneHierarchyWindow",
            "d_UnityEditor.GameView", "d_UnityEditor.AnimationWindow", "d_Lighting", "d_SceneViewCamera",
            "d_Profiler.Audio", "d_Preset.Context", "d_PreMatSphere", "d_BuildSettings.Web.Small",
            "d_AvatarPivot", "d_Audio Mixer", "d_AudioClip On Icon"
        };

        private static Dictionary<Color, Texture2D> _colorTextures;
        private static Dictionary<System.Type, Texture> _componentIconsCache = new Dictionary<System.Type, Texture>();

        public SortifySettingsPopup(GameObject obj, string objID)
        {
            _targetObj = obj;
            _objID = objID;
            _selectedColor = SortifyObjectManager.LoadColor(_targetObj) ?? Color.white;
            _selectedStyle = SortifyObjectManager.LoadStyleType(_targetObj);
            _selectedShowComponents = SortifyObjectManager.LoadShowComponents(_targetObj, true);

            if (_colorTextures == null)
                _colorTextures = new Dictionary<Color, Texture2D>();

            foreach (Color color in _predefinedColors)
            {
                if (!_colorTextures.ContainsKey(color) || _colorTextures[color] == null)
                    _colorTextures[color] = SortifyHelper.MakeTexture(2, 2, color);
            }
        }

        public override Vector2 GetWindowSize()
        {
            float totalHeight = 0f;
            totalHeight += 35f;

            if (SortifyUserDataManager.GetUserSetting("ShowStyleSection", true))
                totalHeight += 55f;
            if (SortifyUserDataManager.GetUserSetting("ShowColorsSection", true))
                totalHeight += 94f;
            if (SortifyUserDataManager.GetUserSetting("ShowNotesSection", true))
                if (_addedNote) totalHeight += 174f; else totalHeight += 37f;
            if (SortifyUserDataManager.GetUserSetting("ShowAddComponentSection", true))
            {
                if (_commentsFoldout) totalHeight += 122f;
                totalHeight += 117;
            }
            return new Vector2(280, totalHeight);
        }

        public override void OnGUI(Rect rect)
        {
            DrawTitleSection();
            EditorGUILayout.Space(2);
            if (SortifyUserDataManager.GetUserSetting("ShowStyleSection", true))
            {
                DrawStyleSection();
                EditorGUILayout.Space(2);
            }
            if (SortifyUserDataManager.GetUserSetting("ShowColorsSection", true))
            {
                DrawColorSection(rect.width);
                EditorGUILayout.Space(2);
            }
            if (SortifyUserDataManager.GetUserSetting("ShowNotesSection", true))
            {
                DrawNoteSection();
                EditorGUILayout.Space(2);
            }
            if (SortifyUserDataManager.GetUserSetting("ShowAddComponentSection", true))
                DrawComponentSection(rect.width);
        }

        private void DrawTitleSection()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"{_targetObj.name} Settings", EditorStyles.boldLabel, GUILayout.Height(24));
            GUILayout.FlexibleSpace();

            GUIStyle style = new GUIStyle()
            {
                fixedWidth = _toolbarButtonSize,
                fixedHeight = _toolbarButtonSize,
                margin = new RectOffset(_padding, _padding, _topPadding, _padding)
            };

            GUIContent eyeIcon = EditorGUIUtility.IconContent("d_scenevis_hidden");
            Color originalColor = GUI.color;
            GUI.color = _selectedShowComponents ? new Color(1f, 1f, 1f, 0.3f) : Color.white;
            if (GUILayout.Button(eyeIcon, style))
            {
                _selectedShowComponents = !_selectedShowComponents;
                SortifyObjectManager.SaveShowComponents(_targetObj, _selectedShowComponents);
            }
            GUI.color = originalColor;
            EditorGUILayout.EndHorizontal();
        }

        private void DrawStyleSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Select Style:", EditorStyles.boldLabel, GUILayout.Height(24));
            GUILayout.FlexibleSpace();

            GUIStyle style = new GUIStyle()
            {
                fixedWidth = _toolbarButtonSize,
                fixedHeight = _toolbarButtonSize,
                margin = new RectOffset(_padding, _padding, _topPadding, _padding)
            };

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_Refresh"), style))
            {
                _selectedStyle = SortifyObjectManager.StyleType.Default;
                SortifyObjectManager.SaveStyleType(_targetObj, _selectedStyle);
            }

            EditorGUILayout.EndHorizontal();

            var selectedStyle = (SortifyObjectManager.StyleType)EditorGUILayout.EnumPopup("Style", _selectedStyle);
            if (selectedStyle != _selectedStyle)
            {
                _selectedStyle = selectedStyle;
                SortifyObjectManager.SaveStyleType(_targetObj, _selectedStyle);
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawColorSection(float windowWidth)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Select Color:", EditorStyles.boldLabel, GUILayout.Height(24));
            GUILayout.FlexibleSpace();

            GUIStyle style = new GUIStyle()
            {
                fixedWidth = _toolbarButtonSize,
                fixedHeight = _toolbarButtonSize,
                margin = new RectOffset(_padding, _padding, _topPadding, _padding)
            };

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_Refresh"), style))
                SortifyObjectManager.ResetColor(_targetObj);

            var selectedColor = new Color();

            EditorGUILayout.EndHorizontal();
            selectedColor = EditorGUILayout.ColorField(_selectedColor);
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginVertical();
            int buttonsPerRow = Mathf.FloorToInt((windowWidth - _padding) / (_colorButtonSize + _padding));
            for (int i = 0; i < _predefinedColors.Length; i += buttonsPerRow)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                for (int j = 0; j < buttonsPerRow && (i + j) < _predefinedColors.Length; j++)
                {
                    Color color = _predefinedColors[i + j];
                    if (GUILayout.Button("", new GUIStyle(GUI.skin.button) { normal = { background = _colorTextures[color] } },
                                          GUILayout.Width(_colorButtonSize), GUILayout.Height(_colorButtonSize)))
                    {
                        selectedColor = color;
                    }
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            if(selectedColor != _selectedColor)
            {
                _selectedColor = selectedColor;
                SortifyObjectManager.SaveColor(_targetObj, _selectedColor);
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }

        private void DrawNoteSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            NoteEntry note = SortifyNoteManager.GetNoteForGameObject(_objID);
            if (note == null)
            {
                if (GUILayout.Button("Add Note", GUILayout.Height(24)))
                    SortifyNoteManager.AddNoteToGameObject(_objID, "New Note Content");

                _addedNote = false;
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Note Content:", EditorStyles.boldLabel, GUILayout.Height(24));
                GUILayout.FlexibleSpace();

                GUIStyle style = new GUIStyle()
                {
                    fixedWidth = _toolbarButtonSize,
                    fixedHeight = _toolbarButtonSize,
                    margin = new RectOffset(_padding, _padding, _topPadding, _padding)
                };

                if (GUILayout.Button(EditorGUIUtility.IconContent("d_Package Manager"), style))
                    SortifyNoteManager.RemoveNoteForGameObject(_objID);

                if (GUILayout.Button(EditorGUIUtility.IconContent("d_FilterSelectedOnly"), style))
                    SortifyNoteManager.UpdateNoteForGameObject(_objID, note.noteContent);

                EditorGUILayout.EndHorizontal();
                note.noteContent = EditorGUILayout.TextArea(note.noteContent, GUILayout.Height(60));
                EditorGUILayout.Space(5);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button((_commentsFoldout ? "▼ " : "▶ ") + "Comments:", EditorStyles.boldLabel, GUILayout.Height(20)))
                    _commentsFoldout = !_commentsFoldout;

                EditorGUILayout.EndHorizontal();

                if (_commentsFoldout)
                {
                    _commentsScrollPosition = EditorGUILayout.BeginScrollView(_commentsScrollPosition, GUILayout.Height(120));
                    foreach (var comment in note.comments)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.LabelField($"{comment.userName} - {comment.timestamp}", EditorStyles.miniLabel);
                        EditorGUILayout.LabelField(comment.text, EditorStyles.wordWrappedLabel);
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndScrollView();
                }

                string userName = SortifyUserDataManager.GetUsername();
                if (string.IsNullOrEmpty(userName))
                {
                    EditorGUILayout.LabelField("Enter User Name:", EditorStyles.boldLabel);
                    _newUserName = EditorGUILayout.TextField(_newUserName);
                    if (GUILayout.Button("Save User Name", GUILayout.Height(24)))
                        SortifyUserDataManager.SaveUsername(_newUserName);
                }
                else
                {
                    _newComment = EditorGUILayout.TextField(_newComment);
                    if (GUILayout.Button("Add Comment", GUILayout.Height(24)))
                    {
                        Comment comment = new Comment(userName, _newComment);
                        _newComment = "New Comment:";
                        SortifyNoteManager.AddCommentToNote(_objID, comment);
                    }
                }

                _addedNote = true;
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawComponentSection(float windowWidth)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Add Component:", EditorStyles.boldLabel, GUILayout.Height(24));
            GUILayout.FlexibleSpace();

            GUIStyle style = new GUIStyle()
            {
                fixedWidth = _toolbarButtonSize,
                fixedHeight = _toolbarButtonSize,
                margin = new RectOffset(_padding, _padding, _topPadding, _padding)
            };

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_Package Manager"), style))
                _isRemovingComponent = !_isRemovingComponent;

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_CreateAddNew"), style))
                PopupWindow.Show(new Rect(280, -60, 300, 400), new SortifyAddComponent(component =>
                {
                    SortifyComponentManager.AddComponent(component.FullName);
                }));

            EditorGUILayout.EndHorizontal();

            var defaultComponents = SortifyComponentManager.GetDefaultComponents();
            var userComponents = SortifyComponentManager.LoadUserComponents();
            var combinedComponents = new List<System.Type>(defaultComponents);
            combinedComponents.AddRange(userComponents);

            EditorGUILayout.BeginVertical();
            _componentsScrollPosition = EditorGUILayout.BeginScrollView(_componentsScrollPosition, GUILayout.Height(80));
            if (_isRemovingComponent)
                EditorGUILayout.LabelField("Removing...", EditorStyles.boldLabel);

            int buttonsPerRow = Mathf.FloorToInt((windowWidth - _padding) / (_componentButtonSize + _padding));
            for (int i = 0; i < combinedComponents.Count; i += buttonsPerRow)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                for (int j = 0; j < buttonsPerRow && (i + j) < combinedComponents.Count; j++)
                {
                    var component = combinedComponents[i + j];
                    if (component != null)
                    {
                        Texture icon;
                        if (!_componentIconsCache.TryGetValue(component, out icon))
                        {
                            icon = typeof(MonoBehaviour).IsAssignableFrom(component)
                                ? EditorGUIUtility.IconContent("cs Script Icon").image
                                : AssetPreview.GetMiniTypeThumbnail(component);
                            _componentIconsCache[component] = icon;
                        }

                        GUIStyle componentButtonStyle = new GUIStyle(GUI.skin.button)
                        {
                            fixedWidth = _componentButtonSize,
                            fixedHeight = _componentButtonSize,
                            margin = new RectOffset(_padding, _padding, _topPadding, _padding)
                        };

                        bool isUserComponent = userComponents.Contains(component);
                        if (isUserComponent)
                        {
                            Rect buttonRect = GUILayoutUtility.GetRect(new GUIContent(icon, component.Name), componentButtonStyle);
                            if (GUI.Button(buttonRect, new GUIContent(icon, component.Name)))
                            {
                                if (_isRemovingComponent)
                                {
                                    SortifyComponentManager.RemoveComponent(component.FullName);
                                }
                                else
                                {
                                    AddComponentToObject(component);
                                }
                            }

                            if (_isRemovingComponent)
                            {
                                GUI.DrawTexture(new Rect(buttonRect.x, buttonRect.y, 16, 16), EditorGUIUtility.IconContent("d_winbtn_mac_close_h@2x").image);
                            }
                            else
                            {
                                GUI.DrawTexture(new Rect(buttonRect.x, buttonRect.y, 16, 16), EditorGUIUtility.IconContent("AvatarMask On Icon").image);
                            }
                        }
                        else
                        {
                            if (GUILayout.Button(new GUIContent(icon, component.Name), componentButtonStyle))
                                AddComponentToObject(component);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Component is null and cannot be displayed.");
                    }
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }

        private void AddComponentToObject(System.Type componentType)
        {
            Undo.AddComponent(_targetObj, componentType);
        }
    }
}
#endif
