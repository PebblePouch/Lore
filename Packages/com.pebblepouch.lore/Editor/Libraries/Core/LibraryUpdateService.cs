using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // After a Lore update with new default Libraries (a raised CatalogVersion): asks once whether to update all,
    // append only new or skip, then stamps the Libraries with the current catalog version. Checked when the Lore
    // window opens (also after the domain reload of an update), never while Unity compiles, imports or plays.
    public static class LibraryUpdateService
    {
        #region Fields
        private static bool _pending;
        #endregion

        #region Public API
        public static void CheckWhenReady()
        {
            if (_pending)
                return;

            _pending = true;

            EditorApplication.update += CheckWhenIdle;
        }
        #endregion

        #region Helpers
        private static void CheckWhenIdle()
        {
            if (EditorApplication.isCompiling ||
                EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            EditorApplication.update -= CheckWhenIdle;

            _pending = false;

            Check();
        }

        private static void Check()
        {
            // Find only: an update never creates a Library (missing ones are created up to date on first use).
            GlyphLibraryCollection glyphs =
                FolderService.FindLibraryAsset<GlyphLibraryCollection>();

            ColorLibraryCollection colors =
                FolderService.FindLibraryAsset<ColorLibraryCollection>();

            bool updateGlyphs = glyphs != null && glyphs.IsCatalogOutdated;
            bool updateColors = colors != null && colors.IsCatalogOutdated;

            if (!updateGlyphs &&
                !updateColors)
            {
                return;
            }

            List<Object> assets = new();
            List<string> names = new();

            if (updateGlyphs)
            {
                assets.Add(glyphs);
                names.Add(LoreText.Library.GlyphLibraryName);
            }

            if (updateColors)
            {
                assets.Add(colors);
                names.Add(LoreText.Library.ColorLibraryName);
            }

            // 0 = first button, 1 = cancel (also closing the dialog), 2 = alternative.
            int choice = EditorUtility.DisplayDialogComplex(
                LoreText.Library.UpdateDialogTitle,
                string.Format(
                    LoreText.Library.UpdateDialogMessage,
                    LoreText.Common.JoinList(names)),
                LoreText.Library.AppendOnlyNewButton,
                LoreText.Library.SkipUpdatesButton,
                LoreText.Library.UpdateAllButton);

            LoreUndo.Record(
                assets,
                LoreText.Library.UpdateLibraries);

            if (updateGlyphs)
                Apply(glyphs, choice);

            if (updateColors)
                Apply(colors, choice);

            foreach (Object asset in assets)
            {
                EditorUtility.SetDirty(
                    asset);
            }

            LoreRefreshService.Refresh();
        }

        private static void Apply(
            GlyphLibraryCollection glyphs,
            int choice)
        {
            switch (choice)
            {
                case 0:
                    glyphs.AppendMissingDefaults();
                    break;

                case 2:
                    glyphs.GenerateDefaultCollection();
                    break;

                default:
                    glyphs.MarkCatalogCurrent();
                    break;
            }
        }

        private static void Apply(
            ColorLibraryCollection colors,
            int choice)
        {
            switch (choice)
            {
                case 0:
                    colors.AppendMissingDefaults();
                    break;

                case 2:
                    colors.GenerateDefaultCollection();
                    break;

                default:
                    colors.MarkCatalogCurrent();
                    break;
            }
        }
        #endregion
    }
}
