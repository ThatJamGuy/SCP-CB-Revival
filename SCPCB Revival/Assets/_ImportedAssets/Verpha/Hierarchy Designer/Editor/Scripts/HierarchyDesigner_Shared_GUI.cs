#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Verpha.HierarchyDesigner
{
    internal static class HierarchyDesigner_Shared_GUI
    {
        #region Properties
        #region Labels
        private static readonly Lazy<GUIStyle> _titleLabelStyle = new(() => new()
        {
            alignment = TextAnchor.MiddleCenter,
            wordWrap = false,
            stretchHeight = true,
            stretchWidth = true,
            imagePosition = ImagePosition.ImageOnly,
            padding = new(-70, -70, -70, -70),
        });
        public static GUIStyle TitleLabelStyle => _titleLabelStyle.Value;

        private static readonly Lazy<GUIStyle> _footerLabelStyle = new(() => new()
        {
            font = HierarchyDesigner_Shared_Resources.Fonts.Bold,
            fontSize = 11,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = false,
            normal = { textColor = HierarchyDesigner_Shared_Color.GetPrimaryFontColorFaded() }
        });
        public static GUIStyle FooterLabelStyle => _footerLabelStyle.Value;

        private static readonly Lazy<GUIStyle> _versionLabelHeaderStyle = new(() => new()
        {
            font = HierarchyDesigner_Shared_Resources.Fonts.Bold,
            fontSize = 11,
            alignment = TextAnchor.LowerRight,
            wordWrap = false,
            normal = { textColor = HierarchyDesigner_Shared_Color.GetPrimaryFontColorFaded() }
        });
        public static GUIStyle VersionLabelHeaderStyle => _versionLabelHeaderStyle.Value;

        private static readonly Lazy<GUIStyle> _headerLeftLabelStyle = new(() => new()
        {
            font = HierarchyDesigner_Shared_Resources.Fonts.Bold,
            fontSize = 20,
            alignment = TextAnchor.MiddleLeft,
            wordWrap = false,
            richText = true,
            normal = { textColor = HierarchyDesigner_Shared_Color.GetPrimaryFontColor(), }
        });
        public static GUIStyle HeaderLabelLeftStyle => _headerLeftLabelStyle.Value;

        private static readonly Lazy<GUIStyle> _divisorLabelStyle = new(() => new()
        {
            font = HierarchyDesigner_Shared_Resources.Fonts.Regular,
            fontSize = 14,
            alignment = TextAnchor.UpperCenter,
            wordWrap = false,
            richText = true,
            padding = new(0, 0, 3, 0),
            normal = { textColor = HierarchyDesigner_Shared_Color.GetPrimaryFontColorFaded(), }
        });
        public static GUIStyle DivisorLabelStyle => _divisorLabelStyle.Value;

        private static readonly Lazy<GUIStyle> _tabLabelStyle = new(() => new()
        {
            font = HierarchyDesigner_Shared_Resources.Fonts.Bold,
            fontSize = 15,
            alignment = TextAnchor.MiddleLeft,
            wordWrap = false,
            richText = true,
            normal = { textColor = HierarchyDesigner_Shared_Color.GetPrimaryFontColorFaded(), }
        });
        public static GUIStyle TabLabelStyle => _tabLabelStyle.Value;

        private static readonly Lazy<GUIStyle> _assignedLabelStyle = new(() => new()
        {
            font = HierarchyDesigner_Shared_Resources.Fonts.Regular,
            fontSize = 14,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = false,
            normal = { textColor = HierarchyDesigner_Shared_Color.GetPrimaryFontColor() }
        });
        public static GUIStyle AssignedLabelStyle => _assignedLabelStyle.Value;

        private static readonly Lazy<GUIStyle> _unassignedLabelStyle = new(() => new()
        {
            font = HierarchyDesigner_Shared_Resources.Fonts.Regular,
            fontSize = 14,
            fontStyle = FontStyle.Italic,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = false,
            normal = { textColor = HierarchyDesigner_Shared_Color.GetPrimaryFontColorFaded() }
        });
        public static GUIStyle UnassignedLabelStyle => _unassignedLabelStyle.Value;

        private static readonly Lazy<GUIStyle> _fieldsCategoryLabelStyle = new(() => new()
        {
            font = HierarchyDesigner_Shared_Resources.Fonts.Bold,
            fontSize = 16,
            alignment = TextAnchor.MiddleLeft,
            wordWrap = false,
            padding = new(0, 5, 0, 5),
            normal = { textColor = HierarchyDesigner_Shared_Color.GetSecondaryFontColor() }
        });
        public static GUIStyle FieldsCategoryLabelStyle => _fieldsCategoryLabelStyle.Value;

        private static readonly Lazy<GUIStyle> _fieldsCategoryCenterLabelStyle = new(() => new()
        {
            font = HierarchyDesigner_Shared_Resources.Fonts.Bold,
            fontSize = 18,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = false,
            padding = new(0, 5, 0, 5),
            normal = { textColor = HierarchyDesigner_Shared_Color.GetSecondaryFontColor() }
        });
        public static GUIStyle FieldsCategoryCenterLabelStyle => _fieldsCategoryCenterLabelStyle.Value;

        private static readonly Lazy<GUIStyle> _boldLabelStyle = new(() => new()
        {
            font = HierarchyDesigner_Shared_Resources.Fonts.Bold,
            fontSize = 16,
            alignment = TextAnchor.UpperLeft,
            wordWrap = false,
            normal = { textColor = HierarchyDesigner_Shared_Color.GetPrimaryFontColor() }
        });
        public static GUIStyle BoldLabelStyle => _boldLabelStyle.Value;

        private static readonly Lazy<GUIStyle> _miniBoldLabelStyle = new(() => new()
        {
            font = HierarchyDesigner_Shared_Resources.Fonts.Bold,
            fontSize = 15,
            wordWrap = false,
            normal = { textColor = HierarchyDesigner_Shared_Color.GetTertiaryFontColor() }
        });
        public static GUIStyle MiniBoldLabelStyle => _miniBoldLabelStyle.Value;

        private static readonly Lazy<GUIStyle> _miniBoldLabelCenterStyle = new(() => new()
        {
            font = HierarchyDesigner_Shared_Resources.Fonts.Bold,
            fontSize = 16,
            wordWrap = false,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = HierarchyDesigner_Shared_Color.GetTertiaryFontColor() }
        });
        public static GUIStyle MiniBoldLabelCenterStyle => _miniBoldLabelCenterStyle.Value;

        private static readonly Lazy<GUIStyle> _regularLabelStyle = new(() => new()
        {
            font = HierarchyDesigner_Shared_Resources.Fonts.Regular,
            alignment = TextAnchor.MiddleLeft,
            fontSize = 14,
            wordWrap = true,
            normal = { textColor = HierarchyDesigner_Shared_Color.GetPrimaryFontColor() }
        });
        public static GUIStyle RegularLabelStyle => _regularLabelStyle.Value;

        private static readonly Lazy<GUIStyle> _regularLabelCenterStyle = new(() => new()
        {
            font = HierarchyDesigner_Shared_Resources.Fonts.Regular,
            alignment = TextAnchor.MiddleCenter,
            fontSize = 14,
            wordWrap = true,
            normal = { textColor = HierarchyDesigner_Shared_Color.GetPrimaryFontColor() }
        });
        public static GUIStyle RegularLabelCenterStyle => _regularLabelCenterStyle.Value;

        private static readonly Lazy<GUIStyle> _layoutLabelStyle = new(() => new()
        {
            font = HierarchyDesigner_Shared_Resources.Fonts.Regular,
            fontSize = 14,
            wordWrap = false,
            alignment = TextAnchor.MiddleLeft,
            padding = new(1, 0, 1, 0),
            normal = { textColor = HierarchyDesigner_Shared_Color.GetPrimaryFontColor() }
        });
        public static GUIStyle LayoutLabelStyle => _layoutLabelStyle.Value;

        private static readonly Lazy<GUIStyle> _inspectorFolderActiveLabelStyle = new(() => new()
        {
            font = HierarchyDesigner_Shared_Resources.Fonts.Regular,
            fontSize = 13,
            alignment = TextAnchor.MiddleLeft,
            wordWrap = false,
            normal = { textColor = HierarchyDesigner_Shared_Color.GetPrimaryFontColor() }
        });
        public static GUIStyle InspectorFolderActiveLabelStyle => _inspectorFolderActiveLabelStyle.Value;

        private static readonly Lazy<GUIStyle> _inspectorFolderInactiveLabelStyle = new(() => new()
        {
            font = HierarchyDesigner_Shared_Resources.Fonts.Regular,
            fontSize = 13,
            fontStyle = FontStyle.Italic,
            alignment = TextAnchor.MiddleLeft,
            wordWrap = false,
            normal = { textColor = HierarchyDesigner_Shared_Color.GetPrimaryFontColorFaded() }
        });
        public static GUIStyle InspectorFolderInactiveLabelStyle => _inspectorFolderInactiveLabelStyle.Value;
        #endregion

        #region Panels
        private static readonly Lazy<GUIStyle> _primaryPanelStyle = new(() => new()
        {
            stretchWidth = true,
            stretchHeight = true,
            margin = new(2, 2, 2, 2),
            normal = { background = HierarchyDesigner_Shared_Texture.CreateGradientTexture(16, 1024, new Color[] { HierarchyDesigner_Shared_Color.GetPrimaryPanelColorBottom(), HierarchyDesigner_Shared_Color.GetPrimaryPanelColorMiddle(), HierarchyDesigner_Shared_Color.GetPrimaryPanelColorTop() }) }
        });
        public static GUIStyle PrimaryPanelStyle => _primaryPanelStyle.Value;

        private static readonly Lazy<GUIStyle> _secondaryPanelStyle = new(() => new()
        {
            padding = new(10, 10, 10, 10),
            margin = new(5, 5, 5, 5),
            normal = { background = HierarchyDesigner_Shared_Texture.CreateGradientTexture(16, 256, new Color[] { HierarchyDesigner_Shared_Color.GetSecondaryPanelColorBottom(), HierarchyDesigner_Shared_Color.GetSecondaryPanelColorTop() }) }
        });
        public static GUIStyle SecondaryPanelStyle => _secondaryPanelStyle.Value;

        private static readonly Lazy<GUIStyle> _tertiaryPanelStyle = new(() => new()
        {
            stretchWidth = true,
            stretchHeight = true,
            normal = { background = HierarchyDesigner_Shared_Texture.CreateGradientTexture(4, 16, new Color[] { HierarchyDesigner_Shared_Color.GetSecondaryPanelColorBottom(), HierarchyDesigner_Shared_Color.GetSecondaryPanelColorTop() }) }
        });
        public static GUIStyle TertiaryPanelStyle => _tertiaryPanelStyle.Value;

        private static readonly Lazy<GUIStyle> _headerPanelStyle = new(() => new()
        {
            fixedHeight = 60,
            stretchWidth = true,
            padding = new(5, 5, 5, 0),
            margin = new(5, 5, 5, 0),
            normal = { background = HierarchyDesigner_Shared_Texture.CreateTexture(2, 2, HierarchyDesigner_Shared_Color.GetSecondaryPanelColorBottom()) }
        });
        public static GUIStyle HeaderPanelStyle => _headerPanelStyle.Value;

        private static readonly Lazy<GUIStyle> _menuPopupPanelStyle = new(() => new()
        {
            font = HierarchyDesigner_Shared_Resources.Fonts.Regular,
            alignment = TextAnchor.MiddleLeft,
            fontSize = 13,
            fixedHeight = 24,
            fixedWidth = 160,
            stretchWidth = true,
            stretchHeight = true,
            padding = new(5, 5, 10, 10),
            margin = new(1, 1, 1, 1),
            border = new(-5, -5, -5, -20),
            normal = 
            {
                textColor = HierarchyDesigner_Shared_Color.GetPrimaryFontColor(),
                background = HierarchyDesigner_Shared_Texture.CreateTexture(2, 2, HierarchyDesigner_Shared_Color.GetPopupPanelColor())
            },
            hover = new()
            {
                textColor = HierarchyDesigner_Shared_Color.GetSecondaryFontColor(),
                background = HierarchyDesigner_Shared_Texture.CreateTexture(2, 2, HierarchyDesigner_Shared_Color.GetPopupPanelColor())
            },
            active = new()
            {
                textColor = HierarchyDesigner_Shared_Color.GetSecondaryFontColorFaded(),
                background = HierarchyDesigner_Shared_Texture.CreateTexture(2, 2, HierarchyDesigner_Shared_Color.GetPopupPanelColor())
            }
        });
        public static GUIStyle MenuPopupPanelStyle => _menuPopupPanelStyle.Value;

        private static readonly Lazy<GUIStyle> _inspectorFolderPanelStyle = new(() => new()
        {
            stretchWidth = true,
            stretchHeight = true,
            padding = new(-5, 0, 0, 0),
            border = new(-19, -5, -5, -7),
        });
        public static GUIStyle InspectorFolderPanelStyle => _inspectorFolderPanelStyle.Value;

        private static readonly Lazy<GUIStyle> _inspectorFolderInnerPanelStyle = new(() => new(EditorStyles.helpBox)
        {
            padding = new(10, 10, 10, 10),
            margin = new(0, 8, 4, 4),
            border = new(2, 2, 2, 2),
            normal = { background = HierarchyDesigner_Shared_Texture.CreateGradientTexture(2, 4, new Color[] { HierarchyDesigner_Shared_Color.GetSecondaryPanelColorTop(), HierarchyDesigner_Shared_Color.GetSecondaryPanelColorBottom() }) }
        });
        public static GUIStyle InspectorFolderInnerPanelStyle => _inspectorFolderInnerPanelStyle.Value;
        #endregion

        #region Buttons
        private static readonly Lazy<GUIStyle> _primaryButtonStyle = new(() => new(GUI.skin.button)
        {
            font = HierarchyDesigner_Shared_Resources.Fonts.Bold,
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter,
            padding = new(4, 4, 0, 0),
            richText = true,
            normal = new()
            {
                textColor = HierarchyDesigner_Shared_Color.GetPrimaryFontColor(),
                background = HierarchyDesigner_Shared_Texture.ClearTexture
            },
            hover = new()
            {
                textColor = HierarchyDesigner_Shared_Color.GetSecondaryFontColor(),
                background = HierarchyDesigner_Shared_Texture.ClearTexture
            },
            active = new()
            {
                textColor = HierarchyDesigner_Shared_Color.GetSecondaryFontColorFaded(),
                background = HierarchyDesigner_Shared_Texture.ClearTexture
            }
        });
        public static GUIStyle PrimaryButtonStyle => _primaryButtonStyle.Value;

        private static readonly Lazy<GUIStyle> _headerButtonStyle = new(() => new(GUI.skin.button)
        {
            font = HierarchyDesigner_Shared_Resources.Fonts.Bold,
            fontSize = 15,
            alignment = TextAnchor.UpperCenter,
            padding = new(2, 2, 1, 0),
            richText = true,
            normal = new()
            {
                textColor = HierarchyDesigner_Shared_Color.GetPrimaryFontColor(),
                background = HierarchyDesigner_Shared_Texture.ClearTexture
            },
            hover = new()
            {
                textColor = HierarchyDesigner_Shared_Color.GetSecondaryFontColor(),
                background = HierarchyDesigner_Shared_Texture.ClearTexture
            },
            active = new()
            {
                textColor = HierarchyDesigner_Shared_Color.GetSecondaryFontColorFaded(),
                background = HierarchyDesigner_Shared_Texture.ClearTexture
            }
        });
        public static GUIStyle HeaderButtonStyle => _headerButtonStyle.Value;

        private static readonly Lazy<GUIStyle> _headerButtonsPanelStyle = new(() => new()
        {
            fixedHeight = 34,
            border = new(2, 2, 2, 2),
            normal = { background = HierarchyDesigner_Shared_Texture.CreateGradientTexture(2, 32, new Color[] { HierarchyDesigner_Shared_Color.GetTertiaryPanelColorBottom(), HierarchyDesigner_Shared_Color.GetTertiaryPanelColorTop() }) }
        });
        public static GUIStyle HeaderButtonsPanelStyle => _headerButtonsPanelStyle.Value;

        private static readonly Lazy<GUIStyle> _resetButtonStyle = new(() => new(GUI.skin.button)
        {
            fixedHeight = 25,
            fixedWidth = 25,
            overflow = new(3, 3, 2, 2),
            margin = new(0, 0, -2, 0),
            normal = { background = HierarchyDesigner_Shared_Resources.Icons.Reset }
        });
        public static GUIStyle ResetButtonStyle => _resetButtonStyle.Value;

        private static readonly Lazy<GUIStyle> _promotionalPicEaseStyle = new(() => new()
        {
            fixedHeight = 68,
            fixedWidth = 68,
            normal = { background = HierarchyDesigner_Shared_Resources.Promotional.PicEasePromotionalIcon }
        });
        public static GUIStyle PromotionalPicEaseStyle => _promotionalPicEaseStyle.Value;

        private static readonly Lazy<GUIStyle> _promotionalPicEaseLiteStyle = new(() => new()
        {
            fixedHeight = 68,
            fixedWidth = 68,
            normal = { background = HierarchyDesigner_Shared_Resources.Promotional.PicEaseLitePromotionalIcon }
        });
        public static GUIStyle PromotionalPicEaseLiteStyle => _promotionalPicEaseLiteStyle.Value;

        private static readonly Lazy<GUIStyle> _tooltipButtonStyle = new(() => new(GUI.skin.button)
        {
            fixedHeight = 25,
            fixedWidth = 16,
            overflow = new(9, 7, 2, 2),
            margin = new(0, 0, -1, 0),
            normal = { background = HierarchyDesigner_Shared_Resources.Icons.Tooltip }
        });
        public static GUIStyle TooltipButtonStyle => _tooltipButtonStyle.Value;

        #endregion
        #endregion

        #region Classes
        private sealed class CustomPopupMenu : PopupWindowContent
        {
            #region Properties
            private readonly Dictionary<string, Action> menuItems;
            #endregion

            #region Constructor
            public CustomPopupMenu(Dictionary<string, Action> items)
            {
                menuItems = items;
            }
            #endregion

            #region Override Methods
            public override void OnGUI(Rect rect)
            {
                GUILayout.BeginVertical();
                foreach (KeyValuePair<string, Action> kvp in menuItems)
                {
                    if (GUILayout.Button(kvp.Key, MenuPopupPanelStyle, GUILayout.Height(MenuPopupPanelStyle.fixedHeight)))
                    {
                        kvp.Value?.Invoke();
                        editorWindow.Close();
                    }
                }
                GUILayout.EndVertical();
            }

            public override Vector2 GetWindowSize()
            {
                return new(MenuPopupPanelStyle.fixedWidth, (menuItems.Count * MenuPopupPanelStyle.fixedHeight) + 6);
            }
            #endregion
        }

        private sealed class TooltipPopup : PopupWindowContent
        {
            #region Properties
            private readonly string tooltipText;
            private static readonly GUIStyle style;
            private const float maxWidth = 240f;
            private const int fontSize = 13;
            #endregion

            #region Constructor
            static TooltipPopup()
            {
                style = new()
                {
                    font = HierarchyDesigner_Shared_Resources.Fonts.Regular,
                    fontSize = fontSize,
                    alignment = TextAnchor.MiddleLeft,
                    wordWrap = true,
                    padding = new(4, 2, 2, 3),
                    border = new(2, 2, 2, 2),
                    normal =
                    {
                        textColor = HierarchyDesigner_Shared_Color.GetPrimaryFontColor(),
                        background = HierarchyDesigner_Shared_Texture.CreateTexture(2, 2, HierarchyDesigner_Shared_Color.GetPopupPanelColor())
                    }
                };
            }
            #endregion

            #region Accessor
            public TooltipPopup(string text)
            {
                tooltipText = text;
            }
            #endregion

            #region Override Methods
            public override void OnGUI(Rect rect)
            {
                GUILayout.Label(tooltipText, style);
            }

            public override Vector2 GetWindowSize()
            {
                GUIContent content = new(tooltipText);
                float height = style.CalcHeight(content, maxWidth);
                return new(maxWidth, height);
            }
            #endregion
        }
        #endregion

        #region Methods
        public static int DrawIntSlider(string label, float labelWidth, int value, int defaultValue, int leftValue, int rightValue, bool showTooltip = false, string tooltipText = "")
        {
            EditorGUILayout.BeginHorizontal();

            if (showTooltip)
            {
                DrawTooltip(tooltipText);
            }

            EditorGUILayout.LabelField(label, LayoutLabelStyle, GUILayout.Width(labelWidth));
            int newValue = EditorGUILayout.IntSlider(value, leftValue, rightValue, GUILayout.ExpandWidth(true));
            GUILayout.Space(3);

            if (GUILayout.Button(string.Empty, ResetButtonStyle))
            {
                newValue = defaultValue;
            }

            EditorGUILayout.EndHorizontal();
            return newValue;
        }

        public static float DrawFloatSlider(string label, float labelWidth, float value, float defaultValue, float leftValue, float rightValue, bool showTooltip = false, string tooltipText = "")
        {
            EditorGUILayout.BeginHorizontal();

            if (showTooltip)
            {
                DrawTooltip(tooltipText);
            }

            EditorGUILayout.LabelField(label, LayoutLabelStyle, GUILayout.Width(labelWidth));
            float newValue = EditorGUILayout.Slider(value, leftValue, rightValue, GUILayout.ExpandWidth(true));
            GUILayout.Space(3);

            if (GUILayout.Button(string.Empty, ResetButtonStyle))
            {
                newValue = defaultValue;
            }

            EditorGUILayout.EndHorizontal();
            return newValue;
        }

        public static bool DrawToggle(string label, float labelWidth, bool value, bool defaultValue, bool showTooltip = false, string tooltipText = "")
        {
            EditorGUILayout.BeginHorizontal();

            if (showTooltip)
            {
                DrawTooltip(tooltipText);
            }

            EditorGUILayout.LabelField(label, LayoutLabelStyle, GUILayout.Width(labelWidth));
            bool newValue = EditorGUILayout.Toggle(value, GUILayout.ExpandWidth(true));
            GUILayout.Space(3);

            if (GUILayout.Button(string.Empty, ResetButtonStyle))
            {
                newValue = defaultValue;
            }

            EditorGUILayout.EndHorizontal();
            return newValue;
        }

        public static T DrawEnumPopup<T>(string label, float labelWidth, T selectedEnum, T defaultEnum, bool showTooltip = false, string tooltipText = "") where T : Enum
        {
            EditorGUILayout.BeginHorizontal();

            if (showTooltip)
            {
                DrawTooltip(tooltipText);
            }

            EditorGUILayout.LabelField(label, LayoutLabelStyle, GUILayout.Width(labelWidth));
            T newEnum = (T)EditorGUILayout.EnumPopup(selectedEnum, GUILayout.ExpandWidth(true));
            GUILayout.Space(3);

            if (GUILayout.Button(string.Empty, ResetButtonStyle))
            {
                newEnum = defaultEnum;
            }

            EditorGUILayout.EndHorizontal();
            return newEnum;
        }

        public static int DrawMaskField(string label, float labelWidth, int mask, int defaultValue, string[] options, bool showTooltip = false, string tooltipText = "")
        {
            EditorGUILayout.BeginHorizontal();

            if (showTooltip)
            {
                DrawTooltip(tooltipText);
            }

            EditorGUILayout.LabelField(label, LayoutLabelStyle, GUILayout.Width(labelWidth));
            int newMask = EditorGUILayout.MaskField(mask, options, GUILayout.ExpandWidth(true));
            GUILayout.Space(3);

            if (GUILayout.Button(string.Empty, ResetButtonStyle))
            {
                newMask = defaultValue;
            }

            EditorGUILayout.EndHorizontal();
            return newMask;
        }

        public static Color DrawColorField(string label, float labelWidth, string defaultColorHexCode, Color colorValue, bool showTooltip = false, string tooltipText = "")
        {
            EditorGUILayout.BeginHorizontal();

            if (showTooltip)
            {
                DrawTooltip(tooltipText);
            }

            EditorGUILayout.LabelField(label, LayoutLabelStyle, GUILayout.Width(labelWidth));
            Color newColor = EditorGUILayout.ColorField(colorValue, GUILayout.ExpandWidth(true));
            GUILayout.Space(3);

            if (GUILayout.Button(string.Empty, ResetButtonStyle))
            {
                newColor = HierarchyDesigner_Shared_Color.HexToColor(defaultColorHexCode);
            }

            EditorGUILayout.EndHorizontal();
            return newColor;
        }

        public static Gradient DrawGradientField(string label, float labelWidth, Gradient defaultGradient, Gradient gradientValue, bool showTooltip = false, string tooltipText = "")
        {
            EditorGUILayout.BeginHorizontal();

            if (showTooltip)
            {
                DrawTooltip(tooltipText);
            }

            EditorGUILayout.LabelField(label, LayoutLabelStyle, GUILayout.Width(labelWidth));
            Gradient newGradient = EditorGUILayout.GradientField(gradientValue, GUILayout.ExpandWidth(true));
            GUILayout.Space(3);

            if (GUILayout.Button(string.Empty, ResetButtonStyle))
            {
                newGradient = defaultGradient;
            }

            EditorGUILayout.EndHorizontal();
            return newGradient;
        }

        public static string DrawTextField(string label, float labelWidth, string defaultValue, string textValue, bool showTooltip = false, string tooltipText = "")
        {
            EditorGUILayout.BeginHorizontal();

            if (showTooltip)
            {
                DrawTooltip(tooltipText);
            }

            EditorGUILayout.LabelField(label, LayoutLabelStyle, GUILayout.Width(labelWidth));
            string newValue = EditorGUILayout.TextField(textValue);
            GUILayout.Space(3);

            if (GUILayout.Button(string.Empty, ResetButtonStyle))
            {
                newValue = defaultValue;
            }

            EditorGUILayout.EndHorizontal();
            return newValue;
        }

        public static int DrawIntField(string label, float labelWidth, int intValue, int defaultValue, bool showTooltip = false, string tooltipText = "")
        {
            EditorGUILayout.BeginHorizontal();

            if (showTooltip)
            {
                DrawTooltip(tooltipText);
            }

            EditorGUILayout.LabelField(label, LayoutLabelStyle, GUILayout.Width(labelWidth));
            int newValue = EditorGUILayout.IntField(intValue);
            GUILayout.Space(3);

            if (GUILayout.Button(string.Empty, ResetButtonStyle))
            {
                newValue = defaultValue;
            }

            EditorGUILayout.EndHorizontal();
            return newValue;
        }

        public static void DrawPropertyField(string label, float labelWidth, SerializedProperty property, object defaultValue, bool showTooltip = false, string tooltipText = "")
        {
            EditorGUILayout.BeginHorizontal();

            if (showTooltip)
            {
                DrawTooltip(tooltipText);
            }

            EditorGUILayout.LabelField(label, InspectorFolderActiveLabelStyle, GUILayout.Width(labelWidth));
            EditorGUILayout.PropertyField(property, GUIContent.none, GUILayout.ExpandWidth(true));
            GUILayout.Space(3);

            if (GUILayout.Button(string.Empty, ResetButtonStyle))
            {
                switch (property.propertyType)
                {
                    case SerializedPropertyType.Integer:
                        property.intValue = Convert.ToInt32(defaultValue);
                        break;
                    case SerializedPropertyType.Float:
                        property.floatValue = Convert.ToSingle(defaultValue);
                        break;
                    case SerializedPropertyType.Boolean:
                        property.boolValue = Convert.ToBoolean(defaultValue);
                        break;
                    case SerializedPropertyType.String:
                        property.stringValue = Convert.ToString(defaultValue);
                        break;
                    case SerializedPropertyType.Color:
                        if (defaultValue is Color colorDefault) property.colorValue = colorDefault;
                        break;
                    case SerializedPropertyType.Enum:
                        if (defaultValue is Enum enumDefault) property.enumValueIndex = Convert.ToInt32(enumDefault);
                        break;
                    case SerializedPropertyType.Vector2:
                        if (defaultValue is Vector2 vector2Default) property.vector2Value = vector2Default;
                        break;
                    case SerializedPropertyType.Vector3:
                        if (defaultValue is Vector3 vector3Default) property.vector3Value = vector3Default;
                        break;
                    case SerializedPropertyType.Vector4:
                        if (defaultValue is Vector4 vector4Default) property.vector4Value = vector4Default;
                        break;
                    case SerializedPropertyType.Rect:
                        if (defaultValue is Rect rectDefault) property.rectValue = rectDefault;
                        break;
                    case SerializedPropertyType.Bounds:
                        if (defaultValue is Bounds boundsDefault) property.boundsValue = boundsDefault;
                        break;
                    case SerializedPropertyType.Quaternion:
                        if (defaultValue is Quaternion quaternionDefault) property.quaternionValue = quaternionDefault;
                        break;
                    default:
                        Debug.LogWarning($"Reset not implemented for property type: {property.propertyType}");
                        break;
                }
                property.serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.EndHorizontal();
        }
        #endregion

        #region Operations
        #region General
        public static float CalculateMaxLabelWidth(IEnumerable<string> names)
        {
            float labelWidth = 0;
            foreach (string name in names)
            {
                GUIContent content = new(name);
                Vector2 size = LayoutLabelStyle.CalcSize(content);
                if (size.x > labelWidth) labelWidth = size.x;
            }
            return labelWidth + 35f;
        }

        public static float CalculateMaxLabelWidth(Transform parent)
        {
            float maxWidth = 0;
            GatherChildNamesAndCalculateMaxWidth(parent, ref maxWidth);
            return maxWidth + 30f;
        }

        private static void GatherChildNamesAndCalculateMaxWidth(Transform parent, ref float maxWidth)
        {
            GUIStyle labelStyle = GUI.skin.label;
            foreach (Transform child in parent)
            {
                GUIContent content = new(child.name);
                Vector2 size = labelStyle.CalcSize(content);
                if (size.x > maxWidth) maxWidth = size.x;
                GatherChildNamesAndCalculateMaxWidth(child, ref maxWidth);
            }
        }
        #endregion

        #region Inner Classes
        public static void DrawPopupButton(string buttonText, GUIStyle buttonStyle, float buttonHeight, Dictionary<string, Action> menuItems)
        {
            Rect buttonRect = GUILayoutUtility.GetRect(new(buttonText), buttonStyle, GUILayout.Height(buttonHeight), GUILayout.ExpandWidth(false));

            if (GUI.Button(buttonRect, buttonText, buttonStyle))
            {
                Rect popupRect = new(buttonRect.x + 4, buttonRect.y + buttonRect.height + 4, 0, 0);
                PopupWindow.Show(popupRect, new CustomPopupMenu(menuItems));
            }

            GUILayout.Space(2);
        }

        private static void DrawTooltip(string tooltipText)
        {
            Rect buttonRect = GUILayoutUtility.GetRect(TooltipButtonStyle.fixedWidth, TooltipButtonStyle.fixedHeight, TooltipButtonStyle, GUILayout.ExpandWidth(false));
            if (GUI.Button(buttonRect, GUIContent.none, TooltipButtonStyle))
            {
                Rect popupRect = new(buttonRect.x, buttonRect.y + buttonRect.height + 4, 0, 0);
                PopupWindow.Show(popupRect, new TooltipPopup(tooltipText));
            }
            GUILayout.Space(2);
        }
        #endregion
        #endregion
    }
}
#endif