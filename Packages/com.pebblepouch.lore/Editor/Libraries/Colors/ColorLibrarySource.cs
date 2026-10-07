using System.Collections.Generic;
using System.Linq;

namespace PebblePouch.Lore.Editor
{
    public sealed class ColorLibrarySource : LibrarySource<ColorLibraryItem>
    {
        #region Fields
        // Asked on every refresh: the Library asset can be created or replaced while the browser is open.
        private readonly System.Func<ColorLibraryCollection> _getCollection;
        #endregion

        #region Constructor
        public ColorLibrarySource(System.Func<ColorLibraryCollection> getCollection) => _getCollection = getCollection;
        #endregion

        #region Public API
        public override IReadOnlyList<ColorLibraryItem> GetItems()
        {
            ColorLibraryCollection collection =
                _getCollection?.Invoke();

            if (collection == null)
                return new List<ColorLibraryItem>();

            return collection.Colors
                .Select(color => new ColorLibraryItem(color))
                .ToList();
        }
        #endregion
    }
}