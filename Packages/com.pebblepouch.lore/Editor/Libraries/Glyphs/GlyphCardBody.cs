using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class GlyphCardBody
    {
        #region Public API
        public void Draw(
            Rect rect,
            GlyphLibraryItem item)
        {
            LibraryCardLayout layout =
                new(
                    rect,
                    false,
                    false);

            DrawGlyph(
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
        private static void DrawGlyph(
            Rect rect,
            GlyphLibraryItem item)
        {
            GUI.Label(
                rect,
                item.Glyph,
                LibraryStyle.Glyph);
        }

        private static void DrawTitle(
            Rect rect,
            GlyphLibraryItem item)
        {
            GUI.Label(
                rect,
                item.Title,
                LibraryStyle.Title);
        }

        private static void DrawCategory(
            Rect rect,
            GlyphLibraryItem item)
        {
            GUI.Label(
                rect,
                item.Category,
                LibraryStyle.Metadata);
        }
        #endregion
    }
}