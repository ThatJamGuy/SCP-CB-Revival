using System;
using UnityEngine;

namespace SickDev.DevConsole {
    [Serializable]
    public class History:Window<string, string> {
        GUIContent tempContent = new GUIContent();

        protected override GUIContent title { get {return new GUIContent("HISTORY"); } }
        protected override int size { get { return entries.Count - 1; } }
        public override string current { get { return entries[currentIndex]; } }

        protected override void OnInitialized() {
            if(entries.Count == 0)
                Add(string.Empty);
        }

        public override void Navigate(int direction) {
            if (currentIndex == entries.Count-1)
                UpdateCurrentEntry(DevConsole.singleton.input.text);
            base.Navigate(direction);
            DevConsole.singleton.input.text = current;
            DevConsole.singleton.input.MoveCursorToEnd();
        }

        protected override void DrawEntry(Rect rect, string entry) {
            GUI.Label(rect, entry, GUIUtils.textStyle);
        }

        protected override void OnEntryClicked(string entry) {
            DevConsole.singleton.input.text = entry;
            DevConsole.singleton.input.MoveCursorToEnd();
        }

        public void UpdateCurrentEntry(string text) {
            entries[currentIndex] = text;
        }

        public void UpdateLastEntry(string text) {
            entries[entries.Count - 1] = text;
        }

        public bool CanEntryBeAdded(string entry) {
            return entries.Count<2 || entry != entries[entries.Count - 2];
        }

        protected override float GetEntryWidth(string entry) {
            tempContent.text = entry;
            return GUIUtils.textStyle.CalcSize(tempContent).x;
        }

        protected override void OnOpen() {
            if (!DevConsole.settings.enableHistory)
                Close();
        }
    }
}