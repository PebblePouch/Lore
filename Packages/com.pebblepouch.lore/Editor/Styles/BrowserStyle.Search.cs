using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static partial class BrowserStyle
    {
        #region Control Names
        // Technical IMGUI control name prefix of the Library searches ("ItemBrowserSearch_FieldLibraryItem" etc.).
        public const string LibrarySearchControlNamePrefix = "ItemBrowserSearch_";
        #endregion

        #region Card
        public const float SearchCardHeight = 29f;

        public static readonly Color SearchCardBackground = LorePalette.Surface3;

        public static readonly GUIStyle SearchCard = new GUIStyle
        {
            padding = new RectOffset(0, 0, 0, 0),   // Inner
            margin = new RectOffset(0, 0, 0, 0)     // Outer
        };
        #endregion

        #region Search
        public const float SearchFieldHeight = 19f;

        public const float SearchFieldInsetLeft = 5f;
        public const float SearchFieldInsetRight = 10f;
        public const float SearchFieldInsetTop = 5f;

        public static readonly GUIStyle SearchField = new(EditorStyles.textField)
        {
            padding = new RectOffset(4, 4, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),

            fontSize = LoreTypography.Small,
            alignment = TextAnchor.MiddleLeft,

            normal = { textColor = LorePalette.Text },
            hover = { textColor = LorePalette.Text },
            active = { textColor = LorePalette.Text },
        };
        #endregion

        #region Placeholder
        public const float SearchPlaceholderInsetLeft = 1f;

        public static readonly GUIStyle SearchPlaceholderLabel = new(EditorStyles.label)
        {
            padding = new RectOffset(4, 4, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),

            fontSize = LoreTypography.Small,
            fontStyle = FontStyle.Italic,
            alignment = TextAnchor.MiddleLeft,

            normal = { textColor = LorePalette.TextMuted },
            hover = { textColor = LorePalette.TextMuted },
            active = { textColor = LorePalette.TextMuted },
        };
        #endregion

        #region Button
        public static readonly GUIStyle SearchActionButton = new(EditorStyles.miniButton)
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 6, 5, 0),

            fixedWidth = 18,
            fixedHeight = 18,

            fontSize = LoreTypography.Tiny,
            alignment = TextAnchor.MiddleCenter,

            normal = { textColor = LorePalette.Text },
            hover = { textColor = LorePalette.Text },
            active = { textColor = LorePalette.Text },
        };
        #endregion
    }
}