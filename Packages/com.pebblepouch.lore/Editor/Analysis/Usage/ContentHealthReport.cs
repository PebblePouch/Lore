namespace PebblePouch.Lore.Editor
{
    // Counts of valid but unfinished content (the writer's to-do list), unlike integrity issues (broken data).
    public sealed class ContentHealthReport
    {
        #region Properties
        public int EntriesWithoutDescriptionCount { get; }
        public int CategoriesWithoutDescriptionCount { get; }
        public int EmptyCategoryCount { get; }
        public int PlaceholderNameCount { get; }
        public int EntriesWithoutTagsCount { get; }
        public int EntriesMissingRequiredCount { get; }

        // "Entries without Tags" is only shown when the project uses Tags at all.
        public bool HasTags { get; }

        // "Entries missing required values" is only shown when some Category has Required Fields.
        public bool HasRequiredFields { get; }
        #endregion

        #region Constructor
        public ContentHealthReport(
            int entriesWithoutDescriptionCount,
            int categoriesWithoutDescriptionCount,
            int emptyCategoryCount,
            int placeholderNameCount,
            int entriesWithoutTagsCount,
            bool hasTags,
            int entriesMissingRequiredCount,
            bool hasRequiredFields)
        {
            EntriesWithoutDescriptionCount = entriesWithoutDescriptionCount;
            CategoriesWithoutDescriptionCount = categoriesWithoutDescriptionCount;
            EmptyCategoryCount = emptyCategoryCount;
            PlaceholderNameCount = placeholderNameCount;
            EntriesWithoutTagsCount = entriesWithoutTagsCount;
            HasTags = hasTags;
            EntriesMissingRequiredCount = entriesMissingRequiredCount;
            HasRequiredFields = hasRequiredFields;
        }
        #endregion
    }
}
