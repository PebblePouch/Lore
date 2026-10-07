using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class ProjectTitleElement
    {
        #region Fields
        private readonly LoreInlineTextField _textField;
        private readonly ProjectService _projectService;

        private bool _wasEditing;

        private Rect _lastScreenRect;
        #endregion

        #region Properties
        public bool IsEditing => _textField.IsEditing;
        #endregion

        #region Constructor
        public ProjectTitleElement()
        {
            _textField = new LoreInlineTextField();
            _projectService = new ProjectService();
        }
        #endregion

        #region Public API
        public void BeginEditing()
        {
            _textField.BeginEditing(
                _projectService.GetDisplayTitle());
        }

        public void Draw()
        {
            DrawContent();
        }

        public void CommitEditing()
        {
            if (!_textField.IsEditing)
                return;

            CommitTitle(_textField.EditingText);
        }
        #endregion

        #region Draw
        private void DrawContent()
        {
            Rect titleRect;

            string title = _textField.Draw(
                out titleRect,
                _projectService.GetDisplayTitle(),
                string.Empty,
                InspectorStyle.ProjectTitle);

            _lastScreenRect = LoreScreenRect.FromGUI(titleRect, _lastScreenRect);

            bool isEditing = _textField.IsEditing;

            if (_wasEditing && !isEditing)
            {
                CommitTitle(title);
            }

            _wasEditing = isEditing;
        }
        #endregion

        #region Commands
        public void HandleOutsideClick(
            Vector2 mousePosition)
        {
            if (!_textField.IsEditing)
                return;

            if (LoreScreenRect.Contains(_lastScreenRect,
                    mousePosition))
            {
                return;
            }

            CommitEditing();
        }
        #endregion

        #region Helpers
        private void CommitTitle(
            string title)
        {
            _textField.StopEditing();

            _wasEditing = false;

            _projectService.UpdateTitle(title);

            LoreRepaintService.Repaint();
        }
        #endregion
    }
}
