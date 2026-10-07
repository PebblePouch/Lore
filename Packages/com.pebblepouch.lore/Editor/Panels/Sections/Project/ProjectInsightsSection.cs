using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class ProjectInsightsSection : IDisposable
    {
        #region Fields
        private readonly LoreRepository _repository = LoreRepository.Instance;

        private readonly ProjectRelationshipAnalyzer _relationshipAnalyzer = new(LoreRepository.Instance);
        private readonly LibraryUsageAnalyzer _libraryUsageAnalyzer = new(LoreRepository.Instance);
        private readonly EntryFieldCoverageAnalyzer _entryFieldCoverageAnalyzer = new(LoreRepository.Instance);
        private readonly ContentHealthAnalyzer _contentHealthAnalyzer = new(LoreRepository.Instance);

        private readonly CategoryRelationshipAnalyzer _categoryRelationshipAnalyzer = new(LoreRepository.Instance);
        private readonly RelationshipQueryService _relationshipQueries = new(LoreRepository.Instance);
        private readonly RelationshipIntegrityAnalyzer _relationshipIntegrityAnalyzer = new(LoreRepository.Instance);
        private readonly UnityReferenceIntegrityAnalyzer _unityReferenceIntegrityAnalyzer = new(LoreRepository.Instance);

        private readonly CategoryIntegrityAnalyzer _categoryIntegrityAnalyzer = new();
        private readonly FieldTypeIntegrityAnalyzer _fieldTypeIntegrityAnalyzer = new();
        private readonly DescriptionLinkIntegrityAnalyzer _descriptionLinkIntegrityAnalyzer = new();
        private readonly LoreRelationshipIndex _relationshipIndex = LoreRelationshipIndex.Instance;

        private readonly IntegrityCalloutCard _calloutCard = new();

        private readonly StatisticCardFlow _categoryStatisticFlow = new();
        private readonly StatisticCardFlow _entryStatisticFlow = new();

        private readonly StatisticCardFlow _outboundStatisticFlow = new();
        private readonly StatisticCardFlow _inboundStatisticFlow = new();

        private readonly StatisticCardFlow _latestModifiedStatisticFlow = new();
        private readonly StatisticCardFlow _latestCreatedStatisticFlow = new();

        private readonly StatisticCardFlow _fieldStatisticFlow = new();
        private readonly StatisticCardFlow _tagStatisticFlow = new();
        private readonly StatisticCardFlow _colorStatisticFlow = new();
        private readonly StatisticCardFlow _glyphStatisticFlow = new();

        private readonly StatisticCardFlow _categoryReferenceStatisticFlow = new();

        private readonly StatisticCardFlow _contentHealthStatisticFlow = new();

        private readonly WorkspaceState _state;

        private readonly InsightsRowHeight _overviewRowHeight = new();
        private readonly InsightsRowHeight _latestRowHeight = new();
        private readonly InsightsRowHeight _hubsRowHeight = new();
        private readonly InsightsRowHeight _librariesRowHeight = new();
        private readonly InsightsRowHeight _detailsRowHeight = new();

        private const int HubEntryLimit = 10;
        private float _availableInsightsWidth;

        #endregion

        #region Constructor
        public ProjectInsightsSection(WorkspaceState state)
        {
            _state = state;

            LoreRefreshService.Refreshed += MarkSnapshotDirty;
            EditorApplication.projectChanged += MarkSnapshotDirty;

            _relationshipIndex.Refreshed += MarkSnapshotDirty;

            // Field value edits do not trigger a full refresh: Health (e.g. missing required values) must update too.
            LoreChangeTracker.Changed += MarkSnapshotDirty;
        }
        #endregion

        #region Public API
        // Integrity callout, drawn by the panel between header and description (prominent: should not happen).
        public void DrawIntegrity()
        {
            EnsureSnapshot();

            if (_snapshot == null)
                return;

            bool hasIntegrityIssues =
                _snapshot.MissingFieldCount > 0
                || _snapshot.MissingReferenceCount > 0
                || _snapshot.MissingUnityReferenceCount > 0
                || _snapshot.MissingObjectTypeCount > 0
                || _snapshot.BrokenDescriptionLinkCount > 0;

            if (!hasIntegrityIssues)
                return;

            DrawIntegrityCallout(
                _snapshot.MissingFieldCount,
                _snapshot.MissingReferenceCount,
                _snapshot.MissingUnityReferenceCount,
                _snapshot.MissingObjectTypeCount,
                _snapshot.BrokenDescriptionLinkCount,
                _snapshot.ProjectBrokenDescriptionLinkCount);
        }

        public void Draw()
        {
            EnsureSnapshot();

            if (_snapshot == null)
                return;

            InsightsRow.UpdateAvailableWidth(
                ref _availableInsightsWidth);

            ProjectRelationshipReport report = _snapshot.RelationshipReport;

            // Row 1: Health | Categories | Entries (compact overview; Health = the writer's to-do list)

            bool overviewExpanded =
                _state.IsInspectorSectionExpanded(
                    InspectorSection.ProjectOverview);

            Action toggleOverview =
                () =>
                    _state.ToggleInspectorSection(
                        InspectorSection.ProjectOverview);

            InsightsRow.Draw(
                _availableInsightsWidth,
                _overviewRowHeight,
                () =>
                    SectionCard.DrawRowCollapsible(
                        LoreText.Insights.ContentHealthTitle,
                        overviewExpanded,
                        toggleOverview,
                        () =>
                            DrawContentHealth(
                                _snapshot.ContentHealth),
                        InsightsStyle.Card,
                        _overviewRowHeight),

                () =>
                    SectionCard.DrawRowCollapsible(
                        LoreText.Insights.CategoriesTitle,
                        overviewExpanded,
                        toggleOverview,
                        () =>
                            DrawCategorySummary(
                                report),
                        InsightsStyle.Card,
                        _overviewRowHeight),

                () =>
                    SectionCard.DrawRowCollapsible(
                        LoreText.Insights.EntriesTitle,
                        overviewExpanded,
                        toggleOverview,
                        () =>
                            DrawEntrySummary(
                                report),
                        InsightsStyle.Card,
                        _overviewRowHeight));

            GUILayout.Space(InsightsStyle.RowSpacing);

            // Row 2: Fields | Tags | Colors | Glyphs

            bool librariesExpanded =
                _state.IsInspectorSectionExpanded(
                    InspectorSection.ProjectLibraries);

            Action toggleLibraries =
                () =>
                    _state.ToggleInspectorSection(
                        InspectorSection.ProjectLibraries);

            InsightsRow.Draw(
                _availableInsightsWidth,
                _librariesRowHeight,
                () =>
                    SectionCard.DrawRowCollapsible(
                        LoreText.Insights.FieldsTitle,
                        librariesExpanded,
                        toggleLibraries,
                        () =>
                            DrawFieldLibrarySummary(
                                _snapshot.LibraryUsage),
                        InsightsStyle.Card,
                        _librariesRowHeight),

                () =>
                    SectionCard.DrawRowCollapsible(
                        LoreText.Insights.TagsTitle,
                        librariesExpanded,
                        toggleLibraries,
                        () =>
                            DrawTagLibrarySummary(
                                _snapshot.LibraryUsage),
                        InsightsStyle.Card,
                        _librariesRowHeight),

                () =>
                    SectionCard.DrawRowCollapsible(
                        LoreText.Insights.ColorsTitle,
                        librariesExpanded,
                        toggleLibraries,
                        () =>
                            DrawColorLibrarySummary(
                                _snapshot.LibraryUsage),
                        InsightsStyle.Card,
                        _librariesRowHeight),

                () =>
                    SectionCard.DrawRowCollapsible(
                        LoreText.Insights.GlyphsTitle,
                        librariesExpanded,
                        toggleLibraries,
                        () =>
                            DrawGlyphLibrarySummary(
                                _snapshot.LibraryUsage),
                        InsightsStyle.Card,
                        _librariesRowHeight));

            GUILayout.Space(InsightsStyle.RowSpacing);

            // Row 3: Latest 10 Modified | Latest 10 Created

            EnsureLatestEntries();

            bool latestExpanded =
                _state.IsInspectorSectionExpanded(
                    InspectorSection.ProjectRecent);

            Action toggleLatest =
                () =>
                    _state.ToggleInspectorSection(
                        InspectorSection.ProjectRecent);

            InsightsRow.Draw(
                _availableInsightsWidth,
                _latestRowHeight,
                () =>
                    SectionCard.DrawRowCollapsible(
                        string.Format(LoreText.Insights.LatestModifiedEntriesTitle, HubEntryLimit),
                        latestExpanded,
                        toggleLatest,
                        () =>
                            DrawLatestEntries(
                                _latestModifiedEntries,
                                entry => entry.Modified,
                                _latestModifiedStatisticFlow),
                        InsightsStyle.Card,
                        _latestRowHeight),

                () =>
                    SectionCard.DrawRowCollapsible(
                        string.Format(LoreText.Insights.LatestCreatedEntriesTitle, HubEntryLimit),
                        latestExpanded,
                        toggleLatest,
                        () =>
                            DrawLatestEntries(
                                _latestCreatedEntries,
                                entry => entry.Created,
                                _latestCreatedStatisticFlow),
                        InsightsStyle.Card,
                        _latestRowHeight));

            GUILayout.Space(InsightsStyle.RowSpacing);

            // Row 4: Top 10 Inbound | Top 10 Outbound

            bool hubsExpanded =
                _state.IsInspectorSectionExpanded(
                    InspectorSection.ProjectHubs);

            Action toggleHubs =
                () =>
                    _state.ToggleInspectorSection(
                        InspectorSection.ProjectHubs);

            InsightsRow.Draw(
                _availableInsightsWidth,
                _hubsRowHeight,
                () =>
                    SectionCard.DrawRowCollapsible(
                        string.Format(LoreText.Insights.TopInboundEntriesTitle, HubEntryLimit),
                        hubsExpanded,
                        toggleHubs,
                        () =>
                            DrawEntryHubGroup(
                                _snapshot.MostReferencedEntries,
                                _inboundStatisticFlow),
                        InsightsStyle.Card,
                        _hubsRowHeight),

                () =>
                    SectionCard.DrawRowCollapsible(
                        string.Format(LoreText.Insights.TopOutboundEntriesTitle, HubEntryLimit),
                        hubsExpanded,
                        toggleHubs,
                        () =>
                            DrawEntryHubGroup(
                                _snapshot.MostReferencingEntries,
                                _outboundStatisticFlow),
                        InsightsStyle.Card,
                        _hubsRowHeight));

            GUILayout.Space(InsightsStyle.RowSpacing);

            // Row 5: Category References | Entry Field Coverage

            DrawProjectDetailRow(
                report,
                _snapshot.CategoryRelationships,
                _snapshot.EntryFieldCoverage);
        }
        #endregion

        #region Draw
        // Integrity callout
        private void DrawIntegrityCallout(
            int brokenFieldCount,
            int brokenEntryCount,
            int missingUnityReferenceCount,
            int missingObjectTypeCount,
            int brokenDescriptionLinkCount,
            int projectBrokenDescriptionLinkCount)
        {
            int totalIssueCount =
                brokenEntryCount +
                brokenFieldCount +
                missingUnityReferenceCount +
                missingObjectTypeCount +
                brokenDescriptionLinkCount;

            var groups =
                new List<IntegrityCalloutPiece>();

            if (brokenEntryCount > 0)
            {
                groups.Add(
                    new IntegrityCalloutPiece(
                        LoreText.Insights.BrokenEntryReferences.Format(brokenEntryCount),
                        ShowBrokenEntryReferences));
            }

            if (brokenFieldCount > 0)
            {
                groups.Add(
                    new IntegrityCalloutPiece(
                        LoreText.Insights.BrokenFieldDefinitions.Format(brokenFieldCount),
                        ShowBrokenFieldDefinitions));
            }

            if (missingUnityReferenceCount > 0)
            {
                groups.Add(
                    new IntegrityCalloutPiece(
                        LoreText.Insights.MissingUnityReferences.Format(missingUnityReferenceCount),
                        ShowMissingUnityReferences));
            }

            if (missingObjectTypeCount > 0)
            {
                groups.Add(
                    new IntegrityCalloutPiece(
                        LoreText.Insights.MissingObjectTypes.Format(missingObjectTypeCount),
                        ShowMissingObjectTypes));
            }

            if (brokenDescriptionLinkCount > 0)
            {
                // Only in the Project description: nothing to filter, the link scrolls up to it instead.
                bool affectsCategoriesOrEntries =
                    brokenDescriptionLinkCount > projectBrokenDescriptionLinkCount;

                groups.Add(
                    new IntegrityCalloutPiece(
                        LoreText.Insights.BrokenDescriptionLinks.Format(brokenDescriptionLinkCount),
                        affectsCategoriesOrEntries
                            ? (Action)ShowBrokenDescriptionLinks
                            : RevealDescription));
            }

            var pieces =
                new List<IntegrityCalloutPiece>();

            // "A", "A and B", "A, B and C" at the {0} of the sentence.
            IntegrityCalloutCard.AddTemplate(
                pieces,
                LoreText.Insights.Integrity.For(totalIssueCount),
                () =>
                {
                    for (int i = 0; i < groups.Count; i++)
                    {
                        IntegrityCalloutCard.AddListSeparator(
                            pieces,
                            i,
                            groups.Count);

                        pieces.Add(
                            groups[i]);
                    }
                });

            _calloutCard.Draw(
                pieces);
        }

        // Row 1: Categories | Entries, Row 4: Top 10 Inbound | Top 10 Outbound
        private void DrawCategorySummary(
            ProjectRelationshipReport report)
        {
            bool hasStandalone =
                report.StandaloneCategoryCount > 0;

            bool hasIsolated =
                report.IsolatedCategoryCount > 0;

            var cards =
                new List<StatisticCardItem>
                {
            new(
                LoreText.Common.CategoryCount.Format(report.CategoryCount),
                InsightsStyle.StatisticsCardDefaultColor),

            new(
                string.Format(LoreText.Insights.StandaloneCount, report.StandaloneCategoryCount),
                hasStandalone
                    ? InsightsStyle.StatisticsCardDefaultLinkColor
                    : InsightsStyle.StatisticsCardDefaultColor,
                hasStandalone
                    ? StatisticCardAction.Filter
                    : StatisticCardAction.None,
                hasStandalone
                    ? ShowStandaloneCategories
                    : null),

            new(
                string.Format(LoreText.Insights.IsolatedCount, report.IsolatedCategoryCount),
                hasIsolated
                    ? InsightsStyle.StatisticsCardDefaultLinkColor
                    : InsightsStyle.StatisticsCardDefaultColor,
                hasIsolated
                    ? StatisticCardAction.Filter
                    : StatisticCardAction.None,
                hasIsolated
                    ? ShowIsolatedCategories
                    : null)
                };

            _categoryStatisticFlow.Draw(
                cards);

            GUILayout.Space(InsightsStyle.CategoriesVerticalExpansion);
        }

        private void DrawEntrySummary(
            ProjectRelationshipReport report)
        {
            bool hasUnreferenced =
                report.UnreferencedEntryCount > 0;

            bool hasIsolated =
                report.IsolatedEntryCount > 0;

            var cards =
                new List<StatisticCardItem>
                {
            new(
                LoreText.Common.EntryCount.Format(report.EntryCount),
                InsightsStyle.StatisticsCardDefaultColor),

            new(
                string.Format(LoreText.Insights.ReferencedCount, report.ReferencedEntryCount),
                InsightsStyle.StatisticsCardDefaultColor),

            new(
                string.Format(LoreText.Insights.UnreferencedCount, report.UnreferencedEntryCount),
                hasUnreferenced
                    ? InsightsStyle.StatisticsCardDefaultLinkColor
                    : InsightsStyle.StatisticsCardDefaultColor,
                hasUnreferenced
                    ? StatisticCardAction.Filter
                    : StatisticCardAction.None,
                hasUnreferenced
                    ? () => ShowUnreferencedEntries(report)
                    : null),

            new(
                string.Format(LoreText.Insights.IsolatedCount, report.IsolatedEntryCount),
                hasIsolated
                    ? InsightsStyle.StatisticsCardDefaultLinkColor
                    : InsightsStyle.StatisticsCardDefaultColor,
                hasIsolated
                    ? StatisticCardAction.Filter
                    : StatisticCardAction.None,
                hasIsolated
                    ? ShowIsolatedEntries
                    : null)
                };

            _entryStatisticFlow.Draw(
                cards);

            GUILayout.Space(InsightsStyle.EntriesVerticalExpansion);
        }

        // Row 3: Latest 10 Modified | Latest 10 Created (same cards as Top 10; date shown dimmed, local time)
        private static void DrawLatestEntries(
            IReadOnlyList<Entry> entries,
            Func<Entry, DateTime> getDate,
            StatisticCardFlow flow)
        {
            if (entries == null ||
                entries.Count == 0)
            {
                GUILayout.Label(
                    LoreText.Insights.NoEntries,
                    InsightsStyle.StatisticsText,
                    GUILayout.MinWidth(0f));

                return;
            }

            var cards =
                new List<StatisticCardItem>();

            foreach (Entry entry in entries)
            {
                Entry captured =
                    entry;

                cards.Add(
                    new StatisticCardItem(
                        entry.Title,
                        entry.Category != null
                            ? entry.Category.Color
                            : InsightsStyle.StatisticsCardDefaultLinkColor,
                        StatisticCardAction.Selection,
                        () =>
                            LoreNavigation.NavigateToEntry(
                                captured),
                        entry.Glyph,
                        getDate(entry)
                            .ToLocalTime()
                            .ToString(
                                LoreText.Insights.LatestEntryDateFormat,
                                System.Globalization.CultureInfo.InvariantCulture)));
            }

            flow.Draw(
                cards);
        }

        private void DrawEntryHubGroup(
            IReadOnlyList<ProjectEntryHub> entries,
            StatisticCardFlow flow)
        {
            if (entries == null ||
                entries.Count == 0)
            {
                GUILayout.Label(
                    LoreText.Insights.NoEntryRelationships,
                    InsightsStyle.StatisticsText,
                    GUILayout.MinWidth(0f));

                return;
            }

            var cards =
                new List<StatisticCardItem>();

            foreach (ProjectEntryHub hub in entries)
            {
                if (hub?.Entry == null)
                    continue;

                Entry entry =
                    hub.Entry;

                Color color =
                    entry.Category != null
                        ? entry.Category.Color
                        : InsightsStyle.StatisticsCardDefaultLinkColor;

                cards.Add(
                    new StatisticCardItem(
                        entry.Title,
                        color,
                        StatisticCardAction.Selection,
                        () =>
                            LoreNavigation.NavigateToEntry(
                                entry),
                        entry.Glyph,
                        string.Format(LoreText.Common.Count, hub.ReferenceCount)));
            }

            flow.Draw(
                cards);
        }

        // Content Health: one card per check; findings filter the Content browser,
        // checks without findings are dimmed with a check mark.
        private void DrawContentHealth(
            ContentHealthReport report)
        {
            var cards =
                new List<StatisticCardItem>
                {
                    CreateContentHealthCard(
                        LoreText.Insights.EntriesWithoutDescription,
                        report.EntriesWithoutDescriptionCount,
                        ShowEntriesWithoutDescription),

                    CreateContentHealthCard(
                        LoreText.Insights.CategoriesWithoutDescription,
                        report.CategoriesWithoutDescriptionCount,
                        ShowCategoriesWithoutDescription),

                    CreateContentHealthCard(
                        LoreText.Insights.EmptyCategories,
                        report.EmptyCategoryCount,
                        ShowEmptyCategories),

                    CreateContentHealthCard(
                        LoreText.Insights.PlaceholderNames,
                        report.PlaceholderNameCount,
                        ShowPlaceholderNames)
                };

            if (report.HasTags)
            {
                cards.Add(
                    CreateContentHealthCard(
                        LoreText.Insights.EntriesWithoutTags,
                        report.EntriesWithoutTagsCount,
                        ShowEntriesWithoutTags));
            }

            if (report.HasRequiredFields)
            {
                cards.Add(
                    CreateContentHealthCard(
                        LoreText.Insights.EntriesMissingRequired,
                        report.EntriesMissingRequiredCount,
                        ShowEntriesMissingRequired));
            }

            _contentHealthStatisticFlow.Draw(
                cards);
        }

        private static StatisticCardItem CreateContentHealthCard(
            LorePlural label,
            int count,
            Action show)
        {
            bool hasFindings =
                count > 0;

            return new StatisticCardItem(
                label.Format(count),
                hasFindings
                    ? InsightsStyle.StatisticsCardDefaultLinkColor
                    : InsightsStyle.StatisticsCardDefaultColor,
                hasFindings
                    ? StatisticCardAction.Filter
                    : StatisticCardAction.None,
                hasFindings
                    ? show
                    : null,
                hasFindings
                    ? null
                    : InsightsStyle.ContentHealthDoneGlyph);
        }

        // Row 2: Fields | Tags | Colors | Glyphs
        private static void DrawLibraryUsage(
            StatisticCardFlow flow,
            LorePlural totalLabel,
            int total,
            int used,
            int unused,
            Action showUnused = null)
        {
            bool canShowUnused =
                unused > 0 &&
                showUnused != null;

            var cards =
                new List<StatisticCardItem>
                {
            new(
                totalLabel.Format(total),
                InsightsStyle.StatisticsCardDefaultColor),

            new(
                string.Format(LoreText.Insights.UsedCount, used),
                InsightsStyle.StatisticsCardDefaultColor),

            new(
                string.Format(LoreText.Insights.UnusedCount, unused),
                canShowUnused
                    ? InsightsStyle.StatisticsCardDefaultLinkColor
                    : InsightsStyle.StatisticsCardDefaultColor,
                canShowUnused
                    ? StatisticCardAction.Filter
                    : StatisticCardAction.None,
                canShowUnused
                    ? showUnused
                    : null)
                };

            flow.Draw(
                cards);
        }

        private void DrawFieldLibrarySummary(
            LibraryUsageReport report)
        {
            DrawLibraryUsage(
                _fieldStatisticFlow,
                LoreText.Insights.FieldCount,
                report.FieldCount,
                report.UsedFieldCount,
                report.UnusedFieldCount,
                () =>
                    LibraryService
                        .OpenUnusedFieldManager?
                        .Invoke());
        }

        private void DrawTagLibrarySummary(
            LibraryUsageReport report)
        {
            DrawLibraryUsage(
                _tagStatisticFlow,
                LoreText.Insights.TagCount,
                report.TagCount,
                report.UsedTagCount,
                report.UnusedTagCount,
                () =>
                    LibraryService
                        .OpenUnusedTagManager?
                        .Invoke());
        }

        private void DrawColorLibrarySummary(
            LibraryUsageReport report)
        {
            // Change once custom glyphs are added,
            // then limit unused search to custom items.
            DrawLibraryUsage(
                _colorStatisticFlow,
                LoreText.Insights.ColorCount,
                report.ColorCount,
                report.UsedColorCount,
                report.UnusedColorCount);
        }

        private void DrawGlyphLibrarySummary(
            LibraryUsageReport report)
        {
            // Change once custom glyphs are added,
            // then limit unused search to custom items.
            DrawLibraryUsage(
                _glyphStatisticFlow,
                LoreText.Insights.GlyphCount,
                report.GlyphCount,
                report.UsedGlyphCount,
                report.UnusedGlyphCount);
        }

        // Row 5: Category References | Entry Field Coverage
        private void DrawProjectDetailRow(
            ProjectRelationshipReport report,
            IReadOnlyList<CategoryReference> categoryRelationships,
            IReadOnlyList<EntryFieldCoverage> entryFieldCoverage)
        {
            bool expanded =
                _state.IsInspectorSectionExpanded(
                    InspectorSection.ProjectDetails);

            Action toggle =
                () =>
                    _state.ToggleInspectorSection(
                        InspectorSection.ProjectDetails);

            InsightsRow.Draw(
                _availableInsightsWidth,
                _detailsRowHeight,
                () =>
                    SectionCard.DrawRowCollapsible(
                        LoreText.Insights.CategoryReferencesTitle,
                        expanded,
                        toggle,
                        () =>
                            DrawCategoryRelationships(
                                report,
                                categoryRelationships),
                        InsightsStyle.Card,
                        _detailsRowHeight),

                () =>
                    SectionCard.DrawRowCollapsible(
                        LoreText.Insights.EntryFieldCoverageTitle,
                        expanded,
                        toggle,
                        () =>
                            DrawEntryFieldCoverage(
                                entryFieldCoverage),
                        InsightsStyle.Card,
                        _detailsRowHeight));
        }

        // Row 5: Category References | Entry Field Coverage
        private void DrawCategoryRelationships(
            ProjectRelationshipReport report,
            IReadOnlyList<CategoryReference> references)
        {
            GUILayout.Label(
                LoreText.Insights.TotalReferenceCount.Format(report.ReferenceCount),
                InsightsStyle.StatisticsText,
                GUILayout.MinWidth(0f));

            if (references == null ||
                references.Count == 0)
            {
                return;
            }

            GUILayout.Space(
                InsightsStyle.Paragraph);

            for (int i = 0;
                 i < references.Count;
                 i++)
            {
                CategoryReference reference =
                    references[i];

                if (reference?.SourceCategory == null ||
                    reference.TargetCategory == null)
                {
                    continue;
                }

                DrawCategoryRelationship(
                    reference);

                DrawCategoryRelationshipFields(
                    reference);

                if (i < references.Count - 1)
                {
                    GUILayout.Space(
                        InsightsStyle.Paragraph);
                }
            }
        }

        private void DrawCategoryRelationship(
    CategoryReference reference)
        {
            var item =
                new StatisticCardItem(
                    string.Format(
                        LoreText.Common.CategoryRelation,
                        reference.SourceCategory.Title,
                        reference.TargetCategory.Title),
                    InsightsStyle.StatisticsCardDefaultLinkColor,
                    StatisticCardAction.Filter,
                    () =>
                        ShowCategoryRelationship(
                            reference),
                    metadata: string.Format(LoreText.Common.Count, reference.ReferenceCount));

            StatisticCard.Draw(
                item);
        }

        private void DrawCategoryRelationshipFields(
            CategoryReference reference)
        {
            if (reference?.Fields == null ||
                reference.Fields.Count == 0)
            {
                return;
            }

            foreach (CategoryReferenceField fieldReference in
                     reference.Fields)
            {
                if (fieldReference?.Field == null)
                    continue;

                GUILayout.Space(
                    InsightsStyle.StatisticCardSpacing);

                GUILayout.BeginHorizontal();

                GUILayout.Space(
                    InsightsStyle.Indentation);

                Field field =
                    fieldReference.Field;

                var item =
                    new StatisticCardItem(
                        field.name,
                        InsightsStyle.StatisticsCardDefaultLinkColor,
                        StatisticCardAction.Filter,
                        () =>
                            ShowCategoryRelationshipField(
                                reference,
                                field),
                        field.Glyph,
                        string.Format(LoreText.Common.Count, fieldReference.ReferenceCount));

                StatisticCard.Draw(
                    item);

                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();
            }
        }

        private void DrawEntryFieldCoverage(
            IReadOnlyList<EntryFieldCoverage> coverage)
        {
            if (coverage == null ||
                coverage.Count == 0)
            {
                return;
            }

            var groups =
                coverage
                    .GroupBy(
                        item =>
                            item.Category)
                    .OrderBy(
                        group =>
                            group.Key != null
                                ? group.Key.SortOrder
                                : int.MaxValue);

            bool firstGroup =
                true;

            foreach (var group in groups)
            {
                Category category =
                    group.Key;

                if (category == null)
                    continue;

                if (!firstGroup)
                {
                    GUILayout.Space(
                        InsightsStyle.Paragraph);
                }

                GUILayout.Label(
                    category.Title,
                    InsightsStyle.StatisticsText,
                    GUILayout.MinWidth(0f));

                foreach (EntryFieldCoverage item in group)
                {
                    DrawEntryFieldCoverageItem(
                        item);
                }

                firstGroup =
                    false;
            }
        }

        private void DrawEntryFieldCoverageItem(
            EntryFieldCoverage coverage)
        {
            if (coverage?.Field == null)
                return;

            bool hasUnpopulatedEntries =
                coverage.UnpopulatedCount > 0;

            Field field =
                coverage.Field;

            var item =
                new StatisticCardItem(
                    field.name,
                    hasUnpopulatedEntries
                        ? InsightsStyle.StatisticsCardDefaultLinkColor
                        : InsightsStyle.StatisticsCardDefaultColor,
                    hasUnpopulatedEntries
                        ? StatisticCardAction.Filter
                        : StatisticCardAction.None,
                    hasUnpopulatedEntries
                        ? () =>
                            ShowUnpopulatedEntryField(
                                coverage)
                        : null,
                    field.Glyph,
                    string.Format(LoreText.Insights.FieldCoverage, coverage.PopulatedCount, coverage.EntryCount));

            GUILayout.Space(
                InsightsStyle.StatisticCardSpacing);

            GUILayout.BeginHorizontal();

            GUILayout.Space(
                InsightsStyle.Indentation);

            StatisticCard.Draw(
                item);

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }
        #endregion

        #region Commands
        // Integrity — the Project description follows header and callout at the top, so scrolling to the top reveals it.
        private void RevealDescription()
        {
            _state.RightScroll = Vector2.zero;

            LoreRepaintService.Repaint();
        }

        // Integrity callout
        private void ShowBrokenEntryReferences()
        {
            ContentBrowserRequestService.RequestEntryFilter(
                entry =>
                    _relationshipIntegrityAnalyzer
                        .Analyze(
                            entry)
                        .Count > 0,
                LoreText.Insights.BrokenEntryReferencesFilter);

            // Select the first affected Entry in browser order and reveal its broken field.
            RelationshipIntegrityIssue firstIssue =
                _repository.Groups
                    .SelectMany(
                        group =>
                            group.Entries)
                    .Select(
                        entry =>
                            _relationshipIntegrityAnalyzer
                                .Analyze(
                                    entry)
                                .FirstOrDefault())
                    .FirstOrDefault(
                        issue =>
                            issue != null);

            LoreNavigation.NavigateToEntryField(
                firstIssue?.SourceEntry,
                firstIssue?.Field);

            LoreRepaintService.Repaint();
        }

        // Content Health (filters only, like the other statistic cards)
        private void ShowEntriesWithoutDescription()
        {
            ContentBrowserRequestService.RequestEntryFilter(
                _contentHealthAnalyzer.HasNoDescription,
                LoreText.Insights.EntriesWithoutDescriptionFilter);

            LoreRepaintService.Repaint();
        }

        private void ShowCategoriesWithoutDescription()
        {
            ContentBrowserRequestService.RequestCategoryFilter(
                _contentHealthAnalyzer.HasNoDescription,
                LoreText.Insights.CategoriesWithoutDescriptionFilter);

            LoreRepaintService.Repaint();
        }

        private void ShowEmptyCategories()
        {
            ContentBrowserRequestService.RequestCategoryFilter(
                _contentHealthAnalyzer.IsEmpty,
                LoreText.Insights.EmptyCategoriesFilter);

            LoreRepaintService.Repaint();
        }

        private void ShowPlaceholderNames()
        {
            ContentBrowserRequestService.RequestCombinedFilter(
                _contentHealthAnalyzer.HasPlaceholderName,
                _contentHealthAnalyzer.HasPlaceholderName,
                LoreText.Insights.PlaceholderNamesFilter);

            LoreRepaintService.Repaint();
        }

        private void ShowEntriesMissingRequired()
        {
            ContentBrowserRequestService.RequestEntryFilter(
                _contentHealthAnalyzer.HasMissingRequiredValues,
                LoreText.Insights.EntriesMissingRequiredFilter);

            LoreRepaintService.Repaint();
        }

        private void ShowEntriesWithoutTags()
        {
            ContentBrowserRequestService.RequestEntryFilter(
                _contentHealthAnalyzer.HasNoTags,
                LoreText.Insights.EntriesWithoutTagsFilter);

            LoreRepaintService.Repaint();
        }

        private void ShowBrokenDescriptionLinks()
        {
            ContentBrowserRequestService.RequestCombinedFilter(
                _descriptionLinkIntegrityAnalyzer.HasBrokenLinks,
                _descriptionLinkIntegrityAnalyzer.HasBrokenLinks,
                LoreText.Insights.BrokenDescriptionLinksFilter);

            // Select the first affected Category or Entry in browser order (a Category before its Entries);
            // its description is at the top of its page.
            foreach (CategoryEntryGroup group in _repository.Groups)
            {
                if (_descriptionLinkIntegrityAnalyzer.HasBrokenLinks(group.Category))
                {
                    LoreNavigation.NavigateToCategory(
                        group.Category);

                    break;
                }

                Entry firstEntry =
                    group.Entries.FirstOrDefault(
                        _descriptionLinkIntegrityAnalyzer.HasBrokenLinks);

                if (firstEntry != null)
                {
                    LoreNavigation.NavigateToEntry(
                        firstEntry);

                    break;
                }
            }

            LoreRepaintService.Repaint();
        }

        private void ShowMissingUnityReferences()
        {
            ContentBrowserRequestService.RequestEntryFilter(
                entry =>
                    _unityReferenceIntegrityAnalyzer
                        .Analyze(
                            entry)
                        .Count > 0,
                LoreText.Insights.MissingUnityReferencesFilter);

            // Select the first affected Entry in browser order and reveal its field.
            UnityReferenceIntegrityIssue firstIssue =
                _repository.Groups
                    .SelectMany(
                        group =>
                            group.Entries)
                    .Select(
                        entry =>
                            _unityReferenceIntegrityAnalyzer
                                .Analyze(
                                    entry)
                                .FirstOrDefault())
                    .FirstOrDefault(
                        issue =>
                            issue != null);

            LoreNavigation.NavigateToEntryField(
                firstIssue?.SourceEntry,
                firstIssue?.Field);

            LoreRepaintService.Repaint();
        }

        // Categories using a ScriptableObject Field with a missing type; selects the first and reveals that Field.
        private void ShowMissingObjectTypes()
        {
            ContentBrowserRequestService.RequestCategoryFilter(
                category =>
                    category != null &&
                    _fieldTypeIntegrityAnalyzer
                        .Analyze(
                            category)
                        .Count > 0,
                LoreText.Insights.MissingObjectTypesFilter);

            FieldTypeIntegrityIssue firstIssue =
                _repository.Categories
                    .Where(
                        category =>
                            category != null)
                    .Select(
                        category =>
                            _fieldTypeIntegrityAnalyzer
                                .Analyze(
                                    category)
                                .FirstOrDefault())
                    .FirstOrDefault(
                        issue =>
                            issue != null);

            if (firstIssue != null)
            {
                LoreNavigation.NavigateToCategoryField(
                    firstIssue.Category,
                    firstIssue.FieldIndex);
            }

            LoreRepaintService.Repaint();
        }

        private void ShowBrokenFieldDefinitions()
        {
            ContentBrowserRequestService.RequestCategoryFilter(
                category =>
                    category != null &&
                    _categoryIntegrityAnalyzer
                        .Analyze(
                            category)
                        .Count > 0,
                LoreText.Insights.BrokenFieldDefinitionsFilter);

            // Select the first affected Category in browser order and reveal its broken field.
            CategoryIntegrityIssue firstIssue =
                _repository.Categories
                    .Where(
                        category =>
                            category != null)
                    .Select(
                        category =>
                            _categoryIntegrityAnalyzer
                                .Analyze(
                                    category)
                                .FirstOrDefault())
                    .FirstOrDefault(
                        issue =>
                            issue != null);

            if (firstIssue != null)
            {
                LoreNavigation.NavigateToCategoryField(
                    firstIssue.Category,
                    firstIssue.FieldIndex);
            }

            LoreRepaintService.Repaint();
        }

        // Row 1: Categories | Entries, Row 4: Top 10 Inbound | Top 10 Outbound
        private void ShowStandaloneCategories()
        {
            ContentBrowserRequestService.RequestCategoryFilter(
                category =>
                    _relationshipAnalyzer
                        .IsStandalone(
                            category),
                LoreText.Insights.StandaloneCategoriesFilter);

            LoreRepaintService.Repaint();
        }

        private void ShowIsolatedCategories()
        {
            ContentBrowserRequestService.RequestCategoryFilter(
                category =>
                    _relationshipAnalyzer
                        .IsIsolated(
                            category),
                LoreText.Insights.IsolatedCategoriesFilter);

            LoreRepaintService.Repaint();
        }

        private void ShowUnreferencedEntries(ProjectRelationshipReport report)
        {
            if (report == null)
                return;

            ContentBrowserRequestService.RequestEntryFilter(
                entry =>
                    _relationshipQueries.IsUnreferenced(
                        entry),
                LoreText.Insights.UnreferencedEntriesFilter);

            LoreRepaintService.Repaint();
        }

        private void ShowIsolatedEntries()
        {
            ContentBrowserRequestService.RequestEntryFilter(
                entry =>
                    _relationshipAnalyzer
                        .IsIsolated(
                            entry),
                LoreText.Insights.IsolatedEntriesFilter);

            LoreRepaintService.Repaint();
        }


        // Row 5: Category References | Entry Field Coverage
        private void ShowCategoryRelationship(CategoryReference reference)
        {
            if (reference?.SourceCategory == null
                || reference.TargetCategory == null)
            {
                return;
            }

            Category sourceCategory = reference.SourceCategory;

            Category targetCategory = reference.TargetCategory;

            ContentBrowserRequestService.RequestEntryFilter(
                entry =>
                    entry != null &&
                    entry.Category ==
                        sourceCategory &&
                    _relationshipQueries
                        .ReferencesCategory(
                            entry,
                            targetCategory),
                string.Format(LoreText.Common.CategoryRelation, sourceCategory.Title, targetCategory.Title));

            LoreRepaintService.Repaint();
        }

        private void ShowCategoryRelationshipField(
            CategoryReference reference,
            Field field)
        {
            if (reference?.SourceCategory == null ||
                reference.TargetCategory == null ||
                field == null)
            {
                return;
            }

            Category sourceCategory =
                reference.SourceCategory;

            Category targetCategory =
                reference.TargetCategory;

            ContentBrowserRequestService.RequestEntryFilter(
                entry =>
                    entry != null &&
                    entry.Category ==
                        sourceCategory &&
                    _relationshipQueries
                        .ReferencesCategoryViaField(
                            entry,
                            targetCategory,
                            field),
                string.Format(LoreText.Insights.CategoryRelationFieldFilter, sourceCategory.Title, targetCategory.Title, field.name),
                field);

            LoreRepaintService.Repaint();
        }

        private void ShowUnpopulatedEntryField(EntryFieldCoverage coverage)
        {
            if (coverage?.Category == null ||
                coverage.Field == null)
            {
                return;
            }

            Category category = coverage.Category;
            Field field = coverage.Field;

            ContentBrowserRequestService.RequestEntryFilter(
                entry =>
                    entry != null &&
                    entry.Category ==
                        category &&
                    _entryFieldCoverageAnalyzer
                        .IsUnpopulated(
                            entry,
                            field),
                string.Format(LoreText.Insights.MissingFieldFilter, category.Title, field.name),
                field);

            LoreRepaintService.Repaint();
        }
        #endregion

        #region Helpers
        private int GetMissingFieldCount()
        {
            int count = 0;

            foreach (Category category in _repository.Categories)
            {
                count +=
                    _categoryIntegrityAnalyzer
                        .Analyze(
                            category)
                        .Count;
            }

            return count;
        }
        #endregion

        #region Snapshot
        private ProjectInsightsSnapshot _snapshot;
        private bool _snapshotDirty = true;

        private void EnsureSnapshot()
        {
            if (!_snapshotDirty && _snapshot != null)
            {
                return;
            }

            RefreshSnapshot();
        }

        private void RefreshSnapshot()
        {
            ProjectRelationshipReport relationshipReport = _relationshipAnalyzer.Analyze();
            LibraryUsageReport libraryUsage = _libraryUsageAnalyzer.Analyze();
            ContentHealthReport contentHealth = _contentHealthAnalyzer.Analyze(libraryUsage.TagCount);
            IReadOnlyList<EntryFieldCoverage> entryFieldCoverage = _entryFieldCoverageAnalyzer.Analyze();

            IReadOnlyList<CategoryReference> categoryRelationships =
                _categoryRelationshipAnalyzer
                    .GetCategoryReferences()
                    .OrderByDescending(
                        reference =>
                            reference.ReferenceCount)
                    .ToList();

            List<ProjectEntryHub> mostReferencedEntries =
                _repository
                    .Entries
                    .Where(
                        entry =>
                            entry != null)
                    .Select(
                        entry =>
                            new ProjectEntryHub(
                                entry,
                                _relationshipIndex
                                    .GetReferencesTo(
                                        entry)
                                    .Count))
                    .Where(
                        hub =>
                            hub.ReferenceCount > 0)
                    .OrderByDescending(
                        hub =>
                            hub.ReferenceCount)
                    .ThenBy(
                        hub =>
                            hub.Entry.Title)
                    .Take(
                        HubEntryLimit)
                    .ToList();

            List<ProjectEntryHub> mostReferencingEntries =
                _repository
                    .Entries
                    .Where(
                        entry =>
                            entry != null)
                    .Select(
                        entry =>
                            new ProjectEntryHub(
                                entry,
                                _relationshipIndex
                                    .GetReferencesFrom(
                                        entry)
                                    .Count))
                    .Where(
                        hub =>
                            hub.ReferenceCount > 0)
                    .OrderByDescending(
                        hub =>
                            hub.ReferenceCount)
                    .ThenBy(
                        hub =>
                            hub.Entry.Title)
                    .Take(
                        HubEntryLimit)
                    .ToList();

            int missingFieldCount = GetMissingFieldCount();
            int missingReferenceCount = _relationshipIntegrityAnalyzer
                    .Analyze()
                    .Count;
            int missingUnityReferenceCount = _unityReferenceIntegrityAnalyzer
                    .Analyze()
                    .Count;
            int missingObjectTypeCount = _fieldTypeIntegrityAnalyzer.CountFields(_repository.Categories);
            int brokenDescriptionLinkCount = _descriptionLinkIntegrityAnalyzer.CountProject();
            int projectBrokenDescriptionLinkCount =
                _repository.Project != null
                    ? _descriptionLinkIntegrityAnalyzer.Count(_repository.Project.Description)
                    : 0;

            _snapshot =
                new ProjectInsightsSnapshot(
                    relationshipReport,
                    libraryUsage,
                    contentHealth,
                    entryFieldCoverage,
                    categoryRelationships,
                    mostReferencedEntries,
                    mostReferencingEntries,
                    missingFieldCount,
                    missingReferenceCount,
                    missingUnityReferenceCount,
                    missingObjectTypeCount,
                    brokenDescriptionLinkCount,
                    projectBrokenDescriptionLinkCount);

            _snapshotDirty = false;
        }

        private void MarkSnapshotDirty()
        {
            _snapshotDirty = true;
            _latestEntriesDirty = true;
        }

        public void Dispose()
        {
            LoreRefreshService.Refreshed -= MarkSnapshotDirty;
            EditorApplication.projectChanged -= MarkSnapshotDirty;

            _relationshipIndex.Refreshed -= MarkSnapshotDirty;

            LoreChangeTracker.Changed -= MarkSnapshotDirty;
        }
        #endregion

        #region Latest Entries
        // Separate from the snapshot: field value edits only call LoreChangeTracker (no refresh),
        // and re-sorting these lists is much cheaper than rebuilding the snapshot on every keystroke.
        private readonly List<Entry> _latestModifiedEntries = new();
        private readonly List<Entry> _latestCreatedEntries = new();

        // Set together with the snapshot (MarkSnapshotDirty).
        private bool _latestEntriesDirty = true;

        private void EnsureLatestEntries()
        {
            if (!_latestEntriesDirty)
                return;

            _latestEntriesDirty = false;

            List<Entry> entries =
                _repository.Entries
                    .Where(entry => entry != null)
                    .ToList();

            _latestModifiedEntries.Clear();
            _latestModifiedEntries.AddRange(
                entries
                    .OrderByDescending(entry => entry.Modified)
                    .ThenBy(entry => entry.Title)
                    .Take(HubEntryLimit));

            _latestCreatedEntries.Clear();
            _latestCreatedEntries.AddRange(
                entries
                    .OrderByDescending(entry => entry.Created)
                    .ThenBy(entry => entry.Title)
                    .Take(HubEntryLimit));
        }
        #endregion
    }
}