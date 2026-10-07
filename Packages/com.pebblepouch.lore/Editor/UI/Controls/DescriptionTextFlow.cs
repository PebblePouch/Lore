using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Draws description text with [[Entry]] links in a word-wrapped flow (view mode only).
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

            // Links only: the stored target and the link's position in the text (for repairs).
            public readonly string Target;
            public readonly int LinkIndex;

            public Token(
                TokenKind kind,
                string text,
                Entry entry = null,
                string target = null,
                int linkIndex = -1)
            {
                Kind = kind;
                Text = text;
                Entry = entry;
                Target = target;
                LinkIndex = linkIndex;
            }
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

            foreach (Token token in Tokenize(text))
            {
                if (token.Kind == TokenKind.NewLine)
                {
                    x = 0f;
                    y += lineHeight;

                    continue;
                }

                GUIStyle style =
                    GetStyle(
                        token.Kind,
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
                    x = 0f;
                    y += lineHeight;
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
            GUIStyle textStyle)
        {
            return kind switch
            {
                TokenKind.Link => InspectorStyle.DescriptionLinkContent,
                TokenKind.BrokenLink => InspectorStyle.DescriptionBrokenLinkContent,
                TokenKind.RepairableLink => InspectorStyle.DescriptionRepairableLinkContent,
                _ => textStyle
            };
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

            foreach (DescriptionSegment segment in DescriptionLinkParser.Parse(text))
            {
                if (segment.IsLink)
                {
                    tokens.Add(
                        CreateLinkToken(
                            segment.Text,
                            linkIndex));

                    linkIndex++;

                    continue;
                }

                AddTextTokens(
                    tokens,
                    segment.Text);
            }

            return tokens;
        }

        private static Token CreateLinkToken(
            string target,
            int linkIndex)
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
                entry);
        }

        private static void AddTextTokens(
            List<Token> tokens,
            string text)
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
                                i - start)));
                }

                if (!atEnd)
                {
                    if (character == '\n')
                    {
                        tokens.Add(
                            new Token(
                                TokenKind.NewLine,
                                string.Empty));
                    }
                    else if (character != '\r')
                    {
                        tokens.Add(
                            new Token(
                                TokenKind.Space,
                                character.ToString()));
                    }
                }

                start = i + 1;
            }
        }
        #endregion
    }
}
