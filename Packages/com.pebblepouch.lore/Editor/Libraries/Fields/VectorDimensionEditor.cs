using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Vector Fields, below their card while it is edited: "Dimension  [3D (x, y, z) ▾]". Works on the card's edit
    // state; FieldService.UpdateField applies it on save. Never clears values: a 2D Field keeps the stored z.
    public static class VectorDimensionEditor
    {
        #region Public API
        public static float GetHeight(FieldCardEditState state)
        {
            return state != null &&
                   state.Type == FieldType.Vector
                ? LibraryStyle.OptionsVerticalPadding * 2f + LibraryStyle.OptionRowHeight
                : 0f;
        }

        public static void Draw(
            Rect rect,
            FieldCardEditState state)
        {
            // Aligned with the card's title column (like the Enum options and allowed Categories).
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
                LoreText.Library.VectorDimensionLabel,
                LibraryStyle.AllowedCategoriesLabel);

            if (GUI.Button(buttonRect, GetLabel(state.VectorDimension), LibraryStyle.OptionField))
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
        private static string GetLabel(int dimension) =>
            dimension == 2
                ? LoreText.Library.VectorDimension2
                : LoreText.Library.VectorDimension3;

        private static void ShowMenu(
            Rect buttonRect,
            FieldCardEditState state)
        {
            var menu = new GenericMenu();

            foreach (int dimension in new[] { 2, 3 })
            {
                int captured = dimension;

                menu.AddItem(
                    new GUIContent(GetLabel(dimension)),
                    state.VectorDimension == dimension,
                    () =>
                    {
                        state.VectorDimension = captured;
                        state.Feedback = string.Empty;

                        LoreRepaintService.Repaint();
                    });
            }

            menu.DropDown(buttonRect);
        }
        #endregion
    }
}
