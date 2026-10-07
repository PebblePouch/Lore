using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Welcome page (no Lore Project yet) and the Multiple Projects window. Cards, headers and texts reuse the
    // Project page styles (InspectorStyle, InsightsStyle); only what the Project page does not have lives here.
    public static class WelcomeStyle
    {
        #region Layout
        // The manual is long: a taller viewport than descriptions (it scrolls like them).
        public const float ManualViewportHeight = 320f;

        public const float ActionButtonHeight = 28f;
        public const float ActionInfoSpacing = 6f;
        public const float SectionTopSpacing = 6f;

        public const float ProjectsWindowWidth = 560f;
        public const float ProjectsWindowHeight = 260f;
        public const float ProjectsRowHeight = 22f;
        public const float ProjectsCountColumnWidth = 80f;
        public const float ProjectsTitleColumnRatio = 0.35f;
        public const float ProjectsSpacing = 8f;
        #endregion

        #region Colors
        public static readonly Color ProjectsSelectedRow = LorePalette.Accent;
        #endregion

        #region Content
        public static readonly GUIStyle ActionButton = new(EditorStyles.miniButton)
        {
            fixedHeight = ActionButtonHeight,
            stretchWidth = true,

            padding = new RectOffset(8, 8, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),

            fontSize = LoreTypography.Body,
            alignment = TextAnchor.MiddleCenter,

            normal = { textColor = LorePalette.Text },
            hover = { textColor = LorePalette.Text },
            active = { textColor = LorePalette.Text },
        };

        public static readonly GUIStyle ActionInfo = new(EditorStyles.label)
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),

            fontSize = LoreTypography.Small,
            alignment = TextAnchor.UpperLeft,
            wordWrap = true,

            normal = { textColor = LorePalette.TextMuted },
            hover = { textColor = LorePalette.TextMuted },
            active = { textColor = LorePalette.TextMuted },
        };

        // Read-only texts in the description look (Getting Started, Credits).
        public static readonly GUIStyle Text = new(InspectorStyle.DescriptionViewerContent);

        // The manual: Markdown converted to rich text (headings bold, see LoreManual).
        public static readonly GUIStyle ManualText = new(InspectorStyle.DescriptionViewerContent)
        {
            richText = true,
        };

        public static readonly GUIStyle ProjectsMessage = new(EditorStyles.wordWrappedLabel)
        {
            fontSize = LoreTypography.Body,

            normal = { textColor = LorePalette.Text },
        };

        public static readonly GUIStyle ProjectsColumnHeader = new(EditorStyles.boldLabel)
        {
            fontSize = LoreTypography.Small,
            clipping = TextClipping.Clip,

            normal = { textColor = LorePalette.TextMuted },
        };

        public static readonly GUIStyle ProjectsCell = new(EditorStyles.label)
        {
            fontSize = LoreTypography.Body,
            alignment = TextAnchor.MiddleLeft,
            clipping = TextClipping.Clip,

            normal = { textColor = LorePalette.Text },
        };

        public static readonly GUIStyle ProjectsMutedCell = new(ProjectsCell)
        {
            fontSize = LoreTypography.Small,

            normal = { textColor = LorePalette.TextMuted },
        };
        #endregion

        #region LoreInlineTextAreaSettings
        public static readonly LoreInlineTextAreaSettings GettingStarted = new()
        {
            ViewerContent = Text,
            PlaceholderContent = InspectorStyle.DescriptionDefaultContent,
            EditorContent = InspectorStyle.DescriptionEditorContent,

            ViewportHeight = InspectorStyle.DescriptionViewportHeight,
            MaxEditorHeight = InspectorStyle.DescriptionViewportHeight,

            HorizontalInset = InspectorStyle.DescriptionHorizontalInset,
            VerticalInset = InspectorStyle.DescriptionVerticalInset,
        };

        public static readonly LoreInlineTextAreaSettings Manual = new()
        {
            ViewerContent = ManualText,
            PlaceholderContent = InspectorStyle.DescriptionDefaultContent,
            EditorContent = InspectorStyle.DescriptionEditorContent,

            ViewportHeight = ManualViewportHeight,
            MaxEditorHeight = ManualViewportHeight,

            HorizontalInset = InspectorStyle.DescriptionHorizontalInset,
            VerticalInset = InspectorStyle.DescriptionVerticalInset,
        };
        #endregion
    }
}
