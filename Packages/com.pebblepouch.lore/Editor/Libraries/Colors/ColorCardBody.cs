using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class ColorCardBody
    {
        #region Public API
        public void Draw(
            Rect rect,
            ColorLibraryItem item)
        {
            LibraryCardLayout layout =
                new(
                    rect,
                    false,
                    false);

            DrawSwatch(
                layout.Visual,
                item);

            DrawTitle(
                layout.Title,
                item);

            DrawCategory(
                layout.Metadata,
                item);
        }
        #endregion

        #region Draw
        private static void DrawSwatch(
            Rect rect,
            ColorLibraryItem item)
        {
            Rect swatchRect = new(
                rect.center.x
                    - LibraryStyle.SwatchSize * 0.5f,
                rect.center.y
                    - LibraryStyle.SwatchSize * 0.5f,
                LibraryStyle.SwatchSize,
                LibraryStyle.SwatchSize);

            EditorGUI.DrawRect(
                swatchRect,
                item.Color);
        }

        private static void DrawTitle(
            Rect rect,
            ColorLibraryItem item)
        {
            GUI.Label(
                rect,
                item.Title,
                LibraryStyle.Title);
        }

        private static void DrawCategory(
            Rect rect,
            ColorLibraryItem item)
        {
            GUI.Label(
                rect,
                item.Category,
                LibraryStyle.Metadata);
        }
        #endregion
    }
}