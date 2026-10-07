namespace PebblePouch.Lore.Editor
{
    // A GameObject/Sprite/Scene field value whose referenced Unity asset no longer exists.
    public sealed class UnityReferenceIntegrityIssue
    {
        #region Properties
        public Entry SourceEntry { get; }
        public Field Field { get; }
        #endregion

        #region Constructor
        public UnityReferenceIntegrityIssue(
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
