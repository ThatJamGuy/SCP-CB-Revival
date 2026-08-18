#if UNITY_EDITOR
namespace PixeLadder.EasyTooltip.Editor {
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Custom Inspector for the TooltipTrigger.
    /// Handles multi-object editing safely, provides WYSIWYG previewing, 
    /// and organizes settings into clean, styled foldouts.
    /// </summary>
    [CustomEditor(typeof(TooltipTrigger))]
    [CanEditMultipleObjects]
    public class TooltipTriggerEditor : Editor {
        private SerializedProperty customPrefab;
        private SerializedProperty title;
        private SerializedProperty content;
        private SerializedProperty secondaryContent;
        private SerializedProperty icon;

        private SerializedProperty overrideStyle;
        private SerializedProperty titleColor;
        private SerializedProperty iconColor;
        private SerializedProperty panelColor;
        private SerializedProperty headerColor;
        private SerializedProperty bodyColor;
        private SerializedProperty headerSprite;
        private SerializedProperty bodySprite;
        private SerializedProperty showOutline;
        private SerializedProperty outlineColor;

        private SerializedProperty overrideSeparators;
        private SerializedProperty separatorSprite;
        private SerializedProperty separatorColor;
        private SerializedProperty separatorHeight;

        private SerializedProperty overrideLayout;
        private SerializedProperty positionMode;
        private SerializedProperty continuousTracking;
        private SerializedProperty anchorPosition;
        private SerializedProperty overrideGap;
        private SerializedProperty fixedGap;
        private SerializedProperty targetOverride;
        private SerializedProperty additionalOffset;
        private SerializedProperty overrideConstraints;
        private SerializedProperty enableClamping;
        private SerializedProperty smartFlipping;

        private SerializedProperty overrideSize;
        private SerializedProperty maxWidth;

        private SerializedProperty overrideTiming;
        private SerializedProperty hoverDelay;
        private SerializedProperty fadeDuration;

        private SerializedProperty onTooltipShow;
        private SerializedProperty onTooltipHide;

        private Texture2D bannerImage;
        private Texture2D iconRate;
        private Texture2D iconCreate;
        private Texture2D iconSupport;
        private Texture2D iconDelete;
        private Texture2D switchTrackOn;
        private Texture2D switchTrackOff;
        private Font customFont;
        private Texture2D panelBackground;

        private GUIStyle panelStyle;
        private GUIStyle headerStyle;
        private GUIStyle customLabelStyle;
        private GUIStyle foldoutStyle;
        private GUIStyle supportFoldoutStyle;
        private GUIStyle textAreaStyle;

        private bool showAdvancedContent {
            get => SessionState.GetBool($"EasyTooltip_Adv_{target.GetHashCode()}", false);
            set => SessionState.SetBool($"EasyTooltip_Adv_{target.GetHashCode()}", value);
        }
        private bool showEvents {
            get => SessionState.GetBool($"EasyTooltip_Evt_{target.GetHashCode()}", false);
            set => SessionState.SetBool($"EasyTooltip_Evt_{target.GetHashCode()}", value);
        }
        private bool showSupport {
            get => SessionState.GetBool($"EasyTooltip_Sup_{target.GetHashCode()}", false);
            set => SessionState.SetBool($"EasyTooltip_Sup_{target.GetHashCode()}", value);
        }

        private bool isPreviewing {
            get => SessionState.GetBool($"EasyTooltip_Preview_{target.GetHashCode()}", false);
            set => SessionState.SetBool($"EasyTooltip_Preview_{target.GetHashCode()}", value);
        }

        private Vector2 lastPreviewSize;

        public override bool RequiresConstantRepaint() => Application.isPlaying || isPreviewing;

