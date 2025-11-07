using System.Collections.Generic;
using UnityEngine;

namespace SickDev.DevConsole {
    public class EntryGroup { 
        List<Entry> entryList = new List<Entry>();
        public GUIContent content;

        public Entry[] entries { get; private set; }
        public Entry lastEntry { get; private set; }

        public EntryGroup(Entry entry) {
            content = new GUIContent();
            Add(entry);
        }

        public void Add(Entry entry) {
            entryList.Add(entry);
            entries = entryList.ToArray();
            lastEntry = entry;
            entry.group = this;
            UpdateContent();
        }

        void UpdateContent() {
            content.text = "("+entryList.Count.ToString()+")";
        }

        public void Remove(Entry entry) {
            entryList.Remove(entry);
            entries = entryList.ToArray();
            if(lastEntry == entry && entries.Length > 0)
                lastEntry = entries[entries.Length - 1];
            UpdateContent();
        }

        public bool CanAcceptEntry(Entry entry) {
            return entry.data.text == lastEntry.data.text && entry.data.stackTrace == lastEntry.data.stackTrace;
        }
    }
}