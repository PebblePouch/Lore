using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

namespace PebblePouch.Lore.Editor
{
    // Quick Search (Ctrl+F / Cmd+F in the Lore window): a small popup window centered over the Lore window that
    // jumps to any Entry, Category or Field. Arrow keys select, Enter or click opens, Esc or a click outside closes
    // (dropdown windows close when they lose focus). Its own window, so the Lore content below can never take
    // its cursor, tooltips, clicks or keyboard focus; the Lore window only dims itself while it is open.
    public sealed class QuickSearchWindow : EditorWindow
    {
        #region Fields
        private static QuickSearchWindow _instance;

        private string _query = string.Empty;
        private string _resultsQuery;
        private List<QuickOpenSearch.Result> _results = new();

        private int _selected;
        private bool _focusRequested = true;
        #endregion

        #region Properties
        public static bool IsOpen => _instance != null;
        #endregion

        #region Public API
        // ownerRect: the Lore window's screen rect (EditorWindow.position).
        public static void Open(Rect ownerRect)
        {
            if (_instance != null)
            {
                _instance.Focus();

                return;
            }

            float width =
                Mathf.Min(
                    QuickOpenStyle.MaxWidth,
                    ownerRect.width - QuickOpenStyle.SideMargin * 2f);

            // Centered for the largest result list, so the window does not jump while typing (it grows down).
            float y =
                ownerRect.y
                + Mathf.Max(
                    QuickOpenStyle.SideMargin,
                    (ownerRect.height - GetWindowHeight(QuickOpenStyle.MaxResults)) * 0.5f);

            var window = CreateInstance<QuickSearchWindow>();

            _instance = window;

            window.wantsMouseMove = true;

            window.ShowAsDropDown(
                new Rect(
                    ownerRect.x + (ownerRect.width - width) * 0.5f,
                    y,
                    width,
                    0f),
                new Vector2(
                    width,
                    GetWindowHeight(1)));

            LoreRepaintService.Repaint();
        }
        #endregion

        #region Unity
        private void OnGUI()
        {
            LoreActiveTextEditor.HandleTextBoundaryKeys();

            HandleKeyboard();

            UpdateResults();

            FitHeight();

            Rect windowRect =
                new(0f, 0f, position.width, position.height);

            Rect boxRect =
                new(
                    QuickOpenStyle.BorderWidth,
                    QuickOpenStyle.BorderWidth,
                    windowRect.width - QuickOpenStyle.BorderWidth * 2f,
                    windowRect.height - QuickOpenStyle.BorderWidth * 2f);

            Rect headerRect =
                new(boxRect.x, boxRect.y, boxRect.width, QuickOpenStyle.HeaderHeight);

            Rect bodyRect =
                new(
                    boxRect.x,
                    headerRect.yMax + QuickOpenStyle.HeaderSpacing,
                    boxRect.width,
                    boxRect.yMax - headerRect.yMax - QuickOpenStyle.HeaderSpacing);

            if (Event.current.type == EventType.Repaint)
            {
                // Border, header and body (the 1px gap shows the border color, like the browser's header spacing).
                EditorGUI.DrawRect(windowRect, QuickOpenStyle.Border);
                EditorGUI.DrawRect(headerRect, QuickOpenStyle.HeaderBackground);
                EditorGUI.DrawRect(bodyRect, QuickOpenStyle.Background);
            }

            GUI.Label(
                new Rect(
                    headerRect.x + QuickOpenStyle.HeaderInset,
                    headerRect.y,
                    headerRect.width - QuickOpenStyle.HeaderInset,
                    headerRect.height),
                LoreText.QuickOpen.Title,
                QuickOpenStyle.Header);

            DrawSearch(bodyRect);

            if (_results.Count == 0)
            {
                GUI.Label(
                    GetRowRect(bodyRect, 0),
                    string.IsNullOrWhiteSpace(_query)
                        ? LoreText.QuickOpen.Hint
                        : LoreText.QuickOpen.NoMatches,
                    QuickOpenStyle.Hint);

                return;
            }

            for (int i = 0; i < _results.Count; i++)
            {
                DrawResult(
                    GetRowRect(bodyRect, i),
                    i);
            }

            if (Event.current.type == EventType.MouseMove)
                Repaint();
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;

            LoreRepaintService.Repaint();
        }
        #endregion

        #region Draw
        private void DrawSearch(Rect bodyRect)
        {
            Rect searchRect =
                new(
                    bodyRect.x + QuickOpenStyle.Padding,
                    bodyRect.y + QuickOpenStyle.Padding,
                    bodyRect.width - QuickOpenStyle.Padding * 2f,
                    QuickOpenStyle.SearchHeight);

            GUI.SetNextControlName(QuickOpenStyle.SearchControlName);

            EditorGUI.BeginChangeCheck();

            _query =
                EditorGUI.TextField(
                    searchRect,
                    _query,
                    QuickOpenStyle.Search);

            if (EditorGUI.EndChangeCheck())
                _selected = 0;

            if (_focusRequested &&
                Event.current.type == EventType.Repaint)
            {
                _focusRequested = false;

                EditorGUI.FocusTextInControl(QuickOpenStyle.SearchControlName);
            }
        }

