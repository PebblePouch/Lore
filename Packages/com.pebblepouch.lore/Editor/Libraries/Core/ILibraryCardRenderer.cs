namespace PebblePouch.Lore.Editor
{
    public interface ILibraryCardRenderer<T> where T : LibraryItem
    {
        #region Public API
        BrowserItemDrawResult Draw(
            T item,
            bool highlighted,
            bool pickable);
        #endregion
    }
}