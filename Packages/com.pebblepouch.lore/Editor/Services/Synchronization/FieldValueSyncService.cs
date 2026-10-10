using System.Linq;

using UnityEditor;

namespace PebblePouch.Lore.Editor
{
    public sealed class FieldValueSyncService
    {
        #region Public API
        // After a type change (field.Type is the new type): keeps the values that fit (FieldValue.ConvertValue),
        // clears all others. Same type (ScriptableObject type change): clears all values.
        public void ConvertValues(
            Field field,
            FieldType previousType)
        {
            if (field == null)
                return;

            foreach (Entry entry in LoreRepository.Instance.Entries)
            {
                bool changed = false;

                foreach (FieldValue fieldValue in entry.Fields)
                {
                    if (fieldValue.Field != field)
                        continue;

                    fieldValue.ConvertValue(
                        previousType,
                        field.Type);

                    changed = true;
                }

                if (changed)
                    LoreChangeTracker.MarkModified(entry);
            }
        }

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