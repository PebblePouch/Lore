using System;

using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Shared row layout of the Project, Category and Entry Insights sections.
    public static class InsightsRow
    {
        #region Public API
        // Call at the top of an Insights section in every event. Layout runs before Repaint and uses this
        // value, so a new width needs one more repaint (otherwise the cards lag one frame while resizing).
        public static void UpdateAvailableWidth(ref float availableWidth)
        {
            Rect widthRect =
                GUILayoutUtility.GetRect(
                    0f,
                    0f,
                    GUILayout.ExpandWidth(true));

            if (Event.current.type != EventType.Repaint ||
                Mathf.Approximately(
                    availableWidth,
                    widthRect.width))
            {
                return;
            }

            availableWidth = widthRect.width;

            LoreRepaintService.Repaint();
        }

        // Equal-width columns; card heights are synced through rowHeight (see InsightsRowHeight).
        // Columns use MaxWidth (not Width): IMGUI passes minimum widths up to the whole Inspector.
        public static void Draw(
            float availableWidth,
            InsightsRowHeight rowHeight,
            params Action[] cells)
        {
            if (cells == null ||
                cells.Length == 0)
            {
                return;
            }

            float totalSpacing =
                InsightsStyle.ColumnSpacing *
                (cells.Length - 1);

            float usableWidth =
                Mathf.Max(
                    0f,
                    availableWidth -
                    totalSpacing);

            float cellWidth =
                Mathf.Floor(
                    usableWidth /
                    cells.Length);

            rowHeight.Begin();

            GUILayout.BeginHorizontal(
                GUILayout.ExpandWidth(true));

            for (int i = 0;
                 i < cells.Length;
                 i++)
            {
                if (i > 0)
                {
                    GUILayout.Space(
                        InsightsStyle.ColumnSpacing);
                }

                // Before the first width measurement: no limit (the row corrects itself on the next repaint).
                float maximumWidth =
                    availableWidth <= 0f
                        ? float.MaxValue
                        : i == cells.Length - 1
                            ? usableWidth -
                              cellWidth * (cells.Length - 1)
                            : cellWidth;

                GUILayout.BeginVertical(
                    GUILayout.MinWidth(0f),
                    GUILayout.MaxWidth(maximumWidth),
                    GUILayout.ExpandWidth(true));

                cells[i]?.Invoke();

                GUILayout.EndVertical();
            }

            GUILayout.EndHorizontal();

            rowHeight.End();
        }
        #endregion
    }
}
