using System;
using System.Linq;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static class CategoryCard
    {
        #region Fields
        private const int MaxFieldGlyphPreviewCount = 10;
        #endregion

        #region Public API
        public static bool Draw(
            CategoryEntryGroup group,
            bool expanded,
            bool highlighted,
            Action createEntry,
            Action<Category> selectCategory,
            Action<Category, Rect, Rect> registerDrag,
            out Rect navigationRect)
        {
            Rect cardRect = EditorGUILayout.BeginHorizontal();
            Rect dragRect = DrawHandle(group.Category);

            EditorGUIUtility.AddCursorRect(
                dragRect,
                MouseCursor.Pan);

            Rect foldoutRect = DrawFoldout(group.Category, expanded);
            Rect selectRect =
                DrawCard(
                    group.Category,
                    highlighted);

            DrawGlyph(group.Category);

            DrawContent(group);

            GUILayout.FlexibleSpace();

            DrawButton(createEntry);

            DrawCardEnd();

            EditorGUILayout.EndHorizontal();

            registerDrag?.Invoke(
                group.Category,
                dragRect,
                cardRect);

            expanded = HandleFoldout(
                foldoutRect,
                expanded);

            HandleSelection(
                selectRect,
                foldoutRect,
                group.Category,
                selectCategory);

            EditorGUIUtility.AddCursorRect(
                selectRect,
                MouseCursor.Link);

            EditorGUIUtility.AddCursorRect(
                foldoutRect,
                MouseCursor.Link);

            navigationRect =
                selectRect;

            return expanded;
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

        private static Rect DrawFoldout(Category category, bool expanded)
        {
            GUILayout.BeginVertical(
                GUILayout.Width(BrowserStyle.FoldoutColumnWidth),
                GUILayout.Height(BrowserStyle.CardHeight));

            GUILayout.FlexibleSpace();

            Rect rect = GUILayoutUtility.GetRect(
                BrowserStyle.FoldoutColumnWidth,
                BrowserStyle.CardHeight,
                GUILayout.Width(BrowserStyle.FoldoutColumnWidth),
                GUILayout.Height(BrowserStyle.CardHeight));

            EditorGUI.DrawRect(rect, LoreColorUtility.GetCategoryColor(category));

            string label = expanded 
                ? BrowserStyle.FoldoutExpandedString 
                : BrowserStyle.FoldoutCollapsedString;

            GUI.Label(
                rect, 
                label,
                BrowserStyle.Foldout);

            GUILayout.FlexibleSpace();

            GUILayout.EndVertical();

            return rect;
        }

        private static Rect DrawCard(
            Category category,
            bool highlighted)
        {
            Rect rect = EditorGUILayout.BeginVertical(
                BrowserStyle.Card,
                GUILayout.Height(BrowserStyle.CardHeight));

            EditorGUI.DrawRect(
                rect,
                GetCardColor(
                    rect,
                    category,
                    highlighted));

            EditorGUILayout.BeginHorizontal();

            return rect;
        }

        private static void DrawGlyph(Category category)
        {
            GUILayout.BeginVertical(
                GUILayout.Width(BrowserStyle.GlyphColumnWidth),
                GUILayout.Height(BrowserStyle.CardHeight));

            GUILayout.FlexibleSpace();

            GUILayout.Label(
                string.IsNullOrEmpty(category.Glyph)
                    ? BrowserStyle.NoGlyphString
                    : category.Glyph,
                BrowserStyle.Glyph,
                GUILayout.Width(BrowserStyle.GlyphColumnWidth));

            GUILayout.FlexibleSpace();

            GUILayout.EndVertical();
        }

        private static void DrawContent(CategoryEntryGroup group)
        {
            GUILayout.BeginVertical(
                GUILayout.ExpandWidth(true),
                GUILayout.Height(BrowserStyle.CardHeight));

            GUILayout.FlexibleSpace();

            GUILayout.Label(string.Format(LoreText.Common.ItemWithCount, group.Title, group.Count), BrowserStyle.Title);

            DrawFieldGlyphs(group.Category);

            GUILayout.FlexibleSpace();

            GUILayout.EndVertical();
        }

        private static void DrawFieldGlyphs(
    Category category)
        {
            if (category == null)
            {
                GUILayout.Label(
                    LoreText.Browser.NoFields,
                    BrowserStyle.Fields);

                return;
            }

            var fields =
                category.Fields
                    .Where(field => field != null)
                    .ToList();

            if (fields.Count == 0)
            {
                GUILayout.Label(
                    LoreText.Browser.NoFields,
                    BrowserStyle.Fields);

                return;
            }

            GUILayout.BeginHorizontal();

            foreach (Field field in
                     fields.Take(
                         MaxFieldGlyphPreviewCount))
            {
                string glyph =
                    field.Glyph ?? string.Empty;

                GUIContent content =
                    new(
                        glyph,
                        string.Format(LoreText.Common.ItemWithCount, field.name, field.Type));

                float width =
                    BrowserStyle.Fields
                        .CalcSize(
                            content)
                        .x;

                GUILayout.Label(
                    content,
                    BrowserStyle.Fields,
                    GUILayout.Width(
                        width));
            }

            int remainingCount =
                fields.Count -
                MaxFieldGlyphPreviewCount;

            if (remainingCount > 0)
            {
                GUILayout.Label(
                    string.Format(LoreText.Browser.MoreCount, remainingCount),
                    BrowserStyle.Fields);
            }

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }

        private static void DrawButton(Action createEntry)
        {
            GUILayout.BeginVertical(
                GUILayout.Width(BrowserStyle.ButtonColumnWidth),
                GUILayout.Height(BrowserStyle.CardHeight));

            GUILayout.FlexibleSpace();

            ButtonDrawer.Add(createEntry, ButtonDrawer.DisplayMode.Glyph, BrowserStyle.Button);

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
        private static bool HandleFoldout(Rect rect, bool expanded)
        {
            if (EditorInteraction.Click(rect))
            {
                return !expanded;
            }

            return expanded;
        }
        
        private static void HandleSelection(
            Rect cardRect,
            Rect foldoutRect,
            Category category,
            Action<Category> selectCategory)
        {
            if (!EditorInteraction.Click(
                    cardRect))
            {
                return;
            }

            if (foldoutRect.Contains(
                    Event.current.mousePosition))
            {
                return;
            }

            selectCategory?.Invoke(
                category);
        }
        /*
        private static void HandleSelection(
            Rect cardRect,
            Rect foldoutRect,
            Category category)
        {
            if (!EditorInteraction.Click(cardRect))
            {
                return;
            }

            if (foldoutRect.Contains(Event.current.mousePosition))
            {
                return;
            }

            SelectionService.SelectCategory(category);

            LoreKeyboardFocus.SetOwner(LoreKeyboardOwner.Content);

            LoreRepaintService.Repaint();
        }
        */
        #endregion

        #region Helpers
        private static Color GetCardColor(
            Rect rect,
            Category category,
            bool highlighted)
        {
            if (SelectionService.CurrentCategory == category)
                return BrowserStyle.CategoryCardActive;

            if (highlighted ||
                LoreHover.Contains(rect))
            {
                return BrowserStyle.CategoryCardHover;
            }

            return BrowserStyle.CategoryCardNormal;
        }
        #endregion
    }
}