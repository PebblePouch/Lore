using System.Collections.Generic;

namespace PebblePouch.Lore.Editor
{
    public abstract class LibrarySource<T> where T : LibraryItem
    {
        #region Public API
        public abstract IReadOnlyList<T> GetItems();
        #endregion
    }
}