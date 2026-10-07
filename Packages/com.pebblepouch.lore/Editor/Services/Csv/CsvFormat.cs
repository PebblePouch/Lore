using System.Globalization;
using System.Text;

namespace PebblePouch.Lore.Editor
{
    // The Category CSV format (export and import), RFC 4180 style with English conventions:
    // "," between cells, "." in numbers, CRLF between rows, UTF-8 with BOM (Excel shows glyphs correctly).
    // Column names and cell keywords are technical (read back by the import), so they are not in LoreText.
    public static class CsvFormat
    {
        #region Constants
        public const char Separator = ',';
        public const char Quote = '"';
        public const string NewLine = "\r\n";
        public const string FileExtension = "csv";

        // Entries / Tags cells: titles joined by "|" (Lore names cannot contain it).
        public const char ListSeparator = '|';

        // Sub-assets (e.g. a Sprite of a sprite sheet): "Assets/Art/Sheet.png#Wolf_0".
        public const char SubAssetSeparator = '#';

        public const string True = "TRUE";
        public const string False = "FALSE";
        #endregion

        #region Columns
        // Entry columns before the Field columns (in this order).
        public const string IdColumn = "Id";
        public const string TitleColumn = "Title";
        public const string SubtitleColumn = "Subtitle";
        public const string GlyphColumn = "Glyph";
        public const string TagsColumn = "Tags";
        public const string DescriptionColumn = "Description";

        public static readonly string[] EntryColumns =
        {
            IdColumn,
            TitleColumn,
            SubtitleColumn,
            GlyphColumn,
            TagsColumn,
            DescriptionColumn,
        };
        #endregion

        #region Encoding
        public static readonly Encoding FileEncoding = new UTF8Encoding(true);
        #endregion

        #region Public API
        // Quotes a cell when it contains a separator, quote or line break (or leading/trailing spaces, which
        // spreadsheets would otherwise trim); quotes inside are doubled.
        public static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            bool needsQuotes =
                value.IndexOf(Separator) >= 0 ||
                value.IndexOf(Quote) >= 0 ||
                value.IndexOf('\n') >= 0 ||
                value.IndexOf('\r') >= 0 ||
                char.IsWhiteSpace(value[0]) ||
                char.IsWhiteSpace(value[value.Length - 1]);

            if (!needsQuotes)
                return value;

            return Quote + value.Replace("\"", "\"\"") + Quote;
        }

        public static string FormatBool(bool value) => value ? True : False;

        public static string FormatInt(int value) => value.ToString(CultureInfo.InvariantCulture);

        // "R": shortest text that reads back as the same float.
        public static string FormatFloat(float value) => value.ToString("R", CultureInfo.InvariantCulture);
        #endregion
    }
}
