#if UNITY_EDITOR
namespace PixeLadder.EasyTooltip.Editor
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(TooltipHintTrigger))]
    [CanEditMultipleObjects]
    public class TooltipHintTriggerEditor : Editor
    {
        private SerializedProperty hintPrefab;
        private SerializedProperty hintText;
        private SerializedProperty mouseOffset;
        private SerializedProperty fadeDuration;

        private Texture2D bannerImage;
        private Texture2D iconRate;
        private Texture2D iconSupport;
        private Font customFont;
        private Texture2D panelBackground;

        private GUIStyle panelStyle;
        private GUIStyle headerStyle;
        private GUIStyle customLabelStyle;
        private GUIStyle textAreaStyle;

        private bool showSupport
        {
            get => SessionState.GetBool($"EasyTooltip_HintSup_{target.GetHashCode()}", false);
            set => SessionState.SetBool($"EasyTooltip_HintSup_{target.GetHashCode()}", value);
        }

        private bool isPreviewing
        {
            get => SessionState.GetBool($"EasyTooltip_HintPreview_{target.GetHashCode()}", false);
            set => SessionState.SetBool($"EasyTooltip_HintPreview_{target.GetHashCode()}", value);
        }

        private Vector3 lastPreviewPosition;
        private Vector2 lastPreviewSize;

        public override bool RequiresConstantRepaint() => Application.isPlaying || isPreviewing;

        private void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedo;
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            hintPrefab = serializedObject.FindProperty("hintPrefab");
            hintText = serializedObject.FindProperty("hintText");
            mouseOffset = serializedObject.FindProperty("mouseOffset");
            fadeDuration = serializedObject.FindProperty("fadeDuration");

            bannerImage = LoadTexture("EasyTooltipBanner_TooltipHintTrigger");
            iconRate = LoadTexture("EasyTooltip_IconRate");
            iconSupport = LoadTexture("EasyTooltip_IconSupport");

            string[] fontGuids = AssetDatabase.FindAssets("EasyTooltipFont t:Font");
            if (fontGuids.Length > 0)
                customFont = AssetDatabase.LoadAssetAtPath<Font>(AssetDatabase.GUIDToAssetPath(fontGuids[0]));

            float bgTone = EditorGUIUtility.isProSkin ? 0.15f : 0.85f;
            panelBackground = MakeTexture(1, 1, new Color(bgTone, bgTone, bgTone, 0.5f));
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

            if (isPreviewing)
            {
                TooltipHintTrigger trigger = (TooltipHintTrigger)target;
                if (trigger != null && Selection.activeGameObject != trigger.gameObject)
                {
                    trigger.EditorHideHint();
                    isPreviewing = false;
                }
            }

            if (panelBackground != null) DestroyImmediate(panelBackground);
        }

        private void OnUndoRedo()
        {
            if (isPreviewing)
            {
                TooltipHintTrigger trigger = (TooltipHintTrigger)target;
                if (trigger != null) trigger.EditorPreviewHint();
                Repaint();
            }
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode && isPreviewing)
            {
                TooltipHintTrigger trigger = (TooltipHintTrigger)target;
                if (trigger != null) trigger.EditorHideHint();
                isPreviewing = false;
            }
        }

        private void OnEditorUpdate()
        {
            if (isPreviewing)
            {
                TooltipHintTrigger trigger = (TooltipHintTrigger)target;
                if (trigger != null)
                {
                    bool changed = false;

                    if (trigger.transform.hasChanged)
                    {
                        trigger.transform.hasChanged = false;
                        changed = true;
                    }

                    RectTransform rect = trigger.GetComponent<RectTransform>();
                    if (rect != null && rect.rect.size != lastPreviewSize)
                    {
                        lastPreviewSize = rect.rect.size;
                        changed = true;
                    }

                    if (changed) trigger.EditorPreviewHint();
                }
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            serializedObject.Update();
            InitializeStyles();

            DrawFullWidthHeader();
            DrawPreviewButton();

            DrawSectionHeader("C O N F I G U R A T I O N");
            EditorGUILayout.BeginVertical(panelStyle);
            EditorGUILayout.PropertyField(hintPrefab);
            GUILayout.Space(5);
            EditorGUILayout.LabelField("Hint Text", EditorStyles.boldLabel);
            hintText.stringValue = EditorGUILayout.TextArea(hintText.stringValue, textAreaStyle, GUILayout.Height(40));
            EditorGUILayout.EndVertical();

            DrawSectionHeader("P O S I T I O N   &   T I M I N G");
            EditorGUILayout.BeginVertical(panelStyle);
            EditorGUILayout.PropertyField(mouseOffset);
            EditorGUILayout.PropertyField(fadeDuration);
            EditorGUILayout.EndVertical();

            DrawSupportSection();

            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck() && isPreviewing)
            {
                TooltipHintTrigger trigger = (TooltipHintTrigger)target;
                trigger.EditorPreviewHint();
            }
        }

        private void InitializeStyles()
        {
            if (panelStyle == null)
            {
                panelStyle = new GUIStyle(GUI.skin.box)
                {
                    normal = { background = panelBackground },
                    margin = new RectOffset(10, 10, 5, 10),
                    padding = new RectOffset(15, 15, 15, 15)
                };

                headerStyle = new GUIStyle(EditorStyles.label)
                {
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 11,
                    font = customFont
                };

                customLabelStyle = new GUIStyle(EditorStyles.label) { font = customFont };

                textAreaStyle = new GUIStyle(EditorStyles.textArea)
                {
                    wordWrap = true,
                    padding = new RectOffset(5, 5, 5, 5)
                };
            }
        }

        private void DrawFullWidthHeader()
        {
            Rect fullRect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 64f);
            fullRect.x = 0;
            fullRect.width = EditorGUIUtility.currentViewWidth;

            if (bannerImage != null)
            {
                GUI.DrawTexture(fullRect, bannerImage, ScaleMode.ScaleAndCrop);
            }
            else
            {
                EditorGUI.DrawRect(fullRect, EditorGUIUtility.isProSkin ? new Color(0.1f, 0.1f, 0.1f) : new Color(0.9f, 0.9f, 0.9f));
                GUI.Label(fullRect, "H I N T   T R I G G E R", headerStyle);
            }
            GUILayout.Space(10);
        }

        private void DrawSectionHeader(string titleText)
        {
            GUILayout.Space(10);
            GUILayout.Label(titleText, headerStyle);

            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            rect.xMin += 10;
            rect.xMax -= 10;
            EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? new Color(0.4f, 0.4f, 0.4f, 0.5f) : new Color(0.6f, 0.6f, 0.6f, 0.5f));
            GUILayout.Space(5);
        }

        private void DrawPreviewButton()
        {
            GUILayout.Space(5);
            Color oldBg = GUI.backgroundColor;
            GUI.backgroundColor = isPreviewing ? new Color(0.8f, 0.3f, 0.3f) : new Color(0.2f, 0.6f, 1f);

            GUIStyle btnStyle = new GUIStyle(GUI.skin.button) { font = customFont, fontStyle = FontStyle.Bold, fontSize = 12 };
            string btnText = isPreviewing ? "Hide Preview" : "Preview Hint";

            if (GUILayout.Button(btnText, btnStyle, GUILayout.Height(30)))
            {
                isPreviewing = !isPreviewing;
                TooltipHintTrigger trigger = (TooltipHintTrigger)target;
                if (isPreviewing) trigger.EditorPreviewHint();
                else trigger.EditorHideHint();
            }
            GUI.backgroundColor = oldBg;
            GUILayout.Space(5);
        }

        private void DrawSupportSection()
        {
            if (EditorPrefs.GetBool("EasyTooltip_HideSupport", false)) return;

            GUILayout.Space(20);
            EditorGUILayout.BeginVertical(panelStyle);

            Rect headerRect = EditorGUILayout.GetControlRect(true, 20f);
            Rect foldoutRect = new Rect(headerRect.x + 5f, headerRect.y, headerRect.width - 5f, headerRect.height);

            showSupport = EditorGUI.Foldout(foldoutRect, showSupport, "", true);

            if (iconRate != null)
            {
                Color oldColor = GUI.color;
                GUI.color = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                GUI.DrawTexture(new Rect(headerRect.x + 10f, headerRect.y + 3f, 14f, 14f), iconRate, ScaleMode.ScaleToFit);
                GUI.color = oldColor;
            }

            GUIStyle labelStyle = new GUIStyle(EditorStyles.label) { font = customFont, fontStyle = FontStyle.Bold };
            GUI.Label(new Rect(headerRect.x + 32f, headerRect.y, headerRect.width - 32f, headerRect.height), "Shape the next update!", labelStyle);

            if (showSupport)
            {
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

                if (iconRate != null)
                {
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

        private Texture2D LoadTexture(string name)
        {
            string[] guids = AssetDatabase.FindAssets(name);
            foreach (string guid in guids)
            {
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guid));
                if (tex != null) return tex;
            }
            return null;
        }

        private Texture2D MakeTexture(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i) pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}
#endif