using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class FieldValueIntegerElement : FieldValueElement
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

            int value = 
                EditorGUI.IntField(
                    rect,
                    _fieldValue.IntValue,
                    InspectorStyle.FieldValueNumber);

            if (EditorGUI.EndChangeCheck())
            {
                RecordChange();

                _fieldValue.SetInteger(
                    value);

                Save();
            }
        }
        #endregion
    }
}