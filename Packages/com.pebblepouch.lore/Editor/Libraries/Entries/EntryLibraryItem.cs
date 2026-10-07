namespace PebblePouch.Lore.Editor
{
    public sealed class EntryLibraryItem : LibraryItem
    {
        #region Constructor
        public EntryLibraryItem(Entry entry)
        {
            Entry = entry;
            Id = entry.Id;
        }
        #endregion

        #region Properties
        public Entry Entry { get; }

        public override string Id { get; }

        public string Glyph => Entry.Glyph;

        public override string Title => Entry.Title;

        public string Category =>
            Entry.Category != null
                ? Entry.Category.Title
                : string.Empty;

        public override string SearchText =>
            $"{Title} {Category} {Glyph}";
        #endregion
    }
}