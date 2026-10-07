namespace PebblePouch.Lore.Editor
{
    public sealed class LibraryUsageReport
    {
        #region Properties
        public int FieldCount { get; }
        public int UsedFieldCount { get; }
        public int UnusedFieldCount =>
            FieldCount - UsedFieldCount;

        public int TagCount { get; }
        public int UsedTagCount { get; }
        public int UnusedTagCount =>
            TagCount - UsedTagCount;

        public int GlyphCount { get; }
        public int UsedGlyphCount { get; }
        public int UnusedGlyphCount =>
            GlyphCount - UsedGlyphCount;

        public int ColorCount { get; }
        public int UsedColorCount { get; }
        public int UnusedColorCount =>
            ColorCount - UsedColorCount;
        #endregion

        #region Constructor
        public LibraryUsageReport(
            int fieldCount,
            int usedFieldCount,
            int tagCount,
            int usedtagCount,
            int glyphCount,
            int usedGlyphCount,
            int colorCount,
            int usedColorCount)
        {
            FieldCount = fieldCount;
            UsedFieldCount = usedFieldCount;

            TagCount = tagCount;
            UsedTagCount = usedtagCount;

            GlyphCount = glyphCount;
            UsedGlyphCount = usedGlyphCount;

            ColorCount = colorCount;
            UsedColorCount = usedColorCount;
        }
        #endregion
    }
}