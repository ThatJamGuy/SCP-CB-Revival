using System.IO;

using UnityEditor;
using UnityEngine;

using PlasticGui;
using Unity.PlasticSCM.Editor.AssetUtils;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.Progress;

namespace Unity.PlasticSCM.Editor.CloudDrive.Workspaces.DirectoryContent
{
    internal class ImportInProjectDialog : PlasticDialog
    {
        protected override Rect DefaultRect
        {
            get
            {
                var baseRect = base.DefaultRect;
                return new Rect(baseRect.x, baseRect.y, 600, 200);
            }
        }

        internal static string GetProjectPathToImport(
            string currentCloudDrivePath,
            string workspacePath,
            EditorWindow parentWindow)
        {
            ImportInProjectDialog dialog = Create(currentCloudDrivePath, workspacePath);

            ResponseType dialogResult = dialog.RunModal(parentWindow);

            if (dialogResult != ResponseType.Ok)
                return string.Empty;

            return dialog.mRelativeProjectPath;
        }

        protected override string GetTitle()
        {
            return PlasticLocalization.Name.ImportInProjectDialogTitle.GetString();
        }

        protected override void OnModalGUI()
        {
            Title(PlasticLocalization.Name.ImportInProjectDialogTitle.GetString());

            GUILayout.Space(10);

            Paragraph(PlasticLocalization.Name.ImportInProjectDialogExplanation.GetString());

            GUILayout.Space(10);

            DoEntriesArea();

            GUILayout.Space(10);

            DrawProgressForDialogs.For(mProgressControls.ProgressData);

            GUILayout.Space(10);

            GUILayout.FlexibleSpace();

            DoButtonsArea();
        }

        static ImportInProjectDialog Create(string currentCloudDrivePath, string workspacePath)
        {
            var instance = CreateInstance<ImportInProjectDialog>();
            instance.mProgressControls = new ProgressControlsForDialogs();
            instance.IsResizable = false;

            instance.mRelativeProjectPath = GetProposedProjectPath(
                currentCloudDrivePath, workspacePath);

            instance.mEnterKeyAction = instance.OkButtonWithValidationAction;
            instance.mEscapeKeyAction = instance.CancelButtonAction;

            return instance;
        }

        void DoEntriesArea()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                mRelativeProjectPath = EntryBuilder.CreateTextEntry(
                    PlasticLocalization.Name.ProjectPathLabel.GetString(),
                    mRelativeProjectPath,
                    ENTRY_WIDTH - BROWSE_BUTTON_WIDTH,
                    ENTRY_X);

                Rect browseButtonRect = new Rect(
                    ENTRY_X + ENTRY_WIDTH - BROWSE_BUTTON_WIDTH + BUTTON_MARGIN,
                    GUILayoutUtility.GetLastRect().y,
                    BROWSE_BUTTON_WIDTH - BUTTON_MARGIN,
                    20);

                if (GUI.Button(browseButtonRect, "..."))
                    DoBrowseForPath();
            }
        }

        void DoBrowseForPath()
        {
            string projectPath = Path.GetFullPath(ProjectPath.Get());

            string selectedPath = Path.GetFullPath(EditorUtility.SaveFolderPanel(
                PlasticLocalization.Name.SelectProjectPathDialogTitle.GetString(),
                projectPath,
                ""));

            if (string.IsNullOrEmpty(selectedPath))
                return;

            if (!selectedPath.StartsWith(projectPath))
            {
                ((IProgressControls)mProgressControls).ShowError(
                    PlasticLocalization.Name.PathMustBeInsideProjectFolder.GetString());
                return;
            }

            ((IProgressControls)mProgressControls).HideProgress();

            if (selectedPath == projectPath)
            {
                mRelativeProjectPath = "/";
                return;
            }

            mRelativeProjectPath = selectedPath.Substring(projectPath.Length).Replace('\\', '/');
        }

        void DoButtonsArea()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    DoOkButton();
                    DoCancelButton();
                    return;
                }

                DoCancelButton();
                DoOkButton();
            }
        }

        void DoOkButton()
        {
            if (AcceptButton(PlasticLocalization.Name.ImportButton.GetString()))
            {
                OkButtonWithValidationAction();
            }
        }

        void OkButtonWithValidationAction()
        {
            if (!mRelativeProjectPath.StartsWith("/"))
            {
                ((IProgressControls)mProgressControls).ShowError(
                    PlasticLocalization.Name.PathMustStartWithSlash.GetString());
                return;
            }

            OkButtonAction();
        }

        void DoCancelButton()
        {
            if (NormalButton(PlasticLocalization.Name.CancelButton.GetString()))
            {
                CancelButtonAction();
            }
        }

        static string GetProposedProjectPath(string currentCloudDrivePath, string workspacePath)
        {
            string relativeProjectPath =
                currentCloudDrivePath.Substring(workspacePath.Length).Replace('\\', '/');

            if (string.IsNullOrEmpty(relativeProjectPath))
                return "/";

            return relativeProjectPath;
        }

        string mRelativeProjectPath;

        ProgressControlsForDialogs mProgressControls;

        const float ENTRY_WIDTH = 400;
        const float ENTRY_X = 120f;
        const float BROWSE_BUTTON_WIDTH = 30;
        const float BUTTON_MARGIN = 5;
    }
}
