using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static class LoreChoiceMenu
    {
        #region Public API
        public static void Show<T>(
            Rect anchor,
            T currentValue,
            Action<T> selected,
            Func<T, string> getName = null)
            where T : Enum
        {
            var items =
                new List<LoreMenuItem>();

            foreach (T value in
                     Enum.GetValues(
                         typeof(T)))
            {
                T captured =
                    value;

                items.Add(
                    new LoreMenuItem(
                        getName != null
                            ? getName(value)
                            : value.ToString(),
                        () => selected?.Invoke(
                            captured),
                        separatorBefore:
                            items.Count > 0,
                        selected:
                            Equals(
                                value,
                                currentValue)));
            }

            PopupWindow.Show(
                anchor,
                new LoreMenuPopup(
                    items));
        }
        #endregion
    }
}