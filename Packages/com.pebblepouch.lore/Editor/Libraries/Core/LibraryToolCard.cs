using System;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static class LibraryToolCard
    {
        #region Public API
        public static void Draw(
            string visual,
            string title,
            Action action,
            bool active = false)
        {
            Rect layoutRect =
                GUILayoutUtility.GetRect(
                    0f,
                    LibraryStyle.CardHeight
                        + LibraryStyle.CardSpacing,
                    GUILayout.ExpandWidth(true));

            Rect cardRect =
                new(
                    layoutRect.x,
                    layoutRect.y,
                    layoutRect.width,
                    LibraryStyle.CardHeight);

            Color background =
                active
                    ? LibraryStyle.CardActive
                    : LoreHover.Contains(cardRect)
                        ? LibraryStyle.CardHover
                        : LibraryStyle.CardNormal;

            EditorGUI.DrawRect(
                cardRect,
                background);

            LibraryCardLayout layout =
                new(
                    cardRect,
                    false,
                    false);

            GUI.Label(
                layout.Visual,
                visual,
                LibraryStyle.Glyph);

            GUI.Label(
                layout.Title,
                title,
                LibraryStyle.Title);

            EditorGUIUtility.AddCursorRect(
                cardRect,
                MouseCursor.Link);

            if (!active &&
                EditorInteraction.Click(
                    cardRect))
            {
                action?.Invoke();
            }
        }
        #endregion
    }
}