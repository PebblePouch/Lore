using System;
using System.Collections.Generic;

using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class DragListController<T>
        where T : class
    {
        private readonly DragContext<T> _drag = new();

        public DragContext<T> Drag => _drag;

        public bool IsDragging => _drag.IsDragging;

        public void BeginDrag(
            IList<DragHandle<T>> handles,
            Vector2 mousePosition,
            Action<DragHandle<T>> onStarted)
        {
            if (!DragController.TryBeginDrag(
                    handles,
                    mousePosition,
                    _drag,
                    out var handle))
            {
                return;
            }

            onStarted?.Invoke(handle);

            GUIUtility.hotControl =
                GUIUtility.GetControlID(FocusType.Passive);

            Event.current.Use();
        }

        public void UpdateInsertionIndex(
            int insertionIndex)
        {
            if (!_drag.IsDragging)
                return;

            _drag.InsertionIndex =
                insertionIndex;

            Event.current.Use();
        }

        public void EndDrag(
            Action<T, int> onDrop)
        {
            if (!_drag.IsDragging)
                return;

            if (_drag.DraggedItem != null)
            {
                onDrop?.Invoke(
                    _drag.DraggedItem,
                    _drag.InsertionIndex);
            }

            Reset();

            Event.current.Use();
        }

        public void Reset()
        {
            _drag.Reset();
        }
    }
}