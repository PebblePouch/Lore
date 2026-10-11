using System.Collections.Generic;

using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class ColorLibraryCollection : ScriptableObject
    {
        #region Fields
        // Raise whenever the default colors change: older assets then offer a Library update (LibraryUpdateService).
        public const int CatalogVersion = 1;

        [SerializeField, HideInInspector]
        private List<ColorDefinition> _colors = new();

        // Catalog version the asset was generated or last updated with (0 = before catalog versions existed).
        [SerializeField, HideInInspector]
        private int _catalogVersion;
        #endregion

        #region Properties
        public IReadOnlyList<ColorDefinition> Colors => _colors;

        public bool IsCatalogOutdated => _catalogVersion < CatalogVersion;
        #endregion

        #region Catalog
        public void MarkCatalogCurrent()
        {
            _catalogVersion = CatalogVersion;
        }

        // Adds the default colors this asset does not have yet (matched by color and title) at their default
        // position; existing colors stay unchanged, colors that are not (or no longer) defaults move to the end.
        // Returns how many were added.
        public int AppendMissingDefaults()
        {
            ColorLibraryCollection defaults = CreateInstance<ColorLibraryCollection>();

            try
            {
                defaults.GenerateDefaultCollection();

                Dictionary<string, ColorDefinition> existing = new();

                foreach (ColorDefinition color in _colors)
                    existing.TryAdd(GetKey(color), color);

                List<ColorDefinition> merged = new();
                HashSet<string> defaultKeys = new();
                int added = 0;

                foreach (ColorDefinition color in defaults._colors)
                {
                    string key = GetKey(color);

                    if (!defaultKeys.Add(key))
                        continue;

                    if (existing.TryGetValue(key, out ColorDefinition own))
                    {
                        merged.Add(own);
                    }
                    else
                    {
                        merged.Add(color);
                        added++;
                    }
                }

                foreach (ColorDefinition color in _colors)
                {
                    if (!defaultKeys.Contains(GetKey(color)))
                        merged.Add(color);
                }

                _colors = merged;

                MarkCatalogCurrent();

                return added;
            }
            finally
            {
                DestroyImmediate(defaults);
            }
        }

        private static string GetKey(ColorDefinition color)
        {
            return ColorUtility.ToHtmlStringRGBA(color.Color) + "|" + color.Title;
        }
        #endregion

        #region Helpers
        private void Add(
            Color color,
            string title,
            string category)
        {
            _colors.Add(
                new ColorDefinition(
                    color,
                    title,
                    category));
        }
        #endregion

#if UNITY_EDITOR
        #region Generation
        public void GenerateDefaultCollection()
        {
            _colors.Clear();

            MarkCatalogCurrent();

            AddPalette("Red", Color.red);
            AddPalette("Orange", new Color(1.0f, 0.5f, 0.0f));
            AddPalette("Yellow", Color.yellow);
            AddPalette("Green", Color.green);
            AddPalette("Cyan", Color.cyan);
            AddPalette("Blue", Color.blue);
            AddPalette("Purple", new Color(0.5f, 0.0f, 1.0f));
            AddPalette("Magenta", Color.magenta);
            AddPalette("Pink", new Color(1.0f, 0.4f, 0.7f));
            AddPalette("White", Color.white);
        }

        private void AddPalette(
            string paletteName,
            Color baseColor)
        {
            const int steps = 10;
            const float minThreshold = 0.35f;
            const float maxThreshold = 0.90f;

            bool startsBelowFullBrightness =
                baseColor == Color.yellow ||
                baseColor == Color.cyan ||
                baseColor == Color.white;

            float startBrightness =
                startsBelowFullBrightness
                    ? maxThreshold
                    : 1f;

            for (int i = 0; i < steps; i++)
            {
                float t = i / (steps - 1f);

                float brightness = Mathf.Lerp(
                    startBrightness,
                    minThreshold,
                    t);

                Color color = baseColor * brightness;
                color.a = 1f;

                string hex = $"#{ColorUtility.ToHtmlStringRGB(color)}";

                Add(
                    color,
                    hex,
                    paletteName);
            }
        }
        #endregion
#endif
    }
}