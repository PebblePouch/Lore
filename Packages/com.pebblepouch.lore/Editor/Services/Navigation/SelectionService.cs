using System;

namespace PebblePouch.Lore.Editor
{
    public static class SelectionService
    {
        #region Fields
        public static Category CurrentCategory { get; private set; }
        public static Entry CurrentEntry { get; private set; }

        public static event Action<Category> CategoryChanged;
        public static event Action<Entry> EntryChanged;
        #endregion

        #region Public API
        public static void SelectProject()
        {
            EndEntryAssignment();

            if (CurrentCategory != null)
            {
                CurrentCategory = null;

                CategoryChanged?.Invoke(
                    null);
            }

            if (CurrentEntry != null)
            {
                CurrentEntry = null;

                EntryChanged?.Invoke(
                    null);
            }

            LoreKeyboardFocus.SetOwner(
                LoreKeyboardOwner.Inspector);

            RecordHistory();
        }

        public static void SelectCategory(Category category)
        {
            if (category != null)
            {
                LoreKeyboardFocus.SetOwner(LoreKeyboardOwner.Inspector);
            }

            if (CurrentCategory == category && CurrentEntry == null)
            {
                return;
            }

            EndEntryAssignment();

            if (CurrentEntry != null)
            {
                CurrentEntry = null;

                EntryChanged?.Invoke(null);
            }

            CurrentCategory = category;

            CategoryChanged?.Invoke(category);

            RecordHistory();
        }

        public static void SelectEntry(Entry entry)
        {
            if (entry != null)
            {
                LoreKeyboardFocus.SetOwner(LoreKeyboardOwner.Inspector);
            }

            if (CurrentEntry == entry && CurrentCategory == null)
            {
                return;
            }

            EndEntryAssignment();

            if (CurrentCategory != null)
            {
                CurrentCategory = null;

                CategoryChanged?.Invoke(null);
            }

            CurrentEntry = entry;

            EntryChanged?.Invoke(entry);

            RecordHistory();
        }

        public static void Clear()
        {
            EndEntryAssignment();

            if (CurrentCategory != null)
            {
                CurrentCategory = null;

                CategoryChanged?.Invoke(null);
            }

            if (CurrentEntry != null)
            {
                CurrentEntry = null;

                EntryChanged?.Invoke(null);
            }

            LoreKeyboardFocus.SetOwner(LoreKeyboardOwner.Content);

            RecordHistory();
        }
        #endregion

        #region Helpers
        private static void RecordHistory()
        {
            NavigationHistoryService.Record(
                CurrentCategory,
                CurrentEntry);
        }

        private static void EndEntryAssignment()
        {
            if (EntryAssignmentContext.IsActive)
            {
                EntryAssignmentContext.EndAssignment();
            }
        }
        #endregion
    }
}