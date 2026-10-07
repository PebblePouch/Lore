using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Tabular Editing (Category page): Entries as rows, Fields as columns. Header row and name column stay
    // visible (sticky) while the cells scroll.
    public static class TableStyle
    {
        #region Layout
        // Column widths: tune these to change how many Field columns fit next to the name column.
        public const float NameColumnWidth = 180f + CheckboxColumnWidth;
        public const float ColumnWidth = 240f;

        // Selection checkbox before the glyph in the name column (the column is wider by this).
        public const float CheckboxWidth = 16f;
        public const float CheckboxColumnWidth = CellPadding + CheckboxWidth;

        public const float HeaderHeight = 25f;
        public const float RowHeight = InspectorStyle.FieldTableRowHeight;

        public const float TopSpacing = 6f;
        public const float CellPadding = 6f;
        // Value controls and Entry cards: inset from the cell edges (grid lines stay visible).
        public const float ControlPadding = 3f;
        public const float GlyphWidth = InspectorStyle.FieldGlyphColumnWidth;
        public const float GlyphSpacing = 4f;

        // The table is at most as tall as the Inspector viewport minus this (section header, card padding),
        // so it fills the view once the page is scrolled to it.
        public const float ViewportMargin = 60f + BrowserStyle.SearchCardHeight;

        // Sorted column: ▲ ascending, ▼ descending at the right edge of the header cell.
        public const float SortIndicatorWidth = 12f;
        public const string SortAscendingGlyph = "▲";
        public const string SortDescendingGlyph = "▼";
        public const float MinHeight = HeaderHeight + RowHeight * 3f;

        // Selection row: Entries chips wrap up to this many lines (the row grows), the rest becomes a "+N" chip.
        public const int SelectionChipMaxLines = 3;

        // Bulk action buttons next to the search: they turn glyph-only when the search would get narrower than this.
        public const float ToolbarMinSearchWidth = 160f;

        // Mouse wheel: pixels per wheel step (Unity scroll views use 20).
        public const float WheelSpeed = 20f;
        #endregion

        #region Colors
        public static readonly Color HeaderBackground = LorePalette.Surface3;
        public static readonly Color NameColumnBackground = LorePalette.Surface2;
        public static readonly Color GridLine = LorePalette.Divider;
        public static readonly Color RowHover = LorePalette.HoverOverlay;
        // Selected rows (bulk editing): covers health and integrity marks.
        public static readonly Color RowSelected = InspectorStyle.FieldSelectBackground;
        // Selection row (bulk editing), sticky under the header.
        public static readonly Color SelectionRowBackground = InspectorStyle.FieldFilterBackground;
        public static readonly Color HealthBackground = InspectorStyle.FieldHealthBackground;
        public static readonly Color BrokenBackground = InspectorStyle.FieldBrokenBackground;
        #endregion

        #region Styles
        public static readonly GUIStyle Glyph = new(InspectorStyle.FieldGlyph)
        {
            clipping = TextClipping.Clip,
        };

        public static readonly GUIStyle HeaderTitle = new(InspectorStyle.FieldTitle)
        {
            fontStyle = FontStyle.Bold,
        };

        public static readonly GUIStyle CornerTitle = new(InspectorStyle.FieldTitle)
        {
            normal = { textColor = LorePalette.TextMuted },
        };

        public static readonly GUIStyle NameTitle = new(InspectorStyle.FieldTitle);

        public static readonly GUIStyle SelectionCount = new(InspectorStyle.FieldTitle)
        {
            fontStyle = FontStyle.Bold,
        };

        public static readonly GUIStyle CellText = new(InspectorStyle.FieldTitle);

        public static readonly GUIStyle CellTextMuted = new(InspectorStyle.FieldTitle)
        {
            normal = { textColor = LorePalette.TextMuted },
        };

        public static readonly GUIStyle SortIndicator = new(InspectorStyle.FieldTypeLabel)
        {
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleCenter,
        };

        // Bulk action buttons: like the search clear button, with text, just as wide as glyph and text need
        // (glyph only: same size as that button).
        public static readonly GUIStyle ToolbarButton = new(BrowserStyle.SearchActionButton)
        {
            // No fixed width: it must not stretch either (the search field takes the rest).
            fixedWidth = 0,
            stretchWidth = false,
            padding = new RectOffset(6, 6, 0, 0),
            margin = new RectOffset(0, 6, 5, 0),
            fontSize = LoreTypography.Small,
        };

        public static readonly GUIStyle ToolbarGlyphButton = new(BrowserStyle.SearchActionButton);

        public static readonly GUIStyle EmptyText = new(InspectorStyle.FieldTitle)
        {
            normal = { textColor = LorePalette.TextMuted },
        };
        #endregion
    }
}
