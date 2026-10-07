using System.Linq;
using System.Collections.Generic;

namespace PebblePouch.Lore.Editor
{
    public sealed class EntryLibrarySource : LibrarySource<EntryLibraryItem>
    {
        #region Public API
        public override IReadOnlyList<EntryLibraryItem> GetItems()
        {
            return LoreRepository.Instance
                .Entries
                .OrderBy(entry => entry.Title)
                .Select(entry => new EntryLibraryItem(entry))
                .ToList();
        }
        #endregion
    }
}