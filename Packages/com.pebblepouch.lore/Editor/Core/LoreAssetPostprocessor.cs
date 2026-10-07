using System;

using UnityEditor;

namespace PebblePouch.Lore.Editor
{
    // Reports Lore-relevant asset changes (instead of projectChanged, which fires for every import in the game
    // project). Relevant: anything under the data root, assets of a Lore type (the Libraries can live anywhere),
    // deleted .asset files (may have been Lore assets) and moved/renamed scenes (Scene field paths).
    // Everything else costs a few string checks per imported path.
    public sealed class LoreAssetPostprocessor : AssetPostprocessor
    {
        #region AssetPostprocessor
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            // Lore not used in this session: the repository is built fresh on first use.
            if (!LoreRepository.HasInstance)
                return;

            // Any deleted or moved asset can turn a field value into a missing reference: cached analyses recalculate.
            if (deletedAssets.Length > 0 ||
                movedAssets.Length > 0)
            {
                LoreDataVersion.Increment();

                LoreRepaintService.Repaint();
            }
            // The generated Field constants were written or edited: the outdated-constants notice recalculates.
            else if (ContainsPath(importedAssets, LoreSettings.instance.LastFieldConstantsPath))
            {
                LoreDataVersion.Increment();

                LoreRepaintService.Repaint();
            }

            string root =
                FolderService.Root + "/";

            if (ContainsLoreChange(importedAssets, root, checkType: true) ||
                ContainsLoreChange(movedAssets, root, checkType: true) ||
                ContainsLoreChange(movedFromAssetPaths, root, checkType: false) ||
                ContainsDeletedAsset(deletedAssets) ||
                ContainsScene(movedAssets))
            {
                LoreRefreshService.RequestExternalRefresh();
            }
        }
        #endregion

        #region Helpers
        private static bool ContainsLoreChange(
            string[] paths,
            string root,
            bool checkType)
        {
            foreach (string path in paths)
            {
                // Scripts are never Lore data (e.g. the generated Field constants next to the Project).
                if (path.StartsWith(
                        root,
                        StringComparison.Ordinal) &&
                    !path.EndsWith(
                        ".cs",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (checkType &&
                    path.EndsWith(
                        ".asset",
                        StringComparison.OrdinalIgnoreCase) &&
                    IsLoreType(
                        AssetDatabase.GetMainAssetTypeAtPath(
                            path)))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsPath(
            string[] paths,
            string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            foreach (string candidate in paths)
            {
                if (string.Equals(
                        candidate,
                        path,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsDeletedAsset(string[] paths)
        {
            foreach (string path in paths)
            {
                if (path.EndsWith(
                        ".asset",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsScene(string[] paths)
        {
            foreach (string path in paths)
            {
                if (path.EndsWith(
                        ".unity",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsLoreType(Type type)
        {
            return type != null &&
                   (typeof(LoreAsset).IsAssignableFrom(type) ||
                    type == typeof(TagCollection) ||
                    type == typeof(GlyphLibraryCollection) ||
                    type == typeof(ColorLibraryCollection));
        }
        #endregion
    }
}
