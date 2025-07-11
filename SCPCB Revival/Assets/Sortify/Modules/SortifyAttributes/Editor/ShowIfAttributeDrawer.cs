#if UNITY_EDITOR && SORTIFY_ATTRIBUTES
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Sortify
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIf = attribute as ShowIfAttribute;
            var shouldShow = ShouldShowProperty(property, showIf);
            if (shouldShow)
                EditorGUI.PropertyField(position, property, label);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIf = attribute as ShowIfAttribute;
            var shouldShow = ShouldShowProperty(property, showIf);
            if (shouldShow)
                return EditorGUI.GetPropertyHeight(property, label);
         
            return 0;
        }

        private bool ShouldShowProperty(SerializedProperty property, ShowIfAttribute showIf)
        {
            var targetObject = property.serializedObject.targetObject;
            var conditionField = targetObject.GetType().GetField(showIf.ConditionName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (conditionField != null)
            {
                object conditionValue = conditionField.GetValue(targetObject);
                bool conditionMet = CompareCondition(conditionValue, showIf.CompareValue);
                return showIf.Inverted ? !conditionMet : conditionMet;
            }

            return true;
        }

        private bool CompareCondition(object conditionValue, object compareValue)
        {
            if (compareValue == null)
                return conditionValue != null && (bool)conditionValue;

            return conditionValue != null && conditionValue.Equals(compareValue);
        }
    }
}
#endif
