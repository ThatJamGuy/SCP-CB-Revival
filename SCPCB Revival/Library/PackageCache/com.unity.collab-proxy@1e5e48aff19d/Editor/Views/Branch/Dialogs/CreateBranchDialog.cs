using UnityEditor;
using UnityEngine;

using Codice.Client.Common;
using Codice.CM.Common;
using PlasticGui;
using PlasticGui.WorkspaceWindow;
using PlasticGui.WorkspaceWindow.QueryViews.Branches;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.Progress;

namespace Unity.PlasticSCM.Editor.Views.Branches.Dialogs
{
    class CreateBranchDialog : PlasticDialog
    {
        internal BranchInfo SelectedBranchBase { get { return mSelectedBaseBranch; } }
        internal string NewBranchName { get { return mNewBranchName; } set { mNewBranchName = value; } }
        internal bool AllowToChangeBaseBranch { get { return mAllowToChangeBaseBranch; } }

        protected override Rect DefaultRect
        {
            get
            {
                var baseRect = base.DefaultRect;
                return new Rect(baseRect.x, baseRect.y, 710, 290);
            }
        }

        protected override string GetTitle()
        {
            return PlasticLocalization.Name.CreateChildBranchTitle.GetString();
        }

        protected override void OnModalGUI()
        {
            DoTitleArea();

            DoFieldsArea();

            DoButtonsArea();
        }

        internal static BranchCreationData CreateBranchFromMainOrCurrentBranch(
            EditorWindow parentWindow,
            RepositorySpec repSpec,
            BranchInfo mainBranch,
            BranchInfo currentBranch,
            string proposedBranchName)
        {
            string changesetStr = PlasticLocalization.Name.LastChangeset.GetString();

            string explanation = BranchCreationUserInfo.GetFromObjectString(
                repSpec, currentBranch, changesetStr);

            return CreateBranch(
                parentWindow,
                repSpec,
                mainBranch,
                mainBranch,
                currentBranch,
                -1,
                explanation,
                true,
                proposedBranchName);
        }

        internal static BranchCreationData CreateBranchFromLastParentBranchChangeset(
            EditorWindow parentWindow,
            RepositorySpec repSpec,
            BranchInfo parentBranchInfo,
            string proposedBranchName)
        {
            string changesetStr = PlasticLocalization.Name.LastChangeset.GetString();

            string explanation = BranchCreationUserInfo.GetFromObjectString(
                repSpec, parentBranchInfo, changesetStr);

            return CreateBranch(
                parentWindow,
                repSpec,
                parentBranchInfo,
                null,
                null,
                -1,
                explanation,
                false,
                proposedBranchName);
        }

        internal static BranchCreationData CreateBranchFromChangeset(
            EditorWindow parentWindow,
            RepositorySpec repSpec,
            ChangesetExtendedInfo changesetInfo,
            string proposedBranchName)
        {
            BranchInfo parentBranchInfo = BranchInfoCache.GetBranch(
                repSpec, changesetInfo.BranchId);

            string changesetStr = SpecPreffix.CHANGESET + changesetInfo.ChangesetId.ToString();

            string explanation = BranchCreationUserInfo.GetFromObjectString(
                repSpec, parentBranchInfo, changesetStr);

            return CreateBranch(
                parentWindow,
                repSpec,
                parentBranchInfo,
                null,
                null,
                changesetInfo.ChangesetId,
                explanation,
                false,
                proposedBranchName);
        }

        internal static BranchCreationData CreateBranchFromLabel(
            EditorWindow parentWindow,
            RepositorySpec repSpec,
            MarkerExtendedInfo labelInfo)
        {
            BranchInfo parentBranchInfo = BranchInfoCache.GetBranch(
                repSpec, labelInfo.BranchId);

            string explanation = BranchCreationUserInfo.GetFromObjectString(
                repSpec, labelInfo);

            return CreateBranch(
                parentWindow,
                repSpec,
                parentBranchInfo,
                null,
                null,
                labelInfo.Changeset,
                explanation,
                false,
                null);
        }

