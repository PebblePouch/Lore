using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static class InsightsStyle
    {
        #region Glyphs
        // Text lives in LoreText.Insights; these are visual icons.
        public const string SelectionGlyph = "\U00002714";
        public const string FilterGlyph = "\U00002207";
        #endregion

        #region Layout
        public const float RowSpacing = 3f;
        public const float ColumnSpacing = 3f;

        public const float Paragraph = 1f;
        public const float Indentation = 18f;

        public const float CategoriesVerticalExpansion = 23f;
        public const float EntriesVerticalExpansion = 2f;

        public const float StatisticCardHeight = 20f;
        public const float StatisticCardSpacing = 1f;

        public const float StatisticHorizontalPadding = 1f;
        public const float StatisticGlyphWidth = 18f;
        public const float StatisticGlyphSpacing = 0f;
        public const float StatisticRightEndSpacing = 3f;
        #endregion

        #region Colors
        public static readonly Color IntegrityBackground = LorePalette.DangerSurface;
        // Neutral notices (e.g. outdated Field constants): the selection blue.
        public static readonly Color NoticeBackground = LorePalette.Accent;
        public static readonly Color IntegrityTextColor = LorePalette.Text;

        public static readonly Color StatisticsCardDefaultColor = LorePalette.Surface2;
        public static readonly Color StatisticsCardDefaultLinkColor = LorePalette.Surface3;
        #endregion

        #region Card
        private static GUIStyle _card;

        public static GUIStyle Card
        {
            get
            {
                if (_card != null)
                    return _card;

                _card =
                    new GUIStyle(InspectorStyle.SectionCard)
                    {
                        margin = new RectOffset(0, 0, 0, 0),
                        fixedWidth = 0f,
                        stretchWidth = true
                    };

                return _card;
            }
        }

        public static readonly GUIStyle CardHeader = new GUIStyle(EditorStyles.boldLabel)
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

        public static readonly GUIStyle CardText = new GUIStyle(EditorStyles.label)
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
        #endregion

        #region Integrity
        private static GUIStyle _integrityCard;
        private static GUIStyle _integrityText;

        public static GUIStyle IntegrityCard
        {
            get
            {
                if (_integrityCard != null)
                    return _integrityCard;

                // Keeps the SectionCard margin: same gap below the callout as between the other section cards.
                _integrityCard =
                    new GUIStyle(
                        InspectorStyle.SectionCard);

                _integrityCard.normal.background = GUITextureUtility.GetTexture(IntegrityBackground);

                return _integrityCard;
            }
        }

        private static GUIStyle _noticeCard;

        // Same layout as the integrity callout, neutral color.
        public static GUIStyle NoticeCard
        {
            get
            {
                if (_noticeCard != null)
                    return _noticeCard;

                _noticeCard =
                    new GUIStyle(
                        InspectorStyle.SectionCard);

                _noticeCard.normal.background = GUITextureUtility.GetTexture(NoticeBackground);

                return _noticeCard;
            }
        }

        public static GUIStyle IntegrityText
        {
            get
            {
                if (_integrityText != null)
                    return _integrityText;

                _integrityText =
                    new GUIStyle(
                        InspectorStyle.SectionCardHeader)
                    {
                        fontSize = LoreTypography.Heading,
                        fontStyle = FontStyle.Normal,
                        wordWrap = true,
                        richText = true,
                    };

                _integrityText.normal.textColor = IntegrityTextColor;
                _integrityText.hover.textColor = IntegrityTextColor;
                _integrityText.active.textColor = IntegrityTextColor;

                return _integrityText;
            }
        }

        public static readonly GUIStyle IntegrityLink = new GUIStyle(EditorStyles.linkLabel)
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),

            fontSize = LoreTypography.Heading,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleLeft,
        };

        // Links on the blue notice card: the link color would not stand out, so bold text color instead.
        public static readonly GUIStyle NoticeLink = new GUIStyle(EditorStyles.linkLabel)
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),

            fontSize = LoreTypography.Heading,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,

            normal = { textColor = LorePalette.TextStrong },
            hover = { textColor = LorePalette.Text },
            active = { textColor = LorePalette.Text },
        };
        #endregion

        #region Statistics
        public const float StatisticsClipTextWidth = 200f;

        // Content Health: glyph of checks without findings (count 0, shown dimmed and not clickable).
        public const string ContentHealthDoneGlyph = "✓";

        public static readonly Color StatisticsHoverOverlay = LorePalette.HoverOverlay;

        public static readonly GUIStyle StatisticsText = new GUIStyle(EditorStyles.miniLabel)
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 6, 3),

            alignment = TextAnchor.MiddleLeft,
            clipping = TextClipping.Clip,

            normal = { textColor = LorePalette.Text },
            hover = { textColor = LorePalette.Text },
            active = { textColor = LorePalette.Text },
        };

        public static readonly GUIStyle StatisticsGlyph = new GUIStyle(EditorStyles.miniLabel)
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),

            alignment = TextAnchor.MiddleCenter,
            clipping = TextClipping.Clip,

            normal = { textColor = LorePalette.Text },
            hover = { textColor = LorePalette.Text },
            active = { textColor = LorePalette.Text },
        };
        #endregion
    }
}