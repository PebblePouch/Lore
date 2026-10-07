using System.Linq;

using UnityEditor;

namespace PebblePouch.Lore.Editor
{
    public sealed class TagSyncService
    {
        #region Public API
        public void RemoveTagAssignments(
            TagDefinition tag)
        {
            if (tag == null)
                return;

            var entries =
                LoreRepository.Instance
                    .Entries
                    .Where(
                        entry =>
                            entry.HasTag(
                                tag.Id))
                    .ToList();

            foreach (Entry entry in
                     entries)
            {
                if (!entry.RemoveTag(
                        tag.Id))
                {
                    continue;
                }

                LoreChangeTracker.MarkModified(
                    entry);
            }
        }
        #endregion
    }
}