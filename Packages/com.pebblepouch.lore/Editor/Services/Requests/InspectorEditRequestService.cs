using System;

namespace PebblePouch.Lore.Editor
{
    public static class InspectorEditRequestService
    {
        #region Fields
        private static Category _categoryTitle;
        private static Action _categoryTitleEditingEnded;

        private static Entry _entryTitle;
        private static Action _entryTitleEditingEnded;
        #endregion

        #region Public API
        public static void RequestCategoryTitle(
            Category category,
            Action editingEnded = null)
        {
            _categoryTitle = category;
            _categoryTitleEditingEnded = editingEnded;
        }

        public static void RequestEntryTitle(
            Entry entry,
            Action editingEnded = null)
        {
            _entryTitle = entry;
            _entryTitleEditingEnded = editingEnded;
        }

        public static bool ConsumeCategoryTitle(
            Category category,
            out Action editingEnded)
        {
            editingEnded = null;

            if (_categoryTitle == null ||
                _categoryTitle != category)
            {
                return false;
            }

            _categoryTitle = null;

            editingEnded = _categoryTitleEditingEnded;
            _categoryTitleEditingEnded = null;

            return true;
        }

        public static bool ConsumeEntryTitle(
            Entry entry,
            out Action editingEnded)
        {
            editingEnded = null;

            if (_entryTitle == null ||
                _entryTitle != entry)
            {
                return false;
            }

            _entryTitle = null;

            editingEnded = _entryTitleEditingEnded;
            _entryTitleEditingEnded = null;

            return true;
        }
        #endregion
    }
}