using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class FieldValueBooleanElement : FieldValueElement
    {
        #region Draw
        protected override void DrawContent(
            Rect rect)
        {
            EntryAssignmentContext.CheckInteraction(
                rect);

            EditorGUI.BeginChangeCheck();

            bool value =
                EditorGUI.Toggle(
                    rect,
                    GUIContent.none,
                    _fieldValue.BoolValue,
                    InspectorStyle.FieldValueToggle);

            if (EditorGUI.EndChangeCheck())
            {
                RecordChange();

                _fieldValue.SetBoolean(
                    value);

                Save();
            }
        }
        #endregion
    }
}