namespace PebblePouch.Lore.Editor
{
    public sealed class EntryIntegrityIssue
    {
        #region Properties
        public Entry Entry { get; }
        public int FieldIndex { get; }
        #endregion

        #region Constructor
        public EntryIntegrityIssue(
            Entry entry,
            int fieldIndex)
        {
            Entry = entry;
            FieldIndex = fieldIndex;
        }
        #endregion
    }
}