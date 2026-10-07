using System;
using System.Collections.Generic;
using System.Linq;

namespace PebblePouch.Lore.Editor
{
    // Broken description links: repair candidates and rewriting. Never repairs on its own:
    // the user picks a candidate (or removes the link) in the link menu of DescriptionTextFlow.
    public static class DescriptionLinkRepair
    {
        #region Fields
        private const int MaxCandidates = 5;

        // Typos up to this many characters count as similar; shorter titles only match exactly (too much noise).
        private const int MaxTypoDistance = 2;
        private const int MinSimilarTitleLength = 3;
        #endregion

        #region Public API
        // Candidates for a broken link target, best first: same title (ignoring case),
        // then typos (fewer differences first), then titles containing the link's title.
        public static List<Entry> FindCandidates(string target)
        {
            string title = GetTitle(target);

            if (string.IsNullOrEmpty(title))
                return new List<Entry>();

            return LoreRepository.Instance.Entries
                .Where(entry => entry != null)
                .Select(entry => (Entry: entry, Score: GetScore(entry.Title, title)))
                .Where(candidate => candidate.Score >= 0)
                .OrderBy(candidate => candidate.Score)
                .ThenBy(candidate => candidate.Entry.Title, StringComparer.OrdinalIgnoreCase)
                .Take(MaxCandidates)
                .Select(candidate => candidate.Entry)
                .ToList();
        }

        public static bool HasCandidates(string target)
        {
            string title = GetTitle(target);

            return !string.IsNullOrEmpty(title) &&
                   LoreRepository.Instance.Entries.Any(
                       entry =>
                           entry != null &&
                           GetScore(entry.Title, title) >= 0);
        }

        // The link becomes a pinned (rename-safe) link to the chosen Entry.
        public static string LinkTo(
            string text,
            int linkIndex,
            Entry entry)
        {
            return DescriptionLinkParser.ReplaceLinkAt(
                text,
                linkIndex,
                DescriptionLinkParser.LinkOpen
                + DescriptionLinkResolver.GetStoredTarget(entry)
                + DescriptionLinkParser.LinkClose);
        }

        // Keeps the text the user sees for the broken link, without the link.
        public static string RemoveLink(
            string text,
            int linkIndex,
            string target)
        {
            return DescriptionLinkParser.ReplaceLinkAt(
                text,
                linkIndex,
                DescriptionLinkResolver.GetLabel(target));
        }
        #endregion

        #region Helpers
        // Pinned "id|Title": the last known title. Unpinned "Category/Title" or "Title": the title.
        private static string GetTitle(string target)
        {
            string label =
                DescriptionLinkResolver.GetLabel(
                    target);

            int separatorIndex =
                label.IndexOf(
                    DescriptionLinkResolver.CategorySeparator);

            return separatorIndex >= 0
                ? label.Substring(separatorIndex + 1).Trim()
                : label.Trim();
        }

        // -1 = no candidate, 0 = same title, 1..MaxTypoDistance = typo, MaxTypoDistance + 1 = contains.
        private static int GetScore(
            string candidate,
            string title)
        {
            if (string.IsNullOrEmpty(candidate))
                return -1;

            if (string.Equals(candidate, title, StringComparison.OrdinalIgnoreCase))
                return 0;

            if (title.Length < MinSimilarTitleLength)
                return -1;

            int distance =
                GetDistance(
                    candidate.ToLowerInvariant(),
                    title.ToLowerInvariant());

            if (distance <= MaxTypoDistance)
                return distance;

            return candidate.IndexOf(title, StringComparison.OrdinalIgnoreCase) >= 0
                ? MaxTypoDistance + 1
                : -1;
        }

        // Edit distance (insert, delete, replace one character).
        private static int GetDistance(
            string a,
            string b)
        {
            // Lengths differing by more than the limit can never be within it.
            if (Math.Abs(a.Length - b.Length) > MaxTypoDistance)
                return MaxTypoDistance + 1;

            var previous = new int[b.Length + 1];
            var current = new int[b.Length + 1];

            for (int j = 0; j <= b.Length; j++)
                previous[j] = j;

            for (int i = 1; i <= a.Length; i++)
            {
                current[0] = i;

                for (int j = 1; j <= b.Length; j++)
                {
                    int cost = a[i - 1] == b[j - 1] ? 0 : 1;

                    current[j] =
                        Math.Min(
                            Math.Min(
                                current[j - 1] + 1,
                                previous[j] + 1),
                            previous[j - 1] + cost);
                }

                (previous, current) = (current, previous);
            }

            return previous[b.Length];
        }
        #endregion
    }
}
