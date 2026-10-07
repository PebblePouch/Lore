using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // ScriptableObject value: an object field restricted to the Field's type (Unity's picker lists matching assets,
    // subclasses included). Without a usable type (none chosen, or the class was deleted/renamed) a hint replaces it.
    public sealed class FieldValueScriptableObjectElement : FieldValueElement
    {
        #region Draw
        protected override void DrawContent(
            Rect rect)
        {
            Field field = _fieldValue.Field;

            System.Type objectType = field.ObjectType;

            if (objectType == null)
            {
                DrawMissingType(
                    rect,
                    field);

                return;
            }

            EntryAssignmentContext.CheckInteraction(
                rect);

            EditorGUI.BeginChangeCheck();

            Object value =
                EditorGUI.ObjectField(
                    rect,
                    GUIContent.none,
                    _fieldValue.ObjectValue,
                    objectType,
                    false);

            if (EditorGUI.EndChangeCheck())
            {
                RecordChange();

                _fieldValue.SetObject(
                    value);

                Save();
            }
        }

        private static void DrawMissingType(
            Rect rect,
            Field field)
        {
            string text =
                string.IsNullOrEmpty(field.ObjectTypeName)
                    ? LoreText.Inspector.ObjectTypeNotSet
                    : string.Format(
                        LoreText.Inspector.ObjectTypeMissing,
                        ScriptableObjectTypeService.GetShortName(
                            field.ObjectTypeName));

            GUI.Label(
                rect,
                new GUIContent(
                    text,
                    LoreText.Inspector.ObjectTypeFixTooltip),
                InspectorStyle.MissingFieldFixStyle);
        }
        #endregion
    }
}
