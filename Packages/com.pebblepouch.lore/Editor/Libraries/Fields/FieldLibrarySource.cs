using System.Collections.Generic;
using System.Linq;

using UnityEditor;

namespace PebblePouch.Lore.Editor
{
    public sealed class FieldLibrarySource : LibrarySource<FieldLibraryItem>
    {
        #region Public API
        public override IReadOnlyList<FieldLibraryItem> GetItems()
        {
            Category category = SelectionService.CurrentCategory;

            string[] assetGuids =
                AssetDatabase.FindAssets("t:Field");

            return assetGuids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(path => AssetDatabase.LoadAssetAtPath<Field>(path))
                .Where(field => field != null)
                .OrderBy(field => field.name)
                .Select(field =>
                    new FieldLibraryItem(
                        field,
                        category != null &&
                        category.Fields.Contains(field)))
                .ToList();
        }
        #endregion
    }
}