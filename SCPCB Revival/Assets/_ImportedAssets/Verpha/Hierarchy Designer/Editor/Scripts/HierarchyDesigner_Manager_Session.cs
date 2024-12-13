#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Verpha.HierarchyDesigner
{
    internal class HierarchyDesigner_Manager_Session : ScriptableSingleton<HierarchyDesigner_Manager_Session>
    {
        #region Properties
        #region HierarchyDesigner_Window_Main
        public bool IsPatchNotesLoaded = false;
        public string PatchNotesContent = string.Empty;
        public HierarchyDesigner_Window_Main.CurrentWindow currentWindow = HierarchyDesigner_Window_Main.CurrentWindow.Home;
        #endregion

        #region HierarchyDesigner_Shared_Texture
        public Font FallbackFont = null;
        public Texture2D FallbackTexture = null;
        #endregion
        #endregion
    }
}
#endif