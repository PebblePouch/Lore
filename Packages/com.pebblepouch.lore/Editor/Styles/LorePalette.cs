using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Lore's colors in one place. Style classes reference these instead of defining their own values.
    // Only plain Color values live here (no EditorStyles/GUIStyle), so this class is safe to use anytime,
    // including in constructors and OnEnable.
    public static class LorePalette
    {
        #region Text
        public static readonly Color Text = new Color(1f, 1f, 1f);
        public static readonly Color TextStrong = new Color(1f, 1f, 1f, 0.9f);
        public static readonly Color TextSubtle = new Color(1f, 1f, 1f, 0.75f);
        public static readonly Color TextMuted = new Color(1f, 1f, 1f, 0.5f);
        public static readonly Color TextFaint = new Color(1f, 1f, 1f, 0.25f);
        public static readonly Color TextHidden = new Color(1f, 1f, 1f, 0f);

        public static readonly Color Danger = new Color(1f, 0.25f, 0.25f);

        // Text counterpart of WarningSurface (e.g. repairable description links).
        public static readonly Color Warning = new Color(1f, 0.8f, 0.25f);
        #endregion

        #region Surfaces (dark to light)
        public static readonly Color Background = new Color(0.078f, 0.078f, 0.078f);
        public static readonly Color Surface1 = new Color(0.11f, 0.11f, 0.11f);
        public static readonly Color Surface2 = new Color(0.15f, 0.15f, 0.15f);
        public static readonly Color Surface3 = new Color(0.20f, 0.20f, 0.20f);
        public static readonly Color Surface4 = new Color(0.25f, 0.25f, 0.25f);
        public static readonly Color Surface5 = new Color(0.30f, 0.30f, 0.30f);

        // Only used for menu row hover; could later be unified with Surface4 or Surface5.
        public static readonly Color MenuRowHover = new Color(0.28f, 0.28f, 0.28f);
        #endregion

        #region Categories
        // Fallback for items without a Category (same as Unity's Color.gray).
        public static readonly Color NoCategory = new Color(0.5f, 0.5f, 0.5f);
        #endregion

        #region States
        // Selected / active items (cards, assignment targets, dialog header).
        public static readonly Color Accent = new Color(0.20f, 0.30f, 0.60f);

        // Rule: red-ish DangerSurface marks integrity issues (broken Fields, links, references: something is wrong),
        // yellow-ish WarningSurface marks health issues (required value empty, empty Entries list: unfinished content).
        // Where both apply, integrity wins. MatchSurface marks matches (field filter).
        public static readonly Color DangerSurface = new Color(0.25f, 0.15f, 0.15f);
        public static readonly Color WarningSurface = new Color(0.25f, 0.22f, 0.12f);
        public static readonly Color MatchSurface = new Color(0.15f, 0.25f, 0.15f);

        // Destructive buttons.
        public static readonly Color DangerStrong = new Color(0.60f, 0.20f, 0.10f);
        public static readonly Color DangerStrongHover = new Color(0.60f, 0.30f, 0.20f);
        #endregion

        #region Overlays and Lines
        public static readonly Color HoverOverlay = new Color(1f, 1f, 1f, 0.1f);
        public static readonly Color Divider = new Color(1f, 1f, 1f, 0.1f);

        // Drag & drop insertion line.
        public static readonly Color DropIndicator = new Color(1f, 1f, 1f, 0.5f);

        public static readonly Color Disabled = new Color(1f, 1f, 1f, 0.25f);

        public static readonly Color Shadow = new Color(0f, 0f, 0f, 0.2f);
        public static readonly Color Clear = new Color(0f, 0f, 0f, 0f);
        #endregion
    }
}
