using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static class MenuStyle
    {
        #region Glyphs
        public const string SelectedString = "\u2713";
        #endregion

        #region Layout
        public const float Width = 150f;
        public const float RowHeight = 25f;

        public const float VerticalEdgeMargin = 0f;
        public const float HorizontalEdgeMargin = 5f;

        public const float ColumnSpacing = 5f;

        public const float GlyphWidth = 16f;

        public const float SeparatorHeight = 1f;
        #endregion

        #region Colors
        public static readonly Color Background =
            LorePalette.Surface3;

        public static readonly Color RowHover =
            LorePalette.MenuRowHover;

        public static readonly Color Separator =
            LorePalette.Divider;
        #endregion

        #region Styles
        public static readonly GUIStyle Row = new(EditorStyles.label)
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),

            fontSize = LoreTypography.Body,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleLeft,

            normal = { textColor = Color.white },
            hover = { textColor = Color.white },
            active = { textColor = Color.white },
        };

        public static readonly GUIStyle DestructiveRow = new(Row)
        {
            normal = { textColor = LorePalette.Danger },
            hover = { textColor = LorePalette.Danger },
            active = { textColor = LorePalette.Danger },
        };

        public static readonly GUIStyle Glyph = new(EditorStyles.label)
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),

            fontSize = LoreTypography.GlyphSmall,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleCenter,

            normal = { textColor = LorePalette.Text },
            hover = { textColor = LorePalette.Text },
            active = { textColor = LorePalette.Text },
        };
        #endregion
    }
}