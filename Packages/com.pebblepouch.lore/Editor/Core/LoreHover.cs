using System.Collections.Generic;

using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Hover checks for hover visuals (card highlights). Instead of repainting the whole window on every mouse move,
    // the window repaints only when the mouse enters or leaves one of the areas recorded during the last Repaint.
    // Use Contains for hover visuals only; clicks keep using rect.Contains(Event.current.mousePosition).
    public static class LoreHover
    {
        #region Fields
        // Screen rects (stable across scroll views and clips) of the current and the last finished Repaint.
        private static readonly List<Rect> RecordingRegions = new();
        private static readonly List<Rect> Regions = new();

        private static int _hoverSignature;
        #endregion

        #region Public API
        public static bool Contains(Rect rect)
        {
            if (Event.current.type == EventType.Repaint)
            {
                RecordingRegions.Add(
                    GUIUtility.GUIToScreenRect(
                        rect));
            }

            return rect.Contains(
                Event.current.mousePosition);
        }

        // Window, start of OnGUI.
        public static void BeginGUI()
        {
            if (Event.current.type == EventType.Repaint)
                RecordingRegions.Clear();
        }

        // Window, end of OnGUI.
        public static void EndGUI()
        {
            if (Event.current.type != EventType.Repaint)
                return;

            Regions.Clear();
            Regions.AddRange(RecordingRegions);

            _hoverSignature =
                GetSignature(
                    GetScreenMouse());
        }

        // True when the mouse entered or left a hover area since the last Repaint.
        public static bool HoverChanged()
        {
            return GetSignature(GetScreenMouse()) != _hoverSignature;
        }

        // Mouse left the window: nothing may stay highlighted.
        public static bool HasHover => _hoverSignature != 0;
        #endregion

        #region Helpers
        private static Vector2 GetScreenMouse()
        {
            return GUIUtility.GUIToScreenPoint(
                Event.current.mousePosition);
        }

        // Which areas contain the point (0 = none).
        private static int GetSignature(Vector2 screenPoint)
        {
            int signature = 0;

            for (int i = 0; i < Regions.Count; i++)
            {
                if (Regions[i].Contains(screenPoint))
                    signature = unchecked(signature * 31 + i + 1);
            }

            return signature;
        }
        #endregion
    }
}
