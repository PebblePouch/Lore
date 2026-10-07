using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static class EmptyStateCard
    {
        public static void Draw(
            string title,
            string message = null)
        {
            Rect rect = EditorGUILayout.BeginVertical(
                LibraryStyle.Card,
                GUILayout.Height(LibraryStyle.CardHeight));

            EditorGUI.DrawRect(
                rect,
                LibraryStyle.CardNormal);

            GUILayout.FlexibleSpace();

            GUILayout.Label(
                title,
                LibraryStyle.Empty);

            if (!string.IsNullOrWhiteSpace(message))
            {
                GUILayout.Label(
                    message,
                    LibraryStyle.Empty);
            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndVertical();
        }
    }
}