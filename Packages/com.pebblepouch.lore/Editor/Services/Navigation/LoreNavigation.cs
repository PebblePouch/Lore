using UnityEditor;

namespace PebblePouch.Lore.Editor
{
    // Shared "go to" actions used by Insights, integrity callouts, field rows and description links.
    // Each one selects its target, reveals it in the Content browser (clearing search/filter if needed)
    // and optionally scrolls the Inspector to a field. Delayed so it never runs mid-OnGUI.
    public static class LoreNavigation
    {
        #region Public API
        public static void NavigateToEntry(Entry entry)
        {
            NavigateToEntryField(
                entry,
                null);
        }

        // field: optional; when set, the Inspector scrolls to that field.
        public static void NavigateToEntryField(
            Entry entry,
            Field field)
        {
            if (entry == null)
                return;

            EditorApplication.delayCall +=
                () =>
                {
                    if (entry == null)
                        return;

                    if (field != null)
                    {
                        EntryFieldRevealRequestService.Request(
                            entry,
                            field);
                    }

                    SelectionService.SelectEntry(
                        entry);

                    ContentBrowserRequestService.RequestEntryReveal(
                        entry);

                    LoreRepaintService.Repaint();
                };
        }

        public static void NavigateToCategory(Category category)
        {
            NavigateToCategoryField(
                category,
                -1);
        }

        // fieldIndex: optional (-1 = none); when set, the Inspector scrolls to that field.
        public static void NavigateToCategoryField(
            Category category,
            int fieldIndex)
        {
            if (category == null)
                return;

            EditorApplication.delayCall +=
                () =>
                {
                    if (category == null)
                        return;

                    if (fieldIndex >= 0)
                    {
                        CategoryFieldRevealRequestService.Request(
                            category,
                            fieldIndex);
                    }

                    SelectionService.SelectCategory(
                        category);

                    ContentBrowserRequestService.RequestCategoryReveal(
                        category);

                    LoreRepaintService.Repaint();
                };
        }
        #endregion
    }
}
