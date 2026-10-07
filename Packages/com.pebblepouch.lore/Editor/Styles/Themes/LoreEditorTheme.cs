using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static class LoreEditorTheme
    {
        // Editor/Resources/DefaultTheme.asset: Resources under an Editor folder load in the editor but stay out of builds.
        private const string DefaultThemePath = "DefaultTheme";
        private static LoreTheme _current;

        public static LoreTheme Current
        {
            get
            {
                if (_current == null)
                {
                    _current = Resources.Load<LoreTheme>(DefaultThemePath);
                }

                return _current;
            }
        }
    }
}