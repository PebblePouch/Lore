using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Project page Links table. Row height, spacing, handle and button columns reuse the Field table
    // values (InspectorStyle.Field*) so both tables look the same. No field initializer here references
    // another InspectorStyle part (initialization order across partial files is undefined).
    public static partial class InspectorStyle
    {
        #region Control Names
        // Technical IMGUI control names (not user-visible text).
        public const string LinkLabelControlName = "ProjectLinkLabel";
        public const string LinkUrlControlName = "ProjectLinkUrl";
        #endregion

        #region Layout
        // Share of the content width used by the label column (the URL column takes the rest).
        public const float LinkLabelColumnRatio = 0.33f;

        public const float LinkEmptyTopSpacing = 4f;
        #endregion

        #region Content
        // Same link blue as description links (EditorStyles.linkLabel).
        public static readonly GUIStyle LinkLabel = new GUIStyle(EditorStyles.linkLabel)
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),

            fontSize = LoreTypography.Body,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleLeft,
            clipping = TextClipping.Clip,
            wordWrap = false,
        };

        public static readonly GUIStyle LinkUrl = new GUIStyle(EditorStyles.label)
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),

            fontSize = LoreTypography.Small,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleLeft,
            clipping = TextClipping.Clip,
            wordWrap = false,

            normal = { textColor = LorePalette.TextMuted },
            hover = { textColor = LorePalette.TextMuted },
            active = { textColor = LorePalette.TextMuted },
        };

        // No horizontal margin: the edit row must not get wider than the view row, otherwise the
        // measured table width grows every repaint (ProjectLinkTable.UpdateAvailableWidth).
        public static readonly GUIStyle LinkEditField = new GUIStyle(EditorStyles.textField)
        {
            margin = new RectOffset(0, 0, EditorStyles.textField.margin.top, EditorStyles.textField.margin.bottom),

            fontSize = LoreTypography.Body,
            alignment = TextAnchor.MiddleLeft,
        };

        public static readonly GUIStyle LinkEmpty = new GUIStyle(EditorStyles.label)
        {
            fontSize = LoreTypography.Body,
            fontStyle = FontStyle.Italic,
            alignment = TextAnchor.MiddleLeft,

            normal = { textColor = LorePalette.TextMuted },
            hover = { textColor = LorePalette.TextMuted },
            active = { textColor = LorePalette.TextMuted },
        };
        #endregion
    }
}
