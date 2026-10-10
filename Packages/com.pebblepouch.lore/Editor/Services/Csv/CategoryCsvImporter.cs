using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

using Plan = PebblePouch.Lore.Editor.CategoryCsvImportPlan;

namespace PebblePouch.Lore.Editor
{
    // Category CSV import: file dialog, analysis (CategoryCsvImportAnalyzer), preview (CsvImportPreviewWindow), apply.
    // Apply order: create new Entries and rename (file operations, not undoable), refresh once, then set all values
    // (one undo step for the existing Entries), refresh and save.
    public static class CategoryCsvImporter
    {
        #region Public API
        public static void ImportWithDialog(Category category)
        {
            if (category == null)
                return;

            string path = EditorUtility.OpenFilePanel(
                LoreText.Csv.ImportTitle,
                LorePreferences.LastCsvFolder,
                CsvFormat.FileExtension);

            if (string.IsNullOrEmpty(path))
                return;

            LorePreferences.LastCsvFolder = System.IO.Path.GetDirectoryName(path);

            CategoryCsvImportPlan plan = CategoryCsvImportAnalyzer.Analyze(category, path);

            CsvImportPreviewWindow.Open(plan, Apply);
        }

        public static void Apply(Plan plan)
        {
            if (plan == null ||
                !plan.HasChanges)
            {
                return;
            }

            EntryService entryService = new();
            List<string> failures = new();

            Dictionary<Plan.Row, Entry> created = CreateEntries(plan, entryService, failures);

            RenameEntries(plan, entryService, failures);

            // New Entries and titles are known before values are set (Entry references, description links).
            LoreRefreshService.Refresh();

            // One undo step for the value changes of existing Entries (new Entries are not undoable).
            LoreUndo.Record(
                plan.Rows
                    .Where(row => !row.IsNew && row.Changes.Count > 0)
                    .Select(row => (Object)row.Entry),
                LoreText.Undo.ImportCsv);

            foreach (Plan.Row row in plan.Rows)
            {
                Entry entry = row.IsNew
                    ? created.TryGetValue(row, out Entry newEntry) ? newEntry : null
                    : row.Entry;

                if (entry == null ||
                    row.Changes.Count == 0)
                {
                    continue;
                }

                foreach (Plan.Change change in row.Changes)
                    ApplyChange(entry, change, created);

                LoreChangeTracker.MarkModified(entry);
            }

            LoreRefreshService.Refresh();

            LoreRefreshService.SaveLoreAssets();

            LoreRepaintService.Repaint();

            Log(plan, created.Count, failures);
        }
        #endregion

        #region File Operations
        private static Dictionary<Plan.Row, Entry> CreateEntries(
            Plan plan,
            EntryService entryService,
            List<string> failures)
        {
            Dictionary<Plan.Row, Entry> created = new();
            int sortOrder = entryService.GetNextSortOrder(plan.Category);

            foreach (Plan.Row row in plan.Rows)
            {
                if (!row.IsNew)
                    continue;

                Entry entry = entryService.CreateEntryAsset(plan.Category, row.Title, sortOrder);

                if (entry == null)
                {
                    failures.Add($"row {row.Number}: '{row.Title}' could not be created");
                    continue;
                }

                created[row] = entry;
                sortOrder++;
            }

            return created;
        }

        private static void RenameEntries(
            Plan plan,
            EntryService entryService,
            List<string> failures)
        {
            foreach (Plan.Row row in plan.Rows)
            {
                if (!row.Renames)
                    continue;

                string oldTitle = row.Entry.Title;

                if (!entryService.RenameEntry(row.Entry, row.Title))
                    failures.Add($"row {row.Number}: '{oldTitle}' could not be renamed to '{row.Title}'");
            }
        }
        #endregion

        #region Values
        private static void ApplyChange(
            Entry entry,
            Plan.Change change,
            Dictionary<Plan.Row, Entry> created)
        {
            Plan.Value value = change.Value;

            switch (change.Column.Kind)
            {
                case Plan.ColumnKind.Subtitle:
                    entry.SetSubtitle(value.Text);
                    break;

                // Empty = the Category's glyph.
                case Plan.ColumnKind.Glyph:
                    entry.SetGlyph(value.Text);
                    break;

                case Plan.ColumnKind.Tags:
                    foreach (string tagId in entry.TagIds.ToList())
                        entry.RemoveTag(tagId);

                    foreach (string tagId in value.TagIds)
                        entry.AddTag(tagId);
                    break;

                case Plan.ColumnKind.Description:
                    entry.SetDescription(DescriptionLinkFormat.ToStored(value.Text, value.BrokenLinkIds));
                    break;

                case Plan.ColumnKind.Field:
                    ApplyFieldValue(entry, change.Column.Field, value, created);
                    break;
            }
        }

