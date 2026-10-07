using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;

namespace PebblePouch.Lore.Editor
{
    public sealed class LibraryModel<T> : ISearchModel, IDisposable where T : LibraryItem
    {
        #region Fields
        private readonly LibrarySource<T> _source;

        private string _search = string.Empty;
        private Func<T, bool> _filter;

        private IReadOnlyList<T> _items = Array.Empty<T>();
        #endregion

        #region Properties
        public string Search
        {
            get => _search;
            set => _search = value ?? string.Empty;
        }
        public bool IsEmpty => _items.Count == 0;
        #endregion

        #region Constructor
        public LibraryModel(LibrarySource<T> source)
        {
            _source = source;

            // Refreshed covers Lore edits and external Lore asset changes (no projectChanged: every import fires it).
            LoreRefreshService.Refreshed += Refresh;

            Refresh();
        }
        #endregion

        #region Public API
        public IReadOnlyList<T> GetItems()
        {
            IEnumerable<T> items = _items;

            if (_filter != null)
            {
                items = items.Where(_filter);
            }

            if (!string.IsNullOrWhiteSpace(Search))
            {
                string search = Search.Trim();

                items =
                    items.Where(
                        item =>
                            !string.IsNullOrEmpty(
                                item.SearchText) &&
                            item.SearchText.IndexOf(
                                search,
                                StringComparison.OrdinalIgnoreCase) >= 0);
            }

            return items.ToList();
        }

        public void SetFilter(Func<T, bool> filter)
        {
            _filter = filter;
        }

        public void ClearFilter()
        {
            _filter = null;
        }

        public void Refresh() => _items = _source.GetItems() ?? Array.Empty<T>();
        public void Dispose()
        {
            LoreRefreshService.Refreshed -= Refresh;
        }
        #endregion
    }
}