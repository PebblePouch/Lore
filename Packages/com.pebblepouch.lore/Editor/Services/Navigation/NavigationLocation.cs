namespace PebblePouch.Lore.Editor
{
    public sealed class NavigationLocation
    {
        #region Properties
        public Category Category { get; }
        public Entry Entry { get; }

        // Captured on creation: a deleted Category/Entry compares equal to null afterwards
        // and must not be mistaken for the Project location.
        public bool IsProject { get; }

        public bool IsValid => IsProject || Entry != null || Category != null;
        #endregion

        #region Constructor
        public NavigationLocation(
            Category category,
            Entry entry)
        {
            Entry = entry;
            Category = entry != null ? null : category;

            IsProject = entry == null && category == null;
        }
        #endregion

        #region Public API
        public bool Matches(NavigationLocation other)
        {
            if (other == null)
                return false;

            return IsProject == other.IsProject
                && Category == other.Category
                && Entry == other.Entry;
        }
        #endregion
    }
}
