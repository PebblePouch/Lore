using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // URL value: text field (stored as typed) + open button. https:// is only added when opening.
    public sealed class FieldValueUrlElement : FieldValueElement
    {
        #region Properties
        protected override bool HasTextInput => true;
        #endregion

        #region Draw
        protected override void DrawContent(
            Rect rect)
        {
            GetRects(
                rect,
                out Rect textRect,
                out Rect buttonRect);

            EntryAssignmentContext.CheckInteraction(
                textRect);

            EditorGUI.BeginChangeCheck();

            string value =
                EditorGUI.TextField(
                    textRect,
                    _fieldValue.StringValue,
                    InspectorStyle.FieldValueText);

            if (EditorGUI.EndChangeCheck())
            {
                RecordChange();

                _fieldValue.SetText(
                    value);

                Save();
            }

            string url = _fieldValue.StringValue;

            using (new EditorGUI.DisabledScope(!LoreUrl.CanOpen(url)))
            {
                ButtonDrawer.Open(
                    buttonRect,
                    () => LoreUrl.Open(url),
                    ButtonDrawer.DisplayMode.Glyph,
                    InspectorStyle.FieldButton);
            }
        }
        #endregion

        #region Helpers
        private static void GetRects(
            Rect rect,
            out Rect textRect,
            out Rect buttonRect)
        {
            float buttonWidth =
                Mathf.Min(
                    InspectorStyle.FieldButtonColumnWidth,
                    rect.width);

            float textWidth =
                Mathf.Max(
                    0f,
                    rect.width -
                        buttonWidth -
                        InspectorStyle.FieldValueUrlButtonSpacing);

            textRect =
                new Rect(
                    rect.x,
                    rect.y,
                    textWidth,
                    rect.height);

            buttonRect =
                new Rect(
                    rect.xMax - buttonWidth,
                    rect.y,
                    buttonWidth,
                    rect.height);
        }
        #endregion
    }
}
