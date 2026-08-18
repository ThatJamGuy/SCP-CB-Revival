#if UNITY_EDITOR
namespace PixeLadder.EasyTooltip.Editor
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(TooltipManager))]
    public class TooltipManagerEditor : Editor
    {
        private SerializedProperty tooltipPrefab;
        private SerializedProperty defaultMaxWidth;
        private SerializedProperty defaultFadeDuration;
        private SerializedProperty defaultHoverDelay;
        private SerializedProperty defaultMouseOffset;
        private SerializedProperty defaultFixedGap;
        private SerializedProperty smartFlipping;
        private SerializedProperty defaultClamping;
        private SerializedProperty defaultContinuousTracking;

        private SerializedProperty defaultTitleColor;
        private SerializedProperty defaultIconColor;
        private SerializedProperty defaultPanelColor;
        private SerializedProperty defaultHeaderColor;
        private SerializedProperty defaultBodyColor;
        private SerializedProperty defaultSeparatorSprite;
        private SerializedProperty defaultSeparatorColor;
        private SerializedProperty defaultSeparatorHeight;
        private SerializedProperty defaultShowOutline;
        private SerializedProperty defaultOutlineColor;

        private Texture2D bannerImage;
        private Texture2D iconRate;
        private Texture2D iconSupport;
        private Texture2D switchTrackOn;
        private Texture2D switchTrackOff;
        private Font customFont;
        private Texture2D panelBackground;
        private Texture2D cmdBackground;

        private GUIStyle panelStyle;
        private GUIStyle headerStyle;
        private GUIStyle customLabelStyle;
        private GUIStyle foldoutStyle;
        private GUIStyle cmdStyle;
        private GUIStyle richTextStyle;

        private bool showSupport = false;

        public override bool RequiresConstantRepaint() => true;

        private void OnEnable()
        {
            tooltipPrefab = serializedObject.FindProperty("tooltipPrefab");
            defaultMaxWidth = serializedObject.FindProperty("defaultMaxWidth");
            defaultFadeDuration = serializedObject.FindProperty("defaultFadeDuration");
            defaultHoverDelay = serializedObject.FindProperty("defaultHoverDelay");
            defaultMouseOffset = serializedObject.FindProperty("defaultMouseOffset");
            defaultFixedGap = serializedObject.FindProperty("defaultFixedGap");
            smartFlipping = serializedObject.FindProperty("smartFlipping");
            defaultClamping = serializedObject.FindProperty("defaultClamping");
            defaultContinuousTracking = serializedObject.FindProperty("defaultContinuousTracking");

            defaultTitleColor = serializedObject.FindProperty("defaultTitleColor");
            defaultIconColor = serializedObject.FindProperty("defaultIconColor");
            defaultPanelColor = serializedObject.FindProperty("defaultPanelColor");
            defaultHeaderColor = serializedObject.FindProperty("defaultHeaderColor");
            defaultBodyColor = serializedObject.FindProperty("defaultBodyColor");
            defaultSeparatorSprite = serializedObject.FindProperty("defaultSeparatorSprite");
            defaultSeparatorColor = serializedObject.FindProperty("defaultSeparatorColor");
            defaultSeparatorHeight = serializedObject.FindProperty("defaultSeparatorHeight");
            defaultShowOutline = serializedObject.FindProperty("defaultShowOutline");
            defaultOutlineColor = serializedObject.FindProperty("defaultOutlineColor");

            bannerImage = LoadTexture("EasyTooltipBanner_TooltipManager");
            iconRate = LoadTexture("EasyTooltip_IconRate");
            iconSupport = LoadTexture("EasyTooltip_IconSupport");
            switchTrackOn = LoadTexture("EasyTooltip_SwitchOn");
            switchTrackOff = LoadTexture("EasyTooltip_SwitchOff");

            string[] fontGuids = AssetDatabase.FindAssets("EasyTooltipFont t:Font");
            if (fontGuids.Length > 0)
                customFont = AssetDatabase.LoadAssetAtPath<Font>(AssetDatabase.GUIDToAssetPath(fontGuids[0]));

            cmdBackground = MakeTexture(1, 1, new Color(0.05f, 0.05f, 0.05f, 1f));
        }

        private void OnDisable()
        {
            if (panelBackground != null) DestroyImmediate(panelBackground);
            if (cmdBackground != null) DestroyImmediate(cmdBackground);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            InitializeStyles();

            DrawFullWidthHeader();

            DrawLifecycleTimeline();

            DrawSectionHeader("C O R E   C O N F I G U R A T I O N");
            EditorGUILayout.BeginVertical(panelStyle);
            EditorGUILayout.PropertyField(tooltipPrefab);
            EditorGUILayout.EndVertical();

            DrawSectionHeader("S I Z E   &   A N I M A T I O N");
            EditorGUILayout.BeginVertical(panelStyle);
            EditorGUILayout.PropertyField(defaultMaxWidth);
            EditorGUILayout.PropertyField(defaultHoverDelay);
            EditorGUILayout.PropertyField(defaultFadeDuration);
            EditorGUILayout.EndVertical();

            DrawSectionHeader("P O S I T I O N I N G");
            EditorGUILayout.BeginVertical(panelStyle);
            EditorGUILayout.PropertyField(defaultMouseOffset);
            EditorGUILayout.PropertyField(defaultFixedGap);
            GUILayout.Space(5);
            DrawCustomSwitch(defaultContinuousTracking, "Continuous Tracking", "If true, tooltips in Follow mode will continuously track the cursor movement while hovered.");
            DrawCustomSwitch(defaultClamping, "Screen Clamping", "If true, tooltips will be clamped to stay within the screen boundaries.");
            DrawCustomSwitch(smartFlipping, "Smart Flipping", "If true, fixed tooltips will automatically flip to the opposite side if they go off-screen.");
            EditorGUILayout.EndVertical();

            DrawSectionHeader("S T Y L E   D E F A U L T S");
            EditorGUILayout.BeginVertical(panelStyle);
            EditorGUILayout.PropertyField(defaultTitleColor);
            EditorGUILayout.PropertyField(defaultIconColor);
            GUILayout.Space(5);
            EditorGUILayout.PropertyField(defaultPanelColor);
            EditorGUILayout.PropertyField(defaultHeaderColor);
            EditorGUILayout.PropertyField(defaultBodyColor);
            GUILayout.Space(5);
            EditorGUILayout.PropertyField(defaultSeparatorSprite);
            EditorGUILayout.PropertyField(defaultSeparatorColor);
            GUILayout.Space(5);
            DrawCustomSwitch(defaultShowOutline, "Show Outline", "Default toggle for the outline border visibility.");
            if (defaultShowOutline.boolValue)
            {
                EditorGUILayout.PropertyField(defaultOutlineColor);
            }
            EditorGUILayout.EndVertical();

            DrawSupportSection();

            serializedObject.ApplyModifiedProperties();
        }

        private void InitializeStyles()
        {
            if (panelStyle == null || richTextStyle == null)
            {
                panelStyle = new GUIStyle(GUI.skin.box)
                {
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

                foldoutStyle = new GUIStyle(EditorStyles.foldout)
                {
                    font = customFont,
                    fontStyle = FontStyle.Bold,
                    padding = new RectOffset(20, 0, 0, 0)
                };

                cmdStyle = new GUIStyle(EditorStyles.label)
                {
                    normal = { background = cmdBackground, textColor = new Color(0.2f, 1f, 0.2f) },
                    fontSize = 11,
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(10, 10, 8, 8)
                };

                richTextStyle = new GUIStyle(EditorStyles.label)
                {
                    font = customFont,
                    richText = true,
                    alignment = TextAnchor.MiddleCenter
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
                GUI.Label(fullRect, "E A S Y   T O O L T I P", headerStyle);
            }
            GUILayout.Space(10);
        }

        private void DrawLifecycleTimeline()
        {
            TooltipManager manager = (TooltipManager)target;
            EditorGUILayout.BeginVertical(panelStyle);

            GUILayout.Label("T O O L T I P   L I F E C Y C L E", headerStyle);
            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            TooltipState currentState = manager.CurrentState;
            bool isPlaying = Application.isPlaying;

            DrawTimelineNode("IDLE", TooltipState.Idle, currentState, isPlaying);
            DrawTimelineNode("WAIT", TooltipState.Delay, currentState, isPlaying);
            DrawTimelineNode("IN", TooltipState.FadingIn, currentState, isPlaying);
            DrawTimelineNode("SHOW", TooltipState.Visible, currentState, isPlaying);
            DrawTimelineNode("OUT", TooltipState.FadingOut, currentState, isPlaying, true);

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawTimelineNode(string label, TooltipState nodeState, TooltipState currentState, bool isPlaying, bool isLast = false)
        {
            bool isActive = isPlaying && currentState == nodeState;
            string colorHex = isActive ? "#FFFFFF" : (EditorGUIUtility.isProSkin ? "#555555" : "#777777");
            string arrowHex = EditorGUIUtility.isProSkin ? "#444444" : "#888888";

            if (isLast)
                GUILayout.Label($"<color={colorHex}><b>{label}</b></color>", richTextStyle);
            else
                GUILayout.Label($"<color={colorHex}><b>{label}</b></color> <color={arrowHex}>➔</color> ", richTextStyle);
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

        private void DrawCustomSwitch(SerializedProperty prop, string label, string tooltip)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent(label, tooltip), customLabelStyle);
            GUILayout.FlexibleSpace();

            if (switchTrackOn == null || switchTrackOff == null)
            {
                prop.boolValue = EditorGUILayout.Toggle(prop.boolValue);
            }
            else
            {
                Rect switchRect = GUILayoutUtility.GetRect(36, 20, GUILayout.ExpandWidth(false));
                bool val = prop.boolValue;

                GUI.DrawTexture(switchRect, val ? switchTrackOn : switchTrackOff);

                if (Event.current.type == EventType.MouseDown && switchRect.Contains(Event.current.mousePosition))
                {
                    prop.boolValue = !val;
                    Event.current.Use();
                }
            }
            EditorGUILayout.EndHorizontal();
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