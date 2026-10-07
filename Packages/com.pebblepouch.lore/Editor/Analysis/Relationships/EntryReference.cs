namespace PebblePouch.Lore.Editor
{
    public sealed class EntryReference
    {
        #region Properties
        public Entry SourceEntry { get; }
        public Field Field { get; }
        public Entry TargetEntry { get; }
        #endregion

        #region Constructor
        public EntryReference(
            Entry sourceEntry,
            Field field,
            Entry targetEntry)
        {
            SourceEntry = sourceEntry;
            Field = field;
            TargetEntry = targetEntry;
        }
        #endregion
    }
}