        private static void ApplyFieldValue(
            Entry entry,
            Field field,
            Plan.Value value,
            Dictionary<Plan.Row, Entry> created)
        {
            if (!entry.TryGetValue(field, out FieldValue fieldValue))
            {
                entry.SyncFields();

                if (!entry.TryGetValue(field, out fieldValue))
                    return;
            }

            switch (field.Type)
            {
                case FieldType.Text:
                case FieldType.Url:
                    fieldValue.SetText(value.Text);
                    break;

                case FieldType.Boolean:
                    fieldValue.SetBoolean(value.Bool);
                    break;

                case FieldType.Integer:
                    fieldValue.SetInteger(value.Int);
                    break;

                case FieldType.Float:
                    fieldValue.SetFloat(value.Float);
                    break;

                case FieldType.Color:
                    fieldValue.SetColor(value.Color);
                    break;

                case FieldType.Vector:
                    fieldValue.SetVector(value.Vector);
                    break;

                case FieldType.Enum:
                    fieldValue.SetEnum(value.EnumOptionId);
                    break;

                // Removed options stay (broken values are never removed silently).
                case FieldType.Enums:
                    fieldValue.SetEnumValues(
                        value.EnumOptionIds.Concat(
                            fieldValue.EnumValues
                                .Where(optionId => field.FindEnumOption(optionId) == null)
                                .ToList()));
                    break;

                case FieldType.Entry:
                    fieldValue.SetEntry(Resolve(value.Entries, created).FirstOrDefault());
                    break;

                case FieldType.Entries:
                    fieldValue.SetEntriesKeepingMissing(Resolve(value.Entries, created));
                    break;

                case FieldType.GameObject:
                    fieldValue.SetGameObject(value.Asset as GameObject);
                    break;

                case FieldType.Sprite:
                    fieldValue.SetSprite(value.Asset as Sprite);
                    break;

                case FieldType.Scene:
                    fieldValue.SetScene(
                        value.Asset,
                        value.Asset != null
                            ? AssetDatabase.GetAssetPath(value.Asset)
                            : string.Empty);
                    break;

                case FieldType.Folder:
                    fieldValue.SetFolder(
                        value.Asset,
                        value.Asset != null
                            ? AssetDatabase.GetAssetPath(value.Asset)
                            : string.Empty);
                    break;

                case FieldType.ScriptableObject:
                case FieldType.Object:
                    fieldValue.SetObject(value.Asset);
                    break;

                case FieldType.Audio:
                    fieldValue.SetAudioClip(value.Asset as AudioClip);
                    break;
            }
        }

        // Existing Entries, or the ones this import created (skipped if their creation failed).
        private static List<Entry> Resolve(
            List<Plan.EntryReference> references,
            Dictionary<Plan.Row, Entry> created)
        {
            List<Entry> entries = new();

            foreach (Plan.EntryReference reference in references)
            {
                Entry entry = reference.Existing != null
                    ? reference.Existing
                    : created.TryGetValue(reference.NewRow, out Entry newEntry)
                        ? newEntry
                        : null;

                if (entry != null)
                    entries.Add(entry);
            }

            return entries;
        }
        #endregion

        #region Log
        private static void Log(
            Plan plan,
            int createdCount,
            List<string> failures)
        {
            string message =
                $"Lore: Imported '{System.IO.Path.GetFileName(plan.Path)}' into '{plan.Category.Title}': " +
                $"{plan.UpdatedCount} updated, {createdCount} created, {plan.RenamedCount} renamed.";

            if (failures.Count == 0)
            {
                Debug.Log(message, plan.Category);

                return;
            }

            Debug.LogWarning(
                message + " Problems: " + string.Join("; ", failures) + ".",
                plan.Category);
        }
        #endregion
    }
}
