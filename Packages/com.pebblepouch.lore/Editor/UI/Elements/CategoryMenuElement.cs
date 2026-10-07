using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class CategoryMenuElement
    {
        #region Fields
        private readonly CategoryDeleteCommand _deleteCommand = new();
        private readonly EntryService _entryService = new();

        private Category _category;

        private Rect _buttonRect;
        #endregion

        #region Public API
        public void SetCategory(Category category)
        {
            _category = category;
        }

        public void Draw()
        {
            if (_category == null)
                return;

            DrawContent();
        }

        public void AddEntry()
        {
            CreateEntry();
        }

        public void Delete()
        {
            DeleteCategory();
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
                        LoreEditorTheme.Current.Add,
                        CreateEntry),

                    new LoreMenuItem(
                        LoreEditorTheme.Current.ExportCsv,
                        ExportCsv,
                        separatorBefore: true),

                    new LoreMenuItem(
                        LoreEditorTheme.Current.ImportCsv,
                        ImportCsv),

                    new LoreMenuItem(
                        LoreEditorTheme.Current.Delete,
                        DeleteCategory,
                        destructive: true,
                        separatorBefore: true)
                            });
        }
        #endregion

        #region Commands
        // After the menu closes: the save dialog is modal.
        private void ExportCsv()
        {
            Category category = _category;

            EditorApplication.delayCall += () => CategoryCsvExporter.ExportWithDialog(category);
        }

        private void ImportCsv()
        {
            Category category = _category;

            EditorApplication.delayCall += () => CategoryCsvImporter.ImportWithDialog(category);
        }

        private void DeleteCategory()
        {
            _deleteCommand.Execute(
                _category);
        }

        private void CreateEntry()
        {
            if (_category == null)
                return;

            Entry entry =
                _entryService.CreateEntry(
                    _category,
                    LoreText.Defaults.NewEntry);

            if (entry == null)
                return;

            SelectionService.SelectEntry(
                entry);

            InspectorEditRequestService.RequestEntryTitle(
                entry);

            ContentBrowserRequestService.RequestEntryReveal(
                entry);

            LoreKeyboardFocus.SetOwner(
                LoreKeyboardOwner.Inspector);

            LoreRepaintService.Repaint();
        }
        #endregion
    }
}