using System.Collections.Generic;
using System.Linq;

namespace PebblePouch.Lore.Editor
{
    public sealed class GlyphLibrarySource : LibrarySource<GlyphLibraryItem>
    {
        #region Fields
        // Asked on every refresh: the Library asset can be created or replaced while the browser is open.
        private readonly System.Func<GlyphLibraryCollection> _getCollection;
        #endregion

        #region Constructor
        public GlyphLibrarySource(System.Func<GlyphLibraryCollection> getCollection) => _getCollection = getCollection;
        #endregion

        #region Public API
        public override IReadOnlyList<GlyphLibraryItem> GetItems()
        {
            GlyphLibraryCollection collection =
                _getCollection?.Invoke();

            if (collection == null)
                return new List<GlyphLibraryItem>();

            return collection.Glyphs
                .Select(glyph => new GlyphLibraryItem(glyph))
                .ToList();
        }
        #endregion
    }
}