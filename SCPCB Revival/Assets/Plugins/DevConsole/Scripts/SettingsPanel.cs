using System;
using UnityEngine;

namespace SickDev.DevConsole {
    [Serializable]
    public class SettingsPanel {
        const float padding = 5;
        static readonly Vector2 spacing = new Vector2(20, 5);

        [SerializeField]
        ScrollView scrollView;

        float panelHeight;
        float heightSeparation;
        GUIContent titleContent;
        Toggle showTimeStampToggle;
        Toggle useAndFilteringToggle;
        Slider preferredHeightSlider;
        Slider scaleSlider;
        Slider fontSizeSlider;

        public void Initialize() {
            titleContent = new GUIContent("Settings");
            showTimeStampToggle = new Toggle("Show time stamp", ()=>DevConsole.settings.showTimeStamp, value=>DevConsole.settings.showTimeStamp = value);
            useAndFilteringToggle = new Toggle("Use AND filtering", ()=>DevConsole.settings.useAndFiltering, value => DevConsole.settings.useAndFiltering = value);
            preferredHeightSlider = new Slider("Preferred Height", () => DevConsole.settings.preferredHeight*Screen.height, value => DevConsole.settings.preferredHeight = value/Screen.height);
            preferredHeightSlider.decimalsToShow = 2;
            scaleSlider = new Slider("Scale", () => DevConsole.settings.scale, value => DevConsole.settings.scale = value);
            scaleSlider.decimalsToShow = 2;
            scaleSlider.delayed = true;
            fontSizeSlider = new Slider("Font size", ()=>DevConsole.settings.fontSize, value=>DevConsole.settings.fontSize = (int)value);

            heightSeparation = showTimeStampToggle.height;
        }

        public void Draw(float positionY, float height) {
            Rect viewRect = new Rect(0, positionY, Screen.width / DevConsole.settings.scale, height);
            Rect contentsRect = new Rect(viewRect.x, viewRect.y, viewRect.width, panelHeight);
            GUIUtils.DrawBox(viewRect, DevConsole.settings.mainColor);
            scrollView.Draw(viewRect, contentsRect, DrawContents);
            if(!scrollView.isScrollbarVisible)
                scrollView.ScrollToTop();
        }

        void DrawContents(Rect rect, Vector2 scrollPosition) {
            rect.y = 0;
            DrawTitle(ref rect);
            rect.y += rect.height;
            rect.x = padding;
            rect.width -= padding * 2;
            rect.width -= spacing.x * 2;
            float elementWidth = rect.width / 3;
            rect.width = elementWidth;
            rect.height = 32;
            DrawShowTimeStamp(rect);
            rect.x += rect.width+ spacing.x;
            DrawUseAndFiltering(rect);
            rect.x += rect.width+ spacing.x;
            DrawPreferredHeight(rect);
            rect.x = padding;
            rect.y += heightSeparation+ spacing.y;
            DrawScale(rect);
            rect.x += rect.width+ spacing.x;
            DrawFontSize(rect);
            rect.x += rect.width + spacing.x;

            panelHeight = rect.y + heightSeparation;
        }

        void DrawTitle(ref Rect rect) {
            rect.height = 20;
            GUIUtils.DrawBox(rect, GUIUtils.darkerGrayColor);
            GUI.Label(rect, titleContent, GUIUtils.centeredTextStyle);
        }

        void DrawShowTimeStamp(Rect rect) {
            showTimeStampToggle.Draw(rect);
        }

        void DrawUseAndFiltering(Rect rect) {
            useAndFilteringToggle.Draw(rect);
        }

        void DrawPreferredHeight(Rect rect) {
            preferredHeightSlider.Draw(rect, 0, Screen.height);
        }

        void DrawScale(Rect rect) {
            scaleSlider.Draw(rect, 0.5f, 3);
        }

        void DrawFontSize(Rect rect) {
            fontSizeSlider.Draw(rect, 0, 30);
        }
    }
}