using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

namespace PebblePouch.Lore.Editor
{
    public sealed class FieldService
    {
        #region Create
        public Field CreateField(string name, FieldType type, string glyph)
        {
            if (!LoreNameValidator.IsValid(name))
            {
                Debug.LogError($"'{name}' is not a valid Lore name.");
                return null;
            }

            FolderService.EnsureStructure();

            if (!AssetNameAvailabilityValidator.IsAvailable<Field>(
                    FolderService.Fields,
                    name))
            {
                Debug.LogError($"Field '{name}' already exists.");
                return null;
            }

            string path = FolderService.GetFieldPath(name);

            var field = ScriptableObject.CreateInstance<Field>();
            field.name = name;
            field.Initialize(type, glyph);

            LoreChangeTracker.MarkCreated(field);

            AssetDatabase.CreateAsset(field, path);

            return field;
        }

        public Field CreateField()
        {
            string baseName = LoreText.Defaults.NewField;

            string fieldName = baseName;

            int counter = 1;

            while (!AssetNameAvailabilityValidator
                .IsAvailable<Field>(
                    FolderService.Fields,
                    fieldName))
            {
                counter++;

                fieldName = string.Format(LoreText.Defaults.NumberedName, baseName, counter);
            }

            return CreateField(
                fieldName,
                FieldType.Text,
                "?");
        }

        public Field CreateField(Category category)
        {
            if (category == null)
            {
                Debug.LogError(
                    "Cannot create field without category.");

                return null;
            }

            Field field = CreateField();

            if (field == null)
                return null;

            RecordCategoryFields(category, LoreText.Undo.AddField);

            category.AddField(field);

            LoreChangeTracker.MarkModified(category);

            new CategoryFieldSyncService()
                .SyncCategory(
                    category);

            LoreRefreshService.Refresh();

            return field;
        }

        public Field CreateField(Category category, string name, FieldType type, string glyph)
        {
            if (category == null)
            {
                Debug.LogError("Cannot create field without category.");
                return null;
            }

            Field field = CreateField(name, type, glyph);

            if (field == null)
                return null;

            RecordCategoryFields(category, LoreText.Undo.AddField);

            category.AddField(field);

            LoreChangeTracker.MarkModified(category);

            new CategoryFieldSyncService().SyncCategory(category);

            LoreRefreshService.Refresh();

            return field;
        }

        public Field DuplicateField(Field source)
        {
            if (source == null)
            {
                Debug.LogError("No field provided.");
                return null;
            }

            string baseName = string.Format(LoreText.Defaults.CopyName, source.name);
            string fieldName = baseName;

            int counter = 2;

            while (!AssetNameAvailabilityValidator.IsAvailable<Field>(
                       FolderService.Fields,
                       fieldName))
            {
                fieldName = string.Format(LoreText.Defaults.NumberedName, baseName, counter);
                counter++;
            }

            Field duplicate = CreateField(
                fieldName,
                source.Type,
                source.Glyph);

            if (duplicate == null)
                return null;

            if (!string.IsNullOrEmpty(source.ObjectTypeName))
            {
                duplicate.SetObjectType(
                    source.ObjectTypeName,
                    source.ObjectScriptGuid);

                EditorUtility.SetDirty(duplicate);
            }

            if (source.EnumOptions.Count > 0)
            {
                duplicate.CopyEnumOptionsFrom(source);

                EditorUtility.SetDirty(duplicate);
            }

            if (source.AllowedCategories.Count > 0)
            {
                duplicate.SetAllowedCategories(source.AllowedCategories);

                EditorUtility.SetDirty(duplicate);
            }

            return duplicate;
        }
        #endregion

        #region Update
        public bool RenameField(
            Field field,
            string newName)
        {
            LoreOperationResult validation = ValidateUpdate(field, newName);

            if (!validation.Success)
                return false;

            if (field.name == newName)
                return true;

            string path = AssetDatabase.GetAssetPath(field);

            string error =
                AssetDatabase.RenameAsset(
                    path,
                    newName);

            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError(error);

                return false;
            }

