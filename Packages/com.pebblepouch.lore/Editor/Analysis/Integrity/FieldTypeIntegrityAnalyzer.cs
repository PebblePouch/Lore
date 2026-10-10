using System.Collections.Generic;

namespace PebblePouch.Lore.Editor
{
    // Finds ScriptableObject / Object Fields whose type cannot be resolved: the class was deleted, or renamed in a way
    // the automatic repair (script GUID, see ScriptableObjectTypeService.RepairTypeName) cannot follow, or no type is set.
    // Only Fields used by Categories count as integrity issues; the Field Library marks unused ones too.
    public sealed class FieldTypeIntegrityAnalyzer
    {
        #region Public API
        public static bool HasMissingType(Field field)
        {
            return field != null &&
                   ScriptableObjectTypeService.HasObjectType(field.Type) &&
                   field.ObjectType == null;
        }

        public IReadOnlyList<FieldTypeIntegrityIssue> Analyze(Category category)
        {
            var issues =
                new List<FieldTypeIntegrityIssue>();

            if (category == null)
                return issues;

            for (int i = 0;
                 i < category.Fields.Count;
                 i++)
            {
                Field field =
                    category.Fields[i];

                if (HasMissingType(field))
                {
                    issues.Add(
                        new FieldTypeIntegrityIssue(
                            category,
                            field,
                            i));
                }
            }

            return issues;
        }

        // Distinct Fields with a missing type over all Categories (a Field used twice counts once).
        public int CountFields(IEnumerable<Category> categories)
        {
            var fields =
                new HashSet<Field>();

            foreach (Category category in categories)
            {
                foreach (FieldTypeIntegrityIssue issue in Analyze(category))
                    fields.Add(issue.Field);
            }

            return fields.Count;
        }
        #endregion
    }
}
