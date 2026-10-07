using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class ColorLibraryService
    {
        #region Fields
        private const string AssetName = "Colors";
        // Found by type wherever the user keeps it (see FolderService); cached, as lookups are frequent.
        private static ColorLibraryCollection _cached;
        #endregion

        #region Public API
        public ColorLibraryCollection GetCollection()
        {
            // Only a saved asset counts: an object from a failed or replaced create would hide the real Library.
            if (_cached != null &&
                AssetDatabase.Contains(_cached))
            {
                return _cached;
            }

            _cached =
                FolderService.GetOrCreateLibraryAsset<ColorLibraryCollection>(
                    AssetName,
                    c => c.GenerateDefaultCollection());

            return _cached;
        }

        public void GenerateDefaultCollection()
        {
            ColorLibraryCollection collection =
                GetCollection();

            if (collection == null)
                return;

            LoreUndo.Record(
                collection,
                LoreText.Library.GenerateDefaultColorLibrary);

            collection.GenerateDefaultCollection();

            EditorUtility.SetDirty(
                collection);

            LoreRefreshService.Refresh();
        }
        #endregion
    }
}