using System.Collections.Generic;
using System.Linq;

namespace PebblePouch.Lore.Editor
{
    public sealed class CategoryFieldUsageAnalyzer
    {
        #region Fields
        private readonly LoreRepository _repository;
        #endregion

        #region Constructor
        public CategoryFieldUsageAnalyzer(
            LoreRepository repository)
        {
            _repository =
                repository;
        }
        #endregion

        #region Public API
        public IReadOnlyList<Entry> GetEntriesWithValues(
            Category category,
            Field field)
        {
            if (category == null ||
                field == null)
            {
                return new List<Entry>();
            }

            return _repository
                .Entries
                .Where(
                    entry =>
                        entry != null &&
                        entry.Category == category &&
                        HasValue(
                            entry,
                            field))
                .OrderBy(
                    entry =>
                        entry.SortOrder)
                .ToList();
        }
        #endregion

        #region Helpers
        private static bool HasValue(
            Entry entry,
            Field field)
        {
            FieldValue fieldValue =
                entry.Fields
                    .FirstOrDefault(
                        value =>
                            value?.Field ==
                            field);

            return FieldValueUtility.IsPopulated(
                fieldValue,
                field.Type);
        }
        #endregion
    }
}