using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static class LoreChoiceMenu
    {
        #region Public API
        // groups: menu order with separators between groups; values in no group follow in a last group.
        // Null = enum order with a separator between all items.
        public static void Show<T>(
            Rect anchor,
            T currentValue,
            Action<T> selected,
            Func<T, string> getName = null,
            IReadOnlyList<IReadOnlyList<T>> groups = null)
            where T : Enum
        {
            var items =
                new List<LoreMenuItem>();

            var listed =
                new HashSet<T>();

            if (groups != null)
            {
                foreach (IReadOnlyList<T> group in groups)
                {
                    AddGroup(
                        items,
                        listed,
                        group,
                        currentValue,
                        selected,
                        getName);
                }
            }

            var rest =
                new List<T>();

            foreach (T value in
                     Enum.GetValues(
                         typeof(T)))
            {
                if (listed.Contains(value))
                    continue;

                if (groups != null)
                {
                    rest.Add(value);

                    continue;
                }

                AddGroup(
                    items,
                    listed,
                    new[] { value },
                    currentValue,
                    selected,
                    getName);
            }

            AddGroup(
                items,
                listed,
                rest,
                currentValue,
                selected,
                getName);

            PopupWindow.Show(
                anchor,
                new LoreMenuPopup(
                    items));
        }
        #endregion

        #region Helpers
        private static void AddGroup<T>(
            List<LoreMenuItem> items,
            HashSet<T> listed,
            IReadOnlyList<T> group,
            T currentValue,
            Action<T> selected,
            Func<T, string> getName)
            where T : Enum
        {
            bool first = true;

            foreach (T value in group)
            {
                if (!listed.Add(value))
                    continue;

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
                            first &&
                            items.Count > 0,
                        selected:
                            Equals(
                                value,
                                currentValue)));

                first = false;
            }
        }
        #endregion
    }
}
