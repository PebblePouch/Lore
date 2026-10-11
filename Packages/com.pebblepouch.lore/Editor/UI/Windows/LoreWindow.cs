using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public partial class LoreWindow : EditorWindow
    {
        #region Fields
        private readonly WorkspaceState _state = new();

        private ContentBrowserModel _browserModel;
        private ContentBrowser _browser;
        private WorkspaceToolbar _toolbar;

        private ProjectInspectorPanel _projectInspector;
        private CategoryInspectorPanel _categoryInspector;
        private EntryInspectorPanel _entryInspector;
        private WelcomeInspectorPanel _welcomeInspector;

        private LibraryHost _libraryHost;

        // No Lore Project: the Welcome page replaces toolbar, browser and Home. Decided during Layout only, so
        // Layout and the following events draw the same controls.
        private bool _showWelcome;

        private const string DefaultSizeInitializedKey = "Lore.DefaultSizeInitialized";
        #endregion

        #region Properties
        public static LoreWindow Current { get; private set; }
        #endregion

        #region Public API
        [MenuItem(LoreMenu.Open, false, LoreMenu.OpenPriority)]
        public static void Open()
        {
            // No title argument: it would replace the title (with icon) set in OnEnable.
            LoreWindow window = GetWindow<LoreWindow>();

            if (EditorPrefs.GetBool(
                    DefaultSizeInitializedKey,
                    false))
            {
                return;
            }

            if (window.docked)
                return;

            Rect position = window.position;
            position.width = WindowStyle.DefaultWindowWidth;
            position.height = WindowStyle.DefaultWindowHeight;
            window.position = position;

            EditorPrefs.SetBool(
                DefaultSizeInitializedKey,
                true);
        }
        #endregion

        #region Unity
        private void OnEnable()
        {
            Current = this;

            titleContent = new GUIContent(
                LoreText.Window.Title,
                FindTabIcon());

            wantsMouseMove = true;
            wantsMouseEnterLeaveWindow = true;

            // TOOLBAR
            _toolbar =
                new WorkspaceToolbar(
                    _state,
                    GoBack,
                    GoForward,
                    GoHome,
                    () => QuickSearchWindow.Open(position));

            // CONTENT BROWSER
            _browserModel = new ContentBrowserModel(LoreRepository.Instance);

            _browser = new ContentBrowser(
                _state,
                _browserModel,
                Repaint,
                () => _libraryHost != null && _libraryHost.IsOpen);

            // LIBRARY HOST
            _libraryHost = new LibraryHost(Repaint);

            // INSPECTORS
            _projectInspector = new ProjectInspectorPanel(
                _state);

            _categoryInspector = new CategoryInspectorPanel(
                _state,
                Repaint,
                _libraryHost.ShowFields,
                _libraryHost.ShowFieldForEdit);

            _entryInspector = new EntryInspectorPanel(
                _state,
                Repaint,
                _libraryHost.ShowTags,
                _libraryHost.ShowTagForEdit);

            _welcomeInspector = new WelcomeInspectorPanel(
                _state);

            _showWelcome = LoreRepository.Instance.Project == null;

            // LIBRARY UPDATE: asks once after a Lore update with new default Libraries (waits for compile/import).
            LibraryUpdateService.CheckWhenReady();

            // LISTENERS
            SelectionService.CategoryChanged += OnCategoryChanged;
            SelectionService.EntryChanged += OnEntryChanged;

            // LOAD SELECTION
            Entry selectedEntry = WorkspacePersistence.LoadEntry();
            Category selectedCategory = WorkspacePersistence.LoadCategory();

            if (selectedEntry != null)
            {
                SelectionService.SelectEntry(selectedEntry);

                ContentBrowserRequestService.RequestEntryReveal(selectedEntry);
            }
            else if (selectedCategory != null)
            {
                SelectionService.SelectCategory(selectedCategory);

                ContentBrowserRequestService.RequestCategoryReveal(selectedCategory);
            }
            else
            {
                SelectionService.SelectProject();
            }

            // KEYBOARD OWNER: the Inspector owns the keyboard after opening (Project, Category or Entry).
            FocusInspectorKeyboard();
        }

        private void OnDisable()
        {
            if (Current == this)
            {
                Current = null;
            }

            WorkspacePersistence.SaveCategory(SelectionService.CurrentCategory);
            WorkspacePersistence.SaveEntry(SelectionService.CurrentEntry);

            _projectInspector?.Dispose();
            _entryInspector?.Dispose();
            _categoryInspector?.Dispose();
            _libraryHost?.Dispose();

            SelectionService.CategoryChanged -= OnCategoryChanged;
            SelectionService.EntryChanged -= OnEntryChanged;
        }
        #endregion

        #region Unity
        private void OnGUI()
        {
            LoreHover.BeginGUI();

            RepaintOnHoverChange();

            Rect windowRect =
                new(
                    0f,
                    0f,
                    position.width,
                    position.height);

            if (Event.current.type == EventType.Layout)
                _showWelcome = LoreRepository.Instance.Project == null;

            MultipleProjectsWindow.OpenIfNeeded();

            if (_showWelcome)
            {
                DrawWelcomePanel();

                LoreHover.EndGUI();

                return;
            }

            HandleFindCommand();

            LoreActiveTextEditor.HandleTextBoundaryKeys();

            LoreDialogSurface.HandleInput(windowRect);

            // While Quick Search (its own popup window) is open, this window is disabled and dimmed.
            bool quickOpen = QuickSearchWindow.IsOpen;

            if (!quickOpen)
            {
                HandleHistoryKeyboardNavigation();

                HandleInspectorKeyboardNavigation();
            }

            EditorGUI.BeginDisabledGroup(quickOpen);

            // While Quick Search is open, the content is drawn as if the mouse were outside the window:
            // no hover, tooltips or clicks reach it (disabled controls still show tooltips otherwise).
            Vector2 mousePosition = Event.current.mousePosition;

            if (quickOpen)
                Event.current.mousePosition = WindowStyle.HiddenMousePosition;

            DrawToolbarPanel();

            Rect panelRowRect = EditorGUILayout.BeginHorizontal();

            EditorGUI.DrawRect(
                panelRowRect,
                WindowStyle.PanelSpacingColor);

            DrawBrowserPanel();

            GUILayout.Space(WindowStyle.PanelHorizontalSpacing);

            DrawInspectorPanel();

            if (_libraryHost.IsOpen)
            {
                GUILayout.Space(WindowStyle.PanelHorizontalSpacing);

                DrawContextPanel();
            }

            EditorGUILayout.EndHorizontal();

            if (quickOpen)
                Event.current.mousePosition = mousePosition;

            EditorGUI.EndDisabledGroup();

            LoreDialogSurface.Draw(windowRect);

            if (quickOpen &&
                Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(windowRect, QuickOpenStyle.Blocker);
            }

            LoreHover.EndGUI();
        }
        #endregion

        #region Draw
        private void DrawToolbarPanel()
        {
            Rect rect =
                EditorGUILayout.BeginVertical(
                    WindowStyle.ToolbarPanel,
                    GUILayout.ExpandWidth(true));

            EditorGUI.DrawRect(
                rect,
                WindowStyle.ToolbarBackground);

            _toolbar.Draw();

            EditorGUILayout.EndVertical();
        }

        private void DrawBrowserPanel()
        {
            Rect rect =
                EditorGUILayout.BeginVertical(
                    WindowStyle.BrowserPanel,
                    GUILayout.Width(WindowStyle.BrowserWidth),
                    GUILayout.ExpandHeight(true));

            HandleContentKeyboardFocus(rect);

            EditorGUI.DrawRect(
                rect,
                WindowStyle.BrowserBackground);

            DrawKeyboardFocusIndicator(LoreKeyboardOwner.Content);

            _browser.SetAvailableHeight(
                Mathf.Max(
                    0f,
                    rect.height -
                    WindowStyle.KeyboardFocusIndicatorHeight));

            _browser.Draw();

            EditorGUILayout.EndVertical();
        }

        private void DrawInspectorPanel()
        {
            Rect rect =
                EditorGUILayout.BeginVertical(
                    WindowStyle.InspectorPanel,
                    GUILayout.MinWidth(0f),
                    GUILayout.ExpandWidth(true),
                    GUILayout.ExpandHeight(true));

            HandleInspectorKeyboardFocus(rect);

            EditorGUI.DrawRect(
                rect,
                WindowStyle.InspectorBackground);

            DrawKeyboardFocusIndicator(LoreKeyboardOwner.Inspector);

            if (Event.current.type ==
                EventType.Repaint)
            {
                _state.RightViewportHeight =
                    Mathf.Max(
                        0f,
                        rect.height -
                        WindowStyle.KeyboardFocusIndicatorHeight);
            }

            if (SelectionService.CurrentEntry != null)
            {
                _entryInspector.Draw();
            }
            else if (SelectionService.CurrentCategory != null)
            {
                _categoryInspector.Draw();
            }
            else
            {
                _projectInspector.Draw();
            }

            EditorGUILayout.EndVertical();
        }

        // The whole window: no toolbar (nothing to navigate) and no browser (nothing to browse yet).
        private void DrawWelcomePanel()
        {
            Rect rect =
                EditorGUILayout.BeginVertical(
                    WindowStyle.InspectorPanel,
                    GUILayout.MinWidth(0f),
                    GUILayout.ExpandWidth(true),
                    GUILayout.ExpandHeight(true));

            EditorGUI.DrawRect(
                rect,
                WindowStyle.InspectorBackground);

            _welcomeInspector.Draw(position.width);

            EditorGUILayout.EndVertical();
        }

        private void DrawContextPanel()
        {
            if (!_libraryHost.IsOpen)
                return;

            Rect rect =
                EditorGUILayout.BeginVertical(
                    WindowStyle.BrowserPanel,
                    GUILayout.Width(
                        WindowStyle.BrowserWidth),
                    GUILayout.ExpandHeight(true));

            EditorGUI.DrawRect(
                rect,
                WindowStyle.BrowserBackground);

            DrawKeyboardFocusIndicator(
                LoreKeyboardOwner.Library);

            _libraryHost.Draw(
                Mathf.Max(
                    0f,
                    rect.height -
                    WindowStyle.KeyboardFocusIndicatorHeight));

            EditorGUILayout.EndVertical();
        }
        #endregion

        #region Listeners
        private void OnCategoryChanged(Category category)
        {
            Repaint();
        }

        private void OnEntryChanged(Entry entry)
        {
            Repaint();
        }
        #endregion

        #region Helpers
        // Ctrl+F / Cmd+F: Unity sends the "Find" command to the focused window; Lore opens Quick Search
        // (not while a dialog is open).
        private void HandleFindCommand()
        {
            Event current = Event.current;

            if ((current.type != EventType.ValidateCommand &&
                 current.type != EventType.ExecuteCommand) ||
                current.commandName != WindowStyle.FindCommandName ||
                LoreDialog.IsOpen)
            {
                return;
            }

            if (current.type == EventType.ExecuteCommand)
                QuickSearchWindow.Open(position);

            current.Use();
        }

        // Performance: a full repaint only when a hover highlight changes (see LoreHover), not on every mouse move.
        private void RepaintOnHoverChange()
        {
            EventType type = Event.current.type;

            bool repaint =
                type == EventType.MouseMove && LoreHover.HoverChanged() ||
                type == EventType.MouseLeaveWindow && LoreHover.HasHover;

            if (repaint)
                Repaint();
        }

        private void GoHome()
        {
            if (_libraryHost.IsOpen &&
                !_libraryHost.TryClose())
            {
                return;
            }

            _browser.ClearFilter();

            _libraryHost.ClearFilters();

            _browser.ReleaseSearchFocus();

            SelectionService.SelectProject();

            Repaint();
        }

        // First built-in tab icon that exists in this Unity version (skin variant), or null (text only).
        private static Texture FindTabIcon()
        {
            string prefix = EditorGUIUtility.isProSkin
                ? WindowStyle.DarkSkinIconPrefix
                : string.Empty;

            foreach (string name in WindowStyle.TabIconNames)
            {
                Texture icon = EditorGUIUtility.FindTexture(prefix + name);

                if (icon == null)
                    icon = EditorGUIUtility.FindTexture(name);

                if (icon != null)
                    return icon;
            }

            return null;
        }

        private static void DrawKeyboardFocusIndicator(LoreKeyboardOwner owner)
        {
            float height = WindowStyle.KeyboardFocusIndicatorHeight;

            Rect rect =
                GUILayoutUtility.GetRect(
                    0f,
                    height,
                    GUILayout.ExpandWidth(true),
                    GUILayout.Height(height));

            if (height <= 0f)
                return;

            Color color =
                LoreKeyboardFocus.IsOwner(owner)
                    ? WindowStyle.KeyboardFocusIndicatorActive
                    : WindowStyle.KeyboardFocusIndicatorInactive;

            EditorGUI.DrawRect(rect, color);
        }
        #endregion
    }
}