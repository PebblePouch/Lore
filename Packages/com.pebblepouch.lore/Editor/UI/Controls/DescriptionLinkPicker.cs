using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Inline Entry picker shown below a description text area while the caret is inside an open "[[query".
    // The text area keeps keyboard focus the whole time:
    // - Up/Down move the highlight, Enter/Tab/click insert the link, Escape closes the picker only.
    // - Insertion simulates typing (public API), so Unity's text field places the caret itself.
    // Visibility and rows only change during Layout so GUILayout stays consistent across events.
    public sealed class DescriptionLinkPicker
    {
        #region Fields
        private const int MaxResults = 8;

        private readonly EntrySearchMatcher _matcher = new();

        private readonly List<Entry> _results = new();
        private readonly List<Rect> _rowRects = new();

        // Layout-stable state (changes only during Layout).
        private bool _visible;

        // Current "[[query" token at the caret.
        private int _tokenStart = -1;
        private int _caretIndex = -1;
        private string _query;

        private int _highlightIndex;

        // Escape closes the picker for this token until the caret leaves it.
        private int _dismissedTokenStart = -1;

        private bool _isInserting;

        // Enter/Tab arrive as a keyCode event followed by a separate character event ('\n' / '\t');
        // the character event must be swallowed too, or the text area inserts it.
        private char _swallowCharacter;
        #endregion

        #region Properties
        public bool IsVisible => _visible;
        #endregion


        #region Public API
        // Before the text area: picker keys and row clicks. Returns true if the event was consumed.
        public bool HandleInput(string text)
        {
            Event current = Event.current;

            if (_swallowCharacter != '\0' &&
                current.type == EventType.KeyDown)
            {
                char swallow = _swallowCharacter;

                _swallowCharacter = '\0';

                if (current.keyCode == KeyCode.None &&
                    current.character == swallow)
                {
                    current.Use();

                    return true;
                }
            }

            if (!_visible ||
                _isInserting)
            {
                return false;
            }

            if (current.type == EventType.MouseDown &&
                current.button == 0)
            {
                for (int i = 0; i < _rowRects.Count && i < _results.Count; i++)
                {
                    if (!_rowRects[i].Contains(current.mousePosition))
                        continue;

                    Insert(
                        text,
                        _results[i]);

                    current.Use();

                    return true;
                }

                return false;
            }

            if (current.type != EventType.KeyDown)
                return false;

            switch (current.keyCode)
            {
                case KeyCode.UpArrow:
                    MoveHighlight(-1);
                    break;

                case KeyCode.DownArrow:
                    MoveHighlight(1);
                    break;

                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                case KeyCode.Tab:
                    if (_results.Count == 0)
                        return false;

                    _swallowCharacter =
                        current.keyCode == KeyCode.Tab
                            ? '\t'
                            : '\n';

                    Insert(
                        text,
                        _results[Mathf.Clamp(_highlightIndex, 0, _results.Count - 1)]);
                    break;

                case KeyCode.Escape:
                    _dismissedTokenStart = _tokenStart;
                    break;

                default:
                    return false;
            }

            current.Use();

            LoreRepaintService.Repaint();

            return true;
        }

        // After the text area: track the "[[query" token at the caret (Layout only).
        public void Update(
            string text,
            string controlName,
            bool isEditing)
        {
            if (Event.current.type != EventType.Layout)
                return;

            if (!isEditing ||
                _isInserting ||
                !LoreActiveTextEditor.TryGetCaret(
                    controlName,
                    out int caretIndex,
                    out _) ||
                !TryFindToken(
                    text,
                    caretIndex,
                    out int tokenStart,
                    out string query))
            {
                Hide();

                return;
            }

            if (tokenStart != _dismissedTokenStart)
                _dismissedTokenStart = -1;

            if (_dismissedTokenStart >= 0)
            {
                Hide();

                return;
            }

            bool queryChanged =
                !_visible ||
                tokenStart != _tokenStart ||
                query != _query;

            _tokenStart = tokenStart;
            _caretIndex = caretIndex;
            _query = query;

            if (queryChanged)
            {
                RefreshResults();

                _highlightIndex = 0;
            }

            _visible = true;
        }

        // Reserves and draws the rows below the text area. Returns the drawn rect (empty if hidden).
        public Rect Draw()
        {
            if (!_visible)
                return Rect.zero;

            // Rows use the compact Insights statistic cards (e.g. Top 10 Inbound Entries).
            StatisticCardStyle cardStyle =
                InspectorStyle.DescriptionLinkPickerCard;

            float rowStep =
                InsightsStyle.StatisticCardHeight
                + InsightsStyle.StatisticCardSpacing;

            int rowCount =
                Mathf.Max(
                    1,
                    _results.Count);

            Rect rect =
                GUILayoutUtility.GetRect(
                    0f,
                    InspectorStyle.DescriptionLinkPickerTopSpacing + rowCount * rowStep,
                    GUILayout.ExpandWidth(true));

            if (Event.current.type != EventType.Repaint)
                return rect;

            _rowRects.Clear();

            float top =
                rect.y + InspectorStyle.DescriptionLinkPickerTopSpacing;

            if (_results.Count == 0)
            {
                var emptyItem =
                    new StatisticCardItem(
                        LoreText.Library.EntryNoMatches,
                        InsightsStyle.StatisticsCardDefaultColor);

                StatisticCard.Draw(
                    new Rect(
                        rect.x,
                        top,
                        Mathf.Min(
                            StatisticCard.GetWidth(
                                emptyItem,
                                cardStyle),
                            rect.width),
                        InsightsStyle.StatisticCardHeight),
                    emptyItem,
                    cardStyle);

                return rect;
            }

            for (int i = 0; i < _results.Count; i++)
            {
                Entry entry = _results[i];

                // Action is a no-op: clicks are handled in HandleInput before the text area sees them;
                // it only enables the card's hover overlay and link cursor.
                var item =
                    new StatisticCardItem(
                        entry.Title,
                        entry.Category != null
                            ? entry.Category.Color
                            : InsightsStyle.StatisticsCardDefaultLinkColor,
                        StatisticCardAction.Selection,
                        () => { },
                        entry.Glyph,
                        entry.Category != null
                            ? entry.Category.Title
                            : null);

                Rect rowRect =
                    new(
                        rect.x,
                        top + i * rowStep,
                        Mathf.Min(
                            StatisticCard.GetWidth(
                                item,
                                cardStyle),
                            rect.width),
                        InsightsStyle.StatisticCardHeight);

                _rowRects.Add(rowRect);

                StatisticCard.Draw(
                    rowRect,
                    item,
                    cardStyle);

                // Keyboard highlight uses the same overlay as mouse hover (not doubled while hovered).
                if (i == _highlightIndex &&
                    !rowRect.Contains(Event.current.mousePosition))
                {
                    EditorGUI.DrawRect(
                        rowRect,
                        InsightsStyle.StatisticsHoverOverlay);
                }
            }

            return rect;
        }

        public void Reset()
        {
            Hide();

            _dismissedTokenStart = -1;
            _isInserting = false;
            _swallowCharacter = '\0';
        }
        #endregion

        #region Helpers
        private void Hide()
        {
            _visible = false;

            _tokenStart = -1;
            _caretIndex = -1;
            _query = null;

            _results.Clear();
            _rowRects.Clear();
        }

        private void MoveHighlight(int delta)
        {
            if (_results.Count == 0)
                return;

            _highlightIndex =
                (_highlightIndex + delta + _results.Count) % _results.Count;
        }

        private void RefreshResults()
        {
            _results.Clear();

            IEnumerable<Entry> entries =
                LoreRepository.Instance.Groups
                    .SelectMany(
                        group =>
                            group.Entries)
                    .Where(
                        entry =>
                            entry != null);

            if (string.IsNullOrWhiteSpace(_query))
            {
                _results.AddRange(
                    entries.Take(
                        MaxResults));

                return;
            }

            string query = _query.Trim();

            // Title prefix matches first, then other title matches, then subtitle/glyph/tag matches.
            _results.AddRange(
                entries
                    .Where(
                        entry =>
                            _matcher.Matches(
                                entry,
                                query))
                    .Select(
                        (entry, index) =>
                            (entry, index))
                    .OrderBy(
                        pair =>
                            GetRank(
                                pair.entry,
                                query))
                    .ThenBy(
                        pair =>
                            pair.index)
                    .Select(
                        pair =>
                            pair.entry)
                    .Take(
                        MaxResults));
        }

        private static int GetRank(
            Entry entry,
            string query)
        {
            if (entry.Title.StartsWith(
                    query,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }

            return EntrySearchMatcher.MatchesText(
                entry.Title,
                query)
                ? 1
                : 2;
        }

        // Finds an open "[[query" directly before the caret on the same line.
        private static bool TryFindToken(
            string text,
            int caretIndex,
            out int tokenStart,
            out string query)
        {
            tokenStart = -1;
            query = null;

            if (string.IsNullOrEmpty(text) ||
                caretIndex < 2 ||
                caretIndex > text.Length)
            {
                return false;
            }

            string beforeCaret =
                text.Substring(
                    0,
                    caretIndex);

            int open =
                beforeCaret.LastIndexOf(
                    DescriptionLinkParser.LinkOpen,
                    System.StringComparison.Ordinal);

            if (open < 0)
                return false;

            string candidate =
                beforeCaret.Substring(
                    open + DescriptionLinkParser.LinkOpen.Length);

            if (candidate.IndexOfAny(new[] { '[', ']', '\n', '\r' }) >= 0)
                return false;

            tokenStart = open;
            query = candidate;

            return true;
        }

        // Length of the target text between the caret and a closing "]]" on the same line
        // (0 = "]]" directly after the caret), or -1 when the caret is not inside a link.
        private static int GetTrailingTargetLength(
            string text,
            int caretIndex)
        {
            if (string.IsNullOrEmpty(text) ||
                caretIndex < 0 ||
                caretIndex > text.Length)
            {
                return -1;
            }

            int close =
                text.IndexOf(
                    DescriptionLinkParser.LinkClose,
                    caretIndex,
                    System.StringComparison.Ordinal);

            if (close < 0)
                return -1;

            string trailing =
                text.Substring(
                    caretIndex,
                    close - caretIndex);

            return trailing.IndexOfAny(new[] { '[', ']', '\n', '\r' }) >= 0
                ? -1
                : trailing.Length;
        }

        // Replaces the typed query with the link target by simulating keystrokes:
        // Unity's own text field handles them, keeping caret, selection and undo consistent.
        private void Insert(
            string text,
            Entry entry)
        {
            if (entry == null ||
                _tokenStart < 0 ||
                _caretIndex < 0)
            {
                return;
            }

            EditorWindow window = EditorWindow.focusedWindow;

            if (window == null)
                return;

            int queryLength =
                _caretIndex - (_tokenStart + DescriptionLinkParser.LinkOpen.Length);

            // Caret inside an existing link ("[[Test |Entry A]]"): the rest up to its "]]" is replaced too,
            // otherwise "[[Test Entry B]]Entry A]]" would keep parts of the old target.
            int trailingLength =
                GetTrailingTargetLength(
                    text,
                    _caretIndex);

            bool hasClose =
                trailingLength >= 0;

            string typed =
                DescriptionLinkResolver.GetEditableTarget(entry)
                + (hasClose
                    ? string.Empty
                    : DescriptionLinkParser.LinkClose);

            if (queryLength < 0)
                return;

            // The picker hides on the next Layout (Update checks _isInserting).
            _isInserting = true;

            // Delayed: sending events from inside OnGUI would re-enter OnGUI.
            EditorApplication.delayCall +=
                () =>
                {
                    for (int i = 0; i < queryLength; i++)
                    {
                        // Unity's own key event format, so the text field recognizes it as an editing key.
                        window.SendEvent(
                            Event.KeyboardEvent(
                                "backspace"));
                    }

                    for (int i = 0; i < trailingLength; i++)
                    {
                        window.SendEvent(
                            Event.KeyboardEvent(
                                "delete"));
                    }

                    foreach (char character in typed)
                    {
                        window.SendEvent(
                            new Event
                            {
                                type = EventType.KeyDown,
                                character = character
                            });
                    }

                    // Step over an existing "]]" so the caret ends after the link.
                    if (hasClose)
                    {
                        for (int i = 0; i < DescriptionLinkParser.LinkClose.Length; i++)
                        {
                            window.SendEvent(
                                Event.KeyboardEvent(
                                    "right"));
                        }
                    }

                    _isInserting = false;

                    LoreRepaintService.Repaint();
                };
        }
        #endregion
    }
}
