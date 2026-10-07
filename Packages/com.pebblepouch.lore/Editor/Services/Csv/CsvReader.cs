using System.Collections.Generic;
using System.Text;

namespace PebblePouch.Lore.Editor
{
    // Splits CSV text (see CsvFormat) into rows of cells: quoted cells may contain separators, doubled quotes and
    // line breaks; CRLF, LF and CR end rows; a leading BOM is ignored. Rows without any content are dropped, but
    // still counted, so row numbers match the spreadsheet.
    public static class CsvReader
    {
        #region Types
        public sealed class Row
        {
            // 1-based, like spreadsheet rows (the header is row 1).
            public int Number { get; }
            public IReadOnlyList<string> Cells { get; }

            public Row(
                int number,
                IReadOnlyList<string> cells)
            {
                Number = number;
                Cells = cells;
            }

            // Missing trailing cells read as empty.
            public string Get(int index) =>
                index >= 0 && index < Cells.Count
                    ? Cells[index]
                    : string.Empty;
        }
        #endregion

        #region Constants
        private const char ByteOrderMark = (char)0xFEFF;
        #endregion

        #region Public API
        public static List<Row> Parse(string text)
        {
            List<Row> rows = new();

            if (string.IsNullOrEmpty(text))
                return rows;

            List<string> cells = new();
            StringBuilder cell = new();
            bool quoted = false;
            int number = 1;
            int i = text[0] == ByteOrderMark ? 1 : 0;

            while (i < text.Length)
            {
                char character = text[i];

                if (quoted)
                {
                    if (character == CsvFormat.Quote)
                    {
                        // "" inside quotes = one quote; a single quote ends the quoted part.
                        if (i + 1 < text.Length && text[i + 1] == CsvFormat.Quote)
                        {
                            cell.Append(CsvFormat.Quote);
                            i += 2;
                            continue;
                        }

                        quoted = false;
                    }
                    else
                    {
                        cell.Append(character);
                    }

                    i++;
                    continue;
                }

                if (character == CsvFormat.Quote)
                {
                    quoted = true;
                }
                else if (character == CsvFormat.Separator)
                {
                    cells.Add(cell.ToString());
                    cell.Clear();
                }
                else if (character == '\r' || character == '\n')
                {
                    cells.Add(cell.ToString());
                    cell.Clear();
                    AddRow(rows, number, cells);
                    cells = new List<string>();
                    number++;

                    if (character == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                        i++;
                }
                else
                {
                    cell.Append(character);
                }

                i++;
            }

            if (cell.Length > 0 || cells.Count > 0)
            {
                cells.Add(cell.ToString());
                AddRow(rows, number, cells);
            }

            return rows;
        }
        #endregion

        #region Helpers
        private static void AddRow(
            List<Row> rows,
            int number,
            List<string> cells)
        {
            foreach (string cell in cells)
            {
                if (cell.Length > 0)
                {
                    rows.Add(new Row(number, cells));

                    return;
                }
            }
        }
        #endregion
    }
}
