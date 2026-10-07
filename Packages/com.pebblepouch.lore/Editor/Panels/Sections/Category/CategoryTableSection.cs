using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Tabular Editing: all Entries of the Category at a glance (see CategoryEntryTable). Collapsed by default.
    public sealed class CategoryTableSection
    {
        #region Fields
        private readonly WorkspaceState _state;

        private readonly CategoryEntryTable _table = new();

        private Category _category;
        #endregion

        #region Constructor
        public CategoryTableSection(
            WorkspaceState state)
        {
            _state = state;
        }
        #endregion

        #region Public API
        public void SetCategory(
            Category category)
        {
            _category = category;

            _table.SetCategory(category);
        }

        public void Draw()
        {
            if (_category == null)
                return;

            bool expanded =
                _state.IsInspectorSectionExpanded(
                    InspectorSection.CategoryTable);

            // Same gap as between the other section cards (the Insights row above has no bottom margin).
            GUILayout.Space(
                InspectorStyle.SectionCard.margin.bottom);

            SectionCard.DrawCollapsible(
                LoreText.Inspector.TabularEditingHeader,
                expanded,
                () =>
                    _state.ToggleInspectorSection(
                        InspectorSection.CategoryTable),
                () =>
                    _table.Draw(
                        _state.RightViewportHeight));
        }
        #endregion
    }
}
