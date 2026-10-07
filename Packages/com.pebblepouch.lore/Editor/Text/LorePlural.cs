namespace PebblePouch.Lore.Editor
{
    // A count-dependent text, e.g. "{0} Entry" / "{0} Entries". {0} is replaced by the count.
    // Supports one/other (English and most Western languages); languages with more plural forms
    // would extend this type without changing its callers.
    public readonly struct LorePlural
    {
        #region Properties
        public string One { get; }
        public string Other { get; }
        #endregion

        #region Constructor
        public LorePlural(
            string one,
            string other)
        {
            One = one;
            Other = other;
        }
        #endregion

        #region Public API
        // The template for the count, without formatting (e.g. for templates with additional placeholders).
        public string For(int count)
        {
            return count == 1
                ? One
                : Other;
        }

        public string Format(int count)
        {
            return string.Format(
                For(count),
                count);
        }
        #endregion
    }
}
