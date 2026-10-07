using System;

using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Inline-editable description of a target (Category, Entry, or the project via ProjectService).
    // The element holds its own target, so a pending edit is always saved to the target it was
    // started on, even when the selection changes before the commit.
    public sealed class DescriptionElement<T> where T : class
    {
        #region Fields
        private readonly LoreInlineTextArea _textArea;

        private readonly Func<T, string> _getDescription;
        private readonly Action<T, string> _saveDescription;

        private readonly string _placeholder;

        // Resolved while drawing: style classes build GUIStyles from EditorStyles, which are not
        // available yet when panels are constructed (OnEnable after a domain reload).
        private readonly Func<LoreInlineTextAreaSettings> _getSettings;

        private T _target;

        private bool _wasEditing;

        private Rect _lastScreenRect;
        #endregion

        #region Properties
        public bool IsEditing => _textArea.IsEditing;

        public bool HasTarget => _target != null;
        #endregion

        #region Constructor
        public DescriptionElement(
            Func<T, string> getDescription,
            Action<T, string> saveDescription,
            string placeholder,
            Func<LoreInlineTextAreaSettings> getSettings)
        {
            _textArea = new LoreInlineTextArea();

            _getDescription = getDescription;
            _saveDescription = saveDescription;

            _placeholder = placeholder;
            _getSettings = getSettings;
        }
        #endregion

        #region Public API
        public void SetTarget(
            T target)
        {
            _target = target;
        }

        public void Draw()
        {
            DrawContent();
        }

        public void CommitEditing()
        {
            if (_target == null)
                return;

            if (!_textArea.IsEditing)
                return;

            string description =
                _textArea.EditingText;

            _textArea.CommitEditing();
            _textArea.ConsumeEndReason();

            _saveDescription(
                _target,
                description);

            _wasEditing = false;
        }

        public void HandleOutsideClick(
            Vector2 mousePosition)
        {
            if (!_textArea.IsEditing)
                return;

            if (LoreScreenRect.Contains(_lastScreenRect,
                    mousePosition))
            {
                return;
            }

            CommitEditing();
        }
        #endregion

        #region Draw
        private void DrawContent()
        {
            if (_target == null)
                return;

            Rect descriptionRect;

            // Link repairs are chosen in a popup after this event: save to the target shown now,
            // even if the selection changes in between.
            T target = _target;

            string description = _textArea.Draw(
                out descriptionRect,
                _getDescription(_target),
                _placeholder,
                _getSettings(),
                repairedDescription =>
                    _saveDescription(
                        target,
                        repairedDescription));

            _lastScreenRect =
                LoreScreenRect.FromGUI(descriptionRect, _lastScreenRect);

            if (_textArea.IsEditing &&
                Event.current.type == EventType.MouseDown &&
                !descriptionRect.Contains(Event.current.mousePosition))
            {
                _textArea.CommitEditing();

                Event.current.Use();
            }

            bool isEditing = _textArea.IsEditing;

            if (_wasEditing && !isEditing)
            {
                LoreInlineEditEndReason endReason = _textArea.ConsumeEndReason();

                switch (endReason)
                {
                    case LoreInlineEditEndReason.Commit:
                        _saveDescription(
                            _target,
                            description);
                        break;

                    case LoreInlineEditEndReason.Cancel:
                        break;
                }
            }

            _wasEditing = isEditing;
        }
        #endregion
    }
}
