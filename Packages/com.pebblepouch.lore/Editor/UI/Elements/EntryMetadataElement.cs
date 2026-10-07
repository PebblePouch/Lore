using System;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class EntryMetadataElement
    {
        #region Fields
        private readonly EntryService _entryService = new();

        private readonly LoreInlineTextField _titleEditor = new();
        private readonly LoreInlineTextField _subtitleEditor = new();

        private bool _wasEditingTitle;
        private bool _wasEditingSubtitle;

        private Entry _entry;

        private Rect _titleScreenRect;
        private Rect _subtitleScreenRect;

        private string _titleFeedback = string.Empty;

        private Action _requestedTitleEditingEnded;
        #endregion

        #region Properties
        public bool IsEditing => _titleEditor.IsEditing || _subtitleEditor.IsEditing;
        #endregion

        #region Public API
        public void CommitEditing()
        {
            if (_titleEditor.IsEditing)
            {
                CommitTitle();
            }

            if (_subtitleEditor.IsEditing)
            {
                CommitSubtitle();
            }
        }

        public void SetEntry(
            Entry entry)
        {
            if (_entry != entry)
            {
                _titleEditor.StopEditing();
                _subtitleEditor.StopEditing();

                _wasEditingTitle = false;
                _wasEditingSubtitle = false;

                _titleFeedback =
                    string.Empty;
            }

            _entry =
                entry;
        }

        public void Draw()
        {
            if (_entry == null)
                return;

            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));

            GUILayout.Space(InspectorStyle.EntryTitleTopSpacing);

            DrawTitle();
            DrawSubtitle();

            EditorGUILayout.EndVertical();
        }

        public void DrawFeedback()
        {
            if (string.IsNullOrWhiteSpace(
                    _titleFeedback))
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
                _titleFeedback,
                InspectorStyle.Feedback);
        }
        #endregion

        #region Draw
        private void DrawTitle()
        {
            if (InspectorEditRequestService
                .ConsumeEntryTitle(
                    _entry,
                    out Action editingEnded))
            {
                _requestedTitleEditingEnded = editingEnded;

                BeginTitleEditing();
            }

            Rect rect;

            string newTitle = _titleEditor.Draw(
                out rect,
                _entry.Title,
                LoreText.Inspector.EntryTitlePlaceholder,
                InspectorStyle.EntryTitle,
                $"EntryTitle_{_entry.Id}");

            _titleScreenRect = LoreScreenRect.FromGUI(rect, _titleScreenRect);

            bool isEditing = _titleEditor.IsEditing;

            if (_wasEditingTitle && !isEditing)
            {
                if (CommitTitle(newTitle))
                {
                    CompleteRequestedTitleEditing();
                }
            }

            _wasEditingTitle = isEditing;
        }

        private void DrawSubtitle()
        {
            Rect rect;

            string newSubtitle = _subtitleEditor.Draw(
                out rect,
                _entry.Subtitle,
                LoreText.Inspector.SubtitlePlaceholder,
                InspectorStyle.Subtitle,
                $"EntrySubtitle_{_entry.Id}");

            _subtitleScreenRect = LoreScreenRect.FromGUI(rect, _subtitleScreenRect);

            bool isEditing = _subtitleEditor.IsEditing;

            if (_wasEditingSubtitle && !isEditing)
            {
                if (newSubtitle != _entry.Subtitle)
                {
                    _entryService.UpdateSubtitle(_entry, newSubtitle);
                }
            }

            _wasEditingSubtitle = isEditing;
        }
        #endregion

        #region Commands
        public void BeginTitleEditing()
        {
            if (_entry == null)
                return;

            _titleEditor.BeginEditing(
                _entry.Title);
        }

        public bool HandleOutsideClick(
            Vector2 mousePosition)
        {
            if (_titleEditor.IsEditing &&
                !LoreScreenRect.Contains(_titleScreenRect,
                    mousePosition))
            {
                if (!CommitTitle())
                    return false;

                CompleteRequestedTitleEditing();
            }

            if (_subtitleEditor.IsEditing &&
                !LoreScreenRect.Contains(_subtitleScreenRect,
                    mousePosition))
            {
                CommitSubtitle();
            }

            return true;
        }
        #endregion

        #region Helpers
        private bool CommitTitle(string newTitle = null)
        {
            if (_entry == null)
                return true;

            newTitle ??= _titleEditor.EditingText;

            if (newTitle == _entry.Title)
            {
                _titleFeedback = string.Empty;

                _titleEditor.StopEditing();

                _wasEditingTitle = false;

                return true;
            }

            LoreOperationResult validation =
                _entryService.ValidateRename(
                    _entry,
                    newTitle);

            if (!validation.Success)
            {
                _titleFeedback = validation.Message;

                _titleEditor.ContinueEditing(newTitle);

                _wasEditingTitle = true;

                LoreRepaintService.Repaint();

                return false;
            }

            _titleFeedback = string.Empty;

            if (!_entryService.RenameEntry(
                    _entry,
                    newTitle))
            {
                return false;
            }

            _titleEditor.StopEditing();

            _wasEditingTitle = false;

            SelectionService.SelectEntry(_entry);

            LoreRepaintService.Repaint();

            return true;
        }

        private void CommitSubtitle()
        {
            if (_entry == null)
                return;

            if (!_subtitleEditor.IsEditing)
                return;

            string newSubtitle = _subtitleEditor.EditingText;

            if (newSubtitle != _entry.Subtitle)
            {
                _entryService.UpdateSubtitle(
                    _entry,
                    newSubtitle);
            }

            _subtitleEditor.StopEditing();

            _wasEditingSubtitle = false;
        }

        private void CompleteRequestedTitleEditing()
        {
            Action editingEnded = _requestedTitleEditingEnded;

            _requestedTitleEditingEnded = null;

            editingEnded?.Invoke();
        }
        #endregion
    }
}