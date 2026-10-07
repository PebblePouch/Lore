namespace PebblePouch.Lore.Editor
{
    public sealed class FieldCardEditState
    {
        #region Properties
        public string Title { get; set; }
        public string Glyph { get; set; }
        public FieldType Type { get; set; }

        // ScriptableObject Fields: chosen type (Field.GetTypeName form) and its script GUID.
        public string ObjectTypeName { get; set; }
        public string ObjectScriptGuid { get; set; }

        // Enum Fields: the options being edited (applied on save, see FieldService.UpdateField).
        public System.Collections.Generic.List<EnumOptionDraft> EnumOptions { get; } = new();

        // Entry / Entries Fields: allowed Categories being edited (empty = all; applied on save).
        public System.Collections.Generic.List<Category> AllowedCategories { get; } = new();

        // Set when ScriptableObject was just chosen: the card opens the type dropdown on its next draw.
        public bool ObjectTypePickerRequested { get; set; }

        public string Feedback { get; set; }
        #endregion

        #region Constructor
        public FieldCardEditState(
            Field field)
        {
            Title = field.name;
            Glyph = field.Glyph;
            Type = field.Type;
            ObjectTypeName = field.ObjectTypeName;
            ObjectScriptGuid = field.ObjectScriptGuid;

            foreach (EnumOption option in field.EnumOptions)
            {
                if (option != null)
                    EnumOptions.Add(new EnumOptionDraft(option.Id, option.Label));
            }

            foreach (Category category in field.AllowedCategories)
            {
                if (category != null)
                    AllowedCategories.Add(category);
            }
        }
        #endregion
    }
}