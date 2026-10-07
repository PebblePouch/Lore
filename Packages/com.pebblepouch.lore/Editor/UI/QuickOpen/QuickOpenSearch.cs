using System;
using System.Collections.Generic;
using System.Linq;

using Object = UnityEngine.Object;

namespace PebblePouch.Lore.Editor
{
    // Quick Open results: Entries, Categories and Fields whose title matches the query, best matches first.
    // Matching (case-insensitive): title starts with the query > a word starts with it > contains it >
    // word initials ("pf" → "Prince Floris") > all letters in order ("wlf" → "Wolf").
    public static class QuickOpenSearch
    {
        #region Types
        public readonly struct Result
        {
            public readonly Object Target;
            public readonly string Glyph;
            public readonly string Title;
            public readonly string Detail;
            public readonly int Score;

            public Result(
                Object target,
                string glyph,
                string title,
                string detail,
                int score)
            {
                Target = target;
                Glyph = glyph;
                Title = title;
                Detail = detail;
                Score = score;
            }
        }
        #endregion

        #region Public API
        public static List<Result> Find(
            string query,
            int maxResults)
        {
            var results = new List<Result>();

            query = query?.Trim();

            if (string.IsNullOrEmpty(query))
                return results;

            LoreRepository repository = LoreRepository.Instance;

            foreach (Entry entry in repository.Entries)
            {
                if (entry == null)
                    continue;

                Add(
                    results,
                    entry,
                    entry.Glyph,
                    entry.Title,
                    entry.Category != null
                        ? entry.Category.Title
                        : LoreText.Common.Uncategorized,
                    query);
            }

            foreach (Category category in repository.Categories)
            {
                if (category == null)
                    continue;

                Add(
                    results,
                    category,
                    category.Glyph,
                    category.Title,
                    LoreText.QuickOpen.CategoryDetail,
                    query);
            }

            foreach (FieldLibraryItem item in new FieldLibrarySource().GetItems())
            {
                Field field = item.Field;

                if (field == null)
                    continue;

                Add(
                    results,
                    field,
                    field.Glyph,
                    field.Title,
                    string.Format(
                        LoreText.QuickOpen.FieldDetail,
                        ScriptableObjectTypeService.GetFieldTypeLabel(field)),
                    query);
            }

            return results
                .OrderByDescending(result => result.Score)
                .ThenBy(result => result.Title, StringComparer.OrdinalIgnoreCase)
                .Take(maxResults)
                .ToList();
        }

        // Higher = better; -1 = no match.
        public static int Score(
            string text,
            string query)
        {
            if (string.IsNullOrEmpty(text) ||
                string.IsNullOrEmpty(query))
            {
                return -1;
            }

            int index =
                text.IndexOf(
                    query,
                    StringComparison.OrdinalIgnoreCase);

            if (index == 0)
                return 1000 - (text.Length - query.Length);

            if (index > 0)
            {
                bool wordStart = !char.IsLetterOrDigit(text[index - 1]);

                return (wordStart ? 800 : 600) - index;
            }

            string initials =
                new string(
                    text
                        .Split(' ', '-', '_')
                        .Where(word => word.Length > 0)
                        .Select(word => word[0])
                        .ToArray());

            if (initials.StartsWith(query, StringComparison.OrdinalIgnoreCase))
                return 500;

            // All query letters in order; fewer gaps = better.
            int textIndex = 0;
            int lastMatch = -1;
            int gaps = 0;

            foreach (char character in query)
            {
                char lower = char.ToLowerInvariant(character);

                while (textIndex < text.Length &&
                       char.ToLowerInvariant(text[textIndex]) != lower)
                {
                    textIndex++;
                }

                if (textIndex >= text.Length)
                    return -1;

                if (lastMatch >= 0)
                    gaps += textIndex - lastMatch - 1;

                lastMatch = textIndex;
                textIndex++;
            }

            return Math.Max(1, 300 - gaps);
        }
        #endregion

        #region Helpers
        private static void Add(
            List<Result> results,
            Object target,
            string glyph,
            string title,
            string detail,
            string query)
        {
            int score =
                Score(
                    title,
                    query);

            if (score < 0)
                return;

            results.Add(
                new Result(
                    target,
                    glyph,
                    title,
                    detail,
                    score));
        }
        #endregion
    }
}
