namespace PebblePouch.Lore.Editor
{
    // A Field type change, including a changed ScriptableObject type (both clear the set values, except the
    // changes that keep them, see KeepsValues).
    public sealed class FieldTypeChangeReport
    {
        #region Properties
        public Field Field { get; }

        public FieldType CurrentType { get; }

        public FieldType NewType { get; }

        // Display names, e.g. "DialogDefinition (SO)".
        public string CurrentLabel { get; }

        public string NewLabel { get; }

        public bool IsChanged { get; }

        public int AffectedEntryCount { get; }

        // Enum ↔ Enums, Entry ↔ Entries: values are kept; only lists with several values are cleared.
        public bool KeepsValues =>
            FieldValueUtility.KeepsValues(
                CurrentType,
                NewType);

        public bool HasDataLoss =>
            AffectedEntryCount > 0;

        public bool RequiresConfirmation =>
            IsChanged &&
            HasDataLoss;
        #endregion

        #region Constructor
        public FieldTypeChangeReport(
            Field field,
            FieldType currentType,
            string currentObjectTypeName,
            FieldType newType,
            string newObjectTypeName,
            int affectedEntryCount)
        {
            Field =
                field;

            CurrentType =
                currentType;

            NewType =
                newType;

            CurrentLabel =
                ScriptableObjectTypeService.GetFieldTypeLabel(
                    currentType,
                    currentObjectTypeName);

            NewLabel =
                ScriptableObjectTypeService.GetFieldTypeLabel(
                    newType,
                    newObjectTypeName);

            IsChanged =
                currentType != newType ||
                ScriptableObjectTypeService.HasObjectType(newType) &&
                (currentObjectTypeName ?? string.Empty) != (newObjectTypeName ?? string.Empty);

            AffectedEntryCount =
                affectedEntryCount;
        }
        #endregion
    }
}
