using System;
using System.Collections.Generic;
using UnityEngine;

namespace SickDev.DevConsole {
    [Serializable]
    public struct EntryData {
        const int MAX_CHARACTERS = 16300;
        const string EXTRA_TEXT = " [...]";

        public string text;
        public Texture2D icon;
        public string stackTrace;
        public List<string> tags;
        public EntryOptions options;

        public EntryData(string text, params string[] tags) :this(text, new EntryOptions(), tags) { }
        public EntryData(string text, EntryOptions options, params string[] tags) :this(text, null, null, options, tags) { }

        public EntryData(string text, Texture2D icon, params string[] tags):this(text, icon, new EntryOptions(), tags) { }
        public EntryData(string text, Texture2D icon, EntryOptions options, params string[] tags): this(text, icon, null, options, tags) { }

        public EntryData(string text, Texture2D icon, string stackTrace, params string[] tags) :this(text, icon, stackTrace, new EntryOptions(), tags) { }
        public EntryData(string text, Texture2D icon, string stackTrace, EntryOptions options, params string[] tags) {
            this.text = text;
            this.icon = icon;
            this.stackTrace = stackTrace;
            this.options = options;
            this.tags = new List<string>(tags);
            SanitizeTags();
        }

        void SanitizeTags() {
            for(int i = 0; i < tags.Count; i++)
                tags[i] = SanitizeTag(tags[i]);
        }

        string SanitizeTag(string tag) {
            tag = tag.Trim();
            tag = tag.Replace("\n", string.Empty);
            tag = tag.Replace("\r", string.Empty);
            tag = tag.Replace(" ", string.Empty);
            return tag;
        }

        public void CutOffExceedingText() {
            if(text.Length + stackTrace.Length > MAX_CHARACTERS) 
                text = text.Substring(0, MAX_CHARACTERS - stackTrace.Length - EXTRA_TEXT.Length) + EXTRA_TEXT;
        }
    }
}