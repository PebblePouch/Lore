using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class FieldMenuElement
    {
        #region Fields
        private readonly FieldService _fieldService = new();

        private Field _field;
        private bool _editing;
        #endregion

        #region Callbacks
        private Action _edit;
        private Func<bool> _save;
        private Action _references;
        private Action _delete;
        private Action<Field> _duplicated;
        private Action _repaint;
        #endregion

        #region Public API
        public void SetField(
            Field field,
            bool editing,
            Action edit,
            Func<bool> save,
            Action references,
            Action delete,
            Action<Field> duplicated,
            Action repaint)
        {
            _field = field;
            _editing = editing;

            _edit = edit;
            _save = save;
            _references = references;
            _delete = delete;
            _duplicated = duplicated;
            _repaint = repaint;
        }

        public void Draw(Rect rect)
        {
            if (_field == null)
                return;

            EditorGUIUtility.AddCursorRect(
                rect,
                MouseCursor.Link);

            if (!GUI.Button(
                    rect,
                    LibraryStyle.MenuString,
                    LibraryStyle.Menu))
            {
                return;
            }

            var items = new List<LoreMenuItem>();

            if (_editing)
            {
                items.Add(
                    new LoreMenuItem(
                        LoreEditorTheme.Current.Save,
                        Save));
            }
            else
            {
                items.Add(
                    new LoreMenuItem(
                        LoreEditorTheme.Current.Edit,
                        Edit));
            }

            items.Add(
                new LoreMenuItem(
                    LoreText.Library.References,
                    ShowReferences,
                    separatorBefore: true));

            items.Add(
                new LoreMenuItem(
                    LoreEditorTheme.Current.Duplicate,
                    Duplicate,
                    separatorBefore: true));

            items.Add(
                new LoreMenuItem(
                    LoreEditorTheme.Current.Delete,
                    Delete,
                    destructive: true,
                    separatorBefore: true));

            LoreActionMenu.Show(
                rect,
                items);
        }
        #endregion

        #region Commands
        private void Edit()
        {
            _edit?.Invoke();

            _repaint?.Invoke();
        }

        private void ShowReferences()
        {
            _references?.Invoke();

            _repaint?.Invoke();
        }

        private void Save()
        {
            if (_save == null)
                return;

            if (!_save.Invoke())
                return;

            _repaint?.Invoke();
        }

        private void Duplicate()
        {
            if (_editing &&
                _save != null &&
                !_save.Invoke())
            {
                return;
            }

            Field duplicate =
                _fieldService.DuplicateField(
                    _field);

            if (duplicate == null)
                return;

            _duplicated?.Invoke(
                duplicate);

            _repaint?.Invoke();
        }

        private void Delete()
        {
            _delete?.Invoke();
        }
        #endregion
    }
}