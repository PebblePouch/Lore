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
    // - LoreInlineTextArea.CommitEditing: keyboardControl = 0 + editingTextField = false only
    //   (also called outside OnGUI, where GUI.FocusControl must not be used).
    public static class LoreGUIFocus
    {
        #region Public API
        public static void Clear()
        {
            GUIUtility.keyboardControl = 0;
            GUI.FocusControl(null);
            EditorGUI.FocusTextInControl(null);
        }
        #endregion
    }
}
