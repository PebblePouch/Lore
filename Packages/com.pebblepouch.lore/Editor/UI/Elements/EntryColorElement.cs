using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class EntryColorElement
    {
        private Entry _entry;

        public void SetEntry(Entry entry)
        {
            _entry = entry;
        }

        public void Draw()
        {
            if (_entry == null)
                return;

            Color color = _entry.Category != null
                ? _entry.Category.Color
                : Color.gray;

            Rect rect = GUILayoutUtility.GetRect(
                InspectorStyle.ColorWidth,
                InspectorStyle.ColorHeight,
                GUILayout.Width(InspectorStyle.ColorWidth),
                GUILayout.Height(InspectorStyle.HeaderHeight));

            EditorGUI.DrawRect(rect, color);
        }
    }
}