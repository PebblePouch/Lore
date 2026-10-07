using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public readonly struct DragHandle<T>
    {
        public readonly T Item;
        public readonly Rect HandleRect;
        public readonly Rect CardRect;

        public DragHandle(
            T item,
            Rect handleRect,
            Rect cardRect)
        {
            Item = item;
            HandleRect = handleRect;
            CardRect = cardRect;
        }
    }
}