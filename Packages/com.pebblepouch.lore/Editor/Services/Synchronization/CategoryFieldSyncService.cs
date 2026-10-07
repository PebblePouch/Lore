using System.Linq;

using UnityEditor;

namespace PebblePouch.Lore.Editor
{
    public sealed class CategoryFieldSyncService
    {
        #region Public API
        public void SyncCategory(Category category)
        {
            if (category == null)
                return;

            var entries =
                LoreRepository.Instance
                    .Entries
                    .Where(
                        entry =>
                            entry.Category ==
                            category)
                    .ToList();

            foreach (Entry entry in
                     entries)
            {
                // Field values added/removed = change; only reordered (Fields moved in the Category) = no change.
                if (entry.SyncFields())
                {
                    LoreChangeTracker.MarkModified(
                        entry);
                }
                else
                {
                    EditorUtility.SetDirty(
                        entry);
                }
            }
        }
        #endregion
    }
}