        private void OnEnable() {
            Undo.undoRedoPerformed += OnUndoRedo;
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            customPrefab = serializedObject.FindProperty("customPrefab");
            title = serializedObject.FindProperty("title");
            content = serializedObject.FindProperty("content");
            secondaryContent = serializedObject.FindProperty("secondaryContent");
            icon = serializedObject.FindProperty("icon");

            overrideStyle = serializedObject.FindProperty("overrideStyle");
            titleColor = serializedObject.FindProperty("titleColor");
            iconColor = serializedObject.FindProperty("iconColor");
            panelColor = serializedObject.FindProperty("panelColor");
            headerColor = serializedObject.FindProperty("headerColor");
            bodyColor = serializedObject.FindProperty("bodyColor");
            headerSprite = serializedObject.FindProperty("headerSprite");
            bodySprite = serializedObject.FindProperty("bodySprite");
            showOutline = serializedObject.FindProperty("showOutline");
            outlineColor = serializedObject.FindProperty("outlineColor");

            overrideSeparators = serializedObject.FindProperty("overrideSeparators");
            separatorSprite = serializedObject.FindProperty("separatorSprite");
            separatorColor = serializedObject.FindProperty("separatorColor");
            separatorHeight = serializedObject.FindProperty("separatorHeight");

            overrideLayout = serializedObject.FindProperty("overrideLayout");
            positionMode = serializedObject.FindProperty("positionMode");
            continuousTracking = serializedObject.FindProperty("continuousTracking");
            anchorPosition = serializedObject.FindProperty("anchorPosition");
            overrideGap = serializedObject.FindProperty("overrideGap");
            fixedGap = serializedObject.FindProperty("fixedGap");
            targetOverride = serializedObject.FindProperty("targetOverride");
            additionalOffset = serializedObject.FindProperty("additionalOffset");
            overrideConstraints = serializedObject.FindProperty("overrideConstraints");
            enableClamping = serializedObject.FindProperty("enableClamping");
            smartFlipping = serializedObject.FindProperty("smartFlipping");

            overrideSize = serializedObject.FindProperty("overrideSize");
            maxWidth = serializedObject.FindProperty("maxWidth");

            overrideTiming = serializedObject.FindProperty("overrideTiming");
            hoverDelay = serializedObject.FindProperty("hoverDelay");
            fadeDuration = serializedObject.FindProperty("fadeDuration");

            onTooltipShow = serializedObject.FindProperty("onTooltipShow");
            onTooltipHide = serializedObject.FindProperty("onTooltipHide");

            bannerImage = LoadTexture("EasyTooltipBanner_TooltipTrigger");
            iconRate = LoadTexture("EasyTooltip_IconRate");
            iconCreate = LoadTexture("EasyTooltip_IconCreate");
            iconSupport = LoadTexture("EasyTooltip_IconSupport");
            iconDelete = LoadTexture("EasyTooltip_IconDelete");
            switchTrackOn = LoadTexture("EasyTooltip_SwitchOn");
            switchTrackOff = LoadTexture("EasyTooltip_SwitchOff");

            string[] fontGuids = AssetDatabase.FindAssets("EasyTooltipFont t:Font");
            if (fontGuids.Length > 0)
                customFont = AssetDatabase.LoadAssetAtPath<Font>(AssetDatabase.GUIDToAssetPath(fontGuids[0]));

            float bgTone = EditorGUIUtility.isProSkin ? 0.15f : 0.85f;
            panelBackground = MakeTexture(1, 1, new Color(bgTone, bgTone, bgTone, 0.5f));
        }

        private void OnDisable() {
            Undo.undoRedoPerformed -= OnUndoRedo;
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

            if (isPreviewing) {
                TooltipTrigger trigger = (TooltipTrigger)target;
                if (trigger != null && Selection.activeGameObject != trigger.gameObject) {
                    TooltipManager manager = TooltipManager.Instance != null ? TooltipManager.Instance : FindAnyObjectByType<TooltipManager>();
                    if (manager != null) manager.HideTooltip();
                    isPreviewing = false;
                }
            }

            if (panelBackground != null) DestroyImmediate(panelBackground);
        }

        private void OnUndoRedo() {
            if (isPreviewing) {
                TooltipTrigger trigger = (TooltipTrigger)target;
                if (trigger != null) trigger.EditorPreviewTooltip();
                Repaint();
            }
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state) {
            if (state == PlayModeStateChange.ExitingEditMode && isPreviewing) {
                TooltipManager manager = TooltipManager.Instance != null ? TooltipManager.Instance : FindAnyObjectByType<TooltipManager>();
                if (manager != null) manager.HideTooltip();
                isPreviewing = false;
            }
        }

