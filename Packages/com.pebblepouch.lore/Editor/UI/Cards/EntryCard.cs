using System;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static class EntryCard
    {
        #region Public API
        public static Rect Draw(
            Entry entry,
            bool highlighted,
            Action<Entry> selectEntry,
            Action<Entry, Rect, Rect> registerDrag)
        {
            Rect cardRect = EditorGUILayout.BeginHorizontal();
            Rect dragRect = DrawHandle(entry.Category);

            EditorGUIUtility.AddCursorRect(
                dragRect,
                MouseCursor.Pan);


            Rect selectRect =
                DrawCard(
                    entry,
                    highlighted);

            GUILayout.Space(BrowserStyle.FoldoutColumnWidth);

            DrawGlyph(entry);

            DrawContent(entry);

            GUILayout.FlexibleSpace();

            DrawCardEnd();

            EditorGUILayout.EndHorizontal();

            registerDrag?.Invoke(
                entry,
                dragRect,
                cardRect);

            HandleSelection(
                selectRect,
                entry,
                selectEntry);

            EditorGUIUtility.AddCursorRect(
                selectRect,
                MouseCursor.Link);

            return selectRect;
        }
        #endregion

        #region Draw
        private static Rect DrawHandle(Category category)
        {
            GUILayout.BeginVertical(
                GUILayout.Width(BrowserStyle.HandleColumnWidth),
                GUILayout.Height(BrowserStyle.CardHeight));

            GUILayout.FlexibleSpace();

            Rect rect = GUILayoutUtility.GetRect(
                BrowserStyle.HandleColumnWidth,
                BrowserStyle.CardHeight,
                GUILayout.Width(BrowserStyle.HandleColumnWidth),
                GUILayout.Height(BrowserStyle.CardHeight));

            EditorGUI.DrawRect(rect, LoreColorUtility.GetCategoryColor(category));

            GUI.Label(rect, BrowserStyle.BurgerMenuString, BrowserStyle.HandleContent);

            GUILayout.FlexibleSpace();

            GUILayout.EndVertical();

            return rect;
        }

        private static Rect DrawCard(
            Entry entry,
            bool highlighted)
        {
            Rect rect = EditorGUILayout.BeginVertical(
                BrowserStyle.Card,
                GUILayout.Height(BrowserStyle.CardHeight));

            EditorGUI.DrawRect(rect, GetCardColor(rect, entry, highlighted));

            EditorGUILayout.BeginHorizontal();

            return rect;
        }

        private static void DrawGlyph(Entry entry)
        {
            GUILayout.BeginVertical(
                GUILayout.Width(BrowserStyle.GlyphColumnWidth),
                GUILayout.Height(BrowserStyle.CardHeight));

            GUILayout.FlexibleSpace();

            GUILayout.Label(
                string.IsNullOrEmpty(entry.Glyph)
                    ? BrowserStyle.NoGlyphString
                    : entry.Glyph,
                BrowserStyle.Glyph,
                GUILayout.Width(BrowserStyle.GlyphColumnWidth));

            GUILayout.FlexibleSpace();

            GUILayout.EndVertical();
        }

        private static void DrawContent(Entry entry)
        {
            GUILayout.BeginVertical(
                GUILayout.ExpandWidth(true),
                GUILayout.Height(BrowserStyle.CardHeight));

            GUILayout.FlexibleSpace();

            GUILayout.Label(
                string.IsNullOrEmpty(entry.Title)
                    ? entry.name
                    : entry.Title,
                BrowserStyle.Title);

            GUILayout.Label(entry.Subtitle, BrowserStyle.Subtitle);

            GUILayout.FlexibleSpace();

            GUILayout.EndVertical();
        }

        private static void DrawCardEnd()
        {
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }
        #endregion

        #region Commands
        private static void HandleSelection(
            Rect rect,
            Entry entry,
            Action<Entry> selectEntry)
        {
            if (!EditorInteraction.Click(
                    rect))
            {
                return;
            }

            selectEntry?.Invoke(
                entry);
        }
        /*
        private static void HandleSelection(
            Rect rect,
            Entry entry)
        {
            if (!EditorInteraction.Click(
                    rect))
            {
                return;
            }

            SelectionService.SelectEntry(
                entry);

            LoreKeyboardFocus.SetOwner(
                LoreKeyboardOwner.Content);

            LoreRepaintService.Repaint();
        }
        */
        #endregion

        #region Helpers
        private static Color GetCardColor(
            Rect rect,
            Entry entry,
            bool highlighted)
        {
            if (SelectionService.CurrentEntry == entry)
                return BrowserStyle.EntryCardActive;

            if (highlighted ||
                LoreHover.Contains(rect))
            {
                return BrowserStyle.EntryCardHover;
            }

            return BrowserStyle.EntryCardNormal;
        }
        #endregion
    }
}