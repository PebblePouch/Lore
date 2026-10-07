using System.Linq;

namespace PebblePouch.Lore.Editor
{
    public sealed class FieldDeleteAnalyzer
    {
        #region Public API
        public FieldDeleteReport Analyze(
            Field field)
        {
            if (field == null)
            {
                return new FieldDeleteReport(
                    null,
                    0,
                    0,
                    0);
            }

            int affectedCategoryCount =
                LoreRepository.Instance
                    .Categories
                    .Count(
                        category =>
                            category.Fields.Contains(
                                field));

            var affectedEntries =
                LoreRepository.Instance
                    .Entries
                    .Where(
                        entry =>
                            entry.Fields.Any(
                                fieldValue =>
                                    fieldValue.Field ==
                                    field))
                    .ToList();

            int populatedEntryCount =
                affectedEntries.Count(
                    entry =>
                        entry.Fields.Any(
                            fieldValue =>
                                fieldValue.Field == field &&
                                FieldValueUtility.IsPopulated(
                                    fieldValue,
                                    field.Type)));

            return new FieldDeleteReport(
                field,
                affectedCategoryCount,
                affectedEntries.Count,
                populatedEntryCount);
        }
        #endregion
    }
}