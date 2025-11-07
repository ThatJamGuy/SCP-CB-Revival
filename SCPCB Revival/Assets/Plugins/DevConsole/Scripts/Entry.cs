using System;
using UnityEngine;

namespace SickDev.DevConsole {
    [Serializable]
    public class Entry {
        public delegate void EntryDelegate(Entry entry);

        const float spacing = 5;
        const string namePrefix = "entry";

        static readonly float alpha = 0.3f;
        static readonly Color backgroundColor;

        static int nextId = 1000;
        static GUIContent foldoutCollapsedContent;
        static GUIContent foldoutExpandedContent;
        static GUIContent stackTraceContent;
        static GUIContent removeContent;

        int id;
        string name;
        GUIContent timeStampContent;
        GUIContent textContent = new GUIContent();
        Vector2 timeStampSize;
        float groupContentWidth;
        int lastGroupContentLength;
        Vector2 textSize;
        float lastTextHeight;
        float lastEntriesSpacing;
        float unExpadedTextHeight;
        int lastFontSize;
        GUIStyle textStyle;
        SimpleTextBuilder builder = new SimpleTextBuilder();

        public event EntryDelegate onEntryRemoved;
        public event EntryDelegate onEntryRebuilt;
        
        public EntryData data { get; private set; }
        public string timeStamp { get; private set; }
        public bool showStackTrace { get; set; }
        public bool isExpanded { get; set; }
        public EntryGroup group { get; set; }

        public float height { get { return textSize.y + DevConsole.settings.entriesSpacing * 2; } }

        static Entry() {
            backgroundColor = Color.black;
            backgroundColor.a = alpha;
            foldoutCollapsedContent = new GUIContent(DevConsole.settings.entryCollapsedIcon);
            foldoutExpandedContent = new GUIContent(DevConsole.settings.entryExpandedIcon);
            stackTraceContent = new GUIContent(DevConsole.settings.stackTraceIcon);
            removeContent = new GUIContent(DevConsole.settings.removeEntryIcon);
        }

        public Entry(EntryData data) {
            id = nextId++;
            name = namePrefix + id;
            data.CutOffExceedingText();
            this.data = data;
            timeStamp = DateTime.Now.ToString("[HH:mm:ss] ");
            timeStampContent = new GUIContent(timeStamp);
            textStyle = new GUIStyle(GUIUtils.textStyle);
        }

        public void Draw(float positionY, float entryWidth, bool isVisible) {
            DoLayout(entryWidth);
            if(!isVisible)
                return;

            GUIUtils.DrawBox(new Rect(0, positionY, entryWidth, height), backgroundColor);
            Rect rect = new Rect(0, positionY + DevConsole.settings.entriesSpacing, 0, 0);
            DrawFoldoutToggle(ref rect);
            rect.x += rect.width;
            if(DevConsole.settings.showTimeStamp) {
                DrawTimeStamp(ref rect);
                rect.x += rect.width;
                rect.x += spacing;
            }
            if(data.icon != null) {
                DrawIcon(ref rect);
                rect.x += rect.width;
                rect.x += spacing;
            }
            DrawText(ref rect, textSize.x);
            rect.x += rect.width;
            if(DevConsole.settings.groupIdenticalEntries) {
                DrawGroupContent(ref rect);
                rect.x += rect.width;
            }
            DrawStackTraceToggle(ref rect);
            rect.x += rect.width;
            DrawRemoveButton(ref rect);
        }

        void DoLayout(float entryWidth) {
            //The styled is applied to calculate sizes properly
            ApplyStyle();
            builder.RebuildIfNecessary(this, textSize.x);
            SetContentText();
            RestoreStyle();

            if(lastFontSize != DevConsole.settings.fontSize) {
                lastFontSize = DevConsole.settings.fontSize;
                timeStampSize = textStyle.CalcSize(timeStampContent);
                groupContentWidth = textStyle.CalcSize(group.content).x;
            }
            else if(lastGroupContentLength != group.content.text.Length) {
                lastGroupContentLength = group.content.text.Length;
                groupContentWidth = textStyle.CalcSize(group.content).x;
            }

            CalculateTextSize(entryWidth);

            if(!isExpanded && !showStackTrace)
                unExpadedTextHeight = textSize.y;

            bool alreadyRebuilt = false;
            if(DevConsole.settings.entriesSpacing != lastEntriesSpacing) {
                lastEntriesSpacing = DevConsole.settings.entriesSpacing;
                onEntryRebuilt(this);
                alreadyRebuilt = true;
            }
            if(textSize.y != lastTextHeight) {
                lastTextHeight = textSize.y;
                if (!alreadyRebuilt)
                    onEntryRebuilt(this);
            }
        }

        void CalculateTextSize(float entryWidth) {
            textSize.x = CalculateTextWidth(entryWidth);
            textSize.y = textStyle.CalcHeight(textContent, textSize.x);
        }

        float CalculateTextWidth(float entryWidth) {
            entryWidth -= foldoutCollapsedContent.image.width;
            entryWidth -= stackTraceContent.image.width;
            entryWidth -= removeContent.image.width;
            if(DevConsole.settings.showTimeStamp) {
                entryWidth -= timeStampSize.x;
                entryWidth -= spacing;
            }
            if(data.icon != null) {
                entryWidth -= Mathf.Min(data.icon.height, unExpadedTextHeight);
                entryWidth -= spacing;
            }
            if(DevConsole.settings.groupIdenticalEntries)
                entryWidth -= groupContentWidth;
            return entryWidth;
        }

