using UnityEditor;

namespace PebblePouch.Lore.Editor
{
    public static class LorePreferences
    {
        #region Keys
        private const string BrowserRenameSelectionModeKey = "Lore.BrowserRenameSelectionMode";
        private const string LastCsvFolderKey = "Lore.LastCsvFolder";
        #endregion

        #region Properties
        public static BrowserRenameSelectionMode BrowserRenameSelectionMode
        {
            get =>
                (BrowserRenameSelectionMode)
                EditorPrefs.GetInt(
                    BrowserRenameSelectionModeKey,
                    (int)BrowserRenameSelectionMode.SelectRenamedItem);

            set =>
                EditorPrefs.SetInt(
                    BrowserRenameSelectionModeKey,
                    (int)value);
        }

        // Folder of the last CSV export/import (file dialogs start there); empty = the dialog's default.
        public static string LastCsvFolder
        {
            get => EditorPrefs.GetString(LastCsvFolderKey, string.Empty);
            set => EditorPrefs.SetString(LastCsvFolderKey, value ?? string.Empty);
        }
        #endregion
    }
}