using System.Collections.Generic;
using System.Linq;

namespace PebblePouch.Lore.Editor
{
    public sealed class CategoryRelationshipAnalyzer
    {
        #region Fields
        private readonly LoreRepository _repository;

        private readonly LoreRelationshipIndex _relationshipIndex;

        private readonly EntryRelationshipAnalyzer _entryRelationshipAnalyzer;
        #endregion

        #region Constructor
        public CategoryRelationshipAnalyzer(
            LoreRepository repository)
        {
            _repository = repository;
            _relationshipIndex = LoreRelationshipIndex.Instance;
            _entryRelationshipAnalyzer = new EntryRelationshipAnalyzer(repository);
        }
        #endregion

        #region Public API
        public CategoryRelationshipReport Analyze(Category category)
        {
            if (category == null)
            {
                return new CategoryRelationshipReport(
                    null,
                    new List<Entry>(),
                    new List<Entry>(),
                    new List<Entry>(),
                    new List<Entry>(),
                    0,
                    0);
            }

            List<Entry> entries =
                _repository.Entries
                    .Where(
                        entry =>
                            entry.Category ==
                            category)
                    .OrderBy(
                        entry =>
                            entry.SortOrder)
                    .ToList();

            var referencedEntries = new List<Entry>();
            var unreferencedEntries = new List<Entry>();
            var isolatedEntries = new List<Entry>();

            int incomingReferenceCount = 0;
            int outgoingReferenceCount = 0;

            foreach (Entry entry in entries)
            {
                IReadOnlyList<EntryReference> referencesTo =
                    _entryRelationshipAnalyzer.GetReferencesTo(
                        entry);

                IReadOnlyList<EntryReference> referencesFrom =
                    _entryRelationshipAnalyzer.GetReferencesFrom(
                        entry);

                if (referencesTo.Count > 0)
                {
                    referencedEntries.Add(entry);
                }
                else
                {
                    unreferencedEntries.Add(entry);
                }

                if (referencesTo.Count == 0 && referencesFrom.Count == 0)
                {
                    isolatedEntries.Add(entry);
                }

                incomingReferenceCount += referencesTo.Count;

                outgoingReferenceCount += referencesFrom.Count;
            }

            return new CategoryRelationshipReport(
                category,
                entries,
                referencedEntries,
                unreferencedEntries,
                isolatedEntries,
                incomingReferenceCount,
                outgoingReferenceCount);
        }

        public IReadOnlyList<CategoryReference> GetCategoryReferences()
        {
            var groups =
                new Dictionary<
                    (Category Source, Category Target),
                    List<EntryReference>>();

            foreach (EntryReference reference in
                     _relationshipIndex.References)
            {
                Category sourceCategory =
                    reference.SourceEntry?.Category;

                Category targetCategory =
                    reference.TargetEntry?.Category;

                if (sourceCategory == null ||
                    targetCategory == null)
                {
                    continue;
                }

                var key =
                    (
                        Source: sourceCategory,
                        Target: targetCategory
                    );

                if (!groups.TryGetValue(
                        key,
                        out List<EntryReference> references))
                {
                    references =
                        new List<EntryReference>();

                    groups.Add(
                        key,
                        references);
                }

                references.Add(
                    reference);
            }

            return groups
                .Select(
                    pair =>
                    {
                        IReadOnlyList<CategoryReferenceField> fields =
                            pair.Value
                                .Where(
                                    reference =>
                                        reference.Field != null)
                                .GroupBy(
                                    reference =>
                                        reference.Field)
                                .Select(
                                    group =>
                                        new CategoryReferenceField(
                                            group.Key,
                                            group.Count()))
                                .OrderByDescending(
                                    field =>
                                        field.ReferenceCount)
                                .ThenBy(
                                    field =>
                                        field.Field.name)
                                .ToList();

                        return new CategoryReference(
                            pair.Key.Source,
                            pair.Key.Target,
                            pair.Value.Count,
                            fields);
                    })
                .OrderBy(
                    reference =>
                        reference.SourceCategory.SortOrder)
                .ThenBy(
                    reference =>
                        reference.TargetCategory.SortOrder)
                .ToList();
        }

        public IReadOnlyList<CategoryReference> GetReferencesFrom(Category sourceCategory)
        {
            if (sourceCategory == null)
                return new List<CategoryReference>();

            return GetCategoryReferences()
                .Where(
                    reference =>
                        reference.SourceCategory ==
                        sourceCategory)
                .ToList();
        }

        public IReadOnlyList<CategoryReference> GetReferencesTo(Category targetCategory)
        {
            if (targetCategory == null)
                return new List<CategoryReference>();

            return GetCategoryReferences()
                .Where(
                    reference =>
                        reference.TargetCategory ==
                        targetCategory)
                .ToList();
        }
        #endregion
    }
}