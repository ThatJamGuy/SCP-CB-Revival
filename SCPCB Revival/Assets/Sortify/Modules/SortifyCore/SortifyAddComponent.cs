#if UNITY_EDITOR && SORTIFY
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Sortify
{
    public class SortifyAddComponent : PopupWindowContent
    {
        private const int _clearButtonSize = 22;
        private const int _padding = 4;

        private string _searchString = "";
        private List<System.Type> _filteredComponents;
        private List<System.Type> _allComponents;
        private Vector2 _scrollPosition;
        private System.Action<System.Type> OnComponentAdded;

        public SortifyAddComponent(Action<Type> onComponentAdded)
        {
            _allComponents = GetValidComponentTypes();
            _filteredComponents = new List<System.Type>(_allComponents);
            OnComponentAdded = onComponentAdded;
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(300, 400);
        }

        public override void OnGUI(Rect rect)
        {
            DrawSearchSection();
            EditorGUILayout.Space(2);
            DrawComponentsList();
        }

        private void DrawSearchSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Add New Component:", EditorStyles.boldLabel, GUILayout.Height(24));
            EditorGUILayout.BeginHorizontal();
            
            _searchString = EditorGUILayout.TextField(_searchString);
            _filteredComponents = _allComponents
               .Where(c => string.IsNullOrEmpty(_searchString) || c.Name.ToLower().Contains(_searchString.ToLower()))
               .ToList();

            GUIStyle style = new GUIStyle()
            {
                fixedWidth = _clearButtonSize,
                fixedHeight = _clearButtonSize,
                margin = new RectOffset(_padding, _padding, _padding, _padding)
            };

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_Refresh"), style))
            {
                _searchString = "";
                GUI.FocusControl(null);
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawComponentsList()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            for (int i = 0; i < _filteredComponents.Count; i++)
            {
                var component = _filteredComponents[i];
                Texture icon = AssetPreview.GetMiniTypeThumbnail(component) ?? EditorGUIUtility.IconContent("cs Script Icon").image;

                GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleLeft,
                    imagePosition = ImagePosition.ImageLeft,
                    fixedWidth = 264f
                };

                if (GUILayout.Button(new GUIContent(component.Name, icon), buttonStyle, GUILayout.Height(24), GUILayout.ExpandWidth(true)))
                {
                    OnComponentAdded?.Invoke(component);
                    editorWindow.Close();
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private List<System.Type> GetValidComponentTypes()
        {
            var allTypes = new List<System.Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => (typeof(Component).IsAssignableFrom(t) || typeof(MonoBehaviour).IsAssignableFrom(t))
                                    && !t.IsAbstract
                                    && t.IsClass);
                    allTypes.AddRange(types);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Debug.LogWarning($"Could not load types from assembly {assembly.FullName}: {ex}");
                }
            }

            return allTypes.OrderBy(t => t.Name).ToList();
        }
    }
}
#endif
