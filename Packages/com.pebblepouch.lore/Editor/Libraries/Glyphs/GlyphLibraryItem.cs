namespace PebblePouch.Lore.Editor
{
    public sealed class GlyphLibraryItem : LibraryItem
    {
        #region Constructor
        public GlyphLibraryItem(GlyphDefinition definition)
        {
            Definition = definition;
            Id = definition.Glyph;
            Glyph = definition.Glyph;
            Title = definition.Title;
            Category = definition.Category;
        }
        #endregion

        #region Properties
        public GlyphDefinition Definition { get; }
        public override string Id { get; }
        public string Glyph { get; }
        public override string Title { get; }
        public string Category { get; }
        public override string SearchText => $"{Title} {Category} {Glyph}";
        #endregion
    }
}