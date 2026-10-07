using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public readonly struct BrowserItemDrawResult
    {
        #region Properties
        public Rect Rect { get; }

        public bool Clicked { get; }
        #endregion

        #region Constructor
        public BrowserItemDrawResult(
            Rect rect,
            bool clicked)
        {
            Rect = rect;
            Clicked = clicked;
        }
        #endregion
    }
}