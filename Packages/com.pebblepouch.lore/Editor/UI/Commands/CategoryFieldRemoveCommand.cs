using System;
using System.Collections.Generic;

namespace PebblePouch.Lore.Editor
{
    public sealed class CategoryFieldRemoveCommand
    {
        #region Fields
        private readonly FieldService _fieldService =
            new();

        private readonly CategoryFieldUsageAnalyzer _usageAnalyzer =
            new(LoreRepository.Instance);
        #endregion

        #region Public API
        public void Execute(
            Category category,
            Field field,
            Action<bool> completed = null)
        {
            if (category == null ||
                field == null)
            {
                completed?.Invoke(
                    false);

                return;
            }

            IReadOnlyList<Entry> entriesWithValues =
                _usageAnalyzer
                    .GetEntriesWithValues(
                        category,
                        field);

            if (entriesWithValues.Count == 0)
            {
                Remove(
                    category,
                    field,
                    completed);

                return;
            }

            LoreDialog.ConfirmDestructive(
                string.Format(LoreText.Dialogs.RemoveFieldTitle, field.name),
                LoreText.Common.Paragraphs(
                    LoreText.Dialogs.FieldRemoveWarning.Format(entriesWithValues.Count),
                    LoreText.Common.CannotBeUndone),
                LoreText.Common.RemoveButton,
                () =>
                    Remove(
                        category,
                        field,
                        completed),
                cancelled:
                    () =>
                        completed?.Invoke(
                            false));
        }
        #endregion

        #region Helpers
        private void Remove(
            Category category,
            Field field,
            Action<bool> completed)
        {
            bool removed =
                _fieldService.RemoveField(
                    category,
                    field);

            if (removed)
            {
                LoreRepaintService.Repaint();
            }

            completed?.Invoke(
                removed);
        }
        #endregion
    }
}