        private void OnEditorUpdate() {
            if (isPreviewing) {
                TooltipTrigger trigger = (TooltipTrigger)target;
                if (trigger != null) {
                    bool changed = false;

                    if (trigger.transform.hasChanged) {
                        trigger.transform.hasChanged = false;
                        changed = true;
                    }

                    RectTransform rect = trigger.GetComponent<RectTransform>();
                    if (rect != null && rect.rect.size != lastPreviewSize) {
                        lastPreviewSize = rect.rect.size;
                        changed = true;
                    }

                    if (changed) trigger.EditorPreviewTooltip();
                }
            }
        }

        public override void OnInspectorGUI() {
            EditorGUI.BeginChangeCheck();

            serializedObject.Update();
            InitializeStyles();

            DrawFullWidthHeader();

            DrawPreviewButton();

            DrawSectionHeader("C O N T E N T");
            DrawContentSection();

            DrawSectionHeader("C U S T O M   P R E F A B");
            DrawPrefabSection();

            DrawSectionHeader("L A Y O U T   &   P O S I T I O N");
            DrawToggleSection(overrideLayout, "Override Global Layout", DrawLayoutSection);
            GUILayout.Space(5);
            DrawToggleSection(overrideConstraints, "Override Constraints", () => {
                EditorGUILayout.PropertyField(enableClamping);
                EditorGUILayout.PropertyField(smartFlipping);
            });

            DrawSectionHeader("S I Z E   &   T I M I N G");
            DrawToggleSection(overrideSize, "Override Global Width", () => EditorGUILayout.PropertyField(maxWidth, new GUIContent("Max Width")));
            GUILayout.Space(5);
            DrawToggleSection(overrideTiming, "Override Global Timing", () => {
                EditorGUILayout.PropertyField(hoverDelay, new GUIContent("Hover Delay"));
                EditorGUILayout.PropertyField(fadeDuration, new GUIContent("Fade Duration"));
            });

            DrawSectionHeader("V I S U A L   S T Y L E");
            DrawToggleSection(overrideStyle, "Override Global Style", DrawStyleSection);
            GUILayout.Space(5);
            DrawToggleSection(overrideSeparators, "Override Separator Style", DrawSeparatorSection);

            DrawSectionHeader("E V E N T   H O O K S");
            DrawEventsSection();

            DrawSupportSection();

            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck() && isPreviewing) {
                TooltipTrigger trigger = (TooltipTrigger)target;
                trigger.EditorPreviewTooltip();
            }
        }

        private void InitializeStyles() {
            if (panelStyle == null || supportFoldoutStyle == null) {
                panelStyle = new GUIStyle(GUI.skin.box) {
                    normal = { background = panelBackground },
                    margin = new RectOffset(10, 10, 5, 10),
                    padding = new RectOffset(15, 15, 15, 15)
                };

                headerStyle = new GUIStyle(EditorStyles.label) {
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 11,
                    font = customFont
                };

                customLabelStyle = new GUIStyle(EditorStyles.label) { font = customFont };

                foldoutStyle = new GUIStyle(EditorStyles.foldout) {
                    font = customFont,
                    fontStyle = FontStyle.Bold,
                    padding = new RectOffset(18, 0, 0, 0)
                };

                supportFoldoutStyle = new GUIStyle(EditorStyles.foldout) {
                    font = customFont,
                    fontStyle = FontStyle.Bold,
                    padding = new RectOffset(32, 0, 0, 0)
                };

                textAreaStyle = new GUIStyle(EditorStyles.textArea) {
                    wordWrap = true,
                    padding = new RectOffset(5, 5, 5, 5)
                };
            }
        }

        private void DrawFullWidthHeader() {
            Rect fullRect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 64f);
            fullRect.x = 0;
            fullRect.width = EditorGUIUtility.currentViewWidth;

