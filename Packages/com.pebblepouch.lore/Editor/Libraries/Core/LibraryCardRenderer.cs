using System;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class LibraryCardRenderer<T> : ILibraryCardRenderer<T> where T : LibraryItem
    {
        #region Fields
        private readonly Action<Rect, T> _drawBody;

        private readonly Func<T, bool> _isInactive;
        private readonly Func<T, string> _getEditingId;

        private readonly Func<T, float> _getAdditionalHeight;
        private readonly Action<Rect, T> _drawAdditionalContent;
        #endregion

        #region Constructor
        public LibraryCardRenderer(
            Action<Rect, T> drawBody,
            Func<T, bool> isInactive = null,
            Func<T, string> getEditingId = null,
            Func<T, float> getAdditionalHeight = null,
            Action<Rect, T> drawAdditionalContent = null)
        {
            _drawBody = drawBody;

            _isInactive = isInactive;
            _getEditingId = getEditingId;

            _getAdditionalHeight = getAdditionalHeight;
            _drawAdditionalContent = drawAdditionalContent;
        }
        #endregion

        #region Public API
        public BrowserItemDrawResult Draw(
            T item,
            bool highlighted,
            bool pickable)
        {
            string editingId =
                _getEditingId?.Invoke(
                    item);

            bool editing =
                !string.IsNullOrEmpty(
                    editingId) &&
                LibraryEditingService.IsEditing(
                    editingId);

            bool inactive =
                _isInactive?.Invoke(
                    item)
                ?? false;

            float availableWidth =
                EditorGUIUtility.currentViewWidth;

            float additionalHeight =
                editing
                    ? _getAdditionalHeight?.Invoke(
                          item)
                      ?? 0f
                    : 0f;

            Rect layoutRect =
                GUILayoutUtility.GetRect(
                    0f,
                    LibraryStyle.CardHeight
                        + additionalHeight
                        + LibraryStyle.CardSpacing,
                    GUILayout.ExpandWidth(true));

            Rect cardRect =
                new(
                    layoutRect.x,
                    layoutRect.y,
                    layoutRect.width,
                    LibraryStyle.CardHeight);

            Rect fullRect =
                new(
                    layoutRect.x,
                    layoutRect.y,
                    layoutRect.width,
                    LibraryStyle.CardHeight
                        + additionalHeight);

            DrawBackground(
                fullRect,
                editing,
                highlighted,
                inactive);

            _drawBody?.Invoke(
                cardRect,
                item);

            if (additionalHeight > 0f)
            {
                Rect additionalRect =
                    new(
                        fullRect.x,
                        cardRect.yMax,
                        fullRect.width,
                        additionalHeight);

                _drawAdditionalContent?.Invoke(
                    additionalRect,
                    item);
            }

            if (!editing && !inactive && pickable)
            {
                EditorGUIUtility.AddCursorRect(cardRect, MouseCursor.Link);
            }

            if (editing)
            {
                LibraryEditingService.HandleOutsideClick<T>(
                    editingId,
                    fullRect);

                return new BrowserItemDrawResult(
                    fullRect,
                    false);
            }

            if (inactive)
            {
                return new BrowserItemDrawResult(
                    fullRect,
                    false);
            }

            if (!pickable)
            {
                return new BrowserItemDrawResult(
                    fullRect,
                    false);
            }

            bool clicked = EditorInteraction.Click(cardRect);

            return new BrowserItemDrawResult(
                fullRect,
                clicked);
        }
        #endregion

        #region Draw
        private static void DrawBackground(
            Rect rect,
            bool editing,
            bool highlighted,
            bool inactive)
        {
            Color color;

            if (editing)
            {
                color = LibraryStyle.CardActive;
            }
            else if (inactive 
                && highlighted)
            {
                color = LibraryStyle.CardHighlightInactive;
            }
            else if (inactive)
            {
                color = LibraryStyle.CardInactive;
            }
            else if (highlighted 
                || LoreHover.Contains(rect))
            {
                color = LibraryStyle.CardHover;
            }
            else
            {
                color = LibraryStyle.CardNormal;
            }

            EditorGUI.DrawRect(
                rect,
                color);
        }
        #endregion
    }
}