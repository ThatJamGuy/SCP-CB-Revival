using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using Codice.CM.Common;
using PlasticGui;

namespace Unity.PlasticSCM.Editor.UI.Errors
{
    internal class ErrorsDialog : PlasticDialog
    {
        protected override Rect DefaultRect
        {
            get
            {
                var baseRect = base.DefaultRect;
                return new Rect(baseRect.x, baseRect.y, 810, 385);
            }
        }

        internal static void ShowDialog(
            string title,
            string explanation,
            List<ErrorMessage> errors,
            EditorWindow parentWindow)
        {
            ErrorsDialog dialog = Create(title, explanation, errors);
            dialog.RunModal(parentWindow);
        }

        protected override void OnModalGUI()
        {
            Title(mTitle);

            Paragraph(mExplanation);

            DoErrorsArea(mErrorsPanel);

            DoButtonsArea();
        }

        protected override string GetTitle()
        {
            return mTitle;
        }

        static void DoErrorsArea(ErrorsPanel errorsPanel)
        {
            errorsPanel.OnGUI();
        }

        void DoButtonsArea()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                DoCloseButton();
            }
        }

        void DoCloseButton()
        {
            if (!NormalButton(PlasticLocalization.Name.CloseButton.GetString()))
                return;

            CancelButtonAction();
        }

        static ErrorsDialog Create(
            string title,
            string explanation,
            List<ErrorMessage> errors)
        {
            var instance = CreateInstance<ErrorsDialog>();
            instance.mEscapeKeyAction = instance.CloseButtonAction;
            instance.mTitle = title;
            instance.mExplanation = explanation;
            instance.BuildComponents();
            instance.SetErrorsList(errors);
            return instance;
        }

        void SetErrorsList(List<ErrorMessage> errors)
        {
            mErrorsPanel.UpdateErrorsList(errors);
        }

        void BuildComponents()
        {
            mErrorsPanel = new ErrorsPanel(
                string.Empty,
                UnityConstants.CloudDrive.ERRORS_DIALOG_SETTINGS_NAME);
        }

        ErrorsPanel mErrorsPanel;
        string mTitle;
        string mExplanation;
    }
}
