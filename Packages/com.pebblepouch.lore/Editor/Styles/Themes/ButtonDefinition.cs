using System;

using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    [Serializable]
    public class ButtonDefinition
    {
        public string Name;
        public string Glyph;
        public string Tooltip;

        public GUIContent GetContent(ButtonDrawer.DisplayMode mode)
        {
            switch (mode)
            {
                case ButtonDrawer.DisplayMode.Glyph:
                    return new($"{Glyph}", Name);
                case ButtonDrawer.DisplayMode.GlyphName:
                    return new($"{Glyph} {Name}", Tooltip);
                case ButtonDrawer.DisplayMode.Name:
                    return new($"{Name}", Tooltip);
                case ButtonDrawer.DisplayMode.Tooltip:
                    return new($"{Tooltip}", Name);
                default:
                    return new($"?", $"ActionDisplayMode undefined");
            }
        }
    }
}