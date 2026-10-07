using System;
using System.Collections.Generic;

using UnityEditor;

namespace PebblePouch.Lore.Editor
{
    // Created/Modified timestamps of Lore assets (Project, Categories, Entries, Fields).
    // Use MarkModified instead of EditorUtility.SetDirty for every change EXCEPT reordering
    // (sort orders, moving Fields/Links within their list): reordering only calls SetDirty.
    public static class LoreChangeTracker
    {
        #region Events
        // Raised after an asset was created or modified (e.g. to update "latest modified" lists).
        // Not every edit triggers LoreRefreshService.Refresh (field values only save and repaint).
        public static event Action Changed;
        #endregion

        #region Public API
        // New asset (or duplicate): created = modified = now.
        public static void MarkCreated(LoreAsset asset)
        {
            if (asset == null)
                return;

            asset.InitializeTimestamps(
                DateTime.UtcNow.Ticks);

            EditorUtility.SetDirty(asset);

            IncrementVersion(asset);

            Changed?.Invoke();
        }

        public static void MarkModified(LoreAsset asset)
        {
            if (asset == null)
                return;

            asset.MarkModified(
                DateTime.UtcNow.Ticks);

            EditorUtility.SetDirty(asset);

            IncrementVersion(asset);

            Changed?.Invoke();
        }

        // Assets without timestamps (created before timestamps existed, or outside Lore) get the current time,
        // so no asset in a project has empty timestamps.
        public static void EnsureTimestamps(IEnumerable<LoreAsset> assets)
        {
            long now = DateTime.UtcNow.Ticks;

            foreach (LoreAsset asset in assets)
            {
                if (asset != null &&
                    asset.EnsureTimestamps(now))
                {
                    EditorUtility.SetDirty(asset);
                }
            }
        }
        #endregion

        #region Helpers
        // Entries and the Project only hold content; Categories and Fields are definitions (see LoreDataVersion).
        private static void IncrementVersion(LoreAsset asset)
        {
            if (asset is Entry or LoreProject)
                LoreDataVersion.IncrementContent();
            else
                LoreDataVersion.Increment();
        }
        #endregion
    }
}
