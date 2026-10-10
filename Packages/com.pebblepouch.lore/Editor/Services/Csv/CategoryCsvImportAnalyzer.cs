using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using UnityEditor;
using UnityEngine;

using Plan = PebblePouch.Lore.Editor.CategoryCsvImportPlan;

namespace PebblePouch.Lore.Editor
{
    // Reads a Category CSV and works out what an import would change (CategoryCsvImportPlan). Changes nothing.
    // Rules: rows match Entries by Id, else by Title (within the Category); unmatched rows become new Entries;
    // Entries missing from the file stay as they are; missing columns are not touched; an empty cell clears the
    // value (broken references stay: they are written as empty cells by the export). Invalid cells and rows are
    // skipped with a warning, never half-applied.
    public static class CategoryCsvImportAnalyzer
    {
        #region Types
        private sealed class SourceRow
        {
            public CsvReader.Row Cells;
            public Plan.Row Row;

            // Title from the file that differs from the matched Entry's title (rename request).
            public string RequestedTitle;
        }

        // Entry reference targets by title (case-insensitive): existing Entries and Entries this import creates.
        private sealed class EntryIndex
        {
            public readonly Dictionary<string, List<Plan.EntryReference>> ByTitle =
                new(StringComparer.OrdinalIgnoreCase);
        }
        #endregion

        #region Public API
        public static Plan Analyze(
            Category category,
            string path)
        {
            Plan plan = new(category, path);

            string text;

            try
            {
                text = File.ReadAllText(path, Encoding.UTF8);
            }
            catch (Exception exception)
            {
                plan.Error = string.Format(LoreText.Csv.ReadFailed, exception.Message);

                return plan;
            }

            List<CsvReader.Row> rows = CsvReader.Parse(text);

            if (rows.Count > 0)
                MapColumns(plan, rows[0]);

            if (plan.Columns.Count == 0)
            {
                plan.Error = LoreText.Csv.NoKnownColumns;

                return plan;
            }

            if (FindColumn(plan, Plan.ColumnKind.Id) == null &&
                FindColumn(plan, Plan.ColumnKind.Title) == null)
            {
                plan.Error = LoreText.Csv.NoIdOrTitleColumn;

                return plan;
            }

            List<Entry> categoryEntries = LoreRepository.Instance.Entries
                .Where(entry => entry != null && entry.Category == category)
                .ToList();

            List<SourceRow> sourceRows = MatchRows(plan, rows.Skip(1), categoryEntries);

            ResolveTitles(plan, sourceRows, categoryEntries);

            plan.UntouchedEntryCount =
                categoryEntries.Count - plan.Rows.Count(row => !row.IsNew);

            EntryIndex index = BuildEntryIndex(plan);
            HashSet<Plan.Row> plannedRows = new(plan.Rows);

            foreach (SourceRow sourceRow in sourceRows)
            {
                if (plannedRows.Contains(sourceRow.Row))
                    ParseCells(plan, sourceRow, index);
            }

            return plan;
        }
        #endregion

        #region Columns
        private static void MapColumns(
            Plan plan,
            CsvReader.Row header)
        {
            List<Field> fields = plan.Category.Fields
                .Where(field => field != null)
                .ToList();

            HashSet<Plan.ColumnKind> usedEntryColumns = new();
            HashSet<Field> usedFields = new();

            for (int i = 0; i < header.Cells.Count; i++)
            {
                string name = header.Cells[i].Trim();

                if (name.Length == 0)
                    continue;

                // Entry columns first; a second column with the same name is a Field of that name.
                if (TryGetEntryColumn(name, out Plan.ColumnKind kind) &&
                    usedEntryColumns.Add(kind))
                {
                    plan.Columns.Add(new Plan.Column(i, kind, name));
                    continue;
                }

                Field field = fields.FirstOrDefault(candidate =>
                    string.Equals(candidate.Title, name, StringComparison.OrdinalIgnoreCase));

                if (field == null)
                {
                    plan.Warnings.Add(
                        string.Format(
                            TryGetEntryColumn(name, out _)
                                ? LoreText.Csv.DuplicateColumn
                                : LoreText.Csv.UnknownColumn,
                            name));
                }
                else if (!usedFields.Add(field))
                {
                    plan.Warnings.Add(string.Format(LoreText.Csv.DuplicateColumn, name));
                }
                else
                {
                    plan.Columns.Add(new Plan.Column(i, Plan.ColumnKind.Field, field.Title, field));
                }
            }
        }

