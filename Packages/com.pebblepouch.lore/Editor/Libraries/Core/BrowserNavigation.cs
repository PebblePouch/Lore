using System;
using System.Collections.Generic;

using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class BrowserNavigation
    {
        #region Fields
        private readonly BrowserState _state;

        private readonly List<BrowserNavigationTarget> _targets = new();

        private string _highlightedId;
        private readonly bool _resetOnActivate;
        #endregion

        #region Properties
        public int HighlightedIndex => FindHighlightedIndex();
        #endregion

        #region Constructor
        public BrowserNavigation(
            BrowserState state,
            bool resetOnActivate = true)
        {
            _state = state;
            _resetOnActivate = resetOnActivate;
        }
        #endregion

        #region Public API
        public void BeginFrame()
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            _targets.Clear();
        }

        public void Register(
            string id,
            Rect rect,
            Action activate,
            Action navigateLeft = null,
            Action navigateRight = null,
            Action edit = null,
            Action delete = null,
            Action createChild = null)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            _targets.Add(
                new BrowserNavigationTarget(
                    id,
                    rect,
                    activate,
                    navigateLeft,
                    navigateRight,
                    edit,
                    delete,
                    createChild));
        }

        public bool IsHighlighted(string id)
        {
            return !string.IsNullOrEmpty(_highlightedId) 
                && _highlightedId == id;
        }

        public void HighlightAtIndex(int index)
        {
            if (_targets.Count == 0)
            {
                Reset();

                return;
            }

            index =
                Mathf.Clamp(
                    index,
                    0,
                    _targets.Count - 1);

            HighlightTarget(index);
        }

        public void HandleKeyboard()
        {
            if (Event.current.type != EventType.KeyDown)
            {
                return;
            }

            switch (Event.current.keyCode)
            {
                case KeyCode.UpArrow:
                    Move(-1);
                    Event.current.Use();
                    break;

                case KeyCode.DownArrow:
                    Move(1);
                    Event.current.Use();
                    break;

                case KeyCode.LeftArrow:
                    if (NavigateLeft())
                    {
                        Event.current.Use();
                    }
                    break;

                case KeyCode.RightArrow:
                    if (NavigateRight())
                    {
                        Event.current.Use();
                    }
                    break;

                case KeyCode.F2:
                    if (Edit())
                    {
                        Event.current.Use();
                    }
                    break;

                case KeyCode.Delete:
                case KeyCode.KeypadPeriod:
                    if (Delete())
                    {
                        Event.current.Use();
                    }
                    break;

                // With Shift or Ctrl, Home/End belong to the focused search field (select / move to start or end).
                case KeyCode.Home when !Event.current.shift && !Event.current.control:
                    MoveToBoundary(false);
                    Event.current.Use();
                    break;

                case KeyCode.End when !Event.current.shift && !Event.current.control:
                    MoveToBoundary(true);
                    Event.current.Use();
                    break;

                case KeyCode.PageUp:
                    MovePage(-1);
                    Event.current.Use();
                    break;

                case KeyCode.PageDown:
                    MovePage(1);
                    Event.current.Use();
                    break;


                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    if (Activate())
                    {
                        Event.current.Use();
                    }
                    break;

                case KeyCode.Plus:
                case KeyCode.KeypadPlus:
                    if (CreateChild())
                    {
                        Event.current.Use();
                    }
                    break;
            }
        }

        public void Reveal(Rect rect)
        {
            EnsureVisible(rect);
        }

        public void Highlight(string id)
        {
            if (string.IsNullOrEmpty(id))
                return;

            _highlightedId = id;

            int index = FindHighlightedIndex();

            if (index < 0)
                return;

            EnsureVisible(_targets[index].Rect);
        }

        public void Reset()
        {
            _highlightedId = null;
        }
        #endregion

        #region Navigation
        private bool CreateChild()
        {
            int index = FindHighlightedIndex();

            if (index < 0)
                return false;

            Action createChild = _targets[index].CreateChild;

            if (createChild == null)
                return false;

            createChild.Invoke();

            return true;
        }

        private bool Activate()
        {
            int index = FindHighlightedIndex();

            if (index < 0)
                return false;

            Action activate = _targets[index].Activate;

            if (activate == null)
                return false;

            activate.Invoke();

            if (_resetOnActivate)
            {
                Reset();
            }

            return true;
        }

        private bool Edit()
        {
            int index = FindHighlightedIndex();

            if (index < 0)
                return false;

            Action edit = _targets[index].Edit;

            if (edit == null)
                return false;

            edit.Invoke();

            return true;
        }

        private bool Delete()
        {
            int index = FindHighlightedIndex();

            if (index < 0)
                return false;

            Action delete = _targets[index].Delete;

            if (delete == null)
                return false;

            delete.Invoke();

            return true;
        }

        private bool NavigateLeft()
        {
            int index = FindHighlightedIndex();

            if (index < 0)
                return false;

            Action action = _targets[index].NavigateLeft;

            if (action == null)
                return false;

            action.Invoke();

            return true;
        }

        private bool NavigateRight()
        {
            int index = FindHighlightedIndex();

            if (index < 0)
                return false;

            Action action = _targets[index].NavigateRight;

            if (action == null)
                return false;

            action.Invoke();

            return true;
        }

        private void Move(int direction)
        {
            if (_targets.Count == 0)
                return;

            int index = FindHighlightedIndex();

            if (index < 0)
            {
                index = direction > 0
                    ? 0
                    : _targets.Count - 1;
            }
            else
            {
                index += direction;

                if (index < 0)
                {
                    index = _targets.Count - 1;
                }
                else if (index >= _targets.Count)
                {
                    index = 0;
                }
            }

            HighlightTarget(index);
        }

        private void MoveToBoundary(bool end)
        {
            if (_targets.Count == 0)
                return;

            int index = end
                ? _targets.Count - 1
                : 0;

            HighlightTarget(index);
        }

        private void MovePage(int direction)
        {
            if (_targets.Count == 0)
                return;

            int currentIndex = FindHighlightedIndex();

            if (currentIndex < 0)
            {
                HighlightTarget(direction > 0
                    ? 0
                    : _targets.Count - 1);

                return;
            }

            float pageHeight = _state.ViewportHeight;

            if (pageHeight <= 0f)
            {
                Move(direction);

                return;
            }

            float targetY =
                _targets[currentIndex].Rect.center.y
                + pageHeight * direction;

            int targetIndex = currentIndex;

            if (direction > 0)
            {
                for (int i = currentIndex + 1;
                     i < _targets.Count;
                     i++)
                {
                    targetIndex = i;

                    if (_targets[i].Rect.center.y >= targetY)
                        break;
                }
            }
            else
            {
                for (int i = currentIndex - 1;
                     i >= 0;
                     i--)
                {
                    targetIndex = i;

                    if (_targets[i].Rect.center.y <= targetY)
                        break;
                }
            }

            HighlightTarget(targetIndex);
        }

        private void HighlightTarget(int index)
        {
            if (index < 0 || index >= _targets.Count)
            {
                return;
            }

            BrowserNavigationTarget target = _targets[index];

            _highlightedId = target.Id;

            EnsureVisible(target.Rect);
        }
        #endregion

        #region Helpers
        private int FindHighlightedIndex()
        {
            if (string.IsNullOrEmpty(_highlightedId))
            {
                return -1;
            }

            for (int i = 0; i < _targets.Count; i++)
            {
                if (_targets[i].Id == _highlightedId)
                {
                    return i;
                }
            }

            return -1;
        }

        private void EnsureVisible(Rect rect)
        {
            if (_state.ViewportHeight <= 0f)
                return;

            float visibleTop = _state.Scroll.y;

            float visibleBottom = visibleTop
                + _state.ViewportHeight;

            if (rect.yMin < visibleTop)
            {
                _state.Scroll.y = rect.yMin;

                return;
            }

            if (rect.yMax > visibleBottom)
            {
                _state.Scroll.y = rect.yMax
                    - _state.ViewportHeight;
            }
        }
        #endregion
    }
}