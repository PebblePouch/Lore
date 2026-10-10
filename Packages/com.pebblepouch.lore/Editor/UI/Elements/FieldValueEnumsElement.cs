using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Enums value: looks like the Enum dropdown and shows the selected options in option order ("Fire, Ice", "—" when
    // none). The menu ticks/unticks one option per click (it closes after each, like Unity's own multi-select
    // menus). Stores option IDs, so renaming an option keeps the value.
    public sealed class FieldValueEnumsElement : FieldValueElement
    {
        #region Draw
        protected override void DrawContent(
            Rect rect)
        {
            EntryAssignmentContext.CheckInteraction(
                rect);

            string text =
                GetText(
                    _fieldValue);

            if (GUI.Button(
                    rect,
                    new GUIContent(text, text),
                    InspectorStyle.FieldValueEnum))
            {
                ShowMenu(
                    rect,
                    _entry,
                    _fieldValue);
            }

            if (Event.current.type == EventType.Repaint)
            {
                GUI.Label(
                    new Rect(
                        rect.xMax - InspectorStyle.FieldValueEnumArrowWidth,
                        rect.y,
                        InspectorStyle.FieldValueEnumArrowWidth,
                        rect.height),
                    InspectorStyle.FieldValueEnumArrow,
                    InspectorStyle.FieldValueEnumArrowContent);
            }
        }
        #endregion

        #region Helpers
        // Selected option labels in option order, "—" when none (removed options are left out).
        internal static string GetText(FieldValue fieldValue)
        {
            List<string> labels =
                GetSelectedLabels(
                    fieldValue);

            return labels.Count > 0
                ? string.Join(LoreText.Inspector.EnumsSeparator, labels)
                : LoreText.Inspector.EnumNotSet;
        }

        internal static List<string> GetSelectedLabels(FieldValue fieldValue)
        {
            var labels = new List<string>();

            Field field = fieldValue?.Field;

            if (field == null)
                return labels;

            foreach (EnumOption option in field.EnumOptions)
            {
                if (option != null &&
                    Contains(fieldValue.EnumValues, option.Id))
                {
                    labels.Add(option.Label);
                }
            }

            return labels;
        }

        internal static bool Contains(
            IReadOnlyList<string> optionIds,
            string optionId)
        {
            foreach (string candidate in optionIds)
            {
                if (candidate == optionId)
                    return true;
            }

            return false;
        }

        // One element draws all rows: the menu works on the Entry and value it was opened for.
        private static void ShowMenu(
            Rect rect,
            Entry entry,
            FieldValue fieldValue)
        {
            var menu = new GenericMenu();

            foreach (EnumOption option in fieldValue.Field.EnumOptions)
            {
                if (option == null)
                    continue;

                string optionId = option.Id;

                bool selected =
                    Contains(
                        fieldValue.EnumValues,
                        optionId);

                menu.AddItem(
                    new GUIContent(FieldValueEnumElement.GetMenuLabel(option)),
                    selected,
                    () =>
                    {
                        if (entry != null)
                        {
                            LoreUndo.Record(
                                entry,
                                LoreText.Undo.EditFieldValue);
                        }

                        if (selected)
                            fieldValue.RemoveEnumValue(optionId);
                        else
                            fieldValue.AddEnumValue(optionId);

                        if (entry != null)
                            LoreChangeTracker.MarkModified(entry);

                        LoreRepaintService.Repaint();
                    });
            }

            menu.DropDown(rect);
        }
        #endregion
    }
}
