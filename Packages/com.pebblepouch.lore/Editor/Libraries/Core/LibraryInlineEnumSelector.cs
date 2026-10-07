using System;
using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class LibraryInlineEnumSelector
    {
        #region Public API
        // label: shown instead of the value's name (the menu still lists the names).
        public void Draw<T>(
            Rect rect,
            T value,
            GUIStyle viewer,
            Action<T> onSelected,
            Func<T, string> getName = null,
            string label = null)
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
                getName);

            Event.current.Use();
        }
        #endregion
    }
}