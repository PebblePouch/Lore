using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class LibraryInlineEnumSelector
    {
        #region Public API
        // label: shown instead of the value's name (the menu still lists the names).
        // groups: menu order with separators between groups (see LoreChoiceMenu).
        public void Draw<T>(
            Rect rect,
            T value,
            GUIStyle viewer,
            Action<T> onSelected,
            Func<T, string> getName = null,
            string label = null,
            IReadOnlyList<IReadOnlyList<T>> groups = null)
            where T : Enum
        {
            GUI.Label(
                rect,
                label ??
                (getName != null
                    ? getName(value)
                    : value.ToString()),
                viewer);

            EditorGUIUtility.AddCursorRect(
                rect,
                MouseCursor.Link);

            if (Event.current.type != EventType.MouseDown 
                || !rect.Contains(Event.current.mousePosition))
            {
                return;
            }

            LoreChoiceMenu.Show(
                rect,
                value,
                onSelected,
                getName,
                groups);

            Event.current.Use();
        }
        #endregion
    }
}