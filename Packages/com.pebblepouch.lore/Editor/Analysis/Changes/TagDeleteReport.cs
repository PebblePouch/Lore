namespace PebblePouch.Lore.Editor
{
    public sealed class TagDeleteReport
    {
        #region Properties
        public TagDefinition Tag { get; }

        public int AffectedEntryCount { get; }

        public bool IsInUse =>
            AffectedEntryCount > 0;
        #endregion

        #region Constructor
        public TagDeleteReport(
            TagDefinition tag,
            int affectedEntryCount)
        {
            Tag =
                tag;

            AffectedEntryCount =
                affectedEntryCount;
        }
        #endregion
    }
}