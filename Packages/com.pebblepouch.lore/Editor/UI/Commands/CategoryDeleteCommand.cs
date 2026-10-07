using System;

namespace PebblePouch.Lore.Editor
{
    public sealed class CategoryDeleteCommand
    {
        #region Fields
        private readonly CategoryService _categoryService = new();
        private readonly CategoryDeleteAnalyzer _deleteAnalyzer = new();
        #endregion

        #region Public API
        public void Execute(
            Category category,
            Action<bool> completed = null)
        {
            if (category == null)
            {
                completed?.Invoke(false);

                return;
            }

            CategoryDeleteReport report =
                _deleteAnalyzer.Analyze(
                    category);

            if (!report.CanDelete)
            {
                LoreDialog.Warning(
                    string.Format(LoreText.Dialogs.CategoryTitle, category.Title),
                    GetBlockedDeleteWarning(
                        report),
                    acknowledged:
                        () => completed?.Invoke(false));

                return;
            }

            LoreDialog.ConfirmDestructive(
                string.Format(LoreText.Dialogs.CategoryTitle, category.Title),
                LoreText.Common.Paragraphs(
                    LoreText.Dialogs.CategoryDeleteWarning,
                    LoreText.Common.CannotBeUndone),
                LoreText.Common.DeleteButton,
                () =>
                {
                    bool deleted =
                        _categoryService.DeleteCategory(
                            category);

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

        #region Helpers
        private static string GetBlockedDeleteWarning(
            CategoryDeleteReport report)
        {
            return LoreText.Common.Paragraphs(
                string.Format(
                    LoreText.Dialogs.CategoryNotEmpty,
                    LoreText.Common.EntryCount.Format(report.EntryCount)),
                LoreText.Dialogs.CategoryNotEmptyAction.For(report.EntryCount));
        }
        #endregion
    }
}