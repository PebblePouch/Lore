namespace PebblePouch.Lore.Editor
{
    // An option of an Enum Field while its card is edited. Id is null for options added in this edit.
    public sealed class EnumOptionDraft
    {
        #region Properties
        public string Id { get; }
        public string Label { get; set; }
        #endregion

        #region Constructor
        public EnumOptionDraft(
            string id,
            string label)
        {
            Id = id;
            Label = label ?? string.Empty;
        }
        #endregion
    }
}
