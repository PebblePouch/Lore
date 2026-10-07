using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Project Settings > Lore. Drawn with Unity's own controls, like other Project Settings pages.
    public static class LoreSettingsProvider
    {
        #region Constants
        public const string SettingsPath = "Project/Lore";
        #endregion

        #region Fields
        // Result of the last "Generate" click, shown below the button (until the next domain reload).
        private static FieldConstantsService.Result _lastResult;
        #endregion

        #region Provider
        [SettingsProvider]
        public static SettingsProvider Create() =>
            new(SettingsPath, SettingsScope.Project)
            {
                guiHandler = _ => DrawGUI(),
                keywords = LoreText.Settings.Keywords,
            };
        #endregion

        #region Drawing
        private static void DrawGUI()
        {
            LoreSettings settings = LoreSettings.instance;

            float previousLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = SettingsStyle.LabelWidth;

            using (new EditorGUILayout.VerticalScope(SettingsStyle.Page))
            {
                EditorGUILayout.LabelField(
                    LoreText.Settings.FieldConstantsHeader,
                    EditorStyles.boldLabel);

                EditorGUILayout.LabelField(
                    LoreText.Settings.FieldConstantsInfo,
                    EditorStyles.wordWrappedMiniLabel);

                GUILayout.Space(SettingsStyle.SectionSpacing);

                DrawPathField(settings);

                settings.SetFieldConstantsNamespace(
                    DrawTextField(
                        LoreText.Settings.Namespace,
                        LoreText.Settings.NamespaceTooltip,
                        settings.FieldConstantsNamespace,
                        LoreText.Settings.GlobalNamespacePlaceholder));

                settings.SetFieldConstantsClassName(
                    DrawTextField(
                        LoreText.Settings.ClassName,
                        LoreText.Settings.ClassNameTooltip,
                        settings.FieldConstantsClassNameSetting,
                        LoreSettings.DefaultFieldConstantsClassName));

                string issue = settings.GetFieldConstantsIssue();

                if (issue != null)
                {
                    GUILayout.Space(SettingsStyle.SectionSpacing);

                    EditorGUILayout.HelpBox(
                        issue,
                        MessageType.Error);
                }

                GUILayout.Space(SettingsStyle.SectionSpacing);

                DrawGenerateButton(issue == null);
            }

            EditorGUIUtility.labelWidth = previousLabelWidth;
        }

        private static void DrawGenerateButton(bool enabled)
        {
            using (new EditorGUI.DisabledScope(!enabled))
            {
                if (GUILayout.Button(
                        LoreText.Settings.GenerateButton,
                        GUILayout.ExpandWidth(false)))
                {
                    _lastResult = FieldConstantsService.GenerateAndLog();
                }
            }

            if (_lastResult == null)
                return;

            GUILayout.Space(SettingsStyle.SectionSpacing);

            EditorGUILayout.HelpBox(
                FieldConstantsService.FormatResult(_lastResult),
                _lastResult.Status is FieldConstantsService.Status.Written or FieldConstantsService.Status.Unchanged
                    ? MessageType.Info
                    : MessageType.Error);
        }

        private static void DrawPathField(LoreSettings settings)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                settings.SetFieldConstantsPath(
                    DrawTextField(
                        LoreText.Settings.OutputFile,
                        LoreText.Settings.OutputFileTooltip,
                        settings.FieldConstantsPathSetting,
                        settings.DefaultFieldConstantsPath));

                if (GUILayout.Button(
                        LoreText.Settings.BrowseButton,
                        GUILayout.Width(SettingsStyle.BrowseButtonWidth)))
                {
                    string current = settings.FieldConstantsPath;

                    string path = EditorUtility.SaveFilePanelInProject(
                        LoreText.Settings.BrowseTitle,
                        System.IO.Path.GetFileNameWithoutExtension(current),
                        "cs",
                        LoreText.Settings.BrowseMessage,
                        System.IO.Path.GetDirectoryName(current)?.Replace('\\', '/') ?? "Assets");

                    // Leave the text field, so it shows the new value.
                    if (!string.IsNullOrEmpty(path))
                    {
                        GUIUtility.keyboardControl = 0;

                        settings.SetFieldConstantsPath(path);
                    }

                    GUIUtility.ExitGUI();
                }
            }
        }

        // Delayed text field (commits on Enter / focus loss) with a placeholder for the empty (default) value.
        private static string DrawTextField(
            string label,
            string tooltip,
            string value,
            string placeholder)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            string controlName = SettingsStyle.TextFieldControlPrefix + label;

            GUI.SetNextControlName(controlName);

            string result = EditorGUI.DelayedTextField(
                rect,
                new GUIContent(label, tooltip),
                value);

            if (string.IsNullOrEmpty(value) &&
                Event.current.type == EventType.Repaint &&
                GUI.GetNameOfFocusedControl() != controlName)
            {
                Rect textRect = rect;
                textRect.xMin += EditorGUIUtility.labelWidth + SettingsStyle.PlaceholderInset;

                SettingsStyle.Placeholder.Draw(
                    textRect,
                    placeholder,
                    false,
                    false,
                    false,
                    false);
            }

            return result;
        }
        #endregion
    }
}
