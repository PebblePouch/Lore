using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class ColorLibraryItem : LibraryItem
    {
        #region Constructor
        public ColorLibraryItem(ColorDefinition definition)
        {
            Definition = definition;
            Color = definition.Color;
            Title = definition.Title;
            Category = definition.Category;

            Id = ColorUtility.ToHtmlStringRGBA(Color);
        }
        #endregion

        #region Properties
        public ColorDefinition Definition { get; }
        public override string Id { get; }
        public Color Color { get; }
        public override string Title { get; }
        public string Category { get; }
        public override string SearchText => $"{Title} {Category}";
        #endregion
    }
}