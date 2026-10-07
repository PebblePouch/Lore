using System.Collections.Generic;
using System.Linq;

namespace PebblePouch.Lore.Editor
{
    public sealed class TagLibrarySource : LibrarySource<TagLibraryItem>
    {
        #region Fields
        // Asked on every refresh: the Library asset can be created or replaced while the browser is open.
        private readonly System.Func<TagCollection> _getCollection;
        #endregion

        #region Constructor
        public TagLibrarySource(System.Func<TagCollection> getCollection) => _getCollection = getCollection;
        #endregion

        #region Public API
        public override IReadOnlyList<TagLibraryItem> GetItems()
        {
            TagCollection collection =
                _getCollection?.Invoke();

            if (collection == null)
                return new List<TagLibraryItem>();

            Entry entry = SelectionService.CurrentEntry;

            return collection.Tags
                .Where(tag => tag != null)
                .OrderBy(tag => tag.Title)
                .Select(tag =>
                    new TagLibraryItem(
                        tag,
                        entry != null &&
                        entry.HasTag(tag.Id)))
                .ToList();
        }
        #endregion
    }
}