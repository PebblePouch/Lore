using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class GlyphLibraryService
    {
        #region Fields
        private const string AssetName = "Glyphs";
        // Found by type wherever the user keeps it (see FolderService); cached, as lookups are frequent.
        private static GlyphLibraryCollection _cached;
        #endregion

        #region Public API
        public GlyphLibraryCollection GetCollection()
        {
            // Only a saved asset counts: an object from a failed or replaced create would hide the real Library.
            if (_cached != null &&
                AssetDatabase.Contains(_cached))
            {
                return _cached;
            }

            _cached =
                FolderService.GetOrCreateLibraryAsset<GlyphLibraryCollection>(
                    AssetName,
                    c => c.GenerateDefaultCollection());

            return _cached;
        }

        public void GenerateDefaultCollection()
        {
            GlyphLibraryCollection collection =
                GetCollection();

            if (collection == null)
                return;

            LoreUndo.Record(
                collection,
                LoreText.Library.GenerateDefaultGlyphLibrary);

            collection.GenerateDefaultCollection();

            EditorUtility.SetDirty(
                collection);

            LoreRefreshService.Refresh();
        }
        #endregion
    }
}