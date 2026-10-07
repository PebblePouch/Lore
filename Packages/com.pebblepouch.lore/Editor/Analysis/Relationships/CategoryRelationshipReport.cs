using System.Collections.Generic;

namespace PebblePouch.Lore.Editor
{
    public sealed class CategoryRelationshipReport
    {
        #region Properties
        public Category Category { get; }

        public IReadOnlyList<Entry> Entries { get; }
        public IReadOnlyList<Entry> ReferencedEntries { get; }
        public IReadOnlyList<Entry> UnreferencedEntries { get; }
        public IReadOnlyList<Entry> IsolatedEntries { get; }

        public int EntryCount => Entries.Count;
        public int ReferencedEntryCount => ReferencedEntries.Count;
        public int UnreferencedEntryCount => UnreferencedEntries.Count;
        public int IsolatedEntryCount => IsolatedEntries.Count;

        public int IncomingReferenceCount { get; }
        public int OutgoingReferenceCount { get; }
        #endregion

        #region Constructor
        public CategoryRelationshipReport(
            Category category,
            IReadOnlyList<Entry> entries,
            IReadOnlyList<Entry> referencedEntries,
            IReadOnlyList<Entry> unreferencedEntries,
            IReadOnlyList<Entry> isolatedEntries,
            int incomingReferenceCount,
            int outgoingReferenceCount)
        {
            Category = category;
            Entries = entries;
            ReferencedEntries = referencedEntries;
            UnreferencedEntries = unreferencedEntries;
            IsolatedEntries = isolatedEntries;
            IncomingReferenceCount = incomingReferenceCount;
            OutgoingReferenceCount = outgoingReferenceCount;
        }
        #endregion
    }
}