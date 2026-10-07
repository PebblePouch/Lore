using System;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Shown instead of Home while the Unity project has no Lore Project. Nothing is created until the user picks
    // one of the actions (Start your Lore / Import Unity Package / Create Example Project); as soon as a Project
    // exists, the window shows Home again. Read-only: same cards and texts as the Project page, no editing.
    public sealed class WelcomeInspectorPanel
    {
        #region Fields
        private readonly WorkspaceState _state;

        private readonly LoreInlineTextArea _gettingStartedText = new();
        private readonly LoreInlineTextArea _manualText = new();

        private readonly InsightsRowHeight _actionsRowHeight = new();
        private readonly InsightsRowHeight _aboutRowHeight = new();

        private float _availableRowWidth;

        // A folder picked with this name is used as it is (no "Lore/Lore").
        private const string LoreFolderName = "Lore";
        private const string AssetsFolder = "Assets";

        private const string GitHubUrl = "https://github.com/PebblePouch/Lore";
        private const string ChangelogUrl =
            "https://github.com/PebblePouch/Lore/blob/main/Packages/com.pebblepouch.lore/CHANGELOG.md";
        private const string LicenseUrl = "https://github.com/PebblePouch/Lore#license";
        private const string PatreonUrl = "https://www.patreon.com/c/pebblepouch";
        #endregion

        #region Constructor
        public WelcomeInspectorPanel(WorkspaceState state)
        {
            _state = state;
        }
        #endregion

        #region Public API
        // windowWidth: the first frame has no measured row width yet; Unity may not repaint again soon (e.g. a
        // compile starts right after the last Project was deleted), so start from the window width.
        public void Draw(float windowWidth)
        {
            if (_availableRowWidth <= 0f)
            {
                GUIStyle scrollbar = GUI.skin.verticalScrollbar;

                _availableRowWidth =
                    Mathf.Max(
                        0f,
                        windowWidth - scrollbar.fixedWidth - scrollbar.margin.horizontal);
            }

            _state.RightScroll =
                EditorLayout.ScrollView(
                    _state.RightScroll,
                    () =>
                    {
                        DrawHeader();

                        DrawGettingStarted();

                        InsightsRow.UpdateAvailableWidth(
                            ref _availableRowWidth);

                        DrawActions();

                        GUILayout.Space(InsightsStyle.RowSpacing);

                        DrawManual();

                        DrawAbout();
                    });
        }
        #endregion

        #region Draw
        // Same place and look as the Project title, without editing, timestamps or menu.
        private static void DrawHeader()
        {
            LoreInlineTextFieldSettings settings = InspectorStyle.ProjectTitle;

            EditorGUILayout.BeginHorizontal(
                InspectorStyle.HeaderLayout,
                GUILayout.Height(
                    InspectorStyle.HeaderHeight));

            Rect rect =
                GUILayoutUtility.GetRect(
                    0f,
                    settings.Height,
                    GUILayout.ExpandWidth(true));

            GUI.Label(
                new Rect(
                    rect.x + settings.HorizontalInset,
                    rect.y + settings.VerticalInset,
                    rect.width - settings.HorizontalInset * 2,
                    rect.height - settings.VerticalInset * 2),
                LoreText.Welcome.Title,
                settings.ViewerContent);

            EditorGUILayout.EndHorizontal();
        }

        // Row 1: like the Project description.
        private void DrawGettingStarted()
        {
            SectionCard.DrawCollapsible(
                LoreText.Welcome.GettingStartedHeader,
                _state.IsInspectorSectionExpanded(InspectorSection.WelcomeGettingStarted),
                () => _state.ToggleInspectorSection(InspectorSection.WelcomeGettingStarted),
                () =>
                    _gettingStartedText.DrawReadOnly(
                        LoreText.Welcome.GettingStartedText,
                        WelcomeStyle.GettingStarted));
        }

        // Row 2: Create | Continue | Demo (collapse together, like Insights rows).
        private void DrawActions()
        {
            bool expanded = _state.IsInspectorSectionExpanded(InspectorSection.WelcomeActions);

            Action toggle = () => _state.ToggleInspectorSection(InspectorSection.WelcomeActions);

            LoreTheme theme = LoreEditorTheme.Current;

            InsightsRow.Draw(
                _availableRowWidth,
                _actionsRowHeight,
                () =>
                    DrawActionCard(
                        LoreText.Welcome.CreateHeader,
                        expanded,
                        toggle,
                        theme.StartLore,
                        LoreText.Welcome.CreateInfo,
                        StartLore),
                () =>
                    DrawActionCard(
                        LoreText.Welcome.ContinueHeader,
                        expanded,
                        toggle,
                        theme.ImportProject,
                        LoreText.Welcome.ContinueInfo,
                        ProjectPackageService.Import),
                () =>
                    DrawActionCard(
                        LoreText.Welcome.DemoHeader,
                        expanded,
                        toggle,
                        theme.CreateDemo,
                        LoreText.Welcome.DemoInfo,
                        DemoSampleService.Import));
        }

        private void DrawActionCard(
            string header,
            bool expanded,
            Action toggle,
            ButtonDefinition button,
            string info,
            Action action)
        {
            SectionCard.DrawRowCollapsible(
                header,
                expanded,
                toggle,
                () =>
                {
                    GUILayout.Space(WelcomeStyle.SectionTopSpacing);

                    // File dialogs and imports must not run inside this OnGUI (layout groups would break).
                    if (GUILayout.Button(
                            button.GetContent(ButtonDrawer.DisplayMode.GlyphName),
                            WelcomeStyle.ActionButton,
                            GUILayout.MinWidth(0f)))
                    {
                        EditorApplication.delayCall += () => action();
                    }

                    EditorGUIUtility.AddCursorRect(
                        GUILayoutUtility.GetLastRect(),
                        MouseCursor.Link);

                    GUILayout.Space(WelcomeStyle.ActionInfoSpacing);

                    GUILayout.Label(
                        info,
                        WelcomeStyle.ActionInfo,
                        GUILayout.MinWidth(0f));
                },
                InsightsStyle.Card,
                _actionsRowHeight);
        }

        // Row 3: like the Project description, with a taller viewport.
        private void DrawManual()
        {
            SectionCard.DrawCollapsible(
                LoreText.Welcome.ManualHeader,
                _state.IsInspectorSectionExpanded(InspectorSection.WelcomeManual),
                () => _state.ToggleInspectorSection(InspectorSection.WelcomeManual),
                () =>
                {
                    string manual = LoreManual.GetRichText();

                    if (manual != null)
                    {
                        _manualText.DrawReadOnly(
                            manual,
                            WelcomeStyle.Manual);
                    }
                    else
                    {
                        GUILayout.Label(
                            LoreText.Welcome.ManualMissing,
                            InspectorStyle.LinkEmpty);
                    }
                });
        }

        // Row 4: Links | Credits.
        private void DrawAbout()
        {
            bool expanded = _state.IsInspectorSectionExpanded(InspectorSection.WelcomeAbout);

            Action toggle = () => _state.ToggleInspectorSection(InspectorSection.WelcomeAbout);

            InsightsRow.Draw(
                _availableRowWidth,
                _aboutRowHeight,
                () =>
                    SectionCard.DrawRowCollapsible(
                        LoreText.Welcome.LinksHeader,
                        expanded,
                        toggle,
                        DrawLinks,
                        InsightsStyle.Card,
                        _aboutRowHeight),
                () =>
                    SectionCard.DrawRowCollapsible(
                        LoreText.Welcome.CreditsHeader,
                        expanded,
                        toggle,
                        () =>
                        {
                            GUILayout.Space(WelcomeStyle.SectionTopSpacing);

                            GUILayout.Label(
                                LoreText.Welcome.CreditsText,
                                WelcomeStyle.Text,
                                GUILayout.MinWidth(0f));
                        },
                        InsightsStyle.Card,
                        _aboutRowHeight));
        }

        // Same rows as the Project Links table (names only), without handles, URLs and buttons.
        private static void DrawLinks()
        {
            GUILayout.Space(WelcomeStyle.SectionTopSpacing);

            DrawLink(LoreText.Welcome.GitHubLink, GitHubUrl, true);
            DrawLink(LoreText.Welcome.ChangelogLink, ChangelogUrl, true);
            DrawLink(LoreText.Welcome.LicenseLink, LicenseUrl, true);
            DrawLink(LoreText.Welcome.PatreonLink, PatreonUrl, false);
        }

        private static void DrawLink(
            string label,
            string url,
            bool drawSeparator)
        {
            EditorGUILayout.BeginHorizontal(
                GUILayout.Height(
                    InspectorStyle.FieldTableRowHeight));

            GUILayout.Label(
                new GUIContent(
                    label,
                    string.Format(
                        LoreText.Inspector.OpenLinkTooltip,
                        url)),
                InspectorStyle.LinkLabel,
                GUILayout.MinWidth(0f),
                GUILayout.ExpandWidth(false),
                GUILayout.Height(
                    InspectorStyle.FieldTableRowHeight));

            Rect labelRect = GUILayoutUtility.GetLastRect();

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();

            EditorGUIUtility.AddCursorRect(
                labelRect,
                MouseCursor.Link);

            if (Event.current.type == EventType.MouseDown &&
                Event.current.button == 0 &&
                labelRect.Contains(Event.current.mousePosition))
            {
                LoreUrl.Open(url);

                Event.current.Use();
            }

            if (!drawSeparator)
                return;

            Rect separator =
                GUILayoutUtility.GetRect(
                    1f,
                    1f,
                    GUILayout.ExpandWidth(true));

            EditorGUI.DrawRect(
                separator,
                InspectorStyle.FieldRowLineColor);
        }
        #endregion

        #region Commands
        // Asks for the parent folder (inside Assets) and creates "<folder>/Lore" with the Project in it.
        private static void StartLore()
        {
            string picked =
                EditorUtility.OpenFolderPanel(
                    LoreText.Welcome.ChooseFolderTitle,
                    Application.dataPath,
                    string.Empty);

            if (string.IsNullOrEmpty(picked))
                return;

            string assetPath = ToAssetPath(picked);

            if (assetPath == null)
            {
                EditorUtility.DisplayDialog(
                    LoreText.Welcome.FolderOutsideAssetsTitle,
                    LoreText.Welcome.FolderOutsideAssetsMessage,
                    LoreText.Common.OkButton);

                return;
            }

            string root =
                assetPath.EndsWith("/" + LoreFolderName, StringComparison.Ordinal)
                    ? assetPath
                    : assetPath + "/" + LoreFolderName;

            if (new ProjectService().CreateProject(root) == null)
                return;

            SelectionService.SelectProject();

            LoreRepaintService.Repaint();
        }

        // "C:/.../MyGame/Assets/Game" → "Assets/Game"; null for folders outside Assets.
        private static string ToAssetPath(string absolutePath)
        {
            string path = absolutePath.Replace('\\', '/').TrimEnd('/');
            string dataPath = Application.dataPath.Replace('\\', '/').TrimEnd('/');

            if (string.Equals(path, dataPath, StringComparison.OrdinalIgnoreCase))
                return AssetsFolder;

            if (!path.StartsWith(dataPath + "/", StringComparison.OrdinalIgnoreCase))
                return null;

            return AssetsFolder + path.Substring(dataPath.Length);
        }
        #endregion
    }
}
