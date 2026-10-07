namespace PebblePouch.Lore.Editor
{
    public sealed class CategoryIntegrityIssue
    {
        #region Properties
        public Category Category { get; }
        public int FieldIndex { get; }
        #endregion

        #region Constructor
        public CategoryIntegrityIssue(
            Category category,
            int fieldIndex)
        {
            Category = category;
            FieldIndex = fieldIndex;
        }
        #endregion
    }
}