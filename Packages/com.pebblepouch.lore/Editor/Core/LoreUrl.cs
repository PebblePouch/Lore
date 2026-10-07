using System;

using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Shared URL handling for Project links and URL field values.
    public static class LoreUrl
    {
        #region Public API
        // Adds https:// when no scheme is given ("github.com/..." works); keeps http(s), mailto: and file: links.
        public static string Normalize(string url)
        {
            url = url?.Trim() ?? string.Empty;

            if (url.Length == 0 ||
                url.Contains("://") ||
                url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
            {
                return url;
            }

            return "https://" + url;
        }

        public static bool CanOpen(string url)
        {
            return Normalize(url).Length > 0;
        }

        public static void Open(string url)
        {
            string normalized = Normalize(url);

            if (normalized.Length == 0)
                return;

            Application.OpenURL(normalized);
        }
        #endregion
    }
}