        void SetContentText() {
            textContent.text = isExpanded ? data.text : builder.simpleText;
            if(showStackTrace)
                textContent.text += "\n\n" + data.stackTrace;
        }

        void DrawFoldoutToggle(ref Rect rect) {
            float oldY = rect.y;
            rect.width = foldoutCollapsedContent.image.width;
            rect.height = height - textSize.y + timeStampSize.y - DevConsole.settings.entriesSpacing * 2;
            bool expand = false;
            if(builder.needsExpandToggle) {
                expand = isExpanded;
                if(GUIUtils.DrawCenteredButton(rect, isExpanded ? foldoutExpandedContent : foldoutCollapsedContent, Color.clear))
                    expand = !isExpanded;
            }
            isExpanded = expand;
            rect.y = oldY;
        }

        void DrawTimeStamp(ref Rect rect) {
            rect.width = timeStampSize.x;
            rect.height = height;
            GUI.Label(rect, timeStampContent, textStyle);
        }

        void DrawIcon(ref Rect rect) {
            rect.width = rect.height = Mathf.Min(data.icon.height, unExpadedTextHeight);
            GUI.DrawTexture(rect, data.icon, ScaleMode.ScaleToFit);
        }

        void DrawText(ref Rect rect, float width) {
            rect.width = width;
            rect.height = height;

            GUIUtility.GetControlID(id, FocusType.Keyboard);
            ApplyStyle();
            GUI.SetNextControlName(name);
            if(Application.isMobilePlatform)
                GUI.Label(rect, textContent.text, textStyle);
            else
                GUI.TextField(rect, textContent.text, textStyle);
            RestoreStyle();
        }

        void ApplyStyle() {
            textStyle.fontStyle = data.options.style;
            if(data.options.size > 0)
                textStyle.fontSize = data.options.size;
            textStyle.normal.textColor = data.options.color;
        }

        void RestoreStyle() {
            textStyle.fontStyle = GUIUtils.textStyle.fontStyle;
            textStyle.fontSize = GUIUtils.textStyle.fontSize;
            textStyle.normal.textColor = GUIUtils.textStyle.normal.textColor;
        }

        void DrawGroupContent(ref Rect rect) {
            rect.width = groupContentWidth;
            rect.height = height;
            GUI.Label(rect, group.content, textStyle); 
        }

        void DrawStackTraceToggle(ref Rect rect) {
            rect.width = stackTraceContent.image.width;
            rect.height = height - textSize.y + timeStampSize.y- DevConsole.settings.entriesSpacing * 2;
            if(GUIUtils.DrawCenteredButton(rect, stackTraceContent, Color.clear))
                showStackTrace = !showStackTrace;
        }

        void DrawRemoveButton(ref Rect rect) {
            rect.width = removeContent.image.width;
            rect.height = height - textSize.y + timeStampSize.y - DevConsole.settings.entriesSpacing * 2;
            if(GUIUtils.DrawCenteredButton(rect, removeContent, Color.clear))
                Remove();
        }

        public void Remove() {
            onEntryRemoved(this);
        }

        [Serializable]
        class SimpleTextBuilder {
            const string toBeContinuedText = " [...]";
            
            float widthReservedForText;
            float realTextWidth;
            GUIContent content = new GUIContent();
            public string simpleText = string.Empty;
            public bool needsExpandToggle;

            //Entry is received as a method parameter instead of as a constructor parameter to avoid Unity recursive serialization problems
            public void RebuildIfNecessary(Entry entry, float widthReservedForText) {
                float realTextWidth = entry.textStyle.CalcSize(entry.textContent).x;
                if(Event.current.type == EventType.Repaint && (!Mathf.Approximately(widthReservedForText, this.widthReservedForText) || realTextWidth != this.realTextWidth)) {
                    this.realTextWidth = realTextWidth;
                    this.widthReservedForText = widthReservedForText;
                    Rebuild(entry);
                }
            }

            void Rebuild(Entry entry) {
                needsExpandToggle = NeedsExpandToggle(entry);
                simpleText = needsExpandToggle? BuildSimpleText(entry):entry.data.text;
            }

            bool NeedsExpandToggle(Entry entry) {
                //Size needs to be recalculated based on the whole data.text, not only textContent.text
                content.text = entry.data.text;
                Vector2 size = entry.textStyle.CalcSize(content);
                return size.x > widthReservedForText || content.text.Contains("\n") || content.text.Contains("\r");
            }

            string BuildSimpleText(Entry entry) {
                Vector2 size;
                content.text = toBeContinuedText;
                string lastSimpleText = content.text;
                
                for(int i = 0; i < entry.data.text.Length; i++) {
                    content.text = string.Concat(entry.data.text.Substring(0, i), toBeContinuedText);
                    size = entry.textStyle.CalcSize(content);
                    if(size.x > widthReservedForText)
                        break;
                    lastSimpleText = content.text;
                    if(entry.data.text[i] == '\n' || entry.data.text[i] == '\r')
                        break;
                }
                return lastSimpleText;
            }
        }
    }
}