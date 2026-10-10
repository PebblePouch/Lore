using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Draws description text with [[Entry]] links and Markdown emphasis (**bold**, *italic*, see
    // DescriptionEmphasisParser) in a word-wrapped flow (view mode only).
    // Resolved links show "Glyph Title" in link blue and navigate on click.
    // Broken links show their label with a strikethrough: in yellow when a repair candidate exists
    // (DescriptionLinkRepair), otherwise in red. Clicking a broken link opens the repair menu
    // (link to a candidate, remove the link but keep the text) when the caller passes textChanged;
    // without it the click falls through to description editing.
    public static class DescriptionTextFlow
    {
        #region Types
        private enum TokenKind
        {
            Word,
            Space,
            NewLine,
            Link,
            BrokenLink,
            RepairableLink
        }

        private readonly struct Token
        {
            public readonly TokenKind Kind;
            public readonly string Text;
            public readonly Entry Entry;
            public readonly FontStyle FontStyle;

            // Links only: the stored target and the link's position in the text (for repairs).
            public readonly string Target;
            public readonly int LinkIndex;

            public Token(
                TokenKind kind,
                string text,
                FontStyle fontStyle,
                Entry entry = null,
                string target = null,
                int linkIndex = -1)
            {
                Kind = kind;
                Text = text;
                FontStyle = fontStyle;
                Entry = entry;
                Target = target;
                LinkIndex = linkIndex;
            }

            // Words and links; consecutive ones without a space between ("**big**,") wrap together.
            public bool IsText =>
                Kind != TokenKind.Space &&
                Kind != TokenKind.NewLine;
        }

        // Repair menu: textChanged receives the repaired description text.
        private readonly struct RepairContext
        {
            public readonly string Text;
            public readonly Action<string> TextChanged;

            public RepairContext(
                string text,
                Action<string> textChanged)
            {
                Text = text;
                TextChanged = textChanged;
            }

            public bool IsAvailable => TextChanged != null;
        }

        // A token at its position in the flow (relative to the flow's top left corner).
        private readonly struct PlacedToken
        {
            public readonly Token Token;
            public readonly Rect Rect;
            public readonly GUIStyle Style;

            public PlacedToken(
                Token token,
                Rect rect,
                GUIStyle style)
            {
                Token = token;
                Rect = rect;
                Style = style;
            }
        }

        private sealed class FlowLayout
        {
            public readonly List<PlacedToken> Tokens = new();
            public float Height;
        }

        private readonly struct LayoutKey : IEquatable<LayoutKey>
        {
            private readonly string _text;
            private readonly GUIStyle _style;
            private readonly float _width;

            public LayoutKey(
                string text,
                GUIStyle style,
                float width)
            {
                _text = text ?? string.Empty;
                _style = style;
                _width = width;
            }

            public bool Equals(LayoutKey other) =>
                _width.Equals(other._width) &&
                ReferenceEquals(_style, other._style) &&
                string.Equals(_text, other._text, StringComparison.Ordinal);

            public override bool Equals(object obj) => obj is LayoutKey other && Equals(other);

            public override int GetHashCode() =>
                HashCode.Combine(_text, _style, _width);
        }
        #endregion

        #region Fields
        // Non-wrapping copies of text styles: the flow wraps per word itself. With wordWrap on,
        // a word drawn in its measured rect can wrap its last letter onto the next row.
        private static readonly Dictionary<GUIStyle, GUIStyle> NoWrapStyles = new();

        // Bold / italic copies of text and link styles.
        private static readonly Dictionary<(GUIStyle, FontStyle), GUIStyle> FontStyles = new();

        // Performance: tokens, word sizes and repair candidates are computed once per text, style and width, not on
        // every OnGUI call (Draw and CalcHeight share the layout). Links resolve against the current data, so the
        // cache is cleared whenever LoreDataVersion changes; the size limit only guards against unbounded growth.
        private static readonly Dictionary<LayoutKey, FlowLayout> Layouts = new();
        private static readonly Dictionary<GUIStyle, float> SpaceWidths = new();
        private static int _layoutVersion;
        private const int MaxCachedLayouts = 64;
        #endregion

        #region Public API
        public static void Draw(
            Rect rect,
            string text,
            GUIStyle textStyle,
            Action<string> textChanged = null)
        {
            FlowLayout layout =
                GetLayout(
                    text,
                    textStyle,
                    rect.width);

            var repair =
                new RepairContext(
                    text,
                    textChanged);

            foreach (PlacedToken placed in layout.Tokens)
            {
                if (placed.Rect.y >= rect.height)
                    break;

                DrawToken(
                    placed.Token,
                    new Rect(
                        rect.x + placed.Rect.x,
                        rect.y + placed.Rect.y,
                        placed.Rect.width,
                        placed.Rect.height),
                    placed.Style,
                    repair);
            }
        }

        // Height of the wrapped flow at the given width (same wrapping as Draw).
        public static float CalcHeight(
            string text,
            GUIStyle textStyle,
            float width)
        {
            return GetLayout(
                    text,
                    textStyle,
                    width)
                .Height;
        }
        #endregion

        #region Draw
        private static FlowLayout GetLayout(
            string text,
            GUIStyle textStyle,
            float width)
        {
            if (_layoutVersion != LoreDataVersion.Current ||
                Layouts.Count >= MaxCachedLayouts)
            {
                Layouts.Clear();

                _layoutVersion = LoreDataVersion.Current;
            }

            var key =
                new LayoutKey(
                    text,
                    textStyle,
                    width);

            if (!Layouts.TryGetValue(
                    key,
                    out FlowLayout layout))
            {
                layout =
                    BuildLayout(
                        text,
                        textStyle,
                        width);

                Layouts[key] = layout;
            }

            return layout;
        }

        // Places the tokens in rows of the given width.
        private static FlowLayout BuildLayout(
            string text,
            GUIStyle textStyle,
            float maxWidth)
        {
            textStyle =
                GetNoWrapStyle(
                    textStyle);

            var layout = new FlowLayout();

            float lineHeight =
                textStyle.lineHeight;

            float x = 0f;
            float y = 0f;

            // First placed token of the current run of glued text tokens (see Token.IsText).
            int groupStart = 0;
            bool previousIsText = false;

            foreach (Token token in Tokenize(text))
            {
                bool glued =
                    previousIsText &&
                    token.IsText;

                previousIsText = token.IsText;

                if (!glued)
                    groupStart = layout.Tokens.Count;

                if (token.Kind == TokenKind.NewLine)
                {
                    x = 0f;
                    y += lineHeight;

                    continue;
                }

                GUIStyle style =
                    GetStyle(
                        token.Kind,
                        token.FontStyle,
                        textStyle);

                float width =
                    token.Kind == TokenKind.Space
                        ? GetSpaceWidth(style) * (token.Text == "\t" ? 4 : 1)
                        : style
                            .CalcSize(
                                new GUIContent(
                                    token.Text))
                            .x;

                if (token.Kind == TokenKind.Space)
                {
                    // Spaces at the start of a wrapped line are dropped.
                    if (x > 0f)
                        x += width;

                    continue;
                }

                if (x > 0f &&
                    x + width > maxWidth)
                {
                    x =
                        WrapGroup(
                            layout,
                            groupStart,
                            y,
                            lineHeight);

                    y += lineHeight;

                    // The group alone is still too wide: break inside it.
                    if (x > 0f &&
                        x + width > maxWidth)
                    {
                        x = 0f;
                        y += lineHeight;
                    }
                }

                Rect tokenRect =
                    new(
                        x,
                        y,
                        Mathf.Min(
                            width,
                            maxWidth),
                        lineHeight);

                layout.Tokens.Add(
                    new PlacedToken(
                        token,
                        tokenRect,
                        style));

                x += tokenRect.width;
            }

            layout.Height = y + lineHeight;

            return layout;
        }

        // Moves the glued tokens already placed on row y (from groupStart) to the start of the next row, so
        // "**big**," wraps as one word. Returns x after the moved tokens (0 when nothing moved).
        private static float WrapGroup(
            FlowLayout layout,
            int groupStart,
            float y,
            float lineHeight)
        {
            if (groupStart >= layout.Tokens.Count)
                return 0f;

            Rect first = layout.Tokens[groupStart].Rect;

            if (first.x <= 0f ||
                !Mathf.Approximately(first.y, y))
            {
                return 0f;
            }

            float x = 0f;

            for (int i = groupStart; i < layout.Tokens.Count; i++)
            {
                PlacedToken placed = layout.Tokens[i];

                Rect rect =
                    new(
                        placed.Rect.x - first.x,
                        y + lineHeight,
                        placed.Rect.width,
                        placed.Rect.height);

                layout.Tokens[i] =
                    new PlacedToken(
                        placed.Token,
                        rect,
                        placed.Style);

                x = rect.xMax;
            }

            return x;
        }

        private static void DrawToken(
            Token token,
            Rect rect,
            GUIStyle style,
            RepairContext repair)
        {
            GUI.Label(
                rect,
                token.Text,
                style);

            if (token.Kind == TokenKind.BrokenLink ||
                token.Kind == TokenKind.RepairableLink)
            {
                EditorGUI.DrawRect(
                    new Rect(
                        rect.x,
                        rect.y + rect.height * InspectorStyle.DescriptionBrokenLinkStrikeOffset,
                        rect.width,
                        InspectorStyle.DescriptionBrokenLinkStrikeThickness),
                    style.normal.textColor);

                HandleBrokenLinkClick(
                    token,
                    rect,
                    repair);

                return;
            }

            if (token.Kind != TokenKind.Link)
                return;

            EditorGUIUtility.AddCursorRect(
                rect,
                MouseCursor.Link);

            if (Event.current.type == EventType.MouseDown &&
                Event.current.button == 0 &&
                rect.Contains(Event.current.mousePosition))
            {
                LoreNavigation.NavigateToEntry(token.Entry);

                // Consumed: the click must not start description editing.
                Event.current.Use();
            }
        }

        // Repair menu: candidates first ("🐺 Wolf  (Characters)"), then remove link (keep text).
        private static void HandleBrokenLinkClick(
            Token token,
            Rect rect,
            RepairContext repair)
        {
            if (!repair.IsAvailable)
                return;

            EditorGUIUtility.AddCursorRect(
                rect,
                MouseCursor.Link);

            if (Event.current.type != EventType.MouseDown ||
                Event.current.button != 0 ||
                !rect.Contains(Event.current.mousePosition))
            {
                return;
            }

            var items =
                new List<LoreMenuItem>();

            if (token.Kind == TokenKind.RepairableLink)
            {
                foreach (Entry candidate in
                         DescriptionLinkRepair.FindCandidates(
                             token.Target))
                {
                    Entry captured =
                        candidate;

                    items.Add(
                        new LoreMenuItem(
                            InspectorStyle.DescriptionLinkRepairGlyph,
                            GetCandidateLabel(candidate),
                            () =>
                                repair.TextChanged(
                                    DescriptionLinkRepair.LinkTo(
                                        repair.Text,
                                        token.LinkIndex,
                                        captured))));
                }
            }

            items.Add(
                new LoreMenuItem(
                    InspectorStyle.DescriptionLinkRemoveGlyph,
                    LoreText.Inspector.RemoveLinkKeepText,
                    () =>
                        repair.TextChanged(
                            DescriptionLinkRepair.RemoveLink(
                                repair.Text,
                                token.LinkIndex,
                                token.Target)),
                    separatorBefore: items.Count > 0));

            PopupWindow.Show(
                rect,
                new LoreMenuPopup(
                    items));

            // Consumed: the click must not start description editing.
            Event.current.Use();
        }

        // "🐺 Wolf  (Characters)"
        private static string GetCandidateLabel(Entry entry)
        {
            return string.Format(
                LoreText.Inspector.LinkCandidate,
                entry.Glyph,
                entry.Title,
                entry.Category != null
                    ? entry.Category.Title
                    : LoreText.Common.Uncategorized);
        }

        private static GUIStyle GetStyle(
            TokenKind kind,
            FontStyle fontStyle,
            GUIStyle textStyle)
        {
            GUIStyle style =
                kind switch
                {
                    TokenKind.Link => InspectorStyle.DescriptionLinkContent,
                    TokenKind.BrokenLink => InspectorStyle.DescriptionBrokenLinkContent,
                    TokenKind.RepairableLink => InspectorStyle.DescriptionRepairableLinkContent,
                    _ => textStyle
                };

            return GetFontStyle(
                style,
                fontStyle);
        }
        #endregion

        #region Helpers
        private static GUIStyle GetNoWrapStyle(GUIStyle style)
        {
            if (!style.wordWrap)
                return style;

            if (!NoWrapStyles.TryGetValue(
                    style,
                    out GUIStyle noWrapStyle))
            {
                noWrapStyle =
                    new GUIStyle(style)
                    {
                        wordWrap = false
                    };

                NoWrapStyles[style] = noWrapStyle;
            }

            return noWrapStyle;
        }

        private static GUIStyle GetFontStyle(
            GUIStyle style,
            FontStyle fontStyle)
        {
            if (fontStyle == FontStyle.Normal)
                return style;

            if (!FontStyles.TryGetValue(
                    (style, fontStyle),
                    out GUIStyle fontStyleCopy))
            {
                fontStyleCopy =
                    new GUIStyle(style)
                    {
                        fontStyle = fontStyle
                    };

                FontStyles[(style, fontStyle)] = fontStyleCopy;
            }

            return fontStyleCopy;
        }

        private static FontStyle GetFontStyle(DescriptionRun run)
        {
            if (run.IsBold && run.IsItalic)
                return FontStyle.BoldAndItalic;

            if (run.IsBold)
                return FontStyle.Bold;

            return run.IsItalic
                ? FontStyle.Italic
                : FontStyle.Normal;
        }

        // A lone " " can measure as ~0 depending on the text backend; measure it in context instead.
        private static float GetSpaceWidth(GUIStyle style)
        {
            if (SpaceWidths.TryGetValue(
                    style,
                    out float spaceWidth))
            {
                return spaceWidth;
            }

            float withSpace =
                style.CalcSize(new GUIContent("a a")).x;

            float withoutSpace =
                style.CalcSize(new GUIContent("aa")).x;

            spaceWidth =
                Mathf.Max(
                    1f,
                    withSpace - withoutSpace);

            SpaceWidths[style] = spaceWidth;

            return spaceWidth;
        }

        // Broken links are checked for repair candidates here; the result is cached with the layout.
        private static List<Token> Tokenize(string text)
        {
            var tokens = new List<Token>();

            int linkIndex = 0;

            // Emphasis never splits a link, so each link lies completely inside one run.
            foreach (DescriptionRun run in DescriptionEmphasisParser.Parse(text))
            {
                FontStyle fontStyle =
                    GetFontStyle(
                        run);

                foreach (DescriptionSegment segment in DescriptionLinkParser.Parse(run.Text))
                {
                    if (segment.IsLink)
                    {
                        tokens.Add(
                            CreateLinkToken(
                                segment.Text,
                                linkIndex,
                                fontStyle));

                        linkIndex++;

                        continue;
                    }

                    AddTextTokens(
                        tokens,
                        segment.Text,
                        fontStyle);
                }
            }

            return tokens;
        }

        private static Token CreateLinkToken(
            string target,
            int linkIndex,
            FontStyle fontStyle)
        {
            Entry entry =
                DescriptionLinkResolver.Resolve(
                    target);

            if (entry == null)
            {
                bool repairable =
                    DescriptionLinkRepair.HasCandidates(
                        target);

                // Pinned links show their last known title, never the ID.
                return new Token(
                    repairable
                        ? TokenKind.RepairableLink
                        : TokenKind.BrokenLink,
                    DescriptionLinkResolver.GetLabel(
                        target),
                    fontStyle,
                    target: target,
                    linkIndex: linkIndex);
            }

            string label =
                string.IsNullOrEmpty(entry.Glyph)
                    ? entry.Title
                    : $"{entry.Glyph} {entry.Title}";

            return new Token(
                TokenKind.Link,
                label,
                fontStyle,
                entry);
        }

        private static void AddTextTokens(
            List<Token> tokens,
            string text,
            FontStyle fontStyle)
        {
            int start = 0;

            for (int i = 0; i <= text.Length; i++)
            {
                bool atEnd = i == text.Length;

                char character =
                    atEnd
                        ? '\0'
                        : text[i];

                bool isBreak =
                    atEnd ||
                    character == ' ' ||
                    character == '\t' ||
                    character == '\n' ||
                    character == '\r';

                if (!isBreak)
                    continue;

                if (i > start)
                {
                    tokens.Add(
                        new Token(
                            TokenKind.Word,
                            text.Substring(
                                start,
                                i - start),
                            fontStyle));
                }

                if (!atEnd)
                {
                    if (character == '\n')
                    {
                        tokens.Add(
                            new Token(
                                TokenKind.NewLine,
                                string.Empty,
                                fontStyle));
                    }
                    else if (character != '\r')
                    {
                        tokens.Add(
                            new Token(
                                TokenKind.Space,
                                character.ToString(),
                                fontStyle));
                    }
                }

                start = i + 1;
            }
        }
        #endregion
    }
}
