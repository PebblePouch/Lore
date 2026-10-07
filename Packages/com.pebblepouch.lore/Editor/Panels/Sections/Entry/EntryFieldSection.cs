using System;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class EntryFieldSection
    {
        #region Fields
        private readonly Action<Entry> _browseRequested;
        private readonly Action<Entry> _createRequested;

        private readonly EntryFieldsHeaderSection _header = new();
        private readonly WorkspaceState _state;

        private Entry _entry;
        private readonly EntryFieldTable _fieldsElement;
        #endregion

        #region Constructor
        public EntryFieldSection(
            WorkspaceState state,
            Action<Rect> revealRequested)
        {
            _state = state;

            _fieldsElement =
                new EntryFieldTable(
                    revealRequested);
        }
        #endregion

        #region Public API
        public void SetEntry(Entry entry)
        {
            _entry = entry;

            _header.SetEntry(
                entry,
                _browseRequested,
                _createRequested);

            _fieldsElement.SetEntry(entry);
        }

        public void Draw()
        {
            if (_entry == null)
                return;

            bool expanded =
                _state.IsInspectorSectionExpanded(
                    InspectorSection.EntryFields);

            SectionCard.Draw(
                () =>
                {
                    _header.Draw(
                        expanded,
                        () =>
                            _state.ToggleInspectorSection(
                                InspectorSection.EntryFields));

                    if (expanded)
                    {
                        DrawContent();
                    }
                });
        }
        #endregion

        #region Draw
        private void DrawContent()
        {
            _fieldsElement.Draw();
        }
        #endregion

        #region Commands
        #endregion
    }
}