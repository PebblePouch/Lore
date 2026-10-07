using System;

using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class CategoryTitleElement
    {
        #region Fields
        private readonly LoreInlineTextField _textField;
        private readonly CategoryService _categoryService;

        private Category _category;

        private bool _wasEditing;

        private Rect _lastScreenRect;

        private string _feedback = string.Empty;

        private Action _requestedEditingEnded;
        #endregion

        #region Properties
        public bool IsEditing => _textField.IsEditing;
        #endregion

        #region Constructor
        public CategoryTitleElement()
        {
            _textField = new LoreInlineTextField();
            _categoryService = new CategoryService();
        }
        #endregion

        #region Public API
        public void SetCategory(Category category)
        {
            if (_category != category)
            {
                _textField.StopEditing();

                _feedback = string.Empty;
            }

            _category = category;
        }

        public void BeginEditing()
        {
            if (_category == null)
                return;

            _textField.BeginEditing(
                _category.Title);
        }

        public void Draw()
        {
            DrawContent();
        }

        public void DrawFeedback()
        {
            if (string.IsNullOrWhiteSpace(
                    _feedback))
            {
                return;
            }

            Rect rect =
                GUILayoutUtility.GetRect(
                    0f,
                    InspectorStyle.FeedbackHeight,
                    GUILayout.ExpandWidth(true));

            Rect contentRect =
                new(
                    rect.x
                        + InspectorStyle.FeedbackHorizontalPadding,
                    rect.y
                        + InspectorStyle.FeedbackVerticalPadding,
                    Mathf.Max(
                        0f,
                        rect.width
                            - InspectorStyle.FeedbackHorizontalPadding * 2f),
                    Mathf.Max(
                        0f,
                        rect.height
                            - InspectorStyle.FeedbackVerticalPadding * 2f));

            GUI.Label(
                contentRect,
                _feedback,
                InspectorStyle.Feedback);
        }
        #endregion

        #region Draw
        private void DrawContent()
        {
            if (_category == null)
                return;

            if (InspectorEditRequestService
                .ConsumeCategoryTitle(
                    _category,
                    out Action editingEnded))
            {
                _requestedEditingEnded = editingEnded;

                BeginEditing();
            }

            Rect titleRect;

            string title = _textField.Draw(
                out titleRect,
                _category.Title,
                string.Empty,
                InspectorStyle.CategoryTitle);

            _lastScreenRect = LoreScreenRect.FromGUI(titleRect, _lastScreenRect);

            bool isEditing = _textField.IsEditing;

            if (_wasEditing && !isEditing)
            {
                if (CommitTitle(title))
                {
                    CompleteRequestedEditing();
                }
            }

            _wasEditing = isEditing;
        }
        #endregion

        #region Commands
        public bool HandleOutsideClick(
            Vector2 mousePosition)
        {
            if (!_textField.IsEditing)
                return true;

            if (LoreScreenRect.Contains(_lastScreenRect,
                    mousePosition))
            {
                return true;
            }

            bool committed = CommitTitle();

            if (committed)
            {
                CompleteRequestedEditing();
            }

            return committed;
        }
        #endregion

        #region Helpers
        private bool CommitTitle(
            string title = null)
        {
            if (_category == null)
                return true;

            title ??= _textField.EditingText;

            if (title == _category.Title)
            {
                _feedback = string.Empty;

                _textField.StopEditing();

                _wasEditing = false;

                return true;
            }

            LoreOperationResult validation =
                _categoryService.ValidateRename(
                    _category,
                    title);

            if (!validation.Success)
            {
                _feedback = validation.Message;

                _textField.ContinueEditing(title);

                _wasEditing = true;

                LoreRepaintService.Repaint();

                return false;
            }

            _feedback = string.Empty;

            if (!_categoryService.RenameCategory(
                    _category,
                    title))
            {
                return false;
            }

            _textField.StopEditing();

            _wasEditing = false;

            LoreRepaintService.Repaint();

            return true;
        }

        private void CompleteRequestedEditing()
        {
            Action editingEnded = _requestedEditingEnded;

            _requestedEditingEnded = null;

            editingEnded?.Invoke();
        }
        #endregion
    }
}