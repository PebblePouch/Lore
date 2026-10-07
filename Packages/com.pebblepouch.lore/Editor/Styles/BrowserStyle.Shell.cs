using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static partial class BrowserStyle
    {
        #region Constructor
        // The only static constructor of the partial BrowserStyle (C# allows one per class).
        // It runs after the field initializers of all parts.
        static BrowserStyle()
        {
            BrowserHeaderLayout.normal.background =
                GUITextureUtility.GetTexture(BrowserHeaderBackground);
        }
        #endregion

        #region Browser
        public const float ViewportBottomInset = 4f; // 2px toolbar-header, 1px header-search, 1px search-library

        public const float BrowserWidth = 300f;

        public static readonly GUIStyle BrowserLayout = new GUIStyle()
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),
        };
        #endregion

        #region Header
        public static readonly Color BrowserHeaderBackground = LorePalette.Surface4;

        public const float BrowserHeaderHeight = 37f;

        public static readonly GUIStyle BrowserHeaderLayout = new()
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 0, 1),
        };

        public static readonly GUIStyle BrowserHeaderContent = new()
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(6, 0, 0, 0),

            fontSize = LoreTypography.Title,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,

            normal = { textColor = LorePalette.TextSubtle },
            hover = { textColor = LorePalette.TextSubtle },
            active = { textColor = LorePalette.TextSubtle },
        };
        #endregion

        #region Button
        public static readonly GUIStyle BrowserHeaderButton = new(EditorStyles.miniButton)
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

        #region Badge
        public static readonly GUIStyle BrowserHeaderBlank = new()
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 6, 6, 0),

            fixedWidth = 25,
            fixedHeight = 25,

            fontSize = LoreTypography.IconSmall,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleCenter,

            normal = { textColor = LorePalette.TextHidden },
            hover = { textColor = LorePalette.TextHidden },
            active = { textColor = LorePalette.TextHidden },
        };
        #endregion

        #region Filter
        public const float FilterHeight = 20f;
        public const float FilterButtonSize = 18f;
        public const float FilterHorizontalPadding = 6f;

        public const float FilterLabelTopSpace = -2f;

        public const float FilterButtonTopSpace = -1f;
        public const float FilterButtonRightSpace = 6f;

        public const float FilterVerticalInset = 0f;

        public static readonly Color FilterBackground = LorePalette.Surface3;

        public static readonly GUIStyle FilterLabel = new(EditorStyles.label)
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),

            fontSize = LoreTypography.Small,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleLeft,
            clipping = TextClipping.Clip,

            normal = { textColor = LorePalette.TextMuted },
            hover = { textColor = LorePalette.TextMuted },
            active = { textColor = LorePalette.TextMuted },
        };

        public static readonly GUIStyle FilterButton = new(EditorStyles.miniButton)
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),

            fixedWidth = FilterButtonSize,
            fixedHeight = FilterButtonSize,

            fontSize = LoreTypography.Tiny,
            alignment = TextAnchor.MiddleCenter,

            normal = { textColor = LorePalette.Text },
            hover = { textColor = LorePalette.Text },
            active = { textColor = LorePalette.Text },
        };
        #endregion
    }
}