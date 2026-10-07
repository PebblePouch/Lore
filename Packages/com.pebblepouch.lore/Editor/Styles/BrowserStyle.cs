using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static partial class BrowserStyle
    {
        #region Glyphs
        public const string BurgerMenuString = "\U00002261";        // Burger Menu

        public const string FoldoutExpandedString = "\U000025BC";   // Arrow Down
        public const string FoldoutCollapsedString = "\U000025B6";  // Arrow Right

        public const string NoGlyphString = "?";                // Question Mark
        #endregion

        #region Card
        public const float CardHeight = 40f;

        public const float ListTopSpacing = 1f;                 // Small space at the top to allow the first DropLine to render
        public const float ListBottomSpacing = 0f;              // Small space at the bottom to allow the last DropLine to render

        public const float GroupTopSpacing = 0f;                // No space at the top of each Group
        public const float GroupBottomSpacing = 0f;             // No space at the bottom of each Group

        public const float CategoryCardBottomSpacing = 1f;      // No space at the bottom of each Category card

        public const float EntryCardTopSpacing = 0f;            // Small space at the top of each Entry card
        public const float EntryCardBottomSpacing = 1f;         // No space at the bottom of each Entry card

        public static readonly Color CategoryCardNormal = LorePalette.Surface3;
        public static readonly Color CategoryCardHover = LorePalette.Surface5;
        public static readonly Color CategoryCardActive = LorePalette.Accent;

        public static readonly Color EntryCardNormal = LorePalette.Surface2;
        public static readonly Color EntryCardHover = LorePalette.Surface4;
        public static readonly Color EntryCardActive = LorePalette.Accent;

        public static readonly GUIStyle Card = new GUIStyle
        {
            padding = new RectOffset(0, 0, 0, 0),               // Inner
            margin = new RectOffset(0, 0, 0, 0),                // Outer
        };
        #endregion

        #region Drag'n'Drop
        public const float HandleColumnWidth = 19f;

        public static readonly float DropLineHeight = 2f;
        public static readonly float DropLineVerticalOffset = -2f;
        public static readonly Color DropLineColor = LorePalette.DropIndicator;

        public static readonly GUIStyle HandleContent = new()
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),

            fontSize = LoreTypography.IconMedium,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleCenter,

            normal = { textColor = LorePalette.Text },
            hover = { textColor = LorePalette.Text },
            active = { textColor = LorePalette.Text },
        };
        #endregion

        #region Foldout
        public const float FoldoutColumnWidth = 13f;

        public static readonly GUIStyle Foldout = new(EditorStyles.label)
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),

            fontSize = LoreTypography.Tiny,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleCenter,

            normal = { textColor = LorePalette.Text },
            hover = { textColor = LorePalette.Text },
            active = { textColor = LorePalette.Text },
        };
        #endregion

        #region Glyph
        public const float GlyphColumnWidth = 40f;

        public static readonly GUIStyle Glyph = new GUIStyle()
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, -2, 0),

            fontSize = LoreTypography.GlyphLarge,
            alignment = TextAnchor.MiddleCenter,
        };
        #endregion

        #region Title
        public static readonly GUIStyle Title = new GUIStyle(UnityEditor.EditorStyles.boldLabel)
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, -1, 0),

            fontSize = LoreTypography.Body,
            fontStyle = FontStyle.Bold,

            normal = { textColor = LorePalette.TextStrong },
            hover = { textColor = LorePalette.TextStrong },
            active = { textColor = LorePalette.TextStrong },
        };
        #endregion

        #region Subtitle
        public static readonly GUIStyle Subtitle = new GUIStyle(UnityEditor.EditorStyles.miniLabel)
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 1, 0),

            fontSize = LoreTypography.Small,
            fontStyle = FontStyle.Italic,

            normal = { textColor = LorePalette.TextMuted },
            hover = { textColor = LorePalette.TextMuted },
            active = { textColor = LorePalette.TextMuted },
        };
        #endregion

        #region Fields
        public static readonly GUIStyle Fields = new GUIStyle(UnityEditor.EditorStyles.miniLabel)
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 2, 0),

            fontSize = LoreTypography.Small,
            fontStyle = FontStyle.Normal,

            normal = { textColor = LorePalette.Text },
            hover = { textColor = LorePalette.Text },
            active = { textColor = LorePalette.Text },
        };
        #endregion

        #region Button
        public const float ButtonColumnWidth = 25f;

        public static readonly GUIStyle Button = new(EditorStyles.miniButton)
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 6, 6, 0),

            fixedWidth = 25,
            fixedHeight = 25,

            fontSize = LoreTypography.IconSmall,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleCenter,
        };
        #endregion

    }

}
