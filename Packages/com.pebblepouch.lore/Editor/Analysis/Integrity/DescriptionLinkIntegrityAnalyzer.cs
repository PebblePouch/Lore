namespace PebblePouch.Lore.Editor
{
    // Counts [[links]] in descriptions whose Entry cannot be resolved (shown red with strikethrough
    // in view mode). Unpinned links count as broken while no Entry with that title exists.
    public sealed class DescriptionLinkIntegrityAnalyzer
    {
        #region Public API
        public int Count(string description)
        {
            if (!DescriptionLinkParser.ContainsLinks(description))
                return 0;

            int count = 0;

            foreach (DescriptionSegment segment in
                     DescriptionLinkParser.Parse(
                         description))
            {
                if (segment.IsLink &&
                    DescriptionLinkResolver.Resolve(
                        segment.Text) == null)
                {
                    count++;
                }
            }

            return count;
        }

        public bool HasBrokenLinks(Category category)
        {
            return category != null &&
                   Count(category.Description) > 0;
        }

        public bool HasBrokenLinks(Entry entry)
        {
            return entry != null &&
                   Count(entry.Description) > 0;
        }

        // Project, all Categories and all Entries.
        public int CountProject()
        {
            LoreRepository repository = LoreRepository.Instance;

            int count =
                repository.Project != null
                    ? Count(repository.Project.Description)
                    : 0;

            foreach (Category category in repository.Categories)
            {
                if (category != null)
                    count += Count(category.Description);
            }

            foreach (Entry entry in repository.Entries)
            {
                if (entry != null)
                    count += Count(entry.Description);
            }

            return count;
        }
        #endregion
    }
}
