using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class LoreInlineTextArea
    {
        #region Fields
        private bool _isEditing;

        private Vector2 _scrollPosition;

        // View mode scrolling (text longer than the viewport); reset when another text is shown.
        private Vector2 _viewerScrollPosition;
        private string _viewerText;

        // Technical: vertical scrollbar width, and the width the editor's height calculation leaves
        // for scrollbar and text field padding (same values as DrawEditor).
        private const float ScrollbarWidth = 16f;
        private const float EditorTextWidthReserve = 20f;

        // Last observed caret state; -1 = no baseline yet (first frame after editing starts).
        private int _lastCursorIndex = -1;
        private int _lastTextLength = -1;

        private string _editingText;
        private LoreInlineEditEndReason _endReason;

        private readonly DescriptionLinkPicker _linkPicker = new();

        // Last rect from a non-Layout event (see Draw).
        private Rect _lastDrawnRect;

        // Links: the user edits [[Title]], callers receive the stored [[id|Title]] form.
        private readonly Dictionary<string, string> _brokenLinkIds = new();

        private string _storedSource;
        private string _storedText;
        #endregion

        #region Properties
        public bool IsEditing => _isEditing;
        public string EditingText => GetStoredText(_editingText);
        public LoreInlineEditEndReason EndReason => _endReason;
        #endregion

        #region Public API
        // textChanged: saves a description changed in view mode (link repair menu); null disables the menu.
        public string Draw(
            out Rect rect,
            string text,
            string placeholder,
            LoreInlineTextAreaSettings settings,
            Action<string> textChanged = null)
        {
            rect =
                GUILayoutUtility.GetRect(
                    0,
                    _isEditing
                        ? GetEditorHeight(settings)
                        : settings.ViewportHeight,
                    GUILayout.ExpandWidth(true));

            string result = text;

            bool drawEditor =
                _isEditing ||
                DrawViewer(
                    rect,
                    text,
                    placeholder,
                    settings,
                    textChanged,
                    readOnly: false);

            if (drawEditor)
            {
                result =
                    GetStoredText(
                        DrawEditor(
                            rect,
                            settings));
            }

            // Always called (also when not editing) so the picker's layout stays consistent across events.
            _linkPicker.Update(
                _editingText,
                settings.ControlName,
                _isEditing);

            Rect pickerRect =
                _linkPicker.Draw();

            // Clicks on the picker count as inside the description, not as outside clicks.
            if (pickerRect.height > 0f)
            {
                rect.yMax =
                    pickerRect.yMax;
            }

            // During Layout, GetRect returns a 1x1 placeholder. Callers keep this rect for outside-click
            // checks, and Layout runs right before every MouseDown: report the last real rect instead,
            // or a click inside the text area is treated as outside (commit + restart editing).
            if (Event.current.type == EventType.Layout)
            {
                rect = _lastDrawnRect;
            }
            else
            {
                _lastDrawnRect = rect;
            }

            // Lets the window keep keyboard focus for clicks inside this text area (incl. picker) while edited.
            LoreEditService.ReportEditor(
                this,
                rect,
                () => _isEditing);

            return result;
        }

        // View mode only (e.g. Welcome page texts): same look and scrolling, a click never starts editing.
        public void DrawReadOnly(
            string text,
            LoreInlineTextAreaSettings settings)
        {
            Rect rect =
                GUILayoutUtility.GetRect(
                    0,
                    settings.ViewportHeight,
                    GUILayout.ExpandWidth(true));

            DrawViewer(
                rect,
                text,
                string.Empty,
                settings,
                null,
                readOnly: true);
        }

        public void CommitEditing()
        {
            // Release focus like Escape does; otherwise the closed text area keeps keyboard focus
            // and keyboard shortcuts (e.g. Backspace = Back) stay blocked until the next click.
            // May run outside OnGUI (e.g. on selection change).
            if (_isEditing)
                LoreGUIFocus.Release();

            _endReason =
                LoreInlineEditEndReason.Commit;

            _isEditing = false;
        }

        public void CancelEditing()
        {
            _endReason =
                LoreInlineEditEndReason.Cancel;

            _editingText = string.Empty;

            _isEditing = false;
        }

        public LoreInlineEditEndReason ConsumeEndReason()
        {
            LoreInlineEditEndReason result =
                _endReason;

            _endReason =
                LoreInlineEditEndReason.None;

            _editingText =
                string.Empty;

            return result;
        }
        #endregion

        #region Draw
        private bool DrawViewer(
            Rect rect,
            string text,
            string placeholder,
            LoreInlineTextAreaSettings settings,
            Action<string> textChanged,
            bool readOnly)
        {
            string displayText =
                string.IsNullOrWhiteSpace(text)
                    ? placeholder
                    : text;

            GUIStyle style =
                string.IsNullOrWhiteSpace(text)
                    ? settings.PlaceholderContent
                    : settings.ViewerContent;

            // Links and Markdown emphasis need the description flow. Read-only texts (Welcome page) have neither;
            // "[[" or "*" there is plain text, and the flow draws word by word, which would break rich text tags.
            bool useFlow =
                !readOnly &&
                (DescriptionLinkParser.ContainsLinks(text) ||
                 DescriptionEmphasisParser.ContainsMarkers(text));

            // Another text (e.g. another Entry): start at the top again.
            if (displayText != _viewerText)
            {
                _viewerText = displayText;
                _viewerScrollPosition = Vector2.zero;
            }

            float textWidth =
                rect.width
                - settings.HorizontalInset * 2;

            float textHeight =
                GetViewerTextHeight(
                    displayText,
                    useFlow,
                    style,
                    textWidth);

            // Fixed height: longer texts scroll (no layout jumps between Categories/Entries).
            bool scrolls =
                textHeight + settings.VerticalInset * 2 > rect.height;

            if (scrolls)
            {
                textWidth -= ScrollbarWidth;

                textHeight =
                    GetViewerTextHeight(
                        displayText,
                        useFlow,
                        style,
                        textWidth);

                _viewerScrollPosition =
                    GUI.BeginScrollView(
                        rect,
                        _viewerScrollPosition,
                        new Rect(
                            0f,
                            0f,
                            rect.width - ScrollbarWidth,
                            textHeight + settings.VerticalInset * 2));
            }
            else
            {
                GUI.BeginClip(
                    rect);
            }

            Rect contentRect =
                new(
                    settings.HorizontalInset,
                    settings.VerticalInset,
                    textWidth,
                    scrolls
                        ? textHeight
                        : rect.height - settings.VerticalInset * 2);

            if (useFlow)
            {
                // Links consume their own clicks, so a link click never starts editing
                // (broken links open the repair menu when the caller can save repaired text).
                DescriptionTextFlow.Draw(
                    contentRect,
                    text,
                    style,
                    textChanged);
            }
            else
            {
                EditorGUI.LabelField(
                    contentRect,
                    displayText,
                    style);
            }

            // The scroll view consumes clicks on its scrollbar, so those never start editing below.
            if (scrolls)
                GUI.EndScrollView();
            else
                GUI.EndClip();

            if (!readOnly &&
                Event.current.type ==
                    EventType.MouseDown &&
                rect.Contains(
                    Event.current.mousePosition))
            {
                BeginEditing(text);

                return true;
            }

            return false;
        }

        private void BeginEditing(string text)
        {
            // The user only ever sees titles in links, never IDs.
            _storedSource = null;
            _storedText = null;

            _editingText =
                DescriptionLinkFormat.ToEditable(
                    text,
                    _brokenLinkIds);

            _isEditing = true;

            _lastCursorIndex = -1;
            _lastTextLength = -1;

            _linkPicker.Reset();
        }

        private string DrawEditor(
            Rect rect,
            LoreInlineTextAreaSettings settings)
        {
            // Before the text area and Escape handling: picker keys/clicks must not reach the text field.
            _linkPicker.HandleInput(
                _editingText);

            if (HandleKeyboard())
            {
                return _editingText;
            }

            Rect viewRect =
                rect;

            Rect contentRect =
                new(
                    0,
                    0,
                    viewRect.width - ScrollbarWidth,
                    Mathf.Max(
                        viewRect.height,
                        settings.EditorContent.CalcHeight(
                            new GUIContent(
                                _editingText + "\n"),
                            viewRect.width - EditorTextWidthReserve)));

            _scrollPosition =
                GUI.BeginScrollView(
                    viewRect,
                    _scrollPosition,
                    contentRect);

            GUI.SetNextControlName(
                settings.ControlName);

            _editingText =
                EditorGUI.TextArea(
                    contentRect,
                    _editingText,
                    settings.EditorContent);

            GUI.EndScrollView();

            FollowCaret(
                viewRect,
                contentRect,
                settings);

            return _editingText;
        }

        // Keeps the caret line visible when the caret moves or the text changes,
        // without fighting manual (mouse wheel) scrolling in between.
        private void FollowCaret(
            Rect viewRect,
            Rect contentRect,
            LoreInlineTextAreaSettings settings)
        {
            // graphicalCursorPos is updated while the text area repaints.
            if (Event.current.type != EventType.Repaint)
                return;

            if (!LoreActiveTextEditor.TryGetCaret(
                    settings.ControlName,
                    out int cursorIndex,
                    out Vector2 caretPosition))
            {
                return;
            }

            int textLength =
                _editingText?.Length ?? 0;

            bool hasBaseline =
                _lastCursorIndex >= 0;

            bool caretChanged =
                cursorIndex != _lastCursorIndex ||
                textLength != _lastTextLength;

            _lastCursorIndex = cursorIndex;
            _lastTextLength = textLength;

            if (!hasBaseline || !caretChanged)
                return;

            GUIStyle style = settings.EditorContent;

            // graphicalCursorPos.y is not reliably the line top (Unity 6 reports a point lower in the line),
            // so treat the line as starting one line height above it. On the first line this clamps to 0.
            float caretTop =
                caretPosition.y
                - style.lineHeight;

            float caretBottom =
                caretPosition.y
                + style.lineHeight
                + style.padding.bottom;

            float viewHeight =
                viewRect.height;

            float targetScroll =
                _scrollPosition.y;

            if (caretTop < targetScroll)
            {
                // Near the top: snap to 0 so the box padding/frame becomes visible too.
                targetScroll =
                    caretTop < style.lineHeight
                        ? 0f
                        : caretTop;
            }
            else if (caretBottom > targetScroll + viewHeight)
            {
                targetScroll = caretBottom - viewHeight;
            }

            targetScroll =
                Mathf.Clamp(
                    targetScroll,
                    0f,
                    Mathf.Max(
                        0f,
                        contentRect.height - viewHeight));

            if (Mathf.Approximately(
                    targetScroll,
                    _scrollPosition.y))
            {
                return;
            }

            _scrollPosition.y = targetScroll;

            LoreRepaintService.Repaint();
        }
        #endregion

        #region Helpers
        // Edit mode grows with the text (plus one empty line for typing) between the viewport and max height.
        // Uses the last drawn width, so Layout and the following event get the same height.
        private float GetEditorHeight(LoreInlineTextAreaSettings settings)
        {
            if (_lastDrawnRect.width <= 0f)
                return settings.ViewportHeight;

            float contentHeight =
                settings.EditorContent.CalcHeight(
                    new GUIContent(
                        _editingText + "\n"),
                    _lastDrawnRect.width - EditorTextWidthReserve);

            return Mathf.Clamp(
                contentHeight,
                settings.ViewportHeight,
                Mathf.Max(
                    settings.ViewportHeight,
                    settings.MaxEditorHeight));
        }

        private static float GetViewerTextHeight(
            string text,
            bool useFlow,
            GUIStyle style,
            float width)
        {
            return useFlow
                ? DescriptionTextFlow.CalcHeight(
                    text,
                    style,
                    width)
                : style.CalcHeight(
                    new GUIContent(
                        text),
                    width);
        }

        // Editable -> stored link form, cached per text so it only reruns when the text changes.
        private string GetStoredText(string editable)
        {
            if (editable == _storedSource &&
                _storedText != null)
            {
                return _storedText;
            }

            _storedSource = editable;

            _storedText =
                DescriptionLinkFormat.ToStored(
                    editable,
                    _brokenLinkIds);

            return _storedText;
        }

        private bool HandleKeyboard()
        {
            if (Event.current.type !=
                EventType.KeyDown)
            {
                return false;
            }

            if (Event.current.keyCode !=
                KeyCode.Escape)
            {
                return false;
            }

            CancelEditing();

            LoreGUIFocus.Clear();

            EditorGUIUtility.editingTextField =
                false;

            Event.current.Use();

            return true;
        }
        #endregion
    }
}