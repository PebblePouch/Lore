using System.Linq;

namespace PebblePouch.Lore.Editor
{
    // Shared field value checks, so adding a FieldType only needs one change here.
    public static class FieldValueUtility
    {
        #region Public API
        // Field types that reference Entries (relationships, integrity, coverage).
        public static bool IsEntryReference(FieldType type) =>
            type == FieldType.Entry ||
            type == FieldType.Entries;

        // Existing Entries referenced by an Entry or Entries value (empty for other types and missing Entries).
        public static System.Collections.Generic.IEnumerable<Entry> GetReferencedEntries(FieldValue fieldValue)
        {
            if (fieldValue?.Field == null)
                yield break;

            switch (fieldValue.Field.Type)
            {
                case FieldType.Entry:
                    if (fieldValue.EntryValue != null)
                        yield return fieldValue.EntryValue;
                    break;

                case FieldType.Entries:
                    foreach (Entry entry in fieldValue.EntriesValue)
                    {
                        if (entry != null)
                            yield return entry;
                    }
                    break;
            }
        }

        // True if the value holds something meaningful for the given field type
        // (used to warn before data is lost on field type change or deletion).
        public static bool IsPopulated(
            FieldValue fieldValue,
            FieldType type)
        {
            if (fieldValue == null)
                return false;

            return type switch
            {
                FieldType.Text =>
                    !string.IsNullOrEmpty(
                        fieldValue.StringValue),

                FieldType.Url =>
                    !string.IsNullOrEmpty(
                        fieldValue.StringValue),

                FieldType.Entry =>
                    fieldValue.EntryValue != null,

                FieldType.GameObject =>
                    fieldValue.GameObjectValue != null,

                FieldType.Sprite =>
                    fieldValue.SpriteValue != null,

                FieldType.Scene =>
                    fieldValue.SceneValue != null,

                FieldType.ScriptableObject =>
                    fieldValue.ObjectValue != null,

                // Deleted or missing Entries do not count.
                FieldType.Entries =>
                    fieldValue.EntriesValue.Any(entry => entry != null),

                // A removed option counts as empty.
                FieldType.Enum =>
                    fieldValue.Field != null &&
                    fieldValue.Field.FindEnumOption(fieldValue.EnumValue) != null,

                FieldType.Boolean =>
                    fieldValue.BoolValue,

                FieldType.Integer =>
                    fieldValue.IntValue != 0,

                FieldType.Float =>
                    fieldValue.FloatValue != 0f,

                _ => false
            };
        }
        #endregion
    }
}
