using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class CategoryGlyphElement
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
                InspectorStyle.GlyphPicker.Width,
                InspectorStyle.GlyphPicker.Height,
                GUILayout.Width(
                    InspectorStyle.GlyphPicker.Width),
                GUILayout.Height(
                    InspectorStyle.GlyphPicker.Height));

            GUI.Label(
                rect,
                string.IsNullOrWhiteSpace(_category.Glyph)
                    ? "?"
                    : _category.Glyph,
                InspectorStyle.GlyphPicker.ViewerContent);

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

            GlyphAssignmentContext.BeginAssignment(
                glyph =>
                {
                    if (_categoryService.UpdateGlyph(
                            category,
                            glyph))
                    {
                        LoreRepaintService.Repaint();
                    }
                });

            LibraryService.OpenGlyphLibraryWithSelection?.Invoke(category.Glyph);

            Event.current.Use();
        }
        #endregion
    }
}