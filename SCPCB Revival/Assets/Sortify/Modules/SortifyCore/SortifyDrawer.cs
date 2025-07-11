#if UNITY_EDITOR && SORTIFY
using UnityEngine;
using UnityEditor;

namespace Sortify
{
    public static class SortifyDrawer
    {
        private const float _iconSize = 14f;
        private const float _padding = 4f;

        public static void Draw(int instanceID, Rect selectionRect, GameObject obj)
        {
            string objID = SortifyHelper.GetObjectID(obj);

            if (SortifyObjectManager.LoadColor(obj).HasValue)
                SortifyObjectManager.DrawColor(obj, selectionRect);

            if (SortifyObjectManager.LoadStyleType(obj) != SortifyObjectManager.StyleType.Default)
                SortifyObjectManager.DrawStyleType(obj, selectionRect);

            bool isPrefab = PrefabUtility.GetPrefabAssetType(obj) != PrefabAssetType.NotAPrefab;
            Rect buttonRect = new Rect(selectionRect.x + selectionRect.width - _padding - (isPrefab ? 10 : 0), selectionRect.y + 1, _iconSize, _iconSize);
            if (GUI.Button(buttonRect, EditorGUIUtility.IconContent("Toolbar Plus"), GUIStyle.none))
                PopupWindow.Show(buttonRect, new SortifySettingsPopup(obj, objID));

            if (SortifyUserDataManager.GetUserSetting("ShowComponentIcons", true) && SortifyObjectManager.LoadShowComponents(obj, true))
            {
                buttonRect.x -= _iconSize + _padding;

                Component[] components = obj.GetComponents<Component>();
                int availableWidthForIcons = CalculateWidthForIcons(obj.name, selectionRect);
                int maxIconsToShow = Mathf.FloorToInt(availableWidthForIcons / (_iconSize + _padding));

                int shownIcons = 0;
                for (int i = 0; i < components.Length; i++)
                {
                    if (shownIcons >= maxIconsToShow - 1)
                        break;

                    Texture icon = AssetPreview.GetMiniThumbnail(components[i]);
                    if (icon != null)
                    {
                        if (GUI.Button(buttonRect, icon, GUIStyle.none)) { }
                        buttonRect.x -= _iconSize + _padding;
                    }
                    shownIcons++;
                }

                if (components.Length > shownIcons)
                {
                    if (GUI.Button(buttonRect, EditorGUIUtility.IconContent("d_CollabChangesDeleted Icon"), GUIStyle.none))
                        PopupWindow.Show(buttonRect, new SortifyRemainingComponentsPopup(components, shownIcons));
                }
            }
        }

        private static int CalculateWidthForIcons(string objName, Rect selectionRect)
        {
            float width = selectionRect.width - (GUI.skin.label.CalcSize(new GUIContent(objName)).x + _padding);
            width -= 15f;
            return (int)(width);
        }
    }
}
#endif
