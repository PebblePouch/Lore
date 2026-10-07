using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class EntryGlyphElement
    {
        #region Fields
        private readonly EntryService _entryService = new();

        private Entry _entry;
        #endregion

        #region Public API
        public void SetEntry(
            Entry entry)
        {
            _entry = entry;
        }

        public void Draw()
        {
            if (_entry == null)
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
                string.IsNullOrWhiteSpace(_entry.Glyph)
                    ? "?"
                    : _entry.Glyph,
                InspectorStyle.GlyphPicker.ViewerContent);

            EditorGUIUtility.AddCursorRect(
                rect,
                MouseCursor.Link);

            if (Event.current.type != EventType.MouseDown ||
                !rect.Contains(Event.current.mousePosition))
            {
                return;
            }

            Entry entry =
                _entry;

            GlyphAssignmentContext.BeginAssignment(
                glyph =>
                {
                    _entryService.UpdateGlyph(
                        entry,
                        glyph);
                });

            LibraryService.OpenEntryGlyphLibrary?.Invoke(entry);

            Event.current.Use();
        }
        #endregion
    }
}