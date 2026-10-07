namespace PebblePouch.Lore.Editor
{
    public sealed class FieldLibraryItem : LibraryItem
    {
        #region Constructor
        public FieldLibraryItem(
            Field field,
            bool isInCategory)
        {
            Field = field;

            Id = field.Id;
            EditingId = $"Field:{field.Id}";

            IsInCategory = isInCategory;
        }
        #endregion

        #region Properties
        public Field Field { get; }

        public override string Id { get; }

        public string EditingId { get; }

        public string Glyph => Field.Glyph;

        public override string Title => Field.name;

        public string Type => ScriptableObjectTypeService.GetFieldTypeLabel(Field);

        public bool IsInCategory { get; }

        public override string SearchText =>
            $"{Title} {Type} {Glyph}";
        #endregion
    }
}