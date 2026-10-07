using System;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static class SectionCard
    {
        #region Public API
        public static void Draw(string title, Action body)
        {
            Begin(title);

            body?.Invoke();

            End();
        }

        public static void Draw(string title, Action top, Action bottom)
        {
            Begin(title);

            top?.Invoke();
            bottom?.Invoke();

            End();
        }

        public static void Draw(Action body)
        {
            Begin();

            body?.Invoke();

            End();
        }

        public static void Draw(
            string title,
            Action body,
            GUIStyle cardStyle,
            params GUILayoutOption[] options)
        {
            Begin(
                title,
                cardStyle,
                options);

            body?.Invoke();

            End();
        }

        public static void Draw(
            Action body,
            GUIStyle cardStyle,
            params GUILayoutOption[] options)
        {
            Begin(
                cardStyle,
                options);

            body?.Invoke();

            End();
        }

        public static Rect DrawMeasured(
            string title,
            Action body,
            GUIStyle cardStyle,
            params GUILayoutOption[] options)
        {
            Rect rect =
                EditorGUILayout.BeginVertical(
                    cardStyle,
                    options);

            EditorGUILayout.LabelField(
                title,
                InspectorStyle.SectionCardHeader,
                GUILayout.MinWidth(0f),
                GUILayout.ExpandWidth(true));

            body?.Invoke();

            EditorGUILayout.EndVertical();

            return rect;
        }

        // Collapsible card in an Insights row: gets the row height as minimum height and reports its natural
        // height (measured by a marker after the content, so without the minimum).
        // Returns the card rect (valid on Repaint).
        public static Rect DrawRowCollapsible(
            string title,
            bool expanded,
            Action toggle,
            Action body,
            GUIStyle cardStyle,
            InsightsRowHeight rowHeight)
        {
            Rect rect =
                EditorGUILayout.BeginVertical(
                    cardStyle,
                    rowHeight.MinHeight);

            DrawCollapsibleHeader(
                title,
                expanded,
                toggle,
                InspectorStyle.SectionCardHeader);

            if (expanded)
            {
                body?.Invoke();
            }

            Rect contentEnd =
                GUILayoutUtility.GetRect(
                    0f,
                    0f);

            EditorGUILayout.EndVertical();

            rowHeight.Report(
                contentEnd.yMax -
                rect.y +
                cardStyle.padding.bottom);

            return rect;
        }

        public static void DrawCollapsible(
            string title,
            bool expanded,
            Action toggle,
            Action body)
        {
            EditorGUILayout.BeginVertical(
                InspectorStyle.SectionCard);

            DrawCollapsibleHeader(
                title,
                expanded,
                toggle,
                InspectorStyle.SectionCardHeader);

            if (expanded)
            {
                body?.Invoke();
            }

            EditorGUILayout.EndVertical();
        }

        public static void DrawCollapsibleHeader(
            string title,
            bool expanded,
            Action toggle,
            GUIStyle titleStyle)
        {
            Rect headerRect =
                EditorGUILayout.BeginHorizontal(
                    GUILayout.ExpandWidth(true));

            GUILayout.Label(
                expanded
                    ? InspectorStyle.SectionFoldoutExpandedString
                    : InspectorStyle.SectionFoldoutCollapsedString,
                InspectorStyle.SectionCardFoldout,
                GUILayout.Width(
                    InspectorStyle.SectionFoldoutWidth));

            // MinWidth(0): long titles must not hold the card (and the Inspector) wider than the window.
            GUILayout.Label(
                title,
                titleStyle,
                GUILayout.MinWidth(0f),
                GUILayout.ExpandWidth(true));

            EditorGUILayout.EndHorizontal();

            EditorGUIUtility.AddCursorRect(
                headerRect,
                MouseCursor.Link);

            if (Event.current.type ==
                    EventType.MouseDown &&
                Event.current.button == 0 &&
                headerRect.Contains(
                    Event.current.mousePosition))
            {
                toggle?.Invoke();

                Event.current.Use();
            }
        }
        #endregion

        #region Private Helpers
        private static void Begin()
        {
            EditorGUILayout.BeginVertical(
                InspectorStyle.SectionCard);
        }

        private static void Begin(string title)
        {
            EditorGUILayout.BeginVertical(
                InspectorStyle.SectionCard);

            EditorGUILayout.LabelField(
                title,
                InspectorStyle.SectionCardHeader);
        }

        private static void Begin(
            GUIStyle cardStyle,
            params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical(
                cardStyle,
                options);
        }

        private static void Begin(
            string title,
            GUIStyle cardStyle,
            params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical(
                cardStyle,
                options);

            // This fixed the issue that SectionCards claimed more width than they actually used
            EditorGUILayout.LabelField(
                title,
                InspectorStyle.SectionCardHeader,
                GUILayout.MinWidth(0f),
                GUILayout.ExpandWidth(true));
        }

        private static void End()
        {
            EditorGUILayout.EndVertical();
        }
        #endregion
    }
}