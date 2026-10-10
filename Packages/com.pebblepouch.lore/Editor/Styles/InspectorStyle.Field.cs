using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static partial class InspectorStyle
    {
        #region Glyphs
        public const string FieldHandleIcon = "\u2261";  // Burger Menu
        #endregion

        #region Layout
        public const float FieldTableTopSpacing = 10f;
        public const float FieldTableRowHeight = 25f;
        public const float FieldTableColumnSpacing = 10f;
        public const float FieldTableEdgeMargin = 3f;

        public const float FieldEntryLeftTabRatio = 0.5f;
        public const float FieldEntryRightTabRatio = 0.5f;
        #endregion

        #region Colors
        public static readonly Color FieldRowLineColor = LorePalette.Divider;
        public static readonly Color FieldDropLineColor = LorePalette.DropIndicator;

        public static readonly Color FieldHandleActiveColor = LorePalette.Text;
        public static readonly Color FieldHandleDisabledColor = LorePalette.Disabled;

        public static readonly Color FieldSelectBackground = LorePalette.Accent;
        public static readonly Color FieldFilterBackground = LorePalette.MatchSurface;
        // Integrity (broken) vs. health (unfinished), see LorePalette: integrity wins where both apply.
        public static readonly Color FieldBrokenBackground = LorePalette.DangerSurface;
        public static readonly Color FieldHealthBackground = LorePalette.WarningSurface;
        #endregion

        #region Handle
        public const float FieldHandleColumnWidth = 16f;

        public static readonly GUIStyle FieldHandleContent = new()
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),

            fontSize = LoreTypography.IconMedium,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleLeft,

            normal = { textColor = LorePalette.Text },
            hover = { textColor = LorePalette.Text },
            active = { textColor = LorePalette.Text },
        };
        #endregion

        #region FieldGlyph
        public const float FieldGlyphColumnWidth = 16f;

        public static readonly GUIStyle FieldGlyph = new()
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),

            fontSize = LoreTypography.GlyphSmall,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleLeft,

            normal = { textColor = LorePalette.Text },
            hover = { textColor = LorePalette.Text },
            active = { textColor = LorePalette.Text },
        };
        #endregion

        #region FieldTitle
        public const float FieldNameColumnWidth = 150f;

        public static readonly GUIStyle FieldTitle = new()
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),

            fontSize = LoreTypography.Body,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleLeft,
            clipping = TextClipping.Clip,

            normal = { textColor = LorePalette.Text },
            hover = { textColor = LorePalette.Text },
            active = { textColor = LorePalette.Text },
        };

        public static readonly GUIStyle MissingFieldTitleStyle = new(FieldTitle)
        {
            // In case we want to give the LoreText.Inspector.MissingFieldTitle a different styling than FieldTitle
        };
        #endregion

        #region FieldTypeLabel
        // Field tables (Category, Entry): the type gets this share of the name + type width.
        public const float FieldTypeColumnRatio = 1f / 3f;

        public static readonly GUIStyle FieldTypeLabel = new()
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),

            fontSize = LoreTypography.Small,
            fontStyle = FontStyle.Italic,
            alignment = TextAnchor.MiddleRight,
            clipping = TextClipping.Clip,

            normal = { textColor = LorePalette.TextMuted },
            hover = { textColor = LorePalette.TextMuted },
            active = { textColor = LorePalette.TextMuted },
        };

        // ScriptableObject Field whose type cannot be resolved (integrity issue).
        public static readonly GUIStyle FieldTypeLabelBroken = new(FieldTypeLabel)
        {
            normal = { textColor = LorePalette.Danger },
            hover = { textColor = LorePalette.Danger },
            active = { textColor = LorePalette.Danger },
        };
        #endregion

        #region Required Fields
        // Category field table: ✱ toggle (muted when optional). Entry field table: ✱ after the name, empty
        // required values get the health background (FieldHealthBackground).
        public const string FieldRequiredGlyph = "✱";

        public static readonly GUIStyle FieldRequiredOn = new GUIStyle()
        {
            fontSize = LoreTypography.Small,
            alignment = TextAnchor.MiddleCenter,

            // Same white as the Field name.
            normal = { textColor = LorePalette.Text },
            hover = { textColor = LorePalette.Text },
        };

        public static readonly GUIStyle FieldRequiredOff = new GUIStyle(FieldRequiredOn)
        {
            normal = { textColor = LorePalette.TextFaint },
            hover = { textColor = LorePalette.TextMuted },
        };

        // Entry field titles of required Fields: "Age ✱" (gap between name and glyph).
        public const float FieldRequiredTitleSpacing = 3f;
        public const float FieldRequiredGlyphWidth = 12f;

        #endregion

        #region Entries (chips)
        // Entries values: one chip per listed Entry ("🐺 Wolf ✕"), then the Add button (as the row's Clear button); chips
        // wrap, the row grows.
        public const float EntriesChipHeight = 18f;
        public const float EntriesChipSpacing = 4f;
        public const float EntriesLineSpacing = 3f;
        public const float EntriesVerticalPadding = 3f;
        public const float EntriesChipHorizontalPadding = 6f;
        public const float EntriesChipRemoveWidth = 14f;
        public const float EntriesAddWidth = FieldButtonColumnWidth;

        public static readonly Color EntriesChipColor = LorePalette.Surface4;
        public static readonly Color EntriesChipMissingColor = LorePalette.DangerSurface;

        public const string EntriesRemoveGlyph = "✕";

        public static readonly GUIStyle EntriesChipText = new GUIStyle()
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),

            fontSize = LoreTypography.Small,
            alignment = TextAnchor.MiddleLeft,
            clipping = TextClipping.Clip,

            normal = { textColor = LorePalette.Text },
        };

        public static readonly GUIStyle EntriesChipMissingText = new GUIStyle(EntriesChipText)
        {
            normal = { textColor = LorePalette.Danger },
        };

        public static readonly GUIStyle EntriesChipButton = new GUIStyle()
        {
            fontSize = LoreTypography.Small,
            alignment = TextAnchor.MiddleCenter,

            normal = { textColor = LorePalette.TextMuted },
            hover = { textColor = LorePalette.Text },
        };
        #endregion

        #region FieldButton
        public const float FieldButtonColumnWidth = 18f;
        public const float FieldEntryButtonVerticalOffset = -1f;

        public static readonly GUIStyle FieldButton = new(EditorStyles.miniButton)
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

        #region Card
        public const float FieldCardColumnWidth = 300f;
        #endregion

        #region Category
        public const float FieldCategoryColumnWidth = 100f;
        #endregion

        #region Field Values
        // URL value: gap between the text field and the open button (button uses FieldButtonColumnWidth / FieldButton).
        public const float FieldValueUrlButtonSpacing = 4f;

        // Audio value: gap between the clip field and the play/stop button.
        public const float FieldValueAudioButtonSpacing = 4f;

        public static readonly GUIStyle FieldValueText = new GUIStyle(EditorStyles.textField)
        {
            padding = new RectOffset(3, 3, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),

            fontSize = LoreTypography.Body,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleLeft,

            normal = { textColor = LorePalette.Text },
            hover = { textColor = LorePalette.Text },
            active = { textColor = LorePalette.Text },
        };

        public static readonly GUIStyle FieldValueNumber = new GUIStyle(EditorStyles.numberField)
        {
            padding = new RectOffset(3, 3, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),

            fontSize = LoreTypography.Body,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleLeft,

            normal = { textColor = LorePalette.Text },
            hover = { textColor = LorePalette.Text },
            active = { textColor = LorePalette.Text },
        };

        // Enum value: looks like the text fields (Unity's popup background stands out); own arrow on the right.
        public static readonly GUIStyle FieldValueEnum = new GUIStyle(FieldValueText)
        {
            padding = new RectOffset(3, FieldValueEnumArrowWidth, 0, 0),
        };

        public const string FieldValueEnumArrow = "▾";
        public const int FieldValueEnumArrowWidth = 16;

        public static readonly GUIStyle FieldValueEnumArrowContent = new GUIStyle()
        {
            fontSize = LoreTypography.Body,
            alignment = TextAnchor.MiddleCenter,

            normal = { textColor = LorePalette.TextMuted },
        };

        public static readonly GUIStyle FieldValueToggle = new GUIStyle(EditorStyles.toggle)
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),

            fontSize = LoreTypography.Body,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleLeft,

            normal = { textColor = LorePalette.Text },
            hover = { textColor = LorePalette.Text },
            active = { textColor = LorePalette.Text },
        };

        public static readonly GUIStyle FieldValueObject = new GUIStyle()
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),

            fontSize = LoreTypography.Body,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleLeft,

            normal = { textColor = LorePalette.Text },
            hover = { textColor = LorePalette.Text },
            active = { textColor = LorePalette.Text },
        };

        public static readonly GUIStyle FieldValueEntryGlyph = new GUIStyle(FieldGlyph)
        {
        };

        public static readonly GUIStyle FieldValueEntryTitle = new GUIStyle(FieldTitle)
        {
            fontSize = LoreTypography.Body,

            normal = { textColor = LorePalette.Text },
            hover = { textColor = LorePalette.Text },
            active = { textColor = LorePalette.Text },
        };

        public static readonly GUIStyle FieldValueEntryCategory = new GUIStyle(FieldTypeLabel)
        {
        };

        // Compact Entry card (Tabular Editing): name gets this share of the text width, the Category the rest.
        public const float FieldValueEntryCompactTitleRatio = 0.6f;
        public const float FieldValueEntryCompactSpacing = 4f;

        public static readonly GUIStyle FieldValueEntryCategoryCompact = new GUIStyle(FieldTypeLabel)
        {
        };

        public static readonly GUIStyle FieldValueEntryEmpty = new GUIStyle(FieldTitle)
        {
            fontSize = LoreTypography.Body,

            normal = { textColor = LorePalette.TextMuted },
            hover = { textColor = LorePalette.TextMuted },
            active = { textColor = LorePalette.TextMuted },
        };

        public static readonly GUIStyle MissingFieldFixStyle = new GUIStyle(FieldTitle)
        {
            fontStyle = FontStyle.Normal,

            normal = { textColor = LorePalette.TextMuted },
            hover = { textColor = LorePalette.TextMuted },
            active = { textColor = LorePalette.TextMuted },
        };
        #endregion
    }
}