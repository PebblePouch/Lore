using System;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public enum StatisticCardAction
    {
        None,
        Selection,
        Filter
    }

    public readonly struct StatisticCardItem
    {
        public string Text { get; }
        public Color Color { get; }
        public StatisticCardAction ActionType { get; }
        public string Glyph { get; }
        public Action Action { get; }

        // Optional secondary text drawn dimmed after Text (e.g. the Category name).
        public string Metadata { get; }

        public StatisticCardItem(
            string text,
            Color color,
            StatisticCardAction actionType = StatisticCardAction.None,
            Action action = null,
            string glyph = null,
            string metadata = null)
        {
            Text = text;
            Color = color;
            ActionType = actionType;
            Action = action;
            Glyph = glyph;
            Metadata = metadata;
        }
    }

    // Optional style overrides; null members fall back to the Insights statistic styles.
    public sealed class StatisticCardStyle
    {
        public GUIStyle Text;
        public GUIStyle Glyph;
        public GUIStyle Metadata;

        public float MaxWidth = InsightsStyle.StatisticsClipTextWidth;
    }

    public static class StatisticCard
    {
        #region Fields
        private static readonly StatisticCardStyle DefaultStyle = new();
        #endregion

        #region Public API
        public static float GetWidth(
            StatisticCardItem item,
            StatisticCardStyle style = null)
        {
            style ??= DefaultStyle;

            float width =
                InsightsStyle.StatisticHorizontalPadding
                + InsightsStyle.StatisticGlyphWidth
                + InsightsStyle.StatisticGlyphSpacing
                + GetTextWidth(item, style)
                + GetMetadataWidth(item, style)
                + InsightsStyle.StatisticHorizontalPadding
                + InsightsStyle.StatisticRightEndSpacing;

            return Mathf.Min(width, style.MaxWidth);
        }

        public static void Draw(StatisticCardItem item)
        {
            float width = GetWidth(item);

            // MinWidth(0): the card may get narrower than its text (Draw(rect) clips), so it never
            // holds the Inspector width open when the window gets narrower.
            Rect cardRect =
                GUILayoutUtility.GetRect(
                    width,
                    InsightsStyle.StatisticCardHeight,
                    GUILayout.MinWidth(0f),
                    GUILayout.MaxWidth(width),
                    GUILayout.ExpandWidth(false),
                    GUILayout.Height(InsightsStyle.StatisticCardHeight));

            Draw(cardRect, item);
        }
        #endregion

        #region Draw
        // Rect-based variant for callers that place cards themselves (e.g. the description link picker).
        public static void Draw(
            Rect rect,
            StatisticCardItem item,
            StatisticCardStyle style = null)
        {
            style ??= DefaultStyle;

            float textWidth =
                GetTextWidth(
                    item,
                    style);

            float metadataWidth =
                GetMetadataWidth(
                    item,
                    style);

            // Only glyph + text sit on the item color; metadata (date, count, Category) follows outside it
            // on the normal background, so it stays readable with every color. Narrow cards keep the text first.
            Rect colorRect =
                new(
                    rect.x,
                    rect.y,
                    Mathf.Min(
                        rect.width,
                        GetColorWidth(textWidth)),
                    rect.height);

            EditorGUI.DrawRect(colorRect, item.Color);

            bool interactive =
                item.ActionType != StatisticCardAction.None &&
                item.Action != null;

            bool hovered =
                interactive &&
                LoreHover.Contains(rect);

            if (hovered)
            {
                EditorGUI.DrawRect(
                    colorRect,
                    InsightsStyle.StatisticsHoverOverlay);
            }

            Rect glyphRect =
                new(
                    colorRect.x +
                    InsightsStyle.StatisticHorizontalPadding,
                    rect.y,
                    InsightsStyle.StatisticGlyphWidth,
                    rect.height);

            Rect contentRect =
                new(
                    glyphRect.xMax +
                    InsightsStyle.StatisticGlyphSpacing,
                    rect.y,
                    Mathf.Max(
                        0f,
                        colorRect.xMax
                        - InsightsStyle.StatisticHorizontalPadding
                        - glyphRect.xMax
                        - InsightsStyle.StatisticGlyphSpacing),
                    rect.height);

            Color textColor =
                LoreColorUtility.GetContrastTextColor(
                    item.Color);

            string glyph = GetGlyph(item);

            if (!string.IsNullOrEmpty(glyph))
            {
                GUI.Label(
                    glyphRect,
                    glyph,
                    GetColoredStyle(
                        style.Glyph ?? InsightsStyle.StatisticsGlyph,
                        textColor));
            }

            Rect textRect =
                new(
                    contentRect.x,
                    contentRect.y,
                    Mathf.Min(
                        textWidth,
                        contentRect.width),
                    contentRect.height);

            GUI.Label(
                textRect,
                item.Text,
                GetColoredStyle(
                    style.Text ?? InsightsStyle.StatisticsText,
                    textColor));

            if (metadataWidth > 0f &&
                colorRect.xMax < rect.xMax)
            {
                Rect metadataRect =
                    new(
                        colorRect.xMax,
                        rect.y,
                        Mathf.Min(
                            metadataWidth,
                            rect.xMax - colorRect.xMax),
                        rect.height);

                // Dimmed text color on the normal background (not the item's contrast color).
                GUI.Label(
                    metadataRect,
                    GetMetadataLabel(item),
                    GetColoredStyle(
                        style.Metadata ?? style.Text ?? InsightsStyle.StatisticsText,
                        LorePalette.TextMuted));
            }

            // We draw a tooltip only if necessary (clipped)
            if (textWidth > contentRect.width ||
                colorRect.xMax + metadataWidth > rect.xMax)
            {
                GUI.Label(
                    rect,
                    new GUIContent(
                        string.Empty,
                        item.Text + GetMetadataLabel(item)));
            }

            if (item.ActionType == StatisticCardAction.None
                || item.Action == null)
            {
                EditorGUIUtility.AddCursorRect(rect, MouseCursor.Arrow);

                return;
            }

            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            if (Event.current.type != EventType.MouseDown
                || Event.current.button != 0
                || !rect.Contains(Event.current.mousePosition))
            {
                return;
            }

            item.Action.Invoke();

            Event.current.Use();
        }
        #endregion

        #region Helpers
        // Natural width of the colored part (glyph + text, without metadata), not clamped to MaxWidth.
        private static float GetColorWidth(float textWidth)
        {
            return
                InsightsStyle.StatisticHorizontalPadding
                + InsightsStyle.StatisticGlyphWidth
                + InsightsStyle.StatisticGlyphSpacing
                + textWidth
                + InsightsStyle.StatisticHorizontalPadding
                + InsightsStyle.StatisticRightEndSpacing;
        }

        private static float GetTextWidth(
            StatisticCardItem item,
            StatisticCardStyle style)
        {
            return (style.Text ?? InsightsStyle.StatisticsText)
                .CalcSize(
                    new GUIContent(
                        item.Text))
                .x;
        }

        private static float GetMetadataWidth(
            StatisticCardItem item,
            StatisticCardStyle style)
        {
            if (string.IsNullOrEmpty(item.Metadata))
                return 0f;

            return (style.Metadata ?? style.Text ?? InsightsStyle.StatisticsText)
                .CalcSize(
                    new GUIContent(
                        GetMetadataLabel(item)))
                .x;
        }

        private static string GetMetadataLabel(StatisticCardItem item)
        {
            return string.IsNullOrEmpty(item.Metadata)
                ? string.Empty
                : $"  {item.Metadata}";
        }

        private static string GetGlyph(StatisticCardItem item)
        {
            if (!string.IsNullOrWhiteSpace(item.Glyph))
            {
                return item.Glyph;
            }

            return item.ActionType switch
            {
                StatisticCardAction.Selection => InsightsStyle.SelectionGlyph,
                StatisticCardAction.Filter => InsightsStyle.FilterGlyph,
                _ => string.Empty
            };
        }

        // Dynamic Styles and Colors should live here
        private static GUIStyle GetColoredStyle(
            GUIStyle baseStyle,
            Color color)
        {
            var style = new GUIStyle(baseStyle);

            style.normal.textColor = color;
            style.hover.textColor = color;
            style.active.textColor = color;

            return style;
        }
        #endregion
    }
}
