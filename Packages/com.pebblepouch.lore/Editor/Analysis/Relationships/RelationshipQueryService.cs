using System.Collections.Generic;
using System.Linq;

namespace PebblePouch.Lore.Editor
{
    public sealed class RelationshipQueryService
    {
        #region Fields
        private readonly LoreRepository _repository;
        private readonly EntryRelationshipAnalyzer _entryRelationshipAnalyzer;
        #endregion

        #region Constructor
        public RelationshipQueryService(
            LoreRepository repository)
        {
            _repository =
                repository;

            _entryRelationshipAnalyzer =
                new EntryRelationshipAnalyzer(
                    repository);
        }
        #endregion

        #region Public API
        public IReadOnlyList<Entry> GetEntriesReferencing(
            Entry target)
        {
            if (target == null)
                return new List<Entry>();

            return _entryRelationshipAnalyzer
                .GetReferencesTo(
                    target)
                .Where(
                    reference =>
                        reference.SourceEntry != null)
                .Select(
                    reference =>
                        reference.SourceEntry)
                .Distinct()
                .ToList();
        }

        public IReadOnlyList<Entry> GetEntriesReferencedBy(
            Entry source)
        {
            if (source == null)
                return new List<Entry>();

            return _entryRelationshipAnalyzer
                .GetReferencesFrom(
                    source)
                .Where(
                    reference =>
                        reference.TargetEntry != null)
                .Select(
                    reference =>
                        reference.TargetEntry)
                .Distinct()
                .ToList();
        }

        public IReadOnlyList<Entry> GetUnreferencedEntries()
        {
            return _repository
                .Entries
                .Where(
                    entry =>
                        _entryRelationshipAnalyzer
                            .GetReferencesTo(
                                entry)
                            .Count == 0)
                .ToList();
        }

        public IReadOnlyList<Entry> GetEntriesBetween(
            Category sourceCategory,
            Category targetCategory)
        {
            if (sourceCategory == null ||
                targetCategory == null)
            {
                return new List<Entry>();
            }

            return _repository
                .Entries
                .Where(
                    entry =>
                        entry.Category ==
                        sourceCategory)
                .Where(
                    entry =>
                        _entryRelationshipAnalyzer
                            .GetReferencesFrom(
                                entry)
                            .Any(
                                reference =>
                                    reference.TargetEntry?.Category ==
                                    targetCategory))
                .ToList();
        }

        public IReadOnlyList<Entry> GetEntriesUsingField(Field field)
        {
            if (field == null)
                return new List<Entry>();

            return _repository
                .Entries
                .Where(
                    entry =>
                        entry.Fields.Any(
                            fieldValue =>
                                fieldValue?.Field == field))
                .ToList();
        }

        public IReadOnlyList<Entry> GetEntriesUsingTag(TagDefinition tag)
        {
            if (tag == null)
                return new List<Entry>();

            return _repository
                .Entries
                .Where(
                    entry =>
                        entry.TagIds.Contains(tag.Id))
                .ToList();
        }

        public bool IsUnreferenced(
    Entry entry)
        {
            if (entry == null)
                return false;

            return _entryRelationshipAnalyzer
                .GetReferencesTo(
                    entry)
                .Count == 0;
        }

        public bool ReferencesCategory(
            Entry entry,
            Category targetCategory)
        {
            if (entry == null ||
                targetCategory == null)
            {
                return false;
            }

            return _entryRelationshipAnalyzer
                .GetReferencesFrom(
                    entry)
                .Any(
                    reference =>
                        reference.TargetEntry?.Category ==
                        targetCategory);
        }

        public bool ReferencesCategoryViaField(
            Entry entry,
            Category targetCategory,
            Field field)
        {
            if (entry == null ||
                targetCategory == null ||
                field == null)
            {
                return false;
            }

            return _entryRelationshipAnalyzer
                .GetReferencesFrom(
                    entry)
                .Any(
                    reference =>
                        reference.Field ==
                            field &&
                        reference.TargetEntry?.Category ==
                            targetCategory);
        }
        #endregion
    }
}