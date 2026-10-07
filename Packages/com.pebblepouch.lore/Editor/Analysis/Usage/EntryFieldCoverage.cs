namespace PebblePouch.Lore.Editor
{
    public sealed class EntryFieldCoverage
    {
        #region Properties
        public Category Category { get; }
        public Field Field { get; }

        public int EntryCount { get; }
        public int PopulatedCount { get; }

        public int UnpopulatedCount =>
            EntryCount - PopulatedCount;
        #endregion

        #region Constructor
        public EntryFieldCoverage(
            Category category,
            Field field,
            int entryCount,
            int populatedCount)
        {
            Category =
                category;

            Field =
                field;

            EntryCount =
                entryCount;

            PopulatedCount =
                populatedCount;
        }
        #endregion
    }
}