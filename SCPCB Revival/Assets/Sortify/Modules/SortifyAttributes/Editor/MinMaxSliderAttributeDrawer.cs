#if UNITY_EDITOR && SORTIFY_ATTRIBUTES
using UnityEditor;
using UnityEngine;

namespace Sortify
{
    [CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
    public class MinMaxSliderAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            MinMaxSliderAttribute minMax = (MinMaxSliderAttribute)attribute;
            SerializedProperty minValue = property.FindPropertyRelative("x");
            SerializedProperty maxValue = property.FindPropertyRelative("y");

            float min = minValue.floatValue;
            float max = maxValue.floatValue;

            EditorGUI.BeginProperty(position, label, property);

            Rect labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
            Rect minFieldRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, 50, position.height);
            Rect sliderRect = new Rect(position.x + EditorGUIUtility.labelWidth + 55, position.y, position.width - EditorGUIUtility.labelWidth - 110, position.height);
            Rect maxFieldRect = new Rect(position.x + position.width - 50, position.y, 50, position.height);

            EditorGUI.LabelField(labelRect, label);
            
            min = Mathf.Round(min * 100f) / 100f;
            max = Mathf.Round(max * 100f) / 100f;
            min = EditorGUI.FloatField(minFieldRect, min);
            max = EditorGUI.FloatField(maxFieldRect, max);

            EditorGUI.MinMaxSlider(sliderRect, ref min, ref max, minMax.Min, minMax.Max);

            minValue.floatValue = min;
            maxValue.floatValue = max;

            EditorGUI.EndProperty();
        }
    }
}
#endif
