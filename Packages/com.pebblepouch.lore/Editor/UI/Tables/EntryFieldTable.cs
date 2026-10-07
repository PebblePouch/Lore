using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class EntryFieldTable
    {
        #region Fields
        private readonly EntryFieldRow _row = new();

        private Entry _entry;

        private readonly EntryIntegrityAnalyzer _integrityAnalyzer = new();

        // Last drawn row width (see DrawRow).
        private float _rowWidth;

        // Performance: cached per Entry and LoreDataVersion (not recalculated on every OnGUI call).
        private readonly LoreCache<Entry, IReadOnlyList<EntryIntegrityIssue>> _integrityIssues = new();
        #endregion

        #region Callbacks
        private readonly Action<Rect> _revealRequested;
        #endregion

        #region Constructor
        public EntryFieldTable(
            System.Action<Rect> revealRequested)
        {
            _revealRequested =
                revealRequested;
        }
        #endregion

        #region Public API
        public void SetEntry(Entry entry)
        {
            _entry = entry;
        }

        public void Draw()
        {
            if (_entry == null)
                return;

            GUILayout.Space(
                InspectorStyle.FieldTableTopSpacing);

            // Keep Category / Entry Field tables vertically aligned.
            EditorGUILayout.GetControlRect(
                false,
                0f,
                GUILayout.ExpandWidth(true));

            DrawRows();
        }
        #endregion

        #region Draw
        private void DrawRows()
        {
            var fields =
                _entry.Fields.ToList();

            var missingIndices =
                _integrityIssues
                    .Get(
                        _entry,
                        _integrityAnalyzer.Analyze)
                    .Select(
                        issue =>
                            issue.FieldIndex)
                    .ToHashSet();

            for (int i = 0;
                 i < fields.Count;
                 i++)
            {
                bool drawSeparator =
                    i < fields.Count - 1;

                if (missingIndices.Contains(i))
                {
                    DrawMissingRow(
                        drawSeparator);

                    continue;
                }

                DrawRow(
                    fields[i],
                    drawSeparator);
            }
        }

        private void DrawRow(
            FieldValue field,
            bool drawSeparator)
        {
            float rowHeight =
                GetRowHeight(
                    field);

            Rect rect =
                GUILayoutUtility.GetRect(
                    0f,
                    rowHeight,
                    GUILayout.ExpandWidth(true),
                    GUILayout.Height(
                        rowHeight));

            // Row width from the last Repaint (GetRect returns a placeholder during Layout); Entries rows wrap
            // their chips at this width, so a changed width needs another frame.
            if (Event.current.type == EventType.Repaint &&
                !Mathf.Approximately(rect.width, _rowWidth))
            {
                _rowWidth = rect.width;

                LoreRepaintService.Repaint();
            }

            bool matchingFilterField =
                ContentBrowserFilterContext
                    .IsMatchingField(
                        field?.Field);

            // Required by the Category but still empty: health color on the value cell (points to what is missing;
            // broken Fields mark the whole row red instead).
            bool missingRequired =
                ContentHealthAnalyzer.IsMissingRequiredValue(
                    _entry,
                    field);

            if (missingRequired &&
                !matchingFilterField &&
                Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(
                    EntryFieldRow.GetValueRect(rect),
                    InspectorStyle.FieldHealthBackground);
            }

            if (matchingFilterField &&
                Event.current.type ==
                    EventType.Repaint)
            {
                Rect matchingRect =
                    new(
                        rect.x,
                        rect.y,
                        Mathf.Max(
                            0f,
                            rect.width
                                - InspectorStyle.FieldTableColumnSpacing
                                - InspectorStyle.FieldButtonColumnWidth),
                        rect.height);

                EditorGUI.DrawRect(
                    matchingRect,
                    InspectorStyle.FieldFilterBackground);
            }

            _row.SetEntry(
                _entry);

            _row.SetFieldValue(
                field);

            _row.Draw(
                rect,
                () =>
                    ClearFieldValue(
                        field));

            HandleRevealRequest(
                field,
                rect);

            EntryAssignmentContext.CheckInteraction(
                rect);

            DrawLines(
                drawSeparator);
        }

        // Entries rows grow with their chips; all other rows have the fixed row height.
        private float GetRowHeight(FieldValue field)
        {
            if (field?.Field == null ||
                field.Field.Type != FieldType.Entries ||
                _rowWidth <= 0f)
            {
                return InspectorStyle.FieldTableRowHeight;
            }

            return FieldValueEntriesElement.CalcHeight(
                field,
                EntryFieldRow.GetValueWidth(_rowWidth));
        }

        private void DrawMissingRow(
            bool drawSeparator)
        {
            Rect rowRect =
                GUILayoutUtility.GetRect(
                    0f,
                    InspectorStyle.FieldTableRowHeight,
                    GUILayout.ExpandWidth(true),
                    GUILayout.Height(
                        InspectorStyle.FieldTableRowHeight));

            if (Event.current.type ==
                EventType.Repaint)
            {
                Rect brokenRect =
                    new(
                        rowRect.x,
                        rowRect.y,
                        Mathf.Max(
                            0f,
                            rowRect.width
                                - InspectorStyle.FieldTableColumnSpacing
                                - InspectorStyle.FieldButtonColumnWidth),
                        rowRect.height);

                EditorGUI.DrawRect(
                    brokenRect,
                    InspectorStyle.FieldBrokenBackground);
            }

            _row.DrawMissing(
                rowRect);

            DrawLines(
                drawSeparator);
        }

        private void DrawLines(bool drawSeparator)
        {
            if (drawSeparator)
            {
                Rect separator = GUILayoutUtility.GetRect(
                    1f,
                    1f,
                    GUILayout.ExpandWidth(true));

                EditorGUI.DrawRect(
                    separator,
                    InspectorStyle.FieldRowLineColor);
            }
        }
        #endregion

        #region Helpers
        private void ClearFieldValue(FieldValue fieldValue)
        {
            if (fieldValue == null)
                return;

            LoreUndo.Record(
                _entry,
                LoreText.Undo.ClearFieldValue);

            fieldValue.Clear();

            LoreChangeTracker.MarkModified(_entry);

            LoreRelationshipIndex.Instance.Refresh();

            LoreRepaintService.Repaint();

            EntryAssignmentContext.EndAssignment();
            LibraryService.CloseLibrary?.Invoke();
        }

        private void HandleRevealRequest(
            FieldValue fieldValue,
            Rect rect)
        {
            if (fieldValue?.Field == null)
                return;

            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            if (!EntryFieldRevealRequestService.Consume(_entry, fieldValue.Field))
            {
                return;
            }

            _revealRequested?.Invoke(rect);

            LoreRepaintService.Repaint();
        }
        #endregion
    }
}