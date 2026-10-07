using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static class LoreActionMenu
    {
        #region Public API
        public static void Show(
            Rect anchor,
            IReadOnlyList<LoreMenuItem> items)
        {
            PopupWindow.Show(
                anchor,
                new LoreMenuPopup(
                    items));
        }
        #endregion
    }
}