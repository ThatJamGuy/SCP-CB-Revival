/*
 * Save Manager (3.x)
 * Copyright (c) 2025-2026 Carter Games
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the
 * GNU General Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version. 
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 * FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. 
 *
 * You should have received a copy of the GNU General Public License along with this program.
 * If not, see <https://www.gnu.org/licenses/>. 
 */

using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.UnityLinker;

namespace CarterGames.Assets.SaveManager.Editor
{
    /// <summary>
    /// A way to get a link.xml to work when in a package... fun...
    /// Used example from this web thread: https://discussions.unity.com/t/the-current-state-of-link-xml-in-packages/814559/7
    /// </summary>
    public class PreserveHandler : IUnityLinkerProcessor
    {
        public int callbackOrder => 0;
        
        public string GenerateAdditionalLinkXmlFile(BuildReport report, UnityLinkerBuildPipelineData data)
        {
            const string linkXmlGuid = "49c7c8a3ca9a43e9b689ce1de97f7daf"; // copied from link.xml.meta
            var assetPath = AssetDatabase.GUIDToAssetPath(linkXmlGuid);
            
            // assets paths are relative to the unity project root, but they don't correspond to actual folders for
            // Packages that are embedded. I.e. it won't work if a package is installed as a git submodule
            // So resolve it to an absolute path:
            return Path.GetFullPath(assetPath);
        }

        public void OnBeforeRun(BuildReport report, UnityLinkerBuildPipelineData data) { }

        public void OnAfterRun(BuildReport report, UnityLinkerBuildPipelineData data) { }
    }
}