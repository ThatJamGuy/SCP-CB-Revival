#if UNITY_EDITOR
namespace PixeLadder.EasyTooltip.Editor
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(TooltipTrigger))]
    [CanEditMultipleObjects]
    public class TooltipTriggerEditor : Editor
    {
        private SerializedProperty title;
        private SerializedProperty content;
        private SerializedProperty icon;

        private SerializedProperty overrideStyle;
        private SerializedProperty titleColor;
        private SerializedProperty iconColor;
        private SerializedProperty backgroundColor;
        private SerializedProperty showOutline;
        private SerializedProperty outlineColor;

        private SerializedProperty overrideLayout;
        private SerializedProperty positionMode;
        private SerializedProperty anchorPosition;
        private SerializedProperty additionalOffset;

        private SerializedProperty overrideSize;
        private SerializedProperty maxWidth;

        private SerializedProperty overrideTimer;
        private SerializedProperty hoverDelay;

        private SerializedProperty onTooltipShow;
        private SerializedProperty onTooltipHide;
        private bool showEvents = false;

        private void OnEnable()
        {
            title = serializedObject.FindProperty("title");
            content = serializedObject.FindProperty("content");
            icon = serializedObject.FindProperty("icon");

            overrideStyle = serializedObject.FindProperty("overrideStyle");
            titleColor = serializedObject.FindProperty("titleColor");
            iconColor = serializedObject.FindProperty("iconColor");
            backgroundColor = serializedObject.FindProperty("backgroundColor");
            showOutline = serializedObject.FindProperty("showOutline");
            outlineColor = serializedObject.FindProperty("outlineColor");

            overrideLayout = serializedObject.FindProperty("overrideLayout");
            positionMode = serializedObject.FindProperty("positionMode");
            anchorPosition = serializedObject.FindProperty("anchorPosition");
            additionalOffset = serializedObject.FindProperty("additionalOffset");

            overrideSize = serializedObject.FindProperty("overrideSize");
            maxWidth = serializedObject.FindProperty("maxWidth");

            overrideTimer = serializedObject.FindProperty("overrideTimer");
            hoverDelay = serializedObject.FindProperty("hoverDelay");

            onTooltipShow = serializedObject.FindProperty("onTooltipShow");
            onTooltipHide = serializedObject.FindProperty("onTooltipHide");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Tooltip Configuration", EditorStyles.boldLabel);

            // --- Content ---
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(title);
            EditorGUILayout.PropertyField(content);
            EditorGUILayout.PropertyField(icon);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);

            // --- Layout ---
            DrawToggleSection(overrideLayout, "Layout & Positioning", () =>
            {
                EditorGUILayout.PropertyField(positionMode);
                if (positionMode.enumValueIndex == (int)TooltipPositionMode.Fixed)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(anchorPosition);
                    EditorGUI.indentLevel--;
                }
                else
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(additionalOffset, new GUIContent("Cursor Offset"));
                    EditorGUI.indentLevel--;
                }
            });

            // --- Size ---
            DrawToggleSection(overrideSize, "Size Constraints", () =>
            {
                EditorGUILayout.PropertyField(maxWidth);
            });

            // --- Style ---
            DrawToggleSection(overrideStyle, "Visual Style", () =>
            {
                EditorGUILayout.PropertyField(titleColor);
                EditorGUILayout.PropertyField(iconColor);
                EditorGUILayout.Space(5);
                EditorGUILayout.PropertyField(backgroundColor);
                EditorGUILayout.PropertyField(showOutline);
                if (showOutline.boolValue)
                {
                    EditorGUILayout.PropertyField(outlineColor);
                }
            });

            // --- Timer ---
            DrawToggleSection(overrideTimer, "Timing", () =>
            {
                EditorGUILayout.PropertyField(hoverDelay);
            });

            // --- Events ---
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            showEvents = EditorGUILayout.Foldout(showEvents, "Events", true, EditorStyles.foldoutHeader);
            if (showEvents)
            {
                EditorGUILayout.PropertyField(onTooltipShow);
                EditorGUILayout.PropertyField(onTooltipHide);
            }
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawToggleSection(SerializedProperty toggleProp, string header, System.Action drawContent)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(header, EditorStyles.boldLabel);
            bool isOverriding = toggleProp.boolValue;
            bool newOverride = EditorGUILayout.ToggleLeft("Override Defaults", isOverriding, GUILayout.Width(130));
            if (newOverride != isOverriding) toggleProp.boolValue = newOverride;
            EditorGUILayout.EndHorizontal();

            if (toggleProp.boolValue)
            {
                EditorGUILayout.Space(2);
                drawContent.Invoke();
                EditorGUILayout.Space(2);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }
    }
}
#endif