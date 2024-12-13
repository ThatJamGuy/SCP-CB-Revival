#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Verpha.HierarchyDesigner
{
    [CustomEditor(typeof(HierarchyDesignerFolder))]
    internal class HierarchyDesigner_Inspector_Folder : Editor
    {
        #region Properties
        #region GUI
        private const int defaultGUISpace = 2;
        private const float labelFieldWidth = 125;
        private const int minButtonWidth = 55;
        private const int maxButtonWidth = 100;
        private const string toggle = "Toggle";
        private const string select = "Select";
        private const string viewInScene = "View in Scene";
        private const string delete = "Delete";
        private const int maxAllowedChildren = 500;
        private float maxLabelWidth = 100f;
        #endregion

        #region Serialized
        private SerializedProperty flattenFolderProp;
        private SerializedProperty flattenEventProp;
        private SerializedProperty onFlattenEventProp;
        private SerializedProperty onFolderDestroyProp;
        #endregion

        #region Cache
        private bool doOnce = false;
        private bool showChildren = true;
        private bool childrenCached = false;
        private HierarchyDesignerFolder folder;
        private readonly List<Transform> cachedChildren = new();
        private int totalChildCount = 0;
        private List<GUILayoutOption[]> cachedGUIOptions;
        #endregion
        #endregion

        #region Initialization
        private void OnEnable()
        {
            folder = (HierarchyDesignerFolder)target;

            flattenFolderProp = serializedObject.FindProperty("flattenFolder");
            flattenEventProp = serializedObject.FindProperty("flattenEvent");
            onFlattenEventProp = serializedObject.FindProperty("OnFlattenEvent");
            onFolderDestroyProp = serializedObject.FindProperty("OnFolderDestroy");

            CacheChildren();
            ProcessChildren();
        }
        #endregion

        #region Main
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.InspectorFolderPanelStyle);

            #region Runtime
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            float originalFieldWidth = EditorGUIUtility.fieldWidth;
            EditorGUIUtility.labelWidth = 90;
            EditorGUIUtility.fieldWidth = maxButtonWidth;

            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.InspectorFolderInnerPanelStyle);
            EditorGUILayout.LabelField("▷ Hierarchy Designer's Folder", HierarchyDesigner_Shared_GUI.TabLabelStyle);
            EditorGUILayout.Space(defaultGUISpace);

            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(defaultGUISpace);

            HierarchyDesigner_Shared_GUI.DrawPropertyField("Flatten Folder", labelFieldWidth, flattenFolderProp, true, true, "If 'Flatten Folder' is set to true, the folder will unparent all child GameObjects on the 'Flatten Event.' Once the operation is complete, the folder will destroy itself.");
            if (flattenFolderProp.boolValue)
            {
                HierarchyDesigner_Shared_GUI.DrawPropertyField("Flatten Event", labelFieldWidth, flattenEventProp, HierarchyDesignerFolder.FlattenEvent.Start, true, "The event on which the 'Flatten Folder' action will occur.\n\nIf set to Awake, the folder will be flattened on the Awake event.\n\nIf set to Start, the folder will be flattened on the Start event.");
                EditorGUILayout.Space(6);

                EditorGUILayout.LabelField("Events", HierarchyDesigner_Shared_GUI.FieldsCategoryLabelStyle);
                EditorGUILayout.Space(defaultGUISpace);

                EditorGUILayout.PropertyField(onFlattenEventProp);
                EditorGUILayout.Space(defaultGUISpace);

                EditorGUILayout.PropertyField(onFolderDestroyProp);
            }
            EditorGUILayout.EndVertical();
            EditorGUIUtility.labelWidth = originalLabelWidth;
            EditorGUIUtility.fieldWidth = originalFieldWidth;

            EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
            #endregion

            #region Editor
            if (HierarchyDesigner_Configurable_AdvancedSettings.IncludeEditorUtilitiesForHierarchyDesignerRuntimeFolder)
            {
                if (totalChildCount > maxAllowedChildren)
                {
                    EditorGUILayout.HelpBox($"This folder contains {totalChildCount} gameObject children, which exceeds the maximum allowed limit of {maxAllowedChildren} children. The editor utility is disabled for this folder.", MessageType.Warning);
                    EditorGUILayout.EndVertical();
                    return;
                }

                EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.InspectorFolderInnerPanelStyle);
                if (!childrenCached)
                {
                    CacheChildren();
                }
                if (!doOnce)
                {
                    maxLabelWidth = HierarchyDesigner_Shared_GUI.CalculateMaxLabelWidth(folder.transform);
                    for (int i = 0; i < cachedGUIOptions.Count; i++)
                    {
                        cachedGUIOptions[i][0] = GUILayout.Width(maxLabelWidth);
                    }
                    doOnce = true;
                }

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("(Editor-Only)", HierarchyDesigner_Shared_GUI.FieldsCategoryLabelStyle);
                EditorGUILayout.Space(defaultGUISpace);

                EditorGUILayout.Space(defaultGUISpace);
                EditorGUILayout.LabelField($"This folder contains: '{totalChildCount}' gameObject children.", HierarchyDesigner_Shared_GUI.RegularLabelStyle);
                EditorGUILayout.Space(defaultGUISpace);

                if (GUILayout.Button("Refresh Children List", GUILayout.Height(20)))
                {
                    childrenCached = false;
                    doOnce = false;
                    CacheChildren();
                    ProcessChildren();
                    maxLabelWidth = HierarchyDesigner_Shared_GUI.CalculateMaxLabelWidth(folder.transform);
                }

                EditorGUILayout.Space(defaultGUISpace);

                if (totalChildCount > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(11);
                    showChildren = EditorGUILayout.Foldout(showChildren, "GameObject's Children List");
                    EditorGUILayout.EndHorizontal();
                }
                if (showChildren)
                {
                    DisplayCachedChildren();
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
            }
            #endregion

            EditorGUILayout.EndVertical();
        }

        private void OnDisable()
        {
            cachedChildren.Clear();
            cachedGUIOptions.Clear();
            childrenCached = false;
        }
        #endregion

        #region Editor Operations
        private void CacheChildren()
        {
            cachedChildren.Clear();
            GetChildTransforms(folder.transform, cachedChildren);
            totalChildCount = cachedChildren.Count;
            childrenCached = true;
        }

        private void GetChildTransforms(Transform parent, List<Transform> children)
        {
            foreach (Transform child in parent)
            {
                children.Add(child);
                GetChildTransforms(child, children);
            }
        }

        private void ProcessChildren()
        {
            cachedGUIOptions = new List<GUILayoutOption[]>(totalChildCount);

            for (int i = 0; i < totalChildCount; i++)
            {
                GUILayoutOption[] options = new GUILayoutOption[4];
                options[0] = GUILayout.Width(maxLabelWidth);
                options[1] = GUILayout.MinWidth(minButtonWidth);
                options[2] = GUILayout.ExpandWidth(true);
                options[3] = GUILayout.MinWidth(maxButtonWidth);
                cachedGUIOptions.Add(options);
            }
        }

        private void DisplayCachedChildren()
        {
            for (int i = 0; i < cachedChildren.Count; i++)
            {
                Transform child = cachedChildren[i];
                GUILayoutOption[] options = cachedGUIOptions[i];

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(child.name, (child.gameObject.activeSelf ? HierarchyDesigner_Shared_GUI.InspectorFolderActiveLabelStyle : HierarchyDesigner_Shared_GUI.InspectorFolderInactiveLabelStyle), options[0]);

                if (GUILayout.Button(toggle, options[1], options[2]))
                {
                    Undo.RecordObject(child.gameObject, "Toggle Active State");
                    child.gameObject.SetActive(!child.gameObject.activeSelf);
                }
                if (GUILayout.Button(select, options[1], options[2]))
                {
                    EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
                    Selection.activeGameObject = child.gameObject;
                }
                if (GUILayout.Button(viewInScene, options[3], options[2]))
                {
                    GameObject originalSelection = Selection.activeGameObject;
                    Selection.activeGameObject = child.gameObject;
                    SceneView.FrameLastActiveSceneView();
                    Selection.activeGameObject = originalSelection;
                }
                if (GUILayout.Button(delete, options[1], options[2]))
                {
                    Undo.DestroyObjectImmediate(child.gameObject);
                    cachedChildren.Remove(child);
                    cachedGUIOptions.RemoveAt(i);
                    GUIUtility.ExitGUI();
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        #endregion
    }
}
#endif