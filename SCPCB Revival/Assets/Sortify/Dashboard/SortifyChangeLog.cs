#if UNITY_EDITOR
using UnityEngine;

namespace Sortify
{
    [System.Serializable]
    public class ChangelogEntry
    {
        public string version;

        public int year;
        public int month;
        public int day;

        [TextArea(3, 10)]
        public string details;

        public System.DateTime releaseDate
        {
            get => new System.DateTime(year, month, day);
        }
    }

    public class SortifyChangeLog : ScriptableObject
    {
        [SerializeField] private ChangelogEntry[] _changelogEntries;
        public ChangelogEntry[] ChangelogEntries
        {
            get => _changelogEntries;
            set => _changelogEntries = value;
        }
    }
}
#endif
