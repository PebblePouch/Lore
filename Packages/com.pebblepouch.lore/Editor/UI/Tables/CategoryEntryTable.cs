using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Tabular Editing: the Category's Entries as rows, its Fields as columns. The header row and the name column
    // are sticky; the cells scroll (scrollbars, mouse wheel, Shift+wheel, dragging with the middle mouse button).
    // Only visible rows and columns are drawn.
    // Cells edit in place with the Entry page controls (undoable); Entries cells are compact and open the Entry page.
    // Search (same full-text rules as the Content browser) and sorting by column (header click).
    // Checkboxes in the name column select rows for bulk editing; selected rows are read-only.
    public sealed class CategoryEntryTable : ISearchModel
    {
        #region Fields
        private Category _category;

        private Vector2 _scroll;

        // Table rect and width from the last Repaint (GetRect returns a placeholder during Layout). Layout uses the last
        // rect, so it draws the same rows and columns (and creates the same control IDs) as the following events.
        private Rect _lastRect;
        private float _width;

        // Performance: cached per Category and LoreDataVersion (not recalculated on every OnGUI call).
        private readonly LoreCache<Category, IReadOnlyList<Entry>> _entries = new();
        private readonly LoreCache<Category, IReadOnlyList<Field>> _fields = new();

        // One element draws all value cells (as in EntryFieldTable); text inputs are identified by Entry + Field.
        private readonly EntryFieldValueElement _valueElement = new() { CompactEntryCard = true };

        // Scroll position of the last drawn frame (see ClearFocusOnScroll).
        private Vector2 _drawnScroll;

        // Sorting (session only): header click cycles ascending, descending, off. Sorting by the name column
        // uses _sortByTitle instead of a Field.
        private Field _sortField;
        private bool _sortByTitle;
        private bool _sortDescending;

        // Rows after search and sorting. Recalculated when search or sorting change, or when the data changed
        // while no cell text is being edited (rows must not jump away while typing).
        private readonly EntrySearchMatcher _matcher = new();
        private IReadOnlyList<Entry> _rows;
        private string _rowsKey;
        private int _rowsVersion = -1;

        // Bulk editing selection (Entry IDs, session only, cleared on Category change). Rows hidden by the search
        // stay selected. _anchorId: last clicked checkbox, start of a Shift+click range.
        private readonly HashSet<string> _selectedIds = new();
        private string _anchorId;
        private int _selectionVersion = -1;

        // Selected visible rows (row order), recalculated when the rows or the selection change (_selectionRevision).
        private IReadOnlyList<Entry> _selectedRows = new List<Entry>();
        private IReadOnlyList<Entry> _selectedRowsSource;
        private int _selectionRevision;
        private int _selectedRowsRevision = -1;

        private readonly TableSelectionRow _selectionRow = new();

        // Bulk action buttons next to the search: glyph only when the full buttons leave the search too narrow
        // (decided on Repaint, see UpdateToolbarMode). _removeTagRect anchors the Remove Tag menu.
        private bool _toolbarCompact;
        private Rect _removeTagRect;

        // Delete Selected: Entries listed in the confirmation (the rest as "… and N more").
        private const int MaxDeleteListed = 10;

        private readonly EntryService _entryService = new();
        private readonly TagLibraryService _tagService = new();
        #endregion

        #region ISearchModel
        public string Search { get; set; } = string.Empty;

        public void Refresh()
        {
            LoreRepaintService.Repaint();
        }
        #endregion

        #region Public API
        public void SetCategory(Category category)
        {
            if (_category != category)
            {
                _scroll = Vector2.zero;

                Search = string.Empty;

                _sortField = null;
                _sortByTitle = false;
                _sortDescending = false;

                _selectedIds.Clear();
                _anchorId = null;
                _selectionRevision++;
            }

            _category = category;
        }

        public void Draw(float viewportHeight)
        {
            if (_category == null)
                return;

            IReadOnlyList<Entry> entries = _entries.Get(_category, GetEntries);
            IReadOnlyList<Field> fields = _fields.Get(_category, GetFields);

            PruneSelection(entries);

            GUILayout.Space(TableStyle.TopSpacing);

            if (entries.Count == 0)
            {
                GUILayout.Label(
                    LoreText.Inspector.TableNoEntries,
                    TableStyle.EmptyText);

                return;
            }

            // Taken once per event (before the search can change), so all events of a frame draw the same controls.
            IReadOnlyList<Entry> rows = GetRows(entries, fields);
            IReadOnlyList<Entry> selectedRows = GetSelectedRows(rows);

            SearchCard.Draw(
                this,
                LoreText.Inspector.TableSearchPlaceholder,
                null,
                false,
                null,
                selectedRows.Count > 0
                    ? () => DrawSelectionToolbar(selectedRows)
                    : null);

            if (selectedRows.Count > 0)
                UpdateToolbarMode(GUILayoutUtility.GetLastRect().width, selectedRows.Count);

            if (rows.Count == 0)
            {
                GUILayout.Label(
                    LoreText.Inspector.TableNoMatches,
                    TableStyle.EmptyText);

                return;
            }

            DrawTable(rows, selectedRows, fields, viewportHeight);
        }
        #endregion

        #region Layout
        private readonly struct TableLayout
        {
            public readonly Rect Table;
            public readonly Rect Corner;
            public readonly Rect Header;
            // Selection row under the header (zero height without selection): name part and cells part.
            public readonly Rect SelectionName;
            public readonly Rect SelectionCells;
            public readonly Rect Names;
            public readonly Rect Body;
            public readonly Rect HorizontalScrollbar;
            public readonly Rect VerticalScrollbar;
            public readonly Vector2 Content;
            public readonly bool HasHorizontalScrollbar;
            public readonly bool HasVerticalScrollbar;

            public TableLayout(
                Rect table,
                Vector2 content,
                bool hasHorizontalScrollbar,
                bool hasVerticalScrollbar,
                float scrollbarSize,
                float selectionHeight)
            {
                Table = table;
                Content = content;
                HasHorizontalScrollbar = hasHorizontalScrollbar;
                HasVerticalScrollbar = hasVerticalScrollbar;

                float bodyX = table.x + TableStyle.NameColumnWidth;
                float selectionY = table.y + TableStyle.HeaderHeight;
                float bodyY = selectionY + selectionHeight;

                float bodyWidth =
                    Mathf.Max(
                        0f,
                        table.xMax - bodyX - (hasVerticalScrollbar ? scrollbarSize : 0f));

                float bodyHeight =
                    Mathf.Max(
                        0f,
                        table.yMax - bodyY - (hasHorizontalScrollbar ? scrollbarSize : 0f));

                Body = new Rect(bodyX, bodyY, bodyWidth, bodyHeight);
                Corner = new Rect(table.x, table.y, TableStyle.NameColumnWidth, TableStyle.HeaderHeight);
                Header = new Rect(bodyX, table.y, bodyWidth, TableStyle.HeaderHeight);
                SelectionName = new Rect(table.x, selectionY, TableStyle.NameColumnWidth, selectionHeight);
                SelectionCells = new Rect(bodyX, selectionY, bodyWidth, selectionHeight);
                Names = new Rect(table.x, bodyY, TableStyle.NameColumnWidth, bodyHeight);
                HorizontalScrollbar = new Rect(bodyX, Body.yMax, bodyWidth, scrollbarSize);
                VerticalScrollbar = new Rect(Body.xMax, bodyY, scrollbarSize, bodyHeight);
            }

            public Vector2 MaxScroll =>
                new(
                    Mathf.Max(0f, Content.x - Body.width),
                    Mathf.Max(0f, Content.y - Body.height));
        }

        private TableLayout CalculateLayout(
            int rowCount,
            int columnCount,
            float viewportHeight,
            float selectionHeight)
        {
            float scrollbarSize = GUI.skin.verticalScrollbar.fixedWidth;

            Vector2 content =
                new(
                    columnCount * TableStyle.ColumnWidth,
                    rowCount * TableStyle.RowHeight);

            float maxHeight =
                Mathf.Max(
                    TableStyle.MinHeight + selectionHeight,
                    viewportHeight - TableStyle.ViewportMargin);

            // The vertical scrollbar takes width, which can make the horizontal one necessary (and vice versa).
            bool horizontal = TableStyle.NameColumnWidth + content.x > _width;
            float desiredHeight =
                TableStyle.HeaderHeight + selectionHeight + content.y + (horizontal ? scrollbarSize : 0f);
            bool vertical = desiredHeight > maxHeight;

            if (vertical && !horizontal &&
                TableStyle.NameColumnWidth + content.x > _width - scrollbarSize)
            {
                horizontal = true;
                desiredHeight += scrollbarSize;
            }

            float height = Mathf.Min(desiredHeight, maxHeight);

            Rect table =
                GUILayoutUtility.GetRect(
                    0f,
                    height,
                    GUILayout.ExpandWidth(true),
                    GUILayout.Height(height));

            if (Event.current.type == EventType.Repaint &&
                !Mathf.Approximately(table.width, _width))
            {
                _width = table.width;

                LoreRepaintService.Repaint();
            }

            if (Event.current.type == EventType.Repaint)
                _lastRect = table;
            else if (Event.current.type == EventType.Layout)
                table = new Rect(_lastRect.x, _lastRect.y, _lastRect.width, height);

            return new TableLayout(table, content, horizontal, vertical, scrollbarSize, selectionHeight);
        }
        #endregion

        #region Draw
        private void DrawTable(
            IReadOnlyList<Entry> entries,
            IReadOnlyList<Entry> selectedRows,
            IReadOnlyList<Field> fields,
            float viewportHeight)
        {
            float selectionHeight =
                selectedRows.Count > 0
                    ? _selectionRow.CalcHeight(
                        selectedRows,
                        fields,
                        GetCellContentRect(new Rect(0f, 0f, TableStyle.ColumnWidth, TableStyle.RowHeight)).width)
                    : 0f;

            TableLayout layout =
                CalculateLayout(
                    entries.Count,
                    fields.Count,
                    viewportHeight,
                    selectionHeight);

            ClampScroll(layout);

            HandleWheel(layout);
            HandlePan(layout);

            // Scrollbars before the cells: the number of cell controls changes while scrolling, which would
            // change the scrollbar control IDs and drop a running thumb drag.
            DrawScrollbars(layout);

            ClearFocusOnScroll();

            GetVisibleRange(
                _scroll.y,
                layout.Body.height,
                TableStyle.RowHeight,
                entries.Count,
                out int firstRow,
                out int lastRow);

            GetVisibleRange(
                _scroll.x,
                layout.Body.width,
                TableStyle.ColumnWidth,
                fields.Count,
                out int firstColumn,
                out int lastColumn);

            int hoveredRow =
                GetHoveredRow(
                    layout,
                    firstRow,
                    lastRow);

            DrawBody(layout, entries, fields, firstRow, lastRow, firstColumn, lastColumn, hoveredRow);
            DrawNames(layout, entries, firstRow, lastRow, hoveredRow);

            if (selectedRows.Count > 0)
                DrawSelectionRow(layout, selectedRows, fields, firstColumn, lastColumn);

            DrawHeader(layout, fields, firstColumn, lastColumn);
            DrawCorner(layout, entries);
        }

        private void DrawBody(
            TableLayout layout,
            IReadOnlyList<Entry> entries,
            IReadOnlyList<Field> fields,
            int firstRow,
            int lastRow,
            int firstColumn,
            int lastColumn,
            int hoveredRow)
        {
            GUI.BeginClip(layout.Body);

            float visibleWidth = GetVisibleContentWidth(layout);

            for (int row = firstRow; row <= lastRow; row++)
            {
                Entry entry = entries[row];
                float y = row * TableStyle.RowHeight - _scroll.y;
                bool selected = IsSelected(entry);

                Rect rowRect = new(0f, y, visibleWidth, TableStyle.RowHeight);

                if (selected &&
                    Event.current.type == EventType.Repaint)
                {
                    EditorGUI.DrawRect(rowRect, TableStyle.RowSelected);
                }

                if (row == hoveredRow &&
                    Event.current.type == EventType.Repaint)
                {
                    EditorGUI.DrawRect(rowRect, TableStyle.RowHover);
                }

                // Selected rows are read-only (bulk edits go through the selection row).
                using (new EditorGUI.DisabledScope(selected))
                {
                    for (int column = firstColumn; column <= lastColumn; column++)
                    {
                        Rect cell =
                            new(
                                column * TableStyle.ColumnWidth - _scroll.x,
                                y,
                                TableStyle.ColumnWidth,
                                TableStyle.RowHeight);

                        DrawCell(cell, entry, fields[column], selected);
                    }
                }

                DrawRowLine(0f, y + TableStyle.RowHeight - 1f, visibleWidth);
            }

            DrawColumnLines(layout, firstColumn, lastColumn);

            GUI.EndClip();
        }

        private void DrawCell(
            Rect cell,
            Entry entry,
            Field field,
            bool selected)
        {
            // Category Field without a value on this Entry: integrity issue, fixed via the Insights callout.
            // Selected rows show the selection color instead of marks.
            if (!entry.TryGetValue(field, out FieldValue fieldValue))
            {
                if (!selected &&
                    Event.current.type == EventType.Repaint)
                {
                    EditorGUI.DrawRect(cell, TableStyle.BrokenBackground);
                }

                return;
            }

            if (!selected &&
                Event.current.type == EventType.Repaint &&
                ContentHealthAnalyzer.IsMissingRequiredValue(entry, fieldValue))
            {
                EditorGUI.DrawRect(cell, TableStyle.HealthBackground);
            }

            Rect content = GetCellContentRect(cell);

            switch (field.Type)
            {
                case FieldType.Entries:
                    DrawEntriesCell(content, entry, fieldValue);
                    break;

                case FieldType.Entry:
                    DrawEntryCell(content, entry, fieldValue);
                    break;

                default:
                    _valueElement.SetEntry(entry);
                    _valueElement.SetFieldValue(fieldValue);
                    _valueElement.Draw(GetCenteredControlRect(content));
                    break;
            }

            // Clicking another cell ends a running Entry assignment (as on the Entry page).
            EntryAssignmentContext.CheckInteraction(cell);
        }

        // Entry card as on the Entry page (click: pick in the Library), clear button when set.
        private void DrawEntryCell(
            Rect rect,
            Entry entry,
            FieldValue fieldValue)
        {
            Rect cardRect = rect;

            if (fieldValue.EntryValue != null)
            {
                cardRect.width =
                    Mathf.Max(
                        0f,
                        rect.width - InspectorStyle.FieldButtonColumnWidth - TableStyle.ControlPadding);

                Rect buttonRect =
                    GetCenteredControlRect(
                        new Rect(
                            cardRect.xMax + TableStyle.ControlPadding,
                            rect.y,
                            InspectorStyle.FieldButtonColumnWidth,
                            rect.height));

                ButtonDrawer.Clear(
                    buttonRect,
                    () => ClearFieldValue(entry, fieldValue),
                    ButtonDrawer.DisplayMode.Glyph,
                    InspectorStyle.FieldButton);
            }

            _valueElement.SetEntry(entry);
            _valueElement.SetFieldValue(fieldValue);
            _valueElement.Draw(cardRect);
        }

        // Compact (see GetEntriesCellText; full list in the tooltip). Click: edit on the Entry page.
        // Disabled (selected row): no marks, no click.
        private static void DrawEntriesCell(
            Rect rect,
            Entry entry,
            FieldValue fieldValue)
        {
            List<Entry> listed = FieldValueUtility.GetReferencedEntries(fieldValue).ToList();

            bool hasMissing = fieldValue.EntriesValue.Count > listed.Count;

            string tooltip =
                listed.Count > 0
                    ? string.Join("\n", listed.Select(listedEntry => listedEntry.Glyph + " " + listedEntry.Title))
                      + "\n\n" + LoreText.Inspector.TableEditEntriesTooltip
                    : LoreText.Inspector.TableEditEntriesTooltip;

            // Integrity (deleted listed Entries) wins over health (empty list), as on the Entry page.
            if (GUI.enabled &&
                Event.current.type == EventType.Repaint)
            {
                if (hasMissing)
                    EditorGUI.DrawRect(rect, TableStyle.BrokenBackground);
                else if (fieldValue.EntriesValue.Count == 0)
                    EditorGUI.DrawRect(rect, TableStyle.HealthBackground);
            }

            GUI.Label(
                rect,
                new GUIContent(
                    listed.Count > 0
                        ? GetEntriesCellText(listed, rect.width)
                        : LoreText.Inspector.EnumNotSet,
                    tooltip),
                listed.Count > 0
                    ? TableStyle.CellText
                    : TableStyle.CellTextMuted);

            if (!GUI.enabled)
                return;

            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            if (Event.current.type == EventType.MouseDown &&
                Event.current.button == 0 &&
                rect.Contains(Event.current.mousePosition))
            {
                LoreNavigation.NavigateToEntryField(entry, fieldValue.Field);

                Event.current.Use();
            }
        }

        // The most detailed text that fits: "🍎 Apple 🪲 Beetle", then "🍎🪲(2)", then "15 Entries".
        private static string GetEntriesCellText(
            IReadOnlyList<Entry> listed,
            float width)
        {
            string full =
                string.Join(
                    LoreText.Inspector.TableEntriesSeparator,
                    listed.Select(listedEntry => listedEntry.Glyph + " " + listedEntry.Title));

            if (Fits(full, width))
                return full;

            string glyphs =
                string.Format(
                    LoreText.Inspector.TableEntriesGlyphs,
                    string.Concat(listed.Select(listedEntry => listedEntry.Glyph)),
                    listed.Count);

            if (Fits(glyphs, width))
                return glyphs;

            return LoreText.Common.EntryCount.Format(listed.Count);
        }

        private static bool Fits(string text, float width) =>
            TableStyle.CellText.CalcSize(new GUIContent(text)).x <= width;

        private void DrawNames(
            TableLayout layout,
            IReadOnlyList<Entry> entries,
            int firstRow,
            int lastRow,
            int hoveredRow)
        {
            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(layout.Names, TableStyle.NameColumnBackground);

            GUI.BeginClip(layout.Names);

            for (int row = firstRow; row <= lastRow; row++)
            {
                Entry entry = entries[row];

                Rect rowRect =
                    new(
                        0f,
                        row * TableStyle.RowHeight - _scroll.y,
                        TableStyle.NameColumnWidth,
                        TableStyle.RowHeight);

                if (IsSelected(entry) &&
                    Event.current.type == EventType.Repaint)
                {
                    EditorGUI.DrawRect(rowRect, TableStyle.RowSelected);
                }

                if (row == hoveredRow &&
                    Event.current.type == EventType.Repaint)
                {
                    EditorGUI.DrawRect(rowRect, TableStyle.RowHover);
                }

                DrawRowCheckbox(GetCheckboxRect(rowRect), entries, row);

                Rect nameRect = GetRectAfterCheckbox(rowRect);

                DrawGlyphAndTitle(
                    nameRect,
                    entry.Glyph,
                    new GUIContent(entry.Title, LoreText.Inspector.TableOpenEntryTooltip),
                    TableStyle.NameTitle);

                EditorGUIUtility.AddCursorRect(nameRect, MouseCursor.Link);

                if (Event.current.type == EventType.MouseDown &&
                    Event.current.button == 0 &&
                    nameRect.Contains(Event.current.mousePosition))
                {
                    LoreNavigation.NavigateToEntry(entry);

                    Event.current.Use();
                }

                DrawRowLine(0f, rowRect.yMax - 1f, TableStyle.NameColumnWidth);
            }

            GUI.EndClip();

            DrawColumnLine(layout.Names.xMax - 1f, layout.Names.y, layout.Names.height);
        }

        // Sticky under the header: the count in the name column, one bulk editing cell per Field (TableSelectionRow).
        private void DrawSelectionRow(
            TableLayout layout,
            IReadOnlyList<Entry> selectedRows,
            IReadOnlyList<Field> fields,
            int firstColumn,
            int lastColumn)
        {
            float visibleWidth = GetVisibleContentWidth(layout);

            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(layout.SelectionName, TableStyle.SelectionRowBackground);

                EditorGUI.DrawRect(
                    new Rect(layout.SelectionCells.x, layout.SelectionCells.y, visibleWidth, layout.SelectionCells.height),
                    TableStyle.SelectionRowBackground);
            }

            int hidden = _selectedIds.Count - selectedRows.Count;

            string count =
                hidden > 0
                    ? string.Format(LoreText.Inspector.TableSelectionCountHidden, selectedRows.Count, hidden)
                    : string.Format(LoreText.Inspector.TableSelectionCount, selectedRows.Count);

            // In the first line (the row grows for Entries chips).
            GUI.Label(
                GetPaddedRect(
                    new Rect(
                        layout.SelectionName.x,
                        layout.SelectionName.y,
                        layout.SelectionName.width,
                        TableStyle.RowHeight)),
                new GUIContent(count, count + "\n" + LoreText.Inspector.TableSelectionTooltip),
                TableStyle.SelectionCount);

            GUI.BeginClip(layout.SelectionCells);

            for (int column = firstColumn; column <= lastColumn; column++)
            {
                Rect cell =
                    new(
                        column * TableStyle.ColumnWidth - _scroll.x,
                        0f,
                        TableStyle.ColumnWidth,
                        layout.SelectionCells.height);

                _selectionRow.DrawCell(GetCellContentRect(cell), selectedRows, fields[column]);

                EntryAssignmentContext.CheckInteraction(cell);

                DrawColumnLine(cell.xMax - 1f, 0f, cell.height);
            }

            GUI.EndClip();

            DrawColumnLine(layout.SelectionName.xMax - 1f, layout.SelectionName.y, layout.SelectionName.height);
            DrawRowLine(layout.SelectionName.x, layout.SelectionName.yMax - 1f, layout.SelectionName.width + visibleWidth);
        }

        private void DrawHeader(
            TableLayout layout,
            IReadOnlyList<Field> fields,
            int firstColumn,
            int lastColumn)
        {
            Rect background =
                new(
                    layout.Header.x,
                    layout.Header.y,
                    GetVisibleContentWidth(layout),
                    layout.Header.height);

            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(background, TableStyle.HeaderBackground);

            GUI.BeginClip(layout.Header);

            for (int column = firstColumn; column <= lastColumn; column++)
            {
                Field field = fields[column];

                Rect cell =
                    new(
                        column * TableStyle.ColumnWidth - _scroll.x,
                        0f,
                        TableStyle.ColumnWidth,
                        TableStyle.HeaderHeight);

                string type = ScriptableObjectTypeService.GetFieldTypeLabel(field);

                bool required = IsRequired(field);

                Rect titleRect =
                    DrawSortIndicator(
                        cell,
                        !_sortByTitle && _sortField == field);

                DrawGlyphAndTitle(
                    titleRect,
                    field.Glyph,
                    new GUIContent(
                        required
                            ? field.name + " " + InspectorStyle.FieldRequiredGlyph
                            : field.name,
                        type + "\n" + LoreText.Inspector.TableSortTooltip),
                    TableStyle.HeaderTitle);

                HandleSortClick(cell, new Rect(0f, 0f, layout.Header.width, layout.Header.height), field, false);

                DrawColumnLine(cell.xMax - 1f, 0f, cell.height);
            }

            GUI.EndClip();

            DrawRowLine(layout.Header.x, layout.Header.yMax - 1f, GetVisibleContentWidth(layout));
        }

        private void DrawCorner(
            TableLayout layout,
            IReadOnlyList<Entry> entries)
        {
            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(layout.Corner, TableStyle.HeaderBackground);

            DrawSelectAllCheckbox(GetCheckboxRect(layout.Corner), entries);

            Rect sortRect = GetRectAfterCheckbox(layout.Corner);
            Rect titleRect = DrawSortIndicator(sortRect, _sortByTitle);

            GUI.Label(
                GetPaddedRect(titleRect),
                new GUIContent(
                    LoreText.Inspector.TableEntryColumn,
                    LoreText.Inspector.TableSortTooltip),
                TableStyle.CornerTitle);

            HandleSortClick(sortRect, sortRect, null, true);

            DrawColumnLine(layout.Corner.xMax - 1f, layout.Corner.y, layout.Corner.height);
            DrawRowLine(layout.Corner.x, layout.Corner.yMax - 1f, layout.Corner.width);
        }

        // ▲ / ▼ at the right edge of a sorted header cell; returns the rect left for glyph and title.
        private Rect DrawSortIndicator(
            Rect cell,
            bool sorted)
        {
            if (!sorted)
                return cell;

            Rect indicatorRect =
                new(
                    cell.xMax - TableStyle.CellPadding - TableStyle.SortIndicatorWidth,
                    cell.y,
                    TableStyle.SortIndicatorWidth,
                    cell.height);

            GUI.Label(
                indicatorRect,
                _sortDescending
                    ? TableStyle.SortDescendingGlyph
                    : TableStyle.SortAscendingGlyph,
                TableStyle.SortIndicator);

            return new Rect(
                cell.x,
                cell.y,
                Mathf.Max(0f, cell.width - TableStyle.SortIndicatorWidth),
                cell.height);
        }

        private void DrawScrollbars(TableLayout layout)
        {
            if (layout.HasHorizontalScrollbar)
            {
                _scroll.x =
                    GUI.HorizontalScrollbar(
                        layout.HorizontalScrollbar,
                        _scroll.x,
                        layout.Body.width,
                        0f,
                        layout.Content.x);
            }

            if (layout.HasVerticalScrollbar)
            {
                _scroll.y =
                    GUI.VerticalScrollbar(
                        layout.VerticalScrollbar,
                        _scroll.y,
                        layout.Body.height,
                        0f,
                        layout.Content.y);
            }
        }

        private static void DrawGlyphAndTitle(
            Rect rect,
            string glyph,
            GUIContent title,
            GUIStyle titleStyle)
        {
            Rect padded = GetPaddedRect(rect);

            GUI.Label(
                new Rect(padded.x, padded.y, TableStyle.GlyphWidth, padded.height),
                glyph,
                TableStyle.Glyph);

            float titleX = padded.x + TableStyle.GlyphWidth + TableStyle.GlyphSpacing;

            GUI.Label(
                new Rect(titleX, padded.y, Mathf.Max(0f, padded.xMax - titleX), padded.height),
                title,
                titleStyle);
        }

        private void DrawColumnLines(
            TableLayout layout,
            int firstColumn,
            int lastColumn)
        {
            float height = Mathf.Min(layout.Body.height, layout.Content.y - _scroll.y);

            for (int column = firstColumn; column <= lastColumn; column++)
            {
                DrawColumnLine(
                    (column + 1) * TableStyle.ColumnWidth - _scroll.x - 1f,
                    0f,
                    height);
            }
        }

        private static void DrawRowLine(float x, float y, float width)
        {
            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(new Rect(x, y, width, 1f), TableStyle.GridLine);
        }

        private static void DrawColumnLine(float x, float y, float height)
        {
            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(new Rect(x, y, 1f, height), TableStyle.GridLine);
        }
        #endregion

        #region Input
        // Wheel over the table scrolls the table; at its end the event is left to the page, so the page keeps
        // scrolling instead of getting stuck on the table.
        private void HandleWheel(TableLayout layout)
        {
            Event current = Event.current;

            if (current.type != EventType.ScrollWheel ||
                !layout.Table.Contains(current.mousePosition))
            {
                return;
            }

            Vector2 delta = current.delta;

            // Shift+wheel scrolls sideways (if the platform did not convert it already).
            if (current.shift && Mathf.Approximately(delta.x, 0f))
                delta = new Vector2(delta.y, 0f);

            Vector2 scroll = Clamp(_scroll + delta * TableStyle.WheelSpeed, layout.MaxScroll);

            if (scroll == _scroll)
                return;

            _scroll = scroll;

            current.Use();

            LoreRepaintService.Repaint();
        }

        // Dragging with the middle mouse button (wheel held down) moves the view.
        private void HandlePan(TableLayout layout)
        {
            int controlId = ScrollPan.GetControlId();

            _scroll = Clamp(ScrollPan.Handle(controlId, layout.Table, _scroll), layout.MaxScroll);
        }

        // Header click: ascending, descending, off (Lore's order). visibleArea: header cells scrolled partly under
        // the corner must not take its clicks.
        private void HandleSortClick(
            Rect cell,
            Rect visibleArea,
            Field field,
            bool byTitle)
        {
            EditorGUIUtility.AddCursorRect(cell, MouseCursor.Link);

            Event current = Event.current;

            if (current.type != EventType.MouseDown ||
                current.button != 0 ||
                !cell.Contains(current.mousePosition) ||
                !visibleArea.Contains(current.mousePosition))
            {
                return;
            }

            bool sameColumn =
                byTitle
                    ? _sortByTitle
                    : !_sortByTitle && _sortField == field;

            if (!sameColumn)
            {
                _sortField = field;
                _sortByTitle = byTitle;
                _sortDescending = false;
            }
            else if (!_sortDescending)
            {
                _sortDescending = true;
            }
            else
            {
                _sortField = null;
                _sortByTitle = false;
                _sortDescending = false;
            }

            current.Use();

            LoreRepaintService.Repaint();
        }
        #endregion

        #region Selection
        private bool IsSelected(Entry entry) =>
            _selectedIds.Contains(entry.Id);

        private void SetSelected(
            Entry entry,
            bool selected)
        {
            bool changed =
                selected
                    ? _selectedIds.Add(entry.Id)
                    : _selectedIds.Remove(entry.Id);

            if (changed)
                OnSelectionChanged();
        }

        // A running Library pick for the selection (Entry fields, Add Tag) targets the old selection: it ends.
        private void OnSelectionChanged()
        {
            _selectionRevision++;

            if (EntryAssignmentContext.IsBulk)
                EntryAssignmentContext.EndAssignment();

            if (TagAssignmentContext.IsActive)
            {
                TagAssignmentContext.End();

                LibraryService.CloseLibrary?.Invoke();
            }
        }

        private IReadOnlyList<Entry> GetSelectedRows(IReadOnlyList<Entry> rows)
        {
            if (rows == _selectedRowsSource &&
                _selectionRevision == _selectedRowsRevision)
            {
                return _selectedRows;
            }

            _selectedRows = rows.Where(IsSelected).ToList();
            _selectedRowsSource = rows;
            _selectedRowsRevision = _selectionRevision;

            return _selectedRows;
        }

        // Deleted Entries leave the selection.
        private void PruneSelection(IReadOnlyList<Entry> entries)
        {
            if (_selectionVersion == LoreDataVersion.Current)
                return;

            _selectionVersion = LoreDataVersion.Current;

            int count = _selectedIds.Count;

            if (count > 0)
                _selectedIds.IntersectWith(entries.Select(entry => entry.Id));

            if (_selectedIds.Count != count)
                OnSelectionChanged();
        }

        // Click toggles the row; Shift+click sets the range from the last clicked row to the same state.
        private void DrawRowCheckbox(
            Rect rect,
            IReadOnlyList<Entry> entries,
            int row)
        {
            Entry entry = entries[row];

            bool shift = Event.current.shift;

            EditorGUI.BeginChangeCheck();

            bool selected = EditorGUI.Toggle(rect, IsSelected(entry));

            if (EditorGUI.EndChangeCheck())
            {
                int anchor = IndexOf(entries, _anchorId);

                if (shift && anchor >= 0)
                {
                    for (int i = Mathf.Min(anchor, row); i <= Mathf.Max(anchor, row); i++)
                        SetSelected(entries[i], selected);
                }
                else
                {
                    SetSelected(entry, selected);
                }

                _anchorId = entry.Id;

                LoreRepaintService.Repaint();
            }

            GUI.Label(rect, new GUIContent(string.Empty, LoreText.Inspector.TableSelectTooltip), GUIStyle.none);
        }

        // Checked when all visible rows are selected, mixed when some are; click selects all, or none when all were.
        private void DrawSelectAllCheckbox(
            Rect rect,
            IReadOnlyList<Entry> entries)
        {
            int selectedCount = entries.Count(IsSelected);
            bool all = selectedCount == entries.Count;

            EditorGUI.showMixedValue = selectedCount > 0 && !all;

            EditorGUI.BeginChangeCheck();

            bool selected = EditorGUI.Toggle(rect, all);

            EditorGUI.showMixedValue = false;

            if (EditorGUI.EndChangeCheck())
            {
                foreach (Entry entry in entries)
                    SetSelected(entry, selected);

                _anchorId = null;

                LoreRepaintService.Repaint();
            }

            GUI.Label(rect, new GUIContent(string.Empty, LoreText.Inspector.TableSelectAllTooltip), GUIStyle.none);
        }

        private static int IndexOf(
            IReadOnlyList<Entry> entries,
            string id)
        {
            if (id == null)
                return -1;

            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].Id == id)
                    return i;
            }

            return -1;
        }

        private static Rect GetCheckboxRect(Rect rect) =>
            GetCenteredControlRect(
                new Rect(
                    rect.x + TableStyle.CellPadding,
                    rect.y,
                    TableStyle.CheckboxWidth,
                    rect.height));

        private static Rect GetRectAfterCheckbox(Rect rect) =>
            new(
                rect.x + TableStyle.CheckboxColumnWidth,
                rect.y,
                Mathf.Max(0f, rect.width - TableStyle.CheckboxColumnWidth),
                rect.height);
        #endregion

        #region Selection toolbar
        // Bulk actions for the selected visible rows, on the search row: Add Tag (Tag Library), Remove Tag (menu of
        // the Tags they have), Clear All Tags (one undo step each), Delete Selected (confirmation, not undoable).
        private void DrawSelectionToolbar(IReadOnlyList<Entry> selectedRows)
        {
            bool glyphOnly = _toolbarCompact;

            GUIStyle style =
                glyphOnly
                    ? TableStyle.ToolbarGlyphButton
                    : TableStyle.ToolbarButton;

            LoreTheme theme = LoreEditorTheme.Current;

            bool hasTags = selectedRows.Any(entry => entry != null && entry.TagIds.Count > 0);

            ButtonDrawer.Labeled(
                theme.AddTag,
                LoreText.Inspector.TableToolbarTag,
                () => BeginAddingTag(selectedRows),
                glyphOnly,
                style);

            using (new EditorGUI.DisabledScope(!hasTags))
            {
                ButtonDrawer.Labeled(
                    theme.RemoveTag,
                    LoreText.Inspector.TableToolbarTag,
                    () => ShowRemoveTagMenu(selectedRows),
                    glyphOnly,
                    style);

                if (Event.current.type == EventType.Repaint)
                    _removeTagRect = GUILayoutUtility.GetLastRect();

                ButtonDrawer.Labeled(
                    theme.ClearTags,
                    LoreText.Inspector.TableToolbarAllTags,
                    () => _entryService.ClearTags(selectedRows),
                    glyphOnly,
                    style);
            }

            ButtonDrawer.Labeled(
                theme.DeleteSelected,
                LoreText.Inspector.TableToolbarEntries.Format(selectedRows.Count),
                () => ConfirmDeleteSelected(selectedRows),
                glyphOnly,
                style);
        }

        // cardWidth: the search card from the last Repaint.
        private void UpdateToolbarMode(
            float cardWidth,
            int selectedCount)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            LoreTheme theme = LoreEditorTheme.Current;

            float buttonsWidth =
                new[]
                    {
                        ButtonDrawer.GetLabeledContent(theme.AddTag, LoreText.Inspector.TableToolbarTag, false),
                        ButtonDrawer.GetLabeledContent(theme.RemoveTag, LoreText.Inspector.TableToolbarTag, false),
                        ButtonDrawer.GetLabeledContent(theme.ClearTags, LoreText.Inspector.TableToolbarAllTags, false),
                        ButtonDrawer.GetLabeledContent(
                            theme.DeleteSelected,
                            LoreText.Inspector.TableToolbarEntries.Format(selectedCount),
                            false),
                    }
                    .Sum(content =>
                        TableStyle.ToolbarButton.CalcSize(content).x
                        + TableStyle.ToolbarButton.margin.horizontal);

            bool compact =
                cardWidth - buttonsWidth - BrowserStyle.SearchFieldInsetLeft - BrowserStyle.SearchFieldInsetRight
                < TableStyle.ToolbarMinSearchWidth;

            if (compact == _toolbarCompact)
                return;

            _toolbarCompact = compact;

            LoreRepaintService.Repaint();
        }

        private static void BeginAddingTag(IReadOnlyList<Entry> selectedRows)
        {
            TagAssignmentContext.Begin(selectedRows);

            LibraryService.OpenTagLibrary?.Invoke();
        }

        // "Boss (2/3)": most common first, then by title.
        private void ShowRemoveTagMenu(IReadOnlyList<Entry> selectedRows)
        {
            TagCollection collection = _tagService.GetCollection();

            if (collection == null)
                return;

            List<LoreMenuItem> items =
                selectedRows
                    .Where(entry => entry != null)
                    .SelectMany(entry => entry.TagIds)
                    .GroupBy(tagId => tagId)
                    .Select(group => (Tag: collection.FindById(group.Key), Count: group.Count()))
                    .Where(item => item.Tag != null)
                    .OrderByDescending(item => item.Count)
                    .ThenBy(item => item.Tag.Title, System.StringComparer.CurrentCultureIgnoreCase)
                    .Select(item =>
                        new LoreMenuItem(
                            string.Format(
                                LoreText.Inspector.TableSelectionTag,
                                item.Tag.Title,
                                item.Count,
                                selectedRows.Count),
                            () => _entryService.RemoveTag(selectedRows, item.Tag)))
                    .ToList();

            if (items.Count == 0)
                return;

            LoreActionMenu.Show(_removeTagRect, items);
        }

        private void ConfirmDeleteSelected(IReadOnlyList<Entry> selectedRows)
        {
            List<Entry> targets = selectedRows.Where(entry => entry != null).ToList();

            if (targets.Count == 0)
                return;

            List<string> lines =
                targets
                    .Take(MaxDeleteListed)
                    .Select(entry =>
                        string.Format(
                            LoreText.Dialogs.DeleteListItem,
                            string.Format(LoreText.Drawers.GlyphTitle, entry.Glyph, entry.Title)))
                    .ToList();

            if (targets.Count > MaxDeleteListed)
                lines.Add(string.Format(LoreText.Dialogs.DeleteListMore, targets.Count - MaxDeleteListed));

            LoreDialog.ConfirmDestructive(
                string.Format(
                    LoreText.Dialogs.DeleteSelectedTitle,
                    LoreText.Common.EntryCount.Format(targets.Count)),
                LoreText.Common.Paragraphs(
                    LoreText.Dialogs.EntriesDeleteWarning.Format(targets.Count),
                    string.Join("\n", lines),
                    new EntryDeleteImpactAnalyzer().Describe(targets),
                    LoreText.Common.CannotBeUndone),
                LoreText.Common.DeleteButton,
                () =>
                {
                    // Before deleting: the deleted Entry objects compare as null afterwards.
                    bool currentDeleted = targets.Contains(SelectionService.CurrentEntry);

                    int deleted = _entryService.DeleteEntries(targets);

                    if (deleted > 0 && currentDeleted)
                        SelectionService.Clear();

                    LoreRepaintService.Repaint();
                });
        }
        #endregion

        #region Rows (search and sorting)
        private IReadOnlyList<Entry> GetRows(
            IReadOnlyList<Entry> entries,
            IReadOnlyList<Field> fields)
        {
            // A removed Field cannot sort anymore.
            if (_sortField != null &&
                !fields.Contains(_sortField))
            {
                _sortField = null;
                _sortDescending = false;
            }

            string key = $"{_category.Id}|{Search}|{(_sortField != null ? _sortField.Id : string.Empty)}|{_sortByTitle}|{_sortDescending}";

            bool stateChanged = key != _rowsKey;
            bool dataChanged = _rowsVersion != LoreDataVersion.Current;

            if (_rows != null &&
                !stateChanged &&
                (!dataChanged || IsEditingCellText()))
            {
                return _rows;
            }

            _rows = Sort(Filter(entries)).ToList();
            _rowsKey = key;
            _rowsVersion = LoreDataVersion.Current;

            return _rows;
        }

        private IEnumerable<Entry> Filter(IReadOnlyList<Entry> entries)
        {
            if (string.IsNullOrWhiteSpace(Search))
                return entries;

            string search = Search.Trim();

            return entries.Where(
                entry =>
                    _matcher.Matches(entry, search) ||
                    _matcher.MatchesContent(entry, search));
        }

        // Empty values always go last; equal values keep Lore's order (stable sort).
        private IEnumerable<Entry> Sort(IEnumerable<Entry> entries)
        {
            if (_sortField == null && !_sortByTitle)
                return entries;

            var keyed =
                entries
                    .Select(entry => (Entry: entry, Key: GetSortKey(entry)))
                    .OrderBy(item => item.Key == null ? 1 : 0);

            keyed =
                _sortDescending
                    ? keyed.ThenByDescending(item => item.Key, TableSortComparer.Instance)
                    : keyed.ThenBy(item => item.Key, TableSortComparer.Instance);

            return keyed.Select(item => item.Entry);
        }

        // Comparable key of the sorted column, or null when the value is empty (or missing).
        private object GetSortKey(Entry entry)
        {
            if (_sortByTitle)
                return entry.Title;

            if (!entry.TryGetValue(_sortField, out FieldValue fieldValue))
                return null;

            switch (_sortField.Type)
            {
                case FieldType.Text:
                case FieldType.Url:
                    return string.IsNullOrEmpty(fieldValue.StringValue) ? null : fieldValue.StringValue;

                case FieldType.Integer:
                    return fieldValue.IntValue;

                case FieldType.Float:
                    return fieldValue.FloatValue;

                case FieldType.Boolean:
                    return fieldValue.BoolValue;

                case FieldType.Enum:
                    int index = _sortField.GetEnumOptionIndex(fieldValue.EnumValue);
                    return index >= 0 ? (object)index : null;

                case FieldType.Entry:
                    return fieldValue.EntryValue != null ? fieldValue.EntryValue.Title : null;

                case FieldType.Entries:
                    int count = FieldValueUtility.GetReferencedEntries(fieldValue).Count();
                    return count > 0 ? (object)count : null;

                case FieldType.GameObject:
                    return fieldValue.GameObjectValue != null ? fieldValue.GameObjectValue.name : null;

                case FieldType.Sprite:
                    return fieldValue.SpriteValue != null ? fieldValue.SpriteValue.name : null;

                case FieldType.ScriptableObject:
                case FieldType.Audio:
                    return fieldValue.ObjectValue != null ? fieldValue.ObjectValue.name : null;

                case FieldType.Scene:
                    return string.IsNullOrEmpty(fieldValue.ScenePath)
                        ? null
                        : System.IO.Path.GetFileNameWithoutExtension(fieldValue.ScenePath);

                default:
                    return null;
            }
        }

        // Texts compare case-insensitively; all other keys of a column share one type (numbers, bools).
        private sealed class TableSortComparer : IComparer<object>
        {
            public static readonly TableSortComparer Instance = new();

            public int Compare(object a, object b)
            {
                if (a == null || b == null)
                    return 0;

                if (a is string textA && b is string textB)
                    return string.Compare(textA, textB, System.StringComparison.CurrentCultureIgnoreCase);

                return a is System.IComparable comparable
                    ? comparable.CompareTo(b)
                    : 0;
            }
        }
        #endregion

        #region Helpers
        private static IReadOnlyList<Entry> GetEntries(Category category)
        {
            return
                LoreRepository.Instance.Entries
                    .Where(entry => entry != null && entry.Category == category)
                    .OrderBy(entry => entry.SortOrder)
                    .ToList();
        }

        // Broken (null) Fields are left out; they are reported by the Category integrity callout.
        private static IReadOnlyList<Field> GetFields(Category category)
        {
            return
                category.Fields
                    .Where(field => field != null)
                    .ToList();
        }

        private bool IsRequired(Field field) =>
            _category != null &&
            _category.IsRequired(field);

        // Row under the mouse (name column or cells), or -1. Registers the row rects as hover regions, so the
        // highlight follows the mouse (see LoreHover).
        private int GetHoveredRow(
            TableLayout layout,
            int firstRow,
            int lastRow)
        {
            if (GUIUtility.hotControl != 0)
                return -1;

            int hovered = -1;

            for (int row = firstRow; row <= lastRow; row++)
            {
                float y = layout.Body.y + row * TableStyle.RowHeight - _scroll.y;
                float yMin = Mathf.Max(y, layout.Body.y);
                float yMax = Mathf.Min(y + TableStyle.RowHeight, layout.Body.yMax);

                if (yMax <= yMin)
                    continue;

                Rect rowRect =
                    new(
                        layout.Table.x,
                        yMin,
                        layout.Body.xMax - layout.Table.x,
                        yMax - yMin);

                if (LoreHover.Contains(rowRect))
                    hovered = row;
            }

            return hovered;
        }

        private static void GetVisibleRange(
            float scroll,
            float visibleSize,
            float itemSize,
            int count,
            out int first,
            out int last)
        {
            // Nothing to draw (e.g. a new Category without Fields): empty range.
            if (count <= 0)
            {
                first = 0;
                last = -1;
                return;
            }

            first = Mathf.Clamp(Mathf.FloorToInt(scroll / itemSize), 0, Mathf.Max(0, count - 1));
            last = Mathf.Clamp(Mathf.CeilToInt((scroll + visibleSize) / itemSize) - 1, first, count - 1);
        }

        private void ClampScroll(TableLayout layout)
        {
            _scroll = Clamp(_scroll, layout.MaxScroll);
        }

        private static Vector2 Clamp(Vector2 scroll, Vector2 max) =>
            new(
                Mathf.Clamp(scroll.x, 0f, max.x),
                Mathf.Clamp(scroll.y, 0f, max.y));

        // Grid lines stay visible next to controls and Entry cards.
        private static Rect GetCellContentRect(Rect cell) =>
            new(
                cell.x + TableStyle.ControlPadding,
                cell.y,
                Mathf.Max(0f, cell.width - TableStyle.ControlPadding * 2f - 1f),
                cell.height - 1f);

        private static Rect GetPaddedRect(Rect rect) =>
            new(
                rect.x + TableStyle.CellPadding,
                rect.y,
                Mathf.Max(0f, rect.width - TableStyle.CellPadding * 2f),
                rect.height);

        // Single-line controls (text, numbers, toggles, object fields) are vertically centered in the cell.
        private static Rect GetCenteredControlRect(Rect rect)
        {
            float height = Mathf.Min(EditorGUIUtility.singleLineHeight, rect.height);

            return new Rect(rect.x, rect.center.y - height * 0.5f, rect.width, height);
        }

        // Width of the columns in view (narrower than the body when the Category has only a few Fields).
        private float GetVisibleContentWidth(TableLayout layout) =>
            Mathf.Max(0f, Mathf.Min(layout.Body.width, layout.Content.x - _scroll.x));

        // Only visible cells are drawn, so scrolling changes which controls exist. A focused text input that
        // scrolls out would hand its keyboard focus to another cell: end the edit instead (row values are saved
        // on every keystroke; the selection row applies on Enter or focus loss).
        private void ClearFocusOnScroll()
        {
            if (_scroll == _drawnScroll)
                return;

            _drawnScroll = _scroll;

            if (IsEditingCellText())
                LoreGUIFocus.Clear();
        }

        // A value cell's or the selection row's text input has the focus (not the search field or inputs elsewhere).
        private static bool IsEditingCellText()
        {
            if (!EditorGUIUtility.editingTextField)
                return false;

            string focused = GUI.GetNameOfFocusedControl();

            return
                focused.StartsWith(FieldValueElement.ControlNamePrefix, System.StringComparison.Ordinal) ||
                focused.StartsWith(TableSelectionRow.ControlNamePrefix, System.StringComparison.Ordinal);
        }

        private static void ClearFieldValue(
            Entry entry,
            FieldValue fieldValue)
        {
            LoreUndo.Record(
                entry,
                LoreText.Undo.ClearFieldValue);

            fieldValue.Clear();

            LoreChangeTracker.MarkModified(entry);

            LoreRelationshipIndex.Instance.Refresh();

            LoreRepaintService.Repaint();

            EntryAssignmentContext.EndAssignment();
        }
        #endregion
    }
}
