using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public readonly struct LibraryCardLayout
    {
        #region Properties
        public Rect Visual { get; }
        public Rect Title { get; }
        public Rect Metadata { get; }
        public Rect Button { get; }
        public Rect Menu { get; }
        #endregion

        #region Constructor
        // metadataRatio > 0: the metadata gets this share of the title + metadata width instead of MetadataWidth.
        public LibraryCardLayout(
            Rect rect,
            bool hasButton,
            bool hasMenu,
            float metadataRatio = 0f)
        {
            Visual = new Rect(
                rect.x + LibraryStyle.ColumnMargin,
                rect.y,
                LibraryStyle.GlyphWidth,
                rect.height);

            float titleX =
                Visual.xMax
                + LibraryStyle.ColumnSpacing;

            float right =
                rect.xMax
                - (hasMenu
                    ? 0f
                    : LibraryStyle.ColumnSpacing); // Intentionally not ColumnMargin, gives metadata more space to breathe

            Menu = hasMenu
                ? new Rect(
                    right - LibraryStyle.MenuWidth,
                    rect.y,
                    LibraryStyle.MenuWidth,
                    rect.height)
                : default;

            if (hasMenu)
            {
                right -=
                    LibraryStyle.MenuWidth
                    + LibraryStyle.ColumnSpacing;
            }

            Button = hasButton
                ? new Rect(
                    right - LibraryStyle.ButtonWidth,
                    rect.y,
                    LibraryStyle.ButtonWidth,
                    rect.height)
                : default;

            if (hasButton)
            {
                right -=
                    LibraryStyle.ButtonWidth
                    + LibraryStyle.ColumnSpacing;
            }

            float metadataWidth =
                metadataRatio > 0f
                    ? Mathf.Floor(
                        Mathf.Max(
                            0f,
                            right - titleX - LibraryStyle.ColumnSpacing)
                        * metadataRatio)
                    : LibraryStyle.MetadataWidth;

            Metadata = new Rect(
                right - metadataWidth,
                rect.y,
                metadataWidth,
                rect.height);

            right -=
                metadataWidth
                + LibraryStyle.ColumnSpacing;

            Title = new Rect(
                titleX,
                rect.y,
                Mathf.Max(0f, right - titleX),
                rect.height);
        }
        #endregion
    }
}