using System;
using System.Collections.Generic;

using UnityEngine;

namespace PebblePouch.Lore
{
    public class LoreProject : LoreAsset
    {
        #region Constants
        // Shown while the Project has no glyph of its own (also for Projects from before Project glyphs).
        // Same form as in the default Glyph library (with emoji variation selector), so the picker finds it.
        public const string DefaultGlyph = "\U0001F4DC\uFE0F"; // Scroll

        // Layout of the Lore data written by this Lore version. Raise it with every data change that needs an
        // upgrade of existing assets, and add the upgrade step in the editor (LoreFormatUpgradeService).
        public const int CurrentFormatVersion = 1;
        #endregion

        #region Fields
        // Format of this Project's data. 0 = written before format versions existed (same layout as format 1).
        [SerializeField, HideInInspector]
        private int _formatVersion;

        [SerializeField, HideInInspector]
        private string _title;

        [SerializeField, HideInInspector]
        private string _glyph;

        [SerializeField, HideInInspector]
        private string _description;

        [SerializeField, HideInInspector]
        private List<ProjectLink> _links = new();

        // Runtime index: everything a game can reach from the Project (also what ends up in a build).
        // Kept in sync by the editor (LoreRepository, before every build); never edited by hand.
        // Categories in sort order; Entries grouped by Category (in Category order), then in sort order.
        [SerializeField, HideInInspector]
        private List<Category> _categories = new();

        [SerializeField, HideInInspector]
        private List<Entry> _entries = new();

        [SerializeField, HideInInspector]
        private TagCollection _tagLibrary;

        // Built on first lookup, reset when the index changes (IDs never change, titles can: no title cache).
        [NonSerialized]
        private Dictionary<string, Entry> _entriesById;

        [NonSerialized]
        private Dictionary<string, Category> _categoriesById;
        #endregion

        #region Properties
        public int FormatVersion => _formatVersion;

        public string Title => _title;

        public string Glyph => string.IsNullOrWhiteSpace(_glyph)
            ? DefaultGlyph
            : _glyph;

        // Stored form with [[id|Title]] links; games use GetPlainText / FormatDescription.
        public string Description => _description;

        public IReadOnlyList<ProjectLink> Links => _links;

        public IReadOnlyList<Category> Categories => _categories;

        public IReadOnlyList<Entry> Entries => _entries;

        public TagCollection TagLibrary => _tagLibrary;
        #endregion

        #region Public API
        // Entries of one Category, in their order. Allocates a new list: cache it when used every frame.
        public List<Entry> GetEntries(Category category)
        {
            var result = new List<Entry>();

            if (category == null)
                return result;

            foreach (Entry entry in _entries)
            {
                if (entry != null &&
                    entry.Category == category)
                {
                    result.Add(entry);
                }
            }

            return result;
        }

        public Entry FindEntry(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            _entriesById ??= BuildLookup(_entries);

            return _entriesById.TryGetValue(
                id,
                out Entry entry)
                ? entry
                : null;
        }

        public Category FindCategory(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            _categoriesById ??= BuildLookup(_categories);

            return _categoriesById.TryGetValue(
                id,
                out Category category)
                ? category
                : null;
        }

