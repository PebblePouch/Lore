using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PebblePouch.Lore.Editor
{
    // Translates description text between the stored and the editable link form.
    //   Stored:   [[id|LastKnownTitle]]  (rename-safe; never shown to the user)
    //   Editable: [[Title]] / [[Category/Title]]
    // ToEditable runs when editing starts, ToStored when the text is handed back for saving.
    public static class DescriptionLinkFormat
    {
        #region Fields
        // Same pattern as the runtime parser (one definition).
        private static Regex LinkPattern => DescriptionLinkParser.LinkPattern;
        #endregion

        #region Public API
        // brokenLinkIds collects editable label -> ID for links whose Entry is missing,
        // so saving does not drop their ID (the Entry may come back, e.g. via version control).
        public static string ToEditable(
            string stored,
            Dictionary<string, string> brokenLinkIds)
        {
            brokenLinkIds?.Clear();

            if (!DescriptionLinkParser.ContainsLinks(stored))
                return stored ?? string.Empty;

            return LinkPattern.Replace(
                stored,
                match =>
                {
                    string target = match.Groups[1].Value;

                    if (!DescriptionLinkResolver.TryParsePinned(
                            target,
                            out string id,
                            out string label))
                    {
                        return match.Value;
                    }

                    if (LoreRepository.Instance.TryGetEntry(
                            id,
                            out Entry entry))
                    {
                        return Wrap(
                            DescriptionLinkResolver.GetEditableTarget(
                                entry));
                    }

                    if (brokenLinkIds != null &&
                        !string.IsNullOrEmpty(label))
                    {
                        brokenLinkIds[label] = id;
                    }

                    return Wrap(label);
                });
        }

        public static string ToStored(
            string editable,
            IReadOnlyDictionary<string, string> brokenLinkIds)
        {
            if (!DescriptionLinkParser.ContainsLinks(editable))
                return editable ?? string.Empty;

            return LinkPattern.Replace(
                editable,
                match =>
                {
                    string target = match.Groups[1].Value.Trim();

                    // Already stored form (e.g. pasted raw text): refresh its label if it resolves.
                    if (DescriptionLinkResolver.TryParsePinned(
                            target,
                            out string pinnedId,
                            out _))
                    {
                        return LoreRepository.Instance.TryGetEntry(
                            pinnedId,
                            out Entry pinnedEntry)
                            ? Wrap(DescriptionLinkResolver.GetStoredTarget(pinnedEntry))
                            : match.Value;
                    }

                    Entry entry =
                        DescriptionLinkResolver.ResolveTitle(
                            target);

                    if (entry != null)
                    {
                        return Wrap(
                            DescriptionLinkResolver.GetStoredTarget(
                                entry));
                    }

                    // Still broken: keep the ID it had, if any.
                    if (brokenLinkIds != null &&
                        brokenLinkIds.TryGetValue(
                            target,
                            out string brokenId))
                    {
                        return Wrap($"{brokenId}{DescriptionLinkResolver.IdSeparator}{target}");
                    }

                    // Unknown title: stays a plain link and gets pinned once such an Entry exists.
                    return match.Value;
                });
        }
        #endregion

        #region Helpers
        private static string Wrap(string target)
        {
            return $"{DescriptionLinkParser.LinkOpen}{target}{DescriptionLinkParser.LinkClose}";
        }
        #endregion
    }
}
