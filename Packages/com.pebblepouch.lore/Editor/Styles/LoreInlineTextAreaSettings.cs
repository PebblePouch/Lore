using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class LoreInlineTextAreaSettings
    {
        public GUIStyle ViewerContent;
        public GUIStyle PlaceholderContent;
        public GUIStyle EditorContent;

        // View mode: fixed height (scrolls). Edit mode: grows with the text up to MaxEditorHeight (then scrolls).
        public float ViewportHeight;
        public float MaxEditorHeight;

        public float HorizontalInset;
        public float VerticalInset;

        public string ControlName;

        public string Tooltip;
    }
}