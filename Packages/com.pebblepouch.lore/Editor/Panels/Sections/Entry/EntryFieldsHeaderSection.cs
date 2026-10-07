using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class EntryFieldsHeaderSection
    {
        #region Fields
        private Entry _entry;

        private System.Action<Entry> _onBrowseRequested;
        private System.Action<Entry> _onCreateRequested;
        #endregion

        #region Public API
        public void SetEntry(
            Entry entry,
            System.Action<Entry> browseRequested,
            System.Action<Entry> createRequested)
        {
            _entry = entry;
            _onBrowseRequested = browseRequested;
            _onCreateRequested = createRequested;
        }

        public void Draw(
            bool expanded,
            System.Action toggle)
        {
            if (_entry == null)
                return;

            CollapsibleSectionHeader.Draw(
                string.Format(LoreText.Inspector.FieldsHeader, _entry.Fields.Count),
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
            /*
            ButtonDrawer.Add(
                () => _onBrowseRequested?.Invoke(_entry),
                ButtonDrawer.DisplayMode.Glyph,
                BrowserStyle.BrowserHeaderButton);
            */
        }

        private void DrawCreateButton()
        {
            ButtonDrawer.Status(
                LoreEditorTheme.Current.Added,
                BrowserStyle.BrowserHeaderBlank);
            /*
            ButtonDrawer.Create(
                () => _onCreateRequested?.Invoke(_entry),
                ButtonDrawer.DisplayMode.Glyph,
                BrowserStyle.BrowserHeaderButton);
            */
        }
        #endregion
    }
}