using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PebblePouch.Lore
{
    // Splits description text into plain text and [[Target]] link segments, and reads link targets.
    // Links are navigation only; they are not relationships.
    // Target forms:
    //   Stored (pinned):  "id|LastKnownTitle"
    //   Editable:         "Title" / "Category/Title"
    // '/' and '|' are not allowed in Lore names, so neither form is ever ambiguous.
    // Shared by the editor (resolving, editing, repair) and the runtime API (LoreProject descriptions).
    internal static class DescriptionLinkParser
    {
        #region Fields
        public const string LinkOpen = "[[";
        public const string LinkClose = "]]";

        public const char CategorySeparator = '/';
        public const char IdSeparator = '|';

        // Not RegexOptions.Compiled: on Mono that compiles IL at first use (a noticeable hitch when a game first
        // shows a description), and IL2CPP ignores it anyway. Descriptions are short; interpreted is fast enough.
        // Also used by the editor (DescriptionLinkFormat).
        public static readonly Regex LinkPattern =
            new(
                @"\[\[([^\[\]\r\n]+)\]\]");
        #endregion

        #region Public API
        public static bool ContainsLinks(string text)
        {
            return !string.IsNullOrEmpty(text) &&
                   text.Contains(LinkOpen);
        }

        // Replaces the whole "[[...]]" of the link at linkIndex (0 = first link in the text) with replacement.
        public static string ReplaceLinkAt(
            string text,
            int linkIndex,
            string replacement)
        {
            if (string.IsNullOrEmpty(text) ||
                linkIndex < 0)
            {
                return text;
            }

            MatchCollection matches = LinkPattern.Matches(text);

            if (linkIndex >= matches.Count)
                return text;

            Match match = matches[linkIndex];

            return text.Substring(0, match.Index)
                   + replacement
                   + text.Substring(match.Index + match.Length);
        }

        public static List<DescriptionSegment> Parse(string text)
        {
            var segments = new List<DescriptionSegment>();

            if (string.IsNullOrEmpty(text))
                return segments;

            int index = 0;

            foreach (Match match in LinkPattern.Matches(text))
            {
                if (match.Index > index)
                {
                    segments.Add(
                        new DescriptionSegment(
                            text.Substring(
                                index,
                                match.Index - index),
                            false));
                }

                segments.Add(
                    new DescriptionSegment(
                        match.Groups[1].Value.Trim(),
                        true));

                index = match.Index + match.Length;
            }

            if (index < text.Length)
            {
                segments.Add(
                    new DescriptionSegment(
                        text.Substring(index),
                        false));
            }

            return segments;
        }

        // "id|LastKnownTitle" -> id + label.
        public static bool TryParsePinned(
            string target,
            out string id,
            out string label)
        {
            id = null;
            label = null;

            if (string.IsNullOrEmpty(target))
                return false;

            int separatorIndex =
                target.IndexOf(
                    IdSeparator);

            if (separatorIndex <= 0)
                return false;

            id = target.Substring(0, separatorIndex).Trim();
            label = target.Substring(separatorIndex + 1).Trim();

            return id.Length > 0;
        }

        // "Category/Title" -> category + title; "Title" -> null + title.
        public static void SplitTitleTarget(
            string target,
            out string categoryTitle,
            out string entryTitle)
        {
            target = target?.Trim() ?? string.Empty;

            categoryTitle = null;
            entryTitle = target;

            int separatorIndex =
                target.IndexOf(
                    CategorySeparator);

            if (separatorIndex < 0)
                return;

            categoryTitle = target.Substring(0, separatorIndex).Trim();
            entryTitle = target.Substring(separatorIndex + 1).Trim();
        }

        // Text to show for a target whose Entry cannot be resolved (never the ID).
        public static string GetLabel(string target)
        {
            if (TryParsePinned(
                    target,
                    out _,
                    out string label))
            {
                return label;
            }

            return target?.Trim() ?? string.Empty;
        }
        #endregion
    }
}
