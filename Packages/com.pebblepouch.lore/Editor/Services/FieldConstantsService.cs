using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Generates the Field constants class (Project Settings > Lore): one FieldKey per Field asset, keyed by Field ID.
    // The output is stable (sorted, no timestamp), and the file is only written when its content changes, so
    // generating again without changes does not trigger a recompile.
    public static class FieldConstantsService
    {
        #region Types
        public enum Status
        {
            Written,
            Unchanged,
            Invalid,
            Failed,
        }

        public sealed class Result
        {
            public Status Status { get; }
            public string Path { get; }
            public int Count { get; }

            // Invalid: the settings issue; Failed: the exception message.
            public string Issue { get; }

            // Constant names that got a number because several Field titles map to the same name.
            public IReadOnlyList<string> NumberedNames { get; }

            public Result(
                Status status,
                string path,
                int count = 0,
                string issue = null,
                IReadOnlyList<string> numberedNames = null)
            {
                Status = status;
                Path = path;
                Count = count;
                Issue = issue;
                NumberedNames = numberedNames ?? Array.Empty<string>();
            }
        }

        private readonly struct Constant
        {
            public readonly Field Field;
            public readonly string Name;

            public Constant(
                Field field,
                string name)
            {
                Field = field;
                Name = name;
            }
        }
        #endregion

        #region Constants
        private const string FallbackName = "Field";
        private const string Indent = "    ";
        private const string NewLine = "\n";
        #endregion

        #region Menu
        [MenuItem(LoreMenu.GenerateFieldConstants, false, LoreMenu.GenerateFieldConstantsPriority)]
        private static void GenerateFromMenu()
        {
            if (GenerateAndLog().Status == Status.Invalid)
                SettingsService.OpenProjectSettings(LoreSettingsProvider.SettingsPath);
        }
        #endregion

        #region Public API
        // Generate + console message (the settings page's result box is lost in the recompile after writing).
        public static Result GenerateAndLog()
        {
            Result result = Generate();

            switch (result.Status)
            {
                case Status.Invalid:
                case Status.Failed:
                    Debug.LogError(FormatResult(result));
                    break;

                default:
                    Debug.Log(
                        FormatResult(result),
                        AssetDatabase.LoadAssetAtPath<MonoScript>(result.Path));
                    break;
            }

            return result;
        }

        public static Result Generate()
        {
            LoreSettings settings = LoreSettings.instance;
            string path = settings.FieldConstantsPath;
            string issue = settings.GetFieldConstantsIssue();

            if (issue != null)
                return new Result(Status.Invalid, path, issue: issue);

            string source = BuildExpectedSource(
                settings,
                out int count,
                out List<string> numberedNames);

            try
            {
                if (IsFileContent(path, source))
                {
                    settings.SetLastFieldConstantsPath(path);

                    return new Result(Status.Unchanged, path, count, numberedNames: numberedNames);
                }

                FolderService.EnsureFolderPath(System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/'));

                File.WriteAllText(
                    path,
                    source,
                    new UTF8Encoding(false));

                settings.SetLastFieldConstantsPath(path);

                AssetDatabase.ImportAsset(path);
            }
            catch (Exception exception)
            {
                return new Result(Status.Failed, path, issue: exception.Message);
            }

            return new Result(Status.Written, path, count, numberedNames: numberedNames);
        }

        // True when constants were generated before and the file no longer matches the Fields and settings (Fields
        // added, renamed or deleted; settings changed; file edited). False when never generated or the file is gone.
        // Reads the file: cache the result (LoreCache).
        public static bool IsOutOfDate()
        {
            LoreSettings settings = LoreSettings.instance;
            string lastPath = settings.LastFieldConstantsPath;

            if (string.IsNullOrEmpty(lastPath) ||
                !File.Exists(lastPath))
            {
                return false;
            }

            if (lastPath != settings.FieldConstantsPath ||
                settings.GetFieldConstantsIssue() != null)
            {
                return true;
            }

            try
            {
                return !IsFileContent(
                    lastPath,
                    BuildExpectedSource(settings, out _, out _));
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string FormatResult(Result result)
        {
            string message = result.Status switch
            {
                Status.Written => string.Format(
                    LoreText.Settings.GeneratedConstants.For(result.Count),
                    result.Count,
                    result.Path),
                Status.Unchanged => string.Format(
                    LoreText.Settings.ConstantsUpToDate,
                    result.Path),
                Status.Invalid => string.Format(
                    LoreText.Settings.ConstantsInvalidSettings,
                    result.Issue),
                _ => string.Format(
                    LoreText.Settings.ConstantsWriteFailed,
                    result.Path,
                    result.Issue),
            };

            if (result.NumberedNames.Count > 0)
            {
                message += " " + string.Format(
                    LoreText.Settings.NumberedConstants,
                    string.Join(", ", result.NumberedNames));
            }

            return message;
        }
        #endregion

        #region Constant Names
        // All Field assets (like the Field Library), with an ID.
        private static List<Field> FindFields()
        {
            return AssetDatabase.FindAssets("t:Field")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(path => AssetDatabase.LoadAssetAtPath<Field>(path))
                .Where(field => field != null && !string.IsNullOrEmpty(field.Id))
                .Distinct()
                .ToList();
        }

        // Sorted by name (then title and ID, so the output never depends on asset order); duplicates get a number.
        private static List<Constant> BuildConstants(
            List<Field> fields,
            string className,
            out List<string> numberedNames)
        {
            numberedNames = new List<string>();

            List<(Field Field, string Name)> candidates = fields
                .Select(field => (Field: field, Name: ToIdentifier(field.Title)))
                .OrderBy(candidate => candidate.Name, StringComparer.Ordinal)
                .ThenBy(candidate => candidate.Field.Title, StringComparer.Ordinal)
                .ThenBy(candidate => candidate.Field.Id, StringComparer.Ordinal)
                .ToList();

            // A member cannot have the name of its class.
            HashSet<string> used = new(StringComparer.Ordinal) { className };
            List<Constant> constants = new();

            foreach ((Field field, string baseName) in candidates)
            {
                string name = baseName;

                for (int number = 2; used.Contains(name); number++)
                    name = baseName + number.ToString(CultureInfo.InvariantCulture);

                if (name != baseName)
                    numberedNames.Add($"{name} ({field.Title})");

                used.Add(name);
                constants.Add(new Constant(field, name));
            }

            constants.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));

            return constants;
        }

        // "Max HP" -> "MaxHP", "2nd weapon" -> "_2ndWeapon", "Größe" -> "Groe": words in PascalCase (rest of a word
        // as written), accents removed, other characters dropped.
        private static string ToIdentifier(string title)
        {
            string normalized = (title ?? string.Empty).Normalize(NormalizationForm.FormD);
            StringBuilder builder = new();
            bool startOfWord = true;

            foreach (char character in normalized)
            {
                bool isAsciiLetterOrDigit =
                    character is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9';

                if (!isAsciiLetterOrDigit)
                {
                    // Accents (combining marks) belong to the previous letter; everything else separates words.
                    if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                        startOfWord = true;

                    continue;
                }

                builder.Append(
                    startOfWord
                        ? char.ToUpperInvariant(character)
                        : character);

                startOfWord = false;
            }

            string name = builder.Length > 0
                ? builder.ToString()
                : FallbackName;

            if (char.IsDigit(name[0]))
                name = "_" + name;

            if (CSharpIdentifier.IsKeyword(name))
                name += "_";

            return name;
        }
        #endregion

        #region Source
        private static string BuildExpectedSource(
            LoreSettings settings,
            out int count,
            out List<string> numberedNames)
        {
            List<Constant> constants = BuildConstants(
                FindFields(),
                settings.FieldConstantsClassName,
                out numberedNames);

            count = constants.Count;

            return BuildSource(
                constants,
                settings.FieldConstantsNamespace,
                settings.FieldConstantsClassName);
        }

        private static bool IsFileContent(
            string path,
            string source) =>
            File.Exists(path) &&
            NormalizeNewLines(File.ReadAllText(path)) == source;

        private static string BuildSource(
            List<Constant> constants,
            string namespaceName,
            string className)
        {
            Dictionary<Field, List<string>> categoriesByField = GetCategoriesByField();

            StringBuilder builder = new();
            string indent = string.IsNullOrEmpty(namespaceName) ? string.Empty : Indent;

            AppendLine(builder, "// <auto-generated>");
            AppendLine(builder, "// Generated by Lore (Tools > Lore > Generate Field Constants). Do not edit:");
            AppendLine(builder, "// changes are lost when the class is generated again.");
            AppendLine(builder, "// </auto-generated>");
            AppendLine(builder, "using PebblePouch.Lore;");
            AppendLine(builder);

            if (!string.IsNullOrEmpty(namespaceName))
            {
                AppendLine(builder, $"namespace {namespaceName}");
                AppendLine(builder, "{");
            }

            AppendLine(builder, $"{indent}public static class {className}");
            AppendLine(builder, $"{indent}{{");

            for (int i = 0; i < constants.Count; i++)
            {
                Constant constant = constants[i];
                Field field = constant.Field;

                if (i > 0)
                    AppendLine(builder);

                AppendLine(
                    builder,
                    $"{indent}{Indent}/// <summary>{EscapeXml(GetSummary(field, categoriesByField))}</summary>");

                AppendLine(
                    builder,
                    $"{indent}{Indent}public static readonly FieldKey {constant.Name} = " +
                    $"FieldKey.FromId(\"{EscapeString(field.Id)}\", \"{EscapeString(field.Title)}\");");
            }

            AppendLine(builder, $"{indent}}}");

            if (!string.IsNullOrEmpty(namespaceName))
                AppendLine(builder, "}");

            return builder.ToString();
        }

        // "Health · Integer · Characters, Monsters" (Categories in their order; "no Category" when unused).
        private static string GetSummary(
            Field field,
            Dictionary<Field, List<string>> categoriesByField)
        {
            string categories =
                categoriesByField.TryGetValue(field, out List<string> titles)
                    ? string.Join(", ", titles)
                    : "no Category";

            return $"{field.Title} · {ScriptableObjectTypeService.GetFieldTypeLabel(field)} · {categories}";
        }

        private static Dictionary<Field, List<string>> GetCategoriesByField()
        {
            Dictionary<Field, List<string>> result = new();

            foreach (Category category in LoreRepository.Instance.Categories)
            {
                if (category == null)
                    continue;

                foreach (Field field in category.Fields)
                {
                    if (field == null)
                        continue;

                    if (!result.TryGetValue(field, out List<string> titles))
                    {
                        titles = new List<string>();
                        result[field] = titles;
                    }

                    if (!titles.Contains(category.Title))
                        titles.Add(category.Title);
                }
            }

            return result;
        }

        private static void AppendLine(
            StringBuilder builder,
            string line = "")
        {
            builder.Append(line);
            builder.Append(NewLine);
        }

        private static string EscapeString(string value) =>
            (value ?? string.Empty)
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"");

        private static string EscapeXml(string value) =>
            value
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;");

        private static string NormalizeNewLines(string value) =>
            value.Replace("\r\n", "\n");
        #endregion
    }
}
