using UnityEngine;
using System;

namespace SickDev.DevConsole {
    [Serializable]
    public class ScrollView {
        public delegate void ScrollViewDrawer(Rect rect, Vector2 scrollPosition);

        [SerializeField]
        ScrollBar scrollBar;
        
        [HideInInspector]
        public Vector2 position;
        float? targetScrollTo;
        Rect viewRect;
        Rect contentsRect;

        public bool isScrollbarVisible { get { return contentsRect.height > viewRect.height; } }
        public float scrollBarWidth { get { return scrollBar.width; } }

        public void Draw(Rect viewRect, Rect contentsRect, ScrollViewDrawer contentsDrawer) {
            this.viewRect = viewRect;
            this.contentsRect = contentsRect;
            HandleScrollWheel();
            DrawVerticalScrollBar();
            DrawViewRect(contentsDrawer);
            if(targetScrollTo.HasValue)
                DoScrollToTarget();
        }

        void HandleScrollWheel() {
            if(isScrollbarVisible) {
                if(Event.current.type != EventType.ScrollWheel || !viewRect.Contains(Event.current.mousePosition))
                    return;
                position.y += scrollBar.sensitivity * Event.current.delta.y;
                Event.current.Use();
            }
        }

        void DrawVerticalScrollBar() {
            if(isScrollbarVisible) {
                viewRect.width -= scrollBar.width;
                position.y = scrollBar.Draw(new Rect(viewRect.x + viewRect.width, viewRect.y, scrollBar.width, viewRect.height), position.y, contentsRect.height);
            }
        }

        void DrawViewRect(ScrollViewDrawer contentsDrawer) {
            GUI.BeginGroup(viewRect);
            contentsRect.y -= viewRect.y;
            contentsRect.y -= position.y;
            GUI.BeginGroup(contentsRect);
            contentsDrawer(viewRect, position);
            GUI.EndGroup();
            GUI.EndGroup();
        }

        void DoScrollToTarget() {
            position.y = targetScrollTo.Value;
            targetScrollTo = null;
        }

        public void ScrollToBottom() {
            ScrollTo(float.PositiveInfinity);
        }

        public void ScrollToTop() {
            ScrollTo(0);
        }

        public void ScrollTo(float position) {
            targetScrollTo = position;
        }
    }
}