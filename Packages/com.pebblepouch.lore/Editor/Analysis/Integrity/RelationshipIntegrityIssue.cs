namespace PebblePouch.Lore.Editor
{
    public sealed class RelationshipIntegrityIssue
    {
        #region Properties
        public Entry SourceEntry { get; }
        public Field Field { get; }
        #endregion

        #region Constructor
        public RelationshipIntegrityIssue(
            Entry sourceEntry,
            Field field)
        {
            SourceEntry =
                sourceEntry;

            Field =
                field;
        }
        #endregion
    }
}