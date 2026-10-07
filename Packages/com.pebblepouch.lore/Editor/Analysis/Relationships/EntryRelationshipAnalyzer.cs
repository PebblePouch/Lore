using System;
using System.Collections.Generic;

namespace PebblePouch.Lore.Editor
{
    public sealed class EntryRelationshipAnalyzer
    {
        #region Fields
        private readonly LoreRelationshipIndex _relationshipIndex;
        #endregion

        #region Constructor
        public EntryRelationshipAnalyzer(
            LoreRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(
                    nameof(repository));
            }

            _relationshipIndex =
                LoreRelationshipIndex.Instance;
        }
        #endregion

        #region Public API
        public IReadOnlyList<EntryReference> GetReferencesTo(
            Entry target)
        {
            return _relationshipIndex
                .GetReferencesTo(
                    target);
        }

        public IReadOnlyList<EntryReference> GetReferencesFrom(
            Entry source)
        {
            return _relationshipIndex
                .GetReferencesFrom(
                    source);
        }
        #endregion
    }
}