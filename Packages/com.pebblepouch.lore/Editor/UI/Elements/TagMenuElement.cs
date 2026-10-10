using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class TagMenuElement
    {
        #region Fields
        private TagDefinition _tag;
        private bool _editing;
        #endregion

        #region Callbacks
        private Action _edit;
        private Func<bool> _save;
        private Action _cancel;
        private Action _references;
        private Action _delete;
        private Action _repaint;
        #endregion

        #region Public API
        public void SetTag(
            TagDefinition tag,
            bool editing,
            Action edit,
            Func<bool> save,
            Action cancel,
            Action references,
            Action delete,
            Action repaint)
        {
            _tag = tag;
            _editing = editing;
            _edit = edit;
            _save = save;
            _cancel = cancel;
            _references = references;
            _delete = delete;
            _repaint = repaint;
        }

        public void Draw(Rect rect)
        {
            if (_tag == null)
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
                    LoreEditorTheme.Current.References,
                    ShowReferences,
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

        private void Save()
        {
            if (_save == null)
                return;

            if (!_save.Invoke())
                return;

            _repaint?.Invoke();
        }

        private void ShowReferences()
        {
            _references?.Invoke();

            _repaint?.Invoke();
        }

        private void Delete()
        {
            _delete?.Invoke();
        }
        #endregion
    }
}