        private static bool TryGetEntryColumn(
            string name,
            out Plan.ColumnKind kind)
        {
            for (int i = 0; i < CsvFormat.EntryColumns.Length; i++)
            {
                if (string.Equals(CsvFormat.EntryColumns[i], name, StringComparison.OrdinalIgnoreCase))
                {
                    // EntryColumns are in ColumnKind order.
                    kind = (Plan.ColumnKind)i;

                    return true;
                }
            }

            kind = default;

            return false;
        }

        private static Plan.Column FindColumn(
            Plan plan,
            Plan.ColumnKind kind) =>
            plan.Columns.FirstOrDefault(column => column.Kind == kind);
        #endregion

        #region Rows
        // Matches rows to Entries (Id, else Title); unmatched rows with a title become new Entries.
        private static List<SourceRow> MatchRows(
            Plan plan,
            IEnumerable<CsvReader.Row> rows,
            List<Entry> categoryEntries)
        {
            Plan.Column idColumn = FindColumn(plan, Plan.ColumnKind.Id);
            Plan.Column titleColumn = FindColumn(plan, Plan.ColumnKind.Title);

            Dictionary<Entry, int> matchedRows = new();
            List<SourceRow> result = new();

            foreach (CsvReader.Row cells in rows)
            {
                string id = idColumn != null ? cells.Get(idColumn.Index).Trim() : string.Empty;
                string title = titleColumn != null ? cells.Get(titleColumn.Index).Trim() : string.Empty;

                Entry entry = null;

                if (LoreRepository.Instance.TryGetEntry(id, out Entry byId))
                {
                    if (byId.Category != plan.Category)
                    {
                        plan.Warnings.Add(
                            string.Format(
                                LoreText.Csv.RowOtherCategory,
                                cells.Number,
                                byId.Title,
                                byId.Category != null ? byId.Category.Title : LoreText.Common.Uncategorized));

                        continue;
                    }

                    entry = byId;
                }
                else if (title.Length > 0)
                {
                    entry = categoryEntries.FirstOrDefault(candidate =>
                        string.Equals(candidate.Title, title, StringComparison.OrdinalIgnoreCase));
                }

                if (entry != null)
                {
                    if (matchedRows.TryGetValue(entry, out int firstRow))
                    {
                        plan.Warnings.Add(
                            string.Format(LoreText.Csv.RowDuplicate, cells.Number, entry.Title, firstRow));

                        continue;
                    }

                    matchedRows[entry] = cells.Number;

                    result.Add(
                        new SourceRow
                        {
                            Cells = cells,
                            Row = new Plan.Row(cells.Number, entry, entry.Title),
                            RequestedTitle = title.Length > 0 && title != entry.Title ? title : null,
                        });

                    continue;
                }

                if (title.Length == 0)
                {
                    plan.Warnings.Add(string.Format(LoreText.Csv.RowNoTitle, cells.Number));

                    continue;
                }

                result.Add(
                    new SourceRow
                    {
                        Cells = cells,
                        Row = new Plan.Row(cells.Number, null, title),
                    });
            }

            return result;
        }

        // Validates renames and new titles (valid name, unique in the Category, case-insensitive) and fills
        // plan.Rows in file order. Invalid renames keep the old title; invalid new rows are skipped.
        private static void ResolveTitles(
            Plan plan,
            List<SourceRow> sourceRows,
            List<Entry> categoryEntries)
        {
            Dictionary<string, object> taken = new(StringComparer.OrdinalIgnoreCase);

            foreach (Entry entry in categoryEntries)
                taken[entry.Title] = entry;

            foreach (SourceRow sourceRow in sourceRows)
            {
                Plan.Row row = sourceRow.Row;

                if (row.IsNew || sourceRow.RequestedTitle == null)
                    continue;

                string issue = GetTitleIssue(sourceRow.RequestedTitle, row.Entry, taken);

                if (issue != null)
                {
                    plan.Warnings.Add(
                        string.Format(
                            LoreText.Csv.RenameFailed,
                            row.Number,
                            row.Entry.Title,
                            sourceRow.RequestedTitle,
                            issue));

                    continue;
                }

                taken.Remove(row.Entry.Title);
                taken[sourceRow.RequestedTitle] = row.Entry;
                row.Title = sourceRow.RequestedTitle;
            }

            foreach (SourceRow sourceRow in sourceRows)
            {
                Plan.Row row = sourceRow.Row;

                if (row.IsNew)
                {
                    string issue = GetTitleIssue(row.Title, row, taken);

                    if (issue != null)
                    {
                        plan.Warnings.Add(
                            string.Format(LoreText.Csv.RowNewTitleFailed, row.Number, row.Title, issue));

                        continue;
                    }

                    taken[row.Title] = row;
                }

                plan.Rows.Add(row);
            }
        }

