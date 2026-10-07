using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public static partial class WindowStyle
    {
        // Technical: Unity's command name for Ctrl+F / Cmd+F (opens Quick Open).
        public const string FindCommandName = "Find";

        // Technical: mouse position used to draw content "without mouse" (under Quick Open).
        public static readonly Vector2 HiddenMousePosition = new(-100000f, -100000f);

        // ...Layout to define padding, margin, width and height (do not use fixedWidth/Height)
        // ...Content to define content behavior: fontSize, color, decorations, alignment
        //
        // Order of Content Definitions:
        //
        // padding - inner distance
        // margin - outer distance
        //
        // fontSize
        // fontStyle
        // alignment
        // wordWrap
        //
        // normal = { textColor = LorePalette.Text },
        // hover = { textColor = LorePalette.Text },
        // active = { textColor = LorePalette.Text },
        //

        #region Tab Icon
        // Built-in editor icons (placeholder until Lore has its own): first one found wins.
        // Dark skin uses the "d_" variants.
        public static readonly string[] TabIconNames = { "Collab", "CustomTool" };
        public const string DarkSkinIconPrefix = "d_";
        #endregion

        #region Layout
        public const float DefaultWindowWidth = 1280f;
        public const float DefaultWindowHeight = 720f;

        public const float BrowserWidth = 300f;

        public const float PanelHorizontalSpacing = 3f;

        public const float KeyboardFocusIndicatorHeight = 1f;
        #endregion

        #region Colors
        public static readonly Color PanelSpacingColor = LorePalette.Background;

        public static readonly Color KeyboardFocusIndicatorActive = LorePalette.Text;
        public static readonly Color KeyboardFocusIndicatorInactive = LorePalette.Background;

        public static readonly Color ToolbarBackground = LorePalette.Surface3;
        public static readonly Color BrowserBackground = LorePalette.Background;
        public static readonly Color InspectorBackground = LorePalette.Background;
        #endregion

        #region Toolbar
        public static readonly GUIStyle ToolbarPanel = new()
        {
            padding = new RectOffset(0, 0, 0, 0),   // Inner
            margin = new RectOffset(0, 0, 0, 0),    // Outer
        };
        #endregion

        #region Browser
        public static readonly GUIStyle BrowserPanel = new()
        {
            padding = new RectOffset(0, 0, 0, 0),   // Inner
            margin = new RectOffset(0, 0, 0, 0),    // Outer
        };
        #endregion

        #region Inspector
        public static readonly GUIStyle InspectorPanel = new()
        {
            padding = new RectOffset(0, 0, 0, 0),   // Inner
            margin = new RectOffset(0, 0, 0, 0),    // Outer
        };
        #endregion
    }
}