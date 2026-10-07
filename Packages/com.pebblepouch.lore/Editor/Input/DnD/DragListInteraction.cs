using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class DragListInteraction<T>
        where T : class
    {
        #region Fields
        private readonly Action _repaint;
        private readonly Func<T, T, bool> _sameGroup;

        private readonly List<DragHandle<T>> _handles = new();
        private readonly DragListController<T> _controller = new();
        #endregion

        #region Properties
        public bool IsDragging => _controller.IsDragging;
        #endregion

        #region Constructor
        public DragListInteraction(
            Action repaint,
            Func<T, T, bool> sameGroup = null)
        {
            _repaint = repaint;
            _sameGroup = sameGroup;
        }
        #endregion

        #region Public API
        public void BeginFrame()
        {
            _handles.Clear();
        }

        public void Register(
            T item,
            Rect handleRect,
            Rect cardRect)
        {
            if (item == null)
                return;

            _handles.Add(
                new DragHandle<T>(
                    item,
                    handleRect,
                    cardRect));
        }

        public void HandleEvents(
            Action<T, int> onDrop)
        {
            switch (Event.current.type)
            {
                case EventType.MouseDown:
                    HandleMouseDown();
                    break;

                case EventType.MouseDrag:
                    HandleMouseDrag();
                    break;

                case EventType.MouseUp:
                    HandleMouseUp(
                        onDrop);
                    break;
            }
        }

        public void DrawDropLine(
            Action<Rect, Rect, Rect, bool, bool> draw)
        {
            if (!_controller.IsDragging)
                return;

            List<DragHandle<T>> handles =
                GetActiveHandles();

            if (handles.Count == 0)
                return;

            Rect first =
                handles[0].CardRect;

            Rect last =
                handles[^1].CardRect;

            int insertionIndex =
                _controller.Drag.InsertionIndex;

            int targetIndex =
                Mathf.Clamp(
                    insertionIndex,
                    0,
                    handles.Count - 1);

            Rect target =
                insertionIndex >= handles.Count
                    ? last
                    : handles[targetIndex].CardRect;

            draw?.Invoke(
                first,
                last,
                target,
                insertionIndex <= 0,
                insertionIndex >= handles.Count);
        }
        #endregion

        #region Event Handlers
        private void HandleMouseDown()
        {
            _controller.BeginDrag(
                _handles,
                Event.current.mousePosition,
                handle =>
                {
                    List<DragHandle<T>> handles =
                        GetHandlesFor(
                            handle.Item);

                    _controller.Drag.InsertionIndex =
                        DragInsertion.GetInsertionIndex(
                            handles,
                            Event.current.mousePosition);
                });
        }

        private void HandleMouseDrag()
        {
            if (!_controller.IsDragging)
                return;

            List<DragHandle<T>> handles =
                GetActiveHandles();

            _controller.UpdateInsertionIndex(
                DragInsertion.GetInsertionIndex(
                    handles,
                    Event.current.mousePosition));

            _repaint?.Invoke();
        }

        private void HandleMouseUp(
            Action<T, int> onDrop)
        {
            if (!_controller.IsDragging)
                return;

            _controller.EndDrag(
                onDrop);

            _repaint?.Invoke();
        }
        #endregion

        #region Helpers
        private List<DragHandle<T>> GetActiveHandles()
        {
            return GetHandlesFor(
                _controller.Drag.DraggedItem);
        }

        private List<DragHandle<T>> GetHandlesFor(
            T item)
        {
            if (_sameGroup == null ||
                item == null)
            {
                return _handles.ToList();
            }

            return _handles
                .Where(handle =>
                    _sameGroup(
                        handle.Item,
                        item))
                .ToList();
        }
        #endregion
    }
}