using System;
using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class WorkspaceToolbar
    {
        #region Fields
        private readonly Action _back;
        private readonly Action _forward;
        private readonly Action _home;
        private readonly Action _quickSearch;
        #endregion

        #region Public API
        public WorkspaceToolbar(
            WorkspaceState state,
            Action back,
            Action forward,
            Action home,
            Action quickSearch)
        {
            _back = back;
            _forward = forward;
            _home = home;
            _quickSearch = quickSearch;
        }

        public void Draw()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            DrawHome();

            DrawHistory();

            GUILayout.FlexibleSpace();

            DrawQuickSearch();

            GUILayout.EndHorizontal();
        }
        #endregion

        #region Draw
        private void DrawHistory()
        {
            using (new EditorGUI.DisabledScope(
                       !NavigationHistoryService.CanGoBack))
            {
                if (GUILayout.Button(
                        new GUIContent(
                            WindowStyle.ToolbarBackText,
                            LoreText.Window.BackTooltip),
                        EditorStyles.toolbarButton,
                        GUILayout.Width(
                            WindowStyle.ToolbarBackWidth)))
                {
                    _back?.Invoke();
                }
            }

            using (new EditorGUI.DisabledScope(
                       !NavigationHistoryService.CanGoForward))
            {
                if (GUILayout.Button(
                        new GUIContent(
                            WindowStyle.ToolbarForwardText,
                            LoreText.Window.ForwardTooltip),
                        EditorStyles.toolbarButton,
                        GUILayout.Width(
                            WindowStyle.ToolbarForwardWidth)))
                {
                    _forward?.Invoke();
                }
            }
        }

        private void DrawHome()
        {
            if (GUILayout.Button(
                    LoreText.Window.HomeButton,
                    EditorStyles.toolbarButton,
                    GUILayout.Width(
                        WindowStyle.ToolbarHomeWidth)))
            {
                _home?.Invoke();
            }
        }

        // Same as Ctrl+F / Cmd+F.
        private void DrawQuickSearch()
        {
            if (GUILayout.Button(
                    new GUIContent(
                        LoreText.Window.QuickSearchButton,
                        LoreText.Window.QuickSearchTooltip),
                    EditorStyles.toolbarButton,
                    GUILayout.Width(WindowStyle.ToolbarQuickSearchWidth)))
            {
                _quickSearch?.Invoke();
            }
        }
        #endregion
    }
}