        internal void CheckMainBranchRadioToggle()
        {
            mIsMainBranchSelected = true;
            mIsCurrentBranchSelected = false;
            mSelectedBaseBranch = mMainBranch;
        }

        internal void CheckCurrentBranchRadioToggle()
        {
            mIsMainBranchSelected = false;
            mIsCurrentBranchSelected = true;
            mSelectedBaseBranch = mCurrentBranch;
        }

        static BranchCreationData CreateBranch(
            EditorWindow parentWindow,
            RepositorySpec repSpec,
            BranchInfo baseBranch,
            BranchInfo mainBranch,
            BranchInfo currentBranch,
            long changesetId,
            string explanation,
            bool allowToChangeBaseBranch,
            string proposedBranchName)
        {
            CreateBranchDialog dialog = Create(
                repSpec,
                baseBranch,
                mainBranch,
                currentBranch,
                changesetId,
                explanation,
                allowToChangeBaseBranch,
                proposedBranchName);
            ResponseType dialogueResult = dialog.RunModal(parentWindow);

            BranchCreationData result = dialog.BuildCreationData();
            result.Result = dialogueResult == ResponseType.Ok;
            return result;
        }

        void DoTitleArea()
        {
            GUILayout.BeginVertical();

            string title = mAllowToChangeBaseBranch ?
                PlasticLocalization.Name.CreateChildBranchBasedOnTitle.GetString() :
                PlasticLocalization.Name.CreateChildBranchTitle.GetString();

            Title(title);

            GUILayout.Space(5);

            DoTopArea();

            GUILayout.EndVertical();
        }

        void DoTopArea()
        {
            if (mAllowToChangeBaseBranch)
            {
                DoBranchSelectionArea();
                return;
            }

            Paragraph(string.Format("{0} {1}",
                PlasticLocalization.Name.CreateChildBranchExplanation.GetString(), mExplanation));
        }

        void DoBranchSelectionArea()
        {
            int descriptionLabelLeftMargin = 25;

            GUILayout.BeginVertical();

            if (GUILayout.Toggle(
                mIsMainBranchSelected,
                GetBranchName.FromFullBranchName(mMainBranch.Name),
                UnityStyles.Dialog.BoldRadioToggle))
            {
                mIsMainBranchSelected = true;
                mIsCurrentBranchSelected = false;
                mSelectedBaseBranch = mMainBranch;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(descriptionLabelLeftMargin);
            GUILayout.Label(
                PlasticLocalization.Name.CreateChildBranchBasedOnMainExplanation.GetString(),
                EditorStyles.miniLabel);
            GUILayout.EndHorizontal();

            if (GUILayout.Toggle(
                    mIsCurrentBranchSelected,
                    GetBranchName.FromFullBranchName(mCurrentBranch.Name),
                    UnityStyles.Dialog.BoldRadioToggle))
            {
                mIsMainBranchSelected = false;
                mIsCurrentBranchSelected = true;
                mSelectedBaseBranch = mCurrentBranch;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(descriptionLabelLeftMargin);
            GUILayout.Label(
                PlasticLocalization.Name.CreateChildBranchBasedOnCurrentBranchExplanation.GetString(),
                EditorStyles.miniLabel);
            GUILayout.EndHorizontal();

            GUILayout.Space(15);

            GUILayout.EndVertical();
        }

        void DoFieldsArea()
        {
            GUILayout.BeginVertical();

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(
                    PlasticLocalization.Name.BranchNameEntry.GetString(),
                    GUILayout.Width(100));

                GUI.SetNextControlName(NAME_FIELD_CONTROL_NAME);
                mNewBranchName = GUILayout.TextField(mNewBranchName);

                if (!mWasNameFieldFocused)
                {
                    EditorGUI.FocusTextInControl(NAME_FIELD_CONTROL_NAME);
                    mWasNameFieldFocused = true;
                }

                GUILayout.Space(5);
            }

            GUILayout.Space(5);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope(GUILayout.Width(100)))
                {
                    GUILayout.Space(49);
                    GUILayout.Label(
                        PlasticLocalization.Name.CommentsEntry.GetString(),
                        GUILayout.Width(100));
                }
                GUI.SetNextControlName(COMMENT_TEXTAREA_CONTROL_NAME);
                mComment = GUILayout.TextArea(mComment, GUILayout.Height(100));
                GUILayout.Space(5);
            }

