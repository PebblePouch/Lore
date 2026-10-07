using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class CategoryColorElement
    {
        #region Fields
        private readonly CategoryService _categoryService = new();

        private Category _category;
        #endregion

        #region Public API
        public void SetCategory(
            Category category)
        {
            _category = category;
        }

        public void Draw()
        {
            if (_category == null)
                return;

            DrawContent();
        }
        #endregion

        #region Draw
        private void DrawContent()
        {
            Rect rect = GUILayoutUtility.GetRect(
                InspectorStyle.ColorPicker.Width,
                InspectorStyle.ColorPicker.Height,
                GUILayout.Width(
                    InspectorStyle.ColorPicker.Width),
                GUILayout.Height(
                    InspectorStyle.ColorPicker.Height));

            EditorGUI.DrawRect(
                rect,
                _category.Color);

            GUI.Label(
                rect,
                new GUIContent(
                    string.Empty,
                    InspectorStyle.ColorPicker.Tooltip));

            EditorGUIUtility.AddCursorRect(
                rect,
                MouseCursor.Link);

            if (Event.current.type != EventType.MouseDown ||
                !rect.Contains(Event.current.mousePosition))
            {
                return;
            }

            Category category =
                _category;

            ColorAssignmentContext.BeginAssignment(
                color =>
                {
                    if (_categoryService.UpdateColor(
                            category,
                            color))
                    {
                        LoreRepaintService.Repaint();
                    }
                });

            LibraryService.OpenColorLibraryWithSelection?.Invoke(category.Color);

            Event.current.Use();
        }
        #endregion
    }
}