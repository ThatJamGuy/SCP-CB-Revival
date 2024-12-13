#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Verpha.HierarchyDesigner
{
    internal class HierarchyDesigner_Window_Rename : EditorWindow
    {
        #region Properties
        #region GUI
        private Vector2 mainScroll;
        private const float fieldsWidth = 130;
        #endregion

        #region Info
        private string newName = "";
        private bool automaticIndexing = true;
        private int startingIndex = 0;
        [SerializeField] private List<GameObject> selectedGameObjects = new();
        private ReorderableList reorderableList;
        #endregion
        #endregion

        #region Window
        public static void OpenWindow(List<GameObject> gameObjects, bool autoIndex = true, int startIndex = 0)
        {
            HierarchyDesigner_Window_Rename window = GetWindow<HierarchyDesigner_Window_Rename>("HD Rename Tool");
            Vector2 size = new(400, 200);
            window.minSize = size;
            window.newName = "";
            window.automaticIndexing = autoIndex;
            window.startingIndex = startIndex;
            window.selectedGameObjects = gameObjects ?? new();
            window.InitializeReorderableList();
        }
        #endregion

        #region Initialization
        private void InitializeReorderableList()
        {
            reorderableList = new(selectedGameObjects, typeof(GameObject), true, true, true, true)
            {
                drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, "GameObjects' List");
                },
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    selectedGameObjects[index] = (GameObject)EditorGUI.ObjectField(new(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), selectedGameObjects[index], typeof(GameObject), true);
                },
                onAddCallback = (ReorderableList list) =>
                {
                    selectedGameObjects.Add(null);
                },
                onRemoveCallback = (ReorderableList list) =>
                {
                    selectedGameObjects.RemoveAt(list.index);
                }
            };
        }
        #endregion

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.TertiaryPanelStyle);

            #region Body
            mainScroll = EditorGUILayout.BeginScrollView(mainScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            #region New Values
            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("▷ RENAME TOOL", HierarchyDesigner_Shared_GUI.TabLabelStyle);
            GUILayout.Space(5);

            newName = HierarchyDesigner_Shared_GUI.DrawTextField("New Name", fieldsWidth, string.Empty, newName, true, "The new name that the selected GameObjects will be renamed to.");
            automaticIndexing = HierarchyDesigner_Shared_GUI.DrawToggle("Auto-Index", fieldsWidth, automaticIndexing, true, true, "The selected GameObjects will be renamed with indexes (e.g., (1), (2), (3), ...).");
            if (automaticIndexing) { startingIndex = HierarchyDesigner_Shared_GUI.DrawIntField("Starting Index", fieldsWidth, startingIndex, 0, true, "The starting value of the index (e.g., a value of 10 will start the indexing at (10), and so on)."); }
            #endregion

            GUILayout.Space(10);

            #region Selected GameObjects List
            if (reorderableList != null) 
            {
                EditorGUILayout.LabelField("Selected GameObjects", HierarchyDesigner_Shared_GUI.FieldsCategoryLabelStyle);
                GUILayout.Space(2);
                reorderableList.DoLayoutList(); 
            }
            EditorGUILayout.EndVertical();
            #endregion

            EditorGUILayout.EndScrollView();
            #endregion

            #region Footer
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reassign Selected GameObjects", GUILayout.Height(25)))
            {
                ReassignSelectedGameObjects();
            }
            if (GUILayout.Button("Clear Selected GameObjects", GUILayout.Height(25)))
            {
                ClearSelectedGameObjects();
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Rename Selected GameObjects", GUILayout.Height(30)))
            {
                RenameSelectedGameObjects();
                Close();
            }
            #endregion

            EditorGUILayout.EndVertical();
        }

        #region Methods
        private void ReassignSelectedGameObjects()
        {
            selectedGameObjects = new(Selection.gameObjects);
            InitializeReorderableList();
        }

        private void ClearSelectedGameObjects()
        {
            selectedGameObjects.Clear();
            InitializeReorderableList();
        }

        private void RenameSelectedGameObjects()
        {
            if (selectedGameObjects == null) return;

            for (int i = 0; i < selectedGameObjects.Count; i++)
            {
                if (selectedGameObjects[i] != null)
                {
                    Undo.RecordObject(selectedGameObjects[i], "Rename GameObject");
                    string objectName = automaticIndexing ? $"{newName} ({startingIndex + i})" : newName;
                    selectedGameObjects[i].name = objectName;
                    EditorUtility.SetDirty(selectedGameObjects[i]);
                }
            }
        }
        #endregion

        private void OnDestroy()
        {
            newName = "";
            selectedGameObjects = null;
            reorderableList = null;
        }
    }
}
#endif