#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Verpha.HierarchyDesigner
{
    internal static class HierarchyDesigner_Shared_File
    {
        #region Methods
        #region Path Management
        private static string GetOrCreateDirectory(string subFolderName)
        {
            string rootPath = GetHierarchyDesignerRootPath();
            string fullPath = Path.Combine(rootPath, HierarchyDesigner_Shared_Constants.EditorFolderName, subFolderName);

            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                AssetDatabase.Refresh();
            }

            return fullPath;
        }

        private static string GetHierarchyDesignerRootPath()
        {
            string[] guids = AssetDatabase.FindAssets(HierarchyDesigner_Shared_Constants.AssetName);
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Directory.Exists(path) && path.EndsWith(HierarchyDesigner_Shared_Constants.AssetName))
                {
                    return path;
                }
            }

            Debug.LogWarning($"Hierarchy Designer root path was not found. Defaulting to {Path.Combine(Application.dataPath, HierarchyDesigner_Shared_Constants.AssetName)}.");
            return Path.Combine(Application.dataPath, HierarchyDesigner_Shared_Constants.AssetName);
        }
        #endregion

        #region File Path Getters
        public static string GetSavedDataFilePath(string fileName)
        {
            string fullPath = GetOrCreateDirectory(HierarchyDesigner_Shared_Constants.SavedDataFolderName);
            return Path.Combine(fullPath, fileName);
        }

        public static string GetScriptsFilePath(string fileName)
        {
            string fullPath = GetOrCreateDirectory(HierarchyDesigner_Shared_Constants.ScriptsFolderName);
            return Path.Combine(fullPath, fileName);
        }

        private static string GetPatchNotesFilePath()
        {
            string fullPath = GetOrCreateDirectory(HierarchyDesigner_Shared_Constants.DocumentationFolderName);
            return Path.Combine(fullPath, HierarchyDesigner_Shared_Constants.PatchNotesTextFileName);
        }
        #endregion

        #region File Handling
        public static string GetPatchNotesData()
        {
            string filePath = GetPatchNotesFilePath();
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"Hierarchy Designer patch notes file not found at path: {filePath}");
                return "Hierarchy Designer patch notes file not found.";
            }

            return ReadFileWithLimit(filePath, 100);
        }

        private static string ReadFileWithLimit(string filePath, int maxLines)
        {
            try
            {
                StringBuilder fileContent = new();
                int lineCount = 0;

                using (StreamReader reader = new(filePath))
                {
                    while (!reader.EndOfStream && lineCount < maxLines)
                    {
                        string line = reader.ReadLine();
                        fileContent.AppendLine(line);
                        lineCount++;
                    }
                }

                if (lineCount == maxLines)
                {
                    fileContent.AppendLine("...more");
                }

                return fileContent.ToString();
            }
            catch (IOException e)
            {
                Debug.LogError($"Error reading file at {filePath}: {e.Message}");
                return "Error reading file.";
            }
        }
        #endregion
        #endregion
    }
}
#endif