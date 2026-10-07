using System;
using System.Globalization;

using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Header element (Project, Category, Entry): "Created: Oct 3, 2026 (14:05:33)" / "Modified: ...",
    // right-aligned between title/subtitle and menu. Same position in every header: fixed column height
    // with calculated top spacing (flexible spaces do not center reliably inside the Inspector scroll view),
    // and headers without a menu reserve its width (see ReserveMenuWidth).
    public sealed class TimestampsElement
    {
        #region Fields
        private LoreAsset _asset;
        #endregion

        #region Public API
        public void SetAsset(LoreAsset asset)
        {
            _asset = asset;
        }

        public void Draw()
        {
            if (_asset == null)
                return;

            var created =
                new GUIContent(
                    string.Format(
                        LoreText.Inspector.CreatedTimestamp,
                        FormatDate(_asset.Created)));

            var modified =
                new GUIContent(
                    string.Format(
                        LoreText.Inspector.ModifiedTimestamp,
                        FormatDate(_asset.Modified)));

            GUIStyle style = InspectorStyle.TimestampsContent;

            float lineHeight =
                style.CalcSize(created).y;

            float topSpacing =
                Mathf.Max(
                    0f,
                    (InspectorStyle.HeaderHeight - lineHeight * 2f) * 0.5f);

            // Exactly the text width: with only ExpandWidth(false) the labels can still stretch the column
            // and leave the title next to it just a small piece of the header.
            float width =
                Mathf.Ceil(
                    Mathf.Max(
                        style.CalcSize(created).x,
                        style.CalcSize(modified).x));

            GUILayout.Space(
                InspectorStyle.TimestampsHorizontalSpacing);

            GUILayout.BeginVertical(
                GUILayout.Width(width),
                GUILayout.Height(
                    InspectorStyle.HeaderHeight));

            GUILayout.Space(topSpacing);

            GUILayout.Label(
                created,
                style,
                GUILayout.Width(width),
                GUILayout.Height(lineHeight));

            GUILayout.Label(
                modified,
                style,
                GUILayout.Width(width),
                GUILayout.Height(lineHeight));

            GUILayout.EndVertical();

            GUILayout.Space(
                InspectorStyle.TimestampsHorizontalSpacing);
        }

        // For headers without a menu (Project): keeps the timestamps where they are in the other headers.
        public static void ReserveMenuWidth()
        {
            GUILayout.Space(
                InspectorStyle.MenuWidth);
        }
        #endregion

        #region Helpers
        // Stored in UTC, shown in local time. Invariant culture: English month names like all other texts
        // (to be revisited with localization).
        private static string FormatDate(DateTime utc)
        {
            return utc
                .ToLocalTime()
                .ToString(
                    LoreText.Inspector.TimestampDateFormat,
                    CultureInfo.InvariantCulture);
        }
        #endregion
    }
}
