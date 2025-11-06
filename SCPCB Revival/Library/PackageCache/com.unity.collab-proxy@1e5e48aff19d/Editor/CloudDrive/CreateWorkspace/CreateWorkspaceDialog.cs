using System;

using UnityEditor;
using UnityEngine;

using Codice.Client.Common.WebApi;
using Codice.CM.Common;
using PlasticGui;
using PlasticGui.CloudDrive.Workspaces;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.Progress;

namespace Unity.PlasticSCM.Editor.CloudDrive.CreateWorkspace
{
    class CreateWorkspaceDialog : PlasticDialog, IPlasticDialogCloser
    {
        protected override Rect DefaultRect
        {
            get
            {
                var baseRect = base.DefaultRect;
                return new Rect(baseRect.x, baseRect.y, 850, 560);
            }
        }

        internal static WorkspaceCreationData CreateWorkspace(
            string proposedOrganization,
            string proposedProject,
            IPlasticWebRestApi restApi,
            IPlasticAPI plasticApi,
            EditorWindow parentWindow)
        {
            CreateWorkspaceDialog dialog = Create(
                proposedOrganization,
                proposedProject,
                restApi,
                plasticApi,
                parentWindow,
                null,
                new ProgressControlsForDialogs());

            ResponseType dialogResult = dialog.RunModal(parentWindow);

            WorkspaceCreationData result = mCreateWorkspacePanel.BuildCreationData();
            result.Result = dialogResult == ResponseType.Ok;
            return result;
        }

        internal static void CreateWorkspace(
            IPlasticWebRestApi restApi,
            IPlasticAPI plasticApi,
            EditorWindow parentWindow,
            Action<WorkspaceInfo> successOperationDelegate)
        {
            CreateWorkspaceDialog dialog = Create(
                string.Empty,
                string.Empty,
                restApi,
                plasticApi,
                parentWindow,
                successOperationDelegate,
                new ProgressControlsForDialogs());

            dialog.RunModal(parentWindow);
        }

        void IPlasticDialogCloser.CloseDialog()
        {
            if (mSuccessOperationDelegate == null)
            {
                OkButtonAction();
                return;
            }

            WorkspaceCreationData wkCreationData = mCreateWorkspacePanel.BuildCreationData();
            wkCreationData.Result = true;

            CreateWorkspaceOperation.CreateWorkspace(
                wkCreationData,
                PlasticGui.Plastic.API,
                mProgressControls,
                (createdWorkspace) =>
                {
                    OkButtonAction();
                    mSuccessOperationDelegate(createdWorkspace);
                });
        }

        static CreateWorkspaceDialog Create(
            string proposedOrganization,
            string proposedProject,
            IPlasticWebRestApi restApi,
            IPlasticAPI plasticApi,
            EditorWindow parentWindow,
            Action<WorkspaceInfo> successOperationDelegate,
            ProgressControlsForDialogs progressControls)
        {
            var instance = CreateInstance<CreateWorkspaceDialog>();
            instance.IsResizable = false;

            mCreateWorkspacePanel = new CreateWorkspacePanel(
                proposedOrganization,
                proposedProject,
                restApi,
                plasticApi,
                parentWindow,
                progressControls);

            instance.mEnterKeyAction = instance.OkButtonWithValidationAction;
            instance.mEscapeKeyAction = instance.CancelButtonAction;
            instance.mTitle = PlasticLocalization.Name.CreateCloudWorkspaceTitle.GetString();
            instance.mProgressControls = progressControls;
            instance.mSuccessOperationDelegate = successOperationDelegate;
            return instance;
        }

        protected override string GetTitle()
        {
            return mTitle;
        }

        protected override void OnModalGUI()
        {
            bool isOperationRunning = mProgressControls.ProgressData.IsWaitingAsyncResult;

            mCreateWorkspacePanel.OnGUI(isOperationRunning);

            DrawProgressForDialogs.For(mProgressControls.ProgressData);

            GUILayout.Space(10f);

            DoButtonsArea(isOperationRunning);
        }

        void DoButtonsArea(bool isOperationRunning)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    DoOkButton(isOperationRunning);
                    DoCancelButton();
                    return;
                }

                DoCancelButton();
                DoOkButton(isOperationRunning);
            }
        }

        void DoOkButton(bool isOperationRunning)
        {
            if (isOperationRunning)
                GUI.enabled = false;

            if (AcceptButton(PlasticLocalization.Name.CreateButton.GetString()))
            {
                OkButtonWithValidationAction();
            }

            GUI.enabled = true;
        }

        void DoCancelButton()
        {
            if (!NormalButton(PlasticLocalization.GetString(
                    PlasticLocalization.Name.CancelButton)))
                return;

            CancelButtonAction();
        }

        void OkButtonWithValidationAction()
        {
            WorkspaceCreationValidation.AsyncValidation(
                mCreateWorkspacePanel.BuildCreationData(), this, mProgressControls);
        }

        string mTitle;
        ProgressControlsForDialogs mProgressControls;
        Action<WorkspaceInfo> mSuccessOperationDelegate;

        static CreateWorkspacePanel mCreateWorkspacePanel;
    }
}
