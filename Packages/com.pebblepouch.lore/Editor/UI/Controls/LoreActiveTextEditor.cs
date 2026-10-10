using System.Reflection;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Access to the TextEditor of the focused EditorGUI text control.
    // EditorGUI does not expose its TextEditor publicly, so this reads the long-standing internal
    // EditorGUI.activeEditor field. Only the public TextEditor API is used on it (caret reads, text-boundary moves);
    // if a Unity update renames the field, the helpers simply do nothing and callers fall back to their default behavior.
    public static class LoreActiveTextEditor
    {
        #region Fields
        private static readonly FieldInfo ActiveEditorField =
            typeof(EditorGUI).GetField(
                "activeEditor",
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Static);
        #endregion

        #region Public API
        public static bool TryGetCaret(
            string controlName,
            out int cursorIndex,
            out Vector2 graphicalCursorPosition)
        {
            cursorIndex = -1;
            graphicalCursorPosition = Vector2.zero;

            if (GUI.GetNameOfFocusedControl() != controlName)
                return false;

            if (!TryGetFocusedEditor(out TextEditor editor))
                return false;

            cursorIndex = editor.cursorIndex;
            graphicalCursorPosition = editor.graphicalCursorPos;

            return true;
        }

        // True while the focused control is a text input being edited. editingTextField alone stays set after a text
        // edit while another control (e.g. an Enum popup) has the focus. Without the internal field: editingTextField.
        public static bool IsEditingText()
        {
            if (!EditorGUIUtility.editingTextField)
                return false;

            if (ActiveEditorField == null)
                return true;

            return TryGetFocusedEditor(out _);
        }

        // Ctrl+Home / Ctrl+End (with Shift: select) in the focused text control: Unity's text editing has no mapping
        // for them on Windows. End (with Shift: select): see HandleLineEndKey. Call at the start of OnGUI, before any
        // control can use the key.
        public static void HandleTextBoundaryKeys()
        {
            Event current = Event.current;

            if (current.type == EventType.KeyDown &&
                !current.control &&
                !current.alt &&
                current.keyCode == KeyCode.End)
            {
                HandleLineEndKey(current);

                return;
            }

            if (current.type != EventType.KeyDown ||
                !current.control ||
                current.alt ||
                (current.keyCode != KeyCode.Home && current.keyCode != KeyCode.End))
            {
                return;
            }

            if (!EditorGUIUtility.editingTextField ||
                !TryGetFocusedEditor(out TextEditor editor))
            {
                return;
            }

            bool end = current.keyCode == KeyCode.End;

            if (current.shift)
            {
                if (end)
                    editor.SelectTextEnd();
                else
                    editor.SelectTextStart();
            }
            else
            {
                if (end)
                    editor.MoveTextEnd();
                else
                    editor.MoveTextStart();
            }

            current.Use();
        }
        #endregion

        #region Helpers
        // Unity's line end can land on the start of the next row (after the line break, or after the space where a
        // wrapped row ends), so End visibly jumps one row down. Moves like Unity does, then steps back onto the row.
        // macOS keeps Unity's own End behavior.
        private static void HandleLineEndKey(Event current)
        {
            if (Application.platform == RuntimePlatform.OSXEditor ||
                !EditorGUIUtility.editingTextField ||
                !TryGetFocusedEditor(out TextEditor editor) ||
                editor.style == null)
            {
                return;
            }

            int origin = editor.cursorIndex;

            if (current.shift)
                editor.SelectGraphicalLineEnd();
            else
                editor.MoveGraphicalLineEnd();

            int target = editor.cursorIndex;

            if (target > origin)
            {
                GUIContent content = new(editor.text);

                float originY =
                    editor.style.GetCursorPixelPosition(
                        editor.position,
                        content,
                        origin).y;

                float targetY =
                    editor.style.GetCursorPixelPosition(
                        editor.position,
                        content,
                        target).y;

                if (targetY > originY + 0.5f)
                    target--;
            }

            editor.cursorIndex = target;

            if (!current.shift)
                editor.selectIndex = target;

            current.Use();
        }

        private static bool TryGetFocusedEditor(out TextEditor editor)
        {
            editor = null;

            if (ActiveEditorField == null)
                return false;

            try
            {
                editor = ActiveEditorField.GetValue(null) as TextEditor;
            }
            catch
            {
                return false;
            }

            return editor != null &&
                   editor.controlID == GUIUtility.keyboardControl;
        }
        #endregion
    }
}
