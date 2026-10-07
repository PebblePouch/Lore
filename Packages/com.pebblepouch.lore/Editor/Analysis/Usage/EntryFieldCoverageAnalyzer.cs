using System.Collections.Generic;
using System.Linq;

namespace PebblePouch.Lore.Editor
{
    public sealed class EntryFieldCoverageAnalyzer
    {
        #region Fields
        private readonly LoreRepository _repository;
        #endregion

        #region Constructor
        public EntryFieldCoverageAnalyzer(
            LoreRepository repository)
        {
            _repository =
                repository;
        }
        #endregion

        #region Public API
        public IReadOnlyList<EntryFieldCoverage> Analyze()
        {
            var coverage =
                new List<EntryFieldCoverage>();

            foreach (Category category in
                     _repository.Categories)
            {
                if (category == null)
                    continue;

                List<Entry> entries =
                    _repository
                        .Entries
                        .Where(
                            entry =>
                                entry != null &&
                                entry.Category ==
                                    category)
                        .ToList();

                if (entries.Count == 0)
                    continue;

                foreach (Field field in
                         category.Fields)
                {
                    if (field == null)
                        continue;

                    if (!FieldValueUtility.IsEntryReference(field.Type))
                    {
                        continue;
                    }

                    int populatedCount =
                        entries.Count(
                            entry =>
                                IsPopulated(
                                    entry,
                                    field));

                    coverage.Add(
                        new EntryFieldCoverage(
                            category,
                            field,
                            entries.Count,
                            populatedCount));
                }
            }

            return coverage;
        }

        public bool IsUnpopulated(
            Entry entry,
            Field field)
        {
            if (entry == null ||
                field == null ||
                !FieldValueUtility.IsEntryReference(field.Type))
            {
                return false;
            }

            return !IsPopulated(
                entry,
                field);
        }
        #endregion

        #region Helpers
        private static bool IsPopulated(
            Entry entry,
            Field field)
        {
            FieldValue fieldValue =
                entry.Fields
                    .FirstOrDefault(
                        value =>
                            value?.Field ==
                            field);

            if (fieldValue == null)
                return false;

            return FieldValueUtility.GetReferencedEntries(fieldValue).Any();
        }
        #endregion
    }
}