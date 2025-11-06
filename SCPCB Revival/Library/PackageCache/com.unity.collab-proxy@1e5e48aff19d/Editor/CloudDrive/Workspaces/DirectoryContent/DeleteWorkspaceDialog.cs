using UnityEditor;
using UnityEngine;

using PlasticGui;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.Progress;

namespace Unity.PlasticSCM.Editor.CloudDrive.Workspaces.DirectoryContent
{
    internal class DeleteWorkspaceDialog : PlasticDialog
    {
        protected override Rect DefaultRect
        {
            get
            {
                var baseRect = base.DefaultRect;
                return new Rect(baseRect.x, baseRect.y, 500, 200);
            }
        }

        internal static bool UserConfirmsDelete(
            string workspaceName,
            EditorWindow parentWindow)
        {
            DeleteWorkspaceDialog dialog = Create(
                workspaceName,
                new ProgressControlsForDialogs());

            ResponseType dialogResult = dialog.RunModal(parentWindow);

            return dialogResult == ResponseType.Ok;
        }

        static DeleteWorkspaceDialog Create(
            string workspaceName,
            ProgressControlsForDialogs progressControls)
        {
            var instance = CreateInstance<DeleteWorkspaceDialog>();

            instance.mWorkspaceName = workspaceName;
            instance.mConfirmationText = string.Empty;
            instance.mProgressControls = progressControls;

            instance.mEnterKeyAction = instance.OkButtonWithValidationAction;
            instance.mEscapeKeyAction = instance.CancelButtonAction;

            return instance;
        }

        protected override string GetTitle()
        {
            return PlasticLocalization.Name.DeleteCloudDriveTitle.GetString();
        }

        protected override void OnModalGUI()
        {
            Title(GetTitle());

            GUILayout.Space(10f);

            DoWarningArea();

            GUILayout.Space(10f);

            DoConfirmationArea();

            GUILayout.Space(10f);

            DrawProgressForDialogs.For(mProgressControls.ProgressData);

            GUILayout.Space(10f);

            DoButtonsArea();
        }

        void DoWarningArea()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField(
                    string.Format(PlasticLocalization.Name.DeleteCloudDriveWarningText.GetString(), mWorkspaceName),
                    EditorStyles.wordWrappedLabel);
            }
        }

        void DoConfirmationArea()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                string confirmText = PlasticLocalization.Name.DeleteCloudDriveConfirmText.GetString();
                EditorGUILayout.LabelField(confirmText, GUILayout.Width(220));

                GUILayout.Space(10f);

                GUI.SetNextControlName(CONFIRMATION_TEXTAREA_NAME);

                mConfirmationText = GUILayout.TextField(
                    mConfirmationText,
                    GUILayout.ExpandWidth(true));

                if (!mTextAreaFocused)
                {
                    EditorGUI.FocusTextInControl(CONFIRMATION_TEXTAREA_NAME);
                    mTextAreaFocused = true;
                }
            }
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
            using (new GuiEnabled(mWorkspaceName == mConfirmationText))
            {
                if (!AcceptButton(PlasticLocalization.Name.DeleteButton.GetString()))
                    return;

                OkButtonWithValidationAction();
            }
        }

        void OkButtonWithValidationAction()
        {
            if (mWorkspaceName != mConfirmationText)
                return;

            OkButtonAction();
        }

        void DoCancelButton()
        {
            if (!NormalButton(PlasticLocalization.GetString(
                    PlasticLocalization.Name.CancelButton)))
                return;

            CancelButtonAction();
        }

        bool mTextAreaFocused;

        string mWorkspaceName;
        string mConfirmationText;

        ProgressControlsForDialogs mProgressControls;

        const string CONFIRMATION_TEXTAREA_NAME = "confirmation_textarea";
    }
}
