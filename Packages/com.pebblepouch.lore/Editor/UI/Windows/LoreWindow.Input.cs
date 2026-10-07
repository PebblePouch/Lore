using System;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Input handling of the Lore window: navigation history (toolbar + shortcuts),
    // Inspector keyboard navigation, and keyboard focus between the panels.
    public partial class LoreWindow
    {
        #region History
        private void GoBack()
        {
            NavigateHistory(NavigationHistoryService.GoBack);
        }

        private void GoForward()
        {
            NavigateHistory(NavigationHistoryService.GoForward);
        }

        private void NavigateHistory(Func<bool> step)
        {
            // Libraries are on-request context panels: a history step closes them like Home or Esc
            // (keyboard focus returns to the main frame). Unsaved library edits that cannot be
            // committed block the step.
            if (_libraryHost.IsOpen &&
                !_libraryHost.TryClose())
            {
                return;
            }

            // Selecting hands focus to the Inspector; keep it in the Content browser if it started there.
            bool fromContent =
                LoreKeyboardFocus.IsOwner(
                    LoreKeyboardOwner.Content);

            _browser.ReleaseSearchFocus();

            step();

            // Refocus even when there was no step left, so the search cursor never disappears.
            if (fromContent)
            {
                _browser.FocusNavigationOnCurrentSelection();
            }

            Repaint();
        }

        private void HandleHistoryKeyboardNavigation()
        {
            Event current = Event.current;

            if (current.type != EventType.KeyDown)
                return;

            bool alt = current.alt && !current.control && !current.shift;
            bool plain = !current.alt && !current.control && !current.shift;

            if (LoreDialog.IsOpen)
                return;

            // Inline editors keep all keys.
            if (IsInspectorEditing())
                return;

            // editingTextField alone can stay stale after inline edits; require an actually focused control.
            bool isTextFocused =
                GUIUtility.keyboardControl != 0 &&
                EditorGUIUtility.editingTextField;

            // The Content browser keeps its search focused while navigating (type-to-search).
            bool isContentSearchFocused =
                isTextFocused &&
                GUI.GetNameOfFocusedControl() == ContentBrowser.SearchControlName;

            // Library searches also stay focused (type-to-search): Alt+Left/Right browse history,
            // Backspace always stays a text key there (unlike the empty Content browser search).
            bool isLibrarySearchFocused =
                isTextFocused &&
                GUI.GetNameOfFocusedControl().StartsWith(
                    BrowserStyle.LibrarySearchControlNamePrefix,
                    StringComparison.Ordinal);

            bool canUseAltArrows =
                !isTextFocused ||
                isContentSearchFocused ||
                isLibrarySearchFocused;

            bool libraryOwnsKeyboard =
                _libraryHost.IsOpen &&
                LoreKeyboardFocus.IsOwner(
                    LoreKeyboardOwner.Library);

            bool canUseBackspace =
                !libraryOwnsKeyboard &&
                (!isTextFocused ||
                 (isContentSearchFocused && string.IsNullOrEmpty(_browserModel.Search)));

            if ((alt && current.keyCode == KeyCode.LeftArrow && canUseAltArrows) ||
                (plain && current.keyCode == KeyCode.Backspace && canUseBackspace))
            {
                GoBack();

                current.Use();

                return;
            }

            if (alt && current.keyCode == KeyCode.RightArrow && canUseAltArrows)
            {
                GoForward();

                current.Use();
            }
        }

        private bool IsInspectorEditing()
        {
            return SelectionService.CurrentEntry != null
                ? _entryInspector.IsEditing
                : SelectionService.CurrentCategory != null
                    ? _categoryInspector.IsEditing
                    : _projectInspector.IsEditing;
        }
        #endregion

        #region Inspector Keyboard and Focus
        private void HandleInspectorKeyboardNavigation()
        {
            if (!LoreKeyboardFocus.IsOwner(LoreKeyboardOwner.Inspector))
            {
                return;
            }

            Event current = Event.current;

            if (current.type != EventType.KeyDown)
                return;

            if (IsInspectorEditing())
                return;

            if (GUIUtility.keyboardControl != 0)
            {
                // A control keeps the focus after a click or Enter (checkbox, popup, text field no longer
                // editing): Left still hands the focus to the browser. While a text field is editing, Left
                // moves the caret.
                if (current.keyCode == KeyCode.LeftArrow &&
                    !LoreActiveTextEditor.IsEditingText())
                {
                    LoreGUIFocus.Clear();

                    _browser.FocusNavigationOnCurrentSelection();

                    current.Use();
                }

                return;
            }

            switch (current.keyCode)
            {
                case KeyCode.LeftArrow:
                    _browser.FocusNavigationOnCurrentSelection();

                    current.Use();
                    break;

                case KeyCode.UpArrow:
                    ScrollInspectorBy(-InspectorStyle.KeyboardScrollStep);

                    current.Use();
                    break;

                case KeyCode.DownArrow:
                    ScrollInspectorBy(InspectorStyle.KeyboardScrollStep);

                    current.Use();
                    break;

                case KeyCode.Home:
                    ScrollInspectorTo(0f);

                    current.Use();
                    break;

                case KeyCode.End:
                    ScrollInspectorTo(GetInspectorMaximumScroll());

                    current.Use();
                    break;

                case KeyCode.PageUp:
                    ScrollInspectorBy(-_state.RightViewportHeight);

                    current.Use();
                    break;

                case KeyCode.PageDown:
                    ScrollInspectorBy(_state.RightViewportHeight);

                    current.Use();
                    break;

                case KeyCode.Insert:
                    InsertInspectorSelection();

                    current.Use();
                    break;

                case KeyCode.F2:
                    BeginInspectorTitleEditing();

                    current.Use();
                    break;

                case KeyCode.Delete:
                case KeyCode.KeypadPeriod:
                    DeleteInspectorSelection();

                    current.Use();
                    break;

                case KeyCode.Plus:
                case KeyCode.KeypadPlus:
                    OpenInspectorLibrary();

                    current.Use();
                    break;
            }
        }

        private void ScrollInspectorBy(float delta)
        {
            float maximumScroll =
                GetInspectorMaximumScroll();

            _state.RightScroll =
                new Vector2(
                    _state.RightScroll.x,
                    Mathf.Clamp(
                        _state.RightScroll.y + delta,
                        0f,
                        maximumScroll));

            Repaint();
        }

        private void ScrollInspectorTo(float y)
        {
            float maximumScroll =
                GetInspectorMaximumScroll();

            _state.RightScroll =
                new Vector2(
                    _state.RightScroll.x,
                    Mathf.Clamp(
                        y,
                        0f,
                        maximumScroll));

            Repaint();
        }

        private float GetInspectorMaximumScroll()
        {
            return Mathf.Max(
                0f,
                _state.RightContentHeight -
                _state.RightViewportHeight);
        }

        private void OpenInspectorLibrary()
        {
            if (SelectionService.CurrentCategory != null)
            {
                _libraryHost.ShowFields();

                return;
            }

            if (SelectionService.CurrentEntry != null)
            {
                _libraryHost.ShowTags();
            }
        }

        private void BeginInspectorTitleEditing()
        {
            if (SelectionService.CurrentCategory != null)
            {
                _categoryInspector.BeginTitleEditing();

                return;
            }

            if (SelectionService.CurrentEntry != null)
            {
                _entryInspector.BeginTitleEditing();

                return;
            }

            _projectInspector.BeginTitleEditing();
        }

        private void InsertInspectorSelection()
        {
            if (SelectionService.CurrentCategory != null)
            {
                _categoryInspector.AddEntry();

                return;
            }

            if (SelectionService.CurrentEntry != null)
            {
                _entryInspector.Duplicate();
            }
        }

        private void DeleteInspectorSelection()
        {
            if (SelectionService.CurrentCategory != null)
            {
                _categoryInspector.Delete();

                return;
            }

            if (SelectionService.CurrentEntry != null)
            {
                _entryInspector.Delete();
            }
        }

        private void HandleContentKeyboardFocus(Rect rect)
        {
            if (Event.current.type != EventType.MouseDown)
            {
                return;
            }

            if (!rect.Contains(Event.current.mousePosition))
            {
                return;
            }

            if (_libraryHost.IsOpen && !_libraryHost.TryClose())
            {
                Event.current.Use();

                return;
            }
        }

        private void HandleInspectorKeyboardFocus(Rect rect)
        {
            if (Event.current.type != EventType.MouseDown)
            {
                return;
            }

            if (!rect.Contains(Event.current.mousePosition))
            {
                return;
            }

            if (_libraryHost.IsOpen && !_libraryHost.TryClose())
            {
                Event.current.Use();

                return;
            }

            bool hasSelection =
                SelectionService.CurrentCategory != null
                || SelectionService.CurrentEntry != null;

            if (!hasSelection)
                return;

            // A click inside the inline editor being edited must keep its keyboard focus, or Unity treats it
            // as a first click and selects all text (no caret placement). Every other click still clears
            // focus, which prevents Unity's shared text editor from carrying text into the next field.
            if (LoreEditService.IsEditingAt(
                    GUIUtility.GUIToScreenPoint(
                        Event.current.mousePosition)))
            {
                LoreKeyboardFocus.SetOwner(LoreKeyboardOwner.Inspector);

                return;
            }

            FocusInspectorKeyboard();
        }

        private void FocusInspectorKeyboard()
        {
            _browser.ReleaseSearchFocus();

            LoreKeyboardFocus.SetOwner(LoreKeyboardOwner.Inspector);
        }
        #endregion
    }
}
