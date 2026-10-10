using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class ProjectHeaderSection
    {
        #region Fields
        private readonly ProjectGlyphElement _glyphElement = new();
        private readonly ProjectTitleElement _titleElement;
        private readonly TimestampsElement _timestampsElement = new();
        #endregion

        #region Properties
        public bool IsEditing => _titleElement.IsEditing;
        #endregion

        #region Constructor
        public ProjectHeaderSection()
        {
            _titleElement =
                new ProjectTitleElement();
        }
        #endregion

        #region Public API
        public void HandleOutsideClick(
            Vector2 mousePosition)
        {
            _titleElement.HandleOutsideClick(
                mousePosition);
        }

        public void BeginTitleEditing()
        {
            _titleElement.BeginEditing();
        }

        public void CommitEditing()
        {
            _titleElement.CommitEditing();
        }

        public void Draw()
        {
            EditorGUILayout.BeginHorizontal(
                InspectorStyle.HeaderLayout,
                GUILayout.Height(
                    InspectorStyle.HeaderHeight));

            // No color here: keep the color strip's width so glyph and title line up with Category/Entry headers.
            GUILayout.Space(
                InspectorStyle.ColorPicker.Width);

            _glyphElement.Draw();
            _titleElement.Draw();

            // The Project asset is created lazily (on the first edit): no timestamps until it exists.
            _timestampsElement.SetAsset(
                LoreRepository.Instance.Project);

            _timestampsElement.Draw();

            // No menu here: reserve its width so the timestamps line up with Category/Entry headers.
            TimestampsElement.ReserveMenuWidth();

            EditorGUILayout.EndHorizontal();
        }
        #endregion
    }
}
