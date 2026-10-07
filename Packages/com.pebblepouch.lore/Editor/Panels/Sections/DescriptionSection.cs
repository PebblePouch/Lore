using System;

using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Collapsible Inspector section around a DescriptionElement (Category description, Entry notes,
    // project description). Expansion state is persisted per InspectorSection.
    public sealed class DescriptionSection<T> where T : class
    {
        #region Fields
        private readonly DescriptionElement<T> _descriptionElement;
        private readonly WorkspaceState _state;

        private readonly InspectorSection _section;
        // Resolved while drawing (see DescriptionElement: styles are not ready during OnEnable).
        private readonly Func<string> _getHeader;
        #endregion

        #region Properties
        public bool IsEditing => _descriptionElement.IsEditing;
        #endregion

        #region Constructor
        public DescriptionSection(
            WorkspaceState state,
            InspectorSection section,
            Func<string> getHeader,
            DescriptionElement<T> descriptionElement)
        {
            _state = state;

            _section = section;
            _getHeader = getHeader;

            _descriptionElement = descriptionElement;
        }
        #endregion

        #region Public API
        public void SetTarget(T target)
        {
            _descriptionElement.SetTarget(target);
        }

        public void Draw()
        {
            if (!_descriptionElement.HasTarget)
                return;

            bool expanded =
                _state.IsInspectorSectionExpanded(
                    _section);

            SectionCard.DrawCollapsible(
                _getHeader(),
                expanded,
                () =>
                    _state.ToggleInspectorSection(
                        _section),
                DrawContent);
        }

        public void CommitEditing()
        {
            _descriptionElement.CommitEditing();
        }

        public void HandleOutsideClick(
            Vector2 mousePosition)
        {
            _descriptionElement.HandleOutsideClick(
                mousePosition);
        }
        #endregion

        #region Draw
        private void DrawContent()
        {
            _descriptionElement.Draw();
        }
        #endregion
    }

    // Configured description sections for each owner (keeps panel setup short and in one place).
    // Called from panel constructors (OnEnable): style values must be passed as lambdas, because touching a
    // style class here would build its GUIStyles before EditorStyles exist. LoreText values are safe (strings only).
    public static class DescriptionSections
    {
        #region Public API
        public static DescriptionSection<Category> ForCategory(WorkspaceState state)
        {
            var categoryService = new CategoryService();

            return new DescriptionSection<Category>(
                state,
                InspectorSection.CategoryDescription,
                () => LoreText.Inspector.CategoryDescriptionHeader,
                new DescriptionElement<Category>(
                    category => category.Description,
                    (category, description) =>
                        categoryService.UpdateDescription(
                            category,
                            description),
                    LoreText.Inspector.CategoryDescriptionPlaceholder,
                    () => InspectorStyle.CategoryDescription));
        }

        public static DescriptionSection<Entry> ForEntry(WorkspaceState state)
        {
            var entryService = new EntryService();

            return new DescriptionSection<Entry>(
                state,
                InspectorSection.EntryDescription,
                () => LoreText.Inspector.EntryNotesHeader,
                new DescriptionElement<Entry>(
                    entry => entry.Description,
                    (entry, description) =>
                        entryService.UpdateDescription(
                            entry,
                            description),
                    LoreText.Inspector.EntryNotesPlaceholder,
                    () => InspectorStyle.EntryDescription));
        }

        // The project has no selectable target object; ProjectService reads/writes its description.
        public static DescriptionSection<ProjectService> ForProject(WorkspaceState state)
        {
            var section =
                new DescriptionSection<ProjectService>(
                    state,
                    InspectorSection.ProjectDescription,
                    () => LoreText.Inspector.ProjectDescriptionHeader,
                    new DescriptionElement<ProjectService>(
                        projectService => projectService.GetDescription(),
                        (projectService, description) =>
                            projectService.UpdateDescription(
                                description),
                        LoreText.Inspector.ProjectDescriptionPlaceholder,
                        () => InspectorStyle.ProjectDescription));

            section.SetTarget(
                new ProjectService());

            return section;
        }
        #endregion
    }
}
