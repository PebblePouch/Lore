namespace PebblePouch.Lore.Editor
{
    public sealed class FieldDeleteReport
    {
        #region Properties
        public Field Field { get; }

        public int AffectedCategoryCount { get; }

        public int AffectedEntryCount { get; }

        public int PopulatedEntryCount { get; }

        public bool IsInUse =>
            AffectedCategoryCount > 0 ||
            AffectedEntryCount > 0;

        public bool HasDataLoss =>
            PopulatedEntryCount > 0;
        #endregion

        #region Constructor
        public FieldDeleteReport(
            Field field,
            int affectedCategoryCount,
            int affectedEntryCount,
            int populatedEntryCount)
        {
            Field =
                field;

            AffectedCategoryCount =
                affectedCategoryCount;

            AffectedEntryCount =
                affectedEntryCount;

            PopulatedEntryCount =
                populatedEntryCount;
        }
        #endregion
    }
}