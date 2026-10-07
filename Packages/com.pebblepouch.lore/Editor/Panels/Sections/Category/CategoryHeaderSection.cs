using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class CategoryHeaderSection
    {
        #region Fields
        private readonly CategoryColorElement _colorElement;
        private readonly CategoryGlyphElement _glyphElement;
        private readonly CategoryTitleElement _metadataElement;
        private readonly CategoryActionsElement _actionsElement;
        private readonly CategoryMenuElement _menuElement;
        private readonly TimestampsElement _timestampsElement = new();

        private Category _category;
        #endregion

        #region Properties
        public bool IsEditing => _metadataElement.IsEditing;
        #endregion

        #region Constructor
        public CategoryHeaderSection()
        {
            _colorElement =
                new CategoryColorElement();

            _glyphElement =
                new CategoryGlyphElement();

            _metadataElement =
                new CategoryTitleElement();

            _actionsElement =
                new CategoryActionsElement();

            _menuElement =
                new CategoryMenuElement();
        }
        #endregion

        #region Public API
        public void SetCategory(
            Category category)
        {
            _category = category;

            _colorElement.SetCategory(
                category);

            _glyphElement.SetCategory(
                category);

            _metadataElement.SetCategory(
                category);

            _actionsElement.SetCategory(
                category);

            _timestampsElement.SetAsset(
                category);

            _menuElement.SetCategory(
                category);
        }

        public bool HandleOutsideClick(
            Vector2 mousePosition)
        {
            return _metadataElement.HandleOutsideClick(
                mousePosition);
        }

        public void BeginTitleEditing()
        {
            _metadataElement.BeginEditing();
        }

        public void AddEntry()
        {
            _menuElement.AddEntry();
        }

        public void Delete()
        {
            _menuElement.Delete();
        }

        public void Draw()
        {
            if (_category == null)
                return;

            EditorGUILayout.BeginHorizontal(
                InspectorStyle.HeaderLayout,
                GUILayout.Height(
                    InspectorStyle.HeaderHeight));

            _colorElement.Draw();
            _glyphElement.Draw();
            _metadataElement.Draw();
            _actionsElement.Draw();
            _timestampsElement.Draw();
            _menuElement.Draw();

            EditorGUILayout.EndHorizontal();

            _metadataElement.DrawFeedback();
        }
        #endregion
    }
}