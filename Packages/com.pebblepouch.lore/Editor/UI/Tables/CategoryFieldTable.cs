using System;
using System.Linq;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class CategoryFieldTable
    {
        #region Struct
        private readonly struct RowInfo
        {
            public readonly Field Field;
            public readonly Rect Rect;

            public RowInfo(Field field, Rect rect)
            {
                Field = field;
                Rect = rect;
            }
        }
        #endregion

        #region Fields
        private readonly Action _repaint;
        private readonly Action<Field> _editRequested;
        private readonly Action<Rect> _revealRequested;

        private readonly Category _category;

        private readonly CategoryFieldRow _row = new();
        private readonly List<RowInfo> _rowInfos = new();

        private readonly FieldService _fieldService = new();
        private readonly CategoryFieldRemoveCommand _fieldRemoveCommand = new();

        private readonly List<DragHandle<Field>> _fieldHandles = new();
        private readonly DragListController<Field> _fieldController = new();

        private readonly CategoryIntegrityAnalyzer _integrityAnalyzer = new();
        private readonly CategoryService _categoryService = new();

        // Performance: cached per Category and LoreDataVersion (not recalculated on every OnGUI call).
        private readonly LoreCache<Category, IReadOnlyList<CategoryIntegrityIssue>> _integrityIssues = new();
        private readonly FieldIntegrityRepairService _integrityRepairService = new();
        #endregion

        #region Constructor
        public CategoryFieldTable(
            Category category,
            Action repaint,
            Action<Field> editRequested,
            Action<Rect> revealRequested)
        {
            _category = category;
            _repaint = repaint;
            _editRequested = editRequested;
            _revealRequested = revealRequested;
        }
        #endregion

        #region Public API
        public void Draw()
        {
            if (_category == null)
                return;

            ClearRowInfosAndFieldHandles();

            GUILayout.Space(InspectorStyle.FieldTableTopSpacing);

            Rect rect = EditorGUILayout.GetControlRect(
                false,
                0f,
                GUILayout.ExpandWidth(true));

            float width = rect.width;

            DrawRows(width);

        }
        #endregion

        #region Draw
        private void DrawRows(
            float width)
        {
            IReadOnlyList<CategoryIntegrityIssue> issues =
                _integrityIssues.Get(
                    _category,
                    _integrityAnalyzer.Analyze);

            var missingIndices =
                issues
                    .Select(
                        issue =>
                            issue.FieldIndex)
                    .ToHashSet();

            var visibleIndices =
                new List<int>();

            for (int i = 0;
                 i < _category.Fields.Count;
                 i++)
            {
                if (missingIndices.Contains(i) ||
                    _category.Fields[i] != null)
                {
                    visibleIndices.Add(i);
                }
            }

            bool hasMissingField =
                missingIndices.Count > 0;

            for (int i = 0;
                 i < visibleIndices.Count;
                 i++)
            {
                int fieldIndex =
                    visibleIndices[i];

                bool drawSeparator =
                    i < visibleIndices.Count - 1;

                if (missingIndices.Contains(
                        fieldIndex))
                {
                    DrawMissingRow(
                        fieldIndex,
                        width,
                        drawSeparator);

                    continue;
                }

                Field field =
                    _category.Fields[fieldIndex];

                if (field == null)
                    continue;

                DrawRow(
                    field,
                    width,
                    drawSeparator,
                    !hasMissingField);
            }

            HandleFieldEditRequest();

            if (!hasMissingField)
            {
                HandleFieldDragAndDrop();
                DrawFieldDropLine();
            }
        }

        private void DrawRow(
            Field field,
            float width,
            bool drawSeparator,
            bool canReorder)
        {
            EditorGUILayout.BeginHorizontal(
                GUILayout.Height(InspectorStyle.FieldTableRowHeight));

            DrawHandle(field, canReorder);

            EditorGUILayout.BeginHorizontal();

            DrawContent(field, width);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndHorizontal();

            DrawLines(field, drawSeparator);
        }

        private void DrawMissingRow(
            int fieldIndex,
            float width,
            bool drawSeparator)
        {
            Rect rowRect =
                EditorGUILayout.BeginHorizontal(
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

            DrawMissingHandle();

            EditorGUILayout.BeginHorizontal();

            _row.SetAvailableWidth(
                width);

            _row.DrawMissing(
                () => RemoveMissingField(
                    fieldIndex));

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndHorizontal();

            HandleMissingFieldClick(fieldIndex, rowRect);

            HandleMissingFieldReveal(fieldIndex, rowRect);

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

        private void DrawHandle(
            Field field,
            bool canReorder)
        {
            Rect rowRect =
                GUILayoutUtility.GetRect(
                    GUIContent.none,
                    GUIStyle.none,
                    GUILayout.Height(
                        InspectorStyle.FieldTableRowHeight),
                    GUILayout.ExpandWidth(false));

            Rect handleRect =
                new(
                    rowRect.x,
                    rowRect.y,
                    InspectorStyle.FieldHandleColumnWidth,
                    rowRect.height);

            Color previousColor = GUI.color;

            if (canReorder)
            {
                GUI.color = InspectorStyle.FieldHandleActiveColor;
            }
            else
            {
                GUI.color = InspectorStyle.FieldHandleDisabledColor;
            }

            GUI.Label(
                    handleRect,
                    InspectorStyle.FieldHandleIcon,
                    InspectorStyle.FieldHandleContent);

            GUI.color = previousColor;

            if (canReorder)
            {
                EditorGUIUtility.AddCursorRect(handleRect, MouseCursor.Pan);

                _fieldHandles.Add(
                    new DragHandle<Field>(
                        field,
                        handleRect,
                        GUILayoutUtility.GetLastRect()));
            }
            else
            {
                EditorGUIUtility.AddCursorRect(handleRect, MouseCursor.Arrow);

                GUI.Label(
                    handleRect,
                    new GUIContent(
                        string.Empty,
                        LoreText.Inspector.ReorderFieldsDisabledTooltip));
            }

            GUILayout.Space(InspectorStyle.FieldHandleColumnWidth);
        }

        private static void DrawMissingHandle()
        {
            GUILayoutUtility.GetRect(
                GUIContent.none,
                GUIStyle.none,
                GUILayout.Height(
                    InspectorStyle.FieldTableRowHeight),
                GUILayout.ExpandWidth(false));

            GUILayout.Space(
                InspectorStyle.FieldHandleColumnWidth);
        }

        private void DrawContent(Field field, float width)
        {
            _row.SetAvailableWidth(width);

            _row.Draw(
                field,
                () =>
                    RemoveField(
                        field),
                _category.IsRequired(field),
                () =>
                    _categoryService.SetFieldRequired(
                        _category,
                        field,
                        !_category.IsRequired(field)));
        }

        private void DrawLines(Field field, bool drawSeparator)
        {
            Rect rowRect = GUILayoutUtility.GetLastRect();

            _rowInfos.Add(
                new RowInfo(
                    field,
                    rowRect));

            EditorGUIUtility.AddCursorRect(
                GetFieldEditRect(
                    rowRect),
                MouseCursor.Link);

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

        #region Commands
        private void RemoveMissingField(int fieldIndex)
        {
            EditorApplication.delayCall +=
                () =>
                {
                    if (_category == null)
                        return;

                    if (!_integrityRepairService
                        .RemoveMissingField(
                            _category,
                            fieldIndex))
                    {
                        return;
                    }

                    _repaint?.Invoke();
                };
        }

        private void ReplaceMissingField(int fieldIndex)
        {
            MissingFieldAssignmentContext
                .BeginAssignment(
                    _category,
                    fieldIndex);

            LibraryService
                .OpenFieldLibrary
                ?.Invoke();

            _repaint?.Invoke();
        }
        #endregion

        #region Drag'n'Drop
        private void HandleFieldDragAndDrop()
        {
            switch (Event.current.type)
            {
                case EventType.MouseDown:
                    HandleFieldMouseDown();
                    break;

                case EventType.MouseDrag:
                    HandleFieldMouseDrag();
                    break;

                case EventType.MouseUp:
                    HandleFieldMouseUp();
                    break;
            }
        }

        private void HandleFieldMouseDown()
        {
            _fieldController.BeginDrag(
                _fieldHandles,
                Event.current.mousePosition,
                handle =>
                {
                    _fieldController.Drag.InsertionIndex =
                        DragInsertion.GetInsertionIndex(
                            _fieldHandles,
                            Event.current.mousePosition);
                });
        }

        private void HandleFieldMouseDrag()
        {
            _fieldController.UpdateInsertionIndex(
                DragInsertion.GetInsertionIndex(
                    _fieldHandles,
                    Event.current.mousePosition));

            _repaint?.Invoke();
        }

        private void HandleFieldMouseUp()
        {
            _fieldController.EndDrag(
                (field, insertionIndex) =>
                {
                    MoveField(
                        field, 
                        insertionIndex);
                });
        }

        private void DrawFieldDropLine()
        {
            if (!_fieldController.IsDragging)
                return;

            if (_rowInfos.Count == 0)
                return;

            float tableX = _rowInfos[0].Rect.x;
            float tableWidth = _rowInfos[0].Rect.width;

            Rect lineRect;

            if (_fieldController.Drag.InsertionIndex == 0)
            {
                Rect first = _rowInfos[0].Rect;

                lineRect = new Rect(
                    tableX,
                    first.y,
                    tableWidth,
                    2);
            }
            else if (_fieldController.Drag.InsertionIndex >= _rowInfos.Count)
            {
                Rect last = _rowInfos[^1].Rect;

                lineRect = new Rect(
                    tableX,
                    last.yMax,
                    tableWidth,
                    2);
            }
            else
            {
                Rect row = _rowInfos[_fieldController.Drag.InsertionIndex].Rect;

                lineRect = new Rect(
                    tableX,
                    row.y,
                    tableWidth,
                    2);
            }

            EditorGUI.DrawRect(lineRect, InspectorStyle.FieldDropLineColor);
        }
        #endregion

        #region Helper
        private static Rect GetFieldEditRect(Rect rowRect)
        {
            return new Rect(
                rowRect.x
                    + InspectorStyle.FieldHandleColumnWidth,
                rowRect.y,
                rowRect.width
                    - InspectorStyle.FieldHandleColumnWidth
                    - InspectorStyle.FieldButtonColumnWidth,
                rowRect.height);
        }

        private void HandleFieldEditRequest()
        {
            Event current = Event.current;

            if (current.type != EventType.MouseDown 
                || current.button != 0)
            {
                return;
            }

            foreach (RowInfo rowInfo in _rowInfos)
            {
                Rect editRect =
                    GetFieldEditRect(
                        rowInfo.Rect);

                if (!editRect.Contains(
                        current.mousePosition))
                {
                    continue;
                }

                current.Use();

                _editRequested?.Invoke(rowInfo.Field);

                return;
            }
        }

        private void ClearRowInfosAndFieldHandles()
        {
            _rowInfos.Clear();
            _fieldHandles.Clear();
        }

        private void RemoveField(Field field)
        {
            if (_category == null || field == null)
            {
                return;
            }

            EditorApplication.delayCall +=
                () =>
                {
                    if (_category == null || field == null)
                    {
                        return;
                    }

                    _fieldRemoveCommand.Execute(
                        _category,
                        field,
                        removed =>
                        {
                            if (!removed)
                                return;

                            _repaint?.Invoke();
                        });
                };
        }

        private void MoveField(
            Field field,
            int insertionIndex)
        {
            if (insertionIndex < 0)
                return;

            int currentIndex = _category.GetFieldIndex(field);

            bool isSamePosition =
                insertionIndex == currentIndex ||
                insertionIndex == currentIndex + 1;

            if (isSamePosition)
                return;

            _fieldService.MoveField(
                _category,
                field,
                insertionIndex);
        }

        private void HandleMissingFieldClick(
            int fieldIndex,
            Rect rowRect)
        {
            Event current = Event.current;

            Rect replaceRect =
                new(
                    rowRect.x,
                    rowRect.y,
                    Mathf.Max(
                        0f,
                        rowRect.width
                            - InspectorStyle.FieldTableColumnSpacing
                            - InspectorStyle.FieldButtonColumnWidth),
                    rowRect.height);

            EditorGUIUtility.AddCursorRect(
                replaceRect,
                MouseCursor.Link);

            if (current.type != EventType.MouseDown 
                || current.button != 0)
            {
                return;
            }

            if (!replaceRect.Contains(current.mousePosition))
            {
                return;
            }

            ReplaceMissingField(fieldIndex);

            current.Use();
        }

        private void HandleMissingFieldReveal(
            int fieldIndex,
            Rect rowRect)
        {
            if (Event.current.type !=
                EventType.Repaint)
            {
                return;
            }

            if (!CategoryFieldRevealRequestService
                .Consume(
                    _category,
                    fieldIndex))
            {
                return;
            }

            _revealRequested?.Invoke(
                rowRect);

            LoreRepaintService.Repaint();
        }
        #endregion
    }
}