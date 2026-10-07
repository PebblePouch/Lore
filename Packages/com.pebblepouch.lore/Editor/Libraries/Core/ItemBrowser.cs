using System;
using System.Collections.Generic;

using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class ItemBrowser<T> : IDisposable where T : LibraryItem
    {
        #region Fields
        private readonly LibraryModel<T> _model;

        private readonly ILibraryCardRenderer<T> _renderer;
        private readonly Func<T, bool> _canPick;

        private readonly Action _create;
        private readonly Action<T> _edit;
        private readonly Action<T, Action<bool>> _delete;


        private readonly string _emptyMessage;
        private readonly string _noMatchesMessage;

        private readonly BrowserState _state;
        private readonly BrowserShell _shell;
        private readonly BrowserNavigation _navigation;

        private readonly Action _drawExternalTools;
        private readonly Func<float> _getExternalToolsHeight;

        private string _activeFilterLabel;

        private Func<T, bool> _pendingPreselection;

        private Func<T, bool> _pendingReveal;
        private Rect? _pendingRevealRect;
        private int? _pendingDeleteRecoveryIndex;
        #endregion

        #region Properties
        public Action<T> ItemClicked;
        #endregion

        #region Constructor
        public ItemBrowser(
            string title,
            string searchPlaceholder,
            string emptyMessage,
            string noMatchesMessage,
            BrowserState state,
            LibraryModel<T> model,
            ILibraryCardRenderer<T> renderer,
            Action create = null,
            Func<T, bool> canPick = null,
            Action<T> edit = null,
            Action<T, Action<bool>> delete = null,
            Action drawTools = null,
            Func<float> getToolsHeight = null)
        {
            _state = state;

            _navigation =
                new BrowserNavigation(
                    _state);

            _model = model;
            _renderer = renderer;

            _canPick = canPick ?? (_ => true);

            _create = create;
            _edit = edit;
            _delete = delete;

            _drawExternalTools = drawTools;
            _getExternalToolsHeight = getToolsHeight;

            _shell =
                new BrowserShell(
                    title,
                    searchPlaceholder,
                    BrowserStyle.LibrarySearchControlNamePrefix + typeof(T).Name,
                    _state,
                    _model,
                    create,
                    ResetKeyboardNavigation,
                    null,
                    drawTools: DrawTools,
                    getToolsHeight: GetToolsHeight);

            _emptyMessage =
                emptyMessage;

            _noMatchesMessage =
                noMatchesMessage;
        }
        #endregion

        #region Public API
        public void SetAvailableHeight(float height)
        {
            _shell.SetAvailableHeight(height);
        }

        public void SetTitle(string title)
        {
            _shell.SetTitle(title);
        }

        public void SetFilter(
            Func<T, bool> filter,
            string label)
        {
            _model.SetFilter(
                filter);

            _activeFilterLabel =
                label ?? string.Empty;

            _state.Scroll =
                Vector2.zero;

            ResetKeyboardNavigation();
        }

        public void ClearFilter()
        {
            _model.ClearFilter();

            _activeFilterLabel =
                string.Empty;

            _state.Scroll =
                Vector2.zero;

            ResetKeyboardNavigation();
        }

        public void Draw()
        {
            HandlePendingRevealScroll();

            HandleKeyboardNavigation();

            _navigation.BeginFrame();

            _shell.Draw(
                DrawContent);
        }

        public bool HandleEscape()
        {
            if (!LoreKeyboardFocus.IsOwner(LoreKeyboardOwner.Library))
            {
                return false;
            }

            if (LibraryEditingService.HandleEscape<T>())
                return true;

            if (_shell.HandleSearchEscape())
                return true;

            if (string.IsNullOrWhiteSpace(_activeFilterLabel))
            {
                return false;
            }

            ClearFilter();

            _shell.FocusSearch();

            Event.current.Use();

            return true;
        }

        public void FocusSearch()
        {
            _shell.FocusSearch();

            ResetKeyboardNavigation();
        }

        public void ClearSearch()
        {
            _shell.ClearSearch();
        }

        public void Preselect(Func<T, bool> predicate)
        {
            _pendingPreselection = predicate;
        }

        public void Reveal(Func<T, bool> predicate)
        {
            _pendingReveal = predicate;
        }

        public void Highlight(string id)
        {
            _navigation.Highlight(id);
        }

        public void Refresh()
        {
            _model.Refresh();

            ResetKeyboardNavigation();
        }

        public void Dispose()
        {
            _model.Dispose();
        }
        #endregion

        #region Draw
        private void DrawContent()
        {
            GUILayout.Space(LibraryStyle.ListTopSpacing);

            IReadOnlyList<T> items = _model.GetItems();

            if (items.Count == 0)
            {
                DrawEmpty();

                HandlePendingDeleteHighlightRecovery();

                return;
            }

            foreach (T item in items)
            {
                bool pickable = _canPick(item);

                bool highlighted = _navigation.IsHighlighted(item.Id);

                BrowserItemDrawResult result =
                    _renderer.Draw(
                        item,
                        highlighted,
                        pickable);

                _navigation.Register(
                    item.Id,
                    result.Rect,
                    pickable
                        ? () => Pick(
                            item)
                        : null,
                    edit:
                        _edit != null
                            ? () => _edit(
                                item)
                            : null,
                    delete:
                        _delete != null
                            ? () =>
                                RequestDelete(
                                    item)
                            : null);

                HandlePendingPreselection(
                    item,
                    result.Rect,
                    pickable);

                HandlePendingReveal(
                    item,
                    result.Rect);

                if (!result.Clicked)
                    continue;

                Pick(item);
            }

            HandlePendingDeleteHighlightRecovery();
        }

        private void DrawEmpty()
        {
            string message =
                _model.IsEmpty
                    ? _emptyMessage
                    : _noMatchesMessage;

            EmptyStateCard.Draw(
                message);
        }

        private void DrawTools()
        {
            if (!string.IsNullOrWhiteSpace(_activeFilterLabel))
            {
                BrowserFilterCard.Draw(_activeFilterLabel, ClearFilter);
            }

            _drawExternalTools?.Invoke();
        }
        #endregion

        #region Commands
        private void Pick(
            T item)
        {
            if (!_canPick(item))
                return;

            ItemClicked?.Invoke(
                item);

            ResetKeyboardNavigation();
        }

        public void ClearHighlight()
        {
            _pendingPreselection = null;

            _navigation.Reset();
        }
        #endregion

        #region Helpers
        private float GetToolsHeight()
        {
            float height = 0f;

            if (!string.IsNullOrWhiteSpace(_activeFilterLabel))
            {
                height += BrowserStyle.FilterHeight;
            }

            height += _getExternalToolsHeight?.Invoke()
                ?? 0f;

            return height;
        }

        private void HandleKeyboardNavigation()
        {
            if (!LoreKeyboardFocus.IsOwner(
                    LoreKeyboardOwner.Library))
            {
                return;
            }

            if (LibraryEditingService.IsEditing<T>())
            {
                LibraryEditingService.HandleCommit<T>();

                return;
            }

            Event current =
                Event.current;

            if (current.type ==
                    EventType.KeyDown &&
                current.keyCode ==
                    KeyCode.Insert &&
                _create != null)
            {
                _create.Invoke();

                current.Use();

                return;
            }

            _navigation.HandleKeyboard();
        }

        private void ResetKeyboardNavigation()
        {
            _navigation.Reset();
        }

        private void HandlePendingPreselection(
            T item,
            Rect rect,
            bool pickable)
        {
            if (_pendingPreselection == null)
                return;

            if (Event.current.type != EventType.Repaint)
                return;

            if (!_pendingPreselection.Invoke(item))
            {
                return;
            }

            _pendingPreselection = null;

            _navigation.Highlight(item.Id);

            _pendingRevealRect = rect;

            LoreRepaintService.Repaint();
        }

        private void HandlePendingReveal(
            T item,
            Rect rect)
        {
            if (_pendingReveal == null)
                return;

            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            if (!_pendingReveal.Invoke(item))
            {
                return;
            }

            _pendingRevealRect = rect;

            _pendingReveal = null;

            LoreRepaintService.Repaint();
        }

        private void HandlePendingRevealScroll()
        {
            if (!_pendingRevealRect.HasValue)
                return;

            _navigation.Reveal(_pendingRevealRect.Value);

            _pendingRevealRect = null;

            LoreRepaintService.Repaint();
        }

        public void RequestDelete(T item)
        {
            if (item == null ||
                _delete == null)
            {
                return;
            }

            IReadOnlyList<T> items =
                _model.GetItems();

            int targetIndex =
                -1;

            for (int i = 0;
                 i < items.Count;
                 i++)
            {
                if (items[i]?.Id != item.Id)
                    continue;

                targetIndex =
                    i;

                break;
            }

            if (targetIndex < 0)
                return;

            _delete(
                item,
                deleted =>
                {
                    if (!deleted)
                    {
                        _navigation.Highlight(
                            item.Id);

                        LoreRepaintService.Repaint();

                        return;
                    }

                    _pendingDeleteRecoveryIndex =
                        targetIndex;

                    LoreRepaintService.Repaint();
                });
        }
        #endregion

        #region Helpers
        private void HandlePendingDeleteHighlightRecovery()
        {
            if (!_pendingDeleteRecoveryIndex.HasValue)
                return;

            if (Event.current.type !=
                EventType.Repaint)
            {
                return;
            }

            _navigation.HighlightAtIndex(
                _pendingDeleteRecoveryIndex.Value);

            _pendingDeleteRecoveryIndex =
                null;

            LoreRepaintService.Repaint();
        }
        #endregion
    }
}