using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class EntryInsightsSection
    {
        #region Fields
        private readonly EntryRelationshipAnalyzer _relationshipAnalyzer =
            new(LoreRepository.Instance);

        private readonly RelationshipIntegrityAnalyzer _integrityAnalyzer =
            new(LoreRepository.Instance);

        private readonly EntryIntegrityAnalyzer _entryIntegrityAnalyzer =
            new();

        private readonly UnityReferenceIntegrityAnalyzer _unityReferenceIntegrityAnalyzer =
            new(LoreRepository.Instance);

        private readonly DescriptionLinkIntegrityAnalyzer _descriptionLinkIntegrityAnalyzer =
            new();

        // Performance: analyses cached per Entry and LoreDataVersion instead of recalculated on every OnGUI call.
        private readonly LoreCache<Entry, IReadOnlyList<EntryIntegrityIssue>> _entryIntegrityIssues = new();
        private readonly LoreCache<Entry, IReadOnlyList<RelationshipIntegrityIssue>> _relationshipIntegrityIssues = new();
        private readonly LoreCache<Entry, IReadOnlyList<UnityReferenceIntegrityIssue>> _unityReferenceIssues = new();
        private readonly LoreCache<string, int> _brokenDescriptionLinkCount = new();
        private readonly LoreCache<Entry, IReadOnlyList<EntryReference>> _referencesFrom = new();
        private readonly LoreCache<Entry, IReadOnlyList<EntryReference>> _referencesTo = new();

        private readonly IntegrityCalloutCard _calloutCard =
            new();

        private readonly StatisticCardFlow _inboundStatisticFlow =
            new();

        private readonly StatisticCardFlow _outboundStatisticFlow =
            new();

        private readonly WorkspaceState _state;

        private Entry _entry;

        private float _availableInsightsWidth;
        private readonly InsightsRowHeight _rowHeight = new();
        #endregion

        #region Constructor
        public EntryInsightsSection(
            WorkspaceState state)
        {
            _state = state;
        }
        #endregion

        #region Public API
        public void SetEntry(
            Entry entry)
        {
            if (_entry != entry)
            {
                _rowHeight.Reset();
            }

            _entry =
                entry;
        }

        // Integrity callout, drawn by the panel between header and description (prominent: should not happen).
        public void DrawIntegrity()
        {
            if (_entry == null)
                return;

            IReadOnlyList<EntryIntegrityIssue> entryIntegrityIssues =
                _entryIntegrityIssues.Get(
                    _entry,
                    _entryIntegrityAnalyzer.Analyze);

            IReadOnlyList<RelationshipIntegrityIssue> relationshipIntegrityIssues =
                _relationshipIntegrityIssues.Get(
                    _entry,
                    _integrityAnalyzer.Analyze);

            IReadOnlyList<UnityReferenceIntegrityIssue> unityReferenceIssues =
                _unityReferenceIssues.Get(
                    _entry,
                    _unityReferenceIntegrityAnalyzer.Analyze);

            int brokenDescriptionLinkCount =
                _brokenDescriptionLinkCount.Get(
                    _entry.Description,
                    _descriptionLinkIntegrityAnalyzer.Count);

            if (entryIntegrityIssues.Count == 0 &&
                relationshipIntegrityIssues.Count == 0 &&
                unityReferenceIssues.Count == 0 &&
                brokenDescriptionLinkCount == 0)
            {
                return;
            }

            DrawIntegrityCallout(
                entryIntegrityIssues,
                relationshipIntegrityIssues,
                unityReferenceIssues,
                brokenDescriptionLinkCount);
        }

        public void Draw()
        {
            if (_entry == null)
                return;

            DrawContent();
        }
        #endregion

        #region Draw
        private void DrawContent()
        {
            IReadOnlyList<EntryReference> referencesFrom =
                _referencesFrom.Get(
                    _entry,
                    _relationshipAnalyzer.GetReferencesFrom);

            IReadOnlyList<EntryReference> referencesTo =
                _referencesTo.Get(
                    _entry,
                    _relationshipAnalyzer.GetReferencesTo);

            InsightsRow.UpdateAvailableWidth(
                ref _availableInsightsWidth);

            // Row 1: Inbound Entries | Outbound Entries
            DrawRelationshipRow(
                referencesTo,
                referencesFrom);
        }

        // Integrity callout (see DrawIntegrity)
        private void DrawIntegrityCallout(
            IReadOnlyList<EntryIntegrityIssue> entryIssues,
            IReadOnlyList<RelationshipIntegrityIssue> relationshipIssues,
            IReadOnlyList<UnityReferenceIntegrityIssue> unityReferenceIssues,
            int brokenDescriptionLinkCount)
        {
            var pieces =
                new List<IntegrityCalloutPiece>();

            var groups =
                new List<List<IntegrityCalloutPiece>>();

            if (relationshipIssues != null &&
                relationshipIssues.Count > 0)
            {
                var group = new List<IntegrityCalloutPiece>();

                AddBrokenReferencePieces(
                    group,
                    relationshipIssues);

                groups.Add(group);
            }

            if (entryIssues != null &&
                entryIssues.Count > 0)
            {
                var group = new List<IntegrityCalloutPiece>();

                AddBrokenDefinitionPieces(
                    group,
                    entryIssues);

                groups.Add(group);
            }

            if (unityReferenceIssues != null &&
                unityReferenceIssues.Count > 0)
            {
                var group = new List<IntegrityCalloutPiece>();

                AddMissingUnityReferencePieces(
                    group,
                    unityReferenceIssues);

                groups.Add(group);
            }

            // Link scrolls up to the description (broken links are shown red there).
            if (brokenDescriptionLinkCount > 0)
            {
                groups.Add(
                    new List<IntegrityCalloutPiece>
                    {
                        new(
                            LoreText.Insights.BrokenDescriptionLinks.Format(brokenDescriptionLinkCount),
                            RevealDescription)
                    });
            }

            int totalIssueCount =
                (relationshipIssues?.Count ?? 0) +
                (entryIssues?.Count ?? 0) +
                (unityReferenceIssues?.Count ?? 0) +
                brokenDescriptionLinkCount;

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

                        pieces.AddRange(
                            groups[i]);
                    }
                });

            _calloutCard.Draw(
                pieces);
        }

        private void AddBrokenReferencePieces(
            List<IntegrityCalloutPiece> pieces,
            IReadOnlyList<RelationshipIntegrityIssue> issues)
        {
            IntegrityCalloutCard.AddTemplate(
                pieces,
                LoreText.Insights.BrokenEntryReferencesIn.For(issues.Count),
                () =>
                {
                    for (int i = 0;
                         i < issues.Count;
                         i++)
                    {
                        RelationshipIntegrityIssue issue =
                            issues[i];

                        if (issue?.Field == null)
                            continue;

                        IntegrityCalloutCard.AddListSeparator(
                            pieces,
                            i,
                            issues.Count);

                        RelationshipIntegrityIssue capturedIssue =
                            issue;

                        pieces.Add(
                            new IntegrityCalloutPiece(
                                issue.Field.name,
                                () =>
                                    RevealIntegrityIssue(
                                        capturedIssue)));
                    }
                });
        }

        private void AddBrokenDefinitionPieces(
            List<IntegrityCalloutPiece> pieces,
            IReadOnlyList<EntryIntegrityIssue> issues)
        {
            IntegrityCalloutCard.AddTemplate(
                pieces,
                LoreText.Insights.BrokenFieldDefinitionsAt.For(issues.Count),
                () =>
                {
                    for (int i = 0;
                         i < issues.Count;
                         i++)
                    {
                        EntryIntegrityIssue issue =
                            issues[i];

                        if (issue == null)
                            continue;

                        IntegrityCalloutCard.AddListSeparator(
                            pieces,
                            i,
                            issues.Count);

                        EntryIntegrityIssue capturedIssue =
                            issue;

                        pieces.Add(
                            new IntegrityCalloutPiece(
                                string.Format(LoreText.Insights.FieldNumber, issue.FieldIndex + 1),
                                () =>
                                    LoreNavigation.NavigateToCategoryField(
                                        _entry?.Category,
                                        capturedIssue.FieldIndex)));
                    }
                });
        }

        private void AddMissingUnityReferencePieces(
            List<IntegrityCalloutPiece> pieces,
            IReadOnlyList<UnityReferenceIntegrityIssue> issues)
        {
            IntegrityCalloutCard.AddTemplate(
                pieces,
                LoreText.Insights.MissingUnityReferencesIn.For(issues.Count),
                () =>
                {
                    for (int i = 0;
                         i < issues.Count;
                         i++)
                    {
                        UnityReferenceIntegrityIssue issue =
                            issues[i];

                        if (issue?.Field == null)
                            continue;

                        IntegrityCalloutCard.AddListSeparator(
                            pieces,
                            i,
                            issues.Count);

                        Entry sourceEntry =
                            issue.SourceEntry;

                        Field field =
                            issue.Field;

                        pieces.Add(
                            new IntegrityCalloutPiece(
                                field.name,
                                () =>
                                    RevealField(
                                        sourceEntry,
                                        field)));
                    }
                });
        }

        // Row 1: Inbound Entries | Outbound Entries
        private void DrawReferences(
            IReadOnlyList<EntryReference> references)
        {
            if (references == null ||
                references.Count == 0)
            {
                GUILayout.Label(
                    LoreText.Insights.NoOutboundEntries,
                    InsightsStyle.StatisticsText,
                    GUILayout.MinWidth(0f));

                return;
            }

            var groups =
                references
                    .Where(
                        reference =>
                            reference.TargetEntry != null)
                    .Select(
                        reference =>
                            reference.TargetEntry)
                    .Distinct()
                    .GroupBy(
                        entry =>
                            entry.Category)
                    .OrderBy(
                        group =>
                            group.Key != null
                                ? group.Key.SortOrder
                                : int.MaxValue);

            DrawEntryGroups(
                groups,
                _outboundStatisticFlow);
        }

        private void DrawReferencedBy(
            IReadOnlyList<EntryReference> references)
        {
            if (references == null ||
                references.Count == 0)
            {
                GUILayout.Label(
                    LoreText.Insights.NoInboundEntries,
                    InsightsStyle.StatisticsText,
                    GUILayout.MinWidth(0f));

                return;
            }

            var groups =
                references
                    .Where(
                        reference =>
                            reference.SourceEntry != null)
                    .Select(
                        reference =>
                            reference.SourceEntry)
                    .Distinct()
                    .GroupBy(
                        entry =>
                            entry.Category)
                    .OrderBy(
                        group =>
                            group.Key != null
                                ? group.Key.SortOrder
                                : int.MaxValue);

            DrawEntryGroups(
                groups,
                _inboundStatisticFlow);
        }

        private static void DrawEntryGroups(
            IEnumerable<IGrouping<Category, Entry>> groups,
            StatisticCardFlow flow)
        {
            bool firstGroup =
                true;

            foreach (IGrouping<Category, Entry> group in groups)
            {
                if (!firstGroup)
                {
                    GUILayout.Space(
                        InsightsStyle.Paragraph);
                }

                Category category =
                    group.Key;

                string categoryTitle =
                    category != null
                        ? category.Title
                        : LoreText.Common.Uncategorized;

                GUILayout.Label(
                    categoryTitle,
                    InsightsStyle.StatisticsText,
                    GUILayout.MinWidth(0f));

                var cards =
                    new List<StatisticCardItem>();

                foreach (Entry entry in
                         group.OrderBy(
                             entry =>
                                 entry.SortOrder))
                {
                    if (entry == null)
                        continue;

                    Color color =
                        entry.Category != null
                            ? entry.Category.Color
                            : InsightsStyle.StatisticsCardDefaultLinkColor;

                    Entry capturedEntry =
                        entry;

                    cards.Add(
                        new StatisticCardItem(
                            entry.Title,
                            color,
                            StatisticCardAction.Selection,
                            () =>
                                LoreNavigation.NavigateToEntry(
                                    capturedEntry),
                            entry.Glyph));
                }

                GUILayout.BeginHorizontal();

                GUILayout.Space(InsightsStyle.Indentation);

                GUILayout.BeginVertical(GUILayout.MinWidth(0f), GUILayout.ExpandWidth(true));

                flow.Draw(cards);

                GUILayout.EndVertical();

                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();

                firstGroup = false;
            }
        }

        private void DrawRelationshipRow(
            IReadOnlyList<EntryReference> inboundReferences,
            IReadOnlyList<EntryReference> outboundReferences)
        {
            bool expanded =
                _state.IsInspectorSectionExpanded(
                    InspectorSection.EntryInsights);

            Action toggle =
                () =>
                    _state.ToggleInspectorSection(
                        InspectorSection.EntryInsights);

            InsightsRow.Draw(
                _availableInsightsWidth,
                _rowHeight,
                () =>
                    SectionCard.DrawRowCollapsible(
                        LoreText.Insights.InboundEntriesTitle,
                        expanded,
                        toggle,
                        () =>
                            DrawReferencedBy(
                                inboundReferences),
                        InsightsStyle.Card,
                        _rowHeight),

                () =>
                    SectionCard.DrawRowCollapsible(
                        LoreText.Insights.OutboundEntriesTitle,
                        expanded,
                        toggle,
                        () =>
                            DrawReferences(
                                outboundReferences),
                        InsightsStyle.Card,
                        _rowHeight));
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
        private void RevealIntegrityIssue(
            RelationshipIntegrityIssue issue)
        {
            RevealField(
                issue?.SourceEntry,
                issue?.Field);
        }

        private static void RevealField(
            Entry entry,
            Field field)
        {
            if (entry == null ||
                field == null)
            {
                return;
            }

            EntryFieldRevealRequestService.Request(
                entry,
                field);

            LoreRepaintService.Repaint();
        }
        #endregion
    }
}