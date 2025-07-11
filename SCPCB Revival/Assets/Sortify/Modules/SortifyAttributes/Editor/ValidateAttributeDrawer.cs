#if UNITY_EDITOR && SORTIFY_ATTRIBUTES
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Sortify
{
    [CustomPropertyDrawer(typeof(ValidateAttribute))]
    public class ValidateAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ValidateAttribute validateInput = (ValidateAttribute)attribute;
            MonoBehaviour target = property.serializedObject.targetObject as MonoBehaviour;

            MethodInfo method = target.GetType().GetMethod(validateInput.ValidationMethod, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            bool isValid = true;
            if (method != null && method.ReturnType == typeof(bool) && method.GetParameters().Length == 0)
            {
                isValid = (bool)method.Invoke(target, null);
            }
            else
            {
                Debug.LogWarning($"Method {validateInput.ValidationMethod} does not exist or is not valid. It must return a bool and take no parameters.");
            }

            float helpBoxHeight = !isValid ? GetHelpBoxHeight(validateInput.Message) : 0f;
            if (!isValid)
            {
                Rect helpBoxRect = new Rect(position.x, position.y, position.width, helpBoxHeight);
                EditorGUI.HelpBox(helpBoxRect, validateInput.Message, MessageType.Error);
                position.y += helpBoxHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ValidateAttribute validateInput = (ValidateAttribute)attribute;
            MonoBehaviour target = property.serializedObject.targetObject as MonoBehaviour;

            MethodInfo method = target.GetType().GetMethod(validateInput.ValidationMethod, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            bool isValid = true;
            if (method != null && method.ReturnType == typeof(bool) && method.GetParameters().Length == 0)
                isValid = (bool)method.Invoke(target, null);

            float helpBoxHeight = !isValid ? GetHelpBoxHeight(validateInput.Message) : 0f;
            return base.GetPropertyHeight(property, label) + helpBoxHeight + (helpBoxHeight > 0 ? EditorGUIUtility.standardVerticalSpacing : 0);
        }

        private float GetHelpBoxHeight(string message)
        {
            if (string.IsNullOrEmpty(message))
                return 0f;

            GUIStyle style = new GUIStyle(EditorStyles.helpBox);
            GUIContent content = new GUIContent(message);

            float infoHeight = style.CalcHeight(content, EditorGUIUtility.currentViewWidth);
            float padding = EditorGUIUtility.singleLineHeight * 0.5f;

            return infoHeight + padding;
        }
    }
}
#endif
