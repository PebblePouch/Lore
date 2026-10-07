using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Quick Search (Ctrl+F): popup window (QuickSearchWindow) centered over the Lore window.
    public static class QuickOpenStyle
    {
        #region Layout
        public const float MaxWidth = 520f;
        public const float SideMargin = 16f;
        public const float Padding = 6f;
        public const float SearchHeight = 22f;
        public const float RowHeight = 22f;
        public const float RowSpacing = 1f;
        public const float GlyphWidth = 24f;
        public const float DetailWidthRatio = 0.4f;
        public const float BorderWidth = 2f;

        // Header like the browser's ("Contents"): same height and colors, 1px gap to the body.
        public const float HeaderHeight = BrowserStyle.BrowserHeaderHeight;
        public const float HeaderSpacing = 1f;
        public const float HeaderInset = 6f;
        public const int MaxResults = 12;
        #endregion

        #region Colors
        public static readonly Color Blocker = new(0f, 0f, 0f, 0.35f);
        public static readonly Color Border = LorePalette.Shadow;
        // Same as the browser search bar: stands out against the dimmed window.
        public static readonly Color HeaderBackground = BrowserStyle.BrowserHeaderBackground;
        public static readonly Color Background = BrowserStyle.SearchCardBackground;
        public static readonly Color RowSelected = LorePalette.Accent;
        public static readonly Color RowHover = LorePalette.HoverOverlay;
        #endregion

        #region Styles
        public static readonly GUIStyle Search = new(EditorStyles.textField)
        {
            fontSize = LoreTypography.Body,
            alignment = TextAnchor.MiddleLeft,
        };

        public static readonly GUIStyle Header = new(BrowserStyle.BrowserHeaderContent)
        {
            margin = new RectOffset(0, 0, 0, 0),
        };

        public static readonly GUIStyle Glyph = new()
        {
            fontSize = LoreTypography.Body,
            alignment = TextAnchor.MiddleCenter,

            normal = { textColor = LorePalette.Text },
        };

        public static readonly GUIStyle Title = new()
        {
            fontSize = LoreTypography.Body,
            alignment = TextAnchor.MiddleLeft,
            clipping = TextClipping.Clip,

            normal = { textColor = LorePalette.Text },
        };

        public static readonly GUIStyle Detail = new()
        {
            fontSize = LoreTypography.Small,
            fontStyle = FontStyle.Italic,
            alignment = TextAnchor.MiddleRight,
            clipping = TextClipping.Clip,

            normal = { textColor = LorePalette.TextMuted },
        };

        public static readonly GUIStyle Hint = new(Detail)
        {
            alignment = TextAnchor.MiddleLeft,
        };
        #endregion

        #region Control Names
        // Technical IMGUI control name of the search field.
        public const string SearchControlName = "LoreQuickOpenSearch";
        #endregion
    }
}
