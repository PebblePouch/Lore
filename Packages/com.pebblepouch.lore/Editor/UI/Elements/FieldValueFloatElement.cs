using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class FieldValueFloatElement : FieldValueElement
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

            float value = 
                EditorGUI.FloatField(
                    rect,
                    _fieldValue.FloatValue,
                    InspectorStyle.FieldValueNumber);

            if (EditorGUI.EndChangeCheck())
            {
                RecordChange();

                _fieldValue.SetFloat(
                    value);

                Save();
            }
        }
        #endregion
    }
}