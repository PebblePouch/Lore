using System;
using System.Collections.Generic;
using System.Linq;

namespace PebblePouch.Lore.Editor
{
    public sealed class ContentBrowserModel : ISearchModel
    {
        #region Fields
        private readonly LoreRepository _index;

        private readonly EntrySearchMatcher _matcher;

        private string _search = string.Empty;

        private Func<Category, bool> _categoryFilter;
        private Func<Entry, bool> _entryFilter;

        // Performance: the filtered groups are cached until search, filter or LoreDataVersion change (filters such
        // as integrity checks are expensive and GetGroups runs on every OnGUI call).
        private IReadOnlyList<CategoryEntryGroup> _filteredGroups;
        private string _filteredSearch;
        private int _filteredVersion;
        #endregion

        #region Properties
        public string Search
        {
            get => _search;
            set => _search = value ?? string.Empty;
        }

        public bool HasFilter => HasEntryFilter || HasCategoryFilter;
        public bool HasCategoryFilter => _categoryFilter != null;
        public bool HasEntryFilter => _entryFilter != null;
        #endregion

        #region Constructor
        public ContentBrowserModel(
            LoreRepository index)
        {
            _index =
                index;

            _matcher =
                new EntrySearchMatcher();
        }
        #endregion

        #region Public API
        public IReadOnlyList<CategoryEntryGroup> GetGroups()
        {
            bool hasSearch = !string.IsNullOrWhiteSpace(_search);

            if (!hasSearch &&
                !HasFilter)
            {
                return _index.Groups;
            }

            if (_filteredGroups == null ||
                _filteredVersion != LoreDataVersion.Current ||
                !string.Equals(_filteredSearch, _search, StringComparison.Ordinal))
            {
                _filteredGroups = FilterGroups();
                _filteredSearch = _search;
                _filteredVersion = LoreDataVersion.Current;
            }

            return _filteredGroups;
        }

        private IReadOnlyList<CategoryEntryGroup> FilterGroups()
        {
            var groups = _index.Groups;

            bool hasSearch = !string.IsNullOrWhiteSpace(_search);

            bool hasEntryFilter = _entryFilter != null;
            bool hasCategoryFilter = _categoryFilter != null;

            string search =
                _search;

            var filteredGroups =
                new List<CategoryEntryGroup>();

            // Both filters set (combined filter): a group is shown when its Category matches
            // OR any of its Entries match; only matching Entries are listed.
            bool isCombinedFilter =
                hasCategoryFilter &&
                hasEntryFilter;

            foreach (var group in groups)
            {
                bool categoryFilterMatches =
                    !hasCategoryFilter ||
                    _categoryFilter(group.Category);

                if (!isCombinedFilter &&
                    !categoryFilterMatches)
                {
                    continue;
                }

                IEnumerable<Entry> entries = group.Entries;

                if (hasEntryFilter)
                {
                    entries = entries.Where(_entryFilter);
                }

                List<Entry> filteredEntries = entries.ToList();

                bool keepWithoutEntries =
                    isCombinedFilter &&
                    categoryFilterMatches;

                if (hasEntryFilter &&
                    filteredEntries.Count == 0 &&
                    !keepWithoutEntries)
                {
                    continue;
                }

                if (!hasSearch)
                {
                    filteredGroups.Add(
                        new CategoryEntryGroup(
                            group.Category,
                            filteredEntries));

                    continue;
                }

                bool categoryMatches =
                    MatchesText(
                        group.Title,
                        search) ||
                    MatchesText(
                        group.Glyph,
                        search);

                if (categoryMatches)
                {
                    filteredGroups.Add(
                        hasEntryFilter
                            ? new CategoryEntryGroup(
                                group.Category,
                                filteredEntries)
                            : group);

                    continue;
                }

                List<Entry> matchingEntries =
                    filteredEntries
                        .Where(
                            entry =>
                                Matches(
                                    entry,
                                    search))
                        .ToList();

                if (matchingEntries.Count == 0)
                    continue;

                filteredGroups.Add(
                    new CategoryEntryGroup(
                        group.Category,
                        matchingEntries));
            }

            return filteredGroups;
        }

        public void SetCategoryFilter(
            Func<Category, bool> predicate)
        {
            _entryFilter = null;
            _categoryFilter = predicate;

            _filteredGroups = null;
        }

        public void SetEntryFilter(
            Func<Entry, bool> predicate)
        {
            _categoryFilter = null;
            _entryFilter = predicate;

            _filteredGroups = null;
        }

        // Shows Categories matching categoryPredicate and Entries matching entryPredicate (see GetGroups).
        public void SetCombinedFilter(
            Func<Category, bool> categoryPredicate,
            Func<Entry, bool> entryPredicate)
        {
            _categoryFilter = categoryPredicate;
            _entryFilter = entryPredicate;

            _filteredGroups = null;
        }

        public void ClearFilter()
        {
            _categoryFilter = null;
            _entryFilter = null;

            _filteredGroups = null;
        }

        public void Refresh()
        {
            // Intentionally empty.
            // ContentBrowser queries LoreRepository on demand.
        }
        #endregion

        #region Helper
        // Title, subtitle, glyph and tags first (cheap), then full-text (description and field values).
        private bool Matches(
            Entry entry,
            string search)
        {
            return _matcher.Matches(entry, search) ||
                   _matcher.MatchesContent(entry, search);
        }

        private static bool MatchesText(string value, string search)
        {
            return EntrySearchMatcher.MatchesText(value, search);
        }
        #endregion
    }
}