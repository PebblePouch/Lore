using System;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static class EditorLayout
    {
        #region Scroll Views
        public static Vector2 ScrollView(
            Vector2 scroll,
            Action body,
            params GUILayoutOption[] options)
        {
            scroll =
                EditorGUILayout.BeginScrollView(
                    scroll,
                    options);

            body?.Invoke();

            EditorGUILayout.EndScrollView();

            return scroll;
        }
        #endregion
    }
}