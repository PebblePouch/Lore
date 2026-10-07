using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class CategoryActionsElement
    {
        #region Fields
        private Category _category;
        #endregion

        #region Public API
        public void SetCategory(Category category)
        {
            _category = category;
        }

        public void Draw()
        {
            DrawContent();
        }
        #endregion

        #region Draw
        private void DrawContent()
        {
            GUILayout.BeginHorizontal(
                InspectorStyle.ActionsLayout,
                GUILayout.Width(InspectorStyle.ActionsWidth),
                GUILayout.Height(InspectorStyle.ActionsHeight));

            DrawButtons();

            GUILayout.EndHorizontal();
        }

        private void DrawButtons()
        {
            /* FOR LATER: mark favorite
            ButtonDrawer.Favorite(
                null,
                ButtonDrawer.DisplayMode.Glyph,
                CategoryInspectorStyle.ActionsContent,
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true));
            */

            GUILayout.Box(
                GUIContent.none,
                GUIStyle.none,
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true));
        }
        #endregion
    }
}