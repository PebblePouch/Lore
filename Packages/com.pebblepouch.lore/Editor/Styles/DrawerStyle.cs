using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Property drawers for Lore types in the user's own Inspectors (Entry fields). They live in Unity's Inspector,
    // not in the Lore window: Unity's own styles and colors that work in light and dark skin, not LorePalette.
    public static class DrawerStyle
    {
        #region Value
        // Object-field look; rich text for the muted detail part ("🐺 Wolf  <color>(Characters)</color>").
        public static readonly GUIStyle Value = new(EditorStyles.objectField)
        {
            richText = true,
        };

        // Before MissingValue: static initializers run in order.
        public static readonly Color MissingColor = new(0.9f, 0.3f, 0.3f);

        public static readonly GUIStyle MissingValue = new(Value)
        {
            normal = { textColor = MissingColor },
            hover = { textColor = MissingColor },
            focused = { textColor = MissingColor },
        };

        // Muted detail in the value text (Category, Field type), readable in light and dark skin.
        public const string DetailColor = "#8C8C8C";

        // Technical: Unity's built-in object field picker button style (falls back to a mini button).
        public const string PickerButtonStyleName = "ObjectFieldButton";
        public const float PickerButtonWidth = 19f;
        #endregion

        #region Hint
        // Filter problems (e.g. unknown Category) below the field.
        public static readonly GUIStyle Hint = new(EditorStyles.miniLabel)
        {
            fontStyle = FontStyle.Italic,
            wordWrap = true,
        };

        public const float HintSpacing = 2f;
        #endregion

        #region Dropdown
        public const float DropdownWidth = 260f;
        public const float DropdownHeight = 320f;
        #endregion
    }
}
