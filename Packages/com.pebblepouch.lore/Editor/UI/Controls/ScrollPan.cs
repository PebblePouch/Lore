using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Dragging with the middle mouse button (wheel held down) moves a scroll view's content (Content browser,
    // Libraries, Tabular Editing). Get the control ID before the scrolled content draws its controls, so the ID
    // stays the same while the content changes during the drag.
    public static class ScrollPan
    {
        #region Fields
        private static readonly int ControlHash = "LoreScrollPan".GetHashCode();
        #endregion

        #region Public API
        public static int GetControlId() =>
            GUIUtility.GetControlID(
                ControlHash,
                FocusType.Passive);

        // Returns the scroll position moved by the drag (never negative; callers or the scroll view clamp the
        // upper end).
        public static Vector2 Handle(
            int controlId,
            Rect area,
            Vector2 scroll)
        {
            Event current = Event.current;

            switch (current.GetTypeForControl(controlId))
            {
                case EventType.MouseDown:
                    if (current.button == 2 &&
                        area.Contains(current.mousePosition))
                    {
                        GUIUtility.hotControl = controlId;

                        current.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlId)
                    {
                        scroll =
                            new Vector2(
                                Mathf.Max(0f, scroll.x - current.delta.x),
                                Mathf.Max(0f, scroll.y - current.delta.y));

                        current.Use();

                        LoreRepaintService.Repaint();
                    }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlId &&
                        current.button == 2)
                    {
                        GUIUtility.hotControl = 0;

                        current.Use();
                    }
                    break;
            }

            if (GUIUtility.hotControl == controlId)
            {
                EditorGUIUtility.AddCursorRect(
                    area,
                    MouseCursor.Pan);
            }

            return scroll;
        }
        #endregion
    }
}
