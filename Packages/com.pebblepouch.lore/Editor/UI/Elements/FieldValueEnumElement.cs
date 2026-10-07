using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Enum value: a dropdown with "—" (not set) and the Field's options in their order. Stores the option ID, so
    // renaming an option in the Field Library keeps the value.
    public sealed class FieldValueEnumElement : FieldValueElement
    {
        #region Draw
        protected override void DrawContent(
            Rect rect)
        {
            EntryAssignmentContext.CheckInteraction(
                rect);

            Field field = _fieldValue.Field;

            var labels =
                new GUIContent[field.EnumOptions.Count + 1];

            labels[0] =
                new GUIContent(
                    LoreText.Inspector.EnumNotSet);

            for (int i = 0; i < field.EnumOptions.Count; i++)
            {
                labels[i + 1] =
                    new GUIContent(
                        GetMenuLabel(
                            field.EnumOptions[i]));
            }

            // 0 = not set (also for a removed option).
            int selected =
                field.GetEnumOptionIndex(
                    _fieldValue.EnumValue) + 1;

            EditorGUI.BeginChangeCheck();

            int index =
                EditorGUI.Popup(
                    rect,
                    selected,
                    labels,
                    InspectorStyle.FieldValueEnum);

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

            if (EditorGUI.EndChangeCheck())
            {
                RecordChange();

                _fieldValue.SetEnum(
                    index > 0
                        ? field.EnumOptions[index - 1]?.Id
                        : string.Empty);

                Save();
            }
        }
        #endregion

        #region Helpers
        // Unity menus treat "/" as a submenu separator: show a look-alike instead.
        internal static string GetMenuLabel(EnumOption option)
        {
            return option != null
                ? option.Label.Replace("/", "\u2215")
                : string.Empty;
        }
        #endregion
    }
}
