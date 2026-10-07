using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static partial class InspectorStyle
    {
        #region Content
        public static readonly GUIStyle DescriptionViewerContent = new GUIStyle()
        {
            fontSize = LoreTypography.Body,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.UpperLeft,
            wordWrap = true,

            normal = { textColor = LorePalette.Text },
            hover = { textColor = LorePalette.Text },
            active = { textColor = LorePalette.Text },
        };

        public static readonly GUIStyle DescriptionDefaultContent = new GUIStyle(DescriptionViewerContent)
        {
            fontStyle = FontStyle.Italic,

            normal = { textColor = LorePalette.TextMuted },
            hover = { textColor = LorePalette.TextMuted },
            active = { textColor = LorePalette.TextMuted },
        };

        public static readonly GUIStyle DescriptionEditorContent = new GUIStyle(EditorStyles.textArea)
        {
            fontSize = LoreTypography.Body,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.UpperLeft,
            wordWrap = true,

            normal = { textColor = LorePalette.Text },
            hover = { textColor = LorePalette.Text },
            active = { textColor = LorePalette.Text },
        };
        #endregion

        #region Links
        // Same link blue as the integrity callout links (EditorStyles.linkLabel).
        public static readonly GUIStyle DescriptionLinkContent = new GUIStyle(EditorStyles.linkLabel)
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),

            fontSize = LoreTypography.Body,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.UpperLeft,
            wordWrap = false,
        };

        // Same red as Lore's destructive menu items (MenuStyle).
        public static readonly Color DescriptionBrokenLinkColor = LorePalette.Danger;

        public static readonly GUIStyle DescriptionBrokenLinkContent = new GUIStyle(DescriptionViewerContent)
        {
            wordWrap = false,

            normal = { textColor = DescriptionBrokenLinkColor },
            hover = { textColor = DescriptionBrokenLinkColor },
            active = { textColor = DescriptionBrokenLinkColor },
        };

        // Broken but repairable (a candidate Entry exists): link blue with strikethrough, click shows repair options.
        public static readonly GUIStyle DescriptionRepairableLinkContent = new GUIStyle(DescriptionBrokenLinkContent)
        {
            normal = { textColor = DescriptionLinkContent.normal.textColor },
            hover = { textColor = DescriptionLinkContent.normal.textColor },
            active = { textColor = DescriptionLinkContent.normal.textColor },
        };

        // Glyphs of the broken link menu: link to a candidate, remove link (keep text).
        public const string DescriptionLinkRepairGlyph = "\U0001F517";
        public const string DescriptionLinkRemoveGlyph = "\U0001F524";

        public const float DescriptionBrokenLinkStrikeThickness = 1f;
        public const float DescriptionBrokenLinkStrikeOffset = 0.55f;

        public const float DescriptionLinkPickerTopSpacing = 2f;

        // Insights statistic cards, sized to the description text (12pt); Category dimmed and italic like the Library.
        public static readonly StatisticCardStyle DescriptionLinkPickerCard = new StatisticCardStyle()
        {
            Text = new GUIStyle(InsightsStyle.StatisticsText)
            {
                fontSize = LoreTypography.Body,
            },

            Glyph = new GUIStyle(InsightsStyle.StatisticsGlyph)
            {
                fontSize = LoreTypography.Body,
            },

            Metadata = new GUIStyle(InsightsStyle.StatisticsText)
            {
                fontSize = LoreTypography.Small,
                fontStyle = FontStyle.Italic,
            },

            // The picker clamps cards to its own width.
            MaxWidth = float.MaxValue,
        };
        #endregion

        #region Layout
        public const float DescriptionViewportHeight = 75f;
        public const float DescriptionEditorMaxHeight = 450f;
        public const float DescriptionHorizontalInset = 0f;
        public const float DescriptionVerticalInset = 2f;
        #endregion

        #region Control Names
        // Technical IMGUI control names (not user-visible text, see LoreText.Inspector).
        public const string DescriptionCategoryControlName = "CategoryDescription";
        public const string DescriptionEntryControlName = "EntryDescription";
        public const string DescriptionProjectControlName = "ProjectDescription";
        #endregion

        #region LoreInlineTextAreaSettings
        public static readonly LoreInlineTextAreaSettings CategoryDescription = new LoreInlineTextAreaSettings()
        {
            ViewerContent = DescriptionViewerContent,
            PlaceholderContent = DescriptionDefaultContent,
            EditorContent = DescriptionEditorContent,

            ViewportHeight = DescriptionViewportHeight,
            MaxEditorHeight = DescriptionEditorMaxHeight,

            HorizontalInset = DescriptionHorizontalInset,
            VerticalInset = DescriptionVerticalInset,

            ControlName = DescriptionCategoryControlName,
            Tooltip = LoreText.Inspector.CategoryDescriptionTooltip
        };

        public static readonly LoreInlineTextAreaSettings EntryDescription = new LoreInlineTextAreaSettings()
        {
            ViewerContent = DescriptionViewerContent,
            PlaceholderContent = DescriptionDefaultContent,
            EditorContent = DescriptionEditorContent,

            ViewportHeight = DescriptionViewportHeight,
            MaxEditorHeight = DescriptionEditorMaxHeight,

            HorizontalInset = DescriptionHorizontalInset,
            VerticalInset = DescriptionVerticalInset,

            ControlName = DescriptionEntryControlName,
            Tooltip = LoreText.Inspector.EntryNotesTooltip
        };

        public static readonly LoreInlineTextAreaSettings ProjectDescription = new LoreInlineTextAreaSettings()
        {
            ViewerContent = DescriptionViewerContent,
            PlaceholderContent = DescriptionDefaultContent,
            EditorContent = DescriptionEditorContent,

            ViewportHeight = DescriptionViewportHeight,
            MaxEditorHeight = DescriptionEditorMaxHeight,

            HorizontalInset = DescriptionHorizontalInset,
            VerticalInset = DescriptionVerticalInset,

            ControlName = DescriptionProjectControlName,
            Tooltip = LoreText.Inspector.ProjectDescriptionTooltip
        };
        #endregion
    }
}