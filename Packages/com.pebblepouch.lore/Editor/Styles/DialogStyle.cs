using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static class DialogStyle
    {
        #region Layout
        public const float Width = 300f;
        public const float Height = 150f;

        public const float BorderWidth = 3f;

        public const float EdgeMargin = 7f;
        public const float SectionSpacing = 10f;
        public const float ButtonSpacing = 6f;

        public const float TitleHeight = 27f;
        public const float ButtonHeight = 27f;
        #endregion

        #region Colors
        public static readonly Color Blocker = LorePalette.Clear;

        public static readonly Color Border = LorePalette.Shadow;

        public static readonly Color Background = LorePalette.Background;
        public static readonly Color HeaderBackground = LorePalette.Accent;

        public static readonly Color ButtonNormal = LorePalette.Surface3;
        public static readonly Color ButtonHover = LorePalette.Surface5;
        public static readonly Color DestructiveButton = LorePalette.DangerStrong;
        public static readonly Color DestructiveButtonHover = LorePalette.DangerStrongHover;
        #endregion

        #region Styles
        public static readonly GUIStyle Title = new(EditorStyles.boldLabel)
        {
            fontSize = LoreTypography.Body,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,

            normal = { textColor = LorePalette.TextStrong },
            hover = { textColor = LorePalette.TextStrong },
            active = { textColor = LorePalette.TextStrong },
        };

        public static readonly GUIStyle Message = new(EditorStyles.wordWrappedLabel)
        {
            fontSize = LoreTypography.Body,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.UpperLeft,
            wordWrap = true,
            richText = true,

            normal = { textColor = LorePalette.TextMuted },
            hover = { textColor = LorePalette.TextMuted },
            active = { textColor = LorePalette.TextMuted },
        };

        public static readonly GUIStyle Button = new(EditorStyles.label)
        {
            fontSize = LoreTypography.Body,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleCenter,

            normal = { textColor = Color.white },
            hover = { textColor = Color.white },
            active = { textColor = Color.white },
        };
        #endregion
    }
}