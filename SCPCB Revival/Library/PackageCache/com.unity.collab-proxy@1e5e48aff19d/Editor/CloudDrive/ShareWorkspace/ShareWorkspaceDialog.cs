using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using Codice.CM.Common;
using PlasticGui;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.Progress;

namespace Unity.PlasticSCM.Editor.CloudDrive.ShareWorkspace
{
    internal class ShareWorkspaceDialog : PlasticDialog
    {
        protected override Rect DefaultRect
        {
            get
            {
                var baseRect = base.DefaultRect;
                return new Rect(baseRect.x, baseRect.y, 750, 450);
            }
        }

        internal static bool ShareWorkspace(
            WorkspaceInfo workspaceInfo,
            EditorWindow parentWindow,
            out List<SecurityMember> collaboratorsToAdd,
            out List<SecurityMember> collaboratorsToRemove)
        {
            ShareWorkspaceDialog dialog = Create(
                workspaceInfo,
                parentWindow.Repaint,
                new ProgressControlsForDialogs());

            ResponseType dialogResult = dialog.RunModal(parentWindow);

            collaboratorsToAdd = mShareWorkspacePanel.GetCollaboratorsToAdd();
            collaboratorsToRemove = mShareWorkspacePanel.GetCollaboratorsToRemove();
            return dialogResult == ResponseType.Ok;
        }

        static ShareWorkspaceDialog Create(
            WorkspaceInfo workspaceInfo,
            Action repaintAction,
            ProgressControlsForDialogs progressControls)
        {
            var instance = CreateInstance<ShareWorkspaceDialog>();
            instance.IsResizable = false;

            mShareWorkspacePanel = new ShareWorkspacePanel(
                workspaceInfo, progressControls, repaintAction);

            RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(workspaceInfo);

            mShareWorkspacePanel.Refresh(
               repSpec.Server,
               CloudProjectRepository.GetProjectName(repSpec.Name));

            instance.mEnterKeyAction = instance.OkButtonWithValidationAction;
            instance.mEscapeKeyAction = instance.CancelButtonAction;
            instance.mProgressControls = progressControls;
            return instance;
        }

        protected override string GetTitle()
        {
            return PlasticLocalization.Name.ShareWorkspaceDialogTitle.GetString();
        }

        protected override void OnModalGUI()
        {
            mShareWorkspacePanel.OnGUI(mProgressControls.ProgressData.IsWaitingAsyncResult);

            GUILayout.Space(10f);

            DrawProgressForDialogs.For(mProgressControls.ProgressData);

            GUILayout.Space(10f);

            DoButtonsArea();
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
            if (!AcceptButton(PlasticLocalization.Name.ShareButton.GetString()))
                return;

            OkButtonWithValidationAction();
        }

        void DoCancelButton()
        {
            if (!NormalButton(PlasticLocalization.Name.CancelButton.GetString()))
                return;

            CancelButtonAction();
        }

        void OkButtonWithValidationAction()
        {
            if (mShareWorkspacePanel.GetCollaboratorsToAdd().Count == 0 &&
                mShareWorkspacePanel.GetCollaboratorsToRemove().Count == 0)
            {
                ((IProgressControls)mProgressControls).ShowError(
                    PlasticLocalization.Name.CollaboratorsUnchanged.GetString());
                return;
            }

            OkButtonAction();
        }

        static ShareWorkspacePanel mShareWorkspacePanel;

        ProgressControlsForDialogs mProgressControls;
    }
}
