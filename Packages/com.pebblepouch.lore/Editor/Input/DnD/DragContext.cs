using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class DragContext<T> where T : class
    {
        public bool IsDragging;

        public T DraggedItem;

        public int InsertionIndex = -1;

        public void Reset()
        {
            IsDragging = false;
            DraggedItem = null;
            InsertionIndex = -1;

            GUIUtility.hotControl = 0;
        }
    }
}