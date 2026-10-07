using System.Text;
using System.Text.RegularExpressions;

namespace PebblePouch.Lore.Editor
{
    // The manual (Documentation~/Manual.md) for the Welcome page: a small Markdown subset as IMGUI rich text.
    // Supported: "#" headings, "-"/"*" bullets, **bold**, *italic*, `code` (shown as is), [label](url) (label only).
    public static class LoreManual
    {
        #region Fields
        private const string FileName = "Manual.md";

        // Heading sizes (rich text <size>); body text uses the style's size.
        private const int HeadingSize = 16;
        private const int SubheadingSize = 14;

        private const string Bullet = "\u2022 ";

        private static readonly Regex BoldPattern = new(@"\*\*(.+?)\*\*");
        private static readonly Regex ItalicPattern = new(@"(?<!\*)\*(?!\s)(.+?)(?<!\s)\*(?!\*)");
        private static readonly Regex CodePattern = new(@"`([^`]+)`");
        private static readonly Regex LinkPattern = new(@"\[([^\]]+)\]\([^)]+\)");

        private static string _richText;
        #endregion

        #region Public API
        // Rich text of the manual (read once per domain load); null when the file is missing.
        public static string GetRichText()
        {
            if (_richText != null)
                return _richText;

            string markdown = LorePackage.ReadDocumentation(FileName);

            if (markdown == null)
                return null;

            _richText = ToRichText(markdown);

            return _richText;
        }
        #endregion

        #region Helpers
        private static string ToRichText(string markdown)
        {
            var builder = new StringBuilder();

            string[] lines = markdown.Replace("\r\n", "\n").Split('\n');

            foreach (string rawLine in lines)
            {
                string line = rawLine.TrimEnd();

                if (line.StartsWith("### "))
                    builder.Append($"<b>{Inline(line.Substring(4))}</b>");
                else if (line.StartsWith("## "))
                    builder.Append($"<size={SubheadingSize}><b>{Inline(line.Substring(3))}</b></size>");
                else if (line.StartsWith("# "))
                    builder.Append($"<size={HeadingSize}><b>{Inline(line.Substring(2))}</b></size>");
                else if (line.TrimStart().StartsWith("- ") || line.TrimStart().StartsWith("* "))
                    builder.Append(Bullet + Inline(line.TrimStart().Substring(2)));
                else
                    builder.Append(Inline(line));

                builder.Append('\n');
            }

            return builder.ToString().TrimEnd();
        }

        private static string Inline(string text)
        {
            // Literal angle brackets would start rich text tags.
            text = text.Replace("<", "\u2039").Replace(">", "\u203A");

            text = LinkPattern.Replace(text, "$1");
            text = CodePattern.Replace(text, "$1");
            text = BoldPattern.Replace(text, "<b>$1</b>");
            text = ItalicPattern.Replace(text, "<i>$1</i>");

            return text;
        }
        #endregion
    }
}
