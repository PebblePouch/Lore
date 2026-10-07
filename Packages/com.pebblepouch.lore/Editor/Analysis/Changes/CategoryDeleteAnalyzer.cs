using System.Linq;

namespace PebblePouch.Lore.Editor
{
    public sealed class CategoryDeleteAnalyzer
    {
        #region Public API
        public CategoryDeleteReport Analyze(
            Category category)
        {
            if (category == null)
            {
                return new CategoryDeleteReport(
                    null,
                    0);
            }

            int entryCount =
                LoreRepository.Instance
                    .Entries
                    .Count(
                        entry =>
                            entry.Category ==
                            category);

            return new CategoryDeleteReport(
                category,
                entryCount);
        }
        #endregion
    }
}