            // The asset name is part of its recorded state: older steps must not restore the old name.
            LoreUndo.Forget(field);

            EditorApplication.delayCall += LoreRefreshService.Refresh;

            return true;
        }

        public bool UpdateField(
            Field field,
            string newName,
            string glyph,
            FieldType type,
            string objectTypeName = null,
            string objectScriptGuid = null,
            IReadOnlyList<EnumOptionDraft> enumOptions = null,
            IReadOnlyList<Category> allowedCategories = null)
        {
            LoreOperationResult validation = ValidateUpdate(field, newName);

            if (!validation.Success)
                return false;

            if (field.name != newName)
            {
                if (!RenameField(field, newName))
                {
                    return false;
                }
            }

            // Enum options: only for Enum Fields, and only when the edited list differs.
            bool optionsChanged =
                type == FieldType.Enum &&
                enumOptions != null &&
                !HasSameEnumOptions(field, enumOptions);

            // Allowed Categories: only Entry / Entries Fields keep them (other types clear them).
            IReadOnlyList<Category> targetCategories =
                FieldValueUtility.IsEntryReference(type)
                    ? allowedCategories ?? field.AllowedCategories
                    : Array.Empty<Category>();

            bool categoriesChanged =
                !HasSameCategories(
                    field.AllowedCategories,
                    targetCategories);

            bool clearsSetValues =
                RecordFieldUndo(
                    field,
                    glyph,
                    type,
                    type == FieldType.ScriptableObject
                        ? objectTypeName ?? string.Empty
                        : string.Empty,
                    optionsChanged
                        ? enumOptions
                        : null,
                    categoriesChanged);

            bool changed = false;

            if (field.Glyph != glyph)
            {
                field.SetGlyph(glyph);

                changed = true;
            }

            // Only ScriptableObject Fields keep a ScriptableObject type.
            if (type != FieldType.ScriptableObject)
            {
                objectTypeName = string.Empty;
                objectScriptGuid = string.Empty;
            }

            objectTypeName ??= string.Empty;

            // A changed ScriptableObject type clears the values like a type change.
            bool typeChanged =
                field.Type != type ||
                field.ObjectTypeName != objectTypeName;

            if (typeChanged)
            {
                field.SetType(type);

                field.SetObjectType(
                    objectTypeName,
                    objectScriptGuid);

                new FieldValueSyncService().ClearValues(field);

                changed = true;
            }

            if (optionsChanged)
            {
                ApplyEnumOptions(
                    field,
                    enumOptions);

                changed = true;
            }

            if (categoriesChanged)
            {
                field.SetAllowedCategories(targetCategories);

                changed = true;
            }

            if (clearsSetValues)
                LoreUndo.ForgetAll();

            if (changed)
            {
                LoreChangeTracker.MarkModified(field);
            }

            LoreRefreshService.Refresh();

            return true;
        }

        public LoreOperationResult ValidateUpdate(
            Field field,
            string newName)
        {
            if (field == null)
            {
                Debug.LogError("No field provided.");

                return LoreOperationResult.Failed(LoreText.Validation.FieldNotFound);
            }

            LoreOperationResult nameValidation = LoreNameValidator.Validate(newName);

            if (!nameValidation.Success)
            {
                return nameValidation;
            }

            if (field.name != newName &&
                !AssetNameAvailabilityValidator
                    .IsAvailable<Field>(
                        FolderService.Fields,
                        newName,
                        field))
            {
                return LoreOperationResult.Failed(string.Format(LoreText.Validation.FieldExists, newName));
            }

            return LoreOperationResult.Succeeded();
        }

        public bool RemoveField(Category category, Field field)
        {
            if (category == null)
            {
                Debug.LogError("No category provided.");

                return false;
            }

            if (field == null)
            {
                Debug.LogError("No field provided.");

                return false;
            }

            if (!category.Fields.Contains(
                    field))
            {
                return false;
            }

            // Without set values the removal is undoable; with set values the user confirmed "cannot be undone".
            bool clearsSetValues =
                new CategoryFieldUsageAnalyzer(LoreRepository.Instance)
                    .GetEntriesWithValues(
                        category,
                        field)
                    .Count > 0;

            if (!clearsSetValues)
                RecordCategoryFields(category, LoreText.Undo.RemoveField);

            category.RemoveField(field);

            LoreChangeTracker.MarkModified(category);

            new CategoryFieldSyncService().SyncCategory(category);

            if (clearsSetValues)
                LoreUndo.ForgetAll();

            LoreRefreshService.Refresh();

            return true;
        }

