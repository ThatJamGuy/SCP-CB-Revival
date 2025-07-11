#if UNITY_EDITOR && SORTIFY
using System.Collections.Generic;
using UnityEngine;

namespace Sortify
{
    public class SortifyNoteDatabase : ScriptableObject
    {
        public List<NoteEntry> notes = new List<NoteEntry>();
    }

    [System.Serializable]
    public class NoteEntry
    {
        public string objID;
        public string noteContent;
        public List<Comment> comments = new List<Comment>();

        public NoteEntry(string objID, string noteContent)
        {
            this.objID = objID;
            this.noteContent = noteContent;
        }
    }

    [System.Serializable]
    public class Comment
    {
        public string userName;
        public string text;
        public string timestamp;

        public Comment(string userName, string text)
        {
            this.userName = userName;
            this.text = text;
            this.timestamp = System.DateTime.Now.ToString("yyyy-MM-dd");
        }
    }
}
#endif
