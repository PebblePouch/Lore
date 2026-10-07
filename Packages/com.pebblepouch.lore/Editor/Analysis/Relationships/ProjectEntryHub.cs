namespace PebblePouch.Lore.Editor
{
    public sealed class ProjectEntryHub
    {
        #region Properties
        public Entry Entry { get; }
        public int ReferenceCount { get; }
        #endregion

        #region Constructor
        public ProjectEntryHub(
            Entry entry,
            int referenceCount)
        {
            Entry =
                entry;

            ReferenceCount =
                referenceCount;
        }
        #endregion
    }
}