        public bool MoveField(Category category, Field field, int newIndex)
        {
            if (category == null)
                return false;

            if (field == null)
                return false;

            // The Entries' field values are reordered with the Category (SyncCategory).
            LoreUndo.Record(
                new Object[] { category }
                    .Concat(GetEntries(category)),
                LoreText.Undo.ReorderFields);

            if (!category.MoveField(field, newIndex))
                return false;

            // Reordering: not a change (no LoreChangeTracker).
            EditorUtility.SetDirty(category);

            new CategoryFieldSyncService().SyncCategory(category);

            LoreRefreshService.Refresh();

            return true;
        }

        public bool AddExistingField(Category category, Field field)
        {
            if (category == null)
            {
                Debug.LogError("No category provided.");
                return false;
            }

            if (field == null)
            {
                Debug.LogError("No field provided.");
                return false;
            }

            if (category.Fields.Contains(field))
            {
                return false;
            }

            RecordCategoryFields(category, LoreText.Undo.AddField);

            category.AddField(field);

            LoreChangeTracker.MarkModified(category);

            new CategoryFieldSyncService().SyncCategory(category);

            LoreRefreshService.Refresh();

            return true;
        }
        #endregion

        #region Delete
        public bool DeleteField(
            Field field)
        {
            if (field == null)
            {
                Debug.LogError("No field provided.");

                return false;
            }

            string path = AssetDatabase.GetAssetPath(field);

            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError($"Could not find asset path for '{field.name}'.");

                return false;
            }

            var categories =
                LoreRepository.Instance
                    .Categories
                    .Where(
                        category =>
                            category.Fields.Contains(
                                field))
                    .ToList();

            var sync = new CategoryFieldSyncService();

            foreach (Category category in categories)
            {
                category.RemoveField(field);

                LoreChangeTracker.MarkModified(category);

                sync.SyncCategory(category);
            }

            if (!AssetDatabase.DeleteAsset(path))
            {
                Debug.LogError($"Failed to delete field '{field.name}'.");

                return false;
            }

            LoreUndo.ForgetAll();

            LoreRefreshService.Refresh();

