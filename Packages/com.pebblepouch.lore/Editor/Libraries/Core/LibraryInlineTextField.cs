using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class LibraryInlineTextField
    {
        #region Fields
        private string _value = string.Empty;

        private int _focusFramesRemaining;
        private bool _initialized;
        #endregion

        #region Public API
        public void BeginEditing(
            string value)
        {
            _value = value;

            _focusFramesRemaining = 2;
            _initialized = true;

            GUI.FocusControl(null);
        }

        public LibraryInlineTextFieldResult Draw(
            string id,
            Rect rect,
            string value,
            string placeholder,
            LibraryInlineTextFieldSettings settings)
        {
            if (!_initialized)
            {
                _value = value;
                _initialized = true;
            }

            string controlName = $"LibraryInlineTextField_{id}";

            string focusedControl = GUI.GetNameOfFocusedControl();

            bool canCommitWithReturn =
                string.IsNullOrEmpty(focusedControl) 
                || focusedControl == controlName;

            GUI.SetNextControlName(
                controlName);

            bool returnPressed =
                Event.current.type == EventType.KeyDown &&
                (Event.current.keyCode == KeyCode.Return ||
                 Event.current.keyCode == KeyCode.KeypadEnter);

            _value = EditorGUI.TextField(
                rect,
                _value,
                settings.Editor);

            if (_focusFramesRemaining > 0 &&
                Event.current.type == EventType.Repaint)
            {
                EditorGUI.FocusTextInControl(
                    controlName);

                _focusFramesRemaining--;
            }

            if (returnPressed && canCommitWithReturn)
            {
                GUIUtility.keyboardControl = 0;
                GUI.FocusControl(null);

                Event.current.Use();

                return new LibraryInlineTextFieldResult(
                    _value,
                    LibraryInlineTextFieldAction.Commit);
            }

            if (Event.current.type == EventType.MouseDown &&
                !rect.Contains(Event.current.mousePosition))
            {
                GUIUtility.keyboardControl = 0;
                GUI.FocusControl(null);
            }

            return new LibraryInlineTextFieldResult(
                _value,
                LibraryInlineTextFieldAction.None);
        }

        public void EndEditing()
        {
            _focusFramesRemaining = 0;
            _initialized = false;

            GUIUtility.keyboardControl = 0;
            GUI.FocusControl(null);
        }
        #endregion
    }
}