            if (bannerImage != null) {
                GUI.DrawTexture(fullRect, bannerImage, ScaleMode.ScaleAndCrop);
            } else {
                EditorGUI.DrawRect(fullRect, EditorGUIUtility.isProSkin ? new Color(0.1f, 0.1f, 0.1f) : new Color(0.9f, 0.9f, 0.9f));
                GUI.Label(fullRect, "E A S Y   T O O L T I P", headerStyle);
            }
            GUILayout.Space(10);
        }

        private void DrawSectionHeader(string titleText) {
            GUILayout.Space(10);
            GUILayout.Label(titleText, headerStyle);

            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            rect.xMin += 10;
            rect.xMax -= 10;
            EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? new Color(0.4f, 0.4f, 0.4f, 0.5f) : new Color(0.6f, 0.6f, 0.6f, 0.5f));
            GUILayout.Space(5);
        }

        private void DrawPrefabSection() {
            EditorGUILayout.BeginVertical(panelStyle);
            EditorGUILayout.PropertyField(customPrefab, new GUIContent("Prefab Override"));
            if (customPrefab.objectReferenceValue != null) {
                GUILayout.Space(5);
                DrawCustomHelpBox("Using a Custom Prefab. Standard content fields will only be applied if they exist on the assigned prefab.", false);
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawContentSection() {
            EditorGUILayout.BeginVertical(panelStyle);

            EditorGUILayout.LabelField(new GUIContent("Title", "The header text of the tooltip. Leave empty to hide."), EditorStyles.boldLabel);

            EditorGUI.showMixedValue = title.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();
            string newTitle = EditorGUILayout.TextArea(title.stringValue, textAreaStyle, GUILayout.Height(40));
            if (EditorGUI.EndChangeCheck()) {
                title.stringValue = newTitle;
            }
            EditorGUI.showMixedValue = false;

            GUILayout.Space(5);

            EditorGUILayout.PropertyField(icon);
            GUILayout.Space(5);

            EditorGUILayout.LabelField(new GUIContent("Main Content", "The main body text of the tooltip."), EditorStyles.boldLabel);

            EditorGUI.showMixedValue = content.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();
            string newContent = EditorGUILayout.TextArea(content.stringValue, textAreaStyle, GUILayout.Height(60));
            if (EditorGUI.EndChangeCheck()) {
                content.stringValue = newContent;
            }
            EditorGUI.showMixedValue = false;

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5);
            showAdvancedContent = EditorGUILayout.Foldout(showAdvancedContent, "Advanced Content (Auto-Separated)", true, foldoutStyle);
            EditorGUILayout.EndHorizontal();

            if (showAdvancedContent) {
                GUILayout.Space(5);
                bool isMultiEditing = targets.Length > 1;

                for (int i = 0; i < secondaryContent.arraySize; i++) {
                    EditorGUILayout.BeginHorizontal();
                    SerializedProperty element = secondaryContent.GetArrayElementAtIndex(i);

                    EditorGUI.showMixedValue = element.hasMultipleDifferentValues;
                    EditorGUI.BeginChangeCheck();
                    string newElemValue = EditorGUILayout.TextArea(element.stringValue, textAreaStyle, GUILayout.Height(50));
                    if (EditorGUI.EndChangeCheck()) {
                        element.stringValue = newElemValue;
                    }
                    EditorGUI.showMixedValue = false;

                    Rect delRect = GUILayoutUtility.GetRect(28, 50, GUILayout.ExpandWidth(false));

                    EditorGUI.BeginDisabledGroup(isMultiEditing);
                    Color oldBg = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(0.8f, 0.2f, 0.2f, 1f);

                    bool deleteClicked = GUI.Button(delRect, GUIContent.none);
                    GUI.backgroundColor = oldBg;

                    if (iconDelete != null) {
                        Color oldColor = GUI.color;
                        GUI.color = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                        if (isMultiEditing) GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, 0.5f);
                        GUI.DrawTexture(new Rect(delRect.x + 7, delRect.y + 18, 14, 14), iconDelete, ScaleMode.ScaleToFit);
                        GUI.color = oldColor;
                    } else {
                        GUI.Label(delRect, "X", new GUIStyle(customLabelStyle) { alignment = TextAnchor.MiddleCenter, fontSize = 12 });
                    }

                    if (deleteClicked && !isMultiEditing) {
                        secondaryContent.DeleteArrayElementAtIndex(i);
                        break;
                    }
                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(2);
                }

                GUILayout.Space(5);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                float rowHeight = 28f;
                Rect btnRect = GUILayoutUtility.GetRect(130, rowHeight);

                EditorGUI.BeginDisabledGroup(isMultiEditing);
                Color oldBgAdd = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
                bool addClicked = GUI.Button(btnRect, GUIContent.none);
                GUI.backgroundColor = oldBgAdd;

                float iconSize = 14f;
                float textWidth = 90f;
                float spacing = 5f;
                float totalW = (iconCreate != null ? iconSize + spacing : 0) + textWidth;
                float startX = btnRect.x + (btnRect.width - totalW) / 2f;

                if (iconCreate != null) {
                    Color oldColor = GUI.color;
                    GUI.color = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                    if (isMultiEditing) GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, 0.5f);
                    GUI.DrawTexture(new Rect(startX, btnRect.y + (rowHeight - iconSize) / 2f, iconSize, iconSize), iconCreate, ScaleMode.ScaleToFit);
                    GUI.color = oldColor;
                }

                float textX = startX + (iconCreate != null ? iconSize + spacing : 0);
                GUI.Label(new Rect(textX, btnRect.y, textWidth, btnRect.height), "Add Text Block", new GUIStyle(customLabelStyle) { alignment = TextAnchor.MiddleLeft });

                if (addClicked && !isMultiEditing) {
                    secondaryContent.arraySize++;
                    secondaryContent.GetArrayElementAtIndex(secondaryContent.arraySize - 1).stringValue = "";
                }
                EditorGUI.EndDisabledGroup();

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawLayoutSection() {
            EditorGUILayout.PropertyField(positionMode);
            EditorGUI.indentLevel++;

            if (positionMode.enumValueIndex == (int)TooltipPositionMode.Fixed) {
                EditorGUILayout.PropertyField(anchorPosition);

                TooltipTrigger trigger = (TooltipTrigger)target;
                TooltipManager manager = TooltipManager.Instance != null ? TooltipManager.Instance : FindAnyObjectByType<TooltipManager>();

                if ((isPreviewing || Application.isPlaying) && manager != null && manager.CurrentState != TooltipState.Idle) {
                    bool isFlipped = manager.CurrentRenderedAnchor != trigger.AnchorPosition;
                    bool isClamped = manager.IsClamped;

                    if (isFlipped || isClamped) {
                        GUIStyle feedbackStyle = new GUIStyle(EditorStyles.label) { fontSize = 10, fontStyle = FontStyle.Bold };
                        feedbackStyle.normal.textColor = new Color(0.9f, 0.6f, 0.2f);

                        if (isFlipped) EditorGUILayout.LabelField($"   ↳ Flipped: {manager.CurrentRenderedAnchor}", feedbackStyle);
                        if (isClamped) EditorGUILayout.LabelField("   ↳ Clamped to screen", feedbackStyle);
                    }
                }

                EditorGUILayout.PropertyField(targetOverride);

                GUILayout.Space(5);
                DrawCustomSwitch(overrideGap, "Override Fixed Gap", "");
                if (overrideGap.boolValue) {
                    EditorGUILayout.PropertyField(fixedGap);
                }
                GUILayout.Space(5);
            } else {
                EditorGUILayout.PropertyField(continuousTracking);
            }

            EditorGUILayout.PropertyField(additionalOffset);

            EditorGUI.indentLevel--;
        }

        private void DrawStyleSection() {
            EditorGUILayout.PropertyField(titleColor);
            EditorGUILayout.PropertyField(iconColor);
            GUILayout.Space(5);

            EditorGUILayout.PropertyField(panelColor);
            EditorGUILayout.PropertyField(headerColor);
            EditorGUILayout.PropertyField(bodyColor);
            GUILayout.Space(5);

            EditorGUILayout.PropertyField(headerSprite);
            EditorGUILayout.PropertyField(bodySprite);
            GUILayout.Space(5);

            EditorGUILayout.PropertyField(showOutline);
            if (showOutline.boolValue) {
                EditorGUILayout.PropertyField(outlineColor);
            }
        }

        private void DrawSeparatorSection() {
            EditorGUILayout.PropertyField(separatorSprite);
            EditorGUILayout.PropertyField(separatorColor);
            EditorGUILayout.PropertyField(separatorHeight);
        }

        private void DrawEventsSection() {
            EditorGUILayout.BeginVertical(panelStyle);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5);
            showEvents = EditorGUILayout.Foldout(showEvents, "Transition Event Hooks", true, foldoutStyle);
            EditorGUILayout.EndHorizontal();

            if (showEvents) {
                GUILayout.Space(10);
                EditorGUILayout.PropertyField(onTooltipShow);
                GUILayout.Space(5);
                EditorGUILayout.PropertyField(onTooltipHide);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawSupportSection() {
            if (EditorPrefs.GetBool("EasyTooltip_HideSupport", false)) return;

            GUILayout.Space(20);
            EditorGUILayout.BeginVertical(panelStyle);

            Rect headerRect = EditorGUILayout.GetControlRect(true, 20f);
            Rect foldoutRect = new Rect(headerRect.x + 5f, headerRect.y, headerRect.width - 5f, headerRect.height);

            showSupport = EditorGUI.Foldout(foldoutRect, showSupport, "", true);

            if (iconRate != null) {
                Color oldColor = GUI.color;
                GUI.color = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                GUI.DrawTexture(new Rect(headerRect.x + 10f, headerRect.y + 3f, 14f, 14f), iconRate, ScaleMode.ScaleToFit);
                GUI.color = oldColor;
            }

            GUIStyle labelStyle = new GUIStyle(EditorStyles.label) { font = customFont, fontStyle = FontStyle.Bold };
            GUI.Label(new Rect(headerRect.x + 32f, headerRect.y, headerRect.width - 32f, headerRect.height), "Shape the next update!", labelStyle);

            if (showSupport) {
                GUILayout.Space(10);
                string supportText = "I read every single review. Your 5-star ratings guarantee long-term Unity LTS support for your projects.\n\nLeave a review with your top feature request. The most requested features get built next!";

                GUIStyle wrapStyle = new GUIStyle(customLabelStyle) { wordWrap = true, padding = new RectOffset(15, 0, 0, 0) };
                float textHeight = wrapStyle.CalcHeight(new GUIContent(supportText), EditorGUIUtility.currentViewWidth - 60);

                Color boxColor = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f, 0.5f) : new Color(0.8f, 0.8f, 0.8f, 0.5f);
                Rect boxRect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth - 40, textHeight + 10f);

                EditorGUI.DrawRect(boxRect, boxColor);
                EditorGUI.DrawRect(new Rect(boxRect.x, boxRect.y, 3, boxRect.height), new Color(0.3f, 0.7f, 1f));

                GUI.Label(boxRect, supportText, wrapStyle);
                GUILayout.Space(15);

                float rowHeight = 32f;
                Rect buttonContainerRect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth - 40, rowHeight);

                Rect rateRect = new Rect(buttonContainerRect.x, buttonContainerRect.y, buttonContainerRect.width - 75f, buttonContainerRect.height);
                Rect hideRect = new Rect(buttonContainerRect.x + buttonContainerRect.width - 70f, buttonContainerRect.y, 70f, buttonContainerRect.height);

                Color oldBg = GUI.backgroundColor;

                GUI.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
                if (GUI.Button(rateRect, GUIContent.none)) Application.OpenURL("https://assetstore.unity.com/packages/tools/gui/easy-tooltip-329113#reviews");

                GUI.backgroundColor = new Color(0.8f, 0.3f, 0.3f);
                if (GUI.Button(hideRect, GUIContent.none)) EditorPrefs.SetBool("EasyTooltip_HideSupport", true);

                GUI.backgroundColor = oldBg;

                GUIStyle rateTextStyle = new GUIStyle(customLabelStyle) { alignment = TextAnchor.MiddleLeft, fontSize = 12, fontStyle = FontStyle.Bold };
                string btnText = "Rate & Request Feature";
                float exactTextWidth = rateTextStyle.CalcSize(new GUIContent(btnText)).x;

                float rateIconSize = 12f;
                float rateTotalW = rateIconSize + 6f + exactTextWidth;
                float rateStartX = rateRect.x + (rateRect.width - rateTotalW) / 2f;

                if (iconRate != null) {
                    Color oldColor = GUI.color;
                    GUI.color = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                    GUI.DrawTexture(new Rect(rateStartX, rateRect.y + (rowHeight - rateIconSize) / 2f, rateIconSize, rateIconSize), iconRate, ScaleMode.ScaleToFit);
                    GUI.color = oldColor;
                }

                GUI.Label(new Rect(rateStartX + rateIconSize + 6f, rateRect.y, exactTextWidth, rateRect.height), btnText, rateTextStyle);

                GUIStyle hideTextStyle = new GUIStyle(customLabelStyle) { alignment = TextAnchor.MiddleCenter, fontSize = 12, fontStyle = FontStyle.Bold };
                GUI.Label(hideRect, "Hide", hideTextStyle);

                GUILayout.Space(5);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawToggleSection(SerializedProperty prop, string label, System.Action drawContent) {
            EditorGUILayout.BeginVertical(panelStyle);
            DrawCustomSwitch(prop, label, "");

            if (prop.boolValue && !prop.hasMultipleDifferentValues) {
                GUILayout.Space(10);
                drawContent.Invoke();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawCustomSwitch(SerializedProperty prop, string label, string tooltip) {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUI.indentLevel * 15f);
            GUILayout.Label(new GUIContent(label, tooltip), customLabelStyle);
            GUILayout.FlexibleSpace();

            if (switchTrackOn == null || switchTrackOff == null) {
                EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
                prop.boolValue = EditorGUILayout.Toggle(prop.boolValue);
                EditorGUI.showMixedValue = false;
            } else {
                Rect switchRect = GUILayoutUtility.GetRect(36, 20, GUILayout.ExpandWidth(false));
                bool val = prop.boolValue;
                bool isMixed = prop.hasMultipleDifferentValues;

                Color oldColor = GUI.color;
                if (isMixed) {
                    GUI.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                }

                GUI.DrawTexture(switchRect, val && !isMixed ? switchTrackOn : switchTrackOff);
                GUI.color = oldColor;

                if (Event.current.type == EventType.MouseDown && switchRect.Contains(Event.current.mousePosition)) {
                    prop.boolValue = isMixed ? true : !val;
                    Event.current.Use();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCustomHelpBox(string message, bool isError) {
            Color bgColor = isError ? new Color(0.8f, 0.2f, 0.2f, 0.15f) : new Color(0.2f, 0.6f, 1f, 0.15f);
            Color accentColor = isError ? new Color(1f, 0.3f, 0.3f) : new Color(0.3f, 0.7f, 1f);

            GUIStyle wrapStyle = new GUIStyle(customLabelStyle) { wordWrap = true };

            Rect rect = EditorGUILayout.BeginVertical();
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.Label(message, wrapStyle);
            GUILayout.Space(5);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);
            EditorGUILayout.EndVertical();

            if (Event.current.type == EventType.Repaint) {
                EditorGUI.DrawRect(rect, bgColor);
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, 3, rect.height), accentColor);
            }
        }

        private Texture2D LoadTexture(string name) {
            string[] guids = AssetDatabase.FindAssets(name);
            foreach (string guid in guids) {
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guid));
                if (tex != null) return tex;
            }
            return null;
        }

        private Texture2D MakeTexture(int width, int height, Color col) {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i) pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private void DrawPreviewButton() {
            GUILayout.Space(5);
            bool isMultiEditing = targets.Length > 1;

            EditorGUI.BeginDisabledGroup(isMultiEditing);
            Color oldBg = GUI.backgroundColor;
            GUI.backgroundColor = isPreviewing ? new Color(0.8f, 0.3f, 0.3f) : new Color(0.2f, 0.6f, 1f);

            GUIStyle btnStyle = new GUIStyle(GUI.skin.button) { font = customFont, fontStyle = FontStyle.Bold, fontSize = 12 };
            string btnText = isPreviewing ? "Hide Preview" : (isMultiEditing ? "Preview Disabled (Multi-Edit)" : "Preview Tooltip");

            if (GUILayout.Button(btnText, btnStyle, GUILayout.Height(30)) && !isMultiEditing) {
                isPreviewing = !isPreviewing;
                TooltipTrigger trigger = (TooltipTrigger)target;
                if (isPreviewing) trigger.EditorPreviewTooltip();
                else trigger.EditorHideTooltip();
            }
            GUI.backgroundColor = oldBg;
            EditorGUI.EndDisabledGroup();
            GUILayout.Space(5);
        }
    }
}
#endif