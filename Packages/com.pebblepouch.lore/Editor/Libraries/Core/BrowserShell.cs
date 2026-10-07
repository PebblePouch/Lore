using System;
using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class BrowserShell
    {
        #region Fields
        private readonly BrowserState _state;
        private readonly BrowserHeaderSection _headerSection;

        private readonly ISearchModel _searchModel;

        private readonly Action _searchChanged;
        private readonly Action _searchActivated;

        private readonly string _searchPlaceholder;
        private readonly string _searchControlName;

        private readonly Action _drawTools;
        private readonly Func<float> _getToolsHeight;

        private bool _searchFocusRequested;

        // Scroll view rect from the last non-Layout event (middle mouse panning, see ScrollPan).
        private Rect _scrollRect;
        #endregion

        #region Constructor
        public BrowserShell(
            string title,
            string searchPlaceholder,
            string searchControlName,
            BrowserState state,
            ISearchModel searchModel,
            Action create = null,
            Action searchChanged = null,
            Action searchActivated = null,
            Action drawTools = null,
            Func<float> getToolsHeight = null)
        {
            _state = state;

            _headerSection = new BrowserHeaderSection(
                title,
                create);

            _searchModel = searchModel;

            _searchChanged = searchChanged;
            _searchActivated = searchActivated;

            _searchPlaceholder = searchPlaceholder;
            _searchControlName = searchControlName;

            _drawTools = drawTools;
            _getToolsHeight = getToolsHeight;
        }
        #endregion

        #region Public API
        public void SetTitle(string title)
        {
            _headerSection.SetTitle(title);
        }

        public void Draw(Action body)
        {
            DrawHeader();
            DrawSearch();
            DrawTools();

            // Before the body: the pan control ID must not depend on the rows drawn.
            int panControlId = ScrollPan.GetControlId();

            _state.Scroll =
                ScrollPan.Handle(
                    panControlId,
                    _scrollRect,
                    _state.Scroll);

            _state.Scroll =
                EditorLayout.ScrollView(
                    _state.Scroll,
                    body);

            if (Event.current.type != EventType.Layout)
                _scrollRect = GUILayoutUtility.GetLastRect();
        }

        public void SetAvailableHeight(float height)
        {
            if (height <= 0f)
                return;

            float toolsHeight =
                _getToolsHeight?.Invoke()
                ?? 0f;

            _state.ViewportHeight =
                Mathf.Max(
                    0f,
                    height
                    - BrowserStyle.BrowserHeaderHeight
                    - BrowserStyle.SearchCardHeight
                    - toolsHeight
                    - BrowserStyle.ViewportBottomInset);
        }

        public void FocusSearch()
        {
            _searchFocusRequested = true;
        }

        public void ClearSearch()
        {
            if (string.IsNullOrWhiteSpace(_searchModel.Search))
            {
                return;
            }

            _searchModel.Search = string.Empty;

            _searchModel.Refresh();

            _searchChanged?.Invoke();
        }

        public void ReleaseSearchFocus()
        {
            _searchFocusRequested = false;

            LoreGUIFocus.Clear();
        }

        public bool HandleSearchEscape()
        {
            if (Event.current.type != EventType.KeyDown)
                return false;

            if (Event.current.keyCode != KeyCode.Escape)
                return false;

            if (string.IsNullOrWhiteSpace(_searchModel.Search))
            {
                return false;
            }

            _searchModel.Search = string.Empty;

            _searchModel.Refresh();

            _searchChanged?.Invoke();

            LoreGUIFocus.Clear();

            FocusSearch();

            Event.current.Use();

            return true;
        }
        #endregion

        #region Draw
        private void DrawHeader()
        {
            _headerSection.Draw();
        }

        private void DrawSearch()
        {
            string previousSearch =
                _searchModel.Search;

            SearchCard.Draw(
                _searchModel,
                _searchPlaceholder,
                _searchControlName,
                _searchFocusRequested,
                _searchActivated);

            if (previousSearch !=
                _searchModel.Search)
            {
                _searchChanged?.Invoke();
            }

            if (_searchFocusRequested &&
                Event.current.type ==
                EventType.Repaint)
            {
                _searchFocusRequested = false;
            }
        }

        private void DrawTools()
        {
            _drawTools?.Invoke();
        }
        #endregion
    }
}