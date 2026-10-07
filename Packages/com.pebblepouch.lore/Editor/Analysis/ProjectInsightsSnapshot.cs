using System.Collections.Generic;

namespace PebblePouch.Lore.Editor
{
    public sealed class ProjectInsightsSnapshot
    {
        #region Properties
        public ProjectRelationshipReport RelationshipReport { get; }
        public LibraryUsageReport LibraryUsage { get; }
        public ContentHealthReport ContentHealth { get; }
        public IReadOnlyList<EntryFieldCoverage> EntryFieldCoverage { get; }

        public IReadOnlyList<CategoryReference> CategoryRelationships { get; }

        public IReadOnlyList<ProjectEntryHub> MostReferencedEntries { get; }
        public IReadOnlyList<ProjectEntryHub> MostReferencingEntries { get; }

        public int MissingFieldCount { get; }
        public int MissingReferenceCount { get; }
        public int MissingUnityReferenceCount { get; }

        // ScriptableObject Fields (used by Categories) whose type cannot be resolved.
        public int MissingObjectTypeCount { get; }

        // All descriptions (Project, Categories, Entries) and the Project description alone.
        public int BrokenDescriptionLinkCount { get; }
        public int ProjectBrokenDescriptionLinkCount { get; }
        #endregion

        #region Constructor
        public ProjectInsightsSnapshot(
            ProjectRelationshipReport relationshipReport,
            LibraryUsageReport libraryUsage,
            ContentHealthReport contentHealth,
            IReadOnlyList<EntryFieldCoverage> entryFieldCoverage,
            IReadOnlyList<CategoryReference> categoryRelationships,
            IReadOnlyList<ProjectEntryHub> mostReferencedEntries,
            IReadOnlyList<ProjectEntryHub> mostReferencingEntries,
            int missingFieldCount,
            int missingReferenceCount,
            int missingUnityReferenceCount,
            int missingObjectTypeCount,
            int brokenDescriptionLinkCount,
            int projectBrokenDescriptionLinkCount)
        {
            RelationshipReport = relationshipReport;
            LibraryUsage = libraryUsage;
            ContentHealth = contentHealth;
            EntryFieldCoverage = entryFieldCoverage;
            CategoryRelationships = categoryRelationships;
            MostReferencedEntries = mostReferencedEntries;
            MostReferencingEntries = mostReferencingEntries;
            MissingFieldCount = missingFieldCount;
            MissingReferenceCount = missingReferenceCount;
            MissingUnityReferenceCount = missingUnityReferenceCount;
            MissingObjectTypeCount = missingObjectTypeCount;
            BrokenDescriptionLinkCount = brokenDescriptionLinkCount;
            ProjectBrokenDescriptionLinkCount = projectBrokenDescriptionLinkCount;
        }
        #endregion
    }
}