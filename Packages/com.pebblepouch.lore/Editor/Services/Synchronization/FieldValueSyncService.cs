using System.Linq;

using UnityEditor;

namespace PebblePouch.Lore.Editor
{
    public sealed class FieldValueSyncService
    {
        #region Public API
        public void ClearValues(Field field)
        {
            if (field == null)
                return;

            var entries =
                LoreRepository.Instance
                    .Entries
                    .Where(
                        entry =>
                            entry.Fields.Any(
                                fieldValue =>
                                    fieldValue.Field ==
                                    field))
                    .ToList();

            foreach (Entry entry in
                     entries)
            {
                foreach (FieldValue fieldValue in
                         entry.Fields)
                {
                    if (fieldValue.Field !=
                        field)
                    {
                        continue;
                    }

                    fieldValue.Clear();
                }

                LoreChangeTracker.MarkModified(
                    entry);
            }
        }
        #endregion
    }
}