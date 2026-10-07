using System;
using System.Linq;

namespace PebblePouch.Lore.Editor
{
    // Resolves a link target to an Entry (target forms: see DescriptionLinkParser).
    // Pinned targets resolve by ID (survive renames anywhere); title targets resolve to the first Entry with
    // that title in browser order, or in the given Category. Users only ever see the editable form; see DescriptionLinkFormat.
    public static class DescriptionLinkResolver
    {
        #region Fields
        public const char CategorySeparator = DescriptionLinkParser.CategorySeparator;
        public const char IdSeparator = DescriptionLinkParser.IdSeparator;
        #endregion

        #region Public API
        public static Entry Resolve(string target)
        {
            if (string.IsNullOrWhiteSpace(target))
                return null;

            if (TryParsePinned(
                    target,
                    out string id,
                    out _))
            {
                // Strict: a pinned link never falls back to a title match (that could be a different Entry).
                return LoreRepository.Instance.TryGetEntry(
                    id,
                    out Entry entry)
                    ? entry
                    : null;
            }

            return ResolveTitle(target);
        }

        // Text to show for a target whose Entry cannot be resolved.
        public static string GetLabel(string target) => DescriptionLinkParser.GetLabel(target);

        // "Title", or "Category/Title" when another Entry shares the title.
        public static string GetEditableTarget(Entry entry)
        {
            bool isAmbiguous =
                LoreRepository.Instance.Entries
                    .Any(
                        other =>
                            other != null &&
                            other != entry &&
                            Matches(
                                other.Title,
                                entry.Title));

            if (!isAmbiguous ||
                entry.Category == null)
            {
                return entry.Title;
            }

            return $"{entry.Category.Title}{CategorySeparator}{entry.Title}";
        }

        public static string GetStoredTarget(Entry entry)
        {
            return $"{entry.Id}{IdSeparator}{entry.Title}";
        }

        public static bool TryParsePinned(
            string target,
            out string id,
            out string label)
        {
            return DescriptionLinkParser.TryParsePinned(
                target,
                out id,
                out label);
        }

        public static Entry ResolveTitle(string target)
        {
            if (string.IsNullOrWhiteSpace(target))
                return null;

            DescriptionLinkParser.SplitTitleTarget(
                target,
                out string categoryTitle,
                out string entryTitle);

            foreach (CategoryEntryGroup group in LoreRepository.Instance.Groups)
            {
                if (categoryTitle != null &&
                    !Matches(group.Category?.Title, categoryTitle))
                {
                    continue;
                }

                foreach (Entry entry in group.Entries)
                {
                    if (entry != null &&
                        Matches(entry.Title, entryTitle))
                    {
                        return entry;
                    }
                }
            }

            return null;
        }
        #endregion

        #region Helpers
        private static bool Matches(
            string value,
            string search)
        {
            return string.Equals(
                value,
                search,
                StringComparison.OrdinalIgnoreCase);
        }
        #endregion
    }
}
