using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static class LoreColorUtility
    {
        #region Fields
        // Perceived luminance weights (ITU-R BT.601) and the threshold above which a background counts as light.
        private const float RedWeight = 0.299f;
        private const float GreenWeight = 0.587f;
        private const float BlueWeight = 0.114f;

        private const float LightBackgroundThreshold = 0.6f;
        #endregion

        #region Public API
        // The Category's color, or a neutral gray when there is no Category.
        public static Color GetCategoryColor(Category category)
        {
            if (category == null)
                return LorePalette.NoCategory;

            return category.Color;
        }

        // Black text on light backgrounds, white text on dark ones (e.g. Category-colored cards and tags).
        public static Color GetContrastTextColor(Color background)
        {
            float luminance =
                background.r * RedWeight +
                background.g * GreenWeight +
                background.b * BlueWeight;

            return luminance > LightBackgroundThreshold
                ? Color.black
                : Color.white;
        }
        #endregion
    }
}
