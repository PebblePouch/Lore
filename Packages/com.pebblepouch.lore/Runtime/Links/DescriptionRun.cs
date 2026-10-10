namespace PebblePouch.Lore
{
    // A piece of description text with one emphasis (markers removed); may still contain [[Target]] links.
    internal readonly struct DescriptionRun
    {
        #region Properties
        public string Text { get; }

        public bool IsBold { get; }

        public bool IsItalic { get; }
        #endregion

        #region Constructor
        public DescriptionRun(
            string text,
            bool isBold,
            bool isItalic)
        {
            Text = text ?? string.Empty;
            IsBold = isBold;
            IsItalic = isItalic;
        }
        #endregion
    }
}
