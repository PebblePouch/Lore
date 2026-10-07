using System;

namespace PebblePouch.Lore.Editor
{
    public static class ContentBrowserRequestService
    {
        #region Fields
        private static Category _category;
        private static Entry _entry;

        private static ContentBrowserFilterRequest _filter;
        #endregion

        #region Public API
        public static void RequestCategoryReveal(Category category)
        {
            _category = category;
        }

        public static void RequestEntryReveal(Entry entry)
        {
            _entry = entry;
        }

        public static Category ConsumeCategoryReveal()
        {
            Category category = _category;

            _category = null;

            return category;
        }

        public static Entry ConsumeEntryReveal()
        {
            Entry entry = _entry;

            _entry = null;

            return entry;
        }

        public static void RequestCategoryFilter(
            Func<Category, bool> predicate,
            string label)
        {
            _filter = new ContentBrowserFilterRequest(predicate, label);
        }

        public static void RequestEntryFilter(
            Func<Entry, bool> predicate,
            string label,
            Field matchingField = null)
        {
            _filter =
                new ContentBrowserFilterRequest(
                    predicate,
                    label,
                    matchingField);
        }

        public static void RequestCombinedFilter(
            Func<Category, bool> categoryPredicate,
            Func<Entry, bool> entryPredicate,
            string label)
        {
            _filter =
                new ContentBrowserFilterRequest(
                    categoryPredicate,
                    entryPredicate,
                    label);
        }

        public static ContentBrowserFilterRequest ConsumeFilter()
        {
            ContentBrowserFilterRequest filter = _filter;

            _filter = null;

            return filter;
        }
        #endregion
    }
}