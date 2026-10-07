using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class LoreInlineTextField
    {
        #region Fields
        private bool _isEditing;
        private bool _requestFocus;

        private string _initialText;
        private string _editingText;

        // Last rect from a non-Layout event (see Draw).
        private Rect _lastDrawnRect;
        #endregion

        #region Properties
        public bool IsEditing => _isEditing;
        public string EditingText => _editingText;
        #endregion

        #region Constructor
        public LoreInlineTextField()
        {
            LoreEditService.Register(this);
        }
        #endregion

        #region Public API
        public string Draw(
            out Rect rect,
            string text,
            string placeholder,
            LoreInlineTextFieldSettings settings,
            string controlName = null)
        {
            rect =
                GUILayoutUtility.GetRect(
                    0,
                    settings.Height,
                    GUILayout.ExpandWidth(true));

            // During Layout, GetRect returns a 1x1 placeholder. Callers keep this rect for outside-click
            // checks, and Layout runs right before every MouseDown: report the last real rect instead,
            // or a click inside the field is treated as outside (commit + restart editing, selecting all).
            if (Event.current.type == EventType.Layout)
            {
                rect = _lastDrawnRect;
            }
            else
            {
                _lastDrawnRect = rect;
            }

            string result;

            if (_isEditing)
            {
                result =
                    DrawEditor(
                        rect,
                        settings,
                        controlName);
            }
            else
            {
                DrawViewer(
                    rect,
                    text,
                    placeholder,
                    settings);

                result = text;
            }

            // Lets the window keep keyboard focus for clicks inside this field while it is edited.
            LoreEditService.ReportEditor(
                this,
                rect,
                () => _isEditing);

            return result;
        }
        #endregion

        #region Draw
        private void DrawViewer(
            Rect rect,
            string text,
            string placeholder,
            LoreInlineTextFieldSettings settings)
        {
            string displayText =
                string.IsNullOrWhiteSpace(text)
                    ? placeholder
                    : text;

            GUIStyle style =
                string.IsNullOrWhiteSpace(text)
                    ? settings.PlaceholderContent
                    : settings.ViewerContent;

            Rect contentRect =
                new(
                    rect.x
                    + settings.HorizontalInset,
                    rect.y
                    + settings.VerticalInset,
                    rect.width
                    - settings.HorizontalInset * 2,
                    rect.height
                    - settings.VerticalInset * 2);

            GUI.Label(
                contentRect,
                displayText,
                style);

            if (Event.current.type ==
                    EventType.MouseDown &&
                rect.Contains(
                    Event.current.mousePosition))
            {
                BeginEditing(
                    text);

                Event.current.Use();
            }
        }

        private string DrawEditor(
            Rect rect,
            LoreInlineTextFieldSettings settings,
            string controlName)
        {
            string finalControlName =
                string.IsNullOrEmpty(
                    controlName)
                    ? settings.ControlName
                    : controlName;

            GUI.SetNextControlName(
                finalControlName);

            if (HandleKeyboard())
            {
                return _editingText;
            }

            _editingText =
                EditorGUI.TextField(
                    rect,
                    _editingText,
                    settings.EditorContent);

            HandleFocus(
                finalControlName);

            if (_isEditing &&
                Event.current.type ==
                    EventType.MouseDown &&
                !rect.Contains(
                    Event.current.mousePosition))
            {
                StopEditing();
            }

            return _editingText;
        }
        #endregion

        #region Commands
        public void BeginEditing(string initialText = "")
        {
            LoreEditService.RequestEditing(this);

            _initialText = initialText ?? string.Empty;
            _editingText = _initialText;

            _isEditing = true;
            _requestFocus = true;
        }

        public void ContinueEditing(string text)
        {
            _editingText = text ?? string.Empty;

            _isEditing = true;
            _requestFocus = true;
        }

        public void StopEditing()
        {
            _isEditing = false;
            _requestFocus = false;
        }

        public void StopEditingFromService()
        {
            _isEditing = false;
            _requestFocus = false;
        }
        #endregion

        #region Helper
        private bool HandleKeyboard()
        {
            if (Event.current.type !=
                EventType.KeyDown)
            {
                return false;
            }

            switch (Event.current.keyCode)
            {
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    StopEditing();

                    GUI.FocusControl(null);
                    EditorGUI.FocusTextInControl(null);

                    Event.current.Use();

                    return true;

                case KeyCode.Escape:
                    _editingText =
                        _initialText;

                    StopEditing();

                    GUI.FocusControl(null);
                    EditorGUI.FocusTextInControl(null);

                    Event.current.Use();

                    return true;
            }

            return false;
        }

        private void HandleFocus(
            string controlName)
        {
            if (!_requestFocus)
                return;

            if (Event.current.type !=
                EventType.Repaint)
            {
                return;
            }

            EditorGUI.FocusTextInControl(
                controlName);

            EditorGUIUtility.editingTextField =
                true;

            if (GUI.GetNameOfFocusedControl() !=
                controlName)
            {
                return;
            }

            _requestFocus = false;
        }
        #endregion
    }
}