        private static string GetTitleIssue(
            string title,
            object owner,
            Dictionary<string, object> taken)
        {
            LoreOperationResult validation = LoreNameValidator.Validate(title);

            if (!validation.Success)
                return validation.Message;

            if (taken.TryGetValue(title, out object existing) &&
                existing != owner)
            {
                return string.Format(LoreText.Validation.EntryExists, title);
            }

            return null;
        }
        #endregion

        #region Cells
        private static void ParseCells(
            Plan plan,
            SourceRow sourceRow,
            EntryIndex index)
        {
            Plan.Row row = sourceRow.Row;
            Entry entry = row.Entry;

            foreach (Plan.Column column in plan.Columns)
            {
                string cell = sourceRow.Cells.Get(column.Index);
                Plan.Value value = new();
                bool changed;

                switch (column.Kind)
                {
                    case Plan.ColumnKind.Subtitle:
                        value.Text = cell;
                        changed = entry == null ? cell.Length > 0 : cell != (entry.Subtitle ?? string.Empty);
                        break;

                    case Plan.ColumnKind.Glyph:
                        value.Text = cell.Trim();
                        changed = entry == null
                            ? value.Text.Length > 0
                            : value.Text != (entry.HasGlyphOverride ? entry.Glyph : string.Empty);
                        break;

                    case Plan.ColumnKind.Tags:
                        value.TagIds = ParseTags(plan, row, cell);
                        changed = entry == null
                            ? value.TagIds.Count > 0
                            : !new HashSet<string>(value.TagIds).SetEquals(entry.TagIds);
                        break;

                    case Plan.ColumnKind.Description:
                        value.Text = cell;
                        value.BrokenLinkIds = new Dictionary<string, string>();

                        if (entry == null)
                        {
                            changed = cell.Length > 0;
                        }
                        else
                        {
                            // Compared in the editable form: the stored form also holds IDs and last known titles.
                            changed = cell != DescriptionLinkFormat.ToEditable(entry.Description, value.BrokenLinkIds);
                        }
                        break;

                    case Plan.ColumnKind.Field:
                        if (!TryParseField(plan, row, column, cell, index, value))
                            continue;

                        changed = IsFieldChange(entry, column.Field, value);
                        break;

                    default:
                        continue;
                }

                if (changed)
                    row.Changes.Add(new Plan.Change(column, value));
            }
        }

        private static List<string> ParseTags(
            Plan plan,
            Plan.Row row,
            string cell)
        {
            List<string> tagIds = new();
            TagCollection library = LoreRepository.Instance.Project != null
                ? LoreRepository.Instance.Project.TagLibrary
                : null;

            foreach (string title in SplitList(cell))
            {
                TagDefinition tag = library != null ? library.FindByTitle(title) : null;

                if (tag == null)
                {
                    plan.Warnings.Add(string.Format(LoreText.Csv.UnknownTag, row.Number, title));
                    continue;
                }

                if (!tagIds.Contains(tag.Id))
                    tagIds.Add(tag.Id);
            }

            return tagIds;
        }
        #endregion

