using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class FieldValueColorElement : FieldValueElement
    {
        #region Draw
        protected override void DrawContent(
            Rect rect)
        {
            EntryAssignmentContext.CheckInteraction(
                rect);

            EditorGUI.BeginChangeCheck();

            Color value =
                EditorGUI.ColorField(
                    rect,
                    GUIContent.none,
                    _fieldValue.ColorValue,
                    true,
                    true,
                    false);

            if (EditorGUI.EndChangeCheck())
            {
                // The color picker changes the value continuously while dragging: one undo step.
                RecordContinuousChange();

                _fieldValue.SetColor(
                    value);

                Save();
            }
        }
        #endregion
    }
}
