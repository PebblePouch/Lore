using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static partial class InspectorStyle
    {
        #region Constructor
        public static readonly Color SectionCardBackground = LorePalette.Surface2;

        static InspectorStyle()
        {
            SectionCard.normal.background = GUITextureUtility.GetTexture(SectionCardBackground);
        }
        #endregion

        #region Glyphs
        public const string SectionFoldoutExpandedString = "\U000025BC";   // Arrow Down
        public const string SectionFoldoutCollapsedString = "\U000025B6";  // Arrow Right
        #endregion

        #region Layout
        public const float SectionFoldoutWidth = 16f;
        #endregion

        #region SectionCard
        public static readonly GUIStyle SectionCard = new GUIStyle(EditorStyles.helpBox)
        {
            padding = new RectOffset(10, 10, 10, 10),
            margin = new RectOffset(0, 0, 0, 3),
        };

        public static readonly GUIStyle SectionCardHeader = new GUIStyle(EditorStyles.boldLabel)
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),

            fontSize = LoreTypography.Heading,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,

            normal = { textColor = LorePalette.Text },
            hover = { textColor = LorePalette.Text },
            active = { textColor = LorePalette.Text },
        };

        public static readonly GUIStyle SectionCardFieldHeader = new GUIStyle(SectionCardHeader)
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(2, 0, 1, 0),
        };

        public static readonly GUIStyle SectionCardFoldout = new(SectionCardHeader)
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 2, 1, 0),

            fontSize = LoreTypography.Tiny,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleCenter,

            normal = { textColor = LorePalette.Text },
            hover = { textColor = LorePalette.Text },
            active = { textColor = LorePalette.Text },
        };

        #endregion
    }
}