using System;
using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static class SearchCard
    {
        #region Public API
        public static void Draw(
            ISearchModel model,
            string placeholder)
        {
            Draw(
                model,
                placeholder,
                null,
                false,
                null);
        }

        public static void Draw(
            ISearchModel model,
            string placeholder,
            string controlName,
            bool requestFocus,
            Action activated = null,
            Action drawTrailing = null)
        {
            Rect rect = EditorGUILayout.BeginVertical(
                BrowserStyle.SearchCard,
                GUILayout.Height(BrowserStyle.SearchCardHeight));

            EditorGUI.DrawRect(
                rect,
                BrowserStyle.SearchCardBackground);

            GUILayout.BeginHorizontal();

            Rect searchRect = GUILayoutUtility.GetRect(
                GUIContent.none,
                BrowserStyle.SearchField,
                GUILayout.Height(
                    BrowserStyle.SearchFieldHeight));

            searchRect.x +=
                BrowserStyle.SearchFieldInsetLeft;

            searchRect.y +=
                BrowserStyle.SearchFieldInsetTop;

            searchRect.width -=
                BrowserStyle.SearchFieldInsetRight;

            if (!string.IsNullOrEmpty(controlName))
            {
                GUI.SetNextControlName(
                    controlName);
            }

            if (Event.current.type == EventType.MouseDown 
                && searchRect.Contains(Event.current.mousePosition))
            {
                activated?.Invoke();
            }

            string newSearch = EditorGUI.TextField(
                searchRect,
                model.Search,
                BrowserStyle.SearchField);

            if (newSearch != model.Search)
            {
                model.Search = newSearch;
                model.Refresh();
            }

            if (requestFocus &&
                Event.current.type == EventType.Repaint &&
                !string.IsNullOrEmpty(controlName))
            {
                EditorGUI.FocusTextInControl(
                    controlName);
            }

            if (string.IsNullOrWhiteSpace(model.Search))
            {
                Rect placeholderRect = searchRect;

                placeholderRect.x +=
                    BrowserStyle.SearchPlaceholderInsetLeft;

                GUI.Label(
                    placeholderRect,
                    placeholder,
                    BrowserStyle.SearchPlaceholderLabel);
            }

            if (!string.IsNullOrWhiteSpace(model.Search))
            {
                ButtonDrawer.Clear(
                    () =>
                    {
                        model.Search = string.Empty;
                        model.Refresh();

                        GUI.FocusControl(null);
                        EditorGUI.FocusTextInControl(null);
                    },
                    ButtonDrawer.DisplayMode.Glyph,
                    BrowserStyle.SearchActionButton);
            }

            // Extra controls on the same row (e.g. Tabular Editing bulk actions); the search field gets narrower.
            drawTrailing?.Invoke();

            GUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }
        #endregion
    }
}