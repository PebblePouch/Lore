using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Fits single-line text into a width by shortening it with an ellipsis ("https://github.com/pebb…").
    public static class LoreTextFit
    {
        #region Fields
        private const string Ellipsis = "…";
        #endregion

        #region Public API
        public static string Ellipsize(
            string text,
            GUIStyle style,
            float width)
        {
            if (string.IsNullOrEmpty(text) ||
                width <= 0f ||
                style.CalcSize(new GUIContent(text)).x <= width)
            {
                return text ?? string.Empty;
            }

            // Longest prefix that still fits together with the ellipsis (binary search).
            int low = 0;
            int high = text.Length;

            while (low < high)
            {
                int middle = (low + high + 1) / 2;

                float middleWidth =
                    style.CalcSize(
                        new GUIContent(
                            text.Substring(0, middle) + Ellipsis))
                    .x;

                if (middleWidth <= width)
                    low = middle;
                else
                    high = middle - 1;
            }

            return text.Substring(0, low) + Ellipsis;
        }
        #endregion
    }
}
