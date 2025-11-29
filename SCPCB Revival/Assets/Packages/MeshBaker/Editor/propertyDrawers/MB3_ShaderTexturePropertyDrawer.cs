using UnityEditor;
using UnityEngine;
using DigitalOpus.MB.Core;

namespace DigitalOpus.MB.MBEditor
{
    [CustomPropertyDrawer(typeof(ShaderTextureProperty))]
    public class MB3_ShaderTexturePropertyDrawer : PropertyDrawer
    {
        public static GUIContent gc_isNormalMap = new GUIContent("isNormalMap", "This atlas texture will be marked as a normal map"),
                                 gc_isGammaCorrected = new GUIContent("isGammaCorrected", "This atlas should be gamma corrected (sRGB checked). If not checked then the atlas will be a linear texture");
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);

            
            Rect contentPosition = EditorGUI.PrefixLabel(position, label);

            float w = contentPosition.width;

            contentPosition.width = w * 0.4f;
            EditorGUI.indentLevel = 0;

            EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative("name"), GUIContent.none);
            
            contentPosition.x += contentPosition.width;
            contentPosition.width = w * 0.3f;
            EditorGUIUtility.labelWidth = 50f;
            EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative("isNormalMap"), gc_isNormalMap);

            contentPosition.x += contentPosition.width;
            EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative("isGammaCorrected"), gc_isGammaCorrected);
            


            //EditorGUILayout.PropertyField(property.FindPropertyRelative("name"));
            //EditorGUILayout.PropertyField(property.FindPropertyRelative("isNormalMap"), gc_isNormalMap);
            //EditorGUILayout.PropertyField(property.FindPropertyRelative("isGammaCorrected"), gc_isGammaCorrected);
            EditorGUI.EndProperty();

        }
    }
}
