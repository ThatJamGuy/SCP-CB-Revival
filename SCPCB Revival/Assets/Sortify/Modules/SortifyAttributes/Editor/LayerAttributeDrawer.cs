#if UNITY_EDITOR && SORTIFY_ATTRIBUTES
using UnityEditor;
using UnityEngine;

namespace Sortify
{
    [CustomPropertyDrawer(typeof(LayerAttribute))]
    public class LayerAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                EditorGUI.BeginProperty(position, label, property);
                property.intValue = EditorGUI.LayerField(position, label, property.intValue);
                EditorGUI.EndProperty();
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use LayerSelector with int.");
            }
        }
    }
}
#endif
