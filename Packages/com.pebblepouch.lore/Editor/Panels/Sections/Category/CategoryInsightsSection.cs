using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class CategoryInsightsSection
    {
        #region Fields
        private readonly CategoryRelationshipAnalyzer _relationshipAnalyzer =
            new(LoreRepository.Instance);

        private readonly RelationshipQueryService _relationshipQueries =
            new(LoreRepository.Instance);

        private readonly CategoryIntegrityAnalyzer _integrityAnalyzer =
            new();

        private readonly FieldTypeIntegrityAnalyzer _fieldTypeIntegrityAnalyzer =
            new();

        private readonly DescriptionLinkIntegrityAnalyzer _descriptionLinkIntegrityAnalyzer =
            new();

        // Performance: analyses cached per Category and LoreDataVersion instead of recalculated on every OnGUI call.
        private readonly LoreCache<Category, IReadOnlyList<CategoryIntegrityIssue>> _integrityIssues = new();
        private readonly LoreCache<Category, IReadOnlyList<FieldTypeIntegrityIssue>> _fieldTypeIssues = new();
        private readonly LoreCache<string, int> _brokenDescriptionLinkCount = new();
        private readonly LoreCache<Category, CategoryRelationshipReport> _relationshipReport = new();
        private readonly LoreCache<Category, IReadOnlyList<CategoryReference>> _inboundReferences = new();
        private readonly LoreCache<Category, IReadOnlyList<CategoryReference>> _outboundReferences = new();

        private readonly IntegrityCalloutCard _calloutCard =
            new();

        private readonly StatisticCardFlow _entryStatisticFlow =
            new();

        private readonly StatisticCardFlow _inboundStatisticFlow =
            new();

        private readonly StatisticCardFlow _outboundStatisticFlow =
            new();

        private readonly WorkspaceState _state;

        private Category _category;

        private float _availableInsightsWidth;
        private readonly InsightsRowHeight _rowHeight = new();
        #endregion

        #region Constructor
        public CategoryInsightsSection(
            WorkspaceState state)
        {
            _state = state;
        }
        #endregion

        #region Public API
        public void SetCategory(
            Category category)
        {
            if (_category != category)
            {
                _rowHeight.Reset();
            }

            _category =
                category;
        }

        // Integrity callout, drawn by the panel between header and description (prominent: should not happen).
        public void DrawIntegrity()
        {
            if (_category == null)
                return;

            IReadOnlyList<CategoryIntegrityIssue> integrityIssues =
                _integrityIssues.Get(
                    _category,
                    _integrityAnalyzer.Analyze);

            IReadOnlyList<FieldTypeIntegrityIssue> typeIssues =
                _fieldTypeIssues.Get(
                    _category,
                    _fieldTypeIntegrityAnalyzer.Analyze);

            int brokenDescriptionLinkCount =
                _brokenDescriptionLinkCount.Get(
                    _category.Description,
                    _descriptionLinkIntegrityAnalyzer.Count);

            if (integrityIssues.Count == 0 &&
                typeIssues.Count == 0 &&
                brokenDescriptionLinkCount == 0)
            {
                return;
            }

            DrawIntegrityCallout(
                integrityIssues,
                typeIssues,
                brokenDescriptionLinkCount);
        }

        public void Draw()
        {
            if (_category == null)
                return;

            DrawContent();
        }
        #endregion

        #region Draw
        private void DrawContent()
        {
            CategoryRelationshipReport report =
                _relationshipReport.Get(
                    _category,
                    _relationshipAnalyzer.Analyze);

            IReadOnlyList<CategoryReference> inboundReferences =
                _inboundReferences.Get(
                    _category,
                    _relationshipAnalyzer.GetReferencesTo);

            IReadOnlyList<CategoryReference> outboundReferences =
                _outboundReferences.Get(
                    _category,
                    _relationshipAnalyzer.GetReferencesFrom);

            InsightsRow.UpdateAvailableWidth(
                ref _availableInsightsWidth);

            // Row 1: Entries | Inbound Categories | Outbound Categories
            DrawCategoryInsightsRow(
                report,
                inboundReferences,
                outboundReferences);
        }

        // Integrity callout (see DrawIntegrity)
        private void DrawIntegrityCallout(
            IReadOnlyList<CategoryIntegrityIssue> issues,
            IReadOnlyList<FieldTypeIntegrityIssue> typeIssues,
            int brokenDescriptionLinkCount)
        {
            int issueCount =
                (issues?.Count ?? 0) +
                typeIssues.Count +
                brokenDescriptionLinkCount;

            if (issueCount == 0)
                return;

            bool hasBrokenFields =
                issues != null &&
                issues.Count > 0;

            int groupCount =
                (hasBrokenFields ? 1 : 0) +
                (typeIssues.Count > 0 ? 1 : 0) +
                (brokenDescriptionLinkCount > 0 ? 1 : 0);

            var pieces =
                new List<IntegrityCalloutPiece>();

            // "<sentence {0}>" with {0} = groups: "broken {0}" with {0} = "Field #1, Field #2 and Field #4" (links),
            // and "2 broken description links" (link scrolls up to the description).
            IntegrityCalloutCard.AddTemplate(
                pieces,
                LoreText.Insights.Integrity.For(issueCount),
                () =>
                {
                    int groupIndex = 0;

                    if (hasBrokenFields)
                    {
                        AddBrokenFields(
                            pieces,
                            issues);

                        groupIndex++;
                    }

                    if (typeIssues.Count > 0)
                    {
                        IntegrityCalloutCard.AddListSeparator(
                            pieces,
                            groupIndex,
                            groupCount);

                        AddMissingTypes(
                            pieces,
                            typeIssues);

                        groupIndex++;
                    }

                    if (brokenDescriptionLinkCount > 0)
                    {
                        IntegrityCalloutCard.AddListSeparator(
                            pieces,
                            groupIndex,
                            groupCount);

                        pieces.Add(
                            new IntegrityCalloutPiece(
                                LoreText.Insights.BrokenDescriptionLinks.Format(brokenDescriptionLinkCount),
                                RevealDescription));
                    }
                });

            _calloutCard.Draw(
                pieces);
        }

        // "missing types in {0}" with {0} = "Dialog and Quest" (links reveal the Field rows).
        private static void AddMissingTypes(
            List<IntegrityCalloutPiece> pieces,
            IReadOnlyList<FieldTypeIntegrityIssue> issues)
        {
            IntegrityCalloutCard.AddTemplate(
                pieces,
                LoreText.Insights.MissingObjectTypesIn.For(issues.Count),
                () =>
                {
                    for (int i = 0;
                         i < issues.Count;
                         i++)
                    {
                        FieldTypeIntegrityIssue issue =
                            issues[i];

                        IntegrityCalloutCard.AddListSeparator(
                            pieces,
                            i,
                            issues.Count);

                        pieces.Add(
                            new IntegrityCalloutPiece(
                                issue.Field.Title,
                                () =>
                                {
                                    CategoryFieldRevealRequestService.Request(
                                        issue.Category,
                                        issue.FieldIndex);

                                    LoreRepaintService.Repaint();
                                }));
                    }
                });
        }

        private static void AddBrokenFields(
            List<IntegrityCalloutPiece> pieces,
            IReadOnlyList<CategoryIntegrityIssue> issues)
        {
            IntegrityCalloutCard.AddTemplate(
                pieces,
                LoreText.Insights.BrokenFields,
                () =>
                {
                    for (int i = 0;
                         i < issues.Count;
                         i++)
                    {
                        CategoryIntegrityIssue issue =
                            issues[i];

                        if (issue == null)
                            continue;

                        IntegrityCalloutCard.AddListSeparator(
                            pieces,
                            i,
                            issues.Count);

                        CategoryIntegrityIssue capturedIssue =
                            issue;

                        pieces.Add(
                            new IntegrityCalloutPiece(
                                string.Format(LoreText.Insights.FieldNumber, issue.FieldIndex + 1),
                                () =>
                                    RevealBrokenField(
                                        capturedIssue)));
                    }
                });
        }

        // Row 1: Entries | Inbound Categories | Outbound Categories
        private void DrawCategoryInsightsRow(
            CategoryRelationshipReport report,
            IReadOnlyList<CategoryReference> inboundReferences,
            IReadOnlyList<CategoryReference> outboundReferences)
        {
            bool expanded =
                _state.IsInspectorSectionExpanded(
                    InspectorSection.CategoryInsights);

            Action toggle =
                () =>
                    _state.ToggleInspectorSection(
                        InspectorSection.CategoryInsights);

            InsightsRow.Draw(
                _availableInsightsWidth,
                _rowHeight,
                () =>
                    SectionCard.DrawRowCollapsible(
                        LoreText.Insights.EntriesTitle,
                        expanded,
                        toggle,
                        () =>
                            DrawEntrySummary(
                                report),
                        InsightsStyle.Card,
                        _rowHeight),

                () =>
                    SectionCard.DrawRowCollapsible(
                        LoreText.Insights.InboundCategoriesTitle,
                        expanded,
                        toggle,
                        () =>
                            DrawInboundCategories(
                                report,
                                inboundReferences),
                        InsightsStyle.Card,
                        _rowHeight),

                () =>
                    SectionCard.DrawRowCollapsible(
                        LoreText.Insights.OutboundCategoriesTitle,
                        expanded,
                        toggle,
                        () =>
                            DrawOutboundCategories(
                                report,
                                outboundReferences),
                        InsightsStyle.Card,
                        _rowHeight));
        }

        private void DrawEntrySummary(
            CategoryRelationshipReport report)
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
                            ? () =>
                                ShowUnreferencedEntries(
                                    report)
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
                            ? () =>
                                ShowIsolatedEntries(
                                    report)
                            : null)
                };

            _entryStatisticFlow.Draw(
                cards);
        }

        private void DrawInboundCategories(
            CategoryRelationshipReport report,
            IReadOnlyList<CategoryReference> references)
        {
            if (references == null ||
                references.Count == 0)
            {
                GUILayout.Label(
                    LoreText.Insights.NoInboundCategories,
                    InsightsStyle.StatisticsText,
                    GUILayout.MinWidth(0f));

                return;
            }

            GUILayout.Label(
                LoreText.Insights.ReferenceCount.Format(report.IncomingReferenceCount),
                InsightsStyle.StatisticsText,
                GUILayout.MinWidth(0f));

            var cards =
                new List<StatisticCardItem>();

            foreach (CategoryReference reference in
                     references)
            {
                if (reference?.SourceCategory == null ||
                    reference.TargetCategory == null)
                {
                    continue;
                }

                Category category =
                    reference.SourceCategory;

                Category sourceCategory =
                    reference.SourceCategory;

                Category targetCategory =
                    reference.TargetCategory;

                cards.Add(
                    new StatisticCardItem(
                        category.Title,
                        category.Color,
                        StatisticCardAction.Filter,
                        () =>
                            ShowRelationship(
                                sourceCategory,
                                targetCategory),
                        category.Glyph,
                        string.Format(LoreText.Common.Count, reference.ReferenceCount)));
            }

            GUILayout.Space(
                InsightsStyle.Paragraph);

            GUILayout.BeginHorizontal();

            GUILayout.Space(
                InsightsStyle.Indentation);

            GUILayout.BeginVertical(
                GUILayout.MinWidth(0f),
                GUILayout.ExpandWidth(true));

            _inboundStatisticFlow.Draw(
                cards);

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }

        private void DrawOutboundCategories(
            CategoryRelationshipReport report,
            IReadOnlyList<CategoryReference> references)
        {
            if (references == null ||
                references.Count == 0)
            {
                GUILayout.Label(
                    LoreText.Insights.NoOutboundCategories,
                    InsightsStyle.StatisticsText,
                    GUILayout.MinWidth(0f));

                return;
            }

            GUILayout.Label(
                LoreText.Insights.ReferenceCount.Format(report.OutgoingReferenceCount),
                InsightsStyle.StatisticsText,
                GUILayout.MinWidth(0f));

            var cards =
                new List<StatisticCardItem>();

            foreach (CategoryReference reference in
                     references)
            {
                if (reference?.SourceCategory == null ||
                    reference.TargetCategory == null)
                {
                    continue;
                }

                Category category =
                    reference.TargetCategory;

                Category sourceCategory =
                    reference.SourceCategory;

                Category targetCategory =
                    reference.TargetCategory;

                cards.Add(
                    new StatisticCardItem(
                        category.Title,
                        category.Color,
                        StatisticCardAction.Filter,
                        () =>
                            ShowRelationship(
                                sourceCategory,
                                targetCategory),
                        category.Glyph,
                        string.Format(LoreText.Common.Count, reference.ReferenceCount)));
            }

            GUILayout.Space(
                InsightsStyle.Paragraph);

            GUILayout.BeginHorizontal();

            GUILayout.Space(
                InsightsStyle.Indentation);

            GUILayout.BeginVertical(
                GUILayout.MinWidth(0f),
                GUILayout.ExpandWidth(true));

            _outboundStatisticFlow.Draw(
                cards);

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }
        #endregion

        #region Commands
        // Integrity — the description follows header and callout at the top, so scrolling to the top reveals it.
        private void RevealDescription()
        {
            _state.RightScroll = Vector2.zero;

            LoreRepaintService.Repaint();
        }

        // Integrity callout
        private static void RevealBrokenField(
            CategoryIntegrityIssue issue)
        {
            if (issue?.Category == null ||
                issue.FieldIndex < 0)
            {
                return;
            }

            CategoryFieldRevealRequestService.Request(
                issue.Category,
                issue.FieldIndex);

            LoreRepaintService.Repaint();
        }

        // Row 1: Entries
        private void ShowUnreferencedEntries(
            CategoryRelationshipReport report)
        {
            if (report?.Category == null)
                return;

            Category category =
                report.Category;

            ContentBrowserRequestService.RequestEntryFilter(
                entry =>
                    entry != null &&
                    entry.Category ==
                        category &&
                    _relationshipQueries
                        .IsUnreferenced(
                            entry),
                string.Format(LoreText.Insights.UnreferencedInCategoryFilter, category.Title));

            LoreRepaintService.Repaint();
        }

        private static void ShowIsolatedEntries(
            CategoryRelationshipReport report)
        {
            if (report?.Category == null)
                return;

            Category category =
                report.Category;

            ContentBrowserRequestService.RequestEntryFilter(
                entry =>
                    entry != null &&
                    entry.Category ==
                        category &&
                    report.IsolatedEntries.Contains(
                        entry),
                string.Format(LoreText.Insights.IsolatedInCategoryFilter, category.Title));

            LoreRepaintService.Repaint();
        }

        // Row 2: Inbound Categories | Outbound Categories
        private void ShowRelationship(
            Category sourceCategory,
            Category targetCategory)
        {
            if (sourceCategory == null ||
                targetCategory == null)
            {
                return;
            }

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
        #endregion
    }
}