using System.Collections.Generic;
using System.Linq;

using UnityEditor;

using Object = UnityEngine.Object;

namespace PebblePouch.Lore.Editor
{
    // Undo for Lore edits. Call Record BEFORE changing an asset: one user action = one undo step, named
    // "Lore: <action>" (LoreText.Undo). Not undoable, like in Unity's Project window: creating, duplicating,
    // deleting and renaming assets (file operations), and actions confirmed as "cannot be undone".
    // After undo/redo of a Lore step, Lore saves and rebuilds like after any edit.
    [InitializeOnLoad]
    public static class LoreUndo
    {
        #region Fields
        // Typing in one input (key = control name) is merged into one step while nothing else happened.
        private static string _typingKey;
        private static int _typingGroup = -1;
        private static int _lastGroup = -1;
        #endregion

        #region Constructor
        static LoreUndo()
        {
            Undo.undoRedoEvent += OnUndoRedo;
        }
        #endregion

        #region Public API
        public static void Record(
            Object asset,
            string action)
        {
            if (asset == null)
                return;

            RecordObjects(
                new[] { asset },
                action);

            _typingKey = null;
        }

        public static void Record(
            IEnumerable<Object> assets,
            string action)
        {
            Object[] objects =
                assets
                    .Where(asset => asset != null)
                    .ToArray();

            if (objects.Length == 0)
                return;

            RecordObjects(
                objects,
                action);

            _typingKey = null;
        }

        // Text and number inputs record on every keystroke: consecutive changes of the same input become one step,
        // unless other undo steps happened in between.
        public static void RecordTyping(
            Object asset,
            string action,
            string inputKey)
        {
            if (asset == null)
                return;

            int group =
                Undo.GetCurrentGroup();

            bool continues =
                inputKey != null &&
                inputKey == _typingKey &&
                _typingGroup >= 0 &&
                group <= _lastGroup + 1;

            RecordObjects(
                new[] { asset },
                action);

            if (continues)
            {
                Undo.CollapseUndoOperations(
                    _typingGroup);
            }
            else
            {
                _typingGroup = group;
            }

            _typingKey = inputKey;
            _lastGroup = Undo.GetCurrentGroup();
        }

        // Undo restores an asset's whole recorded state: undoing an earlier step would also revert later changes
        // that were not recorded. Call after renaming an asset (file name and asset name must stay in sync).
        public static void Forget(Object asset)
        {
            if (asset != null)
                Undo.ClearUndo(asset);

            _typingKey = null;
        }

        // After actions confirmed as "cannot be undone" (deleting, removing, clearing set values): no earlier Lore
        // step may bring anything back, so the undo history of all Lore assets ends here.
        public static void ForgetAll()
        {
            _typingKey = null;

            if (!LoreRepository.HasInstance)
                return;

            LoreRepository repository = LoreRepository.Instance;

            Forget(repository.Project);

            foreach (Category category in repository.Categories)
            {
                Forget(category);

                if (category == null)
                    continue;

                foreach (Field field in category.Fields)
                    Forget(field);
            }

            foreach (Entry entry in repository.Entries)
                Forget(entry);

            Forget(FolderService.FindAsset<TagCollection>());
        }
        #endregion

        #region Helpers
        private static void RecordObjects(
            Object[] assets,
            string action)
        {
            string name =
                LoreText.Undo.Prefix + action;

            Undo.RecordObjects(
                assets,
                name);

            Undo.SetCurrentGroupName(
                name);
        }

        private static void OnUndoRedo(in UndoRedoInfo info)
        {
            if (info.undoName == null ||
                !info.undoName.StartsWith(LoreText.Undo.Prefix))
            {
                return;
            }

            _typingKey = null;

            // Open inline editors would show (and later save) their old buffered text.
            LoreEditService.StopAllEditing();

            // Save the restored state like after any Lore edit; rebuild only if Lore is in use.
            if (LoreRepository.HasInstance)
                LoreRefreshService.Refresh();
            else
                LoreRefreshService.SaveLoreAssets();

            LoreRepaintService.Repaint();
        }
        #endregion
    }
}
