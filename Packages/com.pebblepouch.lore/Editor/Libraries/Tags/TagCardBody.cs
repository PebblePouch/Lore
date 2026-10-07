using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class TagCardBody
    {
        #region Fields
        private readonly TagLibraryService _tagService = new();
        private readonly TagDeleteAnalyzer _deleteAnalyzer = new();

        private readonly RelationshipQueryService _relationshipQueries = new(LoreRepository.Instance);

        private readonly Dictionary<TagDefinition, LibraryInlineTextField> _titleEditors = new();

        private readonly Dictionary<TagDefinition, TagCardEditState> _editStates = new();
        #endregion

        #region Callbacks
        public Action<TagDefinition> TagEditingEnded;
        public Action<TagDefinition> TagEditingCommitted;
        public Action<TagDefinition> TagFeedbackShown;

        public Action<TagLibraryItem> TagDeleteRequested;
        #endregion

        #region Public API
        public void Draw(
            Rect rect,
            TagLibraryItem item)
        {
            if (LibraryEditRequestService.ConsumeTag(item.Tag))
            {
                BeginEditing(item.Tag);
            }

            bool editing =
                LibraryEditingService.IsEditing(
                    item.EditingId);

            LibraryCardLayout layout =
                new(
                    rect,
                    false,
                    true);

            if (editing)
            {
                DrawEditable(
                    layout,
                    item);
            }
            else if (item.IsInEntry)
            {
                DrawInactiveViewer(
                    layout,
                    item);
            }
            else
            {
                DrawViewer(
                    layout,
                    item);
            }

            DrawMenu(
                layout.Menu,
                item);
        }

        public void ShowReferences(TagDefinition tag)
        {
            if (tag == null)
                return;

            IReadOnlyList<Entry> entries =
                _relationshipQueries.GetEntriesUsingTag(
                    tag);

            ContentBrowserRequestService.RequestEntryFilter(
                entry =>
                    entry != null &&
                    entry.TagIds.Contains(
                        tag.Id),
                string.Format(LoreText.Library.ReferencesFilter, tag.Title));

            LibraryService.CloseLibrary?.Invoke();

            LoreRepaintService.Repaint();
        }

        public void Delete(
            TagDefinition tag,
            Action<bool> completed = null)
        {
            if (tag == null)
            {
                completed?.Invoke(
                    false);

                return;
            }

            if (LibraryEditingService.IsEditing(
                    $"Tag:{tag.Id}"))
            {
                if (!LibraryEditingService.CommitEditing())
                {
                    completed?.Invoke(
                        false);

                    return;
                }
            }

            TagDeleteReport report =
                _deleteAnalyzer.Analyze(
                    tag);

            LoreDialog.ConfirmDestructive(
                string.Format(LoreText.Library.TagDialogTitle, tag.Title),
                GetDeleteWarning(
                    report),
                LoreText.Common.DeleteButton,
                () =>
                {
                    bool deleted =
                        _tagService.DeleteTag(
                            tag);

                    if (deleted)
                    {
                        LoreRefreshService.Refresh();
                    }

                    completed?.Invoke(
                        deleted);
                },
                cancelled:
                    () =>
                        completed?.Invoke(
                            false));
        }

        public void BeginEditing(
            TagDefinition tag)
        {
            if (tag == null)
                return;

            string editingId =
                $"Tag:{tag.Id}";

            var state =
                new TagCardEditState(
                    tag);

            _editStates[tag] =
                state;

            LibraryInlineTextField titleEditor =
                GetTitleEditor(
                    tag);

            titleEditor.BeginEditing(
                state.Title);

            bool started =
                LibraryEditingService
                    .BeginEditing<TagLibraryItem>(
                        editingId,
                        () => CommitEditing(
                            tag),
                        () => CancelEditing(
                            tag));

            if (started)
                return;

            titleEditor.EndEditing();

            _editStates.Remove(
                tag);
        }

        public float GetAdditionalHeight(
            TagLibraryItem item)
        {
            if (item?.Tag == null)
                return 0f;

            if (!_editStates.TryGetValue(
                    item.Tag,
                    out TagCardEditState state))
            {
                return 0f;
            }

            if (string.IsNullOrWhiteSpace(
                    state.Feedback))
            {
                return 0f;
            }

            return LibraryStyle.FeedbackHeight;
        }

        public void DrawAdditionalContent(
            Rect rect,
            TagLibraryItem item)
        {
            if (item?.Tag == null)
                return;

            if (!_editStates.TryGetValue(
                    item.Tag,
                    out TagCardEditState state))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(
                    state.Feedback))
            {
                return;
            }

            Rect feedbackRect =
                new(
                    rect.x
                        + LibraryStyle.FeedbackHorizontalPadding,
                    rect.y
                        + LibraryStyle.FeedbackVerticalPadding,
                    Mathf.Max(
                        0f,
                        rect.width
                            - LibraryStyle.FeedbackHorizontalPadding * 2f),
                    Mathf.Max(
                        0f,
                        rect.height
                            - LibraryStyle.FeedbackVerticalPadding * 2f));

            GUI.Label(
                feedbackRect,
                state.Feedback,
                LibraryStyle.Feedback);
        }
        #endregion

        #region Draw
        private static void DrawViewer(
            LibraryCardLayout layout,
            TagLibraryItem item)
        {
            DrawSwatch(
                layout.Visual,
                item.Color);

            GUI.Label(
                layout.Title,
                item.Title,
                LibraryStyle.Title);
        }

        private static void DrawInactiveViewer(
            LibraryCardLayout layout,
            TagLibraryItem item)
        {
            Color previousColor = GUI.color;

            GUI.color =
                new Color(
                    previousColor.r,
                    previousColor.g,
                    previousColor.b,
                    previousColor.a *
                    LibraryStyle.InactiveAlphaMultiplier);

            DrawViewer(
                layout,
                item);

            GUI.color = previousColor;
        }

        private static void DrawSwatch(
            Rect rect,
            Color color)
        {
            Rect swatchRect =
                new(
                    rect.center.x
                        - LibraryStyle.SwatchSize * 0.5f,
                    rect.center.y
                        - LibraryStyle.SwatchSize * 0.5f,
                    LibraryStyle.SwatchSize,
                    LibraryStyle.SwatchSize);

            EditorGUI.DrawRect(
                swatchRect,
                color);
        }

        private void DrawEditable(
            LibraryCardLayout layout,
            TagLibraryItem item)
        {
            if (!_editStates.TryGetValue(
                    item.Tag,
                    out TagCardEditState state))
            {
                return;
            }

            DrawColor(
                layout.Visual,
                item,
                state);

            DrawTitle(
                layout.Title,
                item,
                state);
        }

        private void DrawColor(
            Rect rect,
            TagLibraryItem item,
            TagCardEditState state)
        {
            DrawSwatch(
                rect,
                state.Color);

            EditorGUIUtility.AddCursorRect(
                rect,
                MouseCursor.Link);

            if (Event.current.type !=
                    EventType.MouseDown ||
                !rect.Contains(
                    Event.current.mousePosition))
            {
                return;
            }

            ColorAssignmentContext.BeginAssignment(
                color =>
                {
                    state.Color =
                        color;
                });

            LibraryService.OpenNestedColorLibraryWithSelection?.Invoke(state.Color);

            Event.current.Use();
        }

        private void DrawTitle(
            Rect rect,
            TagLibraryItem item,
            TagCardEditState state)
        {
            LibraryInlineTextFieldResult result =
                GetTitleEditor(
                        item.Tag)
                    .Draw(
                        item.EditingId,
                        rect,
                        state.Title,
                        LoreText.Library.TagTitlePlaceholder,
                        LibraryStyle.TitleField);

            if (state.Title != result.Value)
            {
                state.Feedback = string.Empty;
            }

            state.Title = result.Value;
        }

        private void DrawMenu(
            Rect rect,
            TagLibraryItem item)
        {
            Rect menuRect =
                new(
                    rect.x,
                    rect.y + LibraryStyle.CardMenuVerticalInset,
                    rect.width,
                    rect.height - LibraryStyle.CardMenuVerticalInset * 2f);

            bool editing =
                LibraryEditingService.IsEditing(
                    item.EditingId);

            var menu =
                new TagMenuElement();

            menu.SetTag(
                item.Tag,
                editing,
                () => BeginEditing(
                    item.Tag),
                () => SaveEditing(
                    item),
                () => CancelEditing(
                    item),
                () => ShowReferences(
                    item.Tag),
                () =>
                {
                    if (TagDeleteRequested != null)
                    {
                        TagDeleteRequested.Invoke(
                            item);

                        return;
                    }

                    Delete(
                        item.Tag);
                },
                LoreRefreshService.Refresh);

            menu.Draw(
                menuRect);
        }
        #endregion

        #region Commands
        private bool SaveEditing(
            TagLibraryItem item)
        {
            if (!LibraryEditingService.IsEditing(
                    item.EditingId))
            {
                return true;
            }

            return LibraryEditingService
                .CommitEditing();
        }

        private void CancelEditing(
            TagLibraryItem item)
        {
            if (!LibraryEditingService.IsEditing(
                    item.EditingId))
            {
                return;
            }

            LibraryEditingService
                .CancelEditing();
        }

        private bool CommitEditing(TagDefinition tag)
        {
            if (!_editStates.TryGetValue(
                    tag,
                    out TagCardEditState state))
            {
                return true;
            }

            LoreOperationResult validation =
                _tagService.ValidateRename(
                    tag,
                    state.Title);

            if (!validation.Success)
            {
                state.Feedback = validation.Message;

                TagFeedbackShown?.Invoke(
                    tag);

                LoreRepaintService.Repaint();

                return false;
            }

            state.Feedback = string.Empty;

            bool renamed =
                _tagService.RenameTag(
                    tag,
                    state.Title);

            if (!renamed)
                return false;

            bool colorUpdated =
                _tagService.UpdateColor(
                    tag,
                    state.Color);

            if (!colorUpdated)
                return false;

            GetTitleEditor(
                    tag)
                .EndEditing();

            _editStates.Remove(
                tag);

            TagEditingCommitted?.Invoke(
                tag);

            TagEditingEnded?.Invoke(
                tag);

            return true;
        }

        private void CancelEditing(
            TagDefinition tag)
        {
            GetTitleEditor(
                    tag)
                .EndEditing();

            _editStates.Remove(
                tag);

            TagEditingEnded?.Invoke(
                tag);
        }
        #endregion

        #region Helpers
        private LibraryInlineTextField GetTitleEditor(
            TagDefinition tag)
        {
            if (!_titleEditors.TryGetValue(
                    tag,
                    out LibraryInlineTextField editor))
            {
                editor =
                    new LibraryInlineTextField();

                _titleEditors.Add(
                    tag,
                    editor);
            }

            return editor;
        }

        private static string GetDeleteWarning(TagDeleteReport report)
        {
            if (!report.IsInUse)
            {
                return LoreText.Common.Paragraphs(
                    LoreText.Library.TagDeleteUnusedWarning,
                    LoreText.Common.CannotBeUndone);
            }

            return LoreText.Common.Paragraphs(
                string.Format(
                    LoreText.Library.TagDeleteWarning,
                    LoreText.Common.EntryCount.Format(
                        report.AffectedEntryCount)),
                LoreText.Common.CannotBeUndone);
        }
        #endregion
    }
}