        #region Field Values
        private static bool TryParseField(
            Plan plan,
            Plan.Row row,
            Plan.Column column,
            string cell,
            EntryIndex index,
            Plan.Value value)
        {
            Field field = column.Field;
            string trimmed = cell.Trim();
            string issue = null;

            switch (field.Type)
            {
                case FieldType.Text:
                case FieldType.Url:
                    value.Text = cell;
                    break;

                case FieldType.Boolean:
                    if (!TryParseBool(trimmed, out value.Bool))
                        issue = LoreText.Csv.InvalidBool;
                    break;

                case FieldType.Integer:
                    if (trimmed.Length > 0 &&
                        !int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out value.Int))
                    {
                        issue = LoreText.Csv.InvalidInteger;
                    }
                    break;

                case FieldType.Float:
                    if (trimmed.Length > 0 &&
                        !float.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out value.Float))
                    {
                        issue = LoreText.Csv.InvalidFloat;
                    }
                    break;

                case FieldType.Color:
                    if (!CsvFormat.TryParseColor(trimmed, out value.Color))
                        issue = LoreText.Csv.InvalidColor;
                    break;

                case FieldType.Vector:
                    if (!CsvFormat.TryParseVector(trimmed, field.VectorDimension, out value.Vector))
                        issue = LoreText.Csv.InvalidVector;
                    break;

                case FieldType.Enum:
                    value.EnumOptionId = string.Empty;

                    if (trimmed.Length > 0)
                    {
                        EnumOption option = field.EnumOptions.FirstOrDefault(candidate =>
                            candidate != null &&
                            string.Equals(candidate.Label, trimmed, StringComparison.OrdinalIgnoreCase));

                        if (option != null)
                            value.EnumOptionId = option.Id;
                        else
                            issue = LoreText.Csv.UnknownOption;
                    }
                    break;

                case FieldType.Enums:
                    value.EnumOptionIds = new List<string>();

                    foreach (string label in SplitList(cell))
                    {
                        EnumOption option = field.EnumOptions.FirstOrDefault(candidate =>
                            candidate != null &&
                            string.Equals(candidate.Label, label, StringComparison.OrdinalIgnoreCase));

                        if (option == null)
                        {
                            issue = LoreText.Csv.UnknownOption;
                            trimmed = label;
                            break;
                        }

                        if (!value.EnumOptionIds.Contains(option.Id))
                            value.EnumOptionIds.Add(option.Id);
                    }
                    break;

                case FieldType.Entry:
                case FieldType.Entries:
                    value.Entries = new List<Plan.EntryReference>();

                    List<string> titles = SplitList(cell);

                    if (field.Type == FieldType.Entry && titles.Count > 1)
                    {
                        issue = LoreText.Csv.SingleEntryOnly;
                        break;
                    }

                    foreach (string title in titles)
                    {
                        issue = ResolveEntry(plan, field, title, index, out Plan.EntryReference reference);

                        if (issue != null)
                        {
                            trimmed = title;
                            break;
                        }

                        if (!value.Entries.Contains(reference))
                            value.Entries.Add(reference);
                    }
                    break;

                case FieldType.GameObject:
                case FieldType.Sprite:
                case FieldType.Scene:
                case FieldType.Folder:
                case FieldType.ScriptableObject:
                case FieldType.Object:
                case FieldType.Audio:
                    if (trimmed.Length > 0)
                        issue = ResolveAsset(field, trimmed, out value.Asset);
                    break;
            }

            if (issue == null)
                return true;

            plan.Warnings.Add(
                string.Format(
                    LoreText.Csv.CellSkipped,
                    row.Number,
                    column.Header,
                    string.Format(issue, trimmed, ScriptableObjectTypeService.GetFieldTypeLabel(field))));

