using System;

using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class LoreMenuItem
    {
        #region Properties
        public GUIContent Content { get; }
        public string Glyph { get; }
        public Action Action { get; }
        public bool Destructive { get; }
        public bool SeparatorBefore { get; }
        public bool Selected { get; }
        #endregion

        #region Constructor
        public LoreMenuItem(
            string label,
            Action action,
            bool destructive = false,
            bool separatorBefore = false,
            bool selected = false)
            : this(
                new GUIContent(label),
                action,
                destructive,
                separatorBefore,
                selected)
        {
        }

        public LoreMenuItem(
            GUIContent content,
            Action action,
            bool destructive = false,
            bool separatorBefore = false,
            bool selected = false)
        {
            Content = content;
            Glyph = string.Empty;
            Action = action;
            Destructive = destructive;
            SeparatorBefore = separatorBefore;
            Selected = selected;
        }

        // Text item with a glyph in the glyph column (like the ButtonDefinition items).
        public LoreMenuItem(
            string glyph,
            string label,
            Action action,
            bool destructive = false,
            bool separatorBefore = false)
        {
            Glyph = glyph ?? string.Empty;
            Content = new GUIContent(label);
            Action = action;
            Destructive = destructive;
            SeparatorBefore = separatorBefore;
            Selected = false;
        }

        public LoreMenuItem(
            ButtonDefinition definition,
            Action action,
            bool destructive = false,
            bool separatorBefore = false)
        {
            Glyph = definition?.Glyph ?? string.Empty;
            Content =
                new GUIContent(
                    definition?.Name ?? string.Empty,
                    definition?.Tooltip ?? string.Empty);
            Action = action;
            Destructive = destructive;
            SeparatorBefore = separatorBefore;
            Selected = false;
        }
        #endregion
    }
}