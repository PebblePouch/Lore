using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Writes the Entries of a Category as CSV (see CsvFormat): one row per Entry in browser order, the Entry columns,
    // then one column per Field in the Category's order. Broken references (deleted Entries, missing assets) are
    // written as empty cells and counted (the import keeps broken references for empty cells).
    public static class CategoryCsvExporter
    {
        #region Types
        public sealed class Result
        {
            public bool Success { get; }
            public string Path { get; }
            public int EntryCount { get; }
            public int BrokenReferenceCount { get; }
            public string Error { get; }

            public Result(
                bool success,
                string path,
                int entryCount = 0,
                int brokenReferenceCount = 0,
                string error = null)
            {
                Success = success;
                Path = path;
                EntryCount = entryCount;
                BrokenReferenceCount = brokenReferenceCount;
                Error = error;
            }
        }
        #endregion

        #region Public API
        // Save dialog (starts in the last CSV folder), export, console message.
        public static void ExportWithDialog(Category category)
        {
            if (category == null)
                return;

            string path = EditorUtility.SaveFilePanel(
                LoreText.Csv.ExportTitle,
                LorePreferences.LastCsvFolder,
                category.Title + "." + CsvFormat.FileExtension,
                CsvFormat.FileExtension);

            if (string.IsNullOrEmpty(path))
                return;

            LorePreferences.LastCsvFolder = System.IO.Path.GetDirectoryName(path);

            Result result = Export(category, path);

            if (!result.Success)
            {
                Debug.LogError(
                    $"Lore: Could not export '{category.Title}' to {result.Path}: {result.Error}");

                return;
            }

            string message =
                $"Lore: Exported {result.EntryCount} {(result.EntryCount == 1 ? "Entry" : "Entries")} " +
                $"of '{category.Title}' to {result.Path}.";

            if (result.BrokenReferenceCount > 0)
            {
                message +=
                    $" {result.BrokenReferenceCount} broken {(result.BrokenReferenceCount == 1 ? "reference was" : "references were")} " +
                    "written as empty cells (importing the file keeps them).";

                Debug.LogWarning(message, category);
            }
            else
            {
                Debug.Log(message, category);
            }
        }

        public static Result Export(
            Category category,
            string path)
        {
            List<Field> fields = category.Fields
                .Where(field => field != null)
                .ToList();

            List<Entry> entries = GetEntries(category);

            StringBuilder builder = new();
            int brokenReferenceCount = 0;

            AppendRow(
                builder,
                CsvFormat.EntryColumns.Concat(fields.Select(field => field.Title)));

            foreach (Entry entry in entries)
            {
                List<string> cells = new()
                {
                    entry.Id,
                    entry.Title,
                    entry.Subtitle,
                    entry.HasGlyphOverride ? entry.Glyph : string.Empty,
                    FormatTags(entry),
                    DescriptionLinkFormat.ToEditable(entry.Description, null),
                };

                foreach (Field field in fields)
                {
                    cells.Add(
                        entry.TryGetValue(field, out FieldValue value)
                            ? FormatValue(field, value, ref brokenReferenceCount)
                            : string.Empty);
                }

                AppendRow(builder, cells);
            }

            try
            {
                File.WriteAllText(
                    path,
                    builder.ToString(),
                    CsvFormat.FileEncoding);
            }
            catch (Exception exception)
            {
                return new Result(false, path, error: exception.Message);
            }

            return new Result(true, path, entries.Count, brokenReferenceCount);
        }
        #endregion

        #region Rows
        // Browser order (the Category's group in the repository).
        private static List<Entry> GetEntries(Category category)
        {
            foreach (CategoryEntryGroup group in LoreRepository.Instance.Groups)
            {
                if (group.Category == category)
                    return group.Entries.Where(entry => entry != null).ToList();
            }

            return new List<Entry>();
        }

        private static void AppendRow(
            StringBuilder builder,
            IEnumerable<string> cells)
        {
            bool first = true;

            foreach (string cell in cells)
            {
                if (!first)
                    builder.Append(CsvFormat.Separator);

                builder.Append(CsvFormat.Escape(cell));

                first = false;
            }

            builder.Append(CsvFormat.NewLine);
        }
        #endregion

        #region Cells
        private static string FormatTags(Entry entry)
        {
            TagCollection tags = LoreRepository.Instance.Project != null
                ? LoreRepository.Instance.Project.TagLibrary
                : null;

            if (tags == null)
                return string.Empty;

            return string.Join(
                CsvFormat.ListSeparator.ToString(),
                entry.TagIds
                    .Select(tags.FindById)
                    .Where(tag => tag != null)
                    .Select(tag => tag.Title));
        }

        private static string FormatValue(
            Field field,
            FieldValue value,
            ref int brokenReferenceCount)
        {
            switch (field.Type)
            {
                case FieldType.Text:
                case FieldType.Url:
                    return value.StringValue ?? string.Empty;

                case FieldType.Boolean:
                    return CsvFormat.FormatBool(value.BoolValue);

                case FieldType.Integer:
                    return CsvFormat.FormatInt(value.IntValue);

                case FieldType.Float:
                    return CsvFormat.FormatFloat(value.FloatValue);

                case FieldType.Enum:
                    return FormatEnum(field, value.EnumValue, ref brokenReferenceCount);

                case FieldType.Entry:
                    return FormatEntry(value.EntryValue, ref brokenReferenceCount);

                case FieldType.Entries:
                    return FormatEntries(value.EntriesValue, ref brokenReferenceCount);

                case FieldType.GameObject:
                    return FormatAsset(value.GameObjectValue, ref brokenReferenceCount);

                case FieldType.Sprite:
                    return FormatAsset(value.SpriteValue, ref brokenReferenceCount);

                case FieldType.Scene:
                    return FormatAsset(value.SceneValue, ref brokenReferenceCount);

                case FieldType.ScriptableObject:
                    return FormatAsset(value.ObjectValue, ref brokenReferenceCount);

                default:
                    return string.Empty;
            }
        }

        private static string FormatEnum(
            Field field,
            string optionId,
            ref int brokenReferenceCount)
        {
            if (string.IsNullOrEmpty(optionId))
                return string.Empty;

            foreach (EnumOption option in field.EnumOptions)
            {
                if (option != null &&
                    option.Id == optionId)
                {
                    return option.Label;
                }
            }

            brokenReferenceCount++;

            return string.Empty;
        }

        private static string FormatEntries(
            IReadOnlyList<Entry> entries,
            ref int brokenReferenceCount)
        {
            List<string> titles = new();

            foreach (Entry entry in entries)
            {
                string title = FormatEntry(entry, ref brokenReferenceCount);

                if (title.Length > 0)
                    titles.Add(title);
            }

            return string.Join(CsvFormat.ListSeparator.ToString(), titles);
        }

        // Entry title (the import resolves titles within the Categories the Field allows).
        private static string FormatEntry(
            Entry entry,
            ref int brokenReferenceCount)
        {
            if (entry != null)
                return entry.Title;

            // Destroyed/missing Entry (not a plain empty value).
            if (!ReferenceEquals(entry, null))
                brokenReferenceCount++;

            return string.Empty;
        }

        // Asset path; sub-assets (e.g. one Sprite of a sprite sheet) as "path#name". Single sub-assets of their
        // kind (a texture with one Sprite) need no name.
        private static string FormatAsset(
            UnityEngine.Object asset,
            ref int brokenReferenceCount)
        {
            if (asset == null)
            {
                if (!ReferenceEquals(asset, null))
                    brokenReferenceCount++;

                return string.Empty;
            }

            string path = AssetDatabase.GetAssetPath(asset);

            if (string.IsNullOrEmpty(path))
                return string.Empty;

            if (!AssetDatabase.IsSubAsset(asset))
                return path;

            Type type = asset.GetType();

            int sameTypeCount = AssetDatabase.LoadAllAssetsAtPath(path)
                .Count(candidate => candidate != null && candidate.GetType() == type);

            return sameTypeCount > 1
                ? path + CsvFormat.SubAssetSeparator + asset.name
                : path;
        }
        #endregion
    }
}
