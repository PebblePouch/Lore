using System.Collections.Generic;

using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class ColorLibraryCollection : ScriptableObject
    {
        #region Fields
        [SerializeField, HideInInspector]
        private List<ColorDefinition> _colors = new();
        #endregion

        #region Properties
        public IReadOnlyList<ColorDefinition> Colors => _colors;
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