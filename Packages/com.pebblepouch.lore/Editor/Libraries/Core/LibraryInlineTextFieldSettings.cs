using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class LibraryInlineTextFieldSettings
    {
        #region Properties
        public GUIStyle Viewer { get; set; }

        public GUIStyle Editor { get; set; }

        public GUIStyle Placeholder { get; set; }

        public float Height { get; set; }

        public float HorizontalInset { get; set; }

        public float VerticalInset { get; set; }
        #endregion
    }
}