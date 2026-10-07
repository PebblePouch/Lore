using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class CategoryFieldsHeaderSection
    {
        #region Fields
        private Category _category;

        private System.Action<Category> _onBrowseRequested;
        private System.Action<Category> _onCreateRequested;
        #endregion

        #region Public API
        public void SetCategory(
            Category category,
            System.Action<Category> browseRequested,
            System.Action<Category> createRequested)
        {
            _category = category;
            _onBrowseRequested = browseRequested;
            _onCreateRequested = createRequested;
        }

        public void Draw(
            bool expanded,
            System.Action toggle)
        {
            if (_category == null)
                return;

            CollapsibleSectionHeader.Draw(
                string.Format(LoreText.Inspector.FieldsHeader, _category.Fields.Count),
                expanded,
                toggle,
                () =>
                {
                    DrawBrowseButton();
                    DrawCreateButton();
                });
        }
        #endregion

        #region Draw
        private void DrawBrowseButton()
        {
            ButtonDrawer.Add(
                () => _onBrowseRequested?.Invoke(_category),
                ButtonDrawer.DisplayMode.Glyph,
                BrowserStyle.BrowserHeaderButton);
        }

        private void DrawCreateButton()
        {
            ButtonDrawer.Create(
                () => _onCreateRequested?.Invoke(_category),
                ButtonDrawer.DisplayMode.Glyph,
                BrowserStyle.BrowserHeaderButton);
        }
        #endregion
    }
}