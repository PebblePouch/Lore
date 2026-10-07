using System;
using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class EntryTagsHeaderSection
    {
        #region Fields
        private Entry _entry;

        private Action<Entry> _onBrowseRequested;
        private Action<Entry> _onCreateRequested;
        #endregion

        #region Public API
        public void SetEntry(
            Entry entry,
            Action<Entry> browseRequested,
            Action<Entry> createRequested)
        {
            _entry =
                entry;

            _onBrowseRequested =
                browseRequested;

            _onCreateRequested =
                createRequested;
        }

        public void Draw(
            bool expanded,
            Action toggle)
        {
            if (_entry == null)
                return;

            CollapsibleSectionHeader.Draw(
                string.Format(LoreText.Inspector.TagsHeader, _entry.TagIds.Count),
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
                () =>
                    _onBrowseRequested?.Invoke(
                        _entry),
                ButtonDrawer.DisplayMode.Glyph,
                BrowserStyle.BrowserHeaderButton);
        }

        private void DrawCreateButton()
        {
            ButtonDrawer.Create(
                () =>
                    _onCreateRequested?.Invoke(
                        _entry),
                ButtonDrawer.DisplayMode.Glyph,
                BrowserStyle.BrowserHeaderButton);
        }
        #endregion
    }
}