        private void DrawResult(
            Rect rowRect,
            int index)
        {
            QuickOpenSearch.Result result = _results[index];

            bool hovered = rowRect.Contains(Event.current.mousePosition);

            if (Event.current.type == EventType.Repaint)
            {
                if (index == _selected)
                    EditorGUI.DrawRect(rowRect, QuickOpenStyle.RowSelected);
                else if (hovered)
                    EditorGUI.DrawRect(rowRect, QuickOpenStyle.RowHover);
            }

            EditorGUIUtility.AddCursorRect(rowRect, MouseCursor.Link);

            float detailWidth =
                rowRect.width * QuickOpenStyle.DetailWidthRatio;

            GUI.Label(
                new Rect(rowRect.x, rowRect.y, QuickOpenStyle.GlyphWidth, rowRect.height),
                result.Glyph,
                QuickOpenStyle.Glyph);

            GUI.Label(
                new Rect(
                    rowRect.x + QuickOpenStyle.GlyphWidth,
                    rowRect.y,
                    rowRect.width - QuickOpenStyle.GlyphWidth - detailWidth - QuickOpenStyle.Padding,
                    rowRect.height),
                result.Title,
                QuickOpenStyle.Title);

            GUI.Label(
                new Rect(
                    rowRect.xMax - detailWidth - QuickOpenStyle.Padding,
                    rowRect.y,
                    detailWidth,
                    rowRect.height),
                result.Detail,
                QuickOpenStyle.Detail);

            if (Event.current.type == EventType.MouseDown &&
                Event.current.button == 0 &&
                hovered)
            {
                Event.current.Use();

                OpenResult(index);
            }
        }
        #endregion

        #region Input
        private void HandleKeyboard()
        {
            Event current = Event.current;

            if (current.type != EventType.KeyDown)
                return;

            switch (current.keyCode)
            {
                case KeyCode.Escape:
                    current.Use();
                    Close();
                    break;

                case KeyCode.DownArrow:
                    current.Use();
                    MoveSelection(1);
                    break;

                case KeyCode.UpArrow:
                    current.Use();
                    MoveSelection(-1);
                    break;

                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    current.Use();
                    OpenResult(_selected);
                    break;
            }
        }

        private void MoveSelection(int delta)
        {
            UpdateResults();

            if (_results.Count == 0)
                return;

            _selected =
                (_selected + delta + _results.Count) % _results.Count;

            Repaint();
        }

        private void OpenResult(int index)
        {
            UpdateResults();

            if (index < 0 ||
                index >= _results.Count)
            {
                return;
            }

            Object target = _results[index].Target;

            Close();

            // Back to the Lore window; navigation reveals the target in the browser (clearing search and filter if
            // they hide it). Entries and Categories close an open Library first; Fields open the Field Library.
            LoreWindow.Current?.Focus();

            switch (target)
            {
                case Entry entry:
                    LibraryService.CloseLibrary?.Invoke();
                    LoreNavigation.NavigateToEntry(entry);
                    break;

                case Category category:
                    LibraryService.CloseLibrary?.Invoke();
                    LoreNavigation.NavigateToCategory(category);
                    break;

                case Field field:
                    LibraryService.RevealField?.Invoke(field);
                    break;
            }
        }
        #endregion

        #region Helpers
        // Recalculated only when the query changed.
        private void UpdateResults()
        {
            if (_resultsQuery == _query)
                return;

            _resultsQuery = _query;

            _results =
                QuickOpenSearch.Find(
                    _query,
                    QuickOpenStyle.MaxResults);

            _selected =
                Mathf.Clamp(
                    _selected,
                    0,
                    Mathf.Max(0, _results.Count - 1));
        }

        // The window grows and shrinks with the result list (its top edge stays).
        private void FitHeight()
        {
            if (Event.current.type != EventType.Layout)
                return;

            float height =
                GetWindowHeight(
                    Mathf.Max(1, _results.Count));

            if (Mathf.Approximately(position.height, height))
                return;

            Rect rect = position;
            rect.height = height;

            minSize = new Vector2(rect.width, height);
            maxSize = new Vector2(rect.width, height);

            position = rect;
        }

        private static float GetWindowHeight(int rows)
        {
            return QuickOpenStyle.BorderWidth * 2f
                   + QuickOpenStyle.HeaderHeight
                   + QuickOpenStyle.HeaderSpacing
                   + QuickOpenStyle.Padding * 3f
                   + QuickOpenStyle.SearchHeight
                   + rows * (QuickOpenStyle.RowHeight + QuickOpenStyle.RowSpacing);
        }

        private static Rect GetRowRect(
            Rect bodyRect,
            int index)
        {
            return new Rect(
                bodyRect.x + QuickOpenStyle.Padding,
                bodyRect.y
                + QuickOpenStyle.Padding * 2f
                + QuickOpenStyle.SearchHeight
                + index * (QuickOpenStyle.RowHeight + QuickOpenStyle.RowSpacing),
                bodyRect.width - QuickOpenStyle.Padding * 2f,
                QuickOpenStyle.RowHeight);
        }
        #endregion
    }
}
