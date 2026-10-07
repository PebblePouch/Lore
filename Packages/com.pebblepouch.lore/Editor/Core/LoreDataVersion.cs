using UnityEditor;

namespace PebblePouch.Lore.Editor
{
    // Counter that goes up on every change that can affect what Lore shows: rebuilds (LoreRefreshService), edits
    // (LoreChangeTracker), undo/redo and deleted/moved assets (LoreAssetPostprocessor: missing references).
    // Views cache their analyses per version (LoreCache) instead of recalculating them on every OnGUI call.
    public static class LoreDataVersion
    {
        #region Fields
        private static int _current = 1;
        private static int _definitions = 1;
        #endregion

        #region Constructor
        static LoreDataVersion()
        {
            Undo.undoRedoPerformed += Increment;
        }
        #endregion

        #region Properties
        public static int Current => _current;

        // Like Current, but unchanged by content edits (Entries, Project): for analyses that only depend on
        // Categories, Fields and settings (e.g. outdated Field constants), so they do not recalculate per keystroke.
        public static int Definitions => _definitions;
        #endregion

        #region Public API
        public static void Increment()
        {
            _current++;
            _definitions++;
        }

        // Edit of an Entry or the Project (values, text, tags): Definitions stays.
        public static void IncrementContent()
        {
            _current++;
        }
        #endregion
    }
}
