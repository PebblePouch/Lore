namespace PebblePouch.Lore.Editor
{
    public static class MissingFieldAssignmentContext
    {
        #region Fields
        public static Category TargetCategory { get; private set; }
        public static int TargetFieldIndex { get; private set; } = -1;
        #endregion

        #region Properties
        public static bool IsActive =>
            TargetCategory != null &&
            TargetFieldIndex >= 0;
        #endregion

        #region Public API
        public static void BeginAssignment(
            Category category,
            int fieldIndex)
        {
            if (category == null ||
                fieldIndex < 0)
            {
                return;
            }

            TargetCategory =
                category;

            TargetFieldIndex =
                fieldIndex;
        }

        public static void EndAssignment()
        {
            TargetCategory =
                null;

            TargetFieldIndex =
                -1;
        }
        #endregion
    }
}