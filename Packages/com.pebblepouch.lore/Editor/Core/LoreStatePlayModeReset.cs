using UnityEditor;

namespace PebblePouch.Lore.Editor
{
    // Drops the runtime changes (LoreState) when leaving Play mode, so edit-mode code reads the Lore values again.
    [InitializeOnLoad]
    internal static class LoreStatePlayModeReset
    {
        #region Constructor
        static LoreStatePlayModeReset()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        #endregion

        #region Helpers
        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredEditMode)
                LoreState.Clear();
        }
        #endregion
    }
}
