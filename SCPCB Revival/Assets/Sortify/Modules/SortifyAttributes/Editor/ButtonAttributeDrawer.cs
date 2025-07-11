#if UNITY_EDITOR && SORTIFY_ATTRIBUTES
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Sortify
{
    [CustomPropertyDrawer(typeof(ButtonAttribute))]
    public class ButtonAttributeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ButtonAttribute buttonAttribute = (ButtonAttribute)attribute;
            EditorGUI.BeginProperty(position, label, property);

            if (property.propertyType == SerializedPropertyType.Boolean)
            {
                string buttonText = string.IsNullOrEmpty(buttonAttribute.ButtonText) ? SortifyHelper.FormatName(property.name) : buttonAttribute.ButtonText;
                if (GUI.Button(position, buttonText))
                    property.boolValue = !property.boolValue;
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use with bool or method.");
            }

            EditorGUI.EndProperty();
        }
    }

    public abstract class BaseButtonEditor : Editor
    {
        private static readonly Dictionary<Type, List<MethodInfo>> methodCache = new Dictionary<Type, List<MethodInfo>>();
        private List<MethodInfo> buttonMethods;

        private void OnEnable()
        {
            Type targetType = target.GetType();
            Type buttonAttributeType = typeof(ButtonAttribute);

            if (!methodCache.TryGetValue(targetType, out buttonMethods))
            {
                buttonMethods = GetAllMethodsWithAttribute(targetType, buttonAttributeType);
                methodCache[targetType] = buttonMethods;
            }
        }

        private List<MethodInfo> GetAllMethodsWithAttribute(Type type, Type attributeType)
        {
            List<MethodInfo> methods = new List<MethodInfo>();
            while (type != null && type != typeof(object))
            {
                var typeMethods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                                      .Where(m => m.GetCustomAttributes(attributeType, true).Any());
                methods.AddRange(typeMethods);
                type = type.BaseType;
            }
            return methods;
        }

        public override void OnInspectorGUI()
        {
            try
            {
                DrawDefaultInspector();

                if (buttonMethods != null && buttonMethods.Count > 0)
                {
                    EditorGUILayout.Space();
                    foreach (var method in buttonMethods)
                    {
                        var buttonAttribute = (ButtonAttribute)method.GetCustomAttributes(typeof(ButtonAttribute), true).First();
                        string buttonText = string.IsNullOrEmpty(buttonAttribute.ButtonText) ? SortifyHelper.FormatName(method.Name) : buttonAttribute.ButtonText;

                        if (method.GetParameters().Length > 0)
                        {
                            Debug.LogWarning($"Method '{method.Name}' has parameters and cannot be called by a button.");
                            EditorGUILayout.HelpBox($"Method '{method.Name}' has parameters and cannot be called by a button.", MessageType.Warning);
                            continue;
                        }

                        if (GUILayout.Button(buttonText))
                        {
                            foreach (var obj in targets)
                            {
                                try
                                {
                                    method.Invoke(obj, null);
                                    EditorUtility.SetDirty(obj);
                                }
                                catch (Exception e)
                                {
                                    Debug.LogError($"Error invoking method '{method.Name}': {e.Message}");
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                //Debug.LogError($"Error in BaseButtonEditor: {e.Message}");
                //EditorGUILayout.HelpBox($"Error in BaseButtonEditor: {e.Message}", MessageType.Error);
            }
        }
    }

    [CustomEditor(typeof(ScriptableObject), true)]
    [CanEditMultipleObjects]
    public class ScriptableObjectButtonEditor : BaseButtonEditor { }

    [CustomEditor(typeof(MonoBehaviour), true)]
    [CanEditMultipleObjects]
    public class MonoBehaviourButtonEditor : BaseButtonEditor { }
}
#endif
