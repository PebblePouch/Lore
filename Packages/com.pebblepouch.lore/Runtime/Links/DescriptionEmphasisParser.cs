using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace PebblePouch.Lore
{
    // Markdown emphasis in descriptions: **bold** / __bold__, *italic* / _italic_, ***bold italic***.
    // Simplified CommonMark delimiter rules: a marker opens before and closes after non-whitespace, "_" never works
    // inside a word (snake_case stays as is), emphasis does not cross line breaks, and markers without a partner
    // stay visible. \* and \_ show the marker itself. [[Links]] are never split: markers inside them are ignored,
    // markers around them emphasize the whole link.
    // Shared by the editor (view mode, search) and the runtime API (LoreProject descriptions).
    internal static class DescriptionEmphasisParser
    {
        #region Types
        // A run of equal markers ("**"); Start/Count shrink as markers are used by matches.
        private sealed class Delimiter
        {
            public char Marker;
            public int Start;
            public int Count;
            public int OriginalCount;
            public bool CanOpen;
            public bool CanClose;
        }
        #endregion

        #region Fields
        private const char Escape = '\\';

        private static readonly char[] Markers = { '*', '_' };
        #endregion

        #region Public API
        public static bool ContainsMarkers(string text)
        {
            return !string.IsNullOrEmpty(text) &&
                   text.IndexOfAny(Markers) >= 0;
        }

        // The text as shown: emphasis markers (and their escapes) removed, links kept as they are.
        public static string StripMarkers(string text)
        {
            if (!ContainsMarkers(text))
                return text ?? string.Empty;

            var builder = new StringBuilder(text.Length);

            foreach (DescriptionRun run in Parse(text))
                builder.Append(run.Text);

            return builder.ToString();
        }

        public static List<DescriptionRun> Parse(string text)
        {
            var runs = new List<DescriptionRun>();

            if (string.IsNullOrEmpty(text))
                return runs;

            if (!ContainsMarkers(text))
            {
                runs.Add(
                    new DescriptionRun(
                        text,
                        false,
                        false));

                return runs;
            }

            int length = text.Length;

            var hidden = new bool[length];
            var literal = new bool[length];
            var bold = new int[length];
            var italic = new int[length];

            foreach (Match match in DescriptionLinkParser.LinkPattern.Matches(text))
            {
                for (int i = match.Index; i < match.Index + match.Length; i++)
                    literal[i] = true;
            }

            for (int i = 0; i < length - 1; i++)
            {
                if (literal[i] ||
                    text[i] != Escape ||
                    !IsMarker(text[i + 1]))
                {
                    continue;
                }

                hidden[i] = true;
                literal[i + 1] = true;

                i++;
            }

            var openers = new List<Delimiter>();

            int index = 0;

            while (index < length)
            {
                char character = text[index];

                if (character == '\n')
                {
                    openers.Clear();

                    index++;

                    continue;
                }

                if (literal[index] ||
                    !IsMarker(character))
                {
                    index++;

                    continue;
                }

                int start = index;

                while (index < length &&
                       text[index] == character &&
                       !literal[index])
                {
                    index++;
                }

                Delimiter delimiter =
                    CreateDelimiter(
                        text,
                        character,
                        start,
                        index - start);

                if (delimiter.CanClose)
                {
                    Close(
                        delimiter,
                        openers,
                        hidden,
                        bold,
                        italic);
                }

                if (delimiter.Count > 0 &&
                    delimiter.CanOpen)
                {
                    openers.Add(delimiter);
                }
            }

            return BuildRuns(
                text,
                hidden,
                bold,
                italic);
        }
        #endregion

        #region Helpers
        private static bool IsMarker(char character)
        {
            return character == '*' ||
                   character == '_';
        }

        private static bool IsPunctuation(char character)
        {
            return char.IsPunctuation(character) ||
                   char.IsSymbol(character);
        }

        private static Delimiter CreateDelimiter(
            string text,
            char marker,
            int start,
            int count)
        {
            int end = start + count;

            // Text start/end count as whitespace.
            char previous = start > 0 ? text[start - 1] : '\n';
            char next = end < text.Length ? text[end] : '\n';

            bool previousWhite = char.IsWhiteSpace(previous);
            bool nextWhite = char.IsWhiteSpace(next);
            bool previousPunctuation = IsPunctuation(previous);
            bool nextPunctuation = IsPunctuation(next);

            bool leftFlanking =
                !nextWhite &&
                (!nextPunctuation || previousWhite || previousPunctuation);

            bool rightFlanking =
                !previousWhite &&
                (!previousPunctuation || nextWhite || nextPunctuation);

            bool canOpen;
            bool canClose;

            if (marker == '*')
            {
                canOpen = leftFlanking;
                canClose = rightFlanking;
            }
            else
            {
                // "_" inside a word (snake_case) neither opens nor closes.
                canOpen = leftFlanking && (!rightFlanking || previousPunctuation);
                canClose = rightFlanking && (!leftFlanking || nextPunctuation);
            }

            return new Delimiter
            {
                Marker = marker,
                Start = start,
                Count = count,
                OriginalCount = count,
                CanOpen = canOpen,
                CanClose = canClose
            };
        }

        // Matches the closer with the nearest fitting opener (repeatedly while markers are left):
        // two markers on both sides = bold, otherwise one = italic.
        private static void Close(
            Delimiter closer,
            List<Delimiter> openers,
            bool[] hidden,
            int[] bold,
            int[] italic)
        {
            while (closer.Count > 0)
            {
                int openerIndex = FindOpener(closer, openers);

                if (openerIndex < 0)
                    return;

                Delimiter opener = openers[openerIndex];

                int use =
                    opener.Count >= 2 && closer.Count >= 2
                        ? 2
                        : 1;

                // The opener uses its inner (last) markers, the closer its inner (first) ones.
                int contentStart = opener.Start + opener.Count;
                int contentEnd = closer.Start;

                for (int i = contentStart - use; i < contentStart; i++)
                    hidden[i] = true;

                for (int i = contentEnd; i < contentEnd + use; i++)
                    hidden[i] = true;

                int[] depth =
                    use == 2
                        ? bold
                        : italic;

                for (int i = contentStart; i < contentEnd; i++)
                    depth[i]++;

                opener.Count -= use;

                closer.Start += use;
                closer.Count -= use;

                // Openers inside the emphasis can no longer match anything after it.
                openers.RemoveRange(
                    openerIndex + 1,
                    openers.Count - openerIndex - 1);

                if (opener.Count == 0)
                    openers.RemoveAt(openerIndex);
            }
        }

        private static int FindOpener(
            Delimiter closer,
            List<Delimiter> openers)
        {
            for (int i = openers.Count - 1; i >= 0; i--)
            {
                Delimiter opener = openers[i];

                if (opener.Marker != closer.Marker)
                    continue;

                // CommonMark "multiple of 3" rule: keeps "*a**b*" from pairing the wrong markers.
                bool eitherBoth =
                    opener.CanClose ||
                    closer.CanOpen;

                bool multipleOfThree =
                    (opener.OriginalCount + closer.OriginalCount) % 3 == 0 &&
                    !(opener.OriginalCount % 3 == 0 && closer.OriginalCount % 3 == 0);

                if (eitherBoth && multipleOfThree)
                    continue;

                return i;
            }

            return -1;
        }

        private static List<DescriptionRun> BuildRuns(
            string text,
            bool[] hidden,
            int[] bold,
            int[] italic)
        {
            var runs = new List<DescriptionRun>();
            var builder = new StringBuilder();

            bool runBold = false;
            bool runItalic = false;

            for (int i = 0; i < text.Length; i++)
            {
                if (hidden[i])
                    continue;

                bool isBold = bold[i] > 0;
                bool isItalic = italic[i] > 0;

                if (builder.Length > 0 &&
                    (isBold != runBold || isItalic != runItalic))
                {
                    runs.Add(
                        new DescriptionRun(
                            builder.ToString(),
                            runBold,
                            runItalic));

                    builder.Clear();
                }

                runBold = isBold;
                runItalic = isItalic;

                builder.Append(text[i]);
            }

            if (builder.Length > 0)
            {
                runs.Add(
                    new DescriptionRun(
                        builder.ToString(),
                        runBold,
                        runItalic));
            }

            return runs;
        }
        #endregion
    }
}
