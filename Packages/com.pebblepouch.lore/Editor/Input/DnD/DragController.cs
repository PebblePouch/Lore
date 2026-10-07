using System.Collections.Generic;

using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static class DragController
    {
        public static bool TryBeginDrag<T>(
            IList<DragHandle<T>> handles,
            Vector2 mousePosition,
            DragContext<T> dragContext,
            out DragHandle<T> handle)
            where T : class
        {
            foreach (var current in handles)
            {
                if (!current.HandleRect.Contains(mousePosition))
                    continue;

                dragContext.DraggedItem = current.Item;
                dragContext.IsDragging = true;

                handle = current;

                return true;
            }

            handle = default;

            return false;
        }
    }
}