            return false;
        }

        private static bool TryParseBool(
            string text,
            out bool value)
        {
            switch (text.ToLowerInvariant())
            {
                case "":
                case "false":
                case "0":
                case "no":
                    value = false;
                    return true;

                case "true":
                case "1":
                case "yes":
                    value = true;
                    return true;

                default:
                    value = false;
                    return false;
            }
        }

        // "Title" or "Category/Title" (for titles that exist in several Categories). Only Categories the Field allows.
        private static string ResolveEntry(
            Plan plan,
            Field field,
            string text,
            EntryIndex index,
            out Plan.EntryReference reference)
        {
            reference = default;

            string categoryTitle = null;
            string title = text;
            int slash = text.IndexOf('/');

            if (slash > 0)
            {
                categoryTitle = text.Substring(0, slash).Trim();
                title = text.Substring(slash + 1).Trim();
            }

            List<Plan.EntryReference> candidates = new();

            if (index.ByTitle.TryGetValue(title, out List<Plan.EntryReference> references))
            {
                foreach (Plan.EntryReference candidate in references)
                {
                    Category category = candidate.Existing != null
                        ? candidate.Existing.Category
                        : plan.Category;

                    if (category == null ||
                        !field.AllowsCategory(category) ||
                        (categoryTitle != null &&
                         !string.Equals(category.Title, categoryTitle, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    candidates.Add(candidate);
                }
            }

            if (candidates.Count == 0)
                return LoreText.Csv.EntryNotFound;

            if (candidates.Count > 1)
                return LoreText.Csv.EntryAmbiguous;

            reference = candidates[0];

            return null;
        }

        // Titles after the import: renamed Entries of the Category under their new title, new Entries included.
        private static EntryIndex BuildEntryIndex(Plan plan)
        {
            EntryIndex index = new();
            Dictionary<Entry, string> titles = new();

            foreach (Plan.Row row in plan.Rows)
            {
                if (row.Renames)
                    titles[row.Entry] = row.Title;
            }

            foreach (Entry entry in LoreRepository.Instance.Entries)
            {
                if (entry == null)
                    continue;

                Add(index, titles.TryGetValue(entry, out string title) ? title : entry.Title, new Plan.EntryReference(entry, null));
            }

            foreach (Plan.Row row in plan.Rows)
            {
                if (row.IsNew)
                    Add(index, row.Title, new Plan.EntryReference(null, row));
            }

            return index;

            static void Add(
                EntryIndex target,
                string title,
                Plan.EntryReference reference)
            {
                if (!target.ByTitle.TryGetValue(title, out List<Plan.EntryReference> references))
                {
                    references = new List<Plan.EntryReference>();
                    target.ByTitle[title] = references;
                }

                references.Add(reference);
            }
        }

        // Asset path, or "path#name" for one of several sub-assets (e.g. a Sprite of a sprite sheet).
        private static string ResolveAsset(
            Field field,
            string text,
            out UnityEngine.Object asset)
        {
            asset = null;

            string path = text;
            string name = null;

            if (AssetDatabase.LoadMainAssetAtPath(path) == null)
            {
                int separator = text.LastIndexOf(CsvFormat.SubAssetSeparator);

                if (separator > 0)
                {
                    path = text.Substring(0, separator);
                    name = text.Substring(separator + 1);
                }
            }

            UnityEngine.Object mainAsset = AssetDatabase.LoadMainAssetAtPath(path);

            if (mainAsset == null)
                return LoreText.Csv.AssetNotFound;

            List<UnityEngine.Object> candidates = field.Type switch
            {
                FieldType.GameObject => new List<UnityEngine.Object> { mainAsset as GameObject },
                FieldType.Scene => new List<UnityEngine.Object> { mainAsset as SceneAsset },
                FieldType.Folder => new List<UnityEngine.Object>
                {
                    AssetDatabase.IsValidFolder(path) ? mainAsset : null
                },
                FieldType.Audio => new List<UnityEngine.Object> { mainAsset as AudioClip },
                FieldType.Sprite => AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().Cast<UnityEngine.Object>().ToList(),
                FieldType.Object => GetObjectCandidates(field.ObjectType, path, mainAsset, name),
                _ => AssetDatabase.LoadAllAssetsAtPath(path)
                    .OfType<ScriptableObject>()
                    .Where(candidate => field.ObjectType == null || field.ObjectType.IsInstanceOfType(candidate))
                    .Cast<UnityEngine.Object>()
                    .ToList(),
            };

            candidates.RemoveAll(candidate => candidate == null);

            if (name != null)
                candidates.RemoveAll(candidate => candidate.name != name);

            if (candidates.Count == 0)
                return LoreText.Csv.AssetWrongType;

            // Several of the kind (sprite sheet): the main asset wins if it is one, else a name is needed.
            if (candidates.Count > 1)
            {
                if (!candidates.Contains(mainAsset))
                    return LoreText.Csv.AssetAmbiguous;

                asset = mainAsset;

                return null;
            }

            asset = candidates[0];

            return null;
        }

        // Object Fields: the assets of the Field's type at the path (none while the type is missing). Component types:
        // the prefab root's component, else the components of that type in the prefab (named by their GameObject).
        private static List<UnityEngine.Object> GetObjectCandidates(
            System.Type objectType,
            string path,
            UnityEngine.Object mainAsset,
            string name)
        {
            if (objectType == null)
                return new List<UnityEngine.Object>();

            if (typeof(Component).IsAssignableFrom(objectType) &&
                mainAsset is GameObject root &&
                (name == null || name == root.name))
            {
                Component component = root.GetComponent(objectType);

                if (component != null)
                    return new List<UnityEngine.Object> { component };
            }

            return AssetDatabase.LoadAllAssetsAtPath(path)
                .Where(objectType.IsInstanceOfType)
                .ToList();
        }

        // Empty cells clear values, except broken references: they stay (the export writes them as empty cells).
        private static bool IsFieldChange(
            Entry entry,
            Field field,
            Plan.Value value)
        {
            FieldValue current = null;

            if (entry != null)
                entry.TryGetValue(field, out current);

            switch (field.Type)
            {
                case FieldType.Text:
                case FieldType.Url:
                    return value.Text != (current?.StringValue ?? string.Empty);

                case FieldType.Boolean:
                    return value.Bool != (current != null && current.BoolValue);

                case FieldType.Integer:
                    return value.Int != (current?.IntValue ?? 0);

                case FieldType.Float:
                    return !value.Float.Equals(current?.FloatValue ?? 0f);

                // Compared as exported (8-bit channels), so an unchanged export reads back as unchanged.
                case FieldType.Color:
                    return CsvFormat.FormatColor(value.Color) !=
                           CsvFormat.FormatColor(current?.ColorValue ?? UnityEngine.Color.black);

                // 2D Fields compare x and y only (the import keeps the stored z).
                case FieldType.Vector:
                    return CsvFormat.FormatVector(value.Vector, field.VectorDimension) !=
                           CsvFormat.FormatVector(
                               current?.VectorValue ?? UnityEngine.Vector3.zero,
                               field.VectorDimension);

                case FieldType.Enum:
                    string currentOptionId = current?.EnumValue ?? string.Empty;

                    // Deleted option (broken, exported as empty): stays.
                    if (value.EnumOptionId.Length == 0 &&
                        field.EnumOptions.All(option => option == null || option.Id != currentOptionId))
                    {
                        return false;
                    }

                    return value.EnumOptionId != currentOptionId;

                // Compared as sets of existing options (removed options stay, see the importer).
                case FieldType.Enums:
                    List<string> currentOptionIds =
                        (current?.EnumValues ?? (IReadOnlyList<string>)Array.Empty<string>())
                        .Where(optionId => field.FindEnumOption(optionId) != null)
                        .ToList();

                    return value.EnumOptionIds.Count != currentOptionIds.Count ||
                           value.EnumOptionIds.Any(optionId => !currentOptionIds.Contains(optionId));

                case FieldType.Entry:
                case FieldType.Entries:
                    List<Entry> currentEntries = current == null
                        ? new List<Entry>()
                        : field.Type == FieldType.Entry
                            ? new List<Entry> { current.EntryValue }
                            : current.EntriesValue.ToList();

                    currentEntries.RemoveAll(candidate => candidate == null);

                    return value.Entries.Count != currentEntries.Count ||
                           value.Entries.Where((reference, i) => reference.Existing != currentEntries[i]).Any();

                default:
                    return value.Asset != GetAsset(field.Type, current);
            }
        }

        private static UnityEngine.Object GetAsset(
            FieldType type,
            FieldValue value)
        {
            if (value == null)
                return null;

            return type switch
            {
                FieldType.GameObject => value.GameObjectValue,
                FieldType.Sprite => value.SpriteValue,
                FieldType.Scene => value.SceneValue,
                FieldType.Folder => value.FolderValue,
                _ => value.ObjectValue,
            };
        }

        private static List<string> SplitList(string cell) =>
            cell.Split(CsvFormat.ListSeparator)
                .Select(part => part.Trim())
                .Where(part => part.Length > 0)
                .ToList();
        #endregion
    }
}
