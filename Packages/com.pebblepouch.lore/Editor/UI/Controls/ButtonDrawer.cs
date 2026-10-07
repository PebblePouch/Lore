using System;
using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static class ButtonDrawer
    {
        public enum DisplayMode
        {
            Glyph = 0,
            GlyphName = 1,
            Name = 2,
            Tooltip = 3,
        };

        #region Buttons
        public static bool Menu(
            Action action,
            DisplayMode mode = DisplayMode.GlyphName,
            GUIStyle style = null,
            params GUILayoutOption[] options)
        {
            return Draw(
                LoreEditorTheme.Current.Menu,
                action,
                mode,
                style,
                options);
        }

        public static bool Clear(
            Action action,
            DisplayMode mode = DisplayMode.GlyphName,
            GUIStyle style = null,
            params GUILayoutOption[] options)
        {
            return Draw(
                LoreEditorTheme.Current.Clear,
                action,
                mode,
                style,
                options);
        }

        public static bool Clear(
            Rect rect,
            Action action,
            DisplayMode mode = DisplayMode.GlyphName,
            GUIStyle style = null)
        {
            return Draw(
                rect,
                LoreEditorTheme.Current.Clear,
                action,
                mode,
                style);
        }

        public static bool Create(
            Action action,
            DisplayMode mode = DisplayMode.GlyphName,
            GUIStyle style = null,
            params GUILayoutOption[] options)
        {
            return Draw(
                LoreEditorTheme.Current.Create,
                action,
                mode,
                style,
                options);
        }

        public static bool Add(
            Action action,
            DisplayMode mode = DisplayMode.GlyphName,
            GUIStyle style = null,
            params GUILayoutOption[] options)
        {
            return Draw(
                LoreEditorTheme.Current.Add,
                action,
                mode,
                style,
                options);
        }

        public static bool Add(
            Rect rect,
            Action action,
            DisplayMode mode = DisplayMode.GlyphName,
            GUIStyle style = null)
        {
            return Draw(
                rect,
                LoreEditorTheme.Current.Add,
                action,
                mode,
                style);
        }

        public static bool Remove(
            Action action,
            DisplayMode mode = DisplayMode.GlyphName,
            GUIStyle style = null,
            params GUILayoutOption[] options)
        {
            return Draw(
                LoreEditorTheme.Current.Remove,
                action,
                mode,
                style,
                options);
        }

        public static bool Remove(
            Rect rect,
            Action action,
            DisplayMode mode = DisplayMode.GlyphName,
            GUIStyle style = null)
        {
            return Draw(
                rect,
                LoreEditorTheme.Current.Remove,
                action,
                mode,
                style);
        }

        public static bool Open(
            Rect rect,
            Action action,
            DisplayMode mode = DisplayMode.GlyphName,
            GUIStyle style = null)
        {
            return Draw(
                rect,
                LoreEditorTheme.Current.Open,
                action,
                mode,
                style);
        }

        public static bool Delete(
            Action action,
            DisplayMode mode = DisplayMode.GlyphName,
            GUIStyle style = null,
            params GUILayoutOption[] options)
        {
            return Draw(
                LoreEditorTheme.Current.Delete,
                action,
                mode,
                style,
                options);
        }

        public static bool Duplicate(
            Action action,
            DisplayMode mode = DisplayMode.GlyphName,
            GUIStyle style = null,
            params GUILayoutOption[] options)
        {
            return Draw(
                LoreEditorTheme.Current.Duplicate,
                action,
                mode,
                style,
                options);
        }

        // Glyph and a short label (the glyph names the action: "➕ Tag"), or the glyph only; tooltip from the
        // definition in both cases.
        public static bool Labeled(
            ButtonDefinition definition,
            string label,
            Action action,
            bool glyphOnly,
            GUIStyle style = null,
            params GUILayoutOption[] options)
        {
            bool clicked =
                GUILayout.Button(
                    GetLabeledContent(definition, label, glyphOnly),
                    style ?? GUI.skin.button,
                    options);

            EditorGUIUtility.AddCursorRect(
                GUILayoutUtility.GetLastRect(),
                MouseCursor.Link);

            if (!clicked)
                return false;

            action?.Invoke();

            return true;
        }

        public static GUIContent GetLabeledContent(
            ButtonDefinition definition,
            string label,
            bool glyphOnly) =>
            new(
                glyphOnly
                    ? definition.Glyph
                    : $"{definition.Glyph} {label}",
                definition.Tooltip);
        #endregion

        #region Badges
        public static void Status(
            ButtonDefinition definition,
            GUIStyle style = null,
            params GUILayoutOption[] options)
        {
            GUIStyle buttonStyle =
                style ?? GUI.skin.button;

            GUILayout.Button(
                new GUIContent(definition.Glyph),
                buttonStyle,
                options);
        }
        #endregion

        #region Draw
        private static bool Draw(
            ButtonDefinition definition,
            Action action,
            DisplayMode mode = DisplayMode.GlyphName,
            GUIStyle style = null,
            params GUILayoutOption[] options)
        {
            bool clicked =
                GUILayout.Button(
                    definition.GetContent(mode),
                    style ?? GUI.skin.button,
                    options);

            Rect rect =
                GUILayoutUtility.GetLastRect();

            EditorGUIUtility.AddCursorRect(
                rect,
                MouseCursor.Link);

            if (!clicked)
                return false;

            action?.Invoke();

            return true;
        }

        private static bool Draw(
            Rect rect,
            ButtonDefinition definition,
            Action action,
            DisplayMode mode = DisplayMode.GlyphName,
            GUIStyle style = null)
        {
            bool clicked =
                GUI.Button(
                    rect,
                    definition.GetContent(mode),
                    style ?? GUI.skin.button);

            EditorGUIUtility.AddCursorRect(
                rect,
                MouseCursor.Link);

            if (!clicked)
                return false;

            action?.Invoke();

            return true;
        }
        #endregion
    }
}