using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class FieldValueTextElement : FieldValueElement
    {
        #region Properties
        protected override bool HasTextInput => true;
        #endregion

        #region Draw
        protected override void DrawContent(
            Rect rect)
        {
            EntryAssignmentContext.CheckInteraction(
                rect);

            EditorGUI.BeginChangeCheck();

            string value =
                EditorGUI.TextField(
                    rect,
                    _fieldValue.StringValue,
                    InspectorStyle.FieldValueText);

            if (EditorGUI.EndChangeCheck())
            {
                RecordChange();

                _fieldValue.SetText(
                    value);

                Save();
            }
        }
        #endregion
    }
}