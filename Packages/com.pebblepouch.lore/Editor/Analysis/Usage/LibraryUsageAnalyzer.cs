using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class LibraryUsageAnalyzer
    {
        #region Fields
        private readonly LoreRepository _repository;

        // Fetched per analysis: the Library assets can be created or replaced in the meantime.
        private TagCollection _tagCollection;
        private GlyphLibraryCollection _glyphCollection;
        private ColorLibraryCollection _colorCollection;
        #endregion

        #region Constructor
        public LibraryUsageAnalyzer(
            LoreRepository repository)
        {
            _repository = repository;
        }
        #endregion

        #region Public API
        public LibraryUsageReport Analyze()
        {
            _tagCollection = new TagLibraryService().GetCollection();
            _glyphCollection = new GlyphLibraryService().GetCollection();
            _colorCollection = new ColorLibraryService().GetCollection();

            IReadOnlyList<FieldLibraryItem> fields =
                new FieldLibrarySource()
                    .GetItems();

            int usedFieldCount =
                fields.Count(
                    item =>
                        IsFieldUsed(
                            item.Field));

            int usedTagCount =
                _tagCollection.Tags
                    .Count(
                        tag =>
                            IsTagUsed(
                                tag));

            int usedGlyphCount =
                _glyphCollection.Glyphs
                    .Count(
                        definition =>
                            IsGlyphUsed(
                                definition.Glyph,
                                fields));

            int usedColorCount =
                _colorCollection.Colors
                    .Count(
                        definition =>
                            IsColorUsed(
                                definition.Color));

            return new LibraryUsageReport(
                fields.Count,
                usedFieldCount,
                _tagCollection.Tags.Count,
                usedTagCount,
                _glyphCollection.Glyphs.Count,
                usedGlyphCount,
                _colorCollection.Colors.Count,
                usedColorCount);
        }

        public IReadOnlyList<Field> GetUnusedFields()
        {
            return new FieldLibrarySource()
                .GetItems()
                .Where(
                    item =>
                        item.Field != null &&
                        !IsFieldUsed(
                            item.Field))
                .Select(
                    item =>
                        item.Field)
                .ToList();
        }

        public bool IsFieldUsed(Field field)
        {
            if (field == null)
                return false;

            return _repository
                .Categories
                .Any(
                    category =>
                        category != null &&
                        category.Fields.Contains(
                            field));
        }

        public bool IsTagUsed(TagDefinition tag)
        {
            if (tag == null)
                return false;

            return _repository
                .Entries
                .Any(
                    entry =>
                        entry != null &&
                        entry.TagIds.Contains(
                            tag.Id));
        }

        public bool IsGlyphUsed(string glyph)
        {
            IReadOnlyList<FieldLibraryItem> fields =
                new FieldLibrarySource()
                    .GetItems();

            return IsGlyphUsed(
                glyph,
                fields);
        }

        public bool IsColorUsed(Color color)
        {
            return _repository
                .Categories
                .Any(
                    category =>
                        category != null &&
                        category.Color ==
                        color);
        }
        #endregion

        #region Helpers
        private bool IsGlyphUsed(
            string glyph,
            IReadOnlyList<FieldLibraryItem> fields)
        {
            if (string.IsNullOrEmpty(
                    glyph))
            {
                return false;
            }

            bool usedByCategory =
                _repository
                    .Categories
                    .Any(
                        category =>
                            category != null &&
                            category.Glyph ==
                            glyph);

            if (usedByCategory)
                return true;

            bool usedByField =
                fields.Any(
                    item =>
                        item.Field != null &&
                        item.Field.Glyph ==
                        glyph);

            if (usedByField)
                return true;

            return _repository
                .Entries
                .Any(
                    entry =>
                        entry != null &&
                        entry.HasGlyphOverride &&
                        entry.Glyph ==
                        glyph);
        }
        #endregion
    }
}