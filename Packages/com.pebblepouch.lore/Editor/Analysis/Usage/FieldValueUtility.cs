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

        // Field types with options defined on the Field (Field.EnumOptions).
        public static bool HasEnumOptions(FieldType type) =>
            type == FieldType.Enum ||
            type == FieldType.Enums;

        // Type changes that keep values (see FieldValue.ConvertValue): single value ↔ list of the same kind.
        public static bool KeepsValues(
            FieldType from,
            FieldType to) =>
            from == FieldType.Enum && to == FieldType.Enums ||
            from == FieldType.Enums && to == FieldType.Enum ||
            from == FieldType.Entry && to == FieldType.Entries ||
            from == FieldType.Entries && to == FieldType.Entry;

        // True if changing the Field type from → to loses this value (type changes clear values, except those that
        // keep them; a list with several values cannot become a single value). Same type
        // (ScriptableObject / Object type change): any set value is lost.
        public static bool LosesValue(
            FieldValue fieldValue,
            FieldType from,
            FieldType to)
        {
            if (!IsPopulated(fieldValue, from))
                return false;

            if (!KeepsValues(from, to))
                return true;

            return from switch
            {
                FieldType.Enums => fieldValue.EnumValues.Count > 1,
                FieldType.Entries => fieldValue.EntriesValue.Count > 1,
                _ => false
            };
        }

        // Field types whose default is a valid value (false, black, zero vector): such values are never "missing"
        // (Required Fields).
        public static bool CanBeMissing(FieldType type) =>
            type != FieldType.Boolean &&
            type != FieldType.Color &&
            type != FieldType.Vector;

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

                FieldType.Folder =>
                    fieldValue.FolderValue != null,

                FieldType.ScriptableObject =>
                    fieldValue.ObjectValue != null,

                FieldType.Object =>
                    fieldValue.ObjectValue != null,

                FieldType.Audio =>
                    fieldValue.AudioClipValue != null,

                // Deleted or missing Entries do not count.
                FieldType.Entries =>
                    fieldValue.EntriesValue.Any(entry => entry != null),

                // A removed option counts as empty.
                FieldType.Enum =>
                    fieldValue.Field != null &&
                    fieldValue.Field.FindEnumOption(fieldValue.EnumValue) != null,

                // Removed options do not count.
                FieldType.Enums =>
                    fieldValue.Field != null &&
                    fieldValue.EnumValues.Any(optionId => fieldValue.Field.FindEnumOption(optionId) != null),

                FieldType.Boolean =>
                    fieldValue.BoolValue,

                FieldType.Integer =>
                    fieldValue.IntValue != 0,

                FieldType.Float =>
                    fieldValue.FloatValue != 0f,

                FieldType.Color =>
                    fieldValue.ColorValue != UnityEngine.Color.black,

                FieldType.Vector =>
                    fieldValue.VectorValue != UnityEngine.Vector3.zero,

                _ => false
            };
        }
        #endregion
    }
}
