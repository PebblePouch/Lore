using System;
using System.Collections.Generic;

namespace PebblePouch.Lore.Editor
{
    public sealed class LoreRelationshipIndex
    {
        #region Singleton
        private static LoreRelationshipIndex _instance;

        public static LoreRelationshipIndex Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance =
                        new LoreRelationshipIndex(
                            LoreRepository.Instance);
                }

                return _instance;
            }
        }
        #endregion

        #region Fields
        private readonly LoreRepository _repository;

        private readonly List<EntryReference> _references = new();

        private readonly Dictionary<Entry, List<EntryReference>> _referencesFrom = new();
        private readonly Dictionary<Entry, List<EntryReference>> _referencesTo = new();

        private readonly HashSet<Category> _categoriesWithReferences = new();
        private readonly HashSet<Category> _categoriesWithCrossCategoryReferences = new();
        #endregion

        #region Properties
        public IReadOnlyList<EntryReference> References => _references;
        #endregion

        #region Events
        public event Action Refreshed;
        #endregion

        #region Constructor
        private LoreRelationshipIndex(LoreRepository repository)
        {
            _repository = repository;

            Rebuild();
        }
        #endregion

        #region Public API
        public IReadOnlyList<EntryReference> GetReferencesFrom(
            Entry source)
        {
            if (source == null)
                return System.Array.Empty<EntryReference>();

            if (!_referencesFrom.TryGetValue(
                    source,
                    out List<EntryReference> references))
            {
                return System.Array.Empty<EntryReference>();
            }

            return references;
        }

        public IReadOnlyList<EntryReference> GetReferencesTo(
            Entry target)
        {
            if (target == null)
                return System.Array.Empty<EntryReference>();

            if (!_referencesTo.TryGetValue(
                    target,
                    out List<EntryReference> references))
            {
                return System.Array.Empty<EntryReference>();
            }

            return references;
        }

        public bool HasReferences(Category category)
        {
            if (category == null)
                return false;

            return _categoriesWithReferences.Contains(category);
        }

        public bool HasCrossCategoryReferences(Category category)
        {
            if (category == null)
                return false;

            return _categoriesWithCrossCategoryReferences.Contains(category);
        }
        #endregion

        #region Helpers
        private void Rebuild()
        {
            _references.Clear();
            _referencesFrom.Clear();
            _referencesTo.Clear();

            _categoriesWithReferences.Clear();
            _categoriesWithCrossCategoryReferences.Clear();

            foreach (Entry source in
                     _repository.Entries)
            {
                if (source == null)
                    continue;

                foreach (FieldValue fieldValue in source.Fields)
                {
                    if (fieldValue?.Field == null ||
                        !FieldValueUtility.IsEntryReference(fieldValue.Field.Type))
                    {
                        continue;
                    }

                    // Entry fields: one target; Entries fields: one reference per listed Entry.
                    foreach (Entry target in FieldValueUtility.GetReferencedEntries(fieldValue))
                        AddReference(source, fieldValue.Field, target);
                }
            }
        }

        private void AddReference(
            Entry source,
            Field field,
            Entry target)
        {
            Category sourceCategory = source.Category;
            Category targetCategory = target.Category;

            if (sourceCategory != null)
            {
                _categoriesWithReferences.Add(sourceCategory);
            }

            if (targetCategory != null)
            {
                _categoriesWithReferences.Add(targetCategory);
            }

            if (sourceCategory != null
                && targetCategory != null
                && sourceCategory != targetCategory)
            {
                _categoriesWithCrossCategoryReferences.Add(sourceCategory);
                _categoriesWithCrossCategoryReferences.Add(targetCategory);
            }

            var reference =
                new EntryReference(
                    source,
                    field,
                    target);

            _references.Add(reference);

            AddReference(
                _referencesFrom,
                source,
                reference);

            AddReference(
                _referencesTo,
                target,
                reference);
        }

        private static void AddReference(
            Dictionary<Entry, List<EntryReference>> lookup,
            Entry entry,
            EntryReference reference)
        {
            if (!lookup.TryGetValue(
                    entry,
                    out List<EntryReference> references))
            {
                references =
                    new List<EntryReference>();

                lookup.Add(
                    entry,
                    references);
            }

            references.Add(
                reference);
        }

        public void Refresh()
        {
            Rebuild();

            Refreshed?.Invoke();
        }
        #endregion
    }
}