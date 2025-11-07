using UnityEngine;
using System;

namespace SickDev.DevConsole {
    [Serializable]
    public class ConsoleInput {
        const float lineHeight = 1;
        const float padding = 2;
        const string name = "ConsoleInput";

        public float height;
        [SerializeField]
        Color color;
        [SerializeField]
        Color cursorColor;
        [SerializeField]
        float spacing;

        [HideInInspector]
        public string text = string.Empty;
        GUIContent autoCompleteContent;
        GUIContent submitContent;
        GUIContent deleteContent;
        GUIContent historyContent;
        bool focus;
        TextEditor textEditor;

        public float offsetX { get; private set; }
        float contentHeight { get { return height - padding * 2; } }

        public void Initialize() {
            autoCompleteContent = new GUIContent(DevConsole.settings.autoCompleteIcon);
            submitContent = new GUIContent(DevConsole.settings.submitInputIcon);
            deleteContent = new GUIContent(DevConsole.settings.deleteInputIcon);
            historyContent = new GUIContent(DevConsole.settings.historyIcon);
        }

        public void Draw(float positionY) {
            Rect rect = new Rect(0, positionY, Screen.width/DevConsole.settings.scale, height-lineHeight);
            DrawLine(rect);
            rect.y += lineHeight;
            GUIUtils.DrawBox(rect, DevConsole.settings.mainColor);
            rect.x += padding;
            rect.y += padding;
            rect.width -= padding * 2;
            rect.height -= padding*2;
            if (DevConsole.settings.autoCompleteBehaviour != Settings.AutoCompleteBehaviour.Disabled) {
                DrawGenericButton(ref rect, autoCompleteContent, DevConsole.singleton.ToggleAutoComplete);
                rect.x += rect.width;
                rect.x += spacing;
            }
            DrawInput(ref rect);
            rect.x += rect.width;
            rect.x += spacing;
            DrawGenericButton(ref rect, submitContent, DevConsole.singleton.SubmitInputText);
            rect.x += rect.width;
            rect.x += spacing;
            DrawGenericButton(ref rect, deleteContent, Clear);
            rect.x += rect.width;
            rect.x += spacing;
            if (DevConsole.settings.enableHistory)
                DrawGenericButton(ref rect, historyContent, DevConsole.singleton.ToggleHistory);
        }

        void DrawLine(Rect rect) {
            rect.height = lineHeight;
            GUIUtils.DrawBox(rect, GUIUtils.blackColor);
        }

        void DrawInput(ref Rect rect) {
            rect.width = CalculateInputWidth(rect.height);
            if(focus && GUI.enabled) {
                GUI.FocusControl(name);
                focus = false;
            }
            offsetX = rect.x;
            GUIUtils.DrawBox(rect, color);
            Color cursorColor = GUI.skin.settings.cursorColor;
            GUI.skin.settings.cursorColor = this.cursorColor;
            rect.x += 2;
            rect.width -= 4;
            bool wasEmpty = string.IsNullOrEmpty(text.Trim());
            GUI.SetNextControlName(name);
            text = GUI.TextField(rect, text, GUIUtils.inputStyle);
            if(DevConsole.settings.autoCompleteBehaviour == Settings.AutoCompleteBehaviour.Auto && wasEmpty && !string.IsNullOrEmpty(text.Trim())) {
                DevConsole.singleton.autoComplete.Open();
                DevConsole.singleton.history.Close();
            }
            rect.x -= 2;
            rect.width += 4;
            GUI.skin.settings.cursorColor = cursorColor;
        }

        float CalculateInputWidth(float rectHeight) {
            float width = Screen.width / DevConsole.settings.scale;
            width -= padding * 2;
            if (DevConsole.settings.autoCompleteBehaviour != Settings.AutoCompleteBehaviour.Disabled) {
                width -= GetClampedButtonSize(rectHeight, autoCompleteContent.image.width);
                width -= spacing;
            }
            width -= GetClampedButtonSize(rectHeight, submitContent.image.width);
            width -= GetClampedButtonSize(rectHeight, deleteContent.image.width);
            width -= spacing * 2;
            if (DevConsole.settings.enableHistory) {
                width -= GetClampedButtonSize(rectHeight, historyContent.image.width);
                width -= spacing;
            }
            return width;
        }

        float GetClampedButtonSize(float rectHeight, float imageWidth) {
            return Mathf.Min(rectHeight, imageWidth, DevConsole.settings.maxButtonSize);
        }

        void DrawGenericButton(ref Rect rect, GUIContent content, Action callback) {
            rect.width = GetClampedButtonSize(rect.height, content.image.width);
            if(GUIUtils.DrawCenteredButton(rect, content, Color.clear))
                callback();
        }

        public void Clear() {
            text = string.Empty;
        }

        public void Focus() {
            focus = true;
        }

        public void MoveCursorToEnd() {
#if !(UNITY_ANDROID || UNITY_IOS) || UNITY_EDITOR
            textEditor = (TextEditor)GUIUtility.QueryStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            textEditor.text = text;
            textEditor.MoveTextEnd();
#endif
        }
    }
}