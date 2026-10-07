using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Project Settings > Lore. Uses Unity's skin (light and dark), not the Lore palette.
    public static class SettingsStyle
    {
        #region Layout
        public const float LabelWidth = 150f;
        public const float SectionSpacing = 8f;
        public const float BrowseButtonWidth = 70f;
        public const float PlaceholderInset = 4f;
        #endregion

        #region Control Names
        public const string TextFieldControlPrefix = "LoreSettings.";
        #endregion

        #region Styles
        // Same margins as Unity's own Project Settings pages.
        public static readonly GUIStyle Page = new()
        {
            margin = new RectOffset(10, 10, 10, 10),
        };

        public static readonly GUIStyle Placeholder = new(EditorStyles.label)
        {
            normal = { textColor = EditorStyles.centeredGreyMiniLabel.normal.textColor },
        };
        #endregion
    }
}
