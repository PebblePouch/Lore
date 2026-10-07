using System;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Header of a collapsible Inspector section with a title and optional action buttons on the right
    // (shown only while expanded), e.g. "Links (3)  [+]". Used by Project Links, Category Fields,
    // Entry Fields and Entry Tags.
    public static class CollapsibleSectionHeader
    {
        #region Public API
        public static void Draw(
            string title,
            bool expanded,
            Action toggle,
            Action drawButtons = null)
        {
            GUILayout.BeginHorizontal();

            Rect foldoutRect =
                GUILayoutUtility.GetRect(
                    InspectorStyle.SectionFoldoutWidth,
                    EditorGUIUtility.singleLineHeight,
                    GUILayout.Width(
                        InspectorStyle.SectionFoldoutWidth));

            GUI.Label(
                foldoutRect,
                expanded
                    ? InspectorStyle.SectionFoldoutExpandedString
                    : InspectorStyle.SectionFoldoutCollapsedString,
                InspectorStyle.SectionCardFoldout);

            Rect labelRect =
                GUILayoutUtility.GetRect(
                    new GUIContent(title),
                    InspectorStyle.SectionCardFieldHeader,
                    GUILayout.ExpandWidth(true));

            GUI.Label(
                labelRect,
                title,
                InspectorStyle.SectionCardFieldHeader);

            // Foldout and title together toggle the section.
            Rect toggleRect =
                new(
                    foldoutRect.x,
                    Mathf.Min(
                        foldoutRect.y,
                        labelRect.y),
                    labelRect.xMax -
                        foldoutRect.x,
                    Mathf.Max(
                        foldoutRect.yMax,
                        labelRect.yMax) -
                        Mathf.Min(
                            foldoutRect.y,
                            labelRect.y));

            EditorGUIUtility.AddCursorRect(
                toggleRect,
                MouseCursor.Link);

            if (Event.current.type ==
                    EventType.MouseDown &&
                Event.current.button == 0 &&
                toggleRect.Contains(
                    Event.current.mousePosition))
            {
                toggle?.Invoke();

                Event.current.Use();
            }

            GUILayout.FlexibleSpace();

            if (expanded)
            {
                drawButtons?.Invoke();
            }

            GUILayout.EndHorizontal();
        }
        #endregion
    }
}
