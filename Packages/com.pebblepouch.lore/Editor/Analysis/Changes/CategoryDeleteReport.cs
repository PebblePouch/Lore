namespace PebblePouch.Lore.Editor
{
    public sealed class CategoryDeleteReport
    {
        #region Properties
        public Category Category { get; }

        public int EntryCount { get; }

        public bool CanDelete =>
            EntryCount == 0;
        #endregion

        #region Constructor
        public CategoryDeleteReport(
            Category category,
            int entryCount)
        {
            Category =
                category;

            EntryCount =
                entryCount;
        }
        #endregion
    }
}