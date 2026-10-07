namespace PebblePouch.Lore.Editor
{
    public sealed class CategoryReferenceField
    {
        #region Properties
        public Field Field { get; }

        public int ReferenceCount { get; }
        #endregion

        #region Constructor
        public CategoryReferenceField(
            Field field,
            int referenceCount)
        {
            Field =
                field;

            ReferenceCount =
                referenceCount;
        }
        #endregion
    }
}