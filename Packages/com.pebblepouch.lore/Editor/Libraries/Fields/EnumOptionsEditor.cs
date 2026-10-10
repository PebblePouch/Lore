using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Options of an Enum / Enums Field, below its card while it is edited: one row per option (drag handle, name, remove),
    // then "Add Option". Works on the card's edit state; FieldService.UpdateField applies the list on save.
    public sealed class EnumOptionsEditor
    {
        #region Fields
        private readonly DragListInteraction<EnumOptionDraft> _drag;

        // Focus the name of a newly added option on the next Repaint.
        private EnumOptionDraft _focusRequest;
        #endregion

        #region Constructor
        public EnumOptionsEditor()
        {
            _drag =
                new DragListInteraction<EnumOptionDraft>(
                    LoreRepaintService.Repaint);
        }
        #endregion

        #region Public API
        public static float GetHeight(FieldCardEditState state)
        {
            if (state == null ||
                !FieldValueUtility.HasEnumOptions(state.Type))
            {
                return 0f;
            }

            // Option rows (or the empty hint), then the add row.
            int rows =
                Mathf.Max(1, state.EnumOptions.Count) + 1;

            return LibraryStyle.OptionsVerticalPadding * 2f
                   + rows * LibraryStyle.OptionRowHeight
                   + (rows - 1) * LibraryStyle.OptionRowSpacing;
        }

        public void Draw(
            Rect rect,
            FieldCardEditState state)
        {
            List<EnumOptionDraft> options = state.EnumOptions;

            // Aligned with the card's title column.
            float x =
                rect.x
                + LibraryStyle.ColumnMargin
                + LibraryStyle.GlyphWidth
                + LibraryStyle.ColumnSpacing
                - LibraryStyle.OptionHandleWidth;

            float right =
                rect.xMax
                - LibraryStyle.ColumnSpacing
                - LibraryStyle.MenuWidth;

            float y =
                rect.y
                + LibraryStyle.OptionsVerticalPadding;

            _drag.BeginFrame();

            if (options.Count == 0)
            {
                GUI.Label(
                    new Rect(x + LibraryStyle.OptionHandleWidth, y, right - x, LibraryStyle.OptionRowHeight),
                    LoreText.Library.EnumOptionsEmpty,
                    LibraryStyle.Placeholder);

                y += LibraryStyle.OptionRowHeight + LibraryStyle.OptionRowSpacing;
            }

            EnumOptionDraft removed = null;

            for (int i = 0; i < options.Count; i++)
            {
                EnumOptionDraft option = options[i];

                Rect rowRect =
                    new(x, y, right - x, LibraryStyle.OptionRowHeight);

                if (DrawRow(rowRect, option, i, state))
                    removed = option;

                y += LibraryStyle.OptionRowHeight + LibraryStyle.OptionRowSpacing;
            }

            ButtonDrawer.Add(
                new Rect(
                    x + LibraryStyle.OptionHandleWidth,
                    y,
                    LibraryStyle.OptionAddButtonWidth,
                    LibraryStyle.OptionRowHeight),
                () =>
                {
                    var option =
                        new EnumOptionDraft(
                            null,
                            string.Empty);

                    options.Add(option);

                    _focusRequest = option;
                    state.Feedback = string.Empty;

                    LoreRepaintService.Repaint();
                },
                ButtonDrawer.DisplayMode.GlyphName,
                LibraryStyle.Button);

            // After the loop: the list must not change while its rows are drawn.
            if (removed != null)
            {
                options.Remove(removed);

                state.Feedback = string.Empty;

                LoreRepaintService.Repaint();
            }

            _drag.HandleEvents(
                (option, insertionIndex) =>
                    Move(options, option, insertionIndex));

            _drag.DrawDropLine(DropLineDrawer.Draw);
        }
        #endregion

        #region Draw
        // Returns true when the remove button was clicked.
        private bool DrawRow(
            Rect rect,
            EnumOptionDraft option,
            int index,
            FieldCardEditState state)
        {
            Rect handleRect =
                new(rect.x, rect.y, LibraryStyle.OptionHandleWidth, rect.height);

            Rect removeRect =
                new(rect.xMax - LibraryStyle.ButtonWidth, rect.y, LibraryStyle.ButtonWidth, rect.height);

            Rect nameRect =
                new(
                    handleRect.xMax,
                    rect.y,
                    removeRect.x - LibraryStyle.ColumnSpacing - handleRect.xMax,
                    rect.height);

            GUI.Label(handleRect, InspectorStyle.FieldHandleIcon, LibraryStyle.OptionHandle);

            EditorGUIUtility.AddCursorRect(handleRect, MouseCursor.Pan);

            _drag.Register(option, handleRect, rect);

            string controlName =
                LibraryStyle.OptionControlNamePrefix + index;

            // Enter in an option name saves the card, like Enter on its title (the Library only commits on Enter
            // while no text field is focused).
            if (Event.current.type == EventType.KeyDown &&
                (Event.current.keyCode == KeyCode.Return ||
                 Event.current.keyCode == KeyCode.KeypadEnter) &&
                GUI.GetNameOfFocusedControl() == controlName)
            {
                Event.current.Use();

                GUI.FocusControl(null);

                LibraryEditingService.CommitEditing();
            }

            GUI.SetNextControlName(controlName);

            EditorGUI.BeginChangeCheck();

            string label =
                EditorGUI.TextField(
                    nameRect,
                    option.Label,
                    LibraryStyle.OptionField);

            if (EditorGUI.EndChangeCheck())
            {
                option.Label = label;
                state.Feedback = string.Empty;
            }

            // Clicks inside the focused name keep the caret (window keyboard focus handling).
            LoreEditService.ReportEditor(
                controlName,
                nameRect,
                () => GUI.GetNameOfFocusedControl() == controlName);

            if (_focusRequest == option &&
                Event.current.type == EventType.Repaint)
            {
                _focusRequest = null;

                EditorGUI.FocusTextInControl(controlName);
            }

            bool remove = false;

            ButtonDrawer.Remove(
                removeRect,
                () => remove = true,
                ButtonDrawer.DisplayMode.Glyph,
                LibraryStyle.Button);

            return remove;
        }
        #endregion

        #region Helpers
        // insertionIndex is the gap index (0 = before the first option, Count = after the last one).
        private static void Move(
            List<EnumOptionDraft> options,
            EnumOptionDraft option,
            int insertionIndex)
        {
            int oldIndex = options.IndexOf(option);

            if (oldIndex < 0 ||
                insertionIndex == oldIndex ||
                insertionIndex == oldIndex + 1)
            {
                return;
            }

            options.RemoveAt(oldIndex);

            if (insertionIndex > oldIndex)
                insertionIndex--;

            options.Insert(
                Mathf.Clamp(insertionIndex, 0, options.Count),
                option);

            LoreRepaintService.Repaint();
        }
        #endregion
    }
}
