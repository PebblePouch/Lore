using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class TagCardEditState
    {
        #region Properties
        public string Title { get; set; }
        public Color Color { get; set; }
        public string Feedback { get; set; }
        #endregion

        #region Constructor
        public TagCardEditState(
            TagDefinition tag)
        {
            Title =
                tag.Title;

            Color =
                tag.Color;
        }
        #endregion
    }
}