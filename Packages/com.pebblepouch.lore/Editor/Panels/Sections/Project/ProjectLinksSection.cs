using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Project page section "Links (n)": collapsible card with a Create button and the link table.
    // Same look and interaction pattern as the Category Fields section (without a library).
    public sealed class ProjectLinksSection
    {
        #region Fields
        private readonly WorkspaceState _state;
        private readonly ProjectService _projectService = new();
        private readonly ProjectLinkTable _table = new();
        #endregion

        #region Properties
        public bool IsEditing => _table.IsEditing;
        #endregion

        #region Constructor
        public ProjectLinksSection(WorkspaceState state)
        {
            _state = state;
        }
        #endregion

        #region Public API
        public void Draw()
        {
            bool expanded =
                _state.IsInspectorSectionExpanded(
                    InspectorSection.ProjectLinks);

            SectionCard.Draw(
                () =>
                {
                    CollapsibleSectionHeader.Draw(
                        string.Format(
                            LoreText.Inspector.LinksHeader,
                            _projectService.GetLinks().Count),
                        expanded,
                        () =>
                            _state.ToggleInspectorSection(
                                InspectorSection.ProjectLinks),
                        () =>
                            ButtonDrawer.Create(
                                _table.CreateLink,
                                ButtonDrawer.DisplayMode.Glyph,
                                BrowserStyle.BrowserHeaderButton));

                    if (expanded)
                    {
                        _table.Draw();
                    }
                });
        }

        public void CommitEditing()
        {
            _table.CommitEditing();
        }

        public void HandleOutsideClick(
            Vector2 mousePosition)
        {
            _table.HandleOutsideClick(
                mousePosition);
        }
        #endregion
    }
}
