using System;
using System.Collections.Generic;

using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static class LoreEditService
    {
        #region Fields
        private static readonly List<LoreInlineTextField> _fields = new();

        // The inline editor (text field or text area) currently being edited, with its screen rect.
        // Lets the window keep keyboard focus when a click lands inside that editor (caret placement)
        // while still clearing focus for every other click (prevents text leaking between fields).
        private static object _activeEditor;
        private static Func<bool> _isActiveEditorEditing;
        private static Rect _activeEditorScreenRect;
        #endregion

        #region Properties
        public static bool IsEditing
        {
            get
            {
                foreach (LoreInlineTextField field in _fields)
                {
                    if (field != null &&
                        field.IsEditing)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
        #endregion

        #region Public API
        public static void Register(LoreInlineTextField field)
        {
            if (!_fields.Contains(field))
            {
                _fields.Add(field);
            }
        }

        public static void Unregister(LoreInlineTextField field)
        {
            _fields.Remove(field);
        }

        // Undo/redo: ends all inline editing and releases keyboard focus, so no input keeps showing (or saves) its
        // buffered text over the restored value. Runs in the undo callback: the release applies to the Lore window
        // only (LoreGUIFocus.Release), not to the window the undo came from.
        public static void StopAllEditing()
        {
            foreach (LoreInlineTextField field in _fields)
                field?.StopEditingFromService();

            LoreGUIFocus.Release();
        }

        public static void RequestEditing(LoreInlineTextField field)
        {
            foreach (var editor in _fields)
            {
                if (editor != field)
                {
                    editor.StopEditingFromService();
                }
            }
        }

        // Called by inline editors and text inputs while drawing. editor identifies the input (an object, or a
        // per-row key such as a control name when one element instance draws many rows).
        // guiRect is in the current GUI coordinates
        // (e.g. inside a scroll view); it is stored in screen coordinates to compare with window clicks.
        public static void ReportEditor(
            object editor,
            Rect guiRect,
            Func<bool> isEditing)
        {
            Event current = Event.current;

            // During Layout, rects are placeholders.
            if (current == null ||
                current.type == EventType.Layout)
            {
                return;
            }

            if (isEditing())
            {
                _activeEditor = editor;
                _isActiveEditorEditing = isEditing;
                _activeEditorScreenRect = GUIUtility.GUIToScreenRect(guiRect);

                return;
            }

            // Equals (not reference equality): shared row elements identify themselves by a per-row key.
            if (Equals(_activeEditor, editor))
            {
                _activeEditor = null;
                _isActiveEditorEditing = null;
            }
        }

        // True if an inline editor is being edited and screenPoint lies inside it.
        public static bool IsEditingAt(Vector2 screenPoint)
        {
            return _activeEditor != null &&
                   _isActiveEditorEditing != null &&
                   _isActiveEditorEditing() &&
                   _activeEditorScreenRect.Contains(screenPoint);
        }
        #endregion
    }
}
