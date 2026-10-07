using UnityEditor;

namespace PebblePouch.Lore.Editor
{
    public static class WorkspacePersistence
    {
        #region Fields
        private const string SelectedCategoryKey = "Lore.SelectedCategory";
        private const string SelectedEntryKey = "Lore.SelectedEntry";
        #endregion

        #region Public API
        public static Category LoadCategory()
        {
            string path =
                EditorPrefs.GetString(
                    SelectedCategoryKey,
                    string.Empty);

            if (string.IsNullOrEmpty(path))
                return null;

            return AssetDatabase.LoadAssetAtPath<Category>(path);
        }

        public static Entry LoadEntry()
        {
            string path =
                EditorPrefs.GetString(
                    SelectedEntryKey,
                    string.Empty);

            if (string.IsNullOrEmpty(path))
                return null;

            return AssetDatabase.LoadAssetAtPath<Entry>(path);
        }

        public static void SaveCategory(Category category)
        {
            if (category == null)
            {
                EditorPrefs.DeleteKey(SelectedCategoryKey);
                return;
            }

            EditorPrefs.SetString(
                SelectedCategoryKey,
                AssetDatabase.GetAssetPath(category));
        }

        public static void SaveEntry(Entry entry)
        {
            if (entry == null)
            {
                EditorPrefs.DeleteKey(SelectedEntryKey);
                return;
            }

            EditorPrefs.SetString(
                SelectedEntryKey,
                AssetDatabase.GetAssetPath(entry));
        }
        #endregion
    }
}