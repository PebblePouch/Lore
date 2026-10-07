using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class EntryMenuElement
    {
        #region Fields
        private readonly EntryDeleteCommand _deleteCommand = new();

        private Entry _entry;

        private Rect _buttonRect;
        #endregion

        #region Public API
        public void SetEntry(Entry entry)
        {
            _entry = entry;
        }

        public void Draw()
        {
            if (_entry == null)
                return;

            DrawContent();
        }

        public void Duplicate()
        {
            DuplicateEntry();
        }

        public void Delete()
        {
            DeleteEntry();
        }
        #endregion

        #region Draw
        private void DrawContent()
        {
            GUILayout.BeginHorizontal(
                InspectorStyle.MenuLayout,
                GUILayout.Width(InspectorStyle.MenuWidth),
                GUILayout.Height(InspectorStyle.MenuHeight));

            DrawButton();

            GUILayout.EndHorizontal();
        }

        private void DrawButton()
        {
            bool clicked =
                ButtonDrawer.Menu(
                    null,
                    ButtonDrawer.DisplayMode.Glyph,
                    InspectorStyle.MenuContent,
                    GUILayout.ExpandWidth(true),
                    GUILayout.ExpandHeight(true));

            if (Event.current.type ==
                EventType.Repaint)
            {
                _buttonRect =
                    GUILayoutUtility.GetLastRect();
            }

            if (!clicked)
                return;

            LoreActionMenu.Show(
                _buttonRect,
                new[]
                {
            new LoreMenuItem(
                LoreEditorTheme.Current.Duplicate,
                DuplicateEntry),

            new LoreMenuItem(
                LoreEditorTheme.Current.Delete,
                DeleteEntry,
                destructive: true,
                separatorBefore: true)
                });
        }
        #endregion

        #region Commands
        private void DuplicateEntry()
        {
            Entry duplicate =
                new EntryService()
                    .DuplicateEntry(
                        _entry);

            if (duplicate == null)
                return;

            SelectionService.SelectEntry(
                duplicate);

            InspectorEditRequestService
                .RequestEntryTitle(
                    duplicate);

            ContentBrowserRequestService
                .RequestEntryReveal(
                    duplicate);

            LoreRepaintService.Repaint();
        }

        private void DeleteEntry()
        {
            _deleteCommand.Execute(
                _entry);
        }
        #endregion
    }
}