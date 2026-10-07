using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class FieldValueSpriteElement : FieldValueElement
    {
        #region Draw
        protected override void DrawContent(
            Rect rect)
        {
            EntryAssignmentContext.CheckInteraction(
                rect);

            EditorGUI.BeginChangeCheck();

            Sprite value =
                (Sprite)EditorGUI.ObjectField(
                    rect,
                    GUIContent.none,
                    _fieldValue.SpriteValue,
                    typeof(Sprite),
                    true);

            if (EditorGUI.EndChangeCheck())
            {
                RecordChange();

                _fieldValue.SetSprite(
                    value);

                Save();
            }
        }
        #endregion
    }
}