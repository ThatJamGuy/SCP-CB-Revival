using System; 
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace SickDev.DevConsole{
    [Serializable]
    public class Logger {
        [SerializeField]
        ScrollView scrollView;
        [SerializeField]
        int capacity;

        List<EntryData> buffer = new List<EntryData>();
        List<EntryGroup> groupsList = new List<EntryGroup>();
        List<Entry> entriesList = new List<Entry>();
        List<EntryDrawData> drawDataList = new List<EntryDrawData>();
        EntryGroup[] groups = new EntryGroup[0];
        Entry[] entries = new Entry[0];
        EntryDrawData[] drawData = new EntryDrawData[0];
        float lastContentHeight;
        Rect viewRect;
        Rect contentsRect;
        EntryGroup tempGroup;
        Entry tempEntry;
        bool entryJustRemoved;
        int[] drawingEntriesIndexes = new int[0];
        int[] drawingDrawDataIndexes = new int[0];
        bool lastGroupEntries;
        List<Filter> filtersList = new List<Filter>();
        Filter[] filters = new Filter[0];
        int currentFilter;
        bool lastScrollbarVisible;
        bool[] tagHashesUsed = new bool[32];

        public event Action<Filter> onFilterAdded;
        public event Action<Filter> onFilterRemoved;

        public void Initialize() {
            for(int i = 0; i < entriesList.Count; i++)
                SetupEntry(entriesList[i]);
            lastGroupEntries = DevConsole.settings.groupIdenticalEntries;
        }

        void SetupEntry(Entry entry) {
            SetGroupToEntry(entry);
            entry.onEntryRebuilt += OnEntryRebuilt;
            entry.onEntryRemoved += OnEntryRemoved;
        }

        public void Draw(float positionY, float height) {
            CalculateCurrentFilter();
            viewRect = new Rect(0, positionY, Screen.width / DevConsole.settings.scale, height);
            contentsRect = new Rect(viewRect.x, viewRect.y, viewRect.width, lastContentHeight);
            if(buffer.Count > 0)
                AddNewEntries();
            else if(lastGroupEntries != DevConsole.settings.groupIdenticalEntries)
                SelectDrawingEntries();
            lastGroupEntries = DevConsole.settings.groupIdenticalEntries;
            GUIUtils.DrawBox(viewRect, DevConsole.settings.mainColor);
            entryJustRemoved = false;
            scrollView.Draw(viewRect, contentsRect, DrawOnlyVisibleEntries);
            if(!scrollView.isScrollbarVisible && lastScrollbarVisible)
                scrollView.ScrollToTop();
            else if(scrollView.isScrollbarVisible && !lastScrollbarVisible)
                scrollView.ScrollToBottom();
            lastScrollbarVisible = scrollView.isScrollbarVisible;
        }

        void CalculateCurrentFilter() {
            currentFilter = 0;
            for(int i = 0; i < filters.Length; i++)
                if(filters[i].isActive)
                    currentFilter += filters[i].hash;
        }

        void AddNewEntries() {
            for(int i = 0; i < buffer.Count; i++) {
                tempEntry = new Entry(buffer[i]);
                SetupEntry(tempEntry);
                entriesList.Add(tempEntry);
                EntryDrawData drawData = new EntryDrawData();
                drawData.justCreated = true;
                drawData.tagsHash = CalculateTagsHash(tempEntry); 
                drawDataList.Add(drawData);
            }
            buffer.Clear();

            while(entriesList.Count > capacity)
                RemoveEntry(entriesList[0]);

            MakeEntriesArray();
            SelectDrawingEntries();
        }

        int CalculateTagsHash(Entry entry) {
            int tagsHash = 0;
            string[] tags = entry.data.tags.ToArray();
            AddFilters(tags);
            for(int i = 0; i < tags.Length; i++) {
                for(int j = 0; j < filters.Length; j++) {
                    if(tags[i] == filters[j].tag) {
                        tagsHash += filters[j].hash;
                        break;
                    }
                }
            }
            return tagsHash;
        }

        void SetGroupToEntry(Entry entry) {
            bool createNewGroup = true;
            for(int i = 0; i < groups.Length; i++) {
                if(groups[i].CanAcceptEntry(entry)) {
                    groups[i].Add(entry);
                    createNewGroup = false;
                    break;
                }
            }

            if(createNewGroup) {
                tempGroup = new EntryGroup(entry);
                groupsList.Add(tempGroup);
                groups = groupsList.ToArray();
            }
        }

        void MakeEntriesArray() {
            entries = entriesList.ToArray();
            drawData = drawDataList.ToArray();
        }

        void SelectDrawingEntries() {
            if(DevConsole.settings.groupIdenticalEntries)
                SelectGroupEntries();
            else
                SelectIndividualEntries();
        }

        void SelectGroupEntries() {
            drawingEntriesIndexes = new int[groups.Length];
            drawingDrawDataIndexes = new int[drawingEntriesIndexes.Length];
            for(int i = 0; i < groups.Length; i++) {
                int index = entriesList.IndexOf(groups[i].lastEntry);
                drawingEntriesIndexes[i] = index;
                drawingDrawDataIndexes[i] = index;
            }
        }

        void SelectIndividualEntries() {
            drawingEntriesIndexes = new int[entries.Length];
            drawingDrawDataIndexes = new int[drawingEntriesIndexes.Length];
            for(int i = 0; i < entries.Length; i++) {
                drawingEntriesIndexes[i] = i;
                drawingDrawDataIndexes[i] = i;
            }
        }

        void OnEntryRebuilt(Entry entry) {
            int index = Array.IndexOf(entries, entry);
            float entryHeight = entry.height;
            drawDataList[index].height = entryHeight;
            drawData[index].height = entryHeight;
            if(lastContentHeight + entryHeight > viewRect.height && scrollView.position.y + viewRect.height >= contentsRect.height)
                scrollView.ScrollToBottom();
        }

        void DrawOnlyVisibleEntries(Rect rect, Vector2 scrollPosition) {
            lastContentHeight = 0;
            float rectHeight = rect.height;
            for(int i = 0; i < drawingEntriesIndexes.Length; i++) {
                try {
                    tempEntry = entries[drawingEntriesIndexes[i]];
                }
                catch {
                    tempEntry = null;
                }
                bool isFiltered = CheckIsEntryFiltered(drawData[drawingDrawDataIndexes[i]]);
                if(!isFiltered)
                    continue;
                bool isVisible = lastContentHeight + drawData[drawingDrawDataIndexes[i]].height >= scrollPosition.y && lastContentHeight < scrollPosition.y + rectHeight;
                if(isVisible || drawData[drawingDrawDataIndexes[i]].justCreated) {
                    drawData[drawingDrawDataIndexes[i]].justCreated = false;
                    tempEntry.Draw(lastContentHeight, rect.width, isVisible);
                }
                if(!entryJustRemoved)
                    lastContentHeight += drawData[drawingDrawDataIndexes[i]].height;
                else {
                    entryJustRemoved = false;
                }
            }
        }

        bool CheckIsEntryFiltered(EntryDrawData drawData) {
            if(!DevConsole.settings.useAndFiltering)
                return (currentFilter & drawData.tagsHash) != 0;
            else
                return (currentFilter & drawData.tagsHash) == currentFilter;
        }

        public void AddEntry(EntryData entry) {
            if(string.IsNullOrEmpty(entry.stackTrace))
                entry.stackTrace = StackTraceUtility.ExtractStackTrace().Trim();
            if(entry.tags == null)
                entry.tags = new List<string>();
            if(entry.tags.Count == 0 && !entry.tags.Contains(DevConsole.settings.defaultTag))
                entry.tags.Add(DevConsole.settings.defaultTag);

            buffer.Add(entry);
        }

        public Entry[] GetEntries() {
            return entries;
        }

        public void Clear() {
            for(int i = entries.Length - 1; i >= 0; i--)
                RemoveEntry(entries[i]);
            MakeEntriesArray();
            SelectDrawingEntries();
        }

        void OnEntryRemoved(Entry entry) {
            if(lastGroupEntries)
                RemoveWholeGroup(entry.group);
            else
                RemoveEntry(entry);
            MakeEntriesArray();
            SelectDrawingEntries();
        }

        void RemoveWholeGroup(EntryGroup group) {
            Entry[] groupEntries = group.entries;
            for(int i = 0; i < groupEntries.Length; i++)
                RemoveEntryAlone(groupEntries[i]);
            RemoveGroup(group); 
        }

        void RemoveEntryAlone(Entry entry) {
            int index = entriesList.IndexOf(entry);
            entriesList.Remove(entry);
            drawDataList.RemoveAt(index);
            entryJustRemoved = true;
            RemoveFiltersIfUnused(entry.data.tags.ToArray());
        }

        void RemoveFiltersIfUnused(string[] tags) {
            for(int i = 0; i < tags.Length; i++) {
                int numberOfUses = entriesList.Count(x => x.data.tags.Contains(tags[i]));
                if(numberOfUses == 0) {
                    Filter filter = filters.First(x => x.tag == tags[i]);
                    filtersList.Remove(filter);
                    filters = filtersList.ToArray();
                    int tagHashIndex = (int)Math.Log(filter.hash, 2);
                    tagHashesUsed[tagHashIndex] = false;
                    if (onFilterRemoved != null)
                        onFilterRemoved(filter);
                }
            }
        }

        void RemoveGroup(EntryGroup group) {
            groupsList.Remove(group);
            groups = groupsList.ToArray();
        }

        void RemoveEntry(Entry entry) {
            RemoveEntryAlone(entry);
            entry.group.Remove(entry);
            if(entry.group.entries.Length == 0)
                RemoveGroup(entry.group);
        }

        public void AddFilters(params string[] tags) {
            foreach(string tag in tags)
                AddFilter(tag);
        }

        void AddFilter(string tag) {
            bool alreadyExists = false;
            for(int i = 0; i < filters.Length; i++) {
                if(filters[i].tag == tag) {
                    alreadyExists = true;
                    break;
                }
            }
            if(!alreadyExists) {
                for(int i = 0; i < tagHashesUsed.Length; i++) {
                    if(tagHashesUsed[i])
                        continue;
                    Filter filter = new Filter(tag, (int)Mathf.Pow(2, i));
                    filtersList.Add(filter);
                    filters = filtersList.ToArray();
                    tagHashesUsed[i] = true;
                    if(onFilterAdded != null)
                        onFilterAdded(filter);
                    break;
                }
            }
        }

        public void SetFilter(string tag, bool on) {
            for(int i = 0; i < filters.Length; i++) {
                if(filters[i].tag == tag) {
                    filters[i].isActive = on;
                    return;
                }
            }
        }

        public Filter[] GetFilters() {
            return filters;
        }
    }

    [Serializable]
    class EntryDrawData {
        public int tagsHash;
        public float height;
        public bool justCreated;
    }
}