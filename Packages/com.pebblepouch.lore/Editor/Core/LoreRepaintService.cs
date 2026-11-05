using System;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Repaints the views that show Lore data: Lore's own windows (Lore window, Quick Search, dialogs), Inspectors
    // (Lore reference drawers) and Project Settings (Lore settings). Not every view: Lore stays out of the other
    // windows of the Editor, also while it runs in the background.
    public static class LoreRepaintService
    {
        #region Public API
        public static void Repaint()
        {
            foreach (EditorWindow window in Resources.FindObjectsOfTypeAll<EditorWindow>())
            {
                if (window == null)
                    continue;

                Type type =
                    window.GetType();

                if (type.Assembly == typeof(LoreRepaintService).Assembly ||
                    IsInspector(type))
                {
                    window.Repaint();
                }
            }

            SettingsService.RepaintAllSettingsWindow();
        }
        #endregion

        #region Helpers
        // InspectorWindow and floating Properties windows (both PropertyEditor) are internal: matched by name.
        private static bool IsInspector(Type type)
        {
            for (Type current = type; current != null; current = current.BaseType)
            {
                if (current.FullName is "UnityEditor.InspectorWindow" or "UnityEditor.PropertyEditor")
                    return true;
            }

            return false;
        }
        #endregion
    }
}
