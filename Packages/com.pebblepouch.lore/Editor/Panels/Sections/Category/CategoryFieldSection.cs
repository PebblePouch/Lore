using System;

using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class CategoryFieldSection
    {
        #region Fields
        private readonly Action _repaint;

        private readonly Action<Category> _browseRequested;
        private readonly Action<Field> _editRequested;
        private readonly Action<Rect> _revealRequested;

        private readonly FieldService _fieldService = new();
        private readonly CategoryFieldsHeaderSection _header = new();
        private readonly WorkspaceState _state;

        private Category _category;
        private CategoryFieldTable _fieldTable;
        #endregion

        #region Constructor
        public CategoryFieldSection(
            WorkspaceState state,
            Action repaint,
            Action<Category> browseRequested,
            Action<Field> editRequested,
            Action<Rect> revealRequested)
        {
            _state = state;
            _repaint = repaint;
            _browseRequested = browseRequested;
            _editRequested = editRequested;
            _revealRequested = revealRequested;
        }
        #endregion

        #region Public API
        public void SetCategory(Category category)
        {
            _category = category;

            _header.SetCategory(
                category,
                _browseRequested,
                CreateField);

            if (category != null)
            {
                _fieldTable =
                    new CategoryFieldTable(
                        category,
                        _repaint,
                        _editRequested,
                        _revealRequested);
            }
            else
            {
                _fieldTable = null;
            }
        }

        public void Draw()
        {
            if (_category == null)
                return;

            bool expanded =
                _state.IsInspectorSectionExpanded(
                    InspectorSection.CategoryFields);

            SectionCard.Draw(
                () =>
                {
                    DrawHeader(
                        expanded);

                    if (expanded)
                    {
                        DrawContent();
                    }
                });
        }
        #endregion

        #region Draw
        private void DrawHeader(
            bool expanded)
        {
            _header.Draw(
                expanded,
                () =>
                    _state.ToggleInspectorSection(
                        InspectorSection.CategoryFields));
        }

        private void DrawContent()
        {
            _fieldTable?.Draw();
        }
        #endregion

        #region Commands
        private void CreateField(Category category)
        {
            if (category == null)
                return;

            Field field =
                _fieldService.CreateField(
                    category);

            if (field == null)
                return;

            _editRequested?.Invoke(
                field);

            _repaint?.Invoke();
        }
        #endregion
    }
}