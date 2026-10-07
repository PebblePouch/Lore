using System;

using UnityEditor;

namespace PebblePouch.Lore.Editor
{
    // Keeps Lore's in-memory state (repository, relationship index, open views) in sync with its assets.
    // Performance: Lore must not slow down the game project it supports. It never rescans the project
    // (AssetDatabase.Refresh) and never saves foreign assets (AssetDatabase.SaveAssets): Lore creates and changes its
    // assets through the AssetDatabase, which already knows about them, and only Lore's own assets are saved.
    public static class LoreRefreshService
    {
        #region Fields
        public static event Action Refreshed;

        private static bool _externalRefreshPending;

        // Set during a Lore edit until the next editor tick: the imports caused by saving Lore's own assets would
        // otherwise trigger a second, identical rebuild.
        private static bool _ignoreExternalChanges;
        #endregion

        #region Public API
        // After a Lore edit: save Lore's changed assets and rebuild.
        public static void Refresh()
        {
            IgnoreExternalChangesUntilNextTick();

            SaveLoreAssets();

            Rebuild();
        }

        // Lore assets changed outside Lore's own edits (import, git pull, moved/deleted in the Project window):
        // rebuild once, after the import (see LoreAssetPostprocessor). Nothing to do while Lore was never opened:
        // the repository is built fresh on first use.
        public static void RequestExternalRefresh()
        {
            if (_externalRefreshPending ||
                _ignoreExternalChanges)
            {
                return;
            }

            _externalRefreshPending = true;

            EditorApplication.delayCall += () =>
            {
                _externalRefreshPending = false;

                if (!LoreRepository.HasInstance)
                    return;

                Rebuild();

                LoreRepaintService.Repaint();
            };
        }

        // Only assets under the data root and the Libraries; unchanged assets are skipped by Unity.
        public static void SaveLoreAssets()
        {
            string root = FolderService.Root;

            if (AssetDatabase.IsValidFolder(root))
            {
                foreach (string guid in
                         AssetDatabase.FindAssets(
                             string.Empty,
                             new[] { root }))
                {
                    AssetDatabase.SaveAssetIfDirty(
                        new GUID(guid));
                }
            }

            SaveIfDirty(FolderService.FindLibraryAsset<TagCollection>());
            SaveIfDirty(FolderService.FindLibraryAsset<GlyphLibraryCollection>());
            SaveIfDirty(FolderService.FindLibraryAsset<ColorLibraryCollection>());
        }
        #endregion

        #region Helpers
        private static void IgnoreExternalChangesUntilNextTick()
        {
            if (_ignoreExternalChanges)
                return;

            _ignoreExternalChanges = true;

            EditorApplication.delayCall += () => _ignoreExternalChanges = false;
        }

        private static void Rebuild()
        {
            LoreRepository.Instance.Refresh();

            LoreRelationshipIndex.Instance.Refresh();

            LoreDataVersion.Increment();

            Refreshed?.Invoke();
        }

        private static void SaveIfDirty(UnityEngine.Object asset)
        {
            if (asset != null)
                AssetDatabase.SaveAssetIfDirty(asset);
        }
        #endregion
    }
}
