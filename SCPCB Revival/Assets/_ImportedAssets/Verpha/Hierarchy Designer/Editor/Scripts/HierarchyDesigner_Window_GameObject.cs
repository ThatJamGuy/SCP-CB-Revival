#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Verpha.HierarchyDesigner
{
    internal class HierarchyDesigner_Window_GameObject : EditorWindow
    {
        #region Properties
        #region GUI
        private Vector2 mainScroll;
        #endregion

        #region Tag, Layer
        private GameObject gameObject;
        private bool isTag;
        private string windowLabel;
        #endregion
        #endregion

        #region Window
        public static void OpenWindow(GameObject gameObject, bool isTag, Vector2 position)
        {
            HierarchyDesigner_Window_GameObject window = GetWindow<HierarchyDesigner_Window_GameObject>("HD Tag and Layer");

            window.minSize = new(250, 95);
            Vector2 textSize = HierarchyDesigner_Shared_GUI.RegularLabelStyle.CalcSize(new(gameObject.name));
            window.maxSize = new(textSize.x + 50, textSize.y + 90);

            Vector2 offset = new(-12, 25);
            Vector2 adjustedPosition = position - offset;
            window.position = new(adjustedPosition, window.minSize);

            window.gameObject = gameObject;
            window.isTag = isTag;
            window.windowLabel = isTag ? "Tag" : "Layer";
        }
        #endregion

        #region Main
        private void OnGUI()
        {
            bool cancelLayout = false;
            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.TertiaryPanelStyle);
            mainScroll = GUILayout.BeginScrollView(mainScroll, false, false);

            EditorGUILayout.BeginVertical(HierarchyDesigner_Shared_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField($"▷ {windowLabel}", HierarchyDesigner_Shared_GUI.TabLabelStyle);
            GUILayout.Space(4);

            #region Body
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"<color=#{(HierarchyDesigner_Manager_Editor.IsProSkin ? "67F758" : "50C044")}>GameObject:</color> {gameObject.name}", HierarchyDesigner_Shared_GUI.RegularLabelStyle, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            GUILayout.Space(2);

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            if (isTag)
            {
                string newTag = EditorGUILayout.TagField(gameObject.tag);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(gameObject, "Change Tag");
                    gameObject.tag = newTag;
                    Close();
                }
            }
            else
            {
                int newLayer = EditorGUILayout.LayerField(gameObject.layer);
                if (EditorGUI.EndChangeCheck())
                {
                    bool shouldChangeLayer = true;
                    if (gameObject.transform.childCount > 0)
                    {
                        int result = AskToChangeChildrenLayers(gameObject, newLayer);
                        if (result == 2)
                        {
                            shouldChangeLayer = false;
                            cancelLayout = true;
                        }
                    }
                    if (shouldChangeLayer)
                    {
                        Undo.RecordObject(gameObject, "Change Layer");
                        gameObject.layer = newLayer;
                        Close();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            #endregion

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            if (cancelLayout)
            {
                return;
            }
        }
        #endregion

        #region Methods
        private static int AskToChangeChildrenLayers(GameObject obj, int newLayer)
        {
            int option = EditorUtility.DisplayDialogComplex(
                           "Change Layer",
                           $"Do you want to set the layer to '{LayerMask.LayerToName(newLayer)}' for all child objects as well?",
                           "Yes, change children",
                           "No, this object only",
                           "Cancel"
                       );

            if (option == 0)
            {
                SetLayerRecursively(obj, newLayer);
            }

            return option;
        }

        private static void SetLayerRecursively(GameObject obj, int newLayer)
        {
            foreach (Transform child in obj.transform)
            {
                Undo.RecordObject(child.gameObject, "Change Layer");
                child.gameObject.layer = newLayer;
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }
        #endregion
    }
}
#endif