using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Equal card heights within one Insights row.
    // "Stretch height" on layout groups does not work inside the Inspector scroll view (IMGUI calculates
    // group heights twice and the second pass resets it), so each column reports its natural content
    // height on Repaint and all cards of the row get the tallest one as minimum height in the next frame.
    // Natural heights do not include that minimum, so the row also gets shorter again.
    public sealed class InsightsRowHeight
    {
        #region Fields
        private float _height;
        private float _measuredHeight;
        #endregion

        #region Public API
        // Minimum height of each card in the row (the tallest natural height of the last repaint).
        public GUILayoutOption MinHeight =>
            GUILayout.MinHeight(_height);

        // E.g. when another Category/Entry is shown: start from natural heights instead of the old row height.
        public void Reset()
        {
            _height = 0f;
        }

        public void Begin()
        {
            if (Event.current.type == EventType.Repaint)
            {
                _measuredHeight = 0f;
            }
        }

        public void Report(float naturalColumnHeight)
        {
            if (Event.current.type == EventType.Repaint)
            {
                _measuredHeight =
                    Mathf.Max(
                        _measuredHeight,
                        naturalColumnHeight);
            }
        }

        public void End()
        {
            if (Event.current.type != EventType.Repaint ||
                Mathf.Approximately(
                    _height,
                    _measuredHeight))
            {
                return;
            }

            _height = _measuredHeight;

            LoreRepaintService.Repaint();
        }
        #endregion
    }
}
