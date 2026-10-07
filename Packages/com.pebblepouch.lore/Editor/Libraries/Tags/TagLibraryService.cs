using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class TagLibraryService
    {
        #region Fields
        private const string AssetName = "Tags";
        // Found by type wherever the user keeps it (see FolderService); cached, as lookups are frequent.
        private static TagCollection _cached;
        #endregion

        #region Create
        public TagDefinition CreateTag(string title)
        {
            TagCollection collection = GetCollection();

            if (collection == null)
                return null;

            title = string.IsNullOrWhiteSpace(title)
                ? LoreText.Defaults.NewTag
                : title.Trim();

            string availableTitle =
                GetAvailableTitle(
                    collection,
                    title);

            LoreUndo.Record(collection, LoreText.Undo.CreateTag);

            TagDefinition tag =
                collection.CreateTag(
                    availableTitle,
                    LibraryStyle.DefaultTagColor);

            EditorUtility.SetDirty(collection);

            LoreRefreshService.Refresh();

            return tag;
        }
        #endregion

        #region Read
        public TagCollection GetCollection()
        {
            // Only a saved asset counts: an object from a failed or replaced create would hide the real Library.
            if (_cached != null &&
                AssetDatabase.Contains(_cached))
            {
                return _cached;
            }

            _cached =
                FolderService.GetOrCreateLibraryAsset<TagCollection>(
                    AssetName);

            return _cached;
        }
        #endregion

        #region Update
        public LoreOperationResult ValidateRename(
            TagDefinition tag,
            string title)
        {
            if (tag == null)
            {
                Debug.LogError("No Tag provided.");

                return LoreOperationResult.Failed(LoreText.Validation.TagNotFound);
            }

            LoreOperationResult nameValidation = LoreNameValidator.Validate(title);

            if (!nameValidation.Success)
            {
                return nameValidation;
            }

            TagCollection collection = GetCollection();

            if (collection == null)
            {
                Debug.LogError("Could not load the Tag collection.");

                return LoreOperationResult.Failed(LoreText.Validation.TagNotSaved);
            }

            if (tag.Title == title)
                return LoreOperationResult.Succeeded();

            if (collection.ContainsTitle(title, tag))
            {
                return LoreOperationResult.Failed(string.Format(LoreText.Validation.TagExists, title));
            }

            return LoreOperationResult.Succeeded();
        }

        public bool RenameTag(
            TagDefinition tag,
            string title)
        {
            if (tag == null)
                return false;

            if (!LoreNameValidator.IsValid(title))
            {
                return false;
            }

            TagCollection collection = GetCollection();

            if (collection == null)
                return false;

            if (tag.Title == title)
                return true;

            if (collection.ContainsTitle(title, tag))
            {
                return false;
            }

            LoreUndo.Record(collection, LoreText.Undo.RenameTag);

            tag.SetTitle(title);

            EditorUtility.SetDirty(collection);

            LoreRefreshService.Refresh();

            return true;
        }

        public bool UpdateColor(
            TagDefinition tag,
            Color color)
        {
            if (tag == null)
                return false;

            TagCollection collection = GetCollection();

            if (collection == null)
                return false;

            if (tag.Color == color)
                return true;

            LoreUndo.Record(collection, LoreText.Undo.ChangeColor);

            tag.SetColor(color);

            EditorUtility.SetDirty(collection);

            LoreRefreshService.Refresh();

            return true;
        }
        #endregion

        #region Delete
        public bool DeleteTag(TagDefinition tag)
        {
            if (tag == null)
                return false;

            TagCollection collection = GetCollection();

            if (collection == null)
                return false;

            new TagSyncService().RemoveTagAssignments(tag);

            if (!collection.RemoveTag(tag))
            {
                return false;
            }

            EditorUtility.SetDirty(collection);

            // Confirmed as "cannot be undone" (the Tag was also removed from its Entries).
            LoreUndo.ForgetAll();

            LoreRefreshService.Refresh();

            return true;
        }
        #endregion

        #region Helpers
        private static string GetAvailableTitle(
            TagCollection collection,
            string baseTitle)
        {
            if (!collection.ContainsTitle(baseTitle))
            {
                return baseTitle;
            }

            int suffix = 2;

            while (true)
            {
                string candidate = string.Format(LoreText.Defaults.NumberedName, baseTitle, suffix);

                if (!collection.ContainsTitle(
                        candidate))
                {
                    return candidate;
                }

                suffix++;
            }
        }
        #endregion
    }
}