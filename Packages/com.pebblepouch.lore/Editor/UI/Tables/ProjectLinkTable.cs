using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    /// <summary>
    /// ProjectInspector > ProjectLinksSection > ProjectLinkTable
    /// Drag & drop table of project links, laid out like the Category Fields table.
    /// View row: handle | label (link, opens URL) | URL (dimmed, click to edit) | remove
    /// Edit row: handle | label field | URL field | remove (Enter / click outside saves, Escape cancels)
    /// List changes (add, delete, cancel new, move) run via delayCall so the row count never changes
    /// in the middle of an event (GUILayout requires the same controls as in the Layout pass).
    /// </summary>
    public sealed class ProjectLinkTable
    {
        #region Fields
        private readonly ProjectService _projectService = new();

        private readonly List<DragHandle<ProjectLink>> _handles = new();
        private readonly List<Rect> _rowRects = new();
        private readonly DragListController<ProjectLink> _dragController = new();

        private ProjectLink _editingLink;
        private bool _editingIsNew;

        private string _editLabel = string.Empty;
        private string _editUrl = string.Empty;

        // Control name of the edit field to focus on the next repaint (null = none).
        private string _focusControlPending;

        private string _feedback = string.Empty;
        private bool _showFeedback;

        private Rect _editingRowRect;
        private Rect _editingScreenRect;

        // Last real table width. GetControlRect only returns a placeholder during Layout,
        // and the fixed column widths must already be correct in the Layout pass.
        private float _availableWidth;
        #endregion

        #region Properties
        public bool IsEditing => _editingLink != null;
        #endregion

        #region Public API
        public void Draw()
        {
            IReadOnlyList<ProjectLink> links = _projectService.GetLinks();

            // The link being edited may have been removed elsewhere.
            if (IsEditing &&
                !links.Contains(_editingLink))
            {
                EndEditing();
            }

            // Feedback visibility only changes during Layout (stable GUILayout control count).
            if (Event.current.type == EventType.Layout)
            {
                _showFeedback = IsEditing && !string.IsNullOrEmpty(_feedback);
            }

            _handles.Clear();
            _rowRects.Clear();

            GUILayout.Space(InspectorStyle.FieldTableTopSpacing);

            if (links.Count == 0)
            {
                GUILayout.Space(InspectorStyle.LinkEmptyTopSpacing);

                GUILayout.Label(
                    LoreText.Inspector.NoLinks,
                    InspectorStyle.LinkEmpty);

                return;
            }

            UpdateAvailableWidth();

            HandleEditingKeys();

            List<ProjectLink> snapshot = links.ToList();

            for (int i = 0; i < snapshot.Count; i++)
            {
                DrawRow(
                    snapshot[i],
                    _availableWidth,
                    i < snapshot.Count - 1);
            }

            if (IsEditing)
            {
                HandleClickOutsideEditingRow();
            }
            else
            {
                HandleDragAndDrop();
                DrawDropLine();
            }
        }

        public void CreateLink()
        {
            EditorApplication.delayCall +=
                () =>
                {
                    if (!CommitEditing())
                        return;

                    ProjectLink link =
                        _projectService.AddLink();

                    if (link == null)
                        return;

                    BeginEditing(
                        link,
                        true,
                        InspectorStyle.LinkLabelControlName);

                    LoreRepaintService.Repaint();
                };
        }

        // Saves the edited link. Returns false (and keeps editing) when the URL is missing.
        public bool CommitEditing()
        {
            if (!IsEditing)
                return true;

            string url =
                LoreUrl.Normalize(
                    _editUrl);

            if (url.Length == 0)
            {
                _feedback = LoreText.Inspector.LinkUrlRequired;

                _focusControlPending = null;

                LoreRepaintService.Repaint();

                return false;
            }

            string label =
                string.IsNullOrWhiteSpace(_editLabel)
                    ? url
                    : _editLabel.Trim();

            _projectService.UpdateLink(
                _editingLink,
                label,
                url);

            EndEditing();

            return true;
        }

        public void CancelEditing()
        {
            if (!IsEditing)
                return;

            ProjectLink link = _editingLink;
            bool isNew = _editingIsNew;

            EndEditing();

            // A new link that was never saved is removed again.
            if (isNew)
            {
                RemoveLinkDelayed(link);
            }

            LoreRepaintService.Repaint();
        }

        public void HandleOutsideClick(
            Vector2 mousePosition)
        {
            if (!IsEditing)
                return;

            if (LoreScreenRect.Contains(
                    _editingScreenRect,
                    mousePosition))
            {
                return;
            }

            CommitEditing();
        }
        #endregion

        #region Draw
        private void DrawRow(
            ProjectLink link,
            float width,
            bool drawSeparator)
        {
            bool isEditingRow = link == _editingLink;

            Rect rowRect =
                EditorGUILayout.BeginHorizontal(
                    GUILayout.Height(
                        InspectorStyle.FieldTableRowHeight));

            DrawHandle(
                link,
                !IsEditing);

            float contentWidth =
                Mathf.Max(
                    0f,
                    width
                        - InspectorStyle.FieldHandleColumnWidth
                        - InspectorStyle.FieldTableColumnSpacing * 3f
                        - InspectorStyle.FieldButtonColumnWidth);

            float labelWidth =
                contentWidth * InspectorStyle.LinkLabelColumnRatio;

            // Fixed URL width: a flexible column would grow to the full URL text width
            // and push the remove button out of the visible area.
            float urlWidth =
                Mathf.Max(
                    0f,
                    contentWidth - labelWidth);

            GUILayout.Space(InspectorStyle.FieldTableColumnSpacing);

            // Both modes draw exactly one control per column, so the control count stays the same.
            DrawCentered(
                labelWidth,
                () =>
                {
                    if (isEditingRow)
                        DrawLabelField(labelWidth);
                    else
                        DrawLabel(link, labelWidth);
                });

            GUILayout.Space(InspectorStyle.FieldTableColumnSpacing);

            DrawCentered(
                urlWidth,
                () =>
                {
                    if (isEditingRow)
                        DrawUrlField(urlWidth);
                    else
                        DrawUrl(link, urlWidth);
                });

            GUILayout.Space(InspectorStyle.FieldTableColumnSpacing);

            // Same button as "remove Field" in the Category Fields table.
            DrawCentered(
                InspectorStyle.FieldButtonColumnWidth,
                () =>
                    ButtonDrawer.Remove(
                        () => RequestDelete(link),
                        ButtonDrawer.DisplayMode.Glyph,
                        InspectorStyle.FieldButton));

            EditorGUILayout.EndHorizontal();

            if (Event.current.type != EventType.Layout)
            {
                _rowRects.Add(rowRect);

                if (isEditingRow)
                {
                    _editingRowRect = rowRect;
                    _editingScreenRect = LoreScreenRect.FromGUI(rowRect, _editingScreenRect);

                    LoreEditService.ReportEditor(
                        this,
                        rowRect,
                        () => IsEditing);
                }
            }

            if (isEditingRow &&
                _showFeedback)
            {
                GUILayout.Label(
                    _feedback,
                    InspectorStyle.Feedback);
            }

            if (drawSeparator)
            {
                Rect separator =
                    GUILayoutUtility.GetRect(
                        1f,
                        1f,
                        GUILayout.ExpandWidth(true));

                EditorGUI.DrawRect(
                    separator,
                    InspectorStyle.FieldRowLineColor);
            }
        }

        private void UpdateAvailableWidth()
        {
            Rect rect =
                EditorGUILayout.GetControlRect(
                    false,
                    0f,
                    GUILayout.ExpandWidth(true));

            if (Event.current.type != EventType.Repaint ||
                Mathf.Approximately(rect.width, _availableWidth))
            {
                return;
            }

            _availableWidth = rect.width;

            // Re-run Layout with the new width (first draw, window resize).
            LoreRepaintService.Repaint();
        }

        private void DrawHandle(
            ProjectLink link,
            bool canReorder)
        {
            Rect handleRect =
                GUILayoutUtility.GetRect(
                    InspectorStyle.FieldHandleColumnWidth,
                    InspectorStyle.FieldTableRowHeight,
                    GUILayout.Width(
                        InspectorStyle.FieldHandleColumnWidth),
                    GUILayout.Height(
                        InspectorStyle.FieldTableRowHeight));

            Color previousColor = GUI.color;

            GUI.color =
                canReorder
                    ? InspectorStyle.FieldHandleActiveColor
                    : InspectorStyle.FieldHandleDisabledColor;

            GUI.Label(
                handleRect,
                InspectorStyle.FieldHandleIcon,
                InspectorStyle.FieldHandleContent);

            GUI.color = previousColor;

            if (!canReorder)
                return;

            EditorGUIUtility.AddCursorRect(
                handleRect,
                MouseCursor.Pan);

            _handles.Add(
                new DragHandle<ProjectLink>(
                    link,
                    handleRect,
                    handleRect));
        }

        private static void DrawLabel(
            ProjectLink link,
            float width)
        {
            string text =
                string.IsNullOrWhiteSpace(link.Label)
                    ? link.Url
                    : link.Label;

            GUILayout.Label(
                new GUIContent(
                    LoreTextFit.Ellipsize(
                        text,
                        InspectorStyle.LinkLabel,
                        width),
                    string.Format(
                        LoreText.Inspector.OpenLinkTooltip,
                        link.Url)),
                InspectorStyle.LinkLabel,
                GUILayout.MinWidth(0f),
                GUILayout.MaxWidth(width));

            Rect labelRect = GUILayoutUtility.GetLastRect();

            if (string.IsNullOrEmpty(link.Url))
                return;

            EditorGUIUtility.AddCursorRect(
                labelRect,
                MouseCursor.Link);

            if (Event.current.type == EventType.MouseDown &&
                Event.current.button == 0 &&
                labelRect.Contains(Event.current.mousePosition))
            {
                LoreUrl.Open(link.Url);

                Event.current.Use();
            }
        }

        // The dimmed URL is the edit entry point (like clicking a row in the Category Fields table).
        private void DrawUrl(
            ProjectLink link,
            float width)
        {
            GUILayout.Label(
                new GUIContent(
                    LoreTextFit.Ellipsize(
                        link.Url,
                        InspectorStyle.LinkUrl,
                        width),
                    LoreText.Inspector.EditLinkTooltip),
                InspectorStyle.LinkUrl,
                GUILayout.MinWidth(0f),
                GUILayout.MaxWidth(width));

            Rect urlRect = GUILayoutUtility.GetLastRect();

            EditorGUIUtility.AddCursorRect(
                urlRect,
                MouseCursor.Text);

            if (Event.current.type != EventType.MouseDown ||
                Event.current.button != 0 ||
                !urlRect.Contains(Event.current.mousePosition))
            {
                return;
            }

            // Another link may still be in edit mode (e.g. with a missing URL).
            if (CommitEditing())
            {
                BeginEditing(
                    link,
                    false,
                    InspectorStyle.LinkUrlControlName);
            }

            Event.current.Use();
        }

        private void DrawLabelField(float width)
        {
            GUI.SetNextControlName(
                InspectorStyle.LinkLabelControlName);

            _editLabel =
                EditorGUILayout.TextField(
                    _editLabel,
                    InspectorStyle.LinkEditField,
                    GUILayout.MinWidth(0f),
                    GUILayout.MaxWidth(width));

            ApplyPendingFocus(
                InspectorStyle.LinkLabelControlName);
        }

        private void DrawUrlField(float width)
        {
            GUI.SetNextControlName(
                InspectorStyle.LinkUrlControlName);

            string url =
                EditorGUILayout.TextField(
                    _editUrl,
                    InspectorStyle.LinkEditField,
                    GUILayout.MinWidth(0f),
                    GUILayout.MaxWidth(width));

            if (url != _editUrl)
            {
                _editUrl = url;
                _feedback = string.Empty;
            }

            ApplyPendingFocus(
                InspectorStyle.LinkUrlControlName);
        }

        private void ApplyPendingFocus(string controlName)
        {
            if (_focusControlPending != controlName ||
                Event.current.type != EventType.Repaint)
            {
                return;
            }

            EditorGUI.FocusTextInControl(
                controlName);

            _focusControlPending = null;
        }

        private void DrawDropLine()
        {
            if (!_dragController.IsDragging ||
                _rowRects.Count == 0)
            {
                return;
            }

            int insertionIndex = _dragController.Drag.InsertionIndex;

            float y =
                insertionIndex >= _rowRects.Count
                    ? _rowRects[^1].yMax
                    : _rowRects[Mathf.Max(0, insertionIndex)].y;

            EditorGUI.DrawRect(
                new Rect(
                    _rowRects[0].x,
                    y,
                    _rowRects[0].width,
                    BrowserStyle.DropLineHeight),
                InspectorStyle.FieldDropLineColor);
        }
        #endregion

        #region Editing
        // focusControl: the edit field that gets the cursor (the URL when entered by clicking it,
        // the label for a newly created link).
        private void BeginEditing(
            ProjectLink link,
            bool isNew,
            string focusControl)
        {
            _editingLink = link;
            _editingIsNew = isNew;

            _editLabel = link.Label;
            _editUrl = link.Url;

            _feedback = string.Empty;
            _focusControlPending = focusControl;
        }

        private void EndEditing()
        {
            _editingLink = null;
            _editingIsNew = false;

            _editLabel = string.Empty;
            _editUrl = string.Empty;

            _feedback = string.Empty;
            _focusControlPending = null;

            // Static properties only: may run outside OnGUI (e.g. on selection change).
            GUIUtility.keyboardControl = 0;
            EditorGUIUtility.editingTextField = false;
        }

        // Enter commits, Escape cancels (handled before the text fields see the keys).
        private void HandleEditingKeys()
        {
            if (!IsEditing)
                return;

            Event current = Event.current;

            if (current.type != EventType.KeyDown)
                return;

            switch (current.keyCode)
            {
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    CommitEditing();
                    current.Use();
                    break;

                case KeyCode.Escape:
                    CancelEditing();
                    current.Use();
                    break;
            }
        }

        // Clicks inside the table but outside the edited row (outside the table: see HandleOutsideClick).
        private void HandleClickOutsideEditingRow()
        {
            Event current = Event.current;

            if (current.type != EventType.MouseDown ||
                _editingRowRect.Contains(current.mousePosition))
            {
                return;
            }

            CommitEditing();
        }
        #endregion

        #region Commands
        private void RequestDelete(ProjectLink link)
        {
            string title =
                string.IsNullOrWhiteSpace(link.Label)
                    ? link.Url
                    : link.Label;

            LoreDialog.ConfirmDestructive(
                string.Format(LoreText.Dialogs.LinkTitle, title),
                LoreText.Common.Paragraphs(
                    LoreText.Dialogs.LinkRemoveWarning,
                    LoreText.Common.CannotBeUndone),
                LoreText.Common.RemoveButton,
                () =>
                {
                    if (link == _editingLink)
                    {
                        EndEditing();
                    }

                    RemoveLinkDelayed(link);
                });
        }

        private void RemoveLinkDelayed(ProjectLink link)
        {
            EditorApplication.delayCall +=
                () =>
                {
                    _projectService.RemoveLink(link);

                    LoreRepaintService.Repaint();
                };
        }
        #endregion

        #region Drag'n'Drop
        private void HandleDragAndDrop()
        {
            switch (Event.current.type)
            {
                case EventType.MouseDown:
                    _dragController.BeginDrag(
                        _handles,
                        Event.current.mousePosition,
                        handle =>
                        {
                            _dragController.Drag.InsertionIndex =
                                DragInsertion.GetInsertionIndex(
                                    _handles,
                                    Event.current.mousePosition);
                        });
                    break;

                case EventType.MouseDrag:
                    _dragController.UpdateInsertionIndex(
                        DragInsertion.GetInsertionIndex(
                            _handles,
                            Event.current.mousePosition));

                    LoreRepaintService.Repaint();
                    break;

                case EventType.MouseUp:
                    _dragController.EndDrag(
                        (link, insertionIndex) =>
                            EditorApplication.delayCall +=
                                () =>
                                {
                                    _projectService.MoveLink(
                                        link,
                                        insertionIndex);

                                    LoreRepaintService.Repaint();
                                });
                    break;
            }
        }
        #endregion

        #region Helpers
        // MaxWidth instead of Width: a fixed width would be the row's minimum width, and IMGUI passes
        // minimum widths up to the whole Inspector, so it could never get narrower than the last width.
        private static void DrawCentered(
            float width,
            Action draw)
        {
            GUILayout.BeginVertical(
                GUILayout.MinWidth(0f),
                GUILayout.MaxWidth(width),
                GUILayout.Height(InspectorStyle.FieldTableRowHeight));

            GUILayout.FlexibleSpace();

            draw?.Invoke();

            GUILayout.FlexibleSpace();

            GUILayout.EndVertical();
        }
        #endregion
    }
}
