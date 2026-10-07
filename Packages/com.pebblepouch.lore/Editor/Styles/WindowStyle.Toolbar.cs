namespace PebblePouch.Lore.Editor
{
    public static partial class WindowStyle
    {
        #region Toolbar
        public const string ToolbarBackText = "◀";
        public const float ToolbarBackWidth = 24f;

        public const string ToolbarForwardText = "▶";
        public const float ToolbarForwardWidth = 24f;

        // Same width as the Lore window tab (icon + title) at 100% display scaling. Adjust when the tab icon or
        // title changes (measuring the tab via its GUIStyle came out 7px too narrow).
        public const float ToolbarHomeWidth = 66f;

        public const float ToolbarQuickSearchWidth = 90f;
        #endregion
    }
}
