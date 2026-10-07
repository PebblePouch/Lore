using System;

namespace PebblePouch.Lore.Editor
{
    public sealed class EntryDeleteCommand
    {
        #region Fields
        private readonly EntryService _entryService = new();
        #endregion

        #region Public API
        public void Execute(
            Entry entry,
            Action<bool> completed = null)
        {
            if (entry == null)
            {
                completed?.Invoke(false);

                return;
            }

            LoreDialog.ConfirmDestructive(
                string.Format(LoreText.Dialogs.EntryTitle, entry.Title),
                LoreText.Common.Paragraphs(
                    LoreText.Dialogs.EntryDeleteWarning,
                    LoreText.Common.CannotBeUndone),
                LoreText.Common.DeleteButton,
                () =>
                {
                    bool deleted =
                        _entryService.DeleteEntry(
                            entry);

                    if (deleted &&
                        SelectionService.CurrentEntry == entry)
                    {
                        SelectionService.Clear();
                    }

                    if (deleted)
                    {
                        LoreRepaintService.Repaint();
                    }

                    completed?.Invoke(
                        deleted);
                },
                cancelled:
                    () => completed?.Invoke(false));
        }
        #endregion
    }
}