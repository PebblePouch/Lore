using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static partial class InspectorStyle
    {
        #region Control Names
        // Technical IMGUI control names (not user-visible text, see LoreText.Inspector).
        public const string CategoryTitleControlName = "CategoryTitle";
        public const string EntryTitleControlName = "EntryTitle";
        public const string SubtitleControlName = "EntrySubtitle";
        public const string ProjectTitleControlName = "ProjectTitle";
        #endregion

        #region Layout
        public const float KeyboardScrollStep = 40f;

        public const float HeaderHeight = 50f;

        public const float ColorWidth = 18f;
        public const float ColorHeight = 50f;

        public const float GlyphWidth = 50f;
        public const float GlyphHeight = 50f;

        public const float CategoryTitleHeight = 50f;
        public const float CategoryTitleHorizontalInset = 3f;
        // 0: a positive inset larger than half the height leaves no room for the (clipped) text.
        public const float CategoryTitleVerticalInset = 0f;

        public const float EntryTitleHeight = 25f;
        public const float EntryTitleHorizontalInset = 3f;
        public const float EntryTitleVerticalInset = 0f;
        public const float EntryTitleTopSpacing = 5f;

        public const float SubtitleHeight = 15f;
        public const float SubtitleHorizontalInset = 3f;
        public const float SubtitleVerticalInset = 0f;

        public const float FeedbackHeight = 24f;
        public const float FeedbackVerticalPadding = 3f;
        public const float FeedbackHorizontalPadding = 8f;

        public const float ActionsWidth = 30f;
        public const float ActionsHeight = 50f;

        public const float MenuWidth = 20f;
        public const float MenuHeight = 50f;
        #endregion

        #region Header
        public static readonly GUIStyle HeaderLayout = new GUIStyle()
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 2, 3),
        };
        #endregion

        #region Color
        public static readonly GUIStyle ColorLayout = new GUIStyle()
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),
        };

        public static readonly GUIStyle ColorViewerContent = new GUIStyle();

        public static readonly LoreInlineColorPickerSettings ColorPicker = new LoreInlineColorPickerSettings()
        {
            ViewerContent = ColorViewerContent,

            Width = ColorWidth,
            Height = ColorHeight,

            Tooltip = LoreText.Inspector.ColorTooltip
        };
        #endregion

        #region Glyph
        public static readonly GUIStyle GlyphLayout = new GUIStyle()
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(10, 0, 0, 0),
        };

        public static readonly GUIStyle GlyphViewerContent = new GUIStyle()
        {
            fontSize = LoreTypography.GlyphDisplay,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleCenter,

            normal = { textColor = LorePalette.Text },
            hover = { textColor = LorePalette.Text },
            active = { textColor = LorePalette.Text },
        };

        public static readonly LoreInlineGlyphPickerSettings GlyphPicker = new LoreInlineGlyphPickerSettings()
        {
            ViewerContent = GlyphViewerContent,

            Width = GlyphWidth,
            Height = GlyphHeight,

            Tooltip = LoreText.Inspector.GlyphTooltip
        };
        #endregion

        #region Title
        public static readonly GUIStyle TitleLayout = new GUIStyle()
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(20, 0, 0, 0),
        };
        #endregion

        #region Title (Category)
        public static readonly GUIStyle CategoryTitleViewerContent = new GUIStyle()
        {
            fontSize = LoreTypography.Display,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            clipping = TextClipping.Clip,

            normal = { textColor = LorePalette.Text },
            hover = { textColor = LorePalette.Text },
            active = { textColor = LorePalette.Text },
        };

        public static readonly GUIStyle CategoryTitleDefaultContent = new GUIStyle(CategoryTitleViewerContent)
        {
            fontStyle = FontStyle.Italic,

            normal = { textColor = LorePalette.TextMuted },
            hover = { textColor = LorePalette.TextMuted },
            active = { textColor = LorePalette.TextMuted },
        };

        public static readonly GUIStyle CategoryTitleEditorContent = new GUIStyle(EditorStyles.textField)
        {
            fontSize = LoreTypography.Display,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,

            normal = { textColor = LorePalette.Text },
            hover = { textColor = LorePalette.Text },
            active = { textColor = LorePalette.Text },
        };

        public static readonly LoreInlineTextFieldSettings CategoryTitle = new LoreInlineTextFieldSettings()
        {
            ViewerContent = CategoryTitleViewerContent,
            PlaceholderContent = CategoryTitleDefaultContent,
            EditorContent = CategoryTitleEditorContent,

            Height = CategoryTitleHeight,

            HorizontalInset = CategoryTitleHorizontalInset,
            VerticalInset = CategoryTitleVerticalInset,

            ControlName = CategoryTitleControlName,

            Tooltip = LoreText.Inspector.CategoryTitleTooltip
        };
        #endregion

        #region Title (Project)
        public static readonly LoreInlineTextFieldSettings ProjectTitle = new LoreInlineTextFieldSettings()
        {
            ViewerContent = CategoryTitleViewerContent,
            PlaceholderContent = CategoryTitleDefaultContent,
            EditorContent = CategoryTitleEditorContent,

            Height = CategoryTitleHeight,

            HorizontalInset = CategoryTitleHorizontalInset,
            VerticalInset = CategoryTitleVerticalInset,

            ControlName = ProjectTitleControlName,

            Tooltip = LoreText.Inspector.ProjectTitleTooltip
        };
        #endregion

        #region Title (Entry)
        public static readonly GUIStyle EntryTitleViewerContent = new GUIStyle()
        {
            fontSize = LoreTypography.Title,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            clipping = TextClipping.Clip,

            normal = { textColor = LorePalette.Text },
            hover = { textColor = LorePalette.Text },
            active = { textColor = LorePalette.Text },
        };

        public static readonly GUIStyle EntryTitleDefaultContent = new GUIStyle(EntryTitleViewerContent)
        {
            fontStyle = FontStyle.Italic,

            normal = { textColor = LorePalette.TextMuted },
            hover = { textColor = LorePalette.TextMuted },
            active = { textColor = LorePalette.TextMuted },
        };

        public static readonly GUIStyle EntryTitleEditorContent = new GUIStyle(EditorStyles.textField)
        {
            fontSize = LoreTypography.Title,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,

            normal = { textColor = LorePalette.Text },
            hover = { textColor = LorePalette.Text },
            active = { textColor = LorePalette.Text },
        };

        public static readonly LoreInlineTextFieldSettings EntryTitle = new LoreInlineTextFieldSettings()
        {
            ViewerContent = EntryTitleViewerContent,
            PlaceholderContent = EntryTitleDefaultContent,
            EditorContent = EntryTitleEditorContent,

            Height = EntryTitleHeight,

            HorizontalInset = EntryTitleHorizontalInset,
            VerticalInset = EntryTitleVerticalInset,

            ControlName = EntryTitleControlName,

            Tooltip = LoreText.Inspector.EntryTitleTooltip
        };
        #endregion

        #region Subtitle
        public static readonly GUIStyle SubtitleLayout = new GUIStyle()
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(20, 0, 0, 0),
        };

        public static readonly GUIStyle SubtitleViewerContent = new GUIStyle()
        {
            fontSize = LoreTypography.Body,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleLeft,
            clipping = TextClipping.Clip,

            normal = { textColor = LorePalette.TextSubtle },
            hover = { textColor = LorePalette.TextSubtle },
            active = { textColor = LorePalette.TextSubtle },
        };

        public static readonly GUIStyle SubtitleDefaultContent = new GUIStyle(SubtitleViewerContent)
        {
            fontStyle = FontStyle.Italic,

            normal = { textColor = LorePalette.TextMuted },
            hover = { textColor = LorePalette.TextMuted },
            active = { textColor = LorePalette.TextMuted },
        };

        public static readonly GUIStyle SubtitleEditorContent = new GUIStyle(EditorStyles.textField)
        {
            fontSize = LoreTypography.Body,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleLeft,

            normal = { textColor = LorePalette.TextSubtle },
            hover = { textColor = LorePalette.TextSubtle },
            active = { textColor = LorePalette.TextSubtle },
        };

        public static readonly LoreInlineTextFieldSettings Subtitle = new LoreInlineTextFieldSettings()
        {
            ViewerContent = SubtitleViewerContent,
            PlaceholderContent = SubtitleDefaultContent,
            EditorContent = SubtitleEditorContent,

            Height = SubtitleHeight,

            HorizontalInset = SubtitleHorizontalInset,
            VerticalInset = SubtitleVerticalInset,

            ControlName = SubtitleControlName,

            Tooltip = LoreText.Inspector.SubtitleTooltip
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

        #region Actions
        public static readonly GUIStyle ActionsLayout = new GUIStyle()
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),
        };

        public static readonly GUIStyle ActionsContent = new GUIStyle()
        {
            fontSize = LoreTypography.IconLarge,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleCenter,

            normal = { textColor = LorePalette.Text },
            hover = { textColor = LorePalette.Text },
            active = { textColor = LorePalette.Text },
        };
        #endregion

        #region Timestamps
        // Created/Modified in the header: same look as the Entry card subtitle in the Content browser
        // (small, italic, dimmed), right-aligned.
        public const float TimestampsHorizontalSpacing = 8f;

        public static readonly GUIStyle TimestampsContent = new GUIStyle(BrowserStyle.Subtitle)
        {
            margin = new RectOffset(0, 0, 0, 0),
            alignment = TextAnchor.MiddleRight,
            wordWrap = false,
        };
        #endregion

        #region Menu
        public static readonly GUIStyle MenuLayout = new GUIStyle()
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),
        };

        public static readonly GUIStyle MenuContent = new GUIStyle()
        {
            fontSize = LoreTypography.IconHuge,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleCenter,

            normal = { textColor = LorePalette.Text },
            hover = { textColor = LorePalette.Text },
            active = { textColor = LorePalette.Text },
        };
        #endregion
    }
}