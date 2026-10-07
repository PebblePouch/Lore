namespace PebblePouch.Lore.Editor
{
    // A ScriptableObject Field of a Category whose type cannot be resolved (see FieldTypeIntegrityAnalyzer).
    public sealed class FieldTypeIntegrityIssue
    {
        #region Properties
        public Category Category { get; }
        public Field Field { get; }
        public int FieldIndex { get; }
        #endregion

        #region Constructor
        public FieldTypeIntegrityIssue(
            Category category,
            Field field,
            int fieldIndex)
        {
            Category = category;
            Field = field;
            FieldIndex = fieldIndex;
        }
        #endregion
    }
}
