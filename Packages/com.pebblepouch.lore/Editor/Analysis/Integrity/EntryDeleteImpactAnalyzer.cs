using System.Collections.Generic;

namespace PebblePouch.Lore.Editor
{
    // Counts the references that break when Entries are deleted: Entry/Entries Field values and description
    // [[links]] (Project, Categories, Entries) that resolve to one of them. References between the deleted
    // Entries themselves are not counted (they are deleted too). Shown in the delete confirmations.
    public sealed class EntryDeleteImpactAnalyzer
    {
        #region Public API
        public void Count(
            IReadOnlyCollection<Entry> entries,
            out int fieldReferences,
            out int descriptionLinks)
        {
            fieldReferences = 0;
            descriptionLinks = 0;

            var deleted = new HashSet<Entry>();

            foreach (Entry entry in entries)
            {
                if (entry != null)
                    deleted.Add(entry);
            }

            if (deleted.Count == 0)
                return;

            foreach (Entry target in deleted)
            {
                foreach (EntryReference reference in
                         LoreRelationshipIndex.Instance.GetReferencesTo(target))
                {
                    if (reference.SourceEntry != null &&
                        !deleted.Contains(reference.SourceEntry))
                    {
                        fieldReferences++;
                    }
                }
            }

            LoreRepository repository = LoreRepository.Instance;

            if (repository.Project != null)
                descriptionLinks += CountLinks(repository.Project.Description, deleted);

            foreach (Category category in repository.Categories)
            {
                if (category != null)
                    descriptionLinks += CountLinks(category.Description, deleted);
            }

            foreach (Entry entry in repository.Entries)
            {
                if (entry != null &&
                    !deleted.Contains(entry))
                {
                    descriptionLinks += CountLinks(entry.Description, deleted);
                }
            }
        }

        // The confirmation paragraph: what breaks, or that nothing references the Entries.
        public string Describe(IReadOnlyCollection<Entry> entries)
        {
            Count(
                entries,
                out int fieldReferences,
                out int descriptionLinks);

            var parts = new List<string>();

            if (fieldReferences > 0)
                parts.Add(LoreText.Dialogs.DeleteFieldReferences.Format(fieldReferences));

            if (descriptionLinks > 0)
                parts.Add(LoreText.Dialogs.DeleteDescriptionLinks.Format(descriptionLinks));

            return parts.Count == 0
                ? LoreText.Dialogs.DeleteNoReferences.Format(entries.Count)
                : string.Format(
                    LoreText.Dialogs.DeleteBreaksReferences,
                    LoreText.Common.JoinList(parts));
        }
        #endregion

        #region Helpers
        private static int CountLinks(
            string description,
            HashSet<Entry> deleted)
        {
            if (!DescriptionLinkParser.ContainsLinks(description))
                return 0;

            int count = 0;

            foreach (DescriptionSegment segment in
                     DescriptionLinkParser.Parse(
                         description))
            {
                if (segment.IsLink)
                {
                    Entry target = DescriptionLinkResolver.Resolve(segment.Text);

                    if (target != null &&
                        deleted.Contains(target))
                    {
                        count++;
                    }
                }
            }

            return count;
        }
        #endregion
    }
}
