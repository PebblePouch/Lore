using System.Linq;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Entry / Entries Fields, below their card while it is edited: "Categories  [Characters, Monsters ▾]".
    // The button opens a menu to tick Categories ("All Categories" clears the restriction). Works on the card's
    // edit state; FieldService.UpdateField applies it on save. Never clears values: it only limits picking.
    public static class AllowedCategoriesEditor
    {
        #region Public API
        public static float GetHeight(FieldCardEditState state)
        {
            return state != null &&
                   FieldValueUtility.IsEntryReference(state.Type)
                ? LibraryStyle.OptionsVerticalPadding * 2f + LibraryStyle.OptionRowHeight
                : 0f;
        }

        public static void Draw(
            Rect rect,
            FieldCardEditState state)
        {
            // Aligned with the card's title column (like the Enum options).
            float x =
                rect.x
                + LibraryStyle.ColumnMargin
                + LibraryStyle.GlyphWidth
                + LibraryStyle.ColumnSpacing;

            float right =
                rect.xMax
                - LibraryStyle.ColumnSpacing
                - LibraryStyle.MenuWidth;

            float y =
                rect.y
                + LibraryStyle.OptionsVerticalPadding;

            Rect labelRect =
                new(x, y, LibraryStyle.AllowedCategoriesLabelWidth, LibraryStyle.OptionRowHeight);

            Rect buttonRect =
                new(labelRect.xMax, y, Mathf.Max(0f, right - labelRect.xMax), LibraryStyle.OptionRowHeight);

            GUI.Label(
                labelRect,
                LoreText.Library.AllowedCategoriesLabel,
                LibraryStyle.AllowedCategoriesLabel);

            if (GUI.Button(buttonRect, GetButtonText(state), LibraryStyle.OptionField))
                ShowMenu(buttonRect, state);

            if (Event.current.type == EventType.Repaint)
            {
                GUI.Label(
                    new Rect(
                        buttonRect.xMax - InspectorStyle.FieldValueEnumArrowWidth,
                        buttonRect.y,
                        InspectorStyle.FieldValueEnumArrowWidth,
                        buttonRect.height),
                    InspectorStyle.FieldValueEnumArrow,
                    InspectorStyle.FieldValueEnumArrowContent);
            }
        }
        #endregion

        #region Helpers
        private static string GetButtonText(FieldCardEditState state)
        {
            string[] titles =
                state.AllowedCategories
                    .Where(category => category != null)
                    .Select(category => category.Title)
                    .ToArray();

            return titles.Length == 0
                ? LoreText.Library.AllowedCategoriesAll
                : LoreText.Common.JoinList(titles);
        }

        // Tick/untick per Category (the menu closes after each choice, like Unity's own multi-select menus).
        private static void ShowMenu(
            Rect buttonRect,
            FieldCardEditState state)
        {
            var menu = new GenericMenu();

            menu.AddItem(
                new GUIContent(LoreText.Library.AllowedCategoriesAll),
                state.AllowedCategories.Count == 0,
                () =>
                {
                    state.AllowedCategories.Clear();

                    LoreRepaintService.Repaint();
                });

            menu.AddSeparator(string.Empty);

            foreach (Category category in LoreRepository.Instance.Categories)
            {
                if (category == null)
                    continue;

                Category captured = category;

                menu.AddItem(
                    new GUIContent(
                        string.Format(
                            LoreText.Drawers.GlyphTitle,
                            category.Glyph,
                            category.Title)),
                    state.AllowedCategories.Contains(category),
                    () =>
                    {
                        if (!state.AllowedCategories.Remove(captured))
                            state.AllowedCategories.Add(captured);

                        LoreRepaintService.Repaint();
                    });
            }

            menu.DropDown(buttonRect);
        }
        #endregion
    }
}
