using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class EntryService
    {
        #region Create
        public Entry CreateEntry(Category category, string name)
        {
            Entry entry =
                CreateEntryAsset(
                    category,
                    name,
                    GetNextSortOrder(category));

            if (entry != null)
                LoreRefreshService.Refresh();

            return entry;
        }

        // Sort order after the Category's last Entry.
        public int GetNextSortOrder(Category category)
        {
            var entries =
                LoreRepository.Instance
                    .Entries
                    .Where(entry =>
                        entry.Category == category);

            return entries.Any()
                ? entries.Max(entry => entry.SortOrder) + 1
                : 0;
        }

        // Creates the asset without refreshing Lore, for batches (e.g. CSV import): the caller refreshes once.
        public Entry CreateEntryAsset(
            Category category,
            string name,
            int sortOrder)
        {
            if (!LoreNameValidator.IsValid(name))
            {
                Debug.LogError($"'{name}' is not a valid Lore name.");

                return null;
            }

            if (category == null)
            {
                Debug.LogError("Cannot create entry without category.");

                return null;
            }

            string folder = FolderService.GetCategoryFolder(category.name);

            name = GetAvailableEntryName(folder, name);

            string entryPath = $"{folder}/{name}.asset";

            var entry = ScriptableObject.CreateInstance<Entry>();
            entry.name = name;
            entry.Initialize(category);

            entry.SetSortOrder(sortOrder);

            LoreChangeTracker.MarkCreated(entry);

            AssetDatabase.CreateAsset(entry, entryPath);

            return entry;
        }

        public Entry DuplicateEntry(Entry entry)
        {
            if (entry == null)
                return null;

            string sourcePath = AssetDatabase.GetAssetPath(entry);

            if (string.IsNullOrEmpty(sourcePath))
                return null;

            string directory =
                System.IO.Path.GetDirectoryName(
                    sourcePath);

            string newName =
                GetAvailableEntryName(
                    directory,
                    string.Format(LoreText.Defaults.CopyName, entry.name));

            string targetPath = $"{directory}/{newName}.asset";

            Entry duplicate =
                UnityEngine.Object.Instantiate(
                    entry);

            duplicate.name = newName;

            duplicate.RegenerateId();

            // A duplicate is a new item: it does not keep the original's timestamps.
            LoreChangeTracker.MarkCreated(duplicate);

            AssetDatabase.CreateAsset(
                duplicate,
                targetPath);

            var entries =
                LoreRepository.Instance
                    .Entries
                    .Where(e =>
                        e.Category ==
                        entry.Category)
                    .OrderBy(e =>
                        e.SortOrder)
                    .ToList();

            int originalIndex = entries.IndexOf(entry);

            int insertIndex =
                Mathf.Clamp(
                    originalIndex + 1,
                    0,
                    entries.Count);

            entries.Insert(insertIndex, duplicate);

            for (int i = 0;
                 i < entries.Count;
                 i++)
            {
                entries[i].SetSortOrder(i);

                EditorUtility.SetDirty(entries[i]);
            }

            LoreRefreshService.Refresh();

            return duplicate;
        }
        #endregion

        #region Update
        public bool UpdateGlyph(
            Entry entry,
            string glyph)
        {
            if (entry == null)
            {
                Debug.LogError("No entry provided.");

                return false;
            }

            glyph ??= string.Empty;

            if (entry.Glyph == glyph &&
                entry.HasGlyphOverride)
            {
                return true;
            }

            LoreUndo.Record(entry, LoreText.Undo.ChangeGlyph);

            entry.SetGlyph(glyph);

            LoreChangeTracker.MarkModified(entry);

            LoreRefreshService.Refresh();

            return true;
        }

        public bool UpdateSubtitle(Entry entry, string subtitle)
        {
            if (entry == null)
            {
                Debug.LogError("No entry provided.");
                return false;
            }

            subtitle ??= string.Empty;

            if (entry.Subtitle == subtitle)
                return true;

            LoreUndo.Record(entry, LoreText.Undo.EditSubtitle);

            entry.SetSubtitle(subtitle);

            LoreChangeTracker.MarkModified(entry);

            LoreRefreshService.Refresh();

            return true;
        }

        public bool UpdateDescription(Entry entry, string description)
        {
            if (entry == null)
            {
                Debug.LogError("No entry provided.");
                return false;
            }

            description ??= string.Empty;

            if (entry.Description == description)
                return true;

            LoreUndo.Record(entry, LoreText.Undo.EditDescription);

            entry.SetDescription(description);

            LoreChangeTracker.MarkModified(entry);

            LoreRefreshService.Refresh();

            return true;
        }

        public bool ClearGlyphOverride(Entry entry)
        {
            if (entry == null)
            {
                Debug.LogError("No entry provided.");
                return false;
            }

            if (!entry.HasGlyphOverride)
                return true;

            LoreUndo.Record(entry, LoreText.Undo.UseCategoryGlyph);

            entry.SetGlyph(string.Empty);

            LoreChangeTracker.MarkModified(entry);

            LoreRefreshService.Refresh();

            return true;
        }

        public LoreOperationResult ValidateRename(
            Entry entry,
            string newName)
        {
            if (entry == null)
            {
                Debug.LogError("No entry provided.");

                return LoreOperationResult.Failed(LoreText.Validation.EntryNotFound);
            }

            LoreOperationResult nameValidation = LoreNameValidator.Validate(newName);

            if (!nameValidation.Success)
            {
                return nameValidation;
            }

            if (entry.name == newName)
                return LoreOperationResult.Succeeded();

            string assetPath = AssetDatabase.GetAssetPath(entry);

            string folder = System.IO.Path.GetDirectoryName(assetPath);

            if (!AssetNameAvailabilityValidator
                .IsAvailable<Entry>(
                    folder,
                    newName,
                    entry))
            {
                return LoreOperationResult.Failed(string.Format(LoreText.Validation.EntryExists, newName));
            }

            return LoreOperationResult.Succeeded();
        }

        public bool RenameEntry(
            Entry entry,
            string newName)
        {
            LoreOperationResult validation = ValidateRename(entry, newName);

            if (!validation.Success)
                return false;

            if (entry.name == newName)
                return true;

            string assetPath = AssetDatabase.GetAssetPath(entry);

            string error =
                AssetDatabase.RenameAsset(
                    assetPath,
                    newName);

            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError(error);

                return false;
            }

            // The asset name is part of its recorded state: older steps must not restore the old name.
            LoreUndo.Forget(entry);

            LoreChangeTracker.MarkModified(entry);

            LoreRefreshService.Refresh();

            return true;
        }

        // Reordering: sort orders only, not a change (no LoreChangeTracker).
        public void MoveEntry(
            Entry entry,
            int targetIndex)
        {
            if (entry == null)
                return;

            var entries = LoreRepository.Instance
                .Entries
                .Where(e => e.Category == entry.Category)
                .OrderBy(e => e.SortOrder)
                .ToList();

            int currentIndex = entries.IndexOf(entry);

            bool samePosition =
                targetIndex == currentIndex ||
                targetIndex == currentIndex + 1;

            if (samePosition)
                return;

            entries.Remove(entry);

            // When targetIndex > currentIndex, the index is overstated by 1 as the targetIndex was removed (remove then insert)
            if (targetIndex > currentIndex)
            {
                targetIndex--;
            }

            targetIndex = Mathf.Clamp(
                targetIndex,
                0,
                entries.Count);

            entries.Insert(
                targetIndex,
                entry);

            LoreUndo.Record(entries, LoreText.Undo.ReorderEntries);

            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].SetSortOrder(i);

                EditorUtility.SetDirty(entries[i]);
            }

            LoreRefreshService.Refresh();
        }

        public bool AddTag(Entry entry, TagDefinition tag)
        {
            if (entry == null)
            {
                Debug.LogError("No entry provided.");
                return false;
            }

            if (tag == null)
            {
                Debug.LogError("No tag provided.");
                return false;
            }

            LoreUndo.Record(entry, LoreText.Undo.AddTag);

            if (!entry.AddTag(tag.Id))
                return false;

            LoreChangeTracker.MarkModified(entry);

            LoreRefreshService.Refresh();

            return true;
        }

        public bool RemoveTag(Entry entry, TagDefinition tag)
        {
            if (entry == null)
            {
                Debug.LogError("No entry provided.");
                return false;
            }

            if (tag == null)
            {
                Debug.LogError("No tag provided.");
                return false;
            }

            LoreUndo.Record(entry, LoreText.Undo.RemoveTag);

            if (!entry.RemoveTag(tag.Id))
                return false;

            LoreChangeTracker.MarkModified(entry);

            LoreRefreshService.Refresh();

            return true;
        }

        // Bulk editing (Tabular Editing selection): one undo step for the Entries that change.
        public bool AddTag(IEnumerable<Entry> entries, TagDefinition tag)
        {
            if (tag == null)
                return false;

            return ChangeTags(
                entries.Where(entry => entry != null && !entry.HasTag(tag.Id)),
                LoreText.Undo.AddTag,
                entry => entry.AddTag(tag.Id));
        }

        public bool RemoveTag(IEnumerable<Entry> entries, TagDefinition tag)
        {
            if (tag == null)
                return false;

            return ChangeTags(
                entries.Where(entry => entry != null && entry.HasTag(tag.Id)),
                LoreText.Undo.RemoveTag,
                entry => entry.RemoveTag(tag.Id));
        }

        public bool ClearTags(IEnumerable<Entry> entries)
        {
            return ChangeTags(
                entries.Where(entry => entry != null && entry.TagIds.Count > 0),
                LoreText.Undo.ClearTags,
                entry =>
                {
                    foreach (string tagId in entry.TagIds.ToList())
                        entry.RemoveTag(tagId);
                });
        }

        private static bool ChangeTags(
            IEnumerable<Entry> entries,
            string undoName,
            Action<Entry> change)
        {
            List<Entry> targets = entries.ToList();

            if (targets.Count == 0)
                return false;

            LoreUndo.Record(targets, undoName);

            foreach (Entry entry in targets)
            {
                change(entry);

                LoreChangeTracker.MarkModified(entry);
            }

            LoreRefreshService.Refresh();

            return true;
        }
        #endregion

        #region Delete
        public bool DeleteEntry(Entry entry)
        {
            if (entry == null)
                return false;

            string entryPath = AssetDatabase.GetAssetPath(entry);

            if (string.IsNullOrEmpty(entryPath))
                return false;

            if (!AssetDatabase.DeleteAsset(entryPath))
            {
                Debug.LogError($"Failed to delete '{entry.name}'.");

                return false;
            }

            // Confirmed as "cannot be undone" (references to it stay as broken references).
            LoreUndo.ForgetAll();

            LoreRefreshService.Refresh();

            return true;
        }

        // Bulk editing (Tabular Editing selection): one asset operation and one refresh. Returns the deleted count.
        public int DeleteEntries(IEnumerable<Entry> entries)
        {
            string[] paths =
                entries
                    .Where(entry => entry != null)
                    .Select(AssetDatabase.GetAssetPath)
                    .Where(path => !string.IsNullOrEmpty(path))
                    .ToArray();

            if (paths.Length == 0)
                return 0;

            var failed = new List<string>();

            AssetDatabase.DeleteAssets(paths, failed);

            foreach (string path in failed)
                Debug.LogError($"Failed to delete '{path}'.");

            int deleted = paths.Length - failed.Count;

            if (deleted > 0)
            {
                // Confirmed as "cannot be undone" (as DeleteEntry).
                LoreUndo.ForgetAll();

                LoreRefreshService.Refresh();
            }

            return deleted;
        }
        #endregion

        #region Helpers
        private static string GetAvailableEntryName(
            string folder,
            string baseName)
        {
            if (AssetNameAvailabilityValidator
                .IsAvailable<Entry>(
                    folder,
                    baseName))
            {
                return baseName;
            }

            int suffix = 2;

            while (true)
            {
                string candidate = string.Format(LoreText.Defaults.NumberedName, baseName, suffix);

                if (AssetNameAvailabilityValidator
                    .IsAvailable<Entry>(
                        folder,
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