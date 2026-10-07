using System;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static class BrowserFilterCard
    {
        #region Public API
        public static void Draw(
            string label,
            Action clear)
        {
            if (string.IsNullOrWhiteSpace(label))
                return;

            Rect rect =
                GUILayoutUtility.GetRect(
                    0f,
                    BrowserStyle.FilterHeight,
                    GUILayout.ExpandWidth(true));

            EditorGUI.DrawRect(
                rect,
                BrowserStyle.FilterBackground);

            Rect clearRect =
                new(
                    rect.xMax
                        - BrowserStyle.FilterButtonSize
                        - BrowserStyle.FilterButtonRightSpace,
                    rect.y
                        + BrowserStyle.FilterButtonTopSpace,
                    BrowserStyle.FilterButtonSize,
                    rect.height);

            Rect labelRect =
                new(
                    rect.x
                        + BrowserStyle.FilterHorizontalPadding,
                    rect.y
                        + BrowserStyle.FilterLabelTopSpace,
                    Mathf.Max(
                        0f,
                        rect.width
                            - BrowserStyle.FilterHorizontalPadding
                            - clearRect.width),
                    rect.height);

            GUI.Label(
                labelRect,
                label,
                BrowserStyle.FilterLabel);

            ButtonDrawer.Clear(
                clearRect,
                clear,
                ButtonDrawer.DisplayMode.Glyph,
                BrowserStyle.FilterButton);
        }
        #endregion
    }
}