using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Vector2 or Vector3 (Field.VectorDimension) as X/Y(/Z) number fields (see VectorFieldDrawer).
    public sealed class FieldValueVectorElement : FieldValueElement
    {
        #region Draw
        protected override void DrawContent(
            Rect rect)
        {
            EntryAssignmentContext.CheckInteraction(
                rect);

            Vector3 value =
                _fieldValue.VectorValue;

            int component =
                VectorFieldDrawer.Draw(
                    rect,
                    _fieldValue.Field.VectorDimension,
                    value,
                    GetControlName(),
                    false,
                    null,
                    out float number);

            if (component < 0)
                return;

            // Typing and dragging in this value become one undo step.
            RecordContinuousChange();

            value[component] = number;

            _fieldValue.SetVector(
                value);

            Save();
        }
        #endregion
    }
}
