using System;

namespace PebblePouch.Lore.Editor
{
    public sealed class ContentBrowserFilterRequest
    {
        #region Properties
        public Func<Category, bool> CategoryPredicate { get; }
        public Func<Entry, bool> EntryPredicate { get; }
        public Field MatchingField { get; }

        public string Label { get; }
        #endregion

        #region Constructors
        public ContentBrowserFilterRequest(
            Func<Category, bool> categoryPredicate,
            string label)
        {
            CategoryPredicate = categoryPredicate;
            Label = label ?? string.Empty;
        }

        public ContentBrowserFilterRequest(
            Func<Entry, bool> entryPredicate,
            string label,
            Field matchingField = null)
        {
            EntryPredicate = entryPredicate;
            Label = label ?? string.Empty;

            MatchingField = matchingField;
        }

        // Combined filter: matching Categories and matching Entries (see ContentBrowserModel.GetGroups).
        public ContentBrowserFilterRequest(
            Func<Category, bool> categoryPredicate,
            Func<Entry, bool> entryPredicate,
            string label)
        {
            CategoryPredicate = categoryPredicate;
            EntryPredicate = entryPredicate;
            Label = label ?? string.Empty;
        }
        #endregion
    }
}