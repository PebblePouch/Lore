namespace PebblePouch.Lore
{
    // A piece of description text: plain text, or a [[Target]] link.
    internal readonly struct DescriptionSegment
    {
        #region Properties
        public string Text { get; }

        public bool IsLink { get; }
        #endregion

        #region Constructor
        public DescriptionSegment(
            string text,
            bool isLink)
        {
            Text = text ?? string.Empty;
            IsLink = isLink;
        }
        #endregion
    }
}
