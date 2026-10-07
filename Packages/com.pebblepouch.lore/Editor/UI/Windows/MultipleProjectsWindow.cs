using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Opens while the Unity project contains more than one Lore Project (e.g. the example Project imported next
    // to an own one): lists them and exports + removes the selected one. Reopens from the Lore window until only
    // one Project is left, and closes itself then.
    public sealed class MultipleProjectsWindow : EditorWindow
    {
        #region Types
        private sealed class Row
        {
            public LoreProject Project;
            public string Title;
            public string Folder;
            public int CategoryCount;
            public int EntryCount;
        }
        #endregion

        #region Fields
        private const string AssetsFolder = "Assets";

        private static bool _openRequested;

        private readonly List<Row> _rows = new();

        private int _selectedIndex = -1;
        #endregion

        #region Public API
        // Called by the Lore window on every OnGUI: cheap while there is at most one Project.
        public static void OpenIfNeeded()
        {
            if (_openRequested ||
                LoreRepository.Instance.Projects.Count <= 1 ||
                HasOpenInstances<MultipleProjectsWindow>())
            {
                return;
            }

            // Not inside the Lore window's OnGUI.
            _openRequested = true;

            EditorApplication.delayCall += () =>
            {
                _openRequested = false;

                if (LoreRepository.Instance.Projects.Count <= 1 ||
                    HasOpenInstances<MultipleProjectsWindow>())
                {
                    return;
                }

                var window = CreateInstance<MultipleProjectsWindow>();

                window.titleContent = new GUIContent(LoreText.MultipleProjects.WindowTitle);
                window.minSize = new Vector2(WelcomeStyle.ProjectsWindowWidth, WelcomeStyle.ProjectsWindowHeight);

                window.ShowUtility();
            };
        }
        #endregion

        #region Unity
        private void OnEnable()
        {
            LoreRefreshService.Refreshed += OnRefreshed;

            BuildRows();
        }

        private void OnDisable()
        {
            LoreRefreshService.Refreshed -= OnRefreshed;
        }

        private void OnGUI()
        {
            // Projects deleted elsewhere (e.g. in the Project window).
            if (_rows.Exists(row => row.Project == null))
                BuildRows();

            GUILayout.Space(WelcomeStyle.ProjectsSpacing);

            GUILayout.Label(
                string.Format(
                    LoreText.MultipleProjects.Message,
                    _rows.Count),
                WelcomeStyle.ProjectsMessage);

            GUILayout.Space(WelcomeStyle.ProjectsSpacing);

            DrawRow(
                LoreText.MultipleProjects.TitleColumn,
                LoreText.MultipleProjects.PathColumn,
                LoreText.MultipleProjects.CategoriesColumn,
                LoreText.MultipleProjects.EntriesColumn,
                WelcomeStyle.ProjectsColumnHeader,
                WelcomeStyle.ProjectsColumnHeader,
                -1);

            for (int i = 0; i < _rows.Count; i++)
            {
                Row row = _rows[i];

                DrawRow(
                    row.Title,
                    row.Folder,
                    row.CategoryCount.ToString(),
                    row.EntryCount.ToString(),
                    WelcomeStyle.ProjectsCell,
                    WelcomeStyle.ProjectsMutedCell,
                    i);
            }

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            bool hasSelection =
                _selectedIndex >= 0 &&
                _selectedIndex < _rows.Count;

            EditorGUI.BeginDisabledGroup(!hasSelection);

            if (GUILayout.Button(
                    new GUIContent(
                        LoreText.MultipleProjects.ExportAndRemoveButton,
                        LoreText.MultipleProjects.ExportAndRemoveTooltip),
                    GUILayout.Height(WelcomeStyle.ActionButtonHeight)))
            {
                Row selected = _rows[_selectedIndex];

                // File dialogs must not run inside OnGUI (layout groups would break).
                EditorApplication.delayCall += () => ExportAndRemove(selected);
            }

            EditorGUI.EndDisabledGroup();

            GUILayout.EndHorizontal();

            GUILayout.Space(WelcomeStyle.ProjectsSpacing);
        }
        #endregion

        #region Draw
        // index -1 = header row (not selectable).
        private void DrawRow(
            string title,
            string folder,
            string categories,
            string entries,
            GUIStyle style,
            GUIStyle folderStyle,
            int index)
        {
            Rect rect =
                GUILayoutUtility.GetRect(
                    0f,
                    WelcomeStyle.ProjectsRowHeight,
                    GUILayout.ExpandWidth(true));

            if (index >= 0 &&
                index == _selectedIndex &&
                Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(rect, WelcomeStyle.ProjectsSelectedRow);
            }

            if (index >= 0)
                EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            float countWidth = WelcomeStyle.ProjectsCountColumnWidth;
            float textWidth = Mathf.Max(0f, rect.width - countWidth * 2f - WelcomeStyle.ProjectsSpacing * 2f);
            float titleWidth = textWidth * WelcomeStyle.ProjectsTitleColumnRatio;

            float x = rect.x + WelcomeStyle.ProjectsSpacing;

            GUI.Label(new Rect(x, rect.y, titleWidth, rect.height), new GUIContent(title, title), style);
            x += titleWidth;

            GUI.Label(new Rect(x, rect.y, textWidth - titleWidth, rect.height), new GUIContent(folder, folder), folderStyle);
            x += textWidth - titleWidth;

            GUI.Label(new Rect(x, rect.y, countWidth, rect.height), categories, style);
            x += countWidth;

            GUI.Label(new Rect(x, rect.y, countWidth, rect.height), entries, style);

            if (index < 0)
                return;

            if (Event.current.type == EventType.MouseDown &&
                Event.current.button == 0 &&
                rect.Contains(Event.current.mousePosition))
            {
                _selectedIndex = index;

                Event.current.Use();

                Repaint();
            }
        }
        #endregion

        #region Commands
        private void ExportAndRemove(Row row)
        {
            if (row.Project == null)
                return;

            // The folder is removed as a whole: never Assets itself.
            if (row.Folder == AssetsFolder)
            {
                EditorUtility.DisplayDialog(
                    LoreText.MultipleProjects.ConfirmTitle,
                    string.Format(LoreText.MultipleProjects.CannotRemoveMessage, row.Title, row.Folder),
                    LoreText.Common.OkButton);

                return;
            }

            string path =
                EditorUtility.SaveFilePanel(
                    LoreText.ProjectPackage.SaveTitle,
                    string.Empty,
                    ProjectPackageService.GetFileName(row.Project.Title),
                    ProjectPackageService.PackageExtension);

            if (string.IsNullOrEmpty(path) ||
                !ProjectPackageService.ExportFolder(row.Folder, path))
            {
                return;
            }

            if (!EditorUtility.DisplayDialog(
                    LoreText.MultipleProjects.ConfirmTitle,
                    string.Format(LoreText.MultipleProjects.ConfirmMessage, row.Title, path, row.Folder),
                    LoreText.MultipleProjects.RemoveButton,
                    LoreText.Common.CancelButton))
            {
                return;
            }

            AssetDatabase.DeleteAsset(row.Folder);

            // Undo steps may point into the deleted Project.
            LoreUndo.ForgetAll();

            FolderService.InvalidateRoot();

            LoreRefreshService.Refresh();
        }

        private void OnRefreshed()
        {
            BuildRows();

            if (_rows.Count <= 1)
            {
                Close();

                return;
            }

            Repaint();
        }

        private void BuildRows()
        {
            _rows.Clear();

            foreach (LoreProject project in LoreRepository.Instance.Projects)
            {
                if (project == null)
                    continue;

                string path = AssetDatabase.GetAssetPath(project);
                int separator = path.LastIndexOf('/');
                string folder = separator > 0 ? path.Substring(0, separator) : AssetsFolder;

                _rows.Add(
                    new Row
                    {
                        Project = project,
                        Title = string.IsNullOrWhiteSpace(project.Title)
                            ? LoreText.MultipleProjects.UntitledProject
                            : project.Title,
                        Folder = folder,
                        CategoryCount = AssetDatabase.FindAssets("t:Category", new[] { folder }).Length,
                        EntryCount = AssetDatabase.FindAssets("t:Entry", new[] { folder }).Length,
                    });
            }

            if (_selectedIndex >= _rows.Count)
                _selectedIndex = -1;
        }
        #endregion
    }
}
