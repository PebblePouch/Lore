using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class FieldValueGameObjectElement : FieldValueElement
    {
        #region Draw
        protected override void DrawContent(
            Rect rect)
        {
            EntryAssignmentContext.CheckInteraction(
                rect);

            EditorGUI.BeginChangeCheck();

            GameObject value =
                (GameObject)EditorGUI.ObjectField(
                    rect,
                    GUIContent.none,
                    _fieldValue.GameObjectValue,
                    typeof(GameObject),
                    true);

            if (EditorGUI.EndChangeCheck())
            {
                RecordChange();

                _fieldValue.SetGameObject(
                    value);

                Save();
            }
        }
        #endregion
    }
}