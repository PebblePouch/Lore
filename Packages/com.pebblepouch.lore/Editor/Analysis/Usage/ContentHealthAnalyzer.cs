using System;
using System.Collections.Generic;
using System.Linq;

namespace PebblePouch.Lore.Editor
{
    // Finds valid but unfinished content: missing descriptions, empty Categories, placeholder names,
    // Entries without Tags. The predicates are also used as Content browser filters.
    public sealed class ContentHealthAnalyzer
    {
        #region Fields
        private readonly LoreRepository _repository;
        #endregion

        #region Constructor
        public ContentHealthAnalyzer(LoreRepository repository)
        {
            _repository = repository;
        }
        #endregion

        #region Public API
        public ContentHealthReport Analyze(int tagCount)
        {
            List<Entry> entries =
                _repository.Entries
                    .Where(entry => entry != null)
                    .ToList();

            List<Category> categories =
                _repository.Categories
                    .Where(category => category != null)
                    .ToList();

            return new ContentHealthReport(
                entries.Count(HasNoDescription),
                categories.Count(HasNoDescription),
                categories.Count(IsEmpty),
                entries.Count(HasPlaceholderName) + categories.Count(HasPlaceholderName),
                entries.Count(HasNoTags),
                tagCount > 0,
                entries.Count(HasMissingRequiredValues),
                categories.Any(category => category.RequiredFields.Count > 0));
        }

        // Required Fields (per Category) that are still empty. Booleans never count: false is a valid value.
        public bool HasMissingRequiredValues(Entry entry)
        {
            if (entry?.Category == null)
                return false;

            foreach (FieldValue fieldValue in entry.Fields)
            {
                if (IsMissingRequiredValue(entry, fieldValue))
                    return true;
            }

            return false;
        }

        public static bool IsMissingRequiredValue(
            Entry entry,
            FieldValue fieldValue)
        {
            return entry?.Category != null &&
                   fieldValue?.Field != null &&
                   fieldValue.Field.Type != FieldType.Boolean &&
                   entry.Category.IsRequired(fieldValue.Field) &&
                   !FieldValueUtility.IsPopulated(
                       fieldValue,
                       fieldValue.Field.Type);
        }

        public bool HasNoDescription(Entry entry)
        {
            return entry != null &&
                   string.IsNullOrWhiteSpace(entry.Description);
        }

        public bool HasNoDescription(Category category)
        {
            return category != null &&
                   string.IsNullOrWhiteSpace(category.Description);
        }

        public bool IsEmpty(Category category)
        {
            return category != null &&
                   !_repository.Entries.Any(
                       entry =>
                           entry != null &&
                           entry.Category == category);
        }

        public bool HasNoTags(Entry entry)
        {
            return entry != null &&
                   entry.TagIds.Count == 0;
        }

        // Still the name given on creation: "New Entry", "New Entry 2", ...
        public bool HasPlaceholderName(Entry entry)
        {
            return entry != null &&
                   IsPlaceholder(
                       entry.Title,
                       LoreText.Defaults.NewEntry);
        }

        public bool HasPlaceholderName(Category category)
        {
            return category != null &&
                   IsPlaceholder(
                       category.Title,
                       LoreText.Defaults.NewCategory);
        }
        #endregion

        #region Helpers
        private static bool IsPlaceholder(
            string title,
            string defaultName)
        {
            if (string.IsNullOrEmpty(title))
                return false;

            if (title == defaultName)
                return true;

            // Numbered variant (LoreText.Defaults.NumberedName): default name, space, number.
            string prefix = defaultName + " ";

            return title.StartsWith(prefix, StringComparison.Ordinal) &&
                   title.Length > prefix.Length &&
                   title.Substring(prefix.Length).All(char.IsDigit);
        }
        #endregion
    }
}
