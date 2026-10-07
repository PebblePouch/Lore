using System.Collections.Generic;
using System.Linq;

namespace PebblePouch.Lore.Editor
{
    // Tag Library picks for several Entries (Tabular Editing selection: "Add Tag"). Without it, a pick goes to the
    // current Entry. Ended when the Library closes.
    public static class TagAssignmentContext
    {
        #region Fields
        private static List<Entry> _entries;

        public static bool IsActive => _entries != null;

        public static IReadOnlyList<Entry> Entries => _entries;
        #endregion

        #region Public API
        public static void Begin(IEnumerable<Entry> entries)
        {
            LoreGUIFocus.Clear();

            _entries = entries.Where(entry => entry != null).ToList();
        }

        public static void End()
        {
            _entries = null;
        }
        #endregion
    }
}
