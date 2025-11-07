using UnityEngine;
using System;

namespace SickDev.DevConsole {
    [Serializable]
    public class ScrollBar {
        const float padding = 3;
        static GUISkin lastSkin;

        static GUIStyle originalStyle;
        static GUIStyle originalThumbStyle;
        static GUIStyle style;
        static GUIStyle thumbStyle;
        static GUIContent scrollUpContent;
        static GUIContent scrollDownContent;

        [SerializeField]
        float minHeight;
        [SerializeField]
        bool invert;
        public float width;
        public float sensitivity;

        public static void Initialize() {
            style = new GUIStyle();
            thumbStyle = new GUIStyle();
            thumbStyle.normal = new GUIStyleState() { background = GUIUtils.grayTexture };
            scrollUpContent = new GUIContent(DevConsole.settings.scrollUpIcon);
            scrollDownContent = new GUIContent(DevConsole.settings.scrollDownIcon);
        }

        public float Draw(Rect rect, float position, float maxValue) {
            Rect originalRect = new Rect(rect);
            DrawScrollUpButton(ref rect, ref position);
            rect.height = originalRect.height;
            DrawScrollDownButton(ref rect, ref position);
            CalculateScrollBarRect(ref rect, originalRect);
            DrawScrollBar(rect, ref position, maxValue);
            return position;
        }

        void DrawScrollUpButton(ref Rect rect, ref float position) {
            rect.height = GetButtonWidth(rect.width);
            if(GUIUtils.DrawRepeatButton(rect, scrollUpContent, Color.clear))
                position -= sensitivity;
        }

        void DrawScrollDownButton(ref Rect rect, ref float position) {
            rect.y += rect.height - GetButtonWidth(rect.width);
            rect.height = GetButtonWidth(rect.width);
            if(GUIUtils.DrawRepeatButton(rect, scrollDownContent, Color.clear))
                position += sensitivity;
        }

        void CalculateScrollBarRect(ref Rect rect, Rect originalRect) {
            rect.x += padding;
            rect.y = originalRect.y + rect.height;
            rect.width -= padding*2;
            rect.height = originalRect.height - rect.height*2;
        }

        void DrawScrollBar(Rect rect, ref float position, float maxValue) {
            ChangeSkinStylesIfNecessary();
            thumbStyle.fixedHeight = Mathf.Max(rect.height * rect.height / maxValue, minHeight) / DevConsole.settings.scale;
            maxValue -= rect.height + Mathf.Min(rect.width+padding*2, scrollUpContent.image.height) * 2;
            position = GUI.VerticalScrollbar(rect, position, 0, invert? maxValue : 0, invert?0: maxValue);
            RestoreSkinStylesIfNecessary();
        }

        float GetButtonWidth(float rectWidth) {
            return Mathf.Min(rectWidth, scrollUpContent.image.height);
        }

        void ChangeSkinStylesIfNecessary() {
            if(GUI.skin.verticalScrollbar != style) {
                originalStyle = GUI.skin.verticalScrollbar;
                GUI.skin.verticalScrollbar = style;
            }
            if(GUI.skin.verticalScrollbarThumb != thumbStyle) {
                originalThumbStyle = GUI.skin.verticalScrollbarThumb;
                GUI.skin.verticalScrollbarThumb = thumbStyle;
            }
        }

        void RestoreSkinStylesIfNecessary() {
            if(DevConsole.settings.optimizeForOnGUI)
                return;
            if (originalStyle != null)
                GUI.skin.verticalScrollbar = originalStyle;
            if (originalThumbStyle != null)
                GUI.skin.verticalScrollbarThumb = originalThumbStyle;
        }
    }
}