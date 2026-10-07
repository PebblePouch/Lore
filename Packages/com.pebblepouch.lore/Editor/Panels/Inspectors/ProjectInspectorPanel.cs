using System;

using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class ProjectInspectorPanel : IDisposable
    {
        #region Fields
        private readonly WorkspaceState _state;
        private readonly ProjectHeaderSection _headerSection;
        private readonly DescriptionSection<ProjectService> _descriptionSection;
        private readonly ProjectLinksSection _linksSection;
        private readonly ProjectInsightsSection _insightsSection;
        private readonly FieldConstantsNoticeSection _fieldConstantsNotice = new();
        #endregion

        #region Properties
        public bool IsEditing => _headerSection.IsEditing || _descriptionSection.IsEditing || _linksSection.IsEditing;
        #endregion

        #region Constructor
        public ProjectInspectorPanel(WorkspaceState state)
        {
            _state = state;

            _headerSection = new ProjectHeaderSection();
            _descriptionSection = DescriptionSections.ForProject(state);
            _linksSection = new ProjectLinksSection(state);
            _insightsSection = new ProjectInsightsSection(state);

            SelectionService.CategoryChanged += OnCategoryChanged;
            SelectionService.EntryChanged += OnEntryChanged;
        }
        #endregion

        #region Public API
        public void BeginTitleEditing()
        {
            _headerSection.BeginTitleEditing();
        }

        public void Draw()
        {
            if (Event.current.type ==
                EventType.MouseDown)
            {
                _headerSection.HandleOutsideClick(
                    Event.current.mousePosition);

                _descriptionSection.HandleOutsideClick(
                    Event.current.mousePosition);

                _linksSection.HandleOutsideClick(
                    Event.current.mousePosition);
            }

            _state.RightScroll =
                EditorLayout.ScrollView(
                    _state.RightScroll,
                    () =>
                    {
                        _headerSection.Draw();
                        _insightsSection.DrawIntegrity();
                        _fieldConstantsNotice.Draw();
                        _descriptionSection.Draw();
                        _linksSection.Draw();
                        _insightsSection.Draw();

                        Rect contentEndRect =
                            GUILayoutUtility.GetRect(
                                0f,
                                0f,
                                GUILayout.ExpandWidth(true));

                        if (Event.current.type ==
                            EventType.Repaint)
                        {
                            _state.RightContentHeight =
                                contentEndRect.yMax;
                        }
                    });
        }
        #endregion

        #region Helpers
        private void OnCategoryChanged(Category category)
        {
            if (category != null)
                CommitEditing();
        }

        private void OnEntryChanged(Entry entry)
        {
            if (entry != null)
                CommitEditing();
        }

        private void CommitEditing()
        {
            _headerSection.CommitEditing();
            _descriptionSection.CommitEditing();
            _linksSection.CommitEditing();
        }
        #endregion

        #region Snapshot
        public void Dispose()
        {
            SelectionService.CategoryChanged -= OnCategoryChanged;
            SelectionService.EntryChanged -= OnEntryChanged;

            _insightsSection.Dispose();
        }
        #endregion
    }
}
