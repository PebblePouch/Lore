using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Entry Tags section: colored tag chips with a remove button, wrapped into rows.
    // The remove button reuses FieldButtonColumnWidth / FieldButton. No field initializer here references
    // another InspectorStyle part (initialization order across partial files is undefined).
    public static partial class InspectorStyle
    {
        #region Layout
        public const float TagHeight = 20f;
        public const float TagHorizontalPadding = 4f;
        public const float TagSpacing = 8f;
        public const float TagRowSpacing = 8f;

        public const float TagButtonHorizontalPadding = 1f;
        public const float TagButtonVerticalOffset = 1f;
        #endregion

        #region Content
        // White text, tinted per tag with GUI.contentColor (contrast color of the tag color),
        // so one cached style serves all tags.
        public static readonly GUIStyle TagTitle = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleLeft,
            clipping = TextClipping.Clip,

            normal = { textColor = Color.white },
            hover = { textColor = Color.white },
            active = { textColor = Color.white },
        };
        #endregion
    }
}
