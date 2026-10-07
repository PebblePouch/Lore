using System.Collections.Generic;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static class DragInsertion
    {
        public static int GetInsertionIndex<T>(
            IList<DragHandle<T>> handles,
            Vector2 mousePosition)
        {
            for (int i = 0; i < handles.Count; i++)
            {
                Rect card = handles[i].CardRect;

                float middle =
                    card.y + card.height * 0.5f;

                if (mousePosition.y < middle)
                    return i;
            }

            return handles.Count;
        }
    }
}