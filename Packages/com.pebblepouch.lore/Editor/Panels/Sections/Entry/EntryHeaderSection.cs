using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class EntryHeaderSection
    {
        #region Fields
        private readonly EntryColorElement _colorElement;
        private readonly EntryGlyphElement _glyphElement;
        private readonly EntryMetadataElement _metadataElement;
        private readonly EntryActionsElement _actionsElement;
        private readonly EntryMenuElement _menuElement;
        private readonly TimestampsElement _timestampsElement = new();

        private Entry _entry;
        #endregion

        #region Properties
        public bool IsEditing => _metadataElement.IsEditing;
        #endregion

        #region Constructor
        public EntryHeaderSection()
        {
            _colorElement =
                new EntryColorElement();

            _glyphElement =
                new EntryGlyphElement();

            _metadataElement =
                new EntryMetadataElement();

            _actionsElement =
                new EntryActionsElement();

            _menuElement =
                new EntryMenuElement();
        }
        #endregion

        #region Public API
        public void SetEntry(
            Entry entry)
        {
            _entry = entry;

            _colorElement.SetEntry(
                entry);

            _glyphElement.SetEntry(
                entry);

            _metadataElement.SetEntry(
                entry);

            _actionsElement.SetEntry(
                entry);

            _timestampsElement.SetAsset(
                entry);

            _menuElement.SetEntry(
                entry);
        }

        public void CommitEditing()
        {
            _metadataElement.CommitEditing();
        }

        public bool HandleOutsideClick(
            Vector2 mousePosition)
        {
            return _metadataElement.HandleOutsideClick(
                mousePosition);
        }

        public void BeginTitleEditing()
        {
            _metadataElement.BeginTitleEditing();
        }

        public void Duplicate()
        {
            _menuElement.Duplicate();
        }

        public void Delete()
        {
            _menuElement.Delete();
        }

        public void Draw()
        {
            if (_entry == null)
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