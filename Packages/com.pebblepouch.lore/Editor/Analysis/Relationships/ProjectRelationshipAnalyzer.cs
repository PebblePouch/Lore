using System.Collections.Generic;
using System.Linq;

namespace PebblePouch.Lore.Editor
{
    public sealed class ProjectRelationshipAnalyzer
    {
        #region Fields
        private readonly LoreRepository _repository;
        private readonly LoreRelationshipIndex _relationshipIndex;
        #endregion

        #region Constructor
        public ProjectRelationshipAnalyzer(
            LoreRepository repository)
        {
            _repository =
                repository;

            _relationshipIndex =
                LoreRelationshipIndex.Instance;
        }
        #endregion

        #region Public API
        public ProjectRelationshipReport Analyze()
        {
            List<Category> categories =
                _repository
                    .Categories
                    .Where(
                        category =>
                            category != null)
                    .ToList();

            List<Category> standaloneCategories =
                categories
                    .Where(
                        category =>
                            !_relationshipIndex
                                .HasCrossCategoryReferences(
                                    category))
                    .ToList();

            List<Category> isolatedCategories =
                categories
                    .Where(
                        category =>
                            !_relationshipIndex
                                .HasReferences(
                                    category))
                    .ToList();

            List<Entry> entries =
                _repository
                    .Entries
                    .Where(
                        entry =>
                            entry != null)
                    .ToList();

            var referencedEntries =
                new List<Entry>();

            var unreferencedEntries =
                new List<Entry>();

            var isolatedEntries =
                new List<Entry>();

            foreach (Entry entry in entries)
            {
                bool hasIncomingReferences =
                    _relationshipIndex
                        .GetReferencesTo(
                            entry)
                        .Count > 0;

                bool hasOutgoingReferences =
                    _relationshipIndex
                        .GetReferencesFrom(
                            entry)
                        .Count > 0;

                if (hasIncomingReferences)
                {
                    referencedEntries.Add(entry);
                }
                else
                {
                    unreferencedEntries.Add(entry);
                }

                if (!hasIncomingReferences &&
                    !hasOutgoingReferences)
                {
                    isolatedEntries.Add(entry);
                }
            }

            return new ProjectRelationshipReport(
                categories.Count,
                standaloneCategories,
                isolatedCategories,
                entries,
                referencedEntries,
                unreferencedEntries,
                isolatedEntries,
                _relationshipIndex.References.Count);
        }

        public bool IsStandalone(Category category)
        {
            if (category == null)
                return false;

            return !_relationshipIndex
                .HasCrossCategoryReferences(
                    category);
        }

        public bool IsIsolated(Category category)
        {
            if (category == null)
                return false;

            return !_relationshipIndex
                .HasReferences(
                    category);
        }

        public bool IsIsolated(Entry entry)
        {
            if (entry == null)
                return false;

            bool hasIncomingReferences =
                _relationshipIndex
                    .GetReferencesTo(
                        entry)
                    .Count > 0;

            bool hasOutgoingReferences =
                _relationshipIndex
                    .GetReferencesFrom(
                        entry)
                    .Count > 0;

            return
                !hasIncomingReferences && !hasOutgoingReferences;
        }
        #endregion
    }
}