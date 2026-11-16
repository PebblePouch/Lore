using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Brings the Lore data up to LoreProject.CurrentFormatVersion. Runs on every repository rebuild (cheap when the
    // Project is current), so imported Projects, the Oz Demo and data from older Lore versions are upgraded before
    // anything reads them, and builds always contain current data. Never downgrades: data from a newer Lore version
    // is left untouched with a warning.
    public static class LoreFormatUpgradeService
    {
        #region Fields
        // Warn once per domain load, not on every rebuild.
        private static bool _newerFormatWarned;
        #endregion

        #region Public API
        public static void Upgrade(LoreProject project)
        {
            if (project == null)
                return;

            int version = project.FormatVersion;

            if (version == LoreProject.CurrentFormatVersion)
                return;

            if (version > LoreProject.CurrentFormatVersion)
            {
                WarnNewerFormat(project);

                return;
            }

            // One step per format, in order. 0 (before format versions) has the layout of format 1.
            // Example for a future format 2:
            //   if (version < 2) UpgradeToFormat2(...);
            // A step changes the assets (SetDirty) and must be safe to run again on already upgraded data.

            // Derived data, no content change: the Modified timestamp stays (SetDirty, like the runtime index).
            project.SetFormatVersion(LoreProject.CurrentFormatVersion);

            EditorUtility.SetDirty(project);
        }

        // New Projects start with the current format.
        public static void StampCurrent(LoreProject project)
        {
            if (project != null)
                project.SetFormatVersion(LoreProject.CurrentFormatVersion);
        }
        #endregion

        #region Helpers
        private static void WarnNewerFormat(LoreProject project)
        {
            if (_newerFormatWarned)
                return;

            _newerFormatWarned = true;

            Debug.LogWarning(
                string.Format(
                    LoreText.DataFormat.NewerFormatWarning,
                    AssetDatabase.GetAssetPath(project),
                    project.FormatVersion,
                    LoreProject.CurrentFormatVersion),
                project);
        }
        #endregion
    }
}
