using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace PebblePouch.Lore.Editor
{
    // Shared Entry search rules (title, subtitle, glyph, tags; MatchesContent adds full-text), used by the Content browser
    // and the description link picker so both find the same Entries.
    public sealed class EntrySearchMatcher
    {
        #region Fields
        private readonly TagLibraryService _tagService = new();
        #endregion

        #region Public API
        public bool Matches(
            Entry entry,
            string search)
        {
            return MatchesText(entry.Title, search)
                || MatchesText(entry.Subtitle, search)
                || MatchesText(entry.Glyph, search)
                || MatchesTags(entry, search);
        }

        // Full-text (Content browser only): the description as shown (links as their title) and the field values
        // (text, URL, numbers, Enum labels, titles of referenced Entries, names of assets and scenes).
        public bool MatchesContent(
            Entry entry,
            string search)
        {
            return MatchesDescription(entry.Description, search)
                || MatchesFieldValues(entry, search);
        }

        public static bool MatchesText(string value, string search)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            return value
                .Contains(search, StringComparison.OrdinalIgnoreCase);
        }
        #endregion

        #region Helpers
        private static bool MatchesDescription(
            string description,
            string search)
        {
            if (string.IsNullOrEmpty(description))
                return false;

            // Markdown emphasis markers are not shown, so "**big** wolf" matches "big wolf".
            description =
                DescriptionEmphasisParser.StripMarkers(
                    description);

            if (!DescriptionLinkParser.ContainsLinks(description))
                return MatchesText(description, search);

            // Stored links are "[[id|Title]]": match what the user sees (the current Entry title), never the ID.
            var shown = new System.Text.StringBuilder(description.Length);

            foreach (DescriptionSegment segment in DescriptionLinkParser.Parse(description))
            {
                shown.Append(
                    segment.IsLink
                        ? DescriptionLinkResolver.Resolve(segment.Text)?.Title
                          ?? DescriptionLinkResolver.GetLabel(segment.Text)
                        : segment.Text);
            }

            return MatchesText(shown.ToString(), search);
        }

        private static bool MatchesFieldValues(
            Entry entry,
            string search)
        {
            foreach (FieldValue fieldValue in entry.Fields)
            {
                if (fieldValue?.Field == null)
                    continue;

                foreach (string text in GetSearchableTexts(fieldValue))
                {
                    if (MatchesText(text, search))
                        return true;
                }
            }

            return false;
        }

        // What a value looks like to the user (Booleans are left out: "true" is not something to search for).
        private static IEnumerable<string> GetSearchableTexts(FieldValue fieldValue)
        {
            switch (fieldValue.Field.Type)
            {
                case FieldType.Text:
                case FieldType.Url:
                    yield return fieldValue.StringValue;
                    break;

                case FieldType.Integer:
                    yield return fieldValue.IntValue.ToString(CultureInfo.InvariantCulture);
                    break;

                case FieldType.Float:
                    yield return fieldValue.FloatValue.ToString(CultureInfo.InvariantCulture);
                    break;

                case FieldType.Enum:
                    yield return fieldValue.Field.FindEnumOption(fieldValue.EnumValue)?.Label;
                    break;

                case FieldType.Entry:
                case FieldType.Entries:
                    foreach (Entry referenced in FieldValueUtility.GetReferencedEntries(fieldValue))
                        yield return referenced.Title;
                    break;

                case FieldType.GameObject:
                    yield return fieldValue.GameObjectValue != null ? fieldValue.GameObjectValue.name : null;
                    break;

                case FieldType.Sprite:
                    yield return fieldValue.SpriteValue != null ? fieldValue.SpriteValue.name : null;
                    break;

                case FieldType.ScriptableObject:
                    yield return fieldValue.ObjectValue != null ? fieldValue.ObjectValue.name : null;
                    break;

                case FieldType.Audio:
                    yield return fieldValue.AudioClipValue != null ? fieldValue.AudioClipValue.name : null;
                    break;

                case FieldType.Scene:
                    yield return Path.GetFileNameWithoutExtension(fieldValue.ScenePath);
                    break;
            }
        }

        private bool MatchesTags(
            Entry entry,
            string search)
        {
            // Asked per search (cached by the service): the Tag library can be created or replaced meanwhile.
            TagCollection tagCollection =
                _tagService.GetCollection();

            if (tagCollection == null)
                return false;

            foreach (string tagId in
                     entry.TagIds)
            {
                TagDefinition tag =
                    tagCollection.FindById(
                        tagId);

                if (tag == null)
                    continue;

                if (MatchesText(
                        tag.Title,
                        search))
                {
                    return true;
                }
            }

            return false;
        }
        #endregion
    }
}
