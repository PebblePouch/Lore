using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class LoreMenuPopup
        : PopupWindowContent
    {
        #region Fields
        private readonly IReadOnlyList<LoreMenuItem> _items;
        #endregion

        #region Constructor
        public LoreMenuPopup(IReadOnlyList<LoreMenuItem> items)
        {
            _items = items;
        }
        #endregion

        #region Public API
        public override Vector2 GetWindowSize()
        {
            float height =
                MenuStyle.VerticalEdgeMargin * 2f;

            foreach (LoreMenuItem item in
                     _items)
            {
                if (item.SeparatorBefore)
                {
                    height +=
                        MenuStyle.VerticalEdgeMargin * 2f
                        + MenuStyle.SeparatorHeight;
                }

                height +=
                    MenuStyle.RowHeight;
            }

            return new Vector2(
                GetWidth(),
                height);
        }

        // At least MenuStyle.Width; wider when an item's text needs it (e.g. "Link to 🐺 Wolf  (Characters)").
        private float GetWidth()
        {
            float textWidth = 0f;

            foreach (LoreMenuItem item in
                     _items)
            {
                GUIStyle style =
                    item.Destructive
                        ? MenuStyle.DestructiveRow
                        : MenuStyle.Row;

                textWidth =
                    Mathf.Max(
                        textWidth,
                        style.CalcSize(item.Content).x);
            }

            return Mathf.Max(
                MenuStyle.Width,
                MenuStyle.HorizontalEdgeMargin * 2f
                + MenuStyle.GlyphWidth
                + MenuStyle.ColumnSpacing
                + textWidth);
        }
        #endregion

        #region Unity
        public override void OnGUI(
            Rect rect)
        {
            if (Event.current.type == EventType.KeyDown 
                && Event.current.keyCode == KeyCode.Escape)
            {
                Event.current.Use();

                editorWindow.Close();

                return;
            }

            EditorGUI.DrawRect(
                rect,
                MenuStyle.Background);

            GUILayout.Space(
                MenuStyle.VerticalEdgeMargin);

            foreach (LoreMenuItem item in
                     _items)
            {
                if (item.SeparatorBefore)
                {
                    DrawSeparator();
                }

                DrawItem(
                    item);
            }

            GUILayout.Space(
                MenuStyle.VerticalEdgeMargin);
        }
        #endregion

        #region Draw
        private void DrawItem(
            LoreMenuItem item)
        {
            Rect rowRect =
                GUILayoutUtility.GetRect(
                    0f,
                    MenuStyle.RowHeight,
                    GUILayout.ExpandWidth(true));

            bool hovered =
                rowRect.Contains(
                    Event.current.mousePosition);

            if (hovered)
            {
                EditorGUI.DrawRect(
                    rowRect,
                    MenuStyle.RowHover);
            }

            GUIStyle style =
                item.Destructive
                    ? MenuStyle.DestructiveRow
                    : MenuStyle.Row;

            Rect glyphRect =
                new(
                    rowRect.x
                        + MenuStyle.HorizontalEdgeMargin,
                    rowRect.y,
                    MenuStyle.GlyphWidth,
                    rowRect.height);

            Rect contentRect =
                new(
                    glyphRect.xMax
                        + MenuStyle.ColumnSpacing,
                    rowRect.y,
                    rowRect.xMax
                        - MenuStyle.HorizontalEdgeMargin
                        - glyphRect.xMax
                        - MenuStyle.ColumnSpacing,
                    rowRect.height);

            string glyph =
                item.Selected
                    ? MenuStyle.SelectedString
                    : item.Glyph;

            if (!string.IsNullOrEmpty(
                    glyph))
            {
                GUI.Label(
                    glyphRect,
                    glyph,
                    MenuStyle.Glyph);
            }

            GUI.Label(
                contentRect,
                item.Content,
                style);

            EditorGUIUtility.AddCursorRect(
                rowRect,
                MouseCursor.Link);

            if (!EditorInteraction.Click(
                    rowRect))
            {
                return;
            }

            item.Action?.Invoke();

            editorWindow.Close();
        }

        private static void DrawSeparator()
        {
            GUILayout.Space(
                MenuStyle.VerticalEdgeMargin);

            Rect rect =
                GUILayoutUtility.GetRect(
                    0f,
                    MenuStyle.SeparatorHeight,
                    GUILayout.ExpandWidth(true));

            EditorGUI.DrawRect(
                rect,
                MenuStyle.Separator);

            GUILayout.Space(
                MenuStyle.VerticalEdgeMargin);
        }
        #endregion
    }
}