            return true;
        }
        #endregion

        #region Helpers
        // Undo step for glyph and type changes (a type change also clears the values on the Entries). Not when the
        // type change clears set values: the user confirmed that as "cannot be undone" (returns true; the caller then
        // ends the undo history, see LoreUndo.ForgetAll).
        private bool RecordFieldUndo(
            Field field,
            string glyph,
            FieldType type,
            string objectTypeName,
            IReadOnlyList<EnumOptionDraft> changedOptions,
            bool categoriesChanged)
        {
            bool typeChanged =
                field.Type != type ||
                field.ObjectTypeName != objectTypeName;

            if (!typeChanged)
            {
                // Removing options that are in use clears values (confirmed): no undo, like a type change.
                if (changedOptions != null)
                {
                    if (GetEnumOptionRemoval(field, changedOptions).AffectedEntryCount > 0)
                        return true;

                    LoreUndo.Record(field, LoreText.Undo.EditEnumOptions);

                    return false;
                }

                // Glyph and allowed Categories never clear values: one undoable step.
                if (categoriesChanged)
                    LoreUndo.Record(field, LoreText.Undo.EditAllowedCategories);
                else if (field.Glyph != glyph)
                    LoreUndo.Record(field, LoreText.Undo.ChangeGlyph);

                return false;
            }

            bool clearsSetValues =
                new FieldTypeChangeAnalyzer()
                    .Analyze(
                        field,
                        type,
                        objectTypeName)
                    .HasDataLoss;

            if (clearsSetValues)
                return true;

            LoreUndo.Record(
                new Object[] { field }
                    .Concat(GetEntriesUsing(field)),
                LoreText.Undo.ChangeFieldType);

            return false;
        }

        // Options of an Enum Field that the edited list removes: their labels and the Entries that use them (those
        // values get cleared, so the card asks for confirmation first).
        public (IReadOnlyList<string> RemovedLabels, int AffectedEntryCount) GetEnumOptionRemoval(
            Field field,
            IReadOnlyList<EnumOptionDraft> options)
        {
            HashSet<string> removedIds =
                GetRemovedEnumOptionIds(
                    field,
                    options);

            if (removedIds.Count == 0)
                return (Array.Empty<string>(), 0);

            List<string> removedLabels =
                field.EnumOptions
                    .Where(option => option != null && removedIds.Contains(option.Id))
                    .Select(option => option.Label)
                    .ToList();

            int affectedEntryCount =
                LoreRepository.Instance.Entries
                    .Count(
                        entry =>
                            entry.Fields.Any(
                                fieldValue =>
                                    fieldValue.Field == field &&
                                    removedIds.Contains(fieldValue.EnumValue)));

            return (removedLabels, affectedEntryCount);
        }

        private static bool HasSameCategories(
            IReadOnlyList<Category> current,
            IReadOnlyList<Category> target)
        {
            List<Category> currentCategories = current.Where(category => category != null).ToList();
            List<Category> targetCategories = target.Where(category => category != null).ToList();

            return currentCategories.Count == targetCategories.Count &&
                   currentCategories.All(targetCategories.Contains);
        }

        private static bool HasSameEnumOptions(
            Field field,
            IReadOnlyList<EnumOptionDraft> options)
        {
            if (field.EnumOptions.Count != options.Count)
                return false;

            for (int i = 0; i < options.Count; i++)
            {
                EnumOption option = field.EnumOptions[i];

                if (option == null ||
                    option.Id != options[i].Id ||
                    option.Label != options[i].Label.Trim())
                {
                    return false;
                }
            }

            return true;
        }

        private static HashSet<string> GetRemovedEnumOptionIds(
            Field field,
            IReadOnlyList<EnumOptionDraft> options)
        {
            var keptIds =
                new HashSet<string>(
                    options
                        .Where(option => !string.IsNullOrEmpty(option.Id))
                        .Select(option => option.Id));

            return new HashSet<string>(
                field.EnumOptions
                    .Where(option => option != null && !keptIds.Contains(option.Id))
                    .Select(option => option.Id));
        }

        // Applies the edited options; values of removed options are cleared on all Entries.
        private static void ApplyEnumOptions(
            Field field,
            IReadOnlyList<EnumOptionDraft> options)
        {
            HashSet<string> removedIds =
                GetRemovedEnumOptionIds(
                    field,
                    options);

            field.ApplyEnumOptions(
                options
                    .Select(option => (option.Id, option.Label.Trim()))
                    .ToList());

            if (removedIds.Count == 0)
                return;

            foreach (Entry entry in LoreRepository.Instance.Entries)
            {
                bool cleared = false;

                foreach (FieldValue fieldValue in entry.Fields)
                {
                    if (fieldValue.Field == field &&
                        removedIds.Contains(fieldValue.EnumValue))
                    {
                        fieldValue.SetEnum(string.Empty);

                        cleared = true;
                    }
                }

                if (cleared)
                    LoreChangeTracker.MarkModified(entry);
            }
        }

        // Adding/removing a Field changes the Category and syncs its Entries' field values: one undo step.
        private static void RecordCategoryFields(
            Category category,
            string action)
        {
            LoreUndo.Record(
                new Object[] { category }
                    .Concat(GetEntries(category)),
                action);
        }

        private static IEnumerable<Object> GetEntries(Category category)
        {
            return LoreRepository.Instance
                .Entries
                .Where(entry => entry.Category == category);
        }

        private static IEnumerable<Object> GetEntriesUsing(Field field)
        {
            return LoreRepository.Instance
                .Entries
                .Where(
                    entry =>
                        entry.Fields.Any(
                            fieldValue => fieldValue.Field == field));
        }
        #endregion
    }
}