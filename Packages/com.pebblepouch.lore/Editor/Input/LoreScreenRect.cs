using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Compares positions across GUI coordinate spaces. Rects drawn inside a scroll view have different
    // GUI coordinates than mouse positions read outside it (e.g. outside-click checks in panels), so
    // both are converted to screen coordinates, which are the same everywhere.
    public static class LoreScreenRect
    {
        #region Public API
        // Converts a rect from the current GUI coordinates; keeps previous while Layout returns placeholders.
        public static Rect FromGUI(
            Rect guiRect,
            Rect previous)
        {
            if (Event.current != null &&
                Event.current.type == EventType.Layout)
            {
                return previous;
            }

            return GUIUtility.GUIToScreenRect(guiRect);
        }

        // True if a point in the current GUI coordinates lies inside a screen rect.
        public static bool Contains(
            Rect screenRect,
            Vector2 guiPoint)
        {
            return screenRect.Contains(
                GUIUtility.GUIToScreenPoint(
                    guiPoint));
        }
        #endregion
    }
}
