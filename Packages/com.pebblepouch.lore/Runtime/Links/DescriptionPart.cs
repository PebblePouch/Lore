namespace PebblePouch.Lore
{
    // A piece of a description for games (see LoreProject.ParseDescription): plain text, or a link to an Entry.
    // Links carry the current Entry title as Text; broken links (Entry missing) keep their last known title
    // and have no Entry.
    public readonly struct DescriptionPart
    {
        #region Properties
        public string Text { get; }

        public bool IsLink { get; }

        // The linked Entry; null for plain text and broken links.
        public Entry Entry { get; }

        public bool IsBrokenLink => IsLink && Entry == null;
        #endregion

        #region Constructor
        internal DescriptionPart(
            string text,
            bool isLink,
            Entry entry)
        {
            Text = text ?? string.Empty;
            IsLink = isLink;
            Entry = entry;
        }
        #endregion
    }
}
