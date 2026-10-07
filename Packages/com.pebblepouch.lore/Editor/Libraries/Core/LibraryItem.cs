namespace PebblePouch.Lore.Editor
{
    public abstract class LibraryItem
    {
        #region Properties
        public abstract string Id { get; }
        public abstract string Title { get; }
        public virtual string SearchText => Title;
        #endregion
    }
}