            GUILayout.Space(5);

            mSwitchToBranch = GUILayout.Toggle(mSwitchToBranch, PlasticLocalization.Name.SwitchToBranchCheckButton.GetString());

            GUILayout.Space(5);

            GUILayout.EndVertical();
        }

        void DoButtonsArea()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.HorizontalScope(GUILayout.MinWidth(500)))
                {
                    GUILayout.Space(2);
                    DrawProgressForDialogs.For(
                        mProgressControls.ProgressData);
                    GUILayout.Space(2);
                }

                GUILayout.FlexibleSpace();

                DoCreateButton();
                DoCancelButton();
            }
        }

        void DoCancelButton()
        {
            if (!NormalButton(PlasticLocalization.Name.CancelButton.GetString()))
                return;

            CancelButtonAction();
        }

        void DoCreateButton()
        {
            if (!NormalButton(PlasticLocalization.Name.CreateButton.GetString()))
                return;

            CreateButtonAction();
        }

        void CreateButtonAction()
        {
            BranchCreationValidation.AsyncValidation(
                BuildCreationData(), this, mProgressControls);
        }

        internal static CreateBranchDialog Create(
            RepositorySpec repSpec,
            BranchInfo baseBranch,
            BranchInfo mainBranch,
            BranchInfo currentBranch,
            long changesetId,
            string explanation,
            bool allowToChangeBaseBranch,
            string proposedBranchName)
        {
            CreateBranchDialogAssertions.AssertValidBaseBranchChangeArguments(
                mainBranch,
                currentBranch,
                allowToChangeBaseBranch);

            if (allowToChangeBaseBranch && currentBranch.IsMainBranch())
                allowToChangeBaseBranch = false;

            var instance = CreateInstance<CreateBranchDialog>();
            instance.IsResizable = false;
            instance.mEnterKeyAction = instance.CreateButtonAction;
            instance.AddControlConsumingEnterKey(COMMENT_TEXTAREA_CONTROL_NAME);
            instance.mEscapeKeyAction = instance.CloseButtonAction;
            instance.mRepositorySpec = repSpec;
            instance.mSelectedBaseBranch = baseBranch;
            instance.mMainBranch = mainBranch;
            instance.mCurrentBranch = currentBranch;
            instance.mAllowToChangeBaseBranch = allowToChangeBaseBranch;
            instance.mNewBranchName = proposedBranchName == null ?
                string.Empty : proposedBranchName;
            instance.mComment = "";
            instance.mSwitchToBranch = true;
            instance.mProgressControls = new ProgressControlsForDialogs();
            instance.mExplanation = explanation;
            instance.mChangesetId = changesetId;
            return instance;
        }

        BranchCreationData BuildCreationData()
        {
            return new BranchCreationData(
                mRepositorySpec,
                mSelectedBaseBranch,
                mChangesetId,
                mNewBranchName,
                mComment,
                null,
                mSwitchToBranch);
        }

        ProgressControlsForDialogs mProgressControls;

        RepositorySpec mRepositorySpec;
        BranchInfo mSelectedBaseBranch;
        BranchInfo mMainBranch;
        BranchInfo mCurrentBranch;
        bool mAllowToChangeBaseBranch;
        long mChangesetId;

        string mNewBranchName;
        string mComment;
        bool mSwitchToBranch;
        string mExplanation;

        bool mWasNameFieldFocused;
        bool mIsMainBranchSelected = true;
        bool mIsCurrentBranchSelected;
        const string NAME_FIELD_CONTROL_NAME = "CreateBranchNameField";
        const string COMMENT_TEXTAREA_CONTROL_NAME = "CreateBranchCommentTextArea";
    }
}
