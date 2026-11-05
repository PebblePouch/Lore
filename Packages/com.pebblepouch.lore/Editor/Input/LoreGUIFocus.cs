using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Clears Unity's IMGUI keyboard/text focus (not Lore's panel focus, see LoreKeyboardFocus).
    //
    // Known variants that intentionally still clear focus differently, left untouched until the
    // title-field click issue is investigated (they are part of that behavior):
    // - LoreInlineTextField, Category/Entry inspector panels, SearchCard:
    //   FocusControl(null) + FocusTextInControl(null), without resetting keyboardControl.
    // - LibraryInlineTextField: keyboardControl = 0 + FocusControl(null), without FocusTextInControl.
    //
    // Keyboard focus and the text-editing state are shared by all editor windows. Code that can run outside the
    // Lore window's OnGUI (undo callbacks, selection changes) uses Release, so it never clears the focus of the
    // window the user is typing in.
    public static class LoreGUIFocus
    {
        #region Fields
        // > 0 while the Lore window's OnGUI runs (BeginWindowGUI / EndWindowGUI).
        private static int _windowGUIDepth;
        private static bool _releasePending;
        #endregion

        #region Public API
        public static void Clear()
        {
            GUIUtility.keyboardControl = 0;
            GUI.FocusControl(null);
            EditorGUI.FocusTextInControl(null);
        }

        // Ends text editing like Escape does. Inside the Lore window's OnGUI: at once. Elsewhere: on the Lore
        // window's next OnGUI, where keyboardControl belongs to the Lore window only. The shared text-editing flag
        // is only reset while a Lore window has the keyboard; otherwise it belongs to another window.
        public static void Release()
        {
            if (_windowGUIDepth > 0)
            {
                ReleaseNow();

                return;
            }

            _releasePending = true;
        }

        // Called by LoreWindow around its OnGUI (try/finally: ExitGUI throws).
        public static void BeginWindowGUI()
        {
            _windowGUIDepth++;

            if (!_releasePending)
                return;

            _releasePending = false;

            ReleaseNow();
        }

        public static void EndWindowGUI()
        {
            if (_windowGUIDepth > 0)
                _windowGUIDepth--;
        }

        // True if the focused editor window is one of Lore's (Lore window, Quick Search, Lore dialogs).
        public static bool IsLoreWindowFocused()
        {
            EditorWindow focused =
                EditorWindow.focusedWindow;

            return focused != null &&
                   focused.GetType().Assembly == typeof(LoreGUIFocus).Assembly;
        }
        #endregion

        #region Helpers
        private static void ReleaseNow()
        {
            GUIUtility.keyboardControl = 0;

            if (IsLoreWindowFocused())
                EditorGUIUtility.editingTextField = false;
        }
        #endregion
    }
}
