using System;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Modal preview of a Category CSV import: what would change, warnings, Apply / Cancel.
    // Modal, so the data cannot change between the analysis and Apply. Drawn with Unity's own styles.
    public sealed class CsvImportPreviewWindow : EditorWindow
    {
        #region Fields
        private CategoryCsvImportPlan _plan;
        private Action<CategoryCsvImportPlan> _apply;
        private Vector2 _warningsScroll;
        #endregion

        #region Public API
        // apply: null = preview only (Apply disabled).
        public static void Open(
            CategoryCsvImportPlan plan,
            Action<CategoryCsvImportPlan> apply)
        {
            CsvImportPreviewWindow window = CreateInstance<CsvImportPreviewWindow>();

            window._plan = plan;
            window._apply = apply;
            window.titleContent = new GUIContent(LoreText.Csv.PreviewWindowTitle);
            window.minSize = ImportPreviewStyle.MinWindowSize;

            Rect main = EditorGUIUtility.GetMainWindowPosition();
            Vector2 size = ImportPreviewStyle.WindowSize;

            window.position = new Rect(
                main.x + (main.width - size.x) * 0.5f,
                main.y + (main.height - size.y) * 0.5f,
                size.x,
                size.y);

            window.ShowModalUtility();
        }
        #endregion

        #region Unity
        private void OnGUI()
        {
            if (_plan == null)
            {
                Close();

                return;
            }

            using (new EditorGUILayout.VerticalScope(ImportPreviewStyle.Page))
            {
                EditorGUILayout.LabelField(
                    string.Format(LoreText.Csv.PreviewHeader, _plan.Category != null ? _plan.Category.Title : string.Empty),
                    EditorStyles.boldLabel);

                EditorGUILayout.LabelField(
                    _plan.Path,
                    EditorStyles.wordWrappedMiniLabel);

                GUILayout.Space(ImportPreviewStyle.SectionSpacing);

                if (_plan.Error != null)
                    EditorGUILayout.HelpBox(_plan.Error, MessageType.Error);
                else
                    DrawSummary();

                DrawWarnings();

                GUILayout.FlexibleSpace();

                DrawButtons();
            }
        }
        #endregion

        #region Draw
        private void DrawSummary()
        {
            if (!_plan.HasChanges)
            {
                EditorGUILayout.HelpBox(LoreText.Csv.NoChanges, MessageType.Info);

                DrawCount(LoreText.Csv.UnchangedEntries, _plan.UnchangedCount);
                DrawCount(LoreText.Csv.UntouchedEntries, _plan.UntouchedEntryCount);

                return;
            }

            if (_plan.UpdatedCount > 0)
            {
                EditorGUILayout.LabelField(
                    LoreText.Csv.UpdatedEntries.Format(_plan.UpdatedCount) +
                    " (" + LoreText.Csv.ChangedValues.Format(_plan.ChangedValueCount) + ")");
            }

            DrawCount(LoreText.Csv.CreatedEntries, _plan.CreatedCount);
            DrawCount(LoreText.Csv.RenamedEntries, _plan.RenamedCount);
            DrawCount(LoreText.Csv.UnchangedEntries, _plan.UnchangedCount);
            DrawCount(LoreText.Csv.UntouchedEntries, _plan.UntouchedEntryCount);

            GUILayout.Space(ImportPreviewStyle.SectionSpacing);

            EditorGUILayout.LabelField(
                LoreText.Csv.UndoNote,
                EditorStyles.wordWrappedMiniLabel);
        }

        private static void DrawCount(
            LorePlural text,
            int count)
        {
            if (count > 0)
                EditorGUILayout.LabelField(text.Format(count));
        }

        private void DrawWarnings()
        {
            if (_plan.Warnings.Count == 0)
                return;

            GUILayout.Space(ImportPreviewStyle.SectionSpacing);

            EditorGUILayout.LabelField(
                LoreText.Csv.Warnings.Format(_plan.Warnings.Count),
                EditorStyles.boldLabel);

            _warningsScroll = EditorGUILayout.BeginScrollView(
                _warningsScroll,
                EditorStyles.helpBox,
                GUILayout.ExpandHeight(true));

            foreach (string warning in _plan.Warnings)
            {
                EditorGUILayout.LabelField(
                    warning,
                    EditorStyles.wordWrappedLabel);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawButtons()
        {
            GUILayout.Space(ImportPreviewStyle.SectionSpacing);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button(
                        LoreText.Common.CancelButton,
                        GUILayout.Width(ImportPreviewStyle.ButtonWidth)))
                {
                    Close();
                }

                using (new EditorGUI.DisabledScope(_apply == null || !_plan.HasChanges))
                {
                    if (GUILayout.Button(
                            LoreText.Csv.ApplyButton,
                            GUILayout.Width(ImportPreviewStyle.ButtonWidth)))
                    {
                        CategoryCsvImportPlan plan = _plan;
                        Action<CategoryCsvImportPlan> apply = _apply;

                        Close();

                        // After this GUI event: applying creates, renames and imports assets.
                        EditorApplication.delayCall += () => apply?.Invoke(plan);

                        GUIUtility.ExitGUI();
                    }
                }
            }
        }
        #endregion
    }
}
