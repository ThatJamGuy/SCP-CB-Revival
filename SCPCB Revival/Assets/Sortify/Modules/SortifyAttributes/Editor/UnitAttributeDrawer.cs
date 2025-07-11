#if UNITY_EDITOR && SORTIFY_ATTRIBUTES
using UnityEditor;
using UnityEngine;

namespace Sortify
{
    [CustomPropertyDrawer(typeof(UnitAttribute))]
    public class UnitAttributeDrawer : PropertyDrawer
    {
        private const int _padding = 5;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            UnitAttribute unitAttribute = (UnitAttribute)attribute;

            GUIStyle unitStyle = new GUIStyle(EditorStyles.label);
            unitStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);

            GUIStyle valueStyle = new GUIStyle(EditorStyles.label);
            string valueText = property.floatValue.ToString();
            Vector2 valueTextSize = valueStyle.CalcSize(new GUIContent(valueText));
            float valueTextWidth = valueTextSize.x;
            float labelWidth = EditorGUIUtility.labelWidth;

            Rect fieldRect = new Rect(position.x, position.y, position.width, position.height);
            Rect unitRect = new Rect(position.x + labelWidth + valueTextWidth + _padding, position.y, 50, position.height);

            EditorGUI.PropertyField(fieldRect, property, label);
            EditorGUI.LabelField(unitRect, unitAttribute.Unit, unitStyle);
        }
    }
}
#endif
