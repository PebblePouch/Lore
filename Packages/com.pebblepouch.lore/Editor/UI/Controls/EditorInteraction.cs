using System;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static class EditorInteraction
    {
        public static void Click(Rect rect, Action action)
        {
            if (!Click(rect))
                return;

            action?.Invoke();
        }

        public static bool Click(Rect rect)
        {
            Event e = Event.current;

            if (e.type != EventType.MouseDown)
                return false;

            if (!rect.Contains(e.mousePosition))
                return false;

            e.Use();

            return true;
        }
    }
}