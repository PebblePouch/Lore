namespace PebblePouch.Lore.Editor
{
    // Lore's font sizes in one place. Style classes reference these instead of their own numbers.
    // Constants only, so this class is safe to use anytime (constructors, OnEnable).
    public static class LoreTypography
    {
        #region Text
        public const int Tiny = 10;         // buttons, feedback, foldouts
        public const int Small = 11;        // subtitles, metadata, placeholders, labels
        public const int Body = 12;         // regular text, descriptions, field values
        public const int Heading = 13;      // section and card headers
        public const int Title = 20;        // Entry title, browser header
        public const int Display = 35;      // Category title
        #endregion

        #region Glyphs and Icons
        public const int GlyphSmall = 14;   // field and menu glyphs
        public const int GlyphMedium = 18;  // library glyphs
        public const int GlyphLarge = 26;   // browser glyphs
        public const int GlyphDisplay = 35; // Inspector header glyph

        public const int IconSmall = 15;    // browser buttons
        public const int IconMedium = 20;   // drag handles, library menu
        public const int IconLarge = 25;    // Inspector header actions
        public const int IconHuge = 45;     // Inspector header menu
        #endregion
    }
}
