using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // CSV import preview window. Uses Unity's skin (light and dark), not the Lore palette.
    public static class ImportPreviewStyle
    {
        #region Layout
        public static readonly Vector2 WindowSize = new(520f, 420f);
        public static readonly Vector2 MinWindowSize = new(400f, 260f);

        public const float SectionSpacing = 8f;
        public const float ButtonWidth = 90f;
        #endregion

        #region Styles
        public static readonly GUIStyle Page = new()
        {
            margin = new RectOffset(10, 10, 10, 10),
        };
        #endregion
    }
}
