using System.Collections.Generic;

namespace PebblePouch.Lore.Editor
{
    public sealed class ProjectRelationshipReport
    {
        #region Properties
        public int CategoryCount { get; }

        public IReadOnlyList<Category> StandaloneCategories { get; }
        public IReadOnlyList<Category> IsolatedCategories { get; }

        public int StandaloneCategoryCount => StandaloneCategories.Count;
        public int IsolatedCategoryCount => IsolatedCategories.Count;

        public IReadOnlyList<Entry> Entries { get; }
        public IReadOnlyList<Entry> ReferencedEntries { get; }
        public IReadOnlyList<Entry> UnreferencedEntries { get; }
        public IReadOnlyList<Entry> IsolatedEntries { get; }

        public int EntryCount => Entries.Count;
        public int ReferencedEntryCount => ReferencedEntries.Count;
        public int UnreferencedEntryCount => UnreferencedEntries.Count;
        public int IsolatedEntryCount => IsolatedEntries.Count;

        public int ReferenceCount { get; }
        #endregion

        #region Constructor
        public ProjectRelationshipReport(
            int categoryCount,
            IReadOnlyList<Category> standaloneCategories,
            IReadOnlyList<Category> isolatedCategories,
            IReadOnlyList<Entry> entries,
            IReadOnlyList<Entry> referencedEntries,
            IReadOnlyList<Entry> unreferencedEntries,
            IReadOnlyList<Entry> isolatedEntries,
            int referenceCount)
        {
            CategoryCount = categoryCount;
            StandaloneCategories = standaloneCategories;
            IsolatedCategories = isolatedCategories;

            Entries = entries;
            ReferencedEntries = referencedEntries;
            UnreferencedEntries = unreferencedEntries;
            IsolatedEntries = isolatedEntries;

            ReferenceCount = referenceCount;
        }
        #endregion
    }
}