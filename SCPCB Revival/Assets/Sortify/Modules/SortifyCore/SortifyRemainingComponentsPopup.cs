#if UNITY_EDITOR && SORTIFY
using UnityEditor;
using UnityEngine;

namespace Sortify
{
    public class SortifyRemainingComponentsPopup : PopupWindowContent
    {
        private const int _iconSize = 14;
        private const int _padding = 4;
        private const float _maxWindowWidth = 202f;

        private Component[] _components;
        private int _startIndex;
        private int _componentsInRow;
        private int _totalRows;

        public SortifyRemainingComponentsPopup(Component[] components, int startIndex)
        {
            _components = components;
            _startIndex = startIndex;
        }

        public override Vector2 GetWindowSize()
        {
            float componentWidth = _iconSize + _padding;
            _componentsInRow = Mathf.FloorToInt((_maxWindowWidth - _padding) / componentWidth);
            _totalRows = Mathf.CeilToInt((_components.Length - _startIndex) / (float)_componentsInRow);

            float windowWidth = Mathf.Min(_maxWindowWidth, ((_components.Length - _startIndex) * componentWidth) + _padding);
            float windowHeight = _totalRows * (_iconSize + _padding) + _padding;
            return new Vector2(windowWidth, windowHeight);
        }

        public override void OnGUI(Rect rect)
        {
            int componentIndex = _startIndex;
            for (int row = 0; row < _totalRows; row++)
            {
                EditorGUILayout.BeginHorizontal();

                for (int col = 0; col < _componentsInRow; col++)
                {
                    if (componentIndex >= _components.Length)
                        break;

                    GUIStyle style = new GUIStyle()
                    {
                        fixedWidth = _iconSize,
                        fixedHeight = _iconSize,
                        margin = new RectOffset(_padding, _padding, _padding, _padding)
                    };

                    Texture icon = AssetPreview.GetMiniThumbnail(_components[componentIndex]);
                    if (GUILayout.Button(icon, style)) { }

                    componentIndex++;
                }

                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
#endif
