using System;

using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static class LibraryService
    {
        public static Action OpenEntryLibrary;
        public static Action OpenFieldLibrary;
        public static Action OpenTagLibrary;
        public static Action OpenGlyphLibrary;
        public static Action OpenColorLibrary;

        public static Action OpenNestedGlyphLibrary;
        public static Action OpenNestedColorLibrary;

        public static Action<string> OpenGlyphLibraryWithSelection;
        public static Action<Color> OpenColorLibraryWithSelection;
        public static Action<string> OpenNestedGlyphLibraryWithSelection;
        public static Action<Color> OpenNestedColorLibraryWithSelection;

        public static Action<Entry> OpenEntryGlyphLibrary;

        public static Action OpenUnusedFieldManager;

        // Field Library with this Field revealed and highlighted (e.g. double-click on a Field in an Inspector).
        public static Action<Field> RevealField;
        public static Action OpenUnusedTagManager;
        public static Action OpenUnusedGlyphManager;
        public static Action OpenUnusedColorManager;

        public static Action CloseLibrary;
    }
}