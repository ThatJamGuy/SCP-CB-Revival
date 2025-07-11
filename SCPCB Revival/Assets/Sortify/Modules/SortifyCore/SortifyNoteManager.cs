#if UNITY_EDITOR && SORTIFY
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Sortify
{
    public static class SortifyNoteManager
    {
        private static SortifyNoteDatabase _noteDatabaseCache;

        public static NoteEntry GetNoteForGameObject(string objID)
        {
            LoadNoteDatabase();
            return _noteDatabaseCache.notes.FirstOrDefault(note => note.objID == objID);
        }

        public static void AddNoteToGameObject(string objID, string noteContent)
        {
            LoadNoteDatabase();
            if (GetNoteForGameObject(objID) == null)
            {
                NoteEntry note = new NoteEntry(objID, noteContent);
                _noteDatabaseCache.notes.Add(note);
                SaveNoteDatabase();
            }
        }

        public static void AddCommentToNote(string objID, Comment comment)
        {
            LoadNoteDatabase();
            NoteEntry note = GetNoteForGameObject(objID);
            if (note != null)
            {
                note.comments.Add(comment);
                SaveNoteDatabase();
            }
        }

        public static void UpdateNoteForGameObject(string objID, string updatedContent)
        {
            LoadNoteDatabase();
            NoteEntry note = GetNoteForGameObject(objID);
            if (note != null)
            {
                note.noteContent = updatedContent;
                SaveNoteDatabase();
            }
        }

        public static void RemoveNoteForGameObject(string objID)
        {
            LoadNoteDatabase();
            NoteEntry note = GetNoteForGameObject(objID);
            if (note != null)
            {
                _noteDatabaseCache.notes.Remove(note);
                SaveNoteDatabase();
            }
        }

        private static void LoadNoteDatabase()
        {
            if (_noteDatabaseCache == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:SortifyNoteDatabase");
                if (guids.Length > 0)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                    _noteDatabaseCache = AssetDatabase.LoadAssetAtPath<SortifyNoteDatabase>(assetPath);
                }

                if (_noteDatabaseCache == null)
                    CreateNewNoteDatabaseAsset();
            }
        }

        private static void CreateNewNoteDatabaseAsset()
        {
            string[] scriptGuids = AssetDatabase.FindAssets("SortifyNoteManager");
            string scriptPath = AssetDatabase.GUIDToAssetPath(scriptGuids.First());
            string folderPath = System.IO.Path.GetDirectoryName(scriptPath);

            SortifyNoteDatabase noteDatabase = ScriptableObject.CreateInstance<SortifyNoteDatabase>();
            string assetPath = $"{folderPath}/SortifyNoteDatabase.asset";
            AssetDatabase.CreateAsset(noteDatabase, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            _noteDatabaseCache = noteDatabase;
        }

        private static void SaveNoteDatabase()
        {
            if (_noteDatabaseCache != null)
            {
                EditorUtility.SetDirty(_noteDatabaseCache);
                AssetDatabase.SaveAssets();
            }
        }
    }
}
#endif
