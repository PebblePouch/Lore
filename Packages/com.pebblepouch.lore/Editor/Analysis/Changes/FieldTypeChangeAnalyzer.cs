using System.Linq;

namespace PebblePouch.Lore.Editor
{
    public sealed class FieldTypeChangeAnalyzer
    {
        #region Public API
        // newObjectTypeName: the ScriptableObject type for ScriptableObject Fields (otherwise ignored).
        public FieldTypeChangeReport Analyze(
            Field field,
            FieldType newType,
            string newObjectTypeName = null)
        {
            if (field == null)
            {
                return new FieldTypeChangeReport(
                    null,
                    newType,
                    newObjectTypeName,
                    newType,
                    newObjectTypeName,
                    0);
            }

            var typeChange =
                new FieldTypeChangeReport(
                    field,
                    field.Type,
                    field.ObjectTypeName,
                    newType,
                    newObjectTypeName,
                    0);

            if (!typeChange.IsChanged)
                return typeChange;

            int affectedEntryCount =
                LoreRepository.Instance
                    .Entries
                    .Count(
                        entry =>
                            entry.Fields.Any(
                                fieldValue =>
                                    fieldValue.Field == field &&
                                    FieldValueUtility.IsPopulated(
                                        fieldValue,
                                        field.Type)));

            return new FieldTypeChangeReport(
                field,
                field.Type,
                field.ObjectTypeName,
                newType,
                newObjectTypeName,
                affectedEntryCount);
        }
        #endregion
    }
}
