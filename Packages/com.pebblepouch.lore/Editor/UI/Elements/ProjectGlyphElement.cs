using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class ProjectGlyphElement
    {
        #region Fields
        private readonly ProjectService _projectService = new();
        #endregion

        #region Public API
        public void Draw()
        {
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

            string glyph =
                _projectService.GetGlyph();

            GUI.Label(
                rect,
                glyph,
                InspectorStyle.GlyphPicker.ViewerContent);

            EditorGUIUtility.AddCursorRect(
                rect,
                MouseCursor.Link);

            if (Event.current.type != EventType.MouseDown ||
                !rect.Contains(Event.current.mousePosition))
            {
                return;
            }

            GlyphAssignmentContext.BeginAssignment(
                selectedGlyph =>
                {
                    if (_projectService.UpdateGlyph(
                            selectedGlyph))
                    {
                        LoreRepaintService.Repaint();
                    }
                });

            LibraryService.OpenGlyphLibraryWithSelection?.Invoke(glyph);

            Event.current.Use();
        }
        #endregion
    }
}
