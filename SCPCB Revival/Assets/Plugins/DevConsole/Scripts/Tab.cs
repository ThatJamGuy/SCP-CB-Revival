using System;
using UnityEngine;

namespace SickDev.DevConsole {
    [Serializable]
    public class Tab {
        static GUIStyle style;

        GUIContent content;

        public Filter filter { get; private set; }

        public static void Initialize() {
            style = new GUIStyle(GUIUtils.centeredButtonStyle);
            style.normal.textColor = style.hover.textColor = style.active.textColor = GUIUtils.slightlyGrayColor;
        }

        public Tab(Filter filter) {
            this.filter = filter;
            content = new GUIContent(filter.tag);
        }

        public void Draw(Rect rect) {
            if(GUIUtils.DrawButton(rect, content, filter.isActive ? GUIUtils.grayerColor : GUIUtils.darkerGrayColor, style))
                filter.isActive = !filter.isActive;
        }
    }
}