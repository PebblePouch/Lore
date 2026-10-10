using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static class LibraryStyle
    {
        #region Glyphs
        // Text lives in LoreText.Library; this is a visual icon.
        public static readonly string MenuString = "\U000022EE";
        #endregion

        #region Layout
        public const float ColumnMargin = 4f;
        public const float ColumnSpacing = 6f;

        public const float CardHeight = 27f;
        public const float CardSpacing = 1f;

        public const float SwatchSize = 18f;

        public const float GlyphWidth = 28f;
        public const float MetadataWidth = 60f;

        // Field cards: the type gets this share of the title + type width (long ScriptableObject type names).
        public const float FieldCardMetadataRatio = 0.5f;
        public const float ButtonWidth = 22f;
        public const float MenuWidth = 16f;
        public const float CardMenuVerticalInset = 2f;

        public const float CollectionEditorSpacing = 8f;

        public const float InactiveAlphaMultiplier = 0.40f;

        public const float FeedbackHeight = 24f;
        public const float FeedbackVerticalPadding = 3f;
        public const float FeedbackHorizontalPadding = 8f;

        public const float ListTopSpacing = 1f;
        #endregion

        #region Card
        public static readonly GUIStyle Card = new()
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),
        };

        public static readonly Color CardInactive = LorePalette.Background;
        public static readonly Color CardHighlightInactive = LorePalette.Surface1;

        public static readonly Color CardNormal = LorePalette.Surface2;
        public static readonly Color CardHover = LorePalette.Surface4;
        public static readonly Color CardActive = LorePalette.Accent;
        #endregion

        #region Glyph
        public static readonly GUIStyle Glyph = new(EditorStyles.label)
        {
            fontSize = LoreTypography.GlyphMedium,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleCenter,

            normal = { textColor = Color.white },
            hover = { textColor = Color.white },
            active = { textColor = Color.white },
        };
        #endregion

        #region Title
        public static readonly GUIStyle Title = new(EditorStyles.label)
        {
            fontSize = LoreTypography.Body,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleLeft,
            clipping = TextClipping.Clip,

            normal = { textColor = LorePalette.TextStrong },
            hover = { textColor = LorePalette.TextStrong },
            active = { textColor = LorePalette.TextStrong },
        };

        public static readonly GUIStyle Placeholder = new(EditorStyles.miniLabel)
        {
            fontSize = LoreTypography.Small,
            fontStyle = FontStyle.Italic,

            normal = { textColor = LorePalette.TextFaint },
            hover = { textColor = LorePalette.TextFaint },
            active = { textColor = LorePalette.TextFaint },
        };

        public static readonly LibraryInlineTextFieldSettings TitleField =
            new()
            {
                Viewer = Title,
                Editor = Title,
                Placeholder = Placeholder,

                Height = EditorGUIUtility.singleLineHeight,

                HorizontalInset = 0f,
                VerticalInset = 0f,
            };
        #endregion

        #region Tags
        public static readonly Color DefaultTagColor = Color.black;
        #endregion

        #region Metadata (Type/Category)
        public static readonly GUIStyle Metadata = new()
        {
            fontSize = LoreTypography.Small,
            fontStyle = FontStyle.Italic,
            alignment = TextAnchor.MiddleRight,
            clipping = TextClipping.Clip,

            normal = { textColor = LorePalette.TextMuted },
            hover = { textColor = LorePalette.TextMuted },
            active = { textColor = LorePalette.TextMuted },
        };

        // ScriptableObject Field whose type cannot be resolved (integrity issue).
        public static readonly GUIStyle MetadataBroken = new(Metadata)
        {
            normal = { textColor = LorePalette.Danger },
            hover = { textColor = LorePalette.Danger },
            active = { textColor = LorePalette.Danger },
        };

        // Unset ScriptableObject type in the Field card ("Choose type").
        public static readonly GUIStyle MetadataPlaceholder = new(Metadata)
        {
            normal = { textColor = LorePalette.TextFaint },
            hover = { textColor = LorePalette.TextFaint },
            active = { textColor = LorePalette.TextFaint },
        };
        #endregion

        #region Enum Options (Field card, edit mode)
        public const float OptionsVerticalPadding = 4f;
        public const float OptionRowHeight = 20f;
        public const float OptionRowSpacing = 2f;
        public const float OptionHandleWidth = 16f;
        public const float OptionAddButtonWidth = 110f;

        public static readonly GUIStyle OptionField = new(EditorStyles.textField)
        {
            fontSize = LoreTypography.Small,
            alignment = TextAnchor.MiddleLeft,
        };

        public static readonly GUIStyle OptionHandle = new()
        {
            fontSize = LoreTypography.IconMedium,
            alignment = TextAnchor.MiddleCenter,

            normal = { textColor = LorePalette.TextMuted },
            hover = { textColor = LorePalette.TextMuted },
            active = { textColor = LorePalette.TextMuted },
        };

        // Entry / Entries Fields: "Categories  [All Categories ▾]" (same row height as the Enum options); also the
        // "Dimension" row of Vector Fields.
        public const float AllowedCategoriesLabelWidth = 80f;

        public static readonly GUIStyle AllowedCategoriesLabel = new()
        {
            fontSize = LoreTypography.Small,
            fontStyle = FontStyle.Italic,
            alignment = TextAnchor.MiddleLeft,

            normal = { textColor = LorePalette.TextMuted },
        };

        // Technical: control name prefix of the option text fields ("EnumOption_2").
        public const string OptionControlNamePrefix = "EnumOption_";
        #endregion

        #region ScriptableObject Type Dropdown
        public const float ObjectTypeDropdownWidth = 280f;
        public const float ObjectTypeDropdownHeight = 320f;
        #endregion

        #region Button
        public static readonly GUIStyle Button = new(EditorStyles.miniButton)
        {
            fontSize = LoreTypography.Tiny,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleCenter,

            normal = { textColor = Color.white },
            hover = { textColor = Color.white },
            active = { textColor = Color.white },
        };
        #endregion

        #region Menu
        public static readonly GUIStyle Menu = new()
        {
            fontSize = LoreTypography.IconMedium,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleCenter,

            normal = { textColor = Color.white },
            hover = { textColor = Color.white },
            active = { textColor = Color.white },
        };
        #endregion

        #region Empty
        public static readonly GUIStyle Empty = new(EditorStyles.boldLabel)
        {
            fontSize = LoreTypography.Body,
            fontStyle = FontStyle.Italic,
            alignment = TextAnchor.MiddleLeft,

            normal = { textColor = LorePalette.TextFaint },
            hover = { textColor = LorePalette.TextFaint },
            active = { textColor = LorePalette.TextFaint },
        };
        #endregion

        #region Feedback
        public static readonly GUIStyle Feedback = new(EditorStyles.wordWrappedMiniLabel)
        {
            fontSize = LoreTypography.Tiny,
            fontStyle = FontStyle.Italic,
            alignment = TextAnchor.MiddleLeft,
            wordWrap = true,

            normal = { textColor = LorePalette.Text },
        };
        #endregion
    }
}