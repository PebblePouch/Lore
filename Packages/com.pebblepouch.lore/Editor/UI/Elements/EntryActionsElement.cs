using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class EntryActionsElement
    {
        #region Fields
        private Entry _entry;
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

            // Only as wide as its buttons: a plain group stretches and takes width from title and subtitle.
            GUILayout.BeginHorizontal(
                GUILayout.ExpandWidth(false));

            DrawRenameButton();
            DrawMoveButton();
            DrawDuplicateButton();
            DrawDeleteButton();

            GUILayout.EndHorizontal();
        }
        #endregion

        #region Draw
        private void DrawRenameButton()
        {
            //ButtonDrawer.Rename(Rename);
        }

        private void DrawMoveButton()
        {
            //ButtonDrawer.Move(Move);
        }

        private void DrawDuplicateButton()
        {
            //ButtonDrawer.Duplicate(Duplicate);
        }

        private void DrawDeleteButton()
        {
            //ButtonDrawer.Delete(Delete);
        }
        #endregion

        #region Commands
        private void Rename() { }

        private void Move() { }

        private void Duplicate() { }

        private void Delete() { }
        #endregion
    }
}