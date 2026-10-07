using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class TagLibraryItem : LibraryItem
    {
        #region Constructor
        public TagLibraryItem(
            TagDefinition tag,
            bool isInEntry)
        {
            Tag = tag;

            Id = tag.Id;
            EditingId = $"Tag:{tag.Id}";

            IsInEntry = isInEntry;
        }
        #endregion

        #region Properties
        public TagDefinition Tag { get; }

        public override string Id { get; }

        public string EditingId { get; }

        public override string Title =>
            Tag.Title;

        public Color Color =>
            Tag.Color;

        public bool IsInEntry { get; }
        #endregion
    }
}