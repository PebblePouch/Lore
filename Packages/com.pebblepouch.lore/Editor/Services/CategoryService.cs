using System.Linq;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class CategoryService
    {
        #region Create
        public Category CreateCategory(string name)
        {
            if (!LoreNameValidator.IsValid(name))
            {
                Debug.LogError($"'{name}' is not a valid Lore name.");

                return null;
            }

            FolderService.EnsureStructure();

            name = GetAvailableCategoryName(name);

            string entryPath = FolderService.GetCategoryEntryPath(name);

            var category = ScriptableObject.CreateInstance<Category>();

            category.name = name;

            var categories =
                LoreRepository.Instance
                    .Categories;

            int nextSortOrder =
                categories.Count > 0
                    ? categories.Max(category => category.SortOrder) + 1
                    : 0;

            category.SetSortOrder(nextSortOrder);

            LoreChangeTracker.MarkCreated(category);

            AssetDatabase.CreateAsset(category, entryPath);

            FolderService.GetCategoryFolder(name);

            Save();

            return category;
        }
        #endregion

        #region Update
        public bool UpdateColor(Category category, Color color)
        {
            if (category == null)
            {
                Debug.LogError("No category provided.");
                return false;
            }

            if (category.Color == color)
                return true;

            LoreUndo.Record(category, LoreText.Undo.ChangeColor);

            category.SetColor(color);

            Save(category);

            return true;
        }

        public bool UpdateGlyph(Category category, string glyph)
        {
            if (category == null)
            {
                Debug.LogError("No category provided.");

                return false;
            }

            if (category.Glyph == glyph)
                return true;

            LoreUndo.Record(category, LoreText.Undo.ChangeGlyph);

            category.SetGlyph(glyph);

            Save(category);

            return true;
        }

        // Required Fields: Entries of this Category should fill them (Health). Undoable.
        public bool SetFieldRequired(
            Category category,
            Field field,
            bool required)
        {
            if (category == null ||
                field == null ||
                category.IsRequired(field) == required)
            {
                return false;
            }

            LoreUndo.Record(
                category,
                required
                    ? LoreText.Undo.MarkFieldRequired
                    : LoreText.Undo.MarkFieldOptional);

            if (!category.SetRequired(field, required))
                return false;

            Save(category);

            return true;
        }

        public bool UpdateDescription(Category category, string description)
        {
            if (category == null)
            {
                Debug.LogError("No category provided.");
                return false;
            }

            description ??= string.Empty;

            if (category.Description == description)
                return true;

            LoreUndo.Record(category, LoreText.Undo.EditDescription);

            category.SetDescription(description);

            Save(category);

            return true;
        }

        public LoreOperationResult ValidateRename(
            Category category,
            string newName)
        {
            if (category == null)
            {
                Debug.LogError("No category provided.");

                return LoreOperationResult.Failed(LoreText.Validation.CategoryNotFound);
            }

            LoreOperationResult nameValidation = LoreNameValidator.Validate(newName);

            if (!nameValidation.Success)
            {
                return nameValidation;
            }

            if (category.name == newName)
                return LoreOperationResult.Succeeded();

            if (!AssetNameAvailabilityValidator
                .IsAvailable<Category>(
                    FolderService.Categories,
                    newName,
                    category))
            {
                return LoreOperationResult.Failed(string.Format(LoreText.Validation.CategoryExists, newName));
            }

            string targetFolderPath = FolderService.GetCategoryFolderPath(newName);

            if (AssetDatabase.IsValidFolder(targetFolderPath))
            {
                return LoreOperationResult.Failed(string.Format(LoreText.Validation.CategoryFolderExists, newName));
            }

            return LoreOperationResult.Succeeded();
        }

        public bool RenameCategory(
            Category category,
            string newName)
        {
            LoreOperationResult validation = ValidateRename(category, newName);

            if (!validation.Success)
                return false;

            if (category.name == newName)
                return true;

            string targetFolderPath = FolderService.GetCategoryFolderPath(newName);

            string oldName = category.name;
            string oldEntryPath = AssetDatabase.GetAssetPath(category);
            string oldFolderPath = FolderService.GetCategoryFolderPath(category);

            string entryError = AssetDatabase.RenameAsset(oldEntryPath, newName);

            if (!string.IsNullOrEmpty(entryError))
            {
                Debug.LogError(entryError);

                return false;
            }

            if (AssetDatabase.IsValidFolder(oldFolderPath))
            {
                string folderError =
                    AssetDatabase.MoveAsset(
                        oldFolderPath,
                        targetFolderPath);

                if (!string.IsNullOrEmpty(folderError))
                {
                    string rollbackError =
                        AssetDatabase.RenameAsset(
                            AssetDatabase.GetAssetPath(category),
                            oldName);

                    if (!string.IsNullOrEmpty(rollbackError))
                    {
                        Debug.LogError(
                            $"Failed to move Category folder:\n{folderError}\n\n" +
                            $"The Category asset rename could also not be rolled back:\n{rollbackError}");
                    }
                    else
                    {
                        Debug.LogError($"Failed to move Category folder:\n{folderError}");
                    }

                    return false;
                }
            }

            // The asset name is part of its recorded state: older steps must not restore the old name.
            LoreUndo.Forget(category);

            Save(category);

            return true;
        }

        public bool MoveCategory(
            Category category,
            int targetIndex)
        {
            if (category == null)
            {
                Debug.LogError("No category provided.");
                return false;
            }

            var categories = LoreRepository.Instance
                .Categories
                .OrderBy(c => c.SortOrder)
                .ToList();

            int currentIndex = categories.IndexOf(category);

            if (currentIndex < 0)
                return false;

            bool samePosition =
                targetIndex == currentIndex ||
                targetIndex == currentIndex + 1;

            if (samePosition)
                return true;

            categories.Remove(category);

            // When targetIndex > currentIndex, the index is overstated by 1 as the targetIndex was removed (remove then insert)
            if (targetIndex > currentIndex)
            {
                targetIndex--;
            }

            targetIndex = Mathf.Clamp(
                targetIndex,
                0,
                categories.Count);

            categories.Insert(
                targetIndex,
                category);

            LoreUndo.Record(categories, LoreText.Undo.ReorderCategories);

            for (int i = 0; i < categories.Count; i++)
            {
                categories[i].SetSortOrder(i);
                EditorUtility.SetDirty(categories[i]);
            }

            Save();

            return true;
        }
        #endregion

        #region Delete
        public bool DeleteCategory(Category category)
        {
            if (category == null)
            {
                Debug.LogError("No category provided.");

                return false;
            }

            bool hasEntries =
                LoreRepository.Instance
                    .Entries
                    .Any(
                        entry =>
                            entry.Category ==
                            category);

            if (hasEntries)
                return false;

            string assetPath = AssetDatabase.GetAssetPath(category);
            string folderPath = FolderService.GetCategoryFolderPath(category);

            if (AssetDatabase.IsValidFolder(folderPath))
            {
                bool folderDeleted = AssetDatabase.DeleteAsset(folderPath);

                if (!folderDeleted)
                {
                    Debug.LogError($"Failed to delete folder:\n{folderPath}");

                    return false;
                }
            }

            bool assetDeleted = AssetDatabase.DeleteAsset(assetPath);

            if (!assetDeleted)
            {
                Debug.LogError($"Failed to delete asset:\n{assetPath}");

                return false;
            }

            SelectionService.Clear();

            // Confirmed as "cannot be undone".
            LoreUndo.ForgetAll();

            Save();

            return true;
        }
        #endregion

        #region Helpers
        private static string GetAvailableCategoryName(
           string baseName)
        {
            if (AssetNameAvailabilityValidator
                .IsAvailable<Category>(
                    FolderService.Categories,
                    baseName))
            {
                return baseName;
            }

            int suffix = 2;

            while (true)
            {
                string candidate =
                    string.Format(LoreText.Defaults.NumberedName, baseName, suffix);

                if (AssetNameAvailabilityValidator
                    .IsAvailable<Category>(
                        FolderService.Categories,
                        candidate))
                {
                    return candidate;
                }

                suffix++;
            }
        }

        // category = the changed Category (not passed for reordering, which only sets sort orders).
        private void Save(Category category = null)
        {
            if (category != null)
                LoreChangeTracker.MarkModified(category);

            LoreRefreshService.Refresh();
            LoreRepaintService.Repaint();
        }
        #endregion
    }
}