        // First Entry with this title (case-insensitive); titles are only unique within a Category,
        // so pass the Category to narrow the search.
        public Entry FindEntryByTitle(
            string title,
            Category category = null)
        {
            if (string.IsNullOrEmpty(title))
                return null;

            foreach (Entry entry in _entries)
            {
                if (entry == null)
                    continue;

                if (category != null &&
                    entry.Category != category)
                {
                    continue;
                }

                if (string.Equals(
                        entry.Title,
                        title,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return entry;
                }
            }

            return null;
        }

        // Category with this title (case-insensitive).
        public Category FindCategoryByTitle(string title)
        {
            if (string.IsNullOrEmpty(title))
                return null;

            foreach (Category category in _categories)
            {
                if (category != null &&
                    string.Equals(
                        category.Title,
                        title,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return category;
                }
            }

            return null;
        }

        // Field with this name (case-insensitive) used by any Category. Resolve names once (e.g. in Awake)
        // and keep the reference: a renamed Field then shows up as null right away.
        public Field FindField(string title)
        {
            if (string.IsNullOrEmpty(title))
                return null;

            foreach (Category category in _categories)
            {
                if (category == null)
                    continue;

                foreach (Field field in category.Fields)
                {
                    if (field != null &&
                        string.Equals(
                            field.Title,
                            title,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return field;
                    }
                }
            }

            return null;
        }
        #endregion

        #region Descriptions
        // Descriptions (Project, Category, Entry) are stored with [[id|Title]] links; these methods turn them into
        // text for games. Links resolve like in the editor: by ID, or by title for links that were never pinned.
        // Markdown emphasis (**bold** / __bold__, *italic* / _italic_) is removed, except by GetRichText.

        // "The Wolf hunts at night." (links replaced by the current Entry title)
        public string GetPlainText(string description)
        {
            return FormatDescription(
                description,
                part => part.Text);
        }

        // Plain text as is, links via formatLink, e.g. TextMeshPro links:
        //   part => $"<link={part.Entry?.Id}><u>{part.Text}</u></link>"
        public string FormatDescription(
            string description,
            Func<DescriptionPart, string> formatLink)
        {
            return Format(
                description,
                formatLink,
                richText: false);
        }

        // Like FormatDescription, with bold and italic as rich text tags (<b>, <i>) for TextMeshPro and uGUI.
        public string GetRichText(
            string description,
            Func<DescriptionPart, string> formatLink = null)
        {
            return Format(
                description,
                formatLink,
                richText: true);
        }

        public List<DescriptionPart> ParseDescription(string description)
        {
            var parts = new List<DescriptionPart>();

            foreach (DescriptionRun run in DescriptionEmphasisParser.Parse(description))
            {
                foreach (DescriptionSegment segment in DescriptionLinkParser.Parse(run.Text))
                {
                    if (!segment.IsLink)
                    {
                        parts.Add(
                            new DescriptionPart(
                                segment.Text,
                                false,
                                null,
                                run.IsBold,
                                run.IsItalic));

                        continue;
                    }

                    Entry entry =
                        ResolveLink(
                            segment.Text);

                    parts.Add(
                        new DescriptionPart(
                            entry != null
                                ? entry.Title
                                : DescriptionLinkParser.GetLabel(segment.Text),
                            true,
                            entry,
                            run.IsBold,
                            run.IsItalic));
                }
            }

            return parts;
        }
        #endregion

        #region Tags
        // Tags of an Entry, in the Entry's order (unknown tag IDs are skipped).
        public List<TagDefinition> GetTags(Entry entry)
        {
            var result = new List<TagDefinition>();

            if (entry == null ||
                _tagLibrary == null)
            {
                return result;
            }

            foreach (string tagId in entry.TagIds)
            {
                TagDefinition tag =
                    _tagLibrary.FindById(
                        tagId);

                if (tag != null)
                    result.Add(tag);
            }

            return result;
        }

        // Tag by title (case-insensitive); an unknown title logs a warning in the editor/development builds.
        public bool HasTag(
            Entry entry,
            string tagTitle)
        {
            if (entry == null)
                return false;

            TagDefinition tag =
                FindTag(
                    tagTitle);

            return tag != null &&
                   entry.HasTag(tag.Id);
        }

        // All Entries with this tag, in browser order.
        public List<Entry> GetEntriesWithTag(string tagTitle)
        {
            var result = new List<Entry>();

            TagDefinition tag =
                FindTag(
                    tagTitle);

            if (tag == null)
                return result;

            foreach (Entry entry in _entries)
            {
                if (entry != null &&
                    entry.HasTag(tag.Id))
                {
                    result.Add(entry);
                }
            }

            return result;
        }
        #endregion

        #region Editor API
        internal void SetFormatVersion(int formatVersion)
        {
            _formatVersion = formatVersion;
        }

        internal void SetTitle(string title)
        {
            _title = title ?? string.Empty;
        }

        internal void SetGlyph(string glyph)
        {
            _glyph = glyph ?? string.Empty;
        }

        internal void SetDescription(string description)
        {
            _description = description ?? string.Empty;
        }

        internal void AddLink(ProjectLink link)
        {
            if (link == null ||
                _links.Contains(link))
            {
                return;
            }

            _links.Add(link);
        }

        internal void RemoveLink(ProjectLink link)
        {
            _links.Remove(link);
        }

        // insertionIndex is the gap index (0 = before the first link, Count = after the last one).
        internal bool MoveLink(
            ProjectLink link,
            int insertionIndex)
        {
            int oldIndex = _links.IndexOf(link);

            if (oldIndex < 0)
                return false;

            if (insertionIndex == oldIndex ||
                insertionIndex == oldIndex + 1)
            {
                return false;
            }

            _links.RemoveAt(oldIndex);

            if (insertionIndex > oldIndex)
                insertionIndex--;

            insertionIndex = Mathf.Clamp(
                insertionIndex,
                0,
                _links.Count);

            _links.Insert(insertionIndex, link);

            return true;
        }

        // Replaces the runtime index. Returns false (nothing changed) when it is already up to date.
        internal bool SetIndex(
            IReadOnlyList<Category> categories,
            IReadOnlyList<Entry> entries,
            TagCollection tagLibrary)
        {
            if (_tagLibrary == tagLibrary &&
                SequenceEquals(_categories, categories) &&
                SequenceEquals(_entries, entries))
            {
                return false;
            }

            _categories = new List<Category>(categories);
            _entries = new List<Entry>(entries);
            _tagLibrary = tagLibrary;

            _entriesById = null;
            _categoriesById = null;

            return true;
        }
        #endregion

        #region Helpers
        private string Format(
            string description,
            Func<DescriptionPart, string> formatLink,
            bool richText)
        {
            if (!DescriptionLinkParser.ContainsLinks(description) &&
                !DescriptionEmphasisParser.ContainsMarkers(description))
            {
                return description ?? string.Empty;
            }

            var builder = new System.Text.StringBuilder();

            foreach (DescriptionPart part in ParseDescription(description))
            {
                bool bold = richText && part.IsBold;
                bool italic = richText && part.IsItalic;

                if (bold)
                    builder.Append("<b>");

                if (italic)
                    builder.Append("<i>");

                builder.Append(
                    part.IsLink && formatLink != null
                        ? formatLink(part)
                        : part.Text);

                if (italic)
                    builder.Append("</i>");

                if (bold)
                    builder.Append("</b>");
            }

            return builder.ToString();
        }

        // Same rules as the editor (DescriptionLinkResolver): pinned links strictly by ID,
        // other links by title (optionally "Category/Title").
        private Entry ResolveLink(string target)
        {
            if (DescriptionLinkParser.TryParsePinned(
                    target,
                    out string id,
                    out _))
            {
                return FindEntry(id);
            }

            DescriptionLinkParser.SplitTitleTarget(
                target,
                out string categoryTitle,
                out string entryTitle);

            if (categoryTitle == null)
                return FindEntryByTitle(entryTitle);

            Category category =
                FindCategoryByTitle(
                    categoryTitle);

            return category != null
                ? FindEntryByTitle(entryTitle, category)
                : null;
        }

        private TagDefinition FindTag(string tagTitle)
        {
            if (_tagLibrary == null)
            {
                LoreRuntimeLog.WarnOnce(
                    "The Lore Project has no Tag library.",
                    this);

                return null;
            }

            TagDefinition tag =
                _tagLibrary.FindByTitle(
                    tagTitle);

            if (tag == null)
            {
                LoreRuntimeLog.WarnOnce(
                    $"Tag '{tagTitle}' does not exist.",
                    _tagLibrary);
            }

            return tag;
        }

        private static Dictionary<string, T> BuildLookup<T>(List<T> assets)
            where T : LoreAsset
        {
            var lookup = new Dictionary<string, T>();

            foreach (T asset in assets)
            {
                if (asset != null &&
                    !string.IsNullOrEmpty(asset.Id))
                {
                    lookup.TryAdd(
                        asset.Id,
                        asset);
                }
            }

            return lookup;
        }

        private static bool SequenceEquals<T>(
            List<T> current,
            IReadOnlyList<T> other)
            where T : UnityEngine.Object
        {
            if (current.Count != other.Count)
                return false;

            for (int i = 0; i < current.Count; i++)
            {
                if (current[i] != other[i])
                    return false;
            }

            return true;